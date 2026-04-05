using System.Reflection;
using Atoll.Islands;

namespace Atoll.Giscus;

/// <summary>
/// Provides the embedded JavaScript assets for the <c>Atoll.Giscus</c> island.
/// The asset is written to the SSG output directory during the build pipeline.
/// </summary>
public sealed class GiscusIslandAssetProvider : IIslandAssetProvider
{
    private static readonly Assembly ResourceAssembly = typeof(GiscusIslandAssetProvider).Assembly;

    /// <inheritdoc/>
    public IEnumerable<IslandAssetDescriptor> GetAssets()
    {
        yield return new IslandAssetDescriptor(
            "scripts/atoll-giscus.js",
            "Atoll.Giscus.Islands.Assets.giscus-init.js",
            ResourceAssembly);
    }
}
