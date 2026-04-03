namespace Atoll.Docs.Navigation;

/// <summary>
/// A resolved sidebar item produced by <see cref="SidebarBuilder"/>.
/// Represents either a navigation link or a group header in the sidebar tree.
/// </summary>
public sealed class ResolvedSidebarItem
{
    /// <summary>
    /// Initializes a link item.
    /// </summary>
    /// <param name="label">The display label.</param>
    /// <param name="href">The URL this link points to.</param>
    /// <param name="isCurrent">Whether this is the current page.</param>
    /// <param name="badge">Optional badge text.</param>
    public ResolvedSidebarItem(string label, string href, bool isCurrent, string? badge)
    {
        ArgumentNullException.ThrowIfNull(label);
        ArgumentNullException.ThrowIfNull(href);
        Label = label;
        Href = href;
        IsCurrent = isCurrent;
        IsActive = isCurrent;
        Badge = badge;
        IsGroup = false;
        Items = [];
    }

    /// <summary>
    /// Initializes a group item.
    /// </summary>
    /// <param name="label">The group heading label.</param>
    /// <param name="isActive">Whether any descendant of this group is the current page.</param>
    /// <param name="badge">Optional badge text.</param>
    /// <param name="collapsed">Whether this group is collapsed by default.</param>
    /// <param name="items">The resolved child items.</param>
    public ResolvedSidebarItem(
        string label,
        bool isActive,
        string? badge,
        bool collapsed,
        IReadOnlyList<ResolvedSidebarItem> items)
    {
        ArgumentNullException.ThrowIfNull(label);
        ArgumentNullException.ThrowIfNull(items);
        Label = label;
        Href = null;
        IsCurrent = false;
        IsActive = isActive;
        Badge = badge;
        IsGroup = true;
        Collapsed = collapsed;
        Items = items;
    }

    /// <summary>Gets the display label.</summary>
    public string Label { get; }

    /// <summary>
    /// Gets the URL this item links to.
    /// <c>null</c> for group headers.
    /// </summary>
    public string? Href { get; }

    /// <summary>Gets whether this is the current page (exact match on href).</summary>
    public bool IsCurrent { get; }

    /// <summary>
    /// Gets whether this item or any of its descendants matches the current page.
    /// Always <c>true</c> for items where <see cref="IsCurrent"/> is <c>true</c>.
    /// For groups, <c>true</c> when any child is active.
    /// </summary>
    public bool IsActive { get; }

    /// <summary>Gets optional badge text.</summary>
    public string? Badge { get; }

    /// <summary>Gets whether this item is a group header rather than a link.</summary>
    public bool IsGroup { get; }

    /// <summary>Gets whether a group is collapsed by default. Always <c>false</c> for link items.</summary>
    public bool Collapsed { get; }

    /// <summary>Gets the child items for a group. Empty for link items.</summary>
    public IReadOnlyList<ResolvedSidebarItem> Items { get; }
}
