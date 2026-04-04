namespace Atoll.Lagoon.Search;

/// <summary>
/// The complete search index generated at build time from all content entries.
/// </summary>
public sealed class SearchIndex
{
    /// <summary>
    /// Initializes a new instance of <see cref="SearchIndex"/>.
    /// </summary>
    /// <param name="entries">The searchable document entries.</param>
    /// <param name="generatedAt">The UTC timestamp when the index was generated.</param>
    public SearchIndex(IReadOnlyList<SearchEntry> entries, DateTimeOffset generatedAt)
    {
        ArgumentNullException.ThrowIfNull(entries);
        Entries = entries;
        GeneratedAt = generatedAt;
    }

    /// <summary>Gets the list of searchable document entries.</summary>
    public IReadOnlyList<SearchEntry> Entries { get; }

    /// <summary>Gets the UTC timestamp when this index was generated.</summary>
    public DateTimeOffset GeneratedAt { get; }
}
