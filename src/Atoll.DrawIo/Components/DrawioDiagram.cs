using Atoll.Components;
using Atoll.Css;
using Atoll.DrawIo.Parsing;
using Atoll.DrawIo.Rendering;

namespace Atoll.DrawIo.Components;

/// <summary>
/// An Atoll component that renders a draw.io diagram file as inline SVG.
/// Requires no JavaScript — the diagram is fully rendered on the server side.
/// </summary>
[Styles(".drawio-diagram { display: block; max-width: 100%; } .drawio-diagram svg { width: 100%; height: auto; }")]
public sealed class DrawioDiagram : AtollComponent
{
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

    /// <summary>Gets or sets the SVG background color (e.g. <c>"#ffffff"</c>). Default is transparent.</summary>
    [Parameter]
    public string? Background { get; set; }

    /// <summary>Gets or sets the accessible label for the diagram (used as <c>aria-label</c>).</summary>
    [Parameter]
    public string? Alt { get; set; }

    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context)
    {
        var xml = File.ReadAllText(FilePath);
        var file = DrawioFileParser.Parse(xml);

        var options = new DrawioRenderOptions
        {
            PageIndex    = Page,
            PageName     = PageName,
            Width        = Width,
            Height       = Height,
            VisibleLayers = VisibleLayers,
            HiddenLayers  = HiddenLayers,
            Background   = Background,
        };

        var svg = DrawioSvgRenderer.RenderToSvg(file, options);

        var ariaLabel = string.IsNullOrEmpty(Alt) ? string.Empty : $" aria-label=\"{System.Web.HttpUtility.HtmlEncode(Alt)}\"";
        var role = string.IsNullOrEmpty(Alt) ? string.Empty : " role=\"img\"";

        WriteHtml($"<div class=\"drawio-diagram\"{role}{ariaLabel}>");
        WriteHtml(svg);
        WriteHtml("</div>");

        return Task.CompletedTask;
    }
}
