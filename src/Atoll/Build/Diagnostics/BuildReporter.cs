using System.Text;
using Atoll.Build.Pipeline;
using Atoll.Build.Ssg;

namespace Atoll.Build.Diagnostics;

/// <summary>
/// Collects and reports build diagnostics. Produces a formatted summary
/// of the build results including page counts, asset sizes, timing, and warnings.
/// </summary>
public sealed class BuildReporter
{
    private readonly List<BuildDiagnostic> _diagnostics = [];

    /// <summary>
    /// Gets the diagnostics collected so far.
    /// </summary>
    public IReadOnlyList<BuildDiagnostic> Diagnostics => _diagnostics;

    /// <summary>
    /// Gets the count of warnings.
    /// </summary>
    public int WarningCount => _diagnostics.Count(d => d.Severity == DiagnosticSeverity.Warning);

    /// <summary>
    /// Gets the count of errors.
    /// </summary>
    public int ErrorCount => _diagnostics.Count(d => d.Severity == DiagnosticSeverity.Error);

    /// <summary>
    /// Gets whether any errors were reported.
    /// </summary>
    public bool HasErrors => _diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);

    /// <summary>
    /// Gets whether any warnings were reported.
    /// </summary>
    public bool HasWarnings => _diagnostics.Any(d => d.Severity == DiagnosticSeverity.Warning);

    /// <summary>
    /// Reports an informational diagnostic.
    /// </summary>
    /// <param name="message">The message text.</param>
    public void Info(string message)
    {
        ArgumentNullException.ThrowIfNull(message);
        _diagnostics.Add(new BuildDiagnostic(DiagnosticSeverity.Info, message));
    }

    /// <summary>
    /// Reports an informational diagnostic with source context.
    /// </summary>
    /// <param name="message">The message text.</param>
    /// <param name="source">The source file or route.</param>
    public void Info(string message, string source)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(source);
        _diagnostics.Add(new BuildDiagnostic(DiagnosticSeverity.Info, message, source));
    }

    /// <summary>
    /// Reports a warning diagnostic.
    /// </summary>
    /// <param name="message">The message text.</param>
    public void Warn(string message)
    {
        ArgumentNullException.ThrowIfNull(message);
        _diagnostics.Add(new BuildDiagnostic(DiagnosticSeverity.Warning, message));
    }

    /// <summary>
    /// Reports a warning diagnostic with source context.
    /// </summary>
    /// <param name="message">The message text.</param>
    /// <param name="source">The source file or route.</param>
    public void Warn(string message, string source)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(source);
        _diagnostics.Add(new BuildDiagnostic(DiagnosticSeverity.Warning, message, source));
    }

    /// <summary>
    /// Reports an error diagnostic.
    /// </summary>
    /// <param name="message">The message text.</param>
    public void Error(string message)
    {
        ArgumentNullException.ThrowIfNull(message);
        _diagnostics.Add(new BuildDiagnostic(DiagnosticSeverity.Error, message));
    }

    /// <summary>
    /// Reports an error diagnostic with source context.
    /// </summary>
    /// <param name="message">The message text.</param>
    /// <param name="source">The source file or route.</param>
    public void Error(string message, string source)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(source);
        _diagnostics.Add(new BuildDiagnostic(DiagnosticSeverity.Error, message, source));
    }

    /// <summary>
    /// Collects diagnostics from SSG results, including failed page errors.
    /// </summary>
    /// <param name="ssgResult">The SSG result to analyze.</param>
    public void CollectFromSsgResult(SsgResult ssgResult)
    {
        ArgumentNullException.ThrowIfNull(ssgResult);

        foreach (var page in ssgResult.PageResults)
        {
            if (!page.IsSuccess)
            {
                Error(
                    $"Failed to render: {page.Error?.Message ?? "Unknown error"}",
                    page.Route.UrlPath);
            }
            else if (page.Html.Length == 0)
            {
                Warn("Page rendered with empty content", page.Route.UrlPath);
            }
        }
    }

    /// <summary>
    /// Collects diagnostics from asset pipeline results.
    /// </summary>
    /// <param name="assetResult">The asset pipeline result to analyze.</param>
    public void CollectFromAssetResult(AssetPipelineResult assetResult)
    {
        ArgumentNullException.ThrowIfNull(assetResult);

        if (!assetResult.Css.HasContent && !assetResult.Js.HasContent)
        {
            Info("No CSS or JS assets produced");
        }
    }

    /// <summary>
    /// Formats a build summary string from the SSG and asset pipeline results.
    /// </summary>
    /// <param name="ssgResult">The SSG result.</param>
    /// <param name="assetResult">The asset pipeline result.</param>
    /// <param name="totalElapsed">The total build time.</param>
    /// <returns>A formatted summary string.</returns>
    public string FormatSummary(
        SsgResult ssgResult,
        AssetPipelineResult assetResult,
        TimeSpan totalElapsed)
    {
        ArgumentNullException.ThrowIfNull(ssgResult);
        ArgumentNullException.ThrowIfNull(assetResult);

        var sb = new StringBuilder();

        sb.AppendLine();
        sb.AppendLine("Build Summary");
        sb.AppendLine("─────────────");

        // Pages
        if (ssgResult.SkippedCount > 0)
        {
            sb.AppendLine($"  Pages:    {ssgResult.RenderedCount} rendered, {ssgResult.SkippedCount} skipped ({ssgResult.TotalCount} total)");
        }
        else
        {
            sb.AppendLine($"  Pages:    {ssgResult.SuccessCount} rendered ({ssgResult.TotalCount} total)");
        }

        if (ssgResult.FailureCount > 0)
        {
            sb.AppendLine($"  Failures: {ssgResult.FailureCount}");
        }

        // Page timing
        if (ssgResult.PageResults.Count > 0)
        {
            var successPages = ssgResult.PageResults.Where(p => p.IsSuccess).ToList();
            if (successPages.Count > 0)
            {
                var avgMs = successPages.Average(p => p.Elapsed.TotalMilliseconds);
                var maxMs = successPages.Max(p => p.Elapsed.TotalMilliseconds);
                var minMs = successPages.Min(p => p.Elapsed.TotalMilliseconds);
                sb.AppendLine($"  Render:   avg {avgMs:F1}ms, min {minMs:F1}ms, max {maxMs:F1}ms");
            }
        }

        // Assets
        if (assetResult.Css.HasContent)
        {
            var cssSize = FormatByteSize(assetResult.Css.Css.Length);
            sb.AppendLine($"  CSS:      {assetResult.Css.FileName} ({cssSize})");
        }

        if (assetResult.Js.HasContent)
        {
            var jsSize = FormatByteSize(assetResult.Js.Js.Length);
            sb.AppendLine($"  JS:       {assetResult.Js.FileName} ({jsSize})");
        }

        if (assetResult.StaticAssets is not null)
        {
            sb.AppendLine($"  Static:   {assetResult.StaticAssets.Count} files copied");
        }

        // Output sizes
        var totalHtmlSize = ssgResult.PageResults
            .Where(p => p.IsSuccess)
            .Sum(p => (long)p.Html.Length);
        sb.AppendLine($"  HTML:     {FormatByteSize(totalHtmlSize)} total");

        // Timing
        sb.AppendLine($"  SSG:      {ssgResult.TotalElapsed.TotalMilliseconds:F0}ms");
        sb.AppendLine($"  Assets:   {assetResult.Elapsed.TotalMilliseconds:F0}ms");
        sb.AppendLine($"  Total:    {totalElapsed.TotalMilliseconds:F0}ms");

        // Warnings and errors
        if (HasWarnings)
        {
            sb.AppendLine();
            sb.AppendLine("Warnings:");
            foreach (var diag in _diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning))
            {
                var source = diag.Source is not null ? $" [{diag.Source}]" : "";
                sb.AppendLine($"  WARN {diag.Message}{source}");
            }
        }

        if (HasErrors)
        {
            sb.AppendLine();
            sb.AppendLine("Errors:");
            foreach (var diag in _diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error))
            {
                var source = diag.Source is not null ? $" [{diag.Source}]" : "";
                sb.AppendLine($"  ERR  {diag.Message}{source}");
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Formats a list of individual page timings, sorted by render time (slowest first).
    /// </summary>
    /// <param name="ssgResult">The SSG result.</param>
    /// <param name="maxPages">The maximum number of pages to include. Defaults to 10.</param>
    /// <returns>A formatted string with per-page timing details.</returns>
    public static string FormatPageTimings(SsgResult ssgResult, int maxPages)
    {
        ArgumentNullException.ThrowIfNull(ssgResult);

        var sb = new StringBuilder();
        var sortedPages = ssgResult.PageResults
            .Where(p => p.IsSuccess)
            .OrderByDescending(p => p.Elapsed)
            .Take(maxPages)
            .ToList();

        if (sortedPages.Count == 0)
        {
            return "";
        }

        sb.AppendLine("Page Timings (slowest first):");
        foreach (var page in sortedPages)
        {
            sb.AppendLine($"  {page.Elapsed.TotalMilliseconds,7:F1}ms  {page.Route.UrlPath}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Formats a list of individual page timings, sorted by render time (slowest first).
    /// Uses a default maximum of 10 pages.
    /// </summary>
    /// <param name="ssgResult">The SSG result.</param>
    /// <returns>A formatted string with per-page timing details.</returns>
    public static string FormatPageTimings(SsgResult ssgResult)
    {
        return FormatPageTimings(ssgResult, 10);
    }

    /// <summary>
    /// Formats a byte size into a human-readable string (B, KB, MB).
    /// </summary>
    /// <param name="bytes">The size in bytes.</param>
    /// <returns>A human-readable size string.</returns>
    public static string FormatByteSize(long bytes)
    {
        return bytes switch
        {
            < 1024 => $"{bytes} B",
            < 1024 * 1024 => $"{bytes / 1024.0:F1} KB",
            _ => $"{bytes / (1024.0 * 1024.0):F2} MB",
        };
    }
}
