using Atoll.Components;
using Atoll.Islands;
using Atoll.Reef.Configuration;

namespace Atoll.Reef.Islands;

/// <summary>
/// A client-side view toggle island that lets users switch between list, grid, and table
/// article listing views. Hydrates on load so the button state matches any persisted preference.
/// </summary>
[ClientLoad]
public sealed class ViewToggle : VanillaJsIsland
{
    /// <inheritdoc />
    public override string ClientModuleUrl => "/scripts/atoll-reef-view-toggle.js";

    /// <summary>Gets or sets the currently active view.</summary>
    [Parameter]
    public DefaultView CurrentView { get; set; } = DefaultView.List;

    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<div class=\"view-toggle\" data-view-toggle role=\"group\" aria-label=\"Switch article view\">");
        WriteViewButton("list", "List", CurrentView == DefaultView.List);
        WriteViewButton("grid", "Grid", CurrentView == DefaultView.Grid);
        WriteViewButton("table", "Table", CurrentView == DefaultView.Table);
        WriteHtml("</div>");
        return Task.CompletedTask;
    }

    private void WriteViewButton(string view, string label, bool isActive)
    {
        var pressed = isActive ? "true" : "false";
        var active = isActive ? " view-toggle__btn--active" : "";
        WriteHtml($"<button class=\"view-toggle__btn{active}\" data-view-btn=\"{view}\" aria-pressed=\"{pressed}\">{label}</button>");
    }
}
