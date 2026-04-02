using Atoll.Core.Components;
using Atoll.Routing;

namespace Atoll.Build.Ssg;

/// <summary>
/// Represents a single page to render during static site generation, with its
/// route pattern resolved to a concrete URL path and associated parameters.
/// </summary>
public sealed class SsgRoute
{
    /// <summary>
    /// Initializes a new <see cref="SsgRoute"/> with the specified URL path, component type,
    /// and route parameters.
    /// </summary>
    /// <param name="urlPath">The concrete URL path (e.g., <c>/blog/hello-world</c>).</param>
    /// <param name="componentType">The page component type.</param>
    /// <param name="parameters">The route parameter values.</param>
    /// <param name="props">Additional props to pass to the page component.</param>
    public SsgRoute(
        string urlPath,
        Type componentType,
        IReadOnlyDictionary<string, string> parameters,
        IReadOnlyDictionary<string, object?> props)
    {
        ArgumentNullException.ThrowIfNull(urlPath);
        ArgumentNullException.ThrowIfNull(componentType);
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentNullException.ThrowIfNull(props);
        UrlPath = urlPath;
        ComponentType = componentType;
        Parameters = parameters;
        Props = props;
    }

    /// <summary>
    /// Initializes a new <see cref="SsgRoute"/> with the specified URL path, component type,
    /// and route parameters, with no additional props.
    /// </summary>
    /// <param name="urlPath">The concrete URL path.</param>
    /// <param name="componentType">The page component type.</param>
    /// <param name="parameters">The route parameter values.</param>
    public SsgRoute(
        string urlPath,
        Type componentType,
        IReadOnlyDictionary<string, string> parameters)
        : this(urlPath, componentType, parameters, EmptyProps)
    {
    }

    /// <summary>
    /// Initializes a new <see cref="SsgRoute"/> with the specified URL path and component type,
    /// with no parameters or props.
    /// </summary>
    /// <param name="urlPath">The concrete URL path.</param>
    /// <param name="componentType">The page component type.</param>
    public SsgRoute(string urlPath, Type componentType)
        : this(urlPath, componentType, EmptyParams, EmptyProps)
    {
    }

    /// <summary>
    /// Gets the concrete URL path for this page (e.g., <c>/blog/hello-world</c>).
    /// </summary>
    public string UrlPath { get; }

    /// <summary>
    /// Gets the page component type.
    /// </summary>
    public Type ComponentType { get; }

    /// <summary>
    /// Gets the route parameter values.
    /// </summary>
    public IReadOnlyDictionary<string, string> Parameters { get; }

    /// <summary>
    /// Gets additional props to pass to the page component.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Props { get; }

    private static readonly IReadOnlyDictionary<string, string> EmptyParams =
        new Dictionary<string, string>();

    private static readonly IReadOnlyDictionary<string, object?> EmptyProps =
        new Dictionary<string, object?>();
}

/// <summary>
/// Expands dynamic route entries into concrete URL paths by calling
/// <see cref="IStaticPathsProvider.GetStaticPathsAsync"/> on page components
/// that implement the interface.
/// </summary>
/// <remarks>
/// <para>
/// Static routes (e.g., <c>/about</c>) produce a single <see cref="SsgRoute"/>.
/// Dynamic routes (e.g., <c>/blog/[slug]</c>) are expanded by instantiating
/// the page component, calling <see cref="IStaticPathsProvider.GetStaticPathsAsync"/>,
/// and producing one <see cref="SsgRoute"/> per returned <see cref="StaticPath"/>.
/// </para>
/// <para>
/// Dynamic routes that do NOT implement <see cref="IStaticPathsProvider"/> will
/// produce an error since the SSG engine cannot determine what paths to generate.
/// </para>
/// </remarks>
public sealed class RouteEnumerator
{
    /// <summary>
    /// Enumerates all concrete SSG routes from the specified route entries.
    /// </summary>
    /// <param name="routes">The route entries to expand.</param>
    /// <returns>A list of concrete <see cref="SsgRoute"/> instances ready for rendering.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a dynamic route's component type does not implement <see cref="IStaticPathsProvider"/>.
    /// </exception>
    public async Task<IReadOnlyList<SsgRoute>> EnumerateAsync(IEnumerable<RouteEntry> routes)
    {
        ArgumentNullException.ThrowIfNull(routes);

        var result = new List<SsgRoute>();

        foreach (var route in routes)
        {
            // Skip endpoints — only render pages
            if (typeof(IAtollEndpoint).IsAssignableFrom(route.ComponentType))
            {
                continue;
            }

            if (IsDynamicPattern(route.Pattern))
            {
                var expandedRoutes = await ExpandDynamicRouteAsync(route);
                result.AddRange(expandedRoutes);
            }
            else
            {
                result.Add(new SsgRoute(
                    NormalizeUrlPath(route.Pattern),
                    route.ComponentType));
            }
        }

        return result;
    }

