namespace Atoll.DrawIo.Rendering;

/// <summary>
/// Controls how a draw.io diagram is rendered to SVG.
/// </summary>
public sealed class DrawioRenderOptions
{
    /// <summary>
    /// Gets or sets the zero-based page index to render.
    /// Defaults to <c>0</c> (first page). Takes precedence over <see cref="PageName"/>
    /// when both are set.
    /// </summary>
    public int? PageIndex { get; set; }

    /// <summary>
    /// Gets or sets the name of the page to render.
    /// Used when <see cref="PageIndex"/> is <c>null</c>.
    /// </summary>
    public string? PageName { get; set; }

    /// <summary>
    /// Gets or sets the SVG <c>width</c> attribute value (e.g. <c>"800px"</c>, <c>"100%"</c>).
    /// When <c>null</c> the width attribute is omitted and the viewBox controls sizing.
    /// </summary>
    public string? Width { get; set; }

    /// <summary>
    /// Gets or sets the SVG <c>height</c> attribute value (e.g. <c>"600px"</c>, <c>"auto"</c>).
    /// When <c>null</c> the height attribute is omitted.
    /// </summary>
    public string? Height { get; set; }

    /// <summary>
    /// Gets or sets the explicit list of layer names or IDs to show.
    /// When <c>null</c> all layers are visible.
    /// Takes precedence over <see cref="HiddenLayers"/>.
    /// </summary>
    public IReadOnlyList<string>? VisibleLayers { get; set; }

    /// <summary>
    /// Gets or sets the list of layer names or IDs to hide.
    /// Ignored when <see cref="VisibleLayers"/> is set.
    /// </summary>
    public IReadOnlyList<string>? HiddenLayers { get; set; }

    /// <summary>
    /// Gets or sets the SVG background color (e.g. <c>"#ffffff"</c>, <c>"transparent"</c>).
    /// Defaults to <c>null</c> (no background rectangle is emitted).
    /// </summary>
    public string? Background { get; set; }

    /// <summary>
    /// Gets or sets the padding in user units to add around the diagram content.
    /// Defaults to <c>10</c>.
    /// </summary>
    public double Padding { get; set; } = 10;
}
