using System.Reflection;
using Atoll.Islands;

namespace Atoll.Charts.Islands;

/// <summary>
/// Provides the embedded JavaScript assets for <c>Atoll.Charts</c> islands.
/// These assets are written to the SSG output directory during the build pipeline.
/// </summary>
public sealed class ChartIslandAssetProvider : IIslandAssetProvider
{
    private static readonly Assembly ResourceAssembly = typeof(ChartIslandAssetProvider).Assembly;

    /// <inheritdoc/>
    public IEnumerable<IslandAssetDescriptor> GetAssets()
    {
        yield return new IslandAssetDescriptor(
            "scripts/atoll-charts-vendor.min.js",
            "Atoll.Charts.Islands.Assets.chart.js.min.js",
            ResourceAssembly);

        yield return new IslandAssetDescriptor(
            "scripts/atoll-charts-init.js",
            "Atoll.Charts.Islands.Assets.chart-init.js",
            ResourceAssembly);
    }
}
