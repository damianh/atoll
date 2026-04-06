namespace Atoll.Lagoon.Versioning;

/// <summary>
/// Represents a resolved version for a given URL path, including the version key,
/// configuration, path prefix, and the content path with the version prefix stripped.
/// </summary>
/// <param name="Key">The version dictionary key (e.g. <c>"current"</c>, <c>"v1.0"</c>).</param>
/// <param name="Config">The resolved <see cref="VersionConfig"/>.</param>
/// <param name="PathPrefix">The URL path prefix for this version (e.g. <c>"/v1.0"</c>), or empty for the current version.</param>
/// <param name="ContentPath">The remaining path after stripping the base path and version prefix.</param>
public sealed record ResolvedVersion(
    string Key,
    VersionConfig Config,
    string PathPrefix,
    string ContentPath);

/// <summary>
/// Resolves the current version from a URL path and the configured version dictionary.
/// </summary>
public static class VersionResolver
{
    /// <summary>
    /// Resolves the version for the given URL path.
    /// Returns <c>null</c> when no version configuration exists (single-version mode).
    /// </summary>
    /// <param name="path">
    /// The content path after locale resolution (e.g. <c>"/v1.0/intro"</c> or <c>"/intro"</c>).
    /// This is the path <em>after</em> the locale prefix has been stripped.
    /// </param>
    /// <param name="versions">The version dictionary from <c>DocsConfig.Versions</c>, or <c>null</c>.</param>
    /// <param name="basePath">The site base path (e.g. <c>"/docs"</c>). Stripped before matching version prefixes.</param>
    public static ResolvedVersion? Resolve(
        string path,
        IReadOnlyDictionary<string, VersionConfig>? versions,
        string basePath)
    {
        if (versions is null || versions.Count == 0)
        {
            return null;
        }

        // Strip the base path from the front of the path.
        var remaining = StripPrefix(path, basePath);

        // Try to match a non-current version prefix.
        foreach (var (key, config) in versions)
        {
            if (string.Equals(key, "current", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var versionPrefix = "/" + key;
            if (remaining.Equals(versionPrefix, StringComparison.OrdinalIgnoreCase)
                || remaining.StartsWith(versionPrefix + "/", StringComparison.OrdinalIgnoreCase))
            {
                var contentPath = StripPrefix(remaining, versionPrefix);
                return new ResolvedVersion(key, config, versionPrefix, NormalizePath(contentPath));
            }
        }

        // No prefix matched — fall back to "current" key if configured.
        if (versions.TryGetValue("current", out var currentConfig))
        {
            return new ResolvedVersion("current", currentConfig, "", NormalizePath(remaining));
        }

        // No "current" key — fall back to the first version in the dictionary.
        var first = versions.First();
        return new ResolvedVersion(first.Key, first.Value, "/" + first.Key, NormalizePath(remaining));
    }

    /// <inheritdoc cref="Resolve(string, IReadOnlyDictionary{string, VersionConfig}?, string)"/>
    public static ResolvedVersion? Resolve(
        string path,
        IReadOnlyDictionary<string, VersionConfig>? versions) =>
        Resolve(path, versions, "");

    private static string StripPrefix(string path, string prefix)
    {
        if (string.IsNullOrEmpty(prefix))
        {
            return path;
        }

        if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return path[prefix.Length..];
        }

        return path;
    }

    private static string NormalizePath(string path) =>
        string.IsNullOrEmpty(path) ? "/" : path;
}
