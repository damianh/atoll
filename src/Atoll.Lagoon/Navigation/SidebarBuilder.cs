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
            .ToList();

        // Build a tree of DirectoryNodes from the flat entry list.
        var root = new DirectoryNode("");
        foreach (var entry in matching)
        {
            var relativeSlug = ComputeRelativeSlug(entry.Slug, normalizedDir, localeKey, versionKey);
            var segments = relativeSlug.Split('/');

            if (segments.Length == 1)
            {
                // File directly in the root auto-generate dir.
                if (string.Equals(segments[0], "index", StringComparison.OrdinalIgnoreCase))
                {
                    root.IndexEntry = entry;
                }
                else
                {
                    root.Files.Add(entry);
                }
            }
            else
            {
                // Walk/create subdirectory nodes for all intermediate segments, then place the entry.
                var node = root;
                for (var i = 0; i < segments.Length - 1; i++)
                {
                    var seg = segments[i];
                    if (!node.Subdirectories.TryGetValue(seg, out var child))
                    {
                        child = new DirectoryNode(seg);
                        node.Subdirectories[seg] = child;
                    }

                    node = child;
                }

                var leaf = segments[^1];
                if (string.Equals(leaf, "index", StringComparison.OrdinalIgnoreCase))
                {
                    node.IndexEntry = entry;
                }
                else
                {
                    node.Files.Add(entry);
                }
            }
        }

        // Resolve root node children (the outermost group label/badge comes from the SidebarItem config).
        var children = ResolveNodeChildren(root, currentPath, localePrefix, basePath, versionPrefix, item.Collapsed);
        var isActive = children.Any(c => c.IsActive);
        return new ResolvedSidebarItem(item.Label, isActive, item.Badge, item.Collapsed, children);
    }

    /// <summary>
    /// Resolves the children of a <see cref="DirectoryNode"/> into resolved sidebar items.
    /// Files become leaf link items; subdirectories become nested groups (recursively resolved).
    /// Empty groups (after draft pruning) are omitted.
    /// </summary>
    private IReadOnlyList<ResolvedSidebarItem> ResolveNodeChildren(
        DirectoryNode node,
        string currentPath,
        string localePrefix,
        string basePath,
        string versionPrefix,
        bool groupCollapsed)
    {
        var children = new List<(int Order, string Label, ResolvedSidebarItem Item)>();

        // Resolve file entries in this directory.
        foreach (var entry in node.Files)
        {
            var href = PrefixHref(entry.Href, localePrefix, basePath, versionPrefix);
            var leaf = new ResolvedSidebarItem(entry.Label, href, IsCurrent(href, currentPath), entry.Badge);
            children.Add((entry.Order, entry.Label, leaf));
        }

        // Resolve subdirectory groups.
        foreach (var (name, subdir) in node.Subdirectories)
        {
            var subChildren = ResolveNodeChildren(subdir, currentPath, localePrefix, basePath, versionPrefix, groupCollapsed: false);

            // Prune empty groups (all entries were drafts, etc.).
            if (subChildren.Count == 0 && subdir.IndexEntry is null)
            {
                continue;
            }

            var groupLabel = subdir.IndexEntry?.Label ?? SlugLabelHelper.Humanize(name);
            var subIsActive = subChildren.Any(c => c.IsActive);

            // Determine sort order for this sub-group:
            // 1. Index entry's Order (if present) — frontmatter always wins.
            // 2. Numeric prefix of directory name (e.g., "01-basics" → 1).
            // 3. int.MaxValue — sorts after explicitly-ordered items.
            int groupOrder;
            if (subdir.IndexEntry is not null)
            {
                groupOrder = subdir.IndexEntry.Order;
            }
            else if (SlugLabelHelper.TryParseNumericPrefix(name, out var numericPrefix))
            {
                groupOrder = numericPrefix;
            }
            else
            {
                groupOrder = int.MaxValue;
            }

            ResolvedSidebarItem subGroup;
            if (subdir.IndexEntry is not null)
            {
                var indexHref = PrefixHref(subdir.IndexEntry.Href, localePrefix, basePath, versionPrefix);
                var indexIsCurrent = IsCurrent(indexHref, currentPath);
                subGroup = new ResolvedSidebarItem(
                    groupLabel,
                    indexHref,
                    indexIsCurrent,
                    subIsActive || indexIsCurrent,
                    subdir.IndexEntry.Badge,
                    collapsed: false,
                    subChildren);
            }
            else
            {
                subGroup = new ResolvedSidebarItem(groupLabel, subIsActive, null, collapsed: false, subChildren);
            }

            children.Add((groupOrder, groupLabel, subGroup));
        }

        // Sort combined children by (Order ascending, Label case-insensitive).
        return children
            .OrderBy(t => t.Order)
            .ThenBy(t => t.Label, StringComparer.OrdinalIgnoreCase)
            .Select(t => t.Item)
            .ToList();
    }

    /// <summary>
    /// Computes the relative slug of an entry within the auto-generate directory.
    /// Strips locale and version prefixes (mirroring <c>SlugMatchesDirectory</c>),
    /// then strips the auto-generate directory prefix, returning the portion that
    /// encodes the subdirectory structure within that directory.
    /// </summary>
    private static string ComputeRelativeSlug(
        string slug,
        string normalizedDir,
        string localeKey,
        string versionKey)
    {
        var normalized = slug.Replace('\\', '/');

        // Strip locale prefix (mirrors SlugMatchesDirectory logic).
        if (!string.IsNullOrEmpty(localeKey)
            && !string.Equals(localeKey, "root", StringComparison.OrdinalIgnoreCase))
        {
            normalized = LocaleContentResolver.StripLocaleFromSlug(normalized, localeKey);
        }

        // Strip version prefix.
        if (!string.IsNullOrEmpty(versionKey)
            && !string.Equals(versionKey, "current", StringComparison.OrdinalIgnoreCase))
        {
            normalized = VersionContentResolver.StripVersionFromSlug(normalized, versionKey);
        }

        // Strip the auto-generate directory prefix to get the relative slug.
        if (!string.IsNullOrEmpty(normalizedDir))
        {
            if (normalized.StartsWith(normalizedDir + "/", StringComparison.OrdinalIgnoreCase))
            {
                normalized = normalized[(normalizedDir.Length + 1)..];
            }
            else if (string.Equals(normalized, normalizedDir, StringComparison.OrdinalIgnoreCase))
            {
                normalized = "";
            }
        }

        return normalized;
    }

    /// <summary>
    /// Intermediate tree node used during auto-generate group resolution.
    /// Represents a single directory level within the content hierarchy.
    /// </summary>
    private sealed class DirectoryNode(string name)
    {
        /// <summary>Gets the directory segment name (e.g., <c>"getting-started"</c>).</summary>
        public string Name { get; } = name;

        /// <summary>
        /// Gets or sets the entry for an <c>index.md</c> file in this directory, if present.
        /// When set, this entry's label and href become the group label and clickable link.
        /// </summary>
        public SidebarEntry? IndexEntry { get; set; }

        /// <summary>Gets the non-index entries directly within this directory.</summary>
        public List<SidebarEntry> Files { get; } = [];

        /// <summary>Gets the child directory nodes, keyed by directory segment name.</summary>
        public Dictionary<string, DirectoryNode> Subdirectories { get; } = new(StringComparer.OrdinalIgnoreCase);
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
