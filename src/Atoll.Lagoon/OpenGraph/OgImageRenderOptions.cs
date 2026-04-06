using SkiaSharp;

namespace Atoll.Lagoon.OpenGraph;

/// <summary>
/// Options for configuring an <see cref="OgImageRenderer"/> instance.
/// Derived from <see cref="Atoll.Lagoon.Configuration.OpenGraphConfig"/> and pre-loaded
/// binary data (font, background image) so the renderer can be reused across multiple images.
/// </summary>
public sealed class OgImageRenderOptions
{
    /// <summary>Gets or sets the background image bytes (PNG/JPEG), or <c>null</c> to use a solid color fallback.</summary>
    public byte[]? BackgroundImageBytes { get; set; }

    /// <summary>Gets or sets the typeface to use for text rendering. When <c>null</c>, the SkiaSharp default typeface is used.</summary>
    public SKTypeface? Typeface { get; set; }

    /// <summary>Gets or sets the font size in points for the title text. Default: <c>60</c>.</summary>
    public float TitleFontSize { get; set; } = 60f;

    /// <summary>Gets or sets the font size in points for the description text. Default: <c>32</c>.</summary>
    public float DescriptionFontSize { get; set; } = 32f;

    /// <summary>Gets or sets the font size in points for the category label. Default: <c>28</c>.</summary>
    public float CategoryFontSize { get; set; } = 28f;

    /// <summary>Gets or sets the ARGB color for the title text. Default: white.</summary>
    public SKColor TitleColor { get; set; } = SKColors.White;

    /// <summary>Gets or sets the ARGB color for the description text. Default: near-white.</summary>
    public SKColor DescriptionColor { get; set; } = new SKColor(0xEE, 0xEE, 0xEE);

    /// <summary>Gets or sets the ARGB color for the category label text. Default: gray.</summary>
    public SKColor CategoryColor { get; set; } = new SKColor(0xAA, 0xAA, 0xAA);

    /// <summary>Gets or sets the category mapping (first URL segment → display label).</summary>
    public IReadOnlyDictionary<string, string> Categories { get; set; }
        = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}
