using SkiaSharp;

namespace Atoll.Lagoon.OpenGraph;

/// <summary>
/// Generates branded 1200×630 OpenGraph PNG images using SkiaSharp.
/// Create a single instance per build run and reuse it across all pages.
/// </summary>
public sealed class OgImageRenderer : IDisposable
{
    private const int ImageWidth = 1200;
    private const int ImageHeight = 630;
    private const float Padding = 60f;

    private readonly OgImageRenderOptions _options;
    private readonly SKBitmap? _backgroundBitmap;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of <see cref="OgImageRenderer"/> with the specified options.
    /// </summary>
    /// <param name="options">The render options including background image, fonts, and colors.</param>
    public OgImageRenderer(OgImageRenderOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;

        if (options.BackgroundImageBytes is { Length: > 0 } bgBytes)
        {
            _backgroundBitmap = SKBitmap.Decode(bgBytes);
        }
    }

    /// <summary>
    /// Renders a 1200×630 PNG OG image for the given page input.
    /// </summary>
    /// <param name="input">The page metadata to render.</param>
    /// <returns>The PNG image bytes.</returns>
    public byte[] Render(OgImageInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        ObjectDisposedException.ThrowIf(_disposed, this);

        var info = new SKImageInfo(ImageWidth, ImageHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var surface = SKSurface.Create(info);
        var canvas = surface.Canvas;

        DrawBackground(canvas);

        var category = ResolveCategory(input);
        var contentWidth = ImageWidth - (Padding * 2);
        var titleTop = DrawCategoryAndComputeTitleTop(canvas, category);
        var descriptionTop = DrawTitle(canvas, input.Title, contentWidth, titleTop);
        DrawDescription(canvas, input.Description, contentWidth, descriptionTop);

        canvas.Flush();

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    private void DrawBackground(SKCanvas canvas)
    {
        if (_backgroundBitmap is not null)
        {
            var destRect = new SKRect(0, 0, ImageWidth, ImageHeight);
            var sampling = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear);
            using var bgImage = SKImage.FromBitmap(_backgroundBitmap);
            canvas.DrawImage(bgImage, destRect, sampling);
        }
        else
        {
            // Solid dark fallback
            canvas.Clear(new SKColor(0x1A, 0x1A, 0x2E));
        }
    }

    private string? ResolveCategory(OgImageInput input)
    {
        if (input.Category is not null)
        {
            return input.Category;
        }

        // Auto-detect from the first URL path segment
        var slug = input.Slug.TrimStart('/');
        var firstSlash = slug.IndexOf('/', StringComparison.Ordinal);
        var firstSegment = firstSlash >= 0 ? slug[..firstSlash] : slug;

        if (!string.IsNullOrEmpty(firstSegment)
            && _options.Categories.TryGetValue(firstSegment, out var label))
        {
            return label;
        }

        return null;
    }

    private float DrawCategoryAndComputeTitleTop(SKCanvas canvas, string? category)
    {
        if (category is null)
        {
            return Padding + 80f;
        }

        using var font = CreateFont(_options.CategoryFontSize, italic: true);
        using var paint = new SKPaint { Color = _options.CategoryColor, IsAntialias = true };
        var y = ImageHeight - Padding - _options.CategoryFontSize;
        canvas.DrawText(category, Padding, y, SKTextAlign.Left, font, paint);

        return Padding + 80f;
    }

    private float DrawTitle(SKCanvas canvas, string title, float contentWidth, float topY)
    {
        using var font = CreateFont(_options.TitleFontSize, bold: true);
        using var paint = new SKPaint { Color = _options.TitleColor, IsAntialias = true };
        var lineHeight = _options.TitleFontSize * 1.2f;
        var lines = WrapText(title, font, contentWidth);

        var y = topY + _options.TitleFontSize;
        foreach (var line in lines)
        {
            canvas.DrawText(line, Padding, y, SKTextAlign.Left, font, paint);
            y += lineHeight;
        }

        return y + 16f;
    }

    private void DrawDescription(SKCanvas canvas, string? description, float contentWidth, float topY)
    {
        if (string.IsNullOrEmpty(description))
        {
            return;
        }

        using var font = CreateFont(_options.DescriptionFontSize);
        using var paint = new SKPaint { Color = _options.DescriptionColor, IsAntialias = true };
        var lineHeight = _options.DescriptionFontSize * 1.3f;
        var lines = WrapText(description, font, contentWidth, maxLines: 3);

        var y = topY + _options.DescriptionFontSize;
        foreach (var line in lines)
        {
            canvas.DrawText(line, Padding, y, SKTextAlign.Left, font, paint);
            y += lineHeight;
        }
    }

    private SKFont CreateFont(float size, bool bold = false, bool italic = false)
    {
        var style = (bold, italic) switch
        {
            (true, true) => SKFontStyle.BoldItalic,
            (true, false) => SKFontStyle.Bold,
            (false, true) => SKFontStyle.Italic,
            _ => SKFontStyle.Normal,
        };

        if (_options.Typeface is not null)
        {
            var styledTypeface = SKTypeface.FromFamilyName(
                _options.Typeface.FamilyName,
                style);
            return new SKFont(styledTypeface, size);
        }

        var typeface = SKTypeface.FromFamilyName(null, style);
        return new SKFont(typeface, size);
    }

    private static IReadOnlyList<string> WrapText(string text, SKFont font, float maxWidth, int maxLines = 10)
    {
        var lines = new List<string>();
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var current = new System.Text.StringBuilder();

        foreach (var word in words)
        {
            if (lines.Count >= maxLines)
            {
                break;
            }

            var candidate = current.Length == 0 ? word : current + " " + word;
            if (font.MeasureText(candidate) <= maxWidth)
            {
                if (current.Length > 0)
                {
                    current.Append(' ');
                }

                current.Append(word);
            }
            else
            {
                if (current.Length > 0)
                {
                    lines.Add(current.ToString());
                    current.Clear();
                }

                // If a single word is too long, add it anyway rather than looping forever
                current.Append(word);
            }
        }

        if (current.Length > 0 && lines.Count < maxLines)
        {
            lines.Add(current.ToString());
        }

        return lines;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _backgroundBitmap?.Dispose();
        _disposed = true;
    }
}
