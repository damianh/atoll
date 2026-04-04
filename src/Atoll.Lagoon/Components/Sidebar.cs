using Atoll.Components;
using Atoll.Lagoon.Navigation;

namespace Atoll.Lagoon.Components;

/// <summary>
/// Renders the full sidebar navigation as an accessible <c>&lt;nav&gt;</c> element
/// with nested <c>&lt;ul&gt;</c> lists, section headings, active state highlighting,
/// and collapsible groups.
/// </summary>
public sealed class Sidebar : AtollComponent
{
    /// <summary>Gets or sets the resolved sidebar items to render.</summary>
    [Parameter(Required = true)]
    public IReadOnlyList<ResolvedSidebarItem> Items { get; set; } = [];

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<nav aria-label=\"Main\"><ul>");

        foreach (var item in Items)
        {
            if (item.IsGroup)
            {
                WriteHtml("<li class=\"sidebar-group-item\">");
                var groupFragment = ComponentRenderer.ToFragment<SidebarGroup>(
                    new Dictionary<string, object?> { ["Group"] = item });
                await RenderAsync(groupFragment);
                WriteHtml("</li>");
            }
            else
            {
                var linkFragment = ComponentRenderer.ToFragment<SidebarLink>(
                    new Dictionary<string, object?> { ["Item"] = item });
                await RenderAsync(linkFragment);
            }
        }

        WriteHtml("</ul></nav>");
    }
}
