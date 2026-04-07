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

    /// <summary>
    /// Gets or sets the topic labels for search filtering
    /// (e.g., <c>["IdentityServer", "Security"]</c>).
    /// When set, overrides the auto-seeding of topics from <see cref="Section"/>.
    /// </summary>
    /// <example>
    /// <code>
    /// ---
    /// title: Token Validation
    /// topics:
    ///   - IdentityServer
    ///   - Security
    /// ---
    /// </code>
    /// </example>
    public List<string>? Topics { get; set; }

    /// <summary>
    /// Gets or sets optional raw HTML to inject into the page's &lt;head&gt; section.
    /// Supports analytics scripts, social meta tags, or any custom head content.
    /// Use a YAML literal block (<c>head: |</c>) for multi-line content.
    /// </summary>
    public string? Head { get; set; }

    /// <summary>
    /// Gets or sets a list of old URL paths that should redirect to this page.
    /// Each entry is a base-relative path (e.g., <c>/old-getting-started</c>).
    /// </summary>
    /// <example>
    /// <code>
    /// ---
    /// title: Getting Started
    /// redirectFrom:
    ///   - /old-getting-started
    ///   - /docs/v1/getting-started
    /// ---
    /// </code>
    /// </example>
    public List<string>? RedirectFrom { get; set; }
}
