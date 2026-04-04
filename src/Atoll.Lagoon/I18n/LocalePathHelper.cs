namespace Atoll.Lagoon.I18n;

/// <summary>
/// Static helper for locale-aware URL path manipulation — prefixing, stripping,
/// and testing locale ownership of paths.
/// </summary>
/// <remarks>
/// All methods compose correctly with the site base path: the locale prefix is
/// inserted <em>after</em> the base path (e.g., <c>/docs/fr/intro</c>, not <c>/fr/docs/intro</c>).
/// </remarks>
public static class LocalePathHelper
{
    /// <summary>
    /// Prepends the locale prefix to a content path, respecting the site base path.
    /// The locale prefix is inserted after the base path:
    /// <c>basePath + localePrefix + contentPath</c>
    /// → e.g., <c>"/docs" + "/fr" + "/intro"</c> → <c>"/docs/fr/intro"</c>.
    /// </summary>
    /// <param name="contentPath">The content path (e.g., <c>"/intro"</c> or <c>"/guides/start"</c>).</param>
    /// <param name="localePrefix">The locale URL prefix (e.g., <c>"/fr"</c>), or empty for the root locale.</param>
    /// <param name="basePath">The site base path (e.g., <c>"/docs"</c>), or empty.</param>
    /// <returns>The composed path with base path, locale prefix, and content path.</returns>
    public static string PrefixPath(string contentPath, string localePrefix, string basePath)
    {
        var normalizedBase = basePath.TrimEnd('/');
        var normalizedLocale = localePrefix.TrimEnd('/');
        var normalizedContent = EnsureLeadingSlash(contentPath);

        var result = normalizedBase + normalizedLocale + normalizedContent;

        return string.IsNullOrEmpty(result) ? "/" : result;
    }

    /// <inheritdoc cref="PrefixPath(string, string, string)"/>
    public static string PrefixPath(string contentPath, string localePrefix) =>
        PrefixPath(contentPath, localePrefix, "");

    /// <summary>
    /// Strips the locale prefix (and optional base path) from a full path,
    /// returning just the content path.
    /// </summary>
    /// <param name="fullPath">The full URL path (e.g., <c>"/docs/fr/intro"</c>).</param>
    /// <param name="localePrefix">The locale URL prefix (e.g., <c>"/fr"</c>), or empty for root locale.</param>
    /// <param name="basePath">The site base path (e.g., <c>"/docs"</c>), or empty.</param>
    /// <returns>The content path with base path and locale prefix removed (e.g., <c>"/intro"</c>).</returns>
    public static string StripPrefix(string fullPath, string localePrefix, string basePath)
    {
        var remaining = StripLeadingSegment(fullPath, basePath);
        remaining = StripLeadingSegment(remaining, localePrefix);

        return string.IsNullOrEmpty(remaining) ? "/" : remaining;
    }

    /// <inheritdoc cref="StripPrefix(string, string, string)"/>
    public static string StripPrefix(string fullPath, string localePrefix) =>
        StripPrefix(fullPath, localePrefix, "");

    /// <summary>
    /// Checks whether a path belongs to the given locale prefix (after accounting for the base path).
    /// </summary>
    /// <param name="path">The full URL path to test.</param>
    /// <param name="localePrefix">The locale URL prefix (e.g., <c>"/fr"</c>), or empty for root locale.</param>
    /// <param name="basePath">The site base path (e.g., <c>"/docs"</c>), or empty.</param>
    /// <returns><c>true</c> if the path starts with the base path followed by the locale prefix.</returns>
    public static bool BelongsToLocale(string path, string localePrefix, string basePath)
    {
        if (string.IsNullOrEmpty(localePrefix))
        {
            // Root locale — any path under the base path belongs.
            return string.IsNullOrEmpty(basePath) ||
                   path.Equals(basePath, StringComparison.OrdinalIgnoreCase) ||
                   path.StartsWith(basePath + "/", StringComparison.OrdinalIgnoreCase);
        }

        var remaining = StripLeadingSegment(path, basePath);

        return remaining.Equals(localePrefix, StringComparison.OrdinalIgnoreCase) ||
               remaining.StartsWith(localePrefix + "/", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc cref="BelongsToLocale(string, string, string)"/>
    public static bool BelongsToLocale(string path, string localePrefix) =>
        BelongsToLocale(path, localePrefix, "");

    private static string EnsureLeadingSlash(string path) =>
        path.Length > 0 && path[0] == '/' ? path : "/" + path;

    private static string StripLeadingSegment(string path, string segment)
    {
        if (string.IsNullOrEmpty(segment))
        {
            return path;
        }

        if (path.StartsWith(segment, StringComparison.OrdinalIgnoreCase))
        {
            var stripped = path[segment.Length..];
            // If stripping leaves nothing or starts with /, it's a clean boundary.
            if (stripped.Length == 0 || stripped[0] == '/')
            {
                return stripped;
            }
        }

        return path;
    }
}
