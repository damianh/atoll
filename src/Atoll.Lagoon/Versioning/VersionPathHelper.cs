namespace Atoll.Lagoon.Versioning;

/// <summary>
/// Static helper for version-aware URL path manipulation — prefixing, stripping,
/// and testing version ownership of paths.
/// </summary>
/// <remarks>
/// All methods compose correctly with the site base path and locale prefix:
/// the version prefix is inserted <em>after</em> the locale prefix
/// (e.g., <c>/docs/fr/v1.0/intro</c>).
/// </remarks>
public static class VersionPathHelper
{
    /// <summary>
    /// Prepends the version prefix (and optional locale prefix and base path) to a content path.
    /// Composes: <c>basePath + localePrefix + versionPrefix + contentPath</c>
    /// → e.g., <c>"/docs" + "/fr" + "/v1.0" + "/intro"</c> → <c>"/docs/fr/v1.0/intro"</c>.
    /// </summary>
    /// <param name="contentPath">The content path (e.g., <c>"/intro"</c>).</param>
    /// <param name="versionPrefix">The version URL prefix (e.g., <c>"/v1.0"</c>), or empty for current version.</param>
    /// <param name="localePrefix">The locale URL prefix (e.g., <c>"/fr"</c>), or empty.</param>
    /// <param name="basePath">The site base path (e.g., <c>"/docs"</c>), or empty.</param>
    /// <returns>The composed path.</returns>
    public static string PrefixPath(string contentPath, string versionPrefix, string localePrefix, string basePath)
    {
        var normalizedBase = basePath.TrimEnd('/');
        var normalizedLocale = localePrefix.TrimEnd('/');
        var normalizedVersion = versionPrefix.TrimEnd('/');
        var normalizedContent = EnsureLeadingSlash(contentPath);

        var result = normalizedBase + normalizedLocale + normalizedVersion + normalizedContent;

        return string.IsNullOrEmpty(result) ? "/" : result;
    }

    /// <inheritdoc cref="PrefixPath(string, string, string, string)"/>
    public static string PrefixPath(string contentPath, string versionPrefix, string localePrefix) =>
        PrefixPath(contentPath, versionPrefix, localePrefix, "");

    /// <inheritdoc cref="PrefixPath(string, string, string, string)"/>
    public static string PrefixPath(string contentPath, string versionPrefix) =>
        PrefixPath(contentPath, versionPrefix, "", "");

    /// <summary>
    /// Strips the version prefix (and optional locale prefix and base path) from a full path,
    /// returning just the content path.
    /// </summary>
    /// <param name="fullPath">The full URL path (e.g., <c>"/docs/fr/v1.0/intro"</c>).</param>
    /// <param name="versionPrefix">The version URL prefix (e.g., <c>"/v1.0"</c>), or empty.</param>
    /// <param name="localePrefix">The locale URL prefix (e.g., <c>"/fr"</c>), or empty.</param>
    /// <param name="basePath">The site base path (e.g., <c>"/docs"</c>), or empty.</param>
    /// <returns>The content path with base path, locale prefix, and version prefix removed.</returns>
    public static string StripPrefix(string fullPath, string versionPrefix, string localePrefix, string basePath)
    {
        var remaining = StripLeadingSegment(fullPath, basePath);
        remaining = StripLeadingSegment(remaining, localePrefix);
        remaining = StripLeadingSegment(remaining, versionPrefix);

        return string.IsNullOrEmpty(remaining) ? "/" : remaining;
    }

    /// <inheritdoc cref="StripPrefix(string, string, string, string)"/>
    public static string StripPrefix(string fullPath, string versionPrefix, string localePrefix) =>
        StripPrefix(fullPath, versionPrefix, localePrefix, "");

    /// <inheritdoc cref="StripPrefix(string, string, string, string)"/>
    public static string StripPrefix(string fullPath, string versionPrefix) =>
        StripPrefix(fullPath, versionPrefix, "", "");

    /// <summary>
    /// Checks whether a path belongs to the given version prefix
    /// (after accounting for the base path and locale prefix).
    /// </summary>
    /// <param name="path">The full URL path to test.</param>
    /// <param name="versionPrefix">The version URL prefix (e.g., <c>"/v1.0"</c>), or empty for current version.</param>
    /// <param name="localePrefix">The locale URL prefix (e.g., <c>"/fr"</c>), or empty.</param>
    /// <param name="basePath">The site base path (e.g., <c>"/docs"</c>), or empty.</param>
    /// <returns><c>true</c> if the path belongs to the specified version.</returns>
    public static bool BelongsToVersion(string path, string versionPrefix, string localePrefix, string basePath)
    {
        var remaining = StripLeadingSegment(path, basePath);
        remaining = StripLeadingSegment(remaining, localePrefix);

        if (string.IsNullOrEmpty(versionPrefix))
        {
            // Current version — any path not matched by another version prefix belongs here.
            return true;
        }

        return remaining.Equals(versionPrefix, StringComparison.OrdinalIgnoreCase) ||
               remaining.StartsWith(versionPrefix + "/", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc cref="BelongsToVersion(string, string, string, string)"/>
    public static bool BelongsToVersion(string path, string versionPrefix, string localePrefix) =>
        BelongsToVersion(path, versionPrefix, localePrefix, "");

    /// <inheritdoc cref="BelongsToVersion(string, string, string, string)"/>
    public static bool BelongsToVersion(string path, string versionPrefix) =>
        BelongsToVersion(path, versionPrefix, "", "");

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
