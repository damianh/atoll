using System.Reflection;
using Atoll.Islands;

namespace Atoll.Reef.Islands;

/// <summary>
/// Provides the embedded JavaScript assets for <c>Atoll.Reef</c> islands.
/// These assets are written to the SSG output directory during the build pipeline.
/// </summary>
public sealed class ReefIslandAssetProvider : IIslandAssetProvider
{
    private static readonly Assembly ResourceAssembly = typeof(ReefIslandAssetProvider).Assembly;

    /// <inheritdoc/>
    public IEnumerable<IslandAssetDescriptor> GetAssets()
    {
        yield return new IslandAssetDescriptor(
            "scripts/atoll-reef-article-filter.js",
            "Atoll.Reef.Islands.Assets.article-filter.js",
            ResourceAssembly);

        yield return new IslandAssetDescriptor(
            "scripts/atoll-reef-view-toggle.js",
            "Atoll.Reef.Islands.Assets.view-toggle.js",
            ResourceAssembly);
    }
}
