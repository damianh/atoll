namespace Atoll.Docs.Navigation;

/// <summary>
/// Represents an entry provided to the sidebar builder for auto-generation.
/// Each entry corresponds to one content page.
/// </summary>
public sealed class SidebarEntry
{
    /// <summary>
    /// Initializes a new instance of <see cref="SidebarEntry"/>.
    /// </summary>
    /// <param name="label">The display label for the sidebar link.</param>
    /// <param name="href">The URL this entry links to.</param>
    /// <param name="slug">The content slug (e.g., <c>"guides/getting-started"</c>).</param>
    /// <param name="order">Optional sort order within its group. Lower values appear first.</param>
    /// <param name="badge">Optional badge text to display next to the label.</param>
    public SidebarEntry(string label, string href, string slug, int order, string? badge)
    {
        ArgumentNullException.ThrowIfNull(label);
        ArgumentNullException.ThrowIfNull(href);
        ArgumentNullException.ThrowIfNull(slug);
        Label = label;
        Href = href;
        Slug = slug;
        Order = order;
        Badge = badge;
    }

    /// <summary>Gets the display label.</summary>
    public string Label { get; }

    /// <summary>Gets the URL for the sidebar link.</summary>
    public string Href { get; }

    /// <summary>Gets the content slug (path without extension).</summary>
    public string Slug { get; }

    /// <summary>Gets the sort order within its group. Lower values appear first.</summary>
    public int Order { get; }

    /// <summary>Gets an optional badge text.</summary>
    public string? Badge { get; }
}
