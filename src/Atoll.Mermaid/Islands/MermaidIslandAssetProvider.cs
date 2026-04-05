using System.Reflection;
using Atoll.Islands;

namespace Atoll.Mermaid.Islands;

/// <summary>
/// Provides the embedded JavaScript assets for <c>Atoll.Mermaid</c> islands.
/// These assets are written to the SSG output directory during the build pipeline.
/// </summary>
public sealed class MermaidIslandAssetProvider : IIslandAssetProvider
{
    private static readonly Assembly ResourceAssembly = typeof(MermaidIslandAssetProvider).Assembly;

    /// <inheritdoc/>
    public IEnumerable<IslandAssetDescriptor> GetAssets()
    {
        yield return new IslandAssetDescriptor(
            "scripts/atoll-docs-mermaid-init.js",
            "Atoll.Mermaid.Islands.Assets.mermaid-init.js",
            ResourceAssembly);
    }
}
