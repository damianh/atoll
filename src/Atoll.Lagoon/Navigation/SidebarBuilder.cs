using Atoll.Lagoon.Configuration;
using Atoll.Lagoon.I18n;
using Atoll.Lagoon.Versioning;

namespace Atoll.Lagoon.Navigation;

/// <summary>
/// Builds a resolved sidebar navigation tree from <see cref="DocsConfig"/> sidebar configuration
/// and a set of content entries.
/// </summary>
/// <remarks>
/// <para>
/// The builder processes each top-level <see cref="SidebarItem"/> from the configuration:
/// <list type="bullet">
///   <item>Leaf items (with <see cref="SidebarItem.Link"/>) become link nodes.</item>
///   <item>Group items (with <see cref="SidebarItem.Items"/> children) are resolved recursively.</item>
///   <item>Auto-generate items (with <see cref="SidebarItem.AutoGenerate"/>) are populated
///         from the provided <see cref="SidebarEntry"/> collection matching the configured directory.</item>
/// </list>
/// </para>
/// <para>
/// The current path is matched against each item's <see cref="ResolvedSidebarItem.Href"/>
/// (case-insensitive, with or without trailing slash) to determine the active item.
/// </para>
/// </remarks>
public sealed class SidebarBuilder
{
    private readonly IReadOnlyList<SidebarItem> _config;
    private readonly IReadOnlyList<SidebarEntry> _entries;

    /// <summary>
    /// Initializes a new instance of <see cref="SidebarBuilder"/>.
    /// </summary>
    /// <param name="config">The sidebar item configuration from <see cref="DocsConfig.Sidebar"/>.</param>
    /// <param name="entries">Content entries used to populate auto-generated groups.</param>
    public SidebarBuilder(IReadOnlyList<SidebarItem> config, IReadOnlyList<SidebarEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(entries);
        _config = config;
        _entries = entries;
    }

    /// <summary>
    /// Builds the resolved sidebar for the given current page path.
    /// </summary>
    /// <param name="currentPath">
    /// The path of the current page (e.g., <c>"/docs/guides/getting-started/"</c>).
    /// Used to mark the active item and expand ancestor groups.
    /// </param>
    /// <returns>The resolved top-level sidebar items.</returns>
    public IReadOnlyList<ResolvedSidebarItem> Build(string currentPath)
    {
        ArgumentNullException.ThrowIfNull(currentPath);
        return ResolveItems(_config, currentPath, "", "", "");
    }

    /// <summary>
    /// Builds the resolved sidebar for the given current page path, prefixing all hrefs
    /// with the locale prefix and base path for multi-locale sites, and filtering
    /// auto-generated entries to only those belonging to the specified locale.
    /// </summary>
    /// <param name="currentPath">
    /// The full path of the current page including locale prefix
    /// (e.g., <c>"/docs/fr/guides/getting-started/"</c>).
    /// </param>
    /// <param name="localePrefix">
    /// The locale URL prefix (e.g., <c>"/fr"</c>), or empty for the root locale.
    /// </param>
    /// <param name="basePath">
    /// The site base path (e.g., <c>"/docs"</c>), or empty.
    /// </param>
    /// <returns>The resolved top-level sidebar items with locale-prefixed hrefs.</returns>
    public IReadOnlyList<ResolvedSidebarItem> Build(string currentPath, string localePrefix, string basePath)
    {
        ArgumentNullException.ThrowIfNull(currentPath);
        ArgumentNullException.ThrowIfNull(localePrefix);
        ArgumentNullException.ThrowIfNull(basePath);
        return ResolveItems(_config, currentPath, localePrefix, basePath, "");
    }

    /// <summary>
    /// Builds the resolved sidebar for the given current page path, prefixing all hrefs
    /// with the locale prefix and base path for multi-locale sites, and filtering
    /// auto-generated entries to only those belonging to the specified locale.
    /// </summary>
    /// <param name="currentPath">
    /// The full path of the current page including locale prefix
    /// (e.g., <c>"/docs/fr/guides/getting-started/"</c>).
    /// </param>
    /// <param name="localePrefix">
    /// The locale URL prefix (e.g., <c>"/fr"</c>), or empty for the root locale.
    /// </param>
    /// <param name="basePath">
    /// The site base path (e.g., <c>"/docs"</c>), or empty.
    /// </param>
    /// <param name="localeKey">
    /// The locale key (e.g., <c>"fr"</c>, <c>"root"</c>) used to filter auto-generated entries
    /// to the current locale's content folder. When empty, no locale filtering is applied.
    /// </param>
    /// <returns>The resolved top-level sidebar items with locale-prefixed hrefs.</returns>
    public IReadOnlyList<ResolvedSidebarItem> Build(string currentPath, string localePrefix, string basePath, string localeKey)
    {
        ArgumentNullException.ThrowIfNull(currentPath);
        ArgumentNullException.ThrowIfNull(localePrefix);
        ArgumentNullException.ThrowIfNull(basePath);
        ArgumentNullException.ThrowIfNull(localeKey);
        return ResolveItems(_config, currentPath, localePrefix, basePath, localeKey);
    }

