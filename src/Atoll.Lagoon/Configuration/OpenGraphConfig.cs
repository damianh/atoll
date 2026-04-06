namespace Atoll.Lagoon.Configuration;

/// <summary>
/// Configuration for automatic OpenGraph (OG) image generation during the build step.
/// When set on <see cref="DocsConfig.OpenGraph"/>, branded 1200×630 PNG images are generated
/// for each document page and OG/Twitter Card meta tags are rendered in the document head.
/// </summary>
public sealed class OpenGraphConfig
{
    /// <summary>
    /// Gets or sets the path to the background image file (PNG/JPEG) used as the base for each
    /// generated OG image. The path is resolved relative to the project root.
    /// </summary>
    public string? BackgroundImagePath { get; set; }

    /// <summary>
    /// Gets or sets the path to a TTF or OTF font file to use for text rendering.
    /// When <c>null</c>, the SkiaSharp default system typeface is used.
    /// The path is resolved relative to the project root.
    /// </summary>
    public string? FontPath { get; set; }

    /// <summary>
    /// Gets or sets the font size in points for the page title text.
    /// Default: <c>60</c>.
    /// </summary>
    public float TitleFontSize { get; set; } = 60f;

    /// <summary>
    /// Gets or sets the font size in points for the page description text.
    /// Default: <c>32</c>.
    /// </summary>
    public float DescriptionFontSize { get; set; } = 32f;

    /// <summary>
    /// Gets or sets the font size in points for the category label.
    /// Default: <c>28</c>.
    /// </summary>
    public float CategoryFontSize { get; set; } = 28f;

    /// <summary>
    /// Gets or sets the hex color string for the title text (e.g. <c>"#FFFFFF"</c>).
    /// Default: white (<c>"#FFFFFF"</c>).
    /// </summary>
    public string TitleColor { get; set; } = "#FFFFFF";

    /// <summary>
    /// Gets or sets the hex color string for the description text (e.g. <c>"#EEEEEE"</c>).
    /// Default: near-white (<c>"#EEEEEE"</c>).
    /// </summary>
    public string DescriptionColor { get; set; } = "#EEEEEE";

    /// <summary>
    /// Gets or sets the hex color string for the category label text (e.g. <c>"#AAAAAA"</c>).
    /// Default: gray (<c>"#AAAAAA"</c>).
    /// </summary>
    public string CategoryColor { get; set; } = "#AAAAAA";

    /// <summary>
    /// Gets or sets a mapping from the first URL path segment to a display label used as the
    /// category text on generated OG images.
    /// For example: <c>{ "identityserver", "IdentityServer" }</c> maps pages under
    /// <c>/identityserver/</c> to the label <c>"IdentityServer"</c>.
    /// When no matching entry is found, the category label is omitted.
    /// </summary>
    public IReadOnlyDictionary<string, string> Categories { get; set; }
        = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}
