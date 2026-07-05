using Atoll.Lagoon.Configuration;

namespace Atoll.Lagoon.Navigation;

/// <summary>
/// Represents an entry provided to the sidebar builder for auto-generation.
/// Each entry corresponds to one content page.
/// </summary>
/// <remarks>
/// Entries may carry a <see cref="Tags"/> dictionary populated from frontmatter at construction time.
/// Tags are used by <see cref="Atoll.Lagoon.Configuration.SidebarItem.Filter"/> to filter entries
/// in auto-generated sidebar groups.
/// </remarks>
public sealed class SidebarEntry
{
    /// <summary>
    /// Initializes a new instance of <see cref="SidebarEntry"/>.
    /// </summary>
    /// <param name="label">The display label for the sidebar link.</param>
    /// <param name="href">The URL this entry links to.</param>
    /// <param name="slug">The content slug (e.g., <c>"guides/getting-started"</c>).</param>
    /// <param name="order">Optional sort order within its group. Lower values appear first.</param>
    /// <param name="badge">Optional badge to display next to the label.</param>
    public SidebarEntry(string label, string href, string slug, int order, SidebarBadge? badge)
    {
        ArgumentNullException.ThrowIfNull(label);
        ArgumentNullException.ThrowIfNull(href);
        ArgumentNullException.ThrowIfNull(slug);
        Label = label;
        Href = href;
        Slug = slug;
        Order = order;
        Badge = badge;
        Draft = false;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="SidebarEntry"/> with a draft flag.
    /// </summary>
    /// <param name="label">The display label for the sidebar link.</param>
    /// <param name="href">The URL this entry links to.</param>
    /// <param name="slug">The content slug (e.g., <c>"guides/getting-started"</c>).</param>
    /// <param name="order">Optional sort order within its group. Lower values appear first.</param>
    /// <param name="badge">Optional badge to display next to the label.</param>
    /// <param name="draft">
    /// When <c>true</c>, this entry is a draft and will be excluded from
    /// auto-generated sidebar groups and should be excluded from the search index.
    /// </param>
    public SidebarEntry(string label, string href, string slug, int order, SidebarBadge? badge, bool draft)
    {
        ArgumentNullException.ThrowIfNull(label);
        ArgumentNullException.ThrowIfNull(href);
        ArgumentNullException.ThrowIfNull(slug);
        Label = label;
        Href = href;
        Slug = slug;
        Order = order;
        Badge = badge;
        Draft = draft;
    }

    /// <summary>Gets the display label.</summary>
    public string Label { get; }

    /// <summary>Gets the URL for the sidebar link.</summary>
    public string Href { get; }

    /// <summary>Gets the content slug (path without extension).</summary>
    public string Slug { get; }

    /// <summary>Gets the sort order within its group. Lower values appear first.</summary>
    public int Order { get; }

    /// <summary>Gets an optional badge.</summary>
    public SidebarBadge? Badge { get; }

    /// <summary>
    /// Gets a value indicating whether this entry is a draft.
    /// When <c>true</c>, the entry is excluded from auto-generated sidebar groups.
    /// Callers should also exclude draft entries from the search index before
    /// passing them to <see cref="Atoll.Lagoon.Search.LagoonSearchIndexGenerator"/>.
    /// </summary>
    public bool Draft { get; }

    /// <summary>
    /// Gets a dictionary of string tags derived from frontmatter at entry construction time.
    /// Tags are used by <see cref="Atoll.Lagoon.Configuration.SidebarItem.Filter"/> to filter
    /// entries in auto-generated sidebar groups. When not set, defaults to an empty dictionary.
    /// A filter requiring a specific key will not match untagged entries.
    /// </summary>
    /// <example>
    /// <code>
    /// new SidebarEntry(label, href, slug, order, null)
    /// {
    ///     Tags = new Dictionary&lt;string, string&gt; { ["status"] = "active" }
    /// }
    /// </code>
    /// </example>
    public IReadOnlyDictionary<string, string> Tags { get; init; } = new Dictionary<string, string>();
}
