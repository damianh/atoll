namespace Atoll.Docs.Search;

/// <summary>
/// Represents a single searchable document in the search index.
/// </summary>
public sealed class SearchEntry
{
    /// <summary>
    /// Initializes a new instance of <see cref="SearchEntry"/>.
    /// </summary>
    /// <param name="title">The document title.</param>
    /// <param name="href">The URL path for this document.</param>
    /// <param name="description">Optional short description.</param>
    /// <param name="section">Optional section label (e.g., sidebar group).</param>
    /// <param name="headings">Heading texts extracted from the document.</param>
    /// <param name="body">Plain-text excerpt of the document body (HTML stripped, truncated).</param>
    public SearchEntry(
        string title,
        string href,
        string? description,
        string? section,
        IReadOnlyList<string> headings,
        string body)
    {
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(href);
        ArgumentNullException.ThrowIfNull(headings);
        ArgumentNullException.ThrowIfNull(body);
        Title = title;
        Href = href;
        Description = description;
        Section = section;
        Headings = headings;
        Body = body;
    }

    /// <summary>Gets the document title.</summary>
    public string Title { get; }

    /// <summary>Gets the URL path for this document.</summary>
    public string Href { get; }

    /// <summary>Gets the optional short description.</summary>
    public string? Description { get; }

    /// <summary>Gets the optional section label (e.g., sidebar group name).</summary>
    public string? Section { get; }

    /// <summary>Gets the heading texts extracted from this document.</summary>
    public IReadOnlyList<string> Headings { get; }

    /// <summary>Gets the plain-text excerpt of the document body (HTML stripped).</summary>
    public string Body { get; }
}
