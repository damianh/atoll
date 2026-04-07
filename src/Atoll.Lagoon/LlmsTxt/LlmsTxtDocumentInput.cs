namespace Atoll.Lagoon.LlmsTxt;

/// <summary>
/// Input descriptor for a single document to include in the <c>llms.txt</c> and <c>llms-full.txt</c> output.
/// Callers construct these from content entries and their resolved URLs.
/// </summary>
public sealed class LlmsTxtDocumentInput
{
    /// <summary>
    /// Initializes a new <see cref="LlmsTxtDocumentInput"/>.
    /// </summary>
    /// <param name="title">The document title (used as link text in <c>llms.txt</c>).</param>
    /// <param name="href">The URL path for the document (e.g., <c>/docs/getting-started/</c>).</param>
    public LlmsTxtDocumentInput(string title, string href)
    {
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(href);
        Title = title;
        Href = href;
    }

    /// <summary>Gets the document title.</summary>
    public string Title { get; }

    /// <summary>Gets the URL path.</summary>
    public string Href { get; }

    /// <summary>Gets or sets an optional short description appended after the link.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets an optional section/group label for organising entries under H2 headings.</summary>
    public string? Section { get; set; }

    /// <summary>
    /// Gets or sets the full markdown body of the document.
    /// Used to generate the <c>llms-full.txt</c> expanded output.
    /// When <c>null</c>, the document is included in the index (<c>llms.txt</c>) but omitted
    /// from the full-content output.
    /// </summary>
    public string? MarkdownBody { get; set; }
}
