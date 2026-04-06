using System.Reflection;
using Atoll.Islands;

namespace Atoll.Annotations;

/// <summary>
/// Registers the client-side JavaScript assets for the <see cref="TextAnnotation"/> island.
/// </summary>
public sealed class AnnotationsIslandAssetProvider : IIslandAssetProvider
{
    private static readonly Assembly ResourceAssembly = typeof(AnnotationsIslandAssetProvider).Assembly;

    /// <inheritdoc />
    public IEnumerable<IslandAssetDescriptor> GetAssets()
    {
        yield return new IslandAssetDescriptor(
            "scripts/atoll-annotations.js",
            "Atoll.Annotations.Islands.Assets.annotations-init.js",
            ResourceAssembly);
    }
}
