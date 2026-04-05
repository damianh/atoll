using Atoll.Components;
using Atoll.DrawIo.Parsing;
using Atoll.DrawIo.Rendering;
using Atoll.Islands;

namespace Atoll.DrawIo.Islands;

/// <summary>
/// An interactive draw.io diagram island with client-side pan, zoom, and layer toggling.
/// Renders the diagram as inline SVG on the server; the JavaScript module hydrates it
/// with interactivity when the element becomes visible.
/// </summary>
[ClientVisible]
public sealed class InteractiveDrawioDiagram : VanillaJsIsland
{
    /// <inheritdoc />
    public override string ClientModuleUrl => "/scripts/atoll-drawio-interactive.js";

    /// <summary>Gets or sets the path to the <c>.drawio</c> file to render.</summary>
    [Parameter(Required = true)]
    public string FilePath { get; set; } = string.Empty;

    /// <summary>Gets or sets the zero-based index of the page to render. Defaults to <c>0</c>.</summary>
    [Parameter]
    public int? Page { get; set; }

    /// <summary>Gets or sets the name of the page to render. Used when <see cref="Page"/> is not set.</summary>
    [Parameter]
    public string? PageName { get; set; }

    /// <summary>Gets or sets the SVG width attribute (e.g. <c>"800px"</c>, <c>"100%"</c>).</summary>
    [Parameter]
    public string? Width { get; set; }

    /// <summary>Gets or sets the SVG height attribute (e.g. <c>"600px"</c>, <c>"auto"</c>).</summary>
    [Parameter]
    public string? Height { get; set; }

    /// <summary>Gets or sets the explicit list of layer names/IDs to show. <c>null</c> means all layers.</summary>
    [Parameter]
    public IReadOnlyList<string>? VisibleLayers { get; set; }

    /// <summary>Gets or sets the list of layer names/IDs to hide.</summary>
    [Parameter]
    public IReadOnlyList<string>? HiddenLayers { get; set; }

    /// <summary>Gets or sets the SVG background color. Default is transparent.</summary>
    [Parameter]
    public string? Background { get; set; }

    /// <summary>Gets or sets whether to render layer toggle controls. Defaults to <c>true</c>.</summary>
    [Parameter]
    public bool ShowLayerControls { get; set; } = true;

    /// <summary>Gets or sets whether to enable pan and zoom. Defaults to <c>true</c>.</summary>
    [Parameter]
    public bool EnablePanZoom { get; set; } = true;

    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context)
    {
        var xml = File.ReadAllText(FilePath);
        var file = DrawioFileParser.Parse(xml);

        var options = new DrawioRenderOptions
        {
            PageIndex     = Page,
            PageName      = PageName,
            Width         = Width,
            Height        = Height,
            VisibleLayers = VisibleLayers,
            HiddenLayers  = HiddenLayers,
            Background    = Background,
        };

        var svg = DrawioSvgRenderer.RenderToSvg(file, options);

        // Resolve which page was rendered for metadata
        var pageIndex = Page ?? 0;
        var page = PageName != null
            ? file.Pages.FirstOrDefault(p => string.Equals(p.Name, PageName, StringComparison.OrdinalIgnoreCase))
              ?? (pageIndex < file.Pages.Count ? file.Pages[pageIndex] : null)
            : (pageIndex < file.Pages.Count ? file.Pages[pageIndex] : null);

        var layers = page?.Model.Layers ?? [];

        // Render container with layer metadata
        var enablePanZoomAttr = EnablePanZoom ? " data-pan-zoom=\"true\"" : string.Empty;
        WriteHtml($"<div class=\"drawio-interactive\"{enablePanZoomAttr}>");
        WriteHtml(svg);

        if (ShowLayerControls && layers.Count > 0)
        {
            WriteHtml("<div class=\"drawio-layer-controls\" aria-label=\"Diagram layers\">");
            foreach (var layer in layers)
            {
                var layerName = string.IsNullOrEmpty(layer.Value) ? layer.Id : layer.Value;
                var encodedName = System.Net.WebUtility.HtmlEncode(layerName);
                WriteHtml(
                    $"<button type=\"button\" class=\"drawio-layer-btn\" " +
                    $"data-layer-id=\"layer-{System.Net.WebUtility.HtmlEncode(layer.Id)}\" " +
                    $"data-layer-name=\"{encodedName}\">{encodedName}</button>");
            }
            WriteHtml("</div>");
        }

        WriteHtml("</div>");
        return Task.CompletedTask;
    }
}
