using Atoll.Content.Markdown;

namespace Atoll.Content.Collections;

/// <summary>
/// The result of rendering a content entry's Markdown body to HTML.
/// Contains the rendered HTML, extracted headings for table-of-contents,
/// and the associated content entry metadata.
/// </summary>
public sealed class RenderedContent
{
    /// <summary>
    /// Initializes a new instance of <see cref="RenderedContent"/>.
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
    /// Gets the rendered HTML string.
    /// </summary>
    public string Html { get; }

    /// <summary>
    /// Gets the headings extracted from the content, in document order.
    /// Useful for generating a table of contents.
    /// </summary>
    public IReadOnlyList<MarkdownHeading> Headings { get; }
}
