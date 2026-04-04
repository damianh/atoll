using System.Reflection;

namespace Atoll.Islands;

/// <summary>
/// Describes a single embedded JavaScript asset that should be written to the SSG output directory.
/// </summary>
public sealed class IslandAssetDescriptor
{
    /// <summary>
    /// Initializes a new <see cref="IslandAssetDescriptor"/>.
    /// </summary>
    /// <param name="outputPath">
    /// The relative path within the output directory where the asset should be written,
    /// e.g. <c>scripts/atoll-docs-search-dialog.js</c>. Must not have a leading slash.
    /// </param>
    /// <param name="resourceName">
    /// The fully-qualified name of the embedded resource, e.g.
    /// <c>Atoll.Lagoon.Islands.Assets.search-dialog.js</c>.
    /// </param>
    /// <param name="resourceAssembly">
    /// The assembly that contains the embedded resource.
    /// </param>
    public IslandAssetDescriptor(string outputPath, string resourceName, Assembly resourceAssembly)
    {
        ArgumentNullException.ThrowIfNull(outputPath);
        ArgumentNullException.ThrowIfNull(resourceName);
        ArgumentNullException.ThrowIfNull(resourceAssembly);

        OutputPath = outputPath;
        ResourceName = resourceName;
        ResourceAssembly = resourceAssembly;
    }

    /// <summary>
    /// Gets the relative output path for this asset (no leading slash).
    /// </summary>
    public string OutputPath { get; }

    /// <summary>
    /// Gets the fully-qualified embedded resource name.
    /// </summary>
    public string ResourceName { get; }

    /// <summary>
    /// Gets the assembly containing the embedded resource.
    /// </summary>
    public Assembly ResourceAssembly { get; }
}

/// <summary>
/// Implemented by library assemblies to declare embedded JavaScript assets that should be
/// written to the SSG output directory during the build pipeline.
/// </summary>
/// <remarks>
/// <para>
/// Implementations are discovered at build time via assembly scanning (no hard dependency
/// from <c>Atoll</c> core on library assemblies). The <see cref="GetAssets"/> method
/// returns descriptors that map embedded resources to their output paths.
/// </para>
/// </remarks>
public interface IIslandAssetProvider
{
    /// <summary>
    /// Returns the island asset descriptors for this provider.
    /// Each descriptor maps an embedded resource to its relative output path.
    /// </summary>
    IEnumerable<IslandAssetDescriptor> GetAssets();
}
