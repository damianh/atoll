using Microsoft.Playwright;

namespace Atoll.Swell.Export;

/// <summary>
/// Shared utilities for slide deck exporters (screenshot capture, aspect ratio resolution).
/// </summary>
internal static class ExportHelper
{
    internal const int BaseWidth = 1280;

    /// <summary>
    /// Resolves the viewport height in pixels from an aspect ratio string (e.g. "16/9").
    /// Falls back to 720 (16:9 at 1280px width) if the string is malformed.
    /// </summary>
    internal static int ResolveHeight(string aspectRatio)
    {
        var parts = aspectRatio.Split('/');
        if (parts.Length == 2
            && double.TryParse(parts[0], System.Globalization.CultureInfo.InvariantCulture, out var w)
            && double.TryParse(parts[1], System.Globalization.CultureInfo.InvariantCulture, out var h)
            && w > 0)
        {
            return (int)(BaseWidth / w * h);
        }

        return 720; // Default 16:9
    }

    /// <summary>
    /// Captures a PNG screenshot of each slide using Playwright headless Chromium.
    /// </summary>
    /// <param name="options">Export options containing base URL, slide path, slide count, and aspect ratio.</param>
    /// <param name="height">Viewport height in pixels.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of PNG byte arrays, one per slide.</returns>
    /// <exception cref="ArgumentException"><paramref name="options"/> has <see cref="ExportOptions.SlideCount"/> &lt;= 0.</exception>
    internal static async Task<List<byte[]>> CaptureScreenshotsAsync(
        ExportOptions options,
        int height,
        CancellationToken cancellationToken)
    {
        ValidateSlideCount(options);

        var screenshots = new List<byte[]>(options.SlideCount);
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync();
        await using var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = BaseWidth, Height = height },
        });

        var page = await context.NewPageAsync();
        var baseUrl = options.BaseUrl.TrimEnd('/') + options.SlidePath;

        for (var i = 0; i < options.SlideCount; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var url = baseUrl + "#/" + i;
            await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
            var png = await page.ScreenshotAsync(new PageScreenshotOptions { Type = ScreenshotType.Png });
            screenshots.Add(png);
        }

        return screenshots;
    }

    /// <summary>
    /// Validates that <see cref="ExportOptions.SlideCount"/> is positive.
    /// </summary>
    /// <exception cref="ArgumentException">Slide count is zero or negative.</exception>
    internal static void ValidateSlideCount(ExportOptions options)
    {
        if (options.SlideCount <= 0)
        {
            throw new ArgumentException(
                $"SlideCount must be greater than zero (was {options.SlideCount}). " +
                "Specify --slide-count when using the CLI.",
                nameof(options));
        }
    }
}
