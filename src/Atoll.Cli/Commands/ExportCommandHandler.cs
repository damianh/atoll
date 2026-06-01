using Atoll.Swell.Export;

namespace Atoll.Cli.Commands;

/// <summary>
/// Handles the <c>atoll export</c> command. Exports a Swell slide deck to PDF, PPTX, or ODP.
/// </summary>
/// <remarks>
/// <para>
/// The export command requires a running Atoll dev or preview server to be accessible at
/// <c>--base-url</c>. Start the server first with <c>atoll dev</c>, then run the export.
/// </para>
/// <para>
/// PDF export requires Playwright browsers to be installed:
/// <c>pwsh playwright.ps1 install chromium</c>
/// </para>
/// </remarks>
public sealed class ExportCommandHandler
{
    /// <summary>
    /// Executes the export command.
    /// </summary>
    /// <param name="format">The export format: <c>pdf</c>, <c>pptx</c>, or <c>odp</c>.</param>
    /// <param name="output">The output file path (without extension).</param>
    /// <param name="baseUrl">The base URL of the running Atoll server.</param>
    /// <param name="slidePath">The URL path of the slide deck route.</param>
    /// <param name="slideCount">Total number of slides in the deck.</param>
    /// <param name="aspectRatio">CSS aspect-ratio value (e.g. "16/9").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task ExecuteAsync(
        string format,
        string output,
        string baseUrl,
        string slidePath,
        int slideCount,
        string aspectRatio,
        CancellationToken cancellationToken = default)
    {
        var options = new ExportOptions
        {
            BaseUrl = baseUrl,
            SlidePath = slidePath,
            SlideCount = slideCount,
            OutputPath = output,
            AspectRatio = aspectRatio,
        };

        Console.WriteLine($"Exporting to {format.ToUpperInvariant()} from {baseUrl}{slidePath}...");

        var outputFile = format.ToLowerInvariant() switch
        {
            "pdf"  => await ExportPdfAsync(options, cancellationToken),
            "pptx" => await ExportPptxAsync(options, cancellationToken),
            "odp"  => await ExportOdpAsync(options, cancellationToken),
            _      => throw new ArgumentException($"Unknown export format: '{format}'. Use pdf, pptx, or odp."),
        };

        Console.WriteLine($"Export complete: {outputFile}");
    }

    private static Task<string> ExportPdfAsync(ExportOptions options, CancellationToken ct)
    {
        var exporter = new PdfExporter();
        return exporter.ExportAsync(options, ct);
    }

    private static Task<string> ExportPptxAsync(ExportOptions options, CancellationToken ct)
    {
        var exporter = new PptxExporter();
        return exporter.ExportAsync(options, cancellationToken: ct);
    }

    private static Task<string> ExportOdpAsync(ExportOptions options, CancellationToken ct)
    {
        var exporter = new OdpExporter();
        return exporter.ExportAsync(options, cancellationToken: ct);
    }
}
