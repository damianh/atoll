namespace Atoll.Docs.Configuration;

/// <summary>
/// Represents a single item in the docs sidebar navigation tree.
/// An item can be a leaf link, a group header with children, or an auto-generated directory group.
/// </summary>
public sealed class SidebarItem
{
    /// <summary>
    /// Gets or sets the display label for this sidebar item.
    /// </summary>
    public string Label { get; set; } = "";

    /// <summary>
    /// Gets or sets the URL this item links to.
    /// When <c>null</c>, this item is a group header with no direct link.
    /// </summary>
    public string? Link { get; set; }

    /// <summary>
    /// Gets or sets an optional badge text to display next to the label (e.g., "New", "OSS").
    /// </summary>
    public string? Badge { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this group is collapsed by default.
    /// Only applies to items with <see cref="Items"/> children.
    /// Default: <c>false</c>.
    /// </summary>
    public bool Collapsed { get; set; }

    /// <summary>
    /// Gets or sets the directory path to auto-generate sidebar entries from.
    /// When set, child <see cref="Items"/> are populated from content files in this directory.
    /// Typically a relative path from the content root (e.g., <c>"guides"</c>).
    /// When <c>null</c>, <see cref="Items"/> must be set manually.
    /// </summary>
    public string? AutoGenerate { get; set; }

    /// <summary>
    /// Gets or sets the manually-defined child items for this group.
    /// Ignored when <see cref="AutoGenerate"/> is set.
    /// </summary>
    public IReadOnlyList<SidebarItem> Items { get; set; } = [];
}
