using Atoll.Lagoon.OpenGraph;
using Shouldly;
using SkiaSharp;
using Xunit;

namespace Atoll.Lagoon.Tests.OpenGraph;

public sealed class OgImageRendererTests
{
    private static OgImageRenderOptions DefaultOptions() => new();

    [Fact]
    public void ShouldGenerateImageWithCorrectDimensions()
    {
        using var renderer = new OgImageRenderer(DefaultOptions());
        var input = new OgImageInput("Hello World", "/test/page", null, null);

        var pngBytes = renderer.Render(input);

        pngBytes.ShouldNotBeNull();
        pngBytes.Length.ShouldBeGreaterThan(0);

        using var bitmap = SKBitmap.Decode(pngBytes);
        bitmap.ShouldNotBeNull();
        bitmap.Width.ShouldBe(1200);
        bitmap.Height.ShouldBe(630);
    }

    [Fact]
    public void ShouldReturnValidPngMagicBytes()
    {
        using var renderer = new OgImageRenderer(DefaultOptions());
        var input = new OgImageInput("Test Title", "/test", null, null);

        var pngBytes = renderer.Render(input);

        // PNG magic bytes: 89 50 4E 47 0D 0A 1A 0A
        pngBytes[0].ShouldBe((byte)0x89);
        pngBytes[1].ShouldBe((byte)0x50); // 'P'
        pngBytes[2].ShouldBe((byte)0x4E); // 'N'
        pngBytes[3].ShouldBe((byte)0x47); // 'G'
    }

    [Fact]
    public void ShouldHandleLongTitleWithWordWrapping()
    {
        using var renderer = new OgImageRenderer(DefaultOptions());
        var input = new OgImageInput(
            "This Is A Very Long Title That Exceeds The Width Of The Image And Should Be Word Wrapped Correctly",
            "/test/long-title",
            null,
            null);

        // Should not throw
        var pngBytes = renderer.Render(input);
        pngBytes.Length.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void ShouldHandleNullDescription()
    {
        using var renderer = new OgImageRenderer(DefaultOptions());
        var input = new OgImageInput("Title Only", "/test/no-desc", null, null);

        // Should not throw
        var pngBytes = renderer.Render(input);
        pngBytes.Length.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void ShouldAutoDetectCategoryFromSlugFirstSegment()
    {
        var options = new OgImageRenderOptions
        {
            Categories = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["identityserver"] = "IdentityServer",
                ["bff"] = "BFF",
            },
        };

        using var renderer = new OgImageRenderer(options);
        var input = new OgImageInput(
            "Big Picture",
            "/identityserver/overview/big-picture",
            "An overview of IdentityServer.",
            null);

        // Should not throw and renders a non-empty PNG
        var pngBytes = renderer.Render(input);
        pngBytes.Length.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void ShouldUseFallbackBackgroundWhenNoBytesProvided()
    {
        var options = new OgImageRenderOptions
        {
            BackgroundImageBytes = null,
        };

        using var renderer = new OgImageRenderer(options);
        var input = new OgImageInput("Fallback Test", "/fallback", null, null);

        var pngBytes = renderer.Render(input);

        using var bitmap = SKBitmap.Decode(pngBytes);
        bitmap.Width.ShouldBe(1200);
        bitmap.Height.ShouldBe(630);
    }

    [Fact]
    public void ShouldRenderNonBlankImage()
    {
        using var renderer = new OgImageRenderer(DefaultOptions());
        var input = new OgImageInput("Non-Blank Title", "/test/non-blank", "Some description here.", null);

        var pngBytes = renderer.Render(input);

        using var bitmap = SKBitmap.Decode(pngBytes);

        // Sample a few pixels — they should not all be transparent (alpha = 0)
        var pixel = bitmap.GetPixel(600, 315);
        pixel.Alpha.ShouldBeGreaterThan((byte)0);
    }
}
