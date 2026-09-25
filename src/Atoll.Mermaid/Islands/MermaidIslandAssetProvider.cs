using System.Reflection;
using System.Text.Json;
using Atoll.Islands;

namespace Atoll.Mermaid.Islands;

/// <summary>
/// Provides the embedded JavaScript assets for <c>Atoll.Mermaid</c> islands.
/// These assets are written to the SSG output directory during the build pipeline.
/// </summary>
/// <remarks>
/// In addition to the initialisation script, this serves the bundled, pinned Mermaid build
/// listed in the embedded <c>vendor/mermaid/manifest.json</c> under
/// <c>scripts/atoll-mermaid/{version}/</c>.
/// </remarks>
public sealed class MermaidIslandAssetProvider : IIslandAssetProvider
{
    internal const string InitScriptOutputPath = "scripts/atoll-docs-mermaid-init.js";
    internal const string InitScriptResourceName = "Atoll.Mermaid.Islands.Assets.mermaid-init.js";
    internal const string VendorResourcePrefix = "Atoll.Mermaid.Vendor.mermaid/";
    internal const string ManifestResourceName = VendorResourcePrefix + "manifest.json";
    internal const string LicenseResourceName = VendorResourcePrefix + "LICENSE";

    private static readonly Assembly ResourceAssembly = typeof(MermaidIslandAssetProvider).Assembly;
    private static readonly Lazy<BundledMermaidManifest> LazyManifest = new(LoadManifest);

    /// <summary>
    /// Gets the manifest describing the bundled Mermaid build.
    /// </summary>
    internal static BundledMermaidManifest Manifest => LazyManifest.Value;

    /// <summary>
    /// Gets the output directory (no leading slash) of the bundled Mermaid build.
    /// </summary>
    internal static string BundledOutputDirectory => $"scripts/atoll-mermaid/{Manifest.Version}";

    /// <inheritdoc/>
    public IEnumerable<IslandAssetDescriptor> GetAssets()
    {
        yield return new IslandAssetDescriptor(InitScriptOutputPath, InitScriptResourceName, ResourceAssembly);

        var outputDirectory = BundledOutputDirectory;
        foreach (var path in Manifest.Files.Keys)
        {
            yield return new IslandAssetDescriptor(
                $"{outputDirectory}/{path}",
                VendorResourcePrefix + path,
                ResourceAssembly);
        }

        yield return new IslandAssetDescriptor($"{outputDirectory}/LICENSE", LicenseResourceName, ResourceAssembly);
    }

    private static BundledMermaidManifest LoadManifest()
    {
        using var stream = ResourceAssembly.GetManifestResourceStream(ManifestResourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{ManifestResourceName}' was not found.");
        return JsonSerializer.Deserialize<BundledMermaidManifest>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException($"Embedded resource '{ManifestResourceName}' is empty.");
    }
}

/// <summary>
/// The contents of the bundled Mermaid <c>manifest.json</c>.
/// </summary>
/// <param name="Package">The npm package name.</param>
/// <param name="Version">The bundled package version.</param>
/// <param name="Tarball">The npm tarball URL the files were extracted from.</param>
/// <param name="Integrity">The npm registry integrity (SRI) hash of the tarball.</param>
/// <param name="Files">Bundled file paths (relative, forward slashes) mapped to lowercase hex SHA-256 hashes.</param>
internal sealed record BundledMermaidManifest(
    string Package,
    string Version,
    string Tarball,
    string Integrity,
    IReadOnlyDictionary<string, string> Files);
