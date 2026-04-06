using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html;
using AngleSharp.Html.Parser;

namespace Atoll.Build.Pipeline;

/// <summary>
/// Post-processes rendered HTML to inject asset references with fingerprinted URLs,
/// adjust relative URLs for base path, and optionally strip inline styles that have
/// been extracted to external files.
/// </summary>
/// <remarks>
/// <para>
/// The post-processor uses AngleSharp to parse and manipulate HTML DOM. It runs as the
/// final step after SSG page rendering and asset processing, ensuring that all
/// <c>&lt;link&gt;</c>, <c>&lt;script&gt;</c>, and <c>&lt;img&gt;</c> references in
/// the output HTML point to the correct fingerprinted asset URLs.
/// </para>
/// </remarks>
public sealed class HtmlPostProcessor
{
    private readonly HtmlPostProcessorOptions _options;
    private static readonly IHtmlParser Parser = new HtmlParser();

    /// <summary>
    /// Initializes a new <see cref="HtmlPostProcessor"/> with the specified options.
    /// </summary>
    /// <param name="options">The post-processing options.</param>
    public HtmlPostProcessor(HtmlPostProcessorOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
    }

    /// <summary>
    /// Initializes a new <see cref="HtmlPostProcessor"/> with default options.
    /// </summary>
    public HtmlPostProcessor()
        : this(new HtmlPostProcessorOptions())
    {
    }

    /// <summary>
    /// Post-processes the specified HTML string by injecting asset references
    /// and adjusting URLs.
    /// </summary>
    /// <param name="html">The rendered HTML to post-process.</param>
    /// <returns>The post-processed HTML string.</returns>
    public string Process(string html)
    {
        ArgumentNullException.ThrowIfNull(html);

        if (html.Length == 0)
        {
            return html;
        }

        var document = Parser.ParseDocument(html);
        var modified = false;

        // Inject CSS <link> if we have a fingerprinted CSS file
        if (_options.CssHref.Length > 0)
        {
            InjectCssLink(document);
            modified = true;
        }

        // Inject JS <script> if we have a fingerprinted JS file
        if (_options.JsHref.Length > 0)
        {
            InjectJsScript(document);
            modified = true;
        }

        // Adjust base path on relative URLs
        if (_options.BasePath.Length > 0)
        {
            modified |= AdjustBasePathUrls(document);
        }

        // Remove inline <style> elements that have been extracted to external files
        if (_options.RemoveInlineStyles)
        {
            modified |= RemoveInlineStyles(document);
        }

        if (!modified)
        {
            return html;
        }

        return SerializeDocument(document, html);
    }

    private void InjectCssLink(IDocument document)
    {
        var head = document.Head;
        if (head is null)
        {
            return;
        }

        var link = document.CreateElement("link");
        link.SetAttribute("rel", "stylesheet");
        link.SetAttribute("href", _options.CssHref);
        head.AppendChild(link);
    }

    private void InjectJsScript(IDocument document)
    {
        var body = document.Body;
        if (body is null)
        {
            return;
        }

        var script = document.CreateElement("script");
        script.SetAttribute("src", _options.JsHref);

        if (_options.JsDefer)
        {
            script.SetAttribute("defer", "");
        }

        if (_options.JsModule)
        {
            script.SetAttribute("type", "module");
        }

        body.AppendChild(script);
    }

    private bool AdjustBasePathUrls(IDocument document)
    {
        var basePath = NormalizeBasePath(_options.BasePath);
        if (basePath.Length == 0)
        {
            return false;
        }

        var modified = false;

        // Adjust href attributes on <a>, <link>
        modified |= AdjustAttributes(document, "a[href], link[href]", "href", basePath);

        // Adjust src attributes on <script>, <img>, <source>
        modified |= AdjustAttributes(document, "script[src], img[src], source[src]", "src", basePath);

        // Adjust srcset attributes on <img>, <source>
        modified |= AdjustSrcsetAttributes(document, "img[srcset], source[srcset]", basePath);

        // Adjust action attributes on <form>
        modified |= AdjustAttributes(document, "form[action]", "action", basePath);

        // Adjust island component URLs on <atoll-island>
        modified |= AdjustAttributes(
            document,
            "atoll-island[component-url]",
            "component-url",
            basePath);
        modified |= AdjustAttributes(
            document,
            "atoll-island[before-hydration-url]",
            "before-hydration-url",
            basePath);

        return modified;
    }

