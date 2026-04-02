namespace Atoll.Routing;

/// <summary>
/// Represents a single route entry in the route table, mapping a URL pattern
/// to a component or endpoint type.
/// </summary>
/// <remarks>
/// <para>
/// A <see cref="RouteEntry"/> is produced by <see cref="FileSystem.RouteDiscovery"/>
/// when scanning the pages directory. Each entry captures the URL pattern (e.g.,
/// <c>/blog/[slug]</c>), the .NET type that handles the route, and whether the
/// route should be pre-rendered at build time.
/// </para>
/// </remarks>
public sealed class RouteEntry
{
    /// <summary>
    /// Initializes a new <see cref="RouteEntry"/> with the specified pattern, component type,
    /// relative file path, and prerender flag.
    /// </summary>
    /// <param name="pattern">The URL pattern for this route (e.g., <c>/blog/[slug]</c>).</param>
    /// <param name="componentType">The .NET type that handles this route.</param>
    /// <param name="relativeFilePath">The file path relative to the pages directory.</param>
    /// <param name="prerender">Whether this route should be pre-rendered at build time.</param>
    public RouteEntry(string pattern, Type componentType, string relativeFilePath, bool prerender)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        ArgumentNullException.ThrowIfNull(componentType);
        ArgumentNullException.ThrowIfNull(relativeFilePath);
        Pattern = pattern;
        ComponentType = componentType;
        RelativeFilePath = relativeFilePath;
        Prerender = prerender;
    }

    /// <summary>
    /// Initializes a new <see cref="RouteEntry"/> with the specified pattern, component type,
    /// and relative file path. Prerender defaults to <c>false</c>.
    /// </summary>
    /// <param name="pattern">The URL pattern for this route.</param>
    /// <param name="componentType">The .NET type that handles this route.</param>
    /// <param name="relativeFilePath">The file path relative to the pages directory.</param>
    public RouteEntry(string pattern, Type componentType, string relativeFilePath)
        : this(pattern, componentType, relativeFilePath, false)
    {
    }

    /// <summary>
    /// Gets the URL pattern for this route.
    /// </summary>
    /// <remarks>
    /// Patterns use Astro-style segment syntax:
    /// <list type="bullet">
    /// <item><description>Static: <c>/about</c></description></item>
    /// <item><description>Dynamic: <c>/blog/[slug]</c></description></item>
    /// <item><description>Catch-all: <c>/docs/[...rest]</c></description></item>
    /// </list>
    /// </remarks>
    public string Pattern { get; }

    /// <summary>
    /// Gets the .NET type that handles this route. This type typically implements
    /// <c>IAtollPage</c> or <c>IAtollEndpoint</c>.
    /// </summary>
    public Type ComponentType { get; }

    /// <summary>
    /// Gets the file path relative to the pages directory (using forward slashes).
    /// </summary>
    public string RelativeFilePath { get; }

    /// <summary>
    /// Gets a value indicating whether this route should be pre-rendered at build time (SSG).
    /// </summary>
    public bool Prerender { get; }
}
