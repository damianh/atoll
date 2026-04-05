namespace Atoll.Reef.Components;

/// <summary>
/// Represents a single article entry for use in list, grid, and table views.
/// </summary>
public sealed class ArticleListItem
{
    /// <summary>
    /// Initialises a new instance of <see cref="ArticleListItem"/>.
    /// </summary>
    public ArticleListItem(
        string title,
        string slug,
        string description,
        DateTime pubDate,
        string? author,
        string[] tags,
        int? readingTimeMinutes)
    {
        Title = title;
        Slug = slug;
        Description = description;
        PubDate = pubDate;
        Author = author;
        Tags = tags;
        ReadingTimeMinutes = readingTimeMinutes;
    }

    /// <summary>Gets the article title.</summary>
    public string Title { get; }

    /// <summary>Gets the article URL slug.</summary>
    public string Slug { get; }

    /// <summary>Gets the short article description.</summary>
    public string Description { get; }

    /// <summary>Gets the article publication date.</summary>
    public DateTime PubDate { get; }

    /// <summary>Gets the author display name, or <c>null</c> if not set.</summary>
    public string? Author { get; }

    /// <summary>Gets the tag names associated with this article.</summary>
    public string[] Tags { get; }

    /// <summary>Gets the estimated reading time in minutes, or <c>null</c> if not calculated.</summary>
    public int? ReadingTimeMinutes { get; }
}
