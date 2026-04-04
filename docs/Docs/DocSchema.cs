using System.ComponentModel.DataAnnotations;

namespace Docs;

/// <summary>
/// Frontmatter schema for documentation content entries.
/// Properties are populated from YAML frontmatter in Markdown files.
/// </summary>
public sealed class DocSchema
{
    /// <summary>
    /// Gets or sets the page title.
    /// </summary>
    [Required]
    public string Title { get; set; } = "";

    /// <summary>
    /// Gets or sets the page description (used in meta tags and sidebar).
    /// </summary>
    [Required]
    public string Description { get; set; } = "";

    /// <summary>
    /// Gets or sets the sort order for sidebar navigation (lower = earlier).
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Gets or sets the section group name for sidebar grouping
    /// (e.g., "Basics", "Features", "Advanced").
    /// </summary>
    public string Section { get; set; } = "";
}
