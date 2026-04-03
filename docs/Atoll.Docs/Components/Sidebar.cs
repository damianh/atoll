using Atoll.Components;
using Atoll.Content.Collections;

namespace Atoll.Docs.Components;

/// <summary>
/// Sidebar navigation component for the docs site. Queries all documentation
/// entries, groups them by section, and renders a hierarchical nav list.
/// </summary>
public sealed class Sidebar : AtollComponent
{
    /// <summary>
    /// Gets or sets the collection query used to load doc entries.
    /// </summary>
    [Parameter(Required = true)]
    public CollectionQuery Query { get; set; } = null!;

    /// <summary>
    /// Gets or sets the slug of the currently active page, used to highlight
    /// the active nav item.
    /// </summary>
    [Parameter]
    public string CurrentSlug { get; set; } = "";

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var docs = Query.GetCollection<DocSchema>("docs");

        // Group by section, ordering groups by the minimum Order within each group
        var groups = docs
            .GroupBy(e => e.Data.Section)
            .Select(g => new
            {
                Section = g.Key,
                MinOrder = g.Min(e => e.Data.Order),
                Entries = g.OrderBy(e => e.Data.Order).ToList(),
            })
            .OrderBy(g => g.MinOrder)
            .ToList();

        foreach (var group in groups)
        {
            if (!string.IsNullOrEmpty(group.Section))
            {
                WriteHtml("<h3>");
                WriteText(group.Section);
                WriteHtml("</h3>");
            }

            WriteHtml("<ul>");
            foreach (var entry in group.Entries)
            {
                var navItemProps = new Dictionary<string, object?>
                {
                    ["Title"] = entry.Data.Title,
                    ["Slug"] = entry.Slug,
                    ["IsActive"] = string.Equals(entry.Slug, CurrentSlug, StringComparison.OrdinalIgnoreCase),
                };
                var fragment = ComponentRenderer.ToFragment<NavItem>(navItemProps);
                await RenderAsync(fragment);
            }
            WriteHtml("</ul>");
        }
    }
}
