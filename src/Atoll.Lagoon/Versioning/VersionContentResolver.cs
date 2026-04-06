namespace Atoll.Lagoon.Versioning;

/// <summary>
/// Static utility for resolving content paths in a folder-per-version content organization scheme.
/// Transforms paths by adding or stripping version prefixes from content slugs.
/// </summary>
/// <remarks>
/// The resolver performs pure path transformations — it does not perform file system checks.
/// The caller is responsible for verifying that the resolved content path exists.
/// </remarks>
public static class VersionContentResolver
{
    /// <summary>
    /// Resolves the version-scoped content path.
    /// For version key <c>"v1.0"</c> and content path <c>"guides/intro"</c>, returns <c>"v1.0/guides/intro"</c>.
    /// For the current version (key <c>"current"</c>), returns the content path unchanged.
    /// </summary>
    /// <param name="contentPath">The base content path (e.g., <c>"guides/intro"</c>).</param>
    /// <param name="versionKey">The version key (e.g., <c>"v1.0"</c>, <c>"current"</c>).</param>
    /// <returns>The version-scoped content path.</returns>
    public static string GetVersionContentPath(string contentPath, string versionKey)
    {
        ArgumentNullException.ThrowIfNull(contentPath);
        ArgumentNullException.ThrowIfNull(versionKey);

        if (string.Equals(versionKey, "current", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrEmpty(versionKey))
        {
            return NormalizePath(contentPath);
        }

        var normalized = NormalizePath(contentPath);
        if (string.IsNullOrEmpty(normalized))
        {
            return versionKey;
        }

        return versionKey + "/" + normalized;
    }

    /// <summary>
    /// Strips the version prefix from a content slug to get the base content path.
    /// For <c>"v1.0/guides/intro"</c> with version key <c>"v1.0"</c>, returns <c>"guides/intro"</c>.
    /// For a current version slug <c>"guides/intro"</c>, returns <c>"guides/intro"</c> unchanged.
    /// </summary>
    /// <param name="slug">The content slug, possibly version-prefixed.</param>
    /// <param name="versionKey">The version key to strip (e.g., <c>"v1.0"</c>, <c>"current"</c>).</param>
    /// <returns>The content path with the version prefix removed.</returns>
    public static string StripVersionFromSlug(string slug, string versionKey)
    {
        ArgumentNullException.ThrowIfNull(slug);
        ArgumentNullException.ThrowIfNull(versionKey);

        if (string.Equals(versionKey, "current", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrEmpty(versionKey))
        {
            return slug;
        }

        var normalized = slug.Replace('\\', '/').TrimStart('/');
        var prefix = versionKey.TrimStart('/');

        if (normalized.Equals(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return "";
        }

        if (normalized.StartsWith(prefix + "/", StringComparison.OrdinalIgnoreCase))
        {
            return normalized[(prefix.Length + 1)..];
        }

        // Slug doesn't start with the version prefix — return as-is.
        return slug;
    }

    private static string NormalizePath(string path)
    {
        return path.Replace('\\', '/').TrimStart('/');
    }
}