    private static async Task<IReadOnlyList<SsgRoute>> ExpandDynamicRouteAsync(RouteEntry route)
    {
        if (!typeof(IStaticPathsProvider).IsAssignableFrom(route.ComponentType))
        {
            throw new InvalidOperationException(
                $"Dynamic route '{route.Pattern}' requires component type '{route.ComponentType.FullName}' " +
                $"to implement {nameof(IStaticPathsProvider)} for static site generation. " +
                $"Implement GetStaticPathsAsync() to declare all possible paths.");
        }

        var instance = (IStaticPathsProvider)Activator.CreateInstance(route.ComponentType)!;
        var staticPaths = await instance.GetStaticPathsAsync();

        var result = new List<SsgRoute>(staticPaths.Count);
        foreach (var staticPath in staticPaths)
        {
            var resolvedUrl = ResolveUrlPath(route.Pattern, staticPath.Parameters);
            result.Add(new SsgRoute(
                resolvedUrl,
                route.ComponentType,
                staticPath.Parameters,
                staticPath.Props));
        }

        return result;
    }

    /// <summary>
    /// Resolves a dynamic route pattern to a concrete URL by substituting parameters.
    /// </summary>
    /// <param name="pattern">The route pattern (e.g., <c>/blog/[slug]</c>).</param>
    /// <param name="parameters">The parameter values to substitute.</param>
    /// <returns>The resolved URL path (e.g., <c>/blog/hello-world</c>).</returns>
    internal static string ResolveUrlPath(
        string pattern,
        IReadOnlyDictionary<string, string> parameters)
    {
        var segments = pattern.Trim('/').Split('/');
        var resolved = new List<string>(segments.Length);

        foreach (var segment in segments)
        {
            if (segment.StartsWith("[...", StringComparison.Ordinal) && segment.EndsWith(']'))
            {
                // Catch-all segment: [...rest]
                var paramName = segment[4..^1];
                if (parameters.TryGetValue(paramName, out var catchAllValue) && catchAllValue.Length > 0)
                {
                    resolved.Add(catchAllValue);
                }
                // Empty catch-all produces no segment
            }
            else if (segment.StartsWith('[') && segment.EndsWith(']'))
            {
                // Dynamic segment: [slug]
                var paramName = segment[1..^1];
                if (!parameters.TryGetValue(paramName, out var paramValue))
                {
                    throw new InvalidOperationException(
                        $"Missing parameter '{paramName}' for route pattern '{pattern}'. " +
                        $"Available parameters: {string.Join(", ", parameters.Keys)}.");
                }
                resolved.Add(paramValue);
            }
            else
            {
                // Static segment
                resolved.Add(segment);
            }
        }

        return "/" + string.Join("/", resolved);
    }

    private static bool IsDynamicPattern(string pattern)
    {
        return pattern.Contains('[');
    }

    private static string NormalizeUrlPath(string pattern)
    {
        if (pattern.Length == 0 || pattern == "/")
        {
            return "/";
        }

        return pattern.StartsWith('/')
            ? pattern
            : "/" + pattern;
    }
}
