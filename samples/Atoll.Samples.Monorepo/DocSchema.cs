using System.ComponentModel.DataAnnotations;

namespace Atoll.Samples.Monorepo;

/// <summary>
/// Frontmatter schema for component documentation pulled from an external directory.
/// </summary>
public sealed class DocSchema
{
    /// <summary>Gets or sets the document title.</summary>
    [Required]
    public string Title { get; set; } = "";

    /// <summary>Gets or sets the document description.</summary>
    public string Description { get; set; } = "";

    /// <summary>Gets or sets the sort order.</summary>
    public int Order { get; set; }
}
