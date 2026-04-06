using System.Collections.ObjectModel;

namespace Atoll.Redirects;

/// <summary>
/// An immutable mapping of source paths to redirect target paths.
/// </summary>
/// <remarks>
/// <para>
/// All paths stored in a <see cref="RedirectMap"/> are normalized to lowercase,
/// have a leading <c>/</c>, and have any trailing <c>/</c> stripped.
/// Lookups use the same normalization, so matching is effectively case-insensitive
/// and trailing-slash-insensitive.
/// </para>
/// <para>
/// Paths are <b>base-relative</b> — they do not include any site base path prefix
/// (e.g. <c>DocsConfig.BasePath</c>). The middleware layer strips the base path
/// before performing a lookup.
/// </para>
/// </remarks>
public sealed class RedirectMap
{
    private readonly IReadOnlyDictionary<string, string> _entries;

    private RedirectMap(IReadOnlyDictionary<string, string> entries)
    {
        _entries = entries;
    }

    /// <summary>
    /// Gets the number of redirect entries in the map.
    /// </summary>
    public int Count => _entries.Count;

    /// <summary>
    /// Gets the redirect entries as an immutable dictionary mapping source paths to target paths.
    /// </summary>
    public IReadOnlyDictionary<string, string> Entries => _entries;

    /// <summary>
    /// Gets an empty <see cref="RedirectMap"/> with no entries.
    /// </summary>
    public static RedirectMap Empty { get; } = new RedirectMap(
        new ReadOnlyDictionary<string, string>(new Dictionary<string, string>()));

    /// <summary>
    /// Creates a new <see cref="RedirectMap"/> from the specified redirect entries.
    /// </summary>
    /// <param name="entries">
    /// A sequence of key-value pairs where the key is the source path and the value
    /// is the redirect target path. Both are normalized on ingestion.
    /// </param>
    /// <returns>A new immutable <see cref="RedirectMap"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when two entries share the same normalized source path.
    /// </exception>
    public static RedirectMap Create(IEnumerable<KeyValuePair<string, string>> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (source, target) in entries)
        {
            var normalizedSource = NormalizePath(source);
            var normalizedTarget = NormalizePath(target);

            if (!map.TryAdd(normalizedSource, normalizedTarget))
            {
                throw new InvalidOperationException(
                    $"Duplicate redirect source path '{normalizedSource}'. " +
                    $"Each source path may only appear once in the redirect map.");
            }
        }

        return new RedirectMap(new ReadOnlyDictionary<string, string>(map));
    }

    /// <summary>
    /// Attempts to look up the redirect target for a given source path.
    /// </summary>
    /// <param name="path">The request path to look up (normalized or un-normalized).</param>
    /// <param name="target">
    /// When this method returns <c>true</c>, contains the redirect target path.
    /// When this method returns <c>false</c>, this value is <c>null</c>.
    /// </param>
    /// <returns><c>true</c> if a redirect exists for <paramref name="path"/>; otherwise, <c>false</c>.</returns>
    public bool TryGetRedirect(string path, out string target)
    {
        ArgumentNullException.ThrowIfNull(path);
        var normalized = NormalizePath(path);
        return _entries.TryGetValue(normalized, out target!);
    }

    /// <summary>
    /// Normalizes a path to lowercase with a leading <c>/</c> and no trailing <c>/</c>.
    /// </summary>
    public static string NormalizePath(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        path = path.Trim().ToLowerInvariant();
        if (!path.StartsWith('/'))
        {
            path = "/" + path;
        }

        if (path.Length > 1 && path.EndsWith('/'))
        {
            path = path.TrimEnd('/');
        }

        return path;
    }
}
