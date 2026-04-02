using System.Text;
using Atoll.Core.Head;

namespace Atoll.Core.Css;

/// <summary>
/// Generates <c>&lt;style&gt;</c> elements or <c>&lt;link&gt;</c> elements for
/// injecting CSS into the <c>&lt;head&gt;</c> of a page.
/// </summary>
public static class CssInjector
{
    /// <summary>
    /// Creates a <see cref="HeadElement"/> containing an inline <c>&lt;style&gt;</c> element
    /// with the specified CSS content.
    /// </summary>
    /// <param name="css">The CSS text to inline.</param>
    /// <returns>A <see cref="HeadElement"/> representing the <c>&lt;style&gt;</c> element.</returns>
    public static HeadElement CreateInlineStyle(string css)
    {
        ArgumentNullException.ThrowIfNull(css);

        return new HeadElement("style") { Content = css };
    }

    /// <summary>
    /// Creates a <see cref="HeadElement"/> containing an inline <c>&lt;style&gt;</c> element
    /// with the specified CSS content and a scope identifier attribute for debugging.
    /// </summary>
    /// <param name="css">The CSS text to inline.</param>
    /// <param name="scopeHash">The scope hash for identifying the component (e.g., <c>atoll-a1b2c3d4</c>).</param>
    /// <returns>A <see cref="HeadElement"/> representing the <c>&lt;style&gt;</c> element.</returns>
    public static HeadElement CreateInlineStyle(string css, string scopeHash)
    {
        ArgumentNullException.ThrowIfNull(css);
        ArgumentNullException.ThrowIfNull(scopeHash);

        var element = new HeadElement("style");
        element.SetAttribute("data-atoll-scope", scopeHash);
        element.Content = css;
        return element;
    }

    /// <summary>
    /// Creates a <see cref="HeadElement"/> containing a <c>&lt;link rel="stylesheet"&gt;</c> element
    /// pointing to an external CSS file.
    /// </summary>
    /// <param name="href">The URL of the external stylesheet.</param>
    /// <returns>A <see cref="HeadElement"/> representing the <c>&lt;link&gt;</c> element.</returns>
    public static HeadElement CreateStylesheetLink(string href)
    {
        ArgumentNullException.ThrowIfNull(href);

        var element = new HeadElement("link");
        element.SetAttribute("rel", "stylesheet");
        element.SetAttribute("href", href);
        return element;
    }

    /// <summary>
    /// Renders all aggregated CSS from the specified <see cref="CssAggregator"/> as a single
    /// inline <c>&lt;style&gt;</c> element. Optionally minifies the CSS.
    /// </summary>
    /// <param name="aggregator">The CSS aggregator containing collected styles.</param>
    /// <param name="minify">Whether to minify the CSS before injection.</param>
    /// <returns>A <see cref="HeadElement"/> with the combined CSS, or <c>null</c> if no CSS was collected.</returns>
    public static HeadElement? CreateCombinedStyle(CssAggregator aggregator, bool minify)
    {
        ArgumentNullException.ThrowIfNull(aggregator);

        var css = aggregator.GetCombinedCss();
        if (css.Length == 0)
        {
            return null;
        }

        if (minify)
        {
            css = CssMinifier.Minify(css);
        }

        return new HeadElement("style") { Content = css };
    }

    /// <summary>
    /// Renders the specified CSS to an HTML <c>&lt;style&gt;</c> string.
    /// </summary>
    /// <param name="css">The CSS text.</param>
    /// <returns>The HTML string containing the style element.</returns>
    public static string RenderInlineStyleHtml(string css)
    {
        ArgumentNullException.ThrowIfNull(css);

        return $"<style>{css}</style>";
    }

    /// <summary>
    /// Renders the specified CSS to an HTML <c>&lt;style&gt;</c> string with a scope attribute.
    /// </summary>
    /// <param name="css">The CSS text.</param>
    /// <param name="scopeHash">The scope hash for debugging.</param>
    /// <returns>The HTML string containing the style element with scope attribute.</returns>
    public static string RenderInlineStyleHtml(string css, string scopeHash)
    {
        ArgumentNullException.ThrowIfNull(css);
        ArgumentNullException.ThrowIfNull(scopeHash);

        return $"<style data-atoll-scope=\"{scopeHash}\">{css}</style>";
    }

    /// <summary>
    /// Renders the combined CSS from the aggregator into a <see cref="HeadManager"/>
    /// for head injection during page rendering.
    /// </summary>
    /// <param name="aggregator">The CSS aggregator.</param>
    /// <param name="headManager">The head manager to inject into.</param>
    /// <param name="minify">Whether to minify the CSS.</param>
    /// <returns><c>true</c> if CSS was injected; <c>false</c> if the aggregator had no CSS.</returns>
    public static bool InjectIntoHead(CssAggregator aggregator, HeadManager headManager, bool minify)
    {
        ArgumentNullException.ThrowIfNull(aggregator);
        ArgumentNullException.ThrowIfNull(headManager);

        var style = CreateCombinedStyle(aggregator, minify);
        if (style is null)
        {
            return false;
        }

        headManager.Add(style);
        return true;
    }
}
