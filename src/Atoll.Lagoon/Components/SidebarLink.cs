using Atoll.Components;
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
            WriteHtml($" <span class=\"badge\">{HtmlEncode(Item.Badge)}</span>");
        }

        WriteHtml("</a></li>");
        return Task.CompletedTask;
    }

    private static string HtmlEncode(string value) =>
        System.Net.WebUtility.HtmlEncode(value);
}
