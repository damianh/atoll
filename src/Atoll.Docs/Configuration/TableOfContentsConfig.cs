namespace Atoll.Docs.Configuration;

/// <summary>
/// Configuration for the table of contents (TOC) rendered on each documentation page.
/// Controls which heading levels are included in the "On this page" sidebar.
/// </summary>
public sealed class TableOfContentsConfig
{
    /// <summary>
    /// Gets or sets the minimum heading level to include in the TOC (inclusive).
    /// Default: <c>2</c> (h2).
    /// </summary>
    public int MinHeadingLevel { get; set; } = 2;

    /// <summary>
    /// Gets or sets the maximum heading level to include in the TOC (inclusive).
    /// Default: <c>3</c> (h3).
    /// </summary>
    public int MaxHeadingLevel { get; set; } = 3;
}
