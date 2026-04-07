using Atoll.Components;
using Atoll.Css;
using Atoll.Islands;

namespace Atoll.Charts.Islands;

/// <summary>
/// An Atoll island component that renders a Chart.js chart using the official
/// Chart.js UMD bundle for client-side rendering.
/// Emits a <c>&lt;canvas data-chart-config="..."&gt;</c> element; the Chart.js
/// library reads the config and renders the chart on the client.
/// </summary>
/// <remarks>
/// JavaScript is required to display the chart. A <c>&lt;noscript&gt;</c>
/// fallback message is included for environments with JS disabled.
/// The <c>ConfigJson</c> must be a valid Chart.js configuration object in JSON format:
/// <code>{ "type": "bar", "data": { ... }, "options": { ... } }</code>
/// </remarks>
[ClientVisible]
[Styles(".atoll-chart { position: relative; width: 100%; }")]
public sealed class ChartIsland : VanillaJsIsland
{
    /// <inheritdoc />
    public override string ClientModuleUrl => "/scripts/atoll-charts-init.js";

    /// <summary>Gets or sets the Chart.js configuration JSON (<c>{ type, data, options }</c>).</summary>
    [Parameter(Required = true)]
    public string ConfigJson { get; set; } = string.Empty;

    /// <summary>Gets or sets the accessible label for the chart (used as <c>aria-label</c>).</summary>
    [Parameter]
    public string? Alt { get; set; }

    /// <summary>Gets or sets the optional canvas width attribute in pixels.</summary>
    [Parameter]
    public int? Width { get; set; }

    /// <summary>Gets or sets the optional canvas height attribute in pixels.</summary>
    [Parameter]
    public int? Height { get; set; }

    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context)
    {
        if (string.IsNullOrWhiteSpace(ConfigJson))
        {
            WriteHtml("<div class=\"atoll-chart atoll-chart-error\">Chart configuration is empty.</div>");
            return Task.CompletedTask;
        }

        var encodedConfig = System.Web.HttpUtility.HtmlAttributeEncode(ConfigJson);

        var ariaLabel = string.IsNullOrEmpty(Alt)
            ? string.Empty
            : $" aria-label=\"{System.Web.HttpUtility.HtmlAttributeEncode(Alt)}\"";
        var role = string.IsNullOrEmpty(Alt) ? string.Empty : " role=\"img\"";

        var widthAttr = Width.HasValue ? $" width=\"{Width.Value}\"" : string.Empty;
        var heightAttr = Height.HasValue ? $" height=\"{Height.Value}\"" : string.Empty;

        WriteHtml($"<div class=\"atoll-chart\"{role}{ariaLabel}>");
        WriteHtml($"<canvas data-chart-config=\"{encodedConfig}\"{widthAttr}{heightAttr}></canvas>");
        WriteHtml("<noscript>Chart requires JavaScript to display.</noscript>");
        WriteHtml("</div>");

        return Task.CompletedTask;
    }
}
