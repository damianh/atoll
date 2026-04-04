namespace Atoll.Lagoon.I18n;

/// <summary>
/// Represents a resolved content path for a given locale, indicating whether
/// the content is locale-specific or a fallback to the default locale.
/// </summary>
/// <param name="ContentPath">The resolved content path (locale-scoped or default).</param>
/// <param name="LocaleKey">The locale key the content was resolved for.</param>
/// <param name="IsFallback"><c>true</c> if the content path is a fallback to the default locale.</param>
public sealed record ResolvedContent(
    string ContentPath,
    string LocaleKey,
    bool IsFallback);

/// <summary>
/// Static utility for resolving content paths in a folder-per-locale content organization scheme.
/// Transforms paths by adding or stripping locale prefixes from content slugs.
/// </summary>
/// <remarks>
/// The resolver performs pure path transformations — it does not perform file system checks.
/// The caller is responsible for verifying that the resolved content path exists.
/// </remarks>
public static class LocaleContentResolver
{
    /// <summary>
    /// Resolves the locale-scoped content path.
    /// For locale key <c>"fr"</c> and content path <c>"guides/intro"</c>, returns <c>"fr/guides/intro"</c>.
    /// For the root locale (key <c>"root"</c>), returns the content path unchanged.
    /// </summary>
    /// <param name="contentPath">The base content path (e.g., <c>"guides/intro"</c>).</param>
    /// <param name="localeKey">The locale key (e.g., <c>"fr"</c>, <c>"root"</c>).</param>
    /// <returns>The locale-scoped content path.</returns>
    public static string GetLocaleContentPath(string contentPath, string localeKey)
    {
        ArgumentNullException.ThrowIfNull(contentPath);
        ArgumentNullException.ThrowIfNull(localeKey);

        if (string.Equals(localeKey, "root", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrEmpty(localeKey))
        {
            return NormalizePath(contentPath);
        }

        var normalized = NormalizePath(contentPath);
        if (string.IsNullOrEmpty(normalized))
        {
            return localeKey;
        }

        return localeKey + "/" + normalized;
    }

    /// <summary>
    /// Strips the locale prefix from a content slug to get the base content path.
    /// For <c>"fr/guides/intro"</c> with locale key <c>"fr"</c>, returns <c>"guides/intro"</c>.
    /// For a root locale slug <c>"guides/intro"</c>, returns <c>"guides/intro"</c> unchanged.
    /// </summary>
    /// <param name="slug">The content slug, possibly locale-prefixed.</param>
    /// <param name="localeKey">The locale key to strip (e.g., <c>"fr"</c>, <c>"root"</c>).</param>
    /// <returns>The content path with the locale prefix removed.</returns>
    public static string StripLocaleFromSlug(string slug, string localeKey)
    {
        ArgumentNullException.ThrowIfNull(slug);
        ArgumentNullException.ThrowIfNull(localeKey);

        if (string.Equals(localeKey, "root", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrEmpty(localeKey))
        {
            return slug;
        }

        var normalized = slug.Replace('\\', '/').TrimStart('/');
        var prefix = localeKey.TrimStart('/');

        if (normalized.Equals(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return "";
        }

        if (normalized.StartsWith(prefix + "/", StringComparison.OrdinalIgnoreCase))
        {
            return normalized[(prefix.Length + 1)..];
        }

        // Slug doesn't start with the locale prefix — return as-is.
        return slug;
    }

    private static string NormalizePath(string path)
    {
        return path.Replace('\\', '/').TrimStart('/');
    }
}
