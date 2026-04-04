using System.Reflection;
using Atoll.Islands;

namespace Atoll.Lagoon.Islands;

/// <summary>
/// Provides the embedded JavaScript assets for <c>Atoll.Lagoon</c> islands.
/// These assets are written to the SSG output directory during the build pipeline.
/// </summary>
public sealed class LagoonIslandAssetProvider : IIslandAssetProvider
{
    private static readonly Assembly ResourceAssembly = typeof(LagoonIslandAssetProvider).Assembly;

    /// <inheritdoc/>
    public IEnumerable<IslandAssetDescriptor> GetAssets()
    {
        yield return new IslandAssetDescriptor(
            "scripts/atoll-docs-search-dialog.js",
            "Atoll.Lagoon.Islands.Assets.search-dialog.js",
            ResourceAssembly);

        yield return new IslandAssetDescriptor(
            "scripts/atoll-theme-toggle.js",
            "Atoll.Lagoon.Islands.Assets.theme-toggle.js",
            ResourceAssembly);

        yield return new IslandAssetDescriptor(
            "scripts/atoll-docs-mobile-nav.js",
            "Atoll.Lagoon.Islands.Assets.mobile-nav.js",
            ResourceAssembly);

        yield return new IslandAssetDescriptor(
            "scripts/atoll-docs-mermaid-init.js",
            "Atoll.Lagoon.Islands.Assets.mermaid-init.js",
            ResourceAssembly);
    }
}
