namespace Atoll.Lagoon.Validation;

/// <summary>
/// Builds and stores a map of known page URL paths and their heading anchor IDs,
/// used during link validation to check whether link targets exist.
/// </summary>
/// <remarks>
/// URL paths are normalised to always include a trailing slash so that
/// <c>/docs/page</c> and <c>/docs/page/</c> are treated as the same page.
/// Paths that appear to reference a file (i.e. the last path segment contains a dot)
/// are stored as-is without adding a trailing slash.
/// </remarks>
public sealed class PageRegistry
{
    private readonly Dictionary<string, HashSet<string>> _pages =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Registers a page and its heading anchor IDs.
    /// </summary>
    /// <param name="urlPath">The URL path of the page (e.g. <c>/docs/getting-started/</c>).</param>
    /// <param name="anchorIds">
    /// The IDs of heading anchors present on the page (without leading <c>#</c>).
    /// </param>
    public void Register(string urlPath, IReadOnlyList<string> anchorIds)
    {
        ArgumentNullException.ThrowIfNull(urlPath);
        ArgumentNullException.ThrowIfNull(anchorIds);

        var normalized = NormalizePath(urlPath);
        if (!_pages.TryGetValue(normalized, out var anchors))
        {
            anchors = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _pages[normalized] = anchors;
        }

        foreach (var id in anchorIds)
        {
            anchors.Add(id);
        }
    }

    /// <summary>
    /// Checks whether a page with the given URL path is registered.
    /// Both <c>/docs/page</c> and <c>/docs/page/</c> are accepted.
    /// </summary>
    /// <param name="urlPath">The URL path to look up.</param>
    /// <returns><c>true</c> if the page is registered; otherwise <c>false</c>.</returns>
    public bool PageExists(string urlPath)
    {
        ArgumentNullException.ThrowIfNull(urlPath);
        return _pages.ContainsKey(NormalizePath(urlPath));
    }

    /// <summary>
    /// Checks whether a specific anchor ID exists on the given page.
    /// Returns <c>false</c> if the page itself is not registered.
    /// </summary>
    /// <param name="urlPath">The URL path of the page.</param>
    /// <param name="anchorId">The anchor ID to look up (without leading <c>#</c>).</param>
    /// <returns>
    /// <c>true</c> if the page is registered and the anchor exists; otherwise <c>false</c>.
    /// </returns>
    public bool AnchorExists(string urlPath, string anchorId)
    {
        ArgumentNullException.ThrowIfNull(urlPath);
        ArgumentNullException.ThrowIfNull(anchorId);

        var normalized = NormalizePath(urlPath);
        return _pages.TryGetValue(normalized, out var anchors)
            && anchors.Contains(anchorId);
    }

    private static string NormalizePath(string path)
    {
        if (path.Length == 0)
        {
            return "/";
        }

        // Paths that look like files (last segment has a dot) — preserve as-is
        var lastSegment = path.LastIndexOf('/') is var idx and >= 0
            ? path[(idx + 1)..]
            : path;

        if (lastSegment.Contains('.'))
        {
            return path;
        }

        return path.EndsWith('/') ? path : path + "/";
    }
}
