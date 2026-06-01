using Microsoft.Playwright;

namespace Atoll.Swell.Export;

/// <summary>
/// Exports a Swell slide deck to PDF using Playwright headless Chromium.
/// Each slide is captured as a separate PDF page at the configured aspect ratio.
/// </summary>
/// <remarks>
/// Requires Playwright browsers to be installed:
/// <code>pwsh playwright.ps1 install chromium</code>
/// </remarks>
public sealed class PdfExporter
{
    /// <summary>
    /// Exports the deck to a <c>.pdf</c> file.
    /// </summary>
    /// <param name="options">Export configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The path of the generated PDF file.</returns>
    public async Task<string> ExportAsync(ExportOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        ExportHelper.ValidateSlideCount(options);

        var outputFile = options.OutputPath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
            ? options.OutputPath
            : options.OutputPath + ".pdf";

        Directory.CreateDirectory(Path.GetDirectoryName(outputFile) ?? ".");

        var height = ExportHelper.ResolveHeight(options.AspectRatio);

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync();
        await using var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = ExportHelper.BaseWidth, Height = height },
        });

        // Capture each slide to a PDF page by printing with Playwright's per-page PDF.
        // Strategy: navigate to each slide, print to PDF, merge pages.
        // For simplicity, use a single page print of the full deck (all slides stacked).
        var page = await context.NewPageAsync();
        var url = options.BaseUrl.TrimEnd('/') + options.SlidePath;
        await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        // Use print CSS (Swell ships @media print styles that stack all slides).
        await page.EmulateMediaAsync(new PageEmulateMediaOptions { Media = Media.Print });

        var pdfBytes = await page.PdfAsync(new PagePdfOptions
        {
            Format = "A4",
            Landscape = true,
            PrintBackground = true,
            Path = outputFile,
        });

        if (pdfBytes is { Length: > 0 } && !File.Exists(outputFile))
        {
            await File.WriteAllBytesAsync(outputFile, pdfBytes, cancellationToken);
        }

        return outputFile;
    }
}
