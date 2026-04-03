using Atoll.Rendering;

namespace Atoll.Instructions;

/// <summary>
/// A render instruction that represents content to be injected into the <c>&lt;head&gt;</c>
/// element of the page. This is the primary mechanism for components to add stylesheets,
/// meta tags, title elements, and other head content.
/// </summary>
/// <remarks>
/// <para>
/// Head instructions are collected during component tree rendering and deduplicated
/// by their <see cref="RenderInstruction.Key"/>. The key is typically derived from the
/// content itself (e.g., the href of a stylesheet link) so that multiple components
/// referencing the same resource result in only one head element.
/// </para>
/// <para>
/// Example keys:
/// <list type="bullet">
/// <item><c>link:stylesheet:/css/main.css</c> — for a stylesheet link</item>
/// <item><c>meta:description</c> — for a meta description tag</item>
/// <item><c>style:component-abc123</c> — for a scoped component style</item>
/// </list>
/// </para>
/// </remarks>
public sealed class HeadInstruction : RenderInstruction
{
    private readonly RenderFragment _content;

    /// <summary>
    /// Initializes a new <see cref="HeadInstruction"/> with the specified key and content.
    /// </summary>
    /// <param name="key">The deduplication key.</param>
    /// <param name="content">The head content as a <see cref="RenderFragment"/>.</param>
    public HeadInstruction(string key, RenderFragment content)
        : base(key)
    {
        _content = content;
    }

    /// <summary>
    /// Creates a <see cref="HeadInstruction"/> for a stylesheet link.
    /// </summary>
    /// <param name="href">The stylesheet URL.</param>
    /// <returns>A new <see cref="HeadInstruction"/>.</returns>
    public static HeadInstruction Stylesheet(string href)
    {
        ArgumentNullException.ThrowIfNull(href);
        var html = $"<link rel=\"stylesheet\" href=\"{Rendering.HtmlEncoder.Encode(href)}\">";
        return new HeadInstruction(
            $"link:stylesheet:{href}",
            RenderFragment.FromHtml(html));
    }

    /// <summary>
    /// Creates a <see cref="HeadInstruction"/> for inline style content.
    /// </summary>
    /// <param name="scopeId">A unique scope identifier for deduplication.</param>
    /// <param name="css">The CSS content.</param>
    /// <returns>A new <see cref="HeadInstruction"/>.</returns>
    public static HeadInstruction InlineStyle(string scopeId, string css)
    {
        ArgumentNullException.ThrowIfNull(scopeId);
        ArgumentNullException.ThrowIfNull(css);
        var html = $"<style>{css}</style>";
        return new HeadInstruction(
            $"style:{scopeId}",
            RenderFragment.FromHtml(html));
    }

    /// <summary>
    /// Creates a <see cref="HeadInstruction"/> for a meta tag.
    /// </summary>
    /// <param name="name">The meta tag name attribute.</param>
    /// <param name="content">The meta tag content attribute.</param>
    /// <returns>A new <see cref="HeadInstruction"/>.</returns>
    public static HeadInstruction Meta(string name, string content)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(content);
        var html = $"<meta name=\"{Rendering.HtmlEncoder.Encode(name)}\" content=\"{Rendering.HtmlEncoder.Encode(content)}\">";
        return new HeadInstruction(
            $"meta:{name}",
            RenderFragment.FromHtml(html));
    }

    /// <summary>
    /// Creates a <see cref="HeadInstruction"/> for a title element.
    /// </summary>
    /// <param name="title">The page title text.</param>
    /// <returns>A new <see cref="HeadInstruction"/>.</returns>
    public static HeadInstruction Title(string title)
    {
        ArgumentNullException.ThrowIfNull(title);
        var html = $"<title>{Rendering.HtmlEncoder.Encode(title)}</title>";
        return new HeadInstruction("title", RenderFragment.FromHtml(html));
    }

    /// <inheritdoc />
    public override ValueTask RenderAsync(IRenderDestination destination)
    {
        ArgumentNullException.ThrowIfNull(destination);
        return _content.RenderAsync(destination);
    }
}
