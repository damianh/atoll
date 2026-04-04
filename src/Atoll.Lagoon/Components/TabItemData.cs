using Atoll.Rendering;

namespace Atoll.Lagoon.Components;

/// <summary>
/// Represents a single tab item with a label, optional icon, and content.
/// </summary>
public sealed class TabItemData
{
    /// <summary>Initializes a new tab item.</summary>
    public TabItemData(string label, RenderFragment content, IconName? iconName = null)
    {
        Label = label;
        Content = content;
        IconName = iconName;
    }

    /// <summary>Gets the display label for the tab button.</summary>
    public string Label { get; }

    /// <summary>Gets the content fragment to render in the tab panel.</summary>
    public RenderFragment Content { get; }

    /// <summary>Gets an optional icon to show in the tab button.</summary>
    public IconName? IconName { get; }
}
