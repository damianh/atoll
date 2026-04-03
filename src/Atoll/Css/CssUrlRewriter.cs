using System.Text;
using System.Text.RegularExpressions;

namespace Atoll.Css;

/// <summary>
/// Rewrites relative URLs in CSS <c>url(...)</c> expressions to account for
/// a configured base path. This ensures that CSS references to images, fonts,
/// and other assets resolve correctly when the site is deployed under a sub-path.
/// </summary>
/// <remarks>
/// <para>
/// For example, with a base path of <c>/docs</c>:
/// <c>url('/images/bg.png')</c> becomes <c>url('/docs/images/bg.png')</c>.
/// </para>
/// <para>
/// The rewriter handles:
/// <list type="bullet">
///   <item>Quoted URLs: <c>url('...')</c> and <c>url("...")</c></item>
///   <item>Unquoted URLs: <c>url(...)</c></item>
///   <item>Absolute paths starting with <c>/</c></item>
///   <item>Preserves external URLs (<c>http://</c>, <c>https://</c>, <c>data:</c>, <c>//</c>)</item>
/// </list>
/// </para>
/// </remarks>
public static class CssUrlRewriter
{
    // Matches url(...) expressions in CSS, capturing the content.
    // Handles: url('...'), url("..."), url(...)
    private static readonly Regex UrlPattern = new(
        @"url\(\s*(?:(['""])(?<quoted>.*?)\1|(?<unquoted>[^)'""\s]+))\s*\)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Rewrites absolute-path URLs in the specified CSS to include the base path prefix.
    /// </summary>
    /// <param name="css">The CSS text containing <c>url(...)</c> references.</param>
    /// <param name="basePath">
    /// The base path to prepend (e.g., <c>/docs</c>). Must start with <c>/</c>.
    /// Trailing slashes are ignored.
    /// </param>
    /// <returns>The CSS with rewritten URLs.</returns>
    public static string Rewrite(string css, string basePath)
    {
        ArgumentNullException.ThrowIfNull(css);
        ArgumentNullException.ThrowIfNull(basePath);

        if (css.Length == 0 || basePath.Length == 0)
        {
            return css;
        }

        // Normalize base path: ensure leading slash, remove trailing slash
        var normalizedBase = NormalizeBasePath(basePath);
        if (normalizedBase.Length == 0 || normalizedBase == "/")
        {
            return css;
        }

        return UrlPattern.Replace(css, match =>
        {
            var quotedUrl = match.Groups["quoted"].Value;
            var unquotedUrl = match.Groups["unquoted"].Value;
            var originalUrl = quotedUrl.Length > 0 ? quotedUrl : unquotedUrl;
            var quote = quotedUrl.Length > 0 ? match.Groups[1].Value : string.Empty;

            if (!ShouldRewrite(originalUrl))
            {
                return match.Value;
            }

            var rewrittenUrl = normalizedBase + originalUrl;
            return quote.Length > 0
                ? $"url({quote}{rewrittenUrl}{quote})"
                : $"url({rewrittenUrl})";
        });
    }

    /// <summary>
    /// Determines whether a URL should be rewritten.
    /// Only absolute paths (starting with <c>/</c>) that are not already prefixed
    /// with the base path and are not external URLs are rewritten.
    /// </summary>
    /// <param name="url">The URL to check.</param>
    /// <returns><c>true</c> if the URL should be rewritten; otherwise, <c>false</c>.</returns>
    internal static bool ShouldRewrite(string url)
    {
        if (url.Length == 0)
        {
            return false;
        }

        // Skip external URLs
        if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            || url.StartsWith("//", StringComparison.Ordinal)
            || url.StartsWith("data:", StringComparison.OrdinalIgnoreCase)
            || url.StartsWith("blob:", StringComparison.OrdinalIgnoreCase)
            || url.StartsWith('#'))
        {
            return false;
        }

        // Only rewrite absolute paths
        return url.StartsWith('/');
    }

    private static string NormalizeBasePath(string basePath)
    {
        // Ensure leading slash
        if (!basePath.StartsWith('/'))
        {
            basePath = "/" + basePath;
        }

        // Remove trailing slash
        return basePath.TrimEnd('/');
    }
}
