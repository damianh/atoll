using System.Text.Json;
using System.Text.Json.Serialization;

namespace Atoll.Build.Ssg;

/// <summary>
/// Represents the build manifest generated after static site generation.
/// Contains information about all pages, assets, and routes for debugging,
/// sitemaps, preloading, and tooling integration.
/// </summary>
public sealed class BuildManifest
{
    /// <summary>
    /// Gets or sets the Atoll version that generated this manifest.
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Gets or sets the UTC timestamp when the build was generated.
    /// </summary>
    [JsonPropertyName("generatedAt")]
    public DateTimeOffset GeneratedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the base URL of the site.
    /// </summary>
    [JsonPropertyName("baseUrl")]
    public string BaseUrl { get; set; } = "";

    /// <summary>
    /// Gets or sets the base path prefix.
    /// </summary>
    [JsonPropertyName("basePath")]
    public string BasePath { get; set; } = "";

    /// <summary>
    /// Gets or sets the list of generated pages.
    /// </summary>
    [JsonPropertyName("pages")]
    public List<ManifestPage> Pages { get; set; } = [];

    /// <summary>
    /// Gets or sets the map of processed assets (key = original name, value = fingerprinted path).
    /// </summary>
    [JsonPropertyName("assets")]
    public Dictionary<string, ManifestAsset> Assets { get; set; } = [];

    /// <summary>
    /// Gets or sets build statistics.
    /// </summary>
    [JsonPropertyName("stats")]
    public ManifestStats Stats { get; set; } = new();
}

/// <summary>
/// Represents a single page in the build manifest.
/// </summary>
public sealed class ManifestPage
{
    /// <summary>
    /// Gets or sets the URL path for this page (e.g., <c>/about</c>).
    /// </summary>
    [JsonPropertyName("urlPath")]
    public string UrlPath { get; set; } = "";

    /// <summary>
    /// Gets or sets the output file path relative to the output directory.
    /// </summary>
    [JsonPropertyName("outputFile")]
    public string OutputFile { get; set; } = "";

    /// <summary>
    /// Gets or sets the component type name that generated this page.
    /// </summary>
    [JsonPropertyName("componentType")]
    public string ComponentType { get; set; } = "";

    /// <summary>
    /// Gets or sets the route parameters used for this page.
    /// </summary>
    [JsonPropertyName("parameters")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, string>? Parameters { get; set; }

    /// <summary>
    /// Gets or sets the render time in milliseconds.
    /// </summary>
    [JsonPropertyName("renderTimeMs")]
    public double RenderTimeMs { get; set; }
}

/// <summary>
/// Represents a processed asset in the build manifest.
/// </summary>
public sealed class ManifestAsset
{
    /// <summary>
    /// Gets or sets the original filename.
    /// </summary>
    [JsonPropertyName("originalName")]
    public string OriginalName { get; set; } = "";

    /// <summary>
    /// Gets or sets the fingerprinted output path.
    /// </summary>
    [JsonPropertyName("outputPath")]
    public string OutputPath { get; set; } = "";

    /// <summary>
    /// Gets or sets the content hash.
    /// </summary>
    [JsonPropertyName("hash")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Hash { get; set; }

    /// <summary>
    /// Gets or sets the MIME type of the asset.
    /// </summary>
    [JsonPropertyName("mimeType")]
    public string MimeType { get; set; } = "";

    /// <summary>
    /// Gets or sets the file size in bytes.
    /// </summary>
    [JsonPropertyName("sizeBytes")]
    public long SizeBytes { get; set; }
}

/// <summary>
/// Represents build statistics in the manifest.
/// </summary>
public sealed class ManifestStats
{
    /// <summary>
    /// Gets or sets the total number of pages generated.
    /// </summary>
    [JsonPropertyName("totalPages")]
    public int TotalPages { get; set; }

    /// <summary>
    /// Gets or sets the number of successfully generated pages.
    /// </summary>
    [JsonPropertyName("successPages")]
    public int SuccessPages { get; set; }

    /// <summary>
    /// Gets or sets the number of failed pages.
    /// </summary>
    [JsonPropertyName("failedPages")]
    public int FailedPages { get; set; }

    /// <summary>
    /// Gets or sets the total build time in milliseconds.
    /// </summary>
    [JsonPropertyName("totalBuildTimeMs")]
    public double TotalBuildTimeMs { get; set; }

    /// <summary>
    /// Gets or sets the total number of assets processed.
    /// </summary>
    [JsonPropertyName("totalAssets")]
    public int TotalAssets { get; set; }

    /// <summary>
    /// Gets or sets the total number of static files copied.
    /// </summary>
    [JsonPropertyName("staticFilesCopied")]
    public int StaticFilesCopied { get; set; }
}
