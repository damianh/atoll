namespace Atoll.Swell.Export;

/// <summary>
/// Options for exporting a Swell slide deck to a file.
/// </summary>
public sealed class ExportOptions
{
    /// <summary>
    /// Gets or sets the base URL of the running Atoll dev server (e.g. <c>http://localhost:4321</c>).
    /// Required for PDF and PPTX export which use a headless browser.
    /// </summary>
    public string BaseUrl { get; set; } = "http://localhost:4321";

    /// <summary>
    /// Gets or sets the path to the route of the slide deck (e.g. <c>/</c> or <c>/slides</c>).
    /// </summary>
    public string SlidePath { get; set; } = "/";

    /// <summary>
    /// Gets or sets the total number of slides in the deck.
    /// </summary>
    public int SlideCount { get; set; }

    /// <summary>
    /// Gets or sets the output file path (without extension — the exporter appends it).
    /// </summary>
    public string OutputPath { get; set; } = "dist/slides";

    /// <summary>
    /// Gets or sets the slide aspect ratio as a CSS value (e.g. <c>"16/9"</c>).
    /// Used to determine viewport dimensions for screenshots.
    /// Default: <c>"16/9"</c>.
    /// </summary>
    public string AspectRatio { get; set; } = "16/9";
}
