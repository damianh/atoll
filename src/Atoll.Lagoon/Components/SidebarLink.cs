using Atoll.Components;
using Atoll.Lagoon.Configuration;
using Atoll.Lagoon.Navigation;

namespace Atoll.Lagoon.Components;

/// <summary>
/// Renders a single sidebar navigation link.
/// </summary>
public sealed class SidebarLink : AtollComponent
{
    /// <summary>Gets or sets the resolved sidebar item to render.</summary>
    [Parameter(Required = true)]
    public ResolvedSidebarItem Item { get; set; } = null!;

    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context)
    {
        var ariaCurrent = Item.IsCurrent ? " aria-current=\"page\"" : "";
        var activeClass = Item.IsActive ? " class=\"active\"" : "";
        WriteHtml($"<li{activeClass}><a href=\"{HtmlEncode(Item.Href!)}\"");
        WriteHtml(ariaCurrent);
        WriteHtml(">");
        WriteText(Item.Label);
        if (Item.Badge is not null)
        {
            var badgeClass = BadgeCssClass(Item.Badge.Variant);
            WriteHtml($" <span class=\"{badgeClass}\">{HtmlEncode(Item.Badge.Text)}</span>");
        }

        WriteHtml("</a></li>");
        return Task.CompletedTask;
    }

    internal static string BadgeCssClass(BadgeVariant variant) => variant switch
    {
        BadgeVariant.Default => "sidebar-badge",
        _ => $"sidebar-badge sidebar-badge-{variant.ToString().ToLowerInvariant()}",
    };

    private static string HtmlEncode(string value) =>
        System.Net.WebUtility.HtmlEncode(value);
}
