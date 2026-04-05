using System.Text;
using Atoll.Configuration;

namespace Atoll.Build.Ssg;

/// <summary>
/// Generates a <c>_headers</c> file in the Netlify / Cloudflare Pages format,
/// mapping URL path patterns to HTTP response headers.
/// </summary>
/// <remarks>
/// The default rules set appropriate <c>Cache-Control</c> headers:
/// <list type="bullet">
///   <item><description><c>/_atoll/*</c> — immutable (fingerprinted assets)</description></item>
///   <item><description><c>/*.html</c> — must-revalidate (HTML pages)</description></item>
///   <item><description><c>/</c> — must-revalidate (root)</description></item>
///   <item><description><c>/search-index.json</c> — must-revalidate (search index)</description></item>
/// </list>
/// Custom rules from <see cref="AtollCacheConfig.CustomRules"/> are appended after the defaults.
/// </remarks>
public sealed class HeadersFileGenerator
{
    private const string ImmutableCacheControl = "public, max-age=31536000, immutable";
    private const string MustRevalidateCacheControl = "public, max-age=0, must-revalidate";

    private readonly AtollCacheConfig _config;

    /// <summary>
    /// Initializes a new instance with default cache configuration.
    /// </summary>
    public HeadersFileGenerator()
        : this(new AtollCacheConfig())
    {
    }

    /// <summary>
    /// Initializes a new instance with the specified cache configuration.
    /// </summary>
    /// <param name="config">The cache configuration controlling custom header rules.</param>
    public HeadersFileGenerator(AtollCacheConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        _config = config;
    }

    /// <summary>
    /// Generates the content of the <c>_headers</c> file as a string.
    /// </summary>
    public string Generate()
    {
        var sb = new StringBuilder();

        // Fingerprinted assets — immutable for 1 year
        AppendRule(sb, "/_atoll/*", "Cache-Control", ImmutableCacheControl);

        // HTML pages — always revalidate
        AppendRule(sb, "/*.html", "Cache-Control", MustRevalidateCacheControl);

        // Root path
        AppendRule(sb, "/", "Cache-Control", MustRevalidateCacheControl);

        // Search index
        AppendRule(sb, "/search-index.json", "Cache-Control", MustRevalidateCacheControl);

        // User-supplied custom rules
        foreach (var rule in _config.CustomRules)
        {
            if (string.IsNullOrWhiteSpace(rule.Path) || rule.Headers.Count == 0)
            {
                continue;
            }

            sb.AppendLine(rule.Path);
            foreach (var (headerName, headerValue) in rule.Headers)
            {
                sb.Append("  ").Append(headerName).Append(": ").AppendLine(headerValue);
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Writes the <c>_headers</c> file to the specified output directory.
    /// </summary>
    /// <param name="outputDirectory">The directory in which to write <c>_headers</c>.</param>
    public async Task WriteAsync(string outputDirectory)
    {
        ArgumentNullException.ThrowIfNull(outputDirectory);

        var content = Generate();
        var path = Path.Combine(outputDirectory, "_headers");
        await File.WriteAllTextAsync(path, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    private static void AppendRule(StringBuilder sb, string path, string headerName, string headerValue)
    {
        sb.AppendLine(path);
        sb.Append("  ").Append(headerName).Append(": ").AppendLine(headerValue);
        sb.AppendLine();
    }
}
