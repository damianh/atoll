using Atoll.Components;
using Atoll.Docs.Navigation;

namespace Atoll.Docs.Components;

/// <summary>
/// Renders a collapsible sidebar group with a heading and child items.
/// Uses an HTML <c>&lt;details&gt;</c> / <c>&lt;summary&gt;</c> element for
/// CSS-only collapse/expand — no JavaScript required.
/// </summary>
public sealed class SidebarGroup : AtollComponent
{
    /// <summary>Gets or sets the resolved group item to render.</summary>
    [Parameter(Required = true)]
    public ResolvedSidebarItem Group { get; set; } = null!;

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var open = (!Group.Collapsed || Group.IsActive) ? " open" : "";
        WriteHtml($"<details{open}><summary>");
        WriteText(Group.Label);
        if (Group.Badge is not null)
        {
            WriteHtml($" <span class=\"badge\">{System.Net.WebUtility.HtmlEncode(Group.Badge)}</span>");
        }

        WriteHtml("</summary><ul>");

        foreach (var child in Group.Items)
        {
            if (child.IsGroup)
            {
                WriteHtml("<li class=\"sidebar-group-item\">");
                var groupFragment = ComponentRenderer.ToFragment<SidebarGroup>(
                    new Dictionary<string, object?> { ["Group"] = child });
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
