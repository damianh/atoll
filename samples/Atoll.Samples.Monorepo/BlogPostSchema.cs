using System.ComponentModel.DataAnnotations;

namespace Atoll.Samples.Monorepo;

/// <summary>
/// Frontmatter schema for blog posts in the local content directory.
/// </summary>
public sealed class BlogPostSchema
{
    /// <summary>Gets or sets the blog post title.</summary>
    [Required]
    public string Title { get; set; } = "";

    /// <summary>Gets or sets the blog post description.</summary>
    [Required]
    public string Description { get; set; } = "";

    /// <summary>Gets or sets the publication date.</summary>
    [Required]
    public DateTime PubDate { get; set; }

    /// <summary>Gets or sets a value indicating whether this post is a draft.</summary>
    public bool Draft { get; set; }
}
