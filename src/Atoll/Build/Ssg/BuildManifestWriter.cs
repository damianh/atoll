using System.Text;
using System.Text.Json;
using Atoll.Build.Pipeline;

namespace Atoll.Build.Ssg;

/// <summary>
/// Writes a <see cref="BuildManifest"/> to the output directory as JSON.
/// The manifest is written to <c>.atoll/manifest.json</c> within the output directory.
/// </summary>
public sealed class BuildManifestWriter
{
    private const string ManifestDirectory = ".atoll";
    private const string ManifestFileName = "manifest.json";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly string _outputDirectory;

    /// <summary>
    /// Initializes a new <see cref="BuildManifestWriter"/> with the specified output directory.
    /// </summary>
    /// <param name="outputDirectory">The root output directory (e.g., <c>dist/</c>).</param>
    public BuildManifestWriter(string outputDirectory)
    {
        ArgumentNullException.ThrowIfNull(outputDirectory);
        _outputDirectory = outputDirectory;
    }

    /// <summary>
    /// Builds a <see cref="BuildManifest"/> from SSG and asset pipeline results.
    /// </summary>
    /// <param name="ssgResult">The SSG result containing page render results.</param>
    /// <param name="assetResult">The asset pipeline result, or <c>null</c> if no assets were processed.</param>
    /// <param name="options">The SSG options used for the build.</param>
    /// <returns>A populated <see cref="BuildManifest"/>.</returns>
    public static BuildManifest BuildFrom(SsgResult ssgResult, AssetPipelineResult? assetResult, SsgOptions options)
    {
        ArgumentNullException.ThrowIfNull(ssgResult);
        ArgumentNullException.ThrowIfNull(options);

        var manifest = new BuildManifest
        {
            BaseUrl = options.BaseUrl,
            BasePath = options.BasePath,
            GeneratedAt = DateTimeOffset.UtcNow,
        };

        // Add pages (only successful renders have output paths)
        foreach (var pageResult in ssgResult.PageResults)
        {
            var outputFile = pageResult.IsSuccess && pageResult.OutputPath.Length > 0
                ? Path.GetRelativePath(options.OutputDirectory, pageResult.OutputPath)
                : "";

            var page = new ManifestPage
            {
                UrlPath = pageResult.Route.UrlPath,
                OutputFile = outputFile,
                ComponentType = pageResult.Route.ComponentType.FullName ?? pageResult.Route.ComponentType.Name,
                RenderTimeMs = pageResult.Elapsed.TotalMilliseconds,
            };

            if (pageResult.Route.Parameters.Count > 0)
            {
                page.Parameters = new Dictionary<string, string>(pageResult.Route.Parameters);
            }

            manifest.Pages.Add(page);
        }

        // Add assets
        var assetCount = 0;
        if (assetResult is not null)
        {
            if (assetResult.Css.HasContent)
            {
                manifest.Assets["css"] = new ManifestAsset
                {
                    OriginalName = "styles.css",
                    OutputPath = assetResult.Css.OutputPath,
                    Hash = assetResult.Css.Hash,
                    MimeType = "text/css",
                    SizeBytes = Encoding.UTF8.GetByteCount(assetResult.Css.Css),
                };
                assetCount++;
            }

            if (assetResult.Js.HasContent)
            {
                manifest.Assets["js"] = new ManifestAsset
                {
                    OriginalName = "scripts.js",
                    OutputPath = assetResult.Js.OutputPath,
                    Hash = assetResult.Js.Hash,
                    MimeType = "application/javascript",
                    SizeBytes = Encoding.UTF8.GetByteCount(assetResult.Js.Js),
                };
                assetCount++;
            }
        }

        // Stats
        manifest.Stats = new ManifestStats
        {
            TotalPages = ssgResult.TotalCount,
            SuccessPages = ssgResult.SuccessCount,
            FailedPages = ssgResult.FailureCount,
            TotalBuildTimeMs = ssgResult.TotalElapsed.TotalMilliseconds,
            TotalAssets = assetCount,
            StaticFilesCopied = assetResult?.StaticAssets?.Count ?? 0,
        };

        return manifest;
    }

    /// <summary>
    /// Serializes a <see cref="BuildManifest"/> to a JSON string.
    /// </summary>
    /// <param name="manifest">The manifest to serialize.</param>
    /// <returns>The JSON string representation.</returns>
    public static string Serialize(BuildManifest manifest)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        return JsonSerializer.Serialize(manifest, SerializerOptions);
    }

    /// <summary>
    /// Writes a <see cref="BuildManifest"/> to the output directory as JSON.
    /// </summary>
    /// <param name="manifest">The manifest to write.</param>
    /// <param name="cancellationToken">A token to cancel the write operation.</param>
    /// <returns>The full file path that was written.</returns>
    public async Task<string> WriteAsync(BuildManifest manifest, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(manifest);

        var json = Serialize(manifest);
        var manifestDir = Path.Combine(_outputDirectory, ManifestDirectory);
        Directory.CreateDirectory(manifestDir);

        var filePath = Path.Combine(manifestDir, ManifestFileName);
        await File.WriteAllTextAsync(filePath, json, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), cancellationToken);

        return filePath;
    }

    /// <summary>
    /// Gets the expected manifest file path for the given output directory.
    /// </summary>
    /// <param name="outputDirectory">The output directory.</param>
    /// <returns>The full path to the manifest file.</returns>
    public static string GetManifestPath(string outputDirectory)
    {
        ArgumentNullException.ThrowIfNull(outputDirectory);
        return Path.Combine(outputDirectory, ManifestDirectory, ManifestFileName);
    }
}
