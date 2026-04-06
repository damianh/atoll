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
        : this(html, headings, fragments, allReferences: [])
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="RenderedContent"/> with fragment data
    /// and the full ordered component reference list (including nested references).
    /// </summary>
    /// <param name="html">The rendered HTML string.</param>
    /// <param name="headings">The extracted headings from the Markdown content.</param>
    /// <param name="fragments">
    /// The sequence of HTML and component fragments. When non-null, <see cref="ContentComponent"/>
    /// renders them in order, resolving embedded Atoll components at each
    /// <see cref="ComponentContentFragment"/> site.
    /// </param>
    /// <param name="allReferences">
    /// The complete ordered list of all component references, including those nested inside
    /// other components' <c>ChildHtml</c>. Index N corresponds to the
    /// <c>&lt;!--atoll-tag:N--&gt;</c> placeholder that may appear in a component's
    /// <c>ChildHtml</c>.
    /// </param>
    public RenderedContent(
        string html,
        IReadOnlyList<MarkdownHeading> headings,
        IReadOnlyList<ContentFragment> fragments,
        IReadOnlyList<ComponentReference> allReferences)
    {
        ArgumentNullException.ThrowIfNull(html);
        ArgumentNullException.ThrowIfNull(headings);
        ArgumentNullException.ThrowIfNull(fragments);
        ArgumentNullException.ThrowIfNull(allReferences);
        Html = html;
        Headings = headings;
        Fragments = fragments;
        AllReferences = allReferences;
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

    /// <summary>
    /// Gets the complete ordered list of all component references, including those nested
    /// inside other components' <c>ChildHtml</c>. Index N in this list corresponds to the
    /// <c>&lt;!--atoll-tag:N--&gt;</c> placeholder that may appear in a component's
    /// <c>ChildHtml</c>. Empty when no components were present.
    /// </summary>
    public IReadOnlyList<ComponentReference> AllReferences { get; } = [];
}