    /// <summary>
    /// Builds the resolved sidebar for the given current page path, prefixing all hrefs
    /// with the locale and version prefixes and base path for multi-locale, multi-version sites,
    /// and filtering auto-generated entries to only those belonging to the specified locale and version.
    /// </summary>
    /// <param name="currentPath">
    /// The full path of the current page including locale and version prefix
    /// (e.g., <c>"/docs/fr/v1.0/guides/getting-started/"</c>).
    /// </param>
    /// <param name="localePrefix">
    /// The locale URL prefix (e.g., <c>"/fr"</c>), or empty for the root locale.
    /// </param>
    /// <param name="basePath">
    /// The site base path (e.g., <c>"/docs"</c>), or empty.
    /// </param>
    /// <param name="localeKey">
    /// The locale key (e.g., <c>"fr"</c>, <c>"root"</c>) used to filter auto-generated entries.
    /// When empty, no locale filtering is applied.
    /// </param>
    /// <param name="versionPrefix">
    /// The version URL prefix (e.g., <c>"/v1.0"</c>), or empty for the current version.
    /// </param>
    /// <param name="versionKey">
    /// The version key (e.g., <c>"v1.0"</c>, <c>"current"</c>) used to filter auto-generated entries.
    /// When empty, no version filtering is applied.
    /// </param>
    /// <returns>The resolved top-level sidebar items with locale- and version-prefixed hrefs.</returns>
    public IReadOnlyList<ResolvedSidebarItem> Build(
        string currentPath,
        string localePrefix,
        string basePath,
        string localeKey,
        string versionPrefix,
        string versionKey)
    {
        ArgumentNullException.ThrowIfNull(currentPath);
        ArgumentNullException.ThrowIfNull(localePrefix);
        ArgumentNullException.ThrowIfNull(basePath);
        ArgumentNullException.ThrowIfNull(localeKey);
        ArgumentNullException.ThrowIfNull(versionPrefix);
        ArgumentNullException.ThrowIfNull(versionKey);
        return ResolveItems(_config, currentPath, localePrefix, basePath, localeKey, versionPrefix, versionKey);
    }

    private IReadOnlyList<ResolvedSidebarItem> ResolveItems(
        IReadOnlyList<SidebarItem> items,
        string currentPath,
        string localePrefix,
        string basePath,
        string localeKey) =>
        ResolveItems(items, currentPath, localePrefix, basePath, localeKey, "", "");

    private IReadOnlyList<ResolvedSidebarItem> ResolveItems(
        IReadOnlyList<SidebarItem> items,
        string currentPath,
        string localePrefix,
        string basePath,
        string localeKey,
        string versionPrefix,
        string versionKey)
    {
        var resolved = new List<ResolvedSidebarItem>(items.Count);
        foreach (var item in items)
        {
            resolved.Add(ResolveItem(item, currentPath, localePrefix, basePath, localeKey, versionPrefix, versionKey));
        }

        return resolved;
    }

    private ResolvedSidebarItem ResolveItem(
        SidebarItem item,
        string currentPath,
        string localePrefix,
        string basePath,
        string localeKey,
        string versionPrefix,
        string versionKey)
    {
        // Leaf link item
        if (item.Link is not null)
        {
            var href = PrefixHref(item.Link, localePrefix, basePath, versionPrefix);
            var isCurrent = IsCurrent(href, currentPath);
            return new ResolvedSidebarItem(item.Label, href, isCurrent, item.Badge);
        }

        // Auto-generate group
        if (item.AutoGenerate is not null)
        {
            return ResolveAutoGeneratedGroup(item, currentPath, localePrefix, basePath, localeKey, versionPrefix, versionKey);
        }

        // Manual group with children
        var children = ResolveItems(item.Items, currentPath, localePrefix, basePath, localeKey, versionPrefix, versionKey);
        var isActive = children.Any(c => c.IsActive);
        return new ResolvedSidebarItem(item.Label, isActive, item.Badge, item.Collapsed, children);
    }

