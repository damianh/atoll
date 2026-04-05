namespace Atoll.DrawIo.Content;

/// <summary>
/// Represents the metadata schema for a <c>.drawio</c> diagram file used in Atoll content collections.
/// Unlike Markdown files, <c>.drawio</c> files do not have YAML frontmatter; this schema is
/// auto-populated from the structural metadata extracted by <see cref="Parsing.DrawioFileParser"/>.
/// </summary>
public sealed class DrawioDiagramSchema
{
    /// <summary>Gets or sets the title of the diagram, derived from the file name.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the pages contained in the diagram file.</summary>
    public IReadOnlyList<DrawioPageInfo> Pages { get; set; } = [];

    /// <summary>Gets or sets the total number of pages in the diagram file.</summary>
    public int PageCount { get; set; }
}
