using System.Text.Json.Serialization;

namespace Atoll.Build.Ssg;

/// <summary>
/// Internal build cache that stores hashes from a previous build to enable incremental builds.
/// Stored at <c>{projectRoot}/.atoll/build-cache-{outputDirHash}.json</c>.
/// </summary>
/// <remarks>
/// Separate from <see cref="BuildManifest"/>, which is a public build artifact.
/// The cache is an internal tool artefact and should be gitignored.
/// </remarks>
internal sealed class BuildCache
{
    /// <summary>The current schema version of the cache format.</summary>
    public const string CurrentCacheVersion = "1";

    [JsonPropertyName("cacheVersion")]
    public string CacheVersion { get; set; } = CurrentCacheVersion;

    [JsonPropertyName("atollVersion")]
    public string AtollVersion { get; set; } = "";

    [JsonPropertyName("assemblyHash")]
    public string AssemblyHash { get; set; } = "";

    [JsonPropertyName("contentHash")]
    public string ContentHash { get; set; } = "";

    [JsonPropertyName("cssAsset")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public BuildCacheAsset? CssAsset { get; set; }

    [JsonPropertyName("jsAsset")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public BuildCacheAsset? JsAsset { get; set; }

    [JsonPropertyName("pages")]
    public Dictionary<string, BuildCachePage> Pages { get; set; } = [];
}

/// <summary>
/// Per-page cache entry recording where the page was written and whether it
/// consumes dynamic content (i.e., implements <c>IStaticPathsProvider</c>).
/// </summary>
internal sealed class BuildCachePage
{
    [JsonPropertyName("outputPath")]
    public string OutputPath { get; set; } = "";

    [JsonPropertyName("isDynamic")]
    public bool IsDynamic { get; set; }
}

/// <summary>
/// Cache entry for a processed CSS or JS asset.
/// </summary>
internal sealed class BuildCacheAsset
{
    [JsonPropertyName("outputPath")]
    public string OutputPath { get; set; } = "";

    [JsonPropertyName("fileName")]
    public string FileName { get; set; } = "";

    [JsonPropertyName("hash")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Hash { get; set; }
}
