using System.Text;

namespace Atoll.Cli.Output;

/// <summary>
/// Formats a concise build summary for <c>atoll dev</c> rebuilds.
/// Unlike <see cref="Atoll.Build.Diagnostics.BuildReporter"/>, the dev server does not
/// produce <c>SsgResult</c> or <c>AssetPipelineResult</c>, so this formats the
/// lighter-weight metrics available after a dev rebuild.
/// </summary>
internal static class DevBuildSummary
{
    /// <summary>
    /// Formats a dev rebuild summary string.
    /// </summary>
    /// <param name="routeCount">The number of routes discovered.</param>
    /// <param name="islandAssetCount">The number of island JS assets loaded.</param>
    /// <param name="hasGlobalCss">Whether global CSS was found.</param>
    /// <param name="hasSearchIndex">Whether a search index was generated.</param>
    /// <param name="elapsedMilliseconds">Total elapsed time in milliseconds.</param>
    /// <returns>A formatted multi-line summary string.</returns>
    public static string Format(
        int routeCount,
        int islandAssetCount,
        bool hasGlobalCss,
        bool hasSearchIndex,
        long elapsedMilliseconds)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"  Routes:  {routeCount} discovered");

        if (islandAssetCount > 0)
        {
            sb.AppendLine($"  Islands: {islandAssetCount} JS asset(s)");
        }

        if (hasGlobalCss)
        {
            sb.AppendLine("  CSS:     global styles loaded");
        }

        if (hasSearchIndex)
        {
            sb.AppendLine("  Search:  index generated");
        }

        sb.Append($"  Time:    {elapsedMilliseconds}ms");

        return sb.ToString();
    }
}
