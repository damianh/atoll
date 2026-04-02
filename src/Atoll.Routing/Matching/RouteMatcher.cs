using Atoll.Routing.FileSystem;

namespace Atoll.Routing.Matching;

/// <summary>
/// Represents the result of matching a URL path against a route table.
/// </summary>
public sealed class RouteMatchResult
{
    /// <summary>
    /// Initializes a new <see cref="RouteMatchResult"/> with the matched route entry
    /// and extracted parameters.
    /// </summary>
    /// <param name="routeEntry">The matched route entry.</param>
    /// <param name="parameters">The extracted route parameters.</param>
    public RouteMatchResult(
        RouteEntry routeEntry,
        IReadOnlyDictionary<string, string> parameters)
    {
        ArgumentNullException.ThrowIfNull(routeEntry);
        ArgumentNullException.ThrowIfNull(parameters);
        RouteEntry = routeEntry;
        Parameters = parameters;
    }

    /// <summary>
    /// Gets the matched route entry.
    /// </summary>
    public RouteEntry RouteEntry { get; }

    /// <summary>
    /// Gets the extracted route parameters.
    /// For <c>/blog/[slug]</c> matching <c>/blog/hello-world</c>,
    /// this would contain <c>{ "slug": "hello-world" }</c>.
    /// </summary>
    public IReadOnlyDictionary<string, string> Parameters { get; }
}

/// <summary>
/// Matches incoming URL paths against a route table to find the best-matching route
/// and extract route parameters.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="RouteMatcher"/> takes a collection of <see cref="RouteEntry"/> values,
/// parses their patterns, sorts them by priority (most specific first), and provides
/// efficient URL matching.
/// </para>
/// <para>
/// Route priority ordering follows Astro conventions:
/// <list type="number">
/// <item><description>Static routes take precedence over dynamic routes.</description></item>
/// <item><description>Dynamic routes with more static segments take precedence.</description></item>
/// <item><description>Dynamic routes take precedence over catch-all routes.</description></item>
/// <item><description>Catch-all routes are matched last.</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed class RouteMatcher
{
    private readonly IReadOnlyList<(RouteEntry Entry, RoutePattern Pattern)> _routes;

    /// <summary>
    /// Initializes a new <see cref="RouteMatcher"/> with the specified route entries.
    /// Routes are sorted by priority (most specific first).
    /// </summary>
    /// <param name="routes">The route entries to match against.</param>
    public RouteMatcher(IEnumerable<RouteEntry> routes)
    {
        ArgumentNullException.ThrowIfNull(routes);

        var parsed = routes
            .Select(r => (Entry: r, Pattern: new RoutePattern(r.Pattern)))
            .ToList();

        parsed.Sort((a, b) => RouteComparer.Compare(a.Pattern, b.Pattern));

        _routes = parsed;
    }

    /// <summary>
    /// Gets the sorted route entries (most specific first) for diagnostics and testing.
    /// </summary>
    public IReadOnlyList<RouteEntry> SortedRoutes => _routes.Select(r => r.Entry).ToList();

    /// <summary>
    /// Attempts to match the specified URL path against the route table.
    /// </summary>
    /// <param name="path">The URL path to match (e.g., <c>/blog/hello-world</c>).</param>
    /// <returns>
    /// A <see cref="RouteMatchResult"/> if a match was found; otherwise, <c>null</c>.
    /// </returns>
    public RouteMatchResult? Match(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        var pathSegments = SplitPath(path);

        foreach (var (entry, pattern) in _routes)
        {
            var parameters = TryMatch(pattern, pathSegments);
            if (parameters is not null)
            {
                return new RouteMatchResult(entry, parameters);
            }
        }

        return null;
    }

    private static IReadOnlyDictionary<string, string>? TryMatch(
        RoutePattern pattern,
        string[] pathSegments)
    {
        var routeSegments = pattern.Segments;

        // Root pattern: matches only empty path
        if (routeSegments.Length == 0)
        {
            return pathSegments.Length == 0
                ? new Dictionary<string, string>()
                : null;
        }

        var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < routeSegments.Length; i++)
        {
            var segment = routeSegments[i];

            switch (segment.SegmentType)
            {
                case RouteSegmentType.Static:
                    if (i >= pathSegments.Length)
                    {
                        return null;
                    }
                    if (!string.Equals(segment.Value, pathSegments[i], StringComparison.OrdinalIgnoreCase))
                    {
                        return null;
                    }
                    break;

                case RouteSegmentType.Dynamic:
                    if (i >= pathSegments.Length)
                    {
                        return null;
                    }
                    if (pathSegments[i].Length == 0)
                    {
                        return null;
                    }
                    parameters[segment.Value] = pathSegments[i];
                    break;

                case RouteSegmentType.CatchAll:
                    // Catch-all matches zero or more remaining segments
                    var remaining = i < pathSegments.Length
                        ? string.Join("/", pathSegments.Skip(i))
                        : string.Empty;
                    parameters[segment.Value] = remaining;
                    return parameters;
            }
        }

        // All route segments matched — ensure no extra path segments remain
        return routeSegments.Length == pathSegments.Length
            ? parameters
            : null;
    }

    private static string[] SplitPath(string path)
    {
        var trimmed = path.Trim('/');
        return trimmed.Length == 0
            ? []
            : trimmed.Split('/');
    }
}
