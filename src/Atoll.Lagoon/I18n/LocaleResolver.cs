namespace Atoll.Lagoon.I18n;

/// <summary>
/// Represents a resolved locale for a given URL path, including the locale key,
/// configuration, path prefix, and the content path with the prefix stripped.
/// </summary>
/// <param name="Key">The locale dictionary key (e.g. <c>"root"</c>, <c>"fr"</c>, <c>"zh-cn"</c>).</param>
/// <param name="Config">The resolved <see cref="LocaleConfig"/>.</param>
/// <param name="PathPrefix">The URL path prefix for this locale (e.g. <c>"/fr"</c>), or empty for the root locale.</param>
/// <param name="ContentPath">The remaining path after stripping the base path and locale prefix.</param>
public sealed record ResolvedLocale(
    string Key,
    LocaleConfig Config,
    string PathPrefix,
    string ContentPath);

/// <summary>
/// Resolves the current locale from a URL path and the configured locale dictionary.
/// </summary>
public static class LocaleResolver
{
    /// <summary>
    /// Resolves the locale for the given URL path.
    /// Returns <c>null</c> when no locale configuration exists (single-language mode).
    /// </summary>
    /// <param name="path">The full URL path (e.g. <c>"/docs/fr/intro"</c>).</param>
    /// <param name="locales">The locale dictionary from <c>DocsConfig.Locales</c>, or <c>null</c>.</param>
    /// <param name="basePath">The site base path (e.g. <c>"/docs"</c>). Stripped before matching locale prefixes.</param>
    public static ResolvedLocale? Resolve(
        string path,
        IReadOnlyDictionary<string, LocaleConfig>? locales,
        string basePath)
    {
        if (locales is null || locales.Count == 0)
        {
            return null;
        }

        // Strip the base path from the front of the URL path.
        var remaining = StripPrefix(path, basePath);

        // Try to match a non-root locale prefix.
        foreach (var (key, config) in locales)
        {
            if (string.Equals(key, "root", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var localePrefix = "/" + key;
            if (remaining.Equals(localePrefix, StringComparison.OrdinalIgnoreCase)
                || remaining.StartsWith(localePrefix + "/", StringComparison.OrdinalIgnoreCase))
            {
                var contentPath = StripPrefix(remaining, localePrefix);
                return new ResolvedLocale(key, config, localePrefix, NormalizePath(contentPath));
            }
        }

        // No prefix matched — fall back to "root" locale if configured.
        if (locales.TryGetValue("root", out var rootConfig))
        {
            return new ResolvedLocale("root", rootConfig, "", NormalizePath(remaining));
        }

        // No "root" key — fall back to the first locale in the dictionary.
        var first = locales.First();
        return new ResolvedLocale(first.Key, first.Value, "/" + first.Key, NormalizePath(remaining));
    }

    /// <inheritdoc cref="Resolve(string, IReadOnlyDictionary{string, LocaleConfig}?, string)"/>
    public static ResolvedLocale? Resolve(
        string path,
        IReadOnlyDictionary<string, LocaleConfig>? locales) =>
        Resolve(path, locales, "");

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
