using System.Text.Json.Serialization;

namespace Atoll.Configuration;

/// <summary>
/// Represents the project configuration loaded from <c>atoll.json</c>.
/// Controls site URL, output directory, base path, and build behavior.
/// </summary>
/// <remarks>
/// <para>
/// The configuration file is optional. When absent, all settings use their
/// documented defaults. Partial configuration is supported — only the
/// properties present in the JSON file are applied.
/// </para>
/// <para>
/// Example <c>atoll.json</c>:
/// </para>
/// <code>
/// {
///   "site": "https://example.com",
///   "base": "/docs",
///   "outDir": "dist",
///   "srcDir": "src/pages",
///   "publicDir": "public"
/// }
/// </code>
/// </remarks>
public sealed class AtollConfig
{
    /// <summary>
    /// Gets or sets the site URL for generating canonical URLs and sitemap entries
    /// (e.g., <c>https://example.com</c>).
    /// Defaults to an empty string (no site URL configured).
    /// </summary>
    [JsonPropertyName("site")]
    public string Site { get; set; } = "";

    /// <summary>
    /// Gets or sets the base path prefix for the site (e.g., <c>/docs</c>).
    /// All generated URLs are relative to this path.
    /// Defaults to <c>/</c> (root).
    /// </summary>
    [JsonPropertyName("base")]
    public string Base { get; set; } = "/";

    /// <summary>
    /// Gets or sets the output directory for the built site.
    /// Defaults to <c>dist</c>.
    /// </summary>
    [JsonPropertyName("outDir")]
    public string OutDir { get; set; } = "dist";

    /// <summary>
    /// Gets or sets the source directory containing page components.
    /// Defaults to <c>src/pages</c>.
    /// </summary>
    [JsonPropertyName("srcDir")]
    public string SrcDir { get; set; } = "src/pages";

    /// <summary>
    /// Gets or sets the public directory containing static assets to copy as-is.
    /// Defaults to <c>public</c>.
    /// </summary>
    [JsonPropertyName("publicDir")]
    public string PublicDir { get; set; } = "public";

    /// <summary>
    /// Gets or sets the server configuration section.
    /// </summary>
    [JsonPropertyName("server")]
    public AtollServerConfig Server { get; set; } = new();

    /// <summary>
    /// Gets or sets the build configuration section.
    /// </summary>
    [JsonPropertyName("build")]
    public AtollBuildConfig Build { get; set; } = new();
}

/// <summary>
/// Server-specific configuration for the <c>atoll dev</c> command.
/// </summary>
public sealed class AtollServerConfig
{
    /// <summary>
    /// Gets or sets the hostname to bind to. Defaults to <c>localhost</c>.
    /// </summary>
    [JsonPropertyName("host")]
    public string Host { get; set; } = "localhost";

    /// <summary>
    /// Gets or sets the port number to listen on. Defaults to <c>4321</c> (same as Astro).
    /// </summary>
    [JsonPropertyName("port")]
    public int Port { get; set; } = 4321;
}

/// <summary>
/// Build-specific configuration for the <c>atoll build</c> command.
/// </summary>
public sealed class AtollBuildConfig
{
    /// <summary>
    /// Gets or sets whether to minify CSS and JS output. Defaults to <c>true</c>.
    /// </summary>
    [JsonPropertyName("minify")]
    public bool Minify { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to fingerprint asset filenames for cache busting.
    /// Defaults to <c>true</c>.
    /// </summary>
    [JsonPropertyName("fingerprint")]
    public bool Fingerprint { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to clean the output directory before building.
    /// Defaults to <c>true</c>.
    /// </summary>
    [JsonPropertyName("clean")]
    public bool Clean { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of pages to render in parallel.
    /// A value of <c>-1</c> uses the system default. Defaults to <c>-1</c>.
    /// </summary>
    [JsonPropertyName("concurrency")]
    public int Concurrency { get; set; } = -1;

    /// <summary>
    /// Gets or sets the cache configuration for the build output.
    /// </summary>
    [JsonPropertyName("cache")]
    public AtollCacheConfig Cache { get; set; } = new();
}

/// <summary>
/// Cache-related build configuration — controls <c>_headers</c> file generation
/// and custom HTTP header rules for the SSG output.
/// </summary>
public sealed class AtollCacheConfig
{
    /// <summary>
    /// Gets or sets whether to generate a <c>_headers</c> file in the output directory.
    /// The generated file uses the Netlify / Cloudflare Pages format and sets
    /// appropriate <c>Cache-Control</c> headers for fingerprinted assets and HTML pages.
    /// Defaults to <c>true</c>.
    /// </summary>
    [JsonPropertyName("generateHeadersFile")]
    public bool GenerateHeadersFile { get; set; } = true;

    /// <summary>
    /// Gets or sets custom header rules to append after the default rules in the
    /// generated <c>_headers</c> file. Each rule maps a URL path pattern to a
    /// dictionary of header name → value pairs.
    /// Defaults to an empty list (no custom rules).
    /// </summary>
    [JsonPropertyName("customRules")]
    public IList<AtollCacheHeaderRule> CustomRules { get; set; } = [];
}

/// <summary>
/// A single custom header rule for the <c>_headers</c> file.
/// </summary>
public sealed class AtollCacheHeaderRule
{
    /// <summary>
    /// Gets or sets the URL path pattern (e.g., <c>/api/*</c>).
    /// </summary>
    [JsonPropertyName("path")]
    public string Path { get; set; } = "";

    /// <summary>
    /// Gets or sets the headers to apply for requests matching <see cref="Path"/>.
    /// Keys are header names; values are header values.
    /// </summary>
    [JsonPropertyName("headers")]
    public IDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
}