    private static bool AdjustAttributes(
        IDocument document,
        string selector,
        string attributeName,
        string basePath)
    {
        var elements = document.QuerySelectorAll(selector);
        var modified = false;

        foreach (var element in elements)
        {
            var value = element.GetAttribute(attributeName);
            if (value is null)
            {
                continue;
            }

            if (ShouldPrefixUrl(value, basePath))
            {
                element.SetAttribute(attributeName, basePath + value);
                modified = true;
            }
        }

        return modified;
    }

    private static bool AdjustSrcsetAttributes(
        IDocument document,
        string selector,
        string basePath)
    {
        var elements = document.QuerySelectorAll(selector);
        var modified = false;

        foreach (var element in elements)
        {
            var srcset = element.GetAttribute("srcset");
            if (srcset is null)
            {
                continue;
            }

            var parts = srcset.Split(',');
            var adjustedParts = new List<string>(parts.Length);
            var anyAdjusted = false;

            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                var spaceIndex = trimmed.IndexOf(' ');
                var url = spaceIndex >= 0 ? trimmed[..spaceIndex] : trimmed;
                var descriptor = spaceIndex >= 0 ? trimmed[spaceIndex..] : "";

                if (ShouldPrefixUrl(url, basePath))
                {
                    adjustedParts.Add(basePath + url + descriptor);
                    anyAdjusted = true;
                }
                else
                {
                    adjustedParts.Add(trimmed);
                }
            }

            if (anyAdjusted)
            {
                element.SetAttribute("srcset", string.Join(", ", adjustedParts));
                modified = true;
            }
        }

        return modified;
    }

    private static bool RemoveInlineStyles(IDocument document)
    {
        var styles = document.QuerySelectorAll("style").ToList();
        if (styles.Count == 0)
        {
            return false;
        }

        foreach (var style in styles)
        {
            style.Remove();
        }

        return true;
    }

    private static bool ShouldPrefixUrl(string url, string basePath)
    {
        // Only prefix absolute paths that aren't already prefixed
        if (!url.StartsWith('/'))
        {
            return false;
        }

        // Skip external-protocol URLs
        if (url.StartsWith("//", StringComparison.Ordinal))
        {
            return false;
        }

        // Skip if already has the base path
        if (url.StartsWith(basePath, StringComparison.Ordinal))
        {
            return false;
        }

        return true;
    }

    private static string NormalizeBasePath(string basePath)
    {
        if (basePath.Length == 0)
        {
            return "";
        }

        // Ensure leading slash
        if (!basePath.StartsWith('/'))
        {
            basePath = "/" + basePath;
        }

        // Remove trailing slash
        return basePath.TrimEnd('/');
    }

    private static string SerializeDocument(IDocument document, string originalHtml)
    {
        // Detect if original had DOCTYPE
        var hasDoctype = originalHtml.StartsWith("<!DOCTYPE", StringComparison.OrdinalIgnoreCase)
            || originalHtml.StartsWith("<!doctype", StringComparison.OrdinalIgnoreCase);

        using var writer = new System.IO.StringWriter();
        var formatter = new HtmlMarkupFormatter();

        if (hasDoctype && document.Doctype is not null)
        {
            document.Doctype.ToHtml(writer, formatter);
            writer.Write('\n');
        }

        if (document.DocumentElement is not null)
        {
            document.DocumentElement.ToHtml(writer, formatter);
        }

        return writer.ToString();
    }
}

/// <summary>
/// Configuration options for the <see cref="HtmlPostProcessor"/>.
/// </summary>
public sealed class HtmlPostProcessorOptions
{
    /// <summary>
    /// Gets or sets the href for the processed CSS file to inject as a <c>&lt;link&gt;</c> tag.
    /// An empty string disables CSS injection. Defaults to empty.
    /// </summary>
    public string CssHref { get; set; } = "";

    /// <summary>
    /// Gets or sets the src for the processed JS file to inject as a <c>&lt;script&gt;</c> tag.
    /// An empty string disables JS injection. Defaults to empty.
    /// </summary>
    public string JsHref { get; set; } = "";

    /// <summary>
    /// Gets or sets whether to add the <c>defer</c> attribute to the injected script tag.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool JsDefer { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to add <c>type="module"</c> to the injected script tag.
    /// Defaults to <c>false</c>.
    /// </summary>
    public bool JsModule { get; set; }

    /// <summary>
    /// Gets or sets the base path to prepend to absolute URLs (e.g., <c>/docs</c>).
    /// An empty string disables base path adjustment. Defaults to empty.
    /// </summary>
    public string BasePath { get; set; } = "";

    /// <summary>
    /// Gets or sets whether to remove inline <c>&lt;style&gt;</c> elements from the HTML.
    /// Set to <c>true</c> when CSS has been extracted to external files. Defaults to <c>false</c>.
    /// </summary>
    public bool RemoveInlineStyles { get; set; }
}
