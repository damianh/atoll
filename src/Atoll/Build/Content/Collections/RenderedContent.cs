using Atoll.Build.Content.Markdown;

namespace Atoll.Build.Content.Collections;

/// <summary>
/// The result of rendering a content entry's Markdown body to HTML.
/// Contains the rendered HTML, extracted headings for table-of-contents,
/// and optional content fragments for inline component embedding.
/// </summary>
public sealed class RenderedContent
{
    /// <summary>
    /// Initializes a new instance of <see cref="RenderedContent"/> with plain HTML.
    /// </summary>
    /// <param name="html">The rendered HTML string.</param>
    /// <param name="headings">The extracted headings from the Markdown content.</param>
    public RenderedContent(string html, IReadOnlyList<MarkdownHeading> headings)
    {
        ArgumentNullException.ThrowIfNull(html);
        ArgumentNullException.ThrowIfNull(headings);
        Html = html;
        Headings = headings;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="RenderedContent"/> with fragment data
    /// for documents containing embedded component directives.
    /// </summary>
    /// <param name="html">The rendered HTML string.</param>
    /// <param name="headings">The extracted headings from the Markdown content.</param>
    /// <param name="fragments">
    /// The sequence of HTML and component fragments. When non-null, <see cref="ContentComponent"/>
    /// renders them in order, resolving embedded Atoll components at each
    /// <see cref="ComponentContentFragment"/> site.
    /// </param>
    public RenderedContent(
        string html,
        IReadOnlyList<MarkdownHeading> headings,
        IReadOnlyList<ContentFragment> fragments)
    {
        ArgumentNullException.ThrowIfNull(html);
        ArgumentNullException.ThrowIfNull(headings);
        ArgumentNullException.ThrowIfNull(fragments);
        Html = html;
        Headings = headings;
        Fragments = fragments;
    }

    /// <summary>
    /// Gets the rendered HTML string.
    /// </summary>
    public string Html { get; }

    /// <summary>
    /// Gets the headings extracted from the content, in document order.
    /// Useful for generating a table of contents.
    /// </summary>
    public IReadOnlyList<MarkdownHeading> Headings { get; }

    /// <summary>
    /// Gets the content fragments produced when the Markdown contained embedded component
    /// directives. When <c>null</c>, no directives were present and <see cref="Html"/>
    /// contains the complete rendered output.
    /// </summary>
    public IReadOnlyList<ContentFragment>? Fragments { get; }
}
