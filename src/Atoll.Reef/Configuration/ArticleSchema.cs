using System.ComponentModel.DataAnnotations;

namespace Atoll.Reef.Configuration;

/// <summary>
/// Frontmatter schema for article/blog post content entries.
/// Properties are populated from YAML frontmatter in Markdown files.
/// </summary>
public sealed class ArticleSchema
{
    /// <summary>Gets or sets the article title.</summary>
    [Required]
    public string Title { get; set; } = "";

    /// <summary>Gets or sets the article description (used in summaries and meta tags).</summary>
    [Required]
    public string Description { get; set; } = "";

    /// <summary>Gets or sets the publication date.</summary>
    [Required]
    public DateTime PubDate { get; set; }

    /// <summary>Gets or sets the author name or identifier.</summary>
    public string Author { get; set; } = "";

    /// <summary>
    /// Gets or sets the comma-separated tags for categorisation.
    /// Example: <c>"atoll, tutorial, getting-started"</c>
    /// </summary>
    public string Tags { get; set; } = "";

    /// <summary>
    /// Gets or sets the series name this article belongs to, or <c>null</c> if it is not part of a series.
    /// Example: <c>"Getting Started"</c>
    /// </summary>
    public string? Series { get; set; }

    /// <summary>
    /// Gets or sets the ordinal position of this article within its series (1-based).
    /// Ignored when <see cref="Series"/> is <c>null</c>.
    /// </summary>
    public int? SeriesOrder { get; set; }

    /// <summary>Gets or sets the URL or path to a cover image for the article.</summary>
    public string? Image { get; set; }

    /// <summary>Gets or sets the alt text for the cover image.</summary>
    public string? ImageAlt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this article is a draft.
    /// Drafts are excluded from published listings.
    /// </summary>
    public bool Draft { get; set; }

    /// <summary>
    /// Gets or sets the estimated reading time in minutes.
    /// When <c>null</c>, the reading time is auto-calculated from the word count.
    /// </summary>
    public int? ReadingTimeMinutes { get; set; }

    /// <summary>
    /// Gets the tags as a string array, splitting on commas and trimming whitespace.
    /// Returns an empty array when no tags are set.
    /// </summary>
    public string[] GetTags()
    {
        if (string.IsNullOrWhiteSpace(Tags))
        {
            return [];
        }

        return Tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
