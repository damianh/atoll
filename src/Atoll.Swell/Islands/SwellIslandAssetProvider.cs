using System.Reflection;
using Atoll.Islands;

namespace Atoll.Swell.Islands;

/// <summary>
/// Provides the embedded JavaScript assets for <c>Atoll.Swell</c> islands.
/// These assets are written to the SSG output directory during the build pipeline.
/// </summary>
public sealed class SwellIslandAssetProvider : IIslandAssetProvider
{
    private static readonly Assembly ResourceAssembly = typeof(SwellIslandAssetProvider).Assembly;

    /// <inheritdoc/>
    public IEnumerable<IslandAssetDescriptor> GetAssets()
    {
        yield return new IslandAssetDescriptor(
            "scripts/atoll-swell-nav.js",
            "Atoll.Swell.Islands.Assets.swell-nav.js",
            ResourceAssembly);

        yield return new IslandAssetDescriptor(
            "scripts/atoll-swell-presenter.js",
            "Atoll.Swell.Islands.Assets.swell-presenter.js",
            ResourceAssembly);

        yield return new IslandAssetDescriptor(
            "scripts/atoll-swell-drawing.js",
            "Atoll.Swell.Islands.Assets.swell-drawing.js",
            ResourceAssembly);
    }
}