    private ResolvedSidebarItem ResolveAutoGeneratedGroup(
        SidebarItem item,
        string currentPath,
        string localePrefix,
        string basePath,
        string localeKey,
        string versionPrefix,
        string versionKey)
    {
        var dir = item.AutoGenerate!;
        var normalizedDir = dir.Trim('/');

        // Find all entries whose slug starts with the given directory prefix.
        // When a localeKey is specified, use locale-aware slug matching.
        // When a versionKey is specified, use version-aware slug matching.
        var matching = _entries
            .Where(e => !e.Draft)
            .Where(e => SlugMatchesDirectory(e.Slug, normalizedDir, localeKey, versionKey))
            .OrderBy(e => e.Order)
            .ThenBy(e => e.Label, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var children = matching
            .Select(e =>
            {
                var href = PrefixHref(e.Href, localePrefix, basePath, versionPrefix);
                return new ResolvedSidebarItem(e.Label, href, IsCurrent(href, currentPath), e.Badge);
            })
            .ToList();

        var isActive = children.Any(c => c.IsActive);
        return new ResolvedSidebarItem(item.Label, isActive, item.Badge, item.Collapsed, children);
    }

    /// <summary>
    /// Returns a flattened ordered list of all link items (no group headers) from the resolved sidebar.
    /// Useful for pagination resolution.
    /// </summary>
    /// <param name="items">The resolved sidebar items to flatten.</param>
    /// <returns>All leaf link items in sidebar order.</returns>
    public static IReadOnlyList<ResolvedSidebarItem> Flatten(IReadOnlyList<ResolvedSidebarItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        var result = new List<ResolvedSidebarItem>();
        FlattenInto(items, result);
        return result;
    }

    private static void FlattenInto(IReadOnlyList<ResolvedSidebarItem> items, List<ResolvedSidebarItem> result)
    {
        foreach (var item in items)
        {
            if (item.IsGroup)
            {
                FlattenInto(item.Items, result);
            }
            else
            {
                result.Add(item);
            }
        }
    }

    private static bool SlugMatchesDirectory(string slug, string normalizedDir, string localeKey)
        => SlugMatchesDirectory(slug, normalizedDir, localeKey, "");

    private static bool SlugMatchesDirectory(string slug, string normalizedDir, string localeKey, string versionKey)
    {
        var normalizedSlug = slug.Replace('\\', '/');

        // When a locale key is specified, strip the locale prefix from the slug
        // before comparing against the directory. This allows locale-prefixed slugs
        // (e.g., "fr/guides/intro") to match against directories (e.g., "guides").
        if (!string.IsNullOrEmpty(localeKey)
            && !string.Equals(localeKey, "root", StringComparison.OrdinalIgnoreCase))
        {
            // Only match entries that belong to this locale.
            if (!normalizedSlug.StartsWith(localeKey + "/", StringComparison.OrdinalIgnoreCase)
                && !normalizedSlug.Equals(localeKey, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            normalizedSlug = LocaleContentResolver.StripLocaleFromSlug(normalizedSlug, localeKey);
        }

        // When a version key is specified, strip the version prefix from the slug
        // before comparing against the directory. This allows version-prefixed slugs
        // (e.g., "v1.0/guides/intro") to match against directories (e.g., "guides").
        if (!string.IsNullOrEmpty(versionKey)
            && !string.Equals(versionKey, "current", StringComparison.OrdinalIgnoreCase))
        {
            // Only match entries that belong to this version.
            if (!normalizedSlug.StartsWith(versionKey + "/", StringComparison.OrdinalIgnoreCase)
                && !normalizedSlug.Equals(versionKey, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            normalizedSlug = VersionContentResolver.StripVersionFromSlug(normalizedSlug, versionKey);
        }

        if (string.IsNullOrEmpty(normalizedDir))
        {
            return true; // empty dir = all entries
        }

        return normalizedSlug.Equals(normalizedDir, StringComparison.OrdinalIgnoreCase) ||
               normalizedSlug.StartsWith(normalizedDir + "/", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsCurrent(string href, string currentPath)
    {
        // Normalize trailing slash for comparison.
        var normalizedHref = href.TrimEnd('/');
        var normalizedCurrent = currentPath.TrimEnd('/');
        return string.Equals(normalizedHref, normalizedCurrent, StringComparison.OrdinalIgnoreCase);
    }

    private static string PrefixHref(string href, string localePrefix, string basePath) =>
        PrefixHref(href, localePrefix, basePath, "");

    private static string PrefixHref(string href, string localePrefix, string basePath, string versionPrefix)
    {
        if (string.IsNullOrEmpty(localePrefix) && string.IsNullOrEmpty(versionPrefix))
        {
            return href;
        }

        // Strip the existing base path (if present), then re-compose with locale and version prefixes.
        var contentPath = LocalePathHelper.StripPrefix(href, "", basePath);
        return VersionPathHelper.PrefixPath(contentPath, versionPrefix, localePrefix, basePath);
    }
}
