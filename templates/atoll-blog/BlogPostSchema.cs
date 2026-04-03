using System.ComponentModel.DataAnnotations;

namespace AtollBlog;

/// <summary>
/// Frontmatter schema for blog post content entries.
/// Properties are populated from YAML frontmatter in Markdown files.
/// </summary>
public sealed class BlogPostSchema
{
    /// <summary>
    /// Gets or sets the blog post title.
    /// </summary>
    [Required]
    public string Title { get; set; } = "";

    /// <summary>
    /// Gets or sets the blog post description (used in summaries and meta tags).
    /// </summary>
    [Required]
    public string Description { get; set; } = "";

    /// <summary>
    /// Gets or sets the publication date.
    /// </summary>
    [Required]
    public DateTime PubDate { get; set; }

    /// <summary>
    /// Gets or sets the author name.
    /// </summary>
    public string Author { get; set; } = "";

    /// <summary>
    /// Gets or sets the comma-separated tags for categorization.
    /// </summary>
    public string Tags { get; set; } = "";

    /// <summary>
    /// Gets or sets a value indicating whether this post is a draft.
    /// Drafts are excluded from the published blog listing.
    /// </summary>
    public bool Draft { get; set; }

    /// <summary>
    /// Gets the tags as a string array, splitting on comma.
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
