using Atoll.Components;
using Atoll.Lagoon.Configuration;
using Atoll.Lagoon.I18n;
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

    /// <summary>Gets or sets the UI translations. Defaults to English.</summary>
    [Parameter]
    public UiTranslations Translations { get; set; } = UiTranslations.Default;

    /// <summary>
    /// Gets or sets the chevron position for collapsible group indicators.
    /// Default: <see cref="SidebarChevronPosition.End"/>.
    /// </summary>
    [Parameter]
    public SidebarChevronPosition ChevronPosition { get; set; } = SidebarChevronPosition.End;

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml($"<nav aria-label=\"{System.Net.WebUtility.HtmlEncode(Translations.SidebarNavLabel)}\"><ul>");

        foreach (var item in Items)
        {
            if (item.IsGroup)
            {
                WriteHtml("<li class=\"sidebar-group-item\">");
                var groupFragment = ComponentRenderer.ToFragment<SidebarGroup>(
                    new Dictionary<string, object?>
                    {
                        ["Group"] = item,
                        ["ChevronPosition"] = ChevronPosition,
                    });
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
