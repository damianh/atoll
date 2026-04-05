using Atoll.Components;
using Atoll.Lagoon.Configuration;
using Atoll.Lagoon.Navigation;

namespace Atoll.Lagoon.Components;

/// <summary>
/// Renders a collapsible sidebar group with a heading and child items.
/// Uses an HTML <c>&lt;details&gt;</c> / <c>&lt;summary&gt;</c> element for
/// CSS-only collapse/expand — no JavaScript required.
/// A chevron indicator is rendered whose position is controlled by
/// <see cref="ChevronPosition"/>.
/// </summary>
public sealed class SidebarGroup : AtollComponent
{
    /// <summary>Gets or sets the resolved group item to render.</summary>
    [Parameter(Required = true)]
    public ResolvedSidebarItem Group { get; set; } = null!;

    /// <summary>
    /// Gets or sets the chevron position for this group's collapse indicator.
    /// Default: <see cref="SidebarChevronPosition.End"/>.
    /// </summary>
    [Parameter]
    public SidebarChevronPosition ChevronPosition { get; set; } = SidebarChevronPosition.End;

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var open = (!Group.Collapsed || Group.IsActive) ? " open" : "";
        var positionClass = ChevronPosition == SidebarChevronPosition.Start
            ? "sidebar-chevron-start"
            : "sidebar-chevron-end";

        WriteHtml($"<details class=\"{positionClass}\"{open}><summary>");
        WriteText(Group.Label);
        if (Group.Badge is not null)
        {
            WriteHtml($" <span class=\"badge\">{System.Net.WebUtility.HtmlEncode(Group.Badge)}</span>");
        }

        WriteHtml("""<span class="sidebar-chevron" aria-hidden="true"><svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="9 18 15 12 9 6"/></svg></span>""");
        WriteHtml("</summary><ul>");

        foreach (var child in Group.Items)
        {
            if (child.IsGroup)
            {
                WriteHtml("<li class=\"sidebar-group-item\">");
                var groupFragment = ComponentRenderer.ToFragment<SidebarGroup>(
                    new Dictionary<string, object?>
                    {
                        ["Group"] = child,
                        ["ChevronPosition"] = ChevronPosition,
                    });
                await RenderAsync(groupFragment);
                WriteHtml("</li>");
            }
            else
            {
                var linkFragment = ComponentRenderer.ToFragment<SidebarLink>(
                    new Dictionary<string, object?> { ["Item"] = child });
                await RenderAsync(linkFragment);
            }
        }

        WriteHtml("</ul></details>");
    }
}
