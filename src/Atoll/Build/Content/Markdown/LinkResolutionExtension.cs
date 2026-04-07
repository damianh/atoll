using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Renderers.Html.Inlines;
using Markdig.Syntax.Inlines;

namespace Atoll.Build.Content.Markdown;

/// <summary>
/// Markdig extension that rewrites Markdown file links to clean URL paths.
/// For example, <c>[page](./other-page.md)</c> is rewritten to <c>&lt;a href="/docs/other-page/"&gt;page&lt;/a&gt;</c>
/// and <c>[page](/docs/other-page.md)</c> is rewritten to <c>&lt;a href="/docs/other-page/"&gt;page&lt;/a&gt;</c>.
/// Both relative and root-relative links ending in a configured extension (e.g., <c>.md</c>, <c>.mdx</c>,
/// <c>.mda</c>) are rewritten. Absolute URLs (starting with <c>http://</c> or <c>https://</c>) are left untouched.
/// </summary>
internal sealed class LinkResolutionExtension : IMarkdownExtension
{
    private readonly LinkResolutionOptions _options;

    internal LinkResolutionExtension(LinkResolutionOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
    }

    void IMarkdownExtension.Setup(MarkdownPipelineBuilder pipeline)
    {
        // No AST-level setup needed — we hook the renderer.
    }

    void IMarkdownExtension.Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
        if (renderer is not HtmlRenderer htmlRenderer)
        {
            return;
        }

        EnsureAtollLinkRenderer(htmlRenderer).LinkResolution = _options;
    }

    internal static AtollLinkInlineRenderer EnsureAtollLinkRenderer(HtmlRenderer htmlRenderer)
    {
        // If our combined renderer is already installed, return it.
        if (htmlRenderer.ObjectRenderers.FindExact<AtollLinkInlineRenderer>() is { } existing)
        {
            return existing;
        }

        // Remove the default renderer and install ours.
        var defaultRenderer = htmlRenderer.ObjectRenderers.FindExact<LinkInlineRenderer>();
        if (defaultRenderer is not null)
        {
            htmlRenderer.ObjectRenderers.Remove(defaultRenderer);
        }

        var atollRenderer = new AtollLinkInlineRenderer();
        htmlRenderer.ObjectRenderers.Add(atollRenderer);
        return atollRenderer;
    }
}

/// <summary>
/// Combined <see cref="LinkInline"/> renderer that applies all Atoll link transformations:
/// link resolution (relative .md → clean URL) and external link attributes (target/rel).
/// Only one instance is installed per render pipeline, regardless of how many extensions are active.
/// </summary>
internal sealed class AtollLinkInlineRenderer : LinkInlineRenderer
{
    internal LinkResolutionOptions? LinkResolution { get; set; }
    internal ExternalLinkOptions? ExternalLinks { get; set; }

    protected override void Write(HtmlRenderer renderer, LinkInline link)
    {
        if (link.Url is { } url)
        {
            // Step 1: Resolve relative .md / .mdx links to clean URL paths.
            if (LinkResolution is { } resolution && ShouldResolve(url, resolution))
            {
                url = Resolve(url, resolution);
                link.Url = url;
            }

            // Step 2: Add target/rel to external links.
            if (ExternalLinks is { } externalLinks && IsExternal(url, externalLinks))
            {
                if (externalLinks.Target is { } target)
                {
                    link.GetAttributes().AddProperty("target", target);
                }

                if (externalLinks.Rel is { } rel)
                {
                    link.GetAttributes().AddProperty("rel", rel);
                }
            }
        }

        base.Write(renderer, link);
    }

    private static bool ShouldResolve(string url, LinkResolutionOptions options)
    {
        if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var path = StripFragment(url, out _);
        foreach (var ext in options.ExtensionsToStrip)
        {
            if (path.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string Resolve(string url, LinkResolutionOptions options)
    {
        var isRootRelative = url.StartsWith('/');
        var path = StripFragment(url, out var fragment);

        foreach (var ext in options.ExtensionsToStrip)
        {
            if (path.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
            {
                path = path[..^ext.Length];
                break;
            }
        }

        string resolved;

        if (isRootRelative)
        {
            // Root-relative links already have their full path — don't prepend BasePath.
            resolved = path;
        }
        else
        {
            if (path.StartsWith("./", StringComparison.Ordinal))
            {
                path = path[2..];
            }

            var basePath = options.BasePath.TrimEnd('/');
            var slug = path.TrimStart('/');

            resolved = string.IsNullOrEmpty(slug)
                ? basePath + "/"
                : $"{basePath}/{slug}";
        }

        if (options.AddTrailingSlash && !resolved.EndsWith('/'))
        {
            resolved += "/";
        }

        if (!string.IsNullOrEmpty(fragment))
        {
            resolved += "#" + fragment;
        }

        return resolved;
    }

    private static bool IsExternal(string url, ExternalLinkOptions options)
    {
        if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (options.ExcludedHosts.Count == 0)
        {
            return true;
        }

        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            foreach (var excludedHost in options.ExcludedHosts)
            {
                if (uri.Host.Equals(excludedHost, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static string StripFragment(string url, out string fragment)
    {
        var hashIndex = url.IndexOf('#');
        if (hashIndex >= 0)
        {
            fragment = url[(hashIndex + 1)..];
            return url[..hashIndex];
        }

        fragment = "";
        return url;
    }
}
