using System.Reflection;
using Atoll.Islands;

namespace Atoll.DrawIo.Islands;

/// <summary>
/// Provides the embedded JavaScript assets for <c>Atoll.DrawIo</c> islands.
/// These assets are written to the SSG output directory during the build pipeline.
/// </summary>
public sealed class DrawIoIslandAssetProvider : IIslandAssetProvider
{
    private static readonly Assembly ResourceAssembly = typeof(DrawIoIslandAssetProvider).Assembly;

    /// <inheritdoc/>
    public IEnumerable<IslandAssetDescriptor> GetAssets()
    {
        yield return new IslandAssetDescriptor(
            "scripts/atoll-drawio-interactive.js",
            "Atoll.DrawIo.Islands.Assets.drawio-interactive.js",
            ResourceAssembly);
    }
}
