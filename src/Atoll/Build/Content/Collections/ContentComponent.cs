using Atoll.Content.Markdown;
using Atoll.Components;
using Atoll.Rendering;

namespace Atoll.Content.Collections;

/// <summary>
/// An <see cref="IAtollComponent"/> that renders pre-rendered Markdown content (HTML) into the
/// component tree. This enables content entries to be used inside layouts and composed
/// with other Atoll components.
/// </summary>
/// <remarks>
/// <para>
/// This is the Atoll equivalent of Astro's <c>Content</c> component returned by
/// <c>render(entry)</c>. The component outputs the pre-rendered HTML directly
/// to the render destination. It also exposes the heading metadata for
/// table-of-contents generation.
/// </para>
/// <para>
/// Usage:
/// <code>
/// var entry = query.GetEntry&lt;BlogPost&gt;("blog", "my-post");
/// var rendered = query.Render(entry);
/// var component = ContentComponent.FromRenderedContent(rendered);
/// await componentRenderer.RenderComponentAsync(component, context);
/// </code>
/// </para>
/// </remarks>
public sealed class ContentComponent : IAtollComponent
{
    private readonly string _html;

    private ContentComponent(string html, IReadOnlyList<MarkdownHeading> headings)
    {
        _html = html;
        Headings = headings;
    }

    /// <summary>
    /// Gets the headings extracted from the rendered Markdown content.
    /// </summary>
    public IReadOnlyList<MarkdownHeading> Headings { get; }

    /// <summary>
    /// Creates a <see cref="ContentComponent"/> from a <see cref="RenderedContent"/> instance.
    /// </summary>
    /// <param name="renderedContent">The rendered content.</param>
    /// <returns>A new <see cref="ContentComponent"/> that renders the content's HTML.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="renderedContent"/> is <c>null</c>.</exception>
    public static ContentComponent FromRenderedContent(RenderedContent renderedContent)
    {
        ArgumentNullException.ThrowIfNull(renderedContent);
        return new ContentComponent(renderedContent.Html, renderedContent.Headings);
    }

    /// <summary>
    /// Creates a <see cref="ContentComponent"/> from raw HTML and heading metadata.
    /// </summary>
    /// <param name="html">The HTML content to render.</param>
    /// <param name="headings">The heading metadata.</param>
    /// <returns>A new <see cref="ContentComponent"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="html"/> or <paramref name="headings"/> is <c>null</c>.
    /// </exception>
    public static ContentComponent FromHtml(string html, IReadOnlyList<MarkdownHeading> headings)
    {
        ArgumentNullException.ThrowIfNull(html);
        ArgumentNullException.ThrowIfNull(headings);
        return new ContentComponent(html, headings);
    }

    /// <summary>
    /// Creates a <see cref="ContentComponent"/> from raw HTML with no heading metadata.
    /// </summary>
    /// <param name="html">The HTML content to render.</param>
    /// <returns>A new <see cref="ContentComponent"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="html"/> is <c>null</c>.</exception>
    public static ContentComponent FromHtml(string html)
    {
        ArgumentNullException.ThrowIfNull(html);
        return new ContentComponent(html, []);
    }

    /// <summary>
    /// Creates a <see cref="RenderFragment"/> from this component's HTML content.
    /// Useful when you need to inject content into a slot or compose with other fragments.
    /// </summary>
    /// <returns>A <see cref="RenderFragment"/> containing the content's HTML.</returns>
    public RenderFragment ToRenderFragment()
    {
        return RenderFragment.FromHtml(_html);
    }

    /// <inheritdoc />
    public Task RenderAsync(RenderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.WriteHtml(_html);
        return Task.CompletedTask;
    }
}
