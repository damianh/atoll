using Atoll.Redirects;

namespace Atoll.Lagoon.Redirects;

/// <summary>
/// Collects and merges redirect definitions from config-based and frontmatter-based sources
/// into a single <see cref="RedirectMap"/>.
/// </summary>
/// <remarks>
/// <para>
/// The collector is intentionally decoupled from <c>DocsConfig</c> and from specific
/// frontmatter schema types. The caller extracts <c>redirect_from</c> values from their
/// own content entries and passes them as <see cref="RedirectSource"/> instances.
/// </para>
/// <para>
/// Conflict detection rules:
/// <list type="bullet">
///   <item><description>Two frontmatter entries claiming the same source path.</description></item>
///   <item><description>A config redirect conflicting with a frontmatter redirect.</description></item>
///   <item><description>A redirect source path that matches a canonical content page URL.</description></item>
/// </list>
/// A <see cref="RedirectConflictException"/> is thrown when any conflict is detected.
/// </para>
/// </remarks>
public sealed class RedirectCollector
{
    private readonly IReadOnlyDictionary<string, string>? _configRedirects;

    /// <summary>
    /// Initializes a new <see cref="RedirectCollector"/> with no config-based redirects.
    /// </summary>
    public RedirectCollector()
        : this(null)
    {
    }

    /// <summary>
    /// Initializes a new <see cref="RedirectCollector"/> with the specified config-based redirects.
    /// </summary>
    /// <param name="configRedirects">
    /// Config-declared redirects (e.g., from <c>DocsConfig.Redirects</c>).
    /// Keys are source paths, values are target paths. May be <c>null</c>.
    /// </param>
    public RedirectCollector(IReadOnlyDictionary<string, string>? configRedirects)
    {
        _configRedirects = configRedirects;
    }

    /// <summary>
    /// Collects all redirect entries from frontmatter sources and merges them with
    /// config-based redirects into a single <see cref="RedirectMap"/>.
    /// </summary>
    /// <param name="entries">
    /// Content entries declaring their canonical URL and frontmatter-based redirect sources.
    /// </param>
    /// <returns>A <see cref="RedirectMap"/> containing all merged redirect entries.</returns>
    /// <exception cref="RedirectConflictException">
    /// Thrown when duplicate source paths or source-vs-canonical conflicts are detected.
    /// </exception>
    public RedirectMap Collect(IReadOnlyList<RedirectSource> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        // Collect all canonical URLs for conflict detection (source must not be an existing page)
        var canonicalUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in entries)
        {
            canonicalUrls.Add(RedirectMap.NormalizePath(entry.CanonicalUrl));
        }

        // source → target map being built
        var allRedirects = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // 1. Seed with config-based redirects
        if (_configRedirects is not null)
        {
            foreach (var (source, target) in _configRedirects)
            {
                var normalizedSource = RedirectMap.NormalizePath(source);
                var normalizedTarget = RedirectMap.NormalizePath(target);

                if (canonicalUrls.Contains(normalizedSource))
                {
                    throw new RedirectConflictException(
                        $"Redirect conflict: source path '{normalizedSource}' is also a canonical content URL. " +
                        $"Redirect sources cannot overlap with existing pages.");
                }

                if (!allRedirects.TryAdd(normalizedSource, normalizedTarget))
                {
                    throw new RedirectConflictException(
                        $"Redirect conflict: source path '{normalizedSource}' is declared more than once in config redirects.");
                }
            }
        }

        // 2. Merge frontmatter redirects
        foreach (var entry in entries)
        {
            if (entry.RedirectFrom is null || entry.RedirectFrom.Count == 0)
            {
                continue;
            }

            var normalizedTarget = RedirectMap.NormalizePath(entry.CanonicalUrl);

            foreach (var sourcePath in entry.RedirectFrom)
            {
                var normalizedSource = RedirectMap.NormalizePath(sourcePath);

                if (canonicalUrls.Contains(normalizedSource))
                {
                    throw new RedirectConflictException(
                        $"Redirect conflict: source path '{normalizedSource}' (declared in frontmatter of '{entry.CanonicalUrl}') " +
                        $"is also a canonical content URL. Redirect sources cannot overlap with existing pages.");
                }

                if (allRedirects.TryGetValue(normalizedSource, out var existingTarget))
                {
                    throw new RedirectConflictException(
                        $"Redirect conflict: source path '{normalizedSource}' is already mapped to '{existingTarget}'. " +
                        $"Cannot also redirect it to '{normalizedTarget}' (declared in frontmatter of '{entry.CanonicalUrl}').");
                }

                allRedirects[normalizedSource] = normalizedTarget;
            }
        }

        return RedirectMap.Create(allRedirects);
    }
}
