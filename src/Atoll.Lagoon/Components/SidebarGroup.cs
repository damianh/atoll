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
/// <remarks>
/// Each group consumes one index from the shared <see cref="Counter"/>, emitting
/// it as a <c>data-index</c> attribute on the <c>&lt;details&gt;</c> element and
/// on an <c>&lt;sl-sidebar-restore&gt;</c> custom element inside the group.
/// The client-side script uses these indices to restore open/closed state from
/// <c>sessionStorage</c> without a flash of wrong state.
/// </remarks>
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

    /// <summary>
    /// Gets or sets the shared group index counter. Each group calls
    /// <see cref="GroupIndexCounter.Next"/> once during rendering to obtain
    /// its unique sequential index.
    /// </summary>
    [Parameter(Required = true)]
    public GroupIndexCounter Counter { get; set; } = null!;

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var groupIndex = Counter.Next();
        var open = Group.IsActive ? " open" : "";
        var activeAttr = Group.IsActive ? " data-active" : "";
        var positionClass = ChevronPosition == SidebarChevronPosition.Start
            ? "sidebar-chevron-start"
            : "sidebar-chevron-end";

        WriteHtml($"<details class=\"{positionClass}\" data-index=\"{groupIndex}\"{activeAttr}{open}><summary>");
        WriteText(Group.Label);
        if (Group.Badge is not null)
        {
            var badgeClass = SidebarLink.BadgeCssClass(Group.Badge.Variant);
            WriteHtml($" <span class=\"{badgeClass}\">{System.Net.WebUtility.HtmlEncode(Group.Badge.Text)}</span>");
        }

        WriteHtml("""<span class="sidebar-chevron" aria-hidden="true"><svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="9 18 15 12 9 6"/></svg></span>""");
        WriteHtml($"</summary><sl-sidebar-restore data-index=\"{groupIndex}\"></sl-sidebar-restore><ul>");

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
                        ["Counter"] = Counter,
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
