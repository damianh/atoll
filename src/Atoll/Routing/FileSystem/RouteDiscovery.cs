using System.Reflection;
using Atoll.Components;

namespace Atoll.Routing.FileSystem;

/// <summary>
/// Discovers routes by scanning a pages directory structure and matching source files
/// to types that implement <see cref="IAtollComponent"/>.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="RouteDiscovery"/> implements Astro-style file-based routing: it maps a
/// directory tree of <c>.cs</c> files to URL patterns using <see cref="RouteConventions"/>.
/// Each file is expected to contain exactly one type implementing <see cref="IAtollComponent"/>
/// (or a related interface such as <c>IAtollPage</c> or <c>IAtollEndpoint</c>).
/// </para>
/// <para>
/// In production, the discovery scans compiled assemblies for types and maps them
/// to file paths via convention. For testing and SSG scenarios, an explicit type map
/// can be provided.
/// </para>
/// </remarks>
public sealed class RouteDiscovery
{
    private readonly string _pagesDirectory;

    /// <summary>
    /// Initializes a new <see cref="RouteDiscovery"/> for the specified pages directory.
    /// </summary>
    /// <param name="pagesDirectory">
    /// The root pages directory (e.g., <c>src/pages</c>). Route patterns are derived
    /// relative to this directory.
    /// </param>
    public RouteDiscovery(string pagesDirectory)
    {
        ArgumentNullException.ThrowIfNull(pagesDirectory);
        _pagesDirectory = pagesDirectory;
    }

    /// <summary>
    /// Discovers routes by scanning the pages directory for <c>.cs</c> files and resolving
    /// their types from the provided assemblies. If no file-based routes are found,
    /// falls back to scanning for types with <see cref="PageRouteAttribute"/>.
    /// </summary>
    /// <param name="assemblies">
    /// The assemblies to search for component types. Each type must implement
    /// <see cref="IAtollComponent"/> and have a class name matching the file name.
    /// </param>
    /// <returns>A list of discovered <see cref="RouteEntry"/> values.</returns>
    public IReadOnlyList<RouteEntry> DiscoverRoutes(IEnumerable<Assembly> assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);
        var assemblyList = assemblies.ToList();
        var typeMap = BuildTypeMap(assemblyList);
        var routes = DiscoverRoutesCore(typeMap);

        if (routes.Count > 0)
        {
            return routes;
        }

        // Fall back to attribute-based route discovery
        return DiscoverRoutesFromAttributes(assemblyList);
    }

    /// <summary>
    /// Discovers routes from assemblies by scanning for types annotated with
    /// <see cref="PageRouteAttribute"/>.
    /// </summary>
    /// <param name="assemblies">The assemblies to scan.</param>
    /// <returns>A list of discovered <see cref="RouteEntry"/> values.</returns>
    public static IReadOnlyList<RouteEntry> DiscoverRoutesFromAttributes(IEnumerable<Assembly> assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);
        var routes = new List<RouteEntry>();

        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.GetExportedTypes())
            {
                if (!IsRoutableType(type))
                {
                    continue;
                }

                var routeAttr = type.GetCustomAttribute<PageRouteAttribute>();
                if (routeAttr is null)
                {
                    continue;
                }

                var pattern = routeAttr.Pattern.StartsWith('/')
                    ? routeAttr.Pattern
                    : "/" + routeAttr.Pattern;
                routes.Add(new RouteEntry(pattern, type, pattern));
            }
        }

        return routes;
    }

    /// <summary>
    /// Discovers routes by scanning the pages directory for <c>.cs</c> files and resolving
    /// their types from the provided type map.
    /// </summary>
    /// <param name="typeMap">
    /// A dictionary mapping file names (without extension, case-insensitive) to their
    /// corresponding component types. For example, <c>"About" → typeof(AboutPage)</c>.
    /// </param>
    /// <returns>A list of discovered <see cref="RouteEntry"/> values.</returns>
    public IReadOnlyList<RouteEntry> DiscoverRoutes(IReadOnlyDictionary<string, Type> typeMap)
    {
        ArgumentNullException.ThrowIfNull(typeMap);
        return DiscoverRoutesCore(typeMap);
    }

    /// <summary>
    /// Discovers routes from an explicit list of relative file paths and their corresponding types.
    /// This overload does not require a physical pages directory to exist on disk.
    /// </summary>
    /// <param name="fileTypeEntries">
    /// A collection of tuples where each entry contains a relative file path (e.g.,
    /// <c>blog/[slug].cs</c>) and its corresponding component type.
    /// </param>
    /// <returns>A list of discovered <see cref="RouteEntry"/> values.</returns>
    public static IReadOnlyList<RouteEntry> DiscoverRoutesFromEntries(
        IEnumerable<(string RelativeFilePath, Type ComponentType)> fileTypeEntries)
    {
        ArgumentNullException.ThrowIfNull(fileTypeEntries);

        var routes = new List<RouteEntry>();

        foreach (var (relativeFilePath, componentType) in fileTypeEntries)
        {
            var pattern = RouteConventions.FilePathToPattern(relativeFilePath);
            routes.Add(new RouteEntry(pattern, componentType, relativeFilePath));
        }

        return routes;
    }

    private IReadOnlyList<RouteEntry> DiscoverRoutesCore(IReadOnlyDictionary<string, Type> typeMap)
    {
        if (!Directory.Exists(_pagesDirectory))
        {
            return [];
        }

        var routes = new List<RouteEntry>();
        var csFiles = Directory.GetFiles(_pagesDirectory, "*" + RouteConventions.PageFileExtension, SearchOption.AllDirectories);

        foreach (var absolutePath in csFiles)
        {
            var relativePath = GetRelativePath(absolutePath);
            var fileName = Path.GetFileNameWithoutExtension(absolutePath);

            // Skip files that start with underscore (private/partial files, e.g., _Layout.cs)
            if (fileName.StartsWith('_'))
            {
                continue;
            }

            if (!typeMap.TryGetValue(fileName, out var componentType))
            {
                continue;
            }

            var pattern = RouteConventions.FilePathToPattern(relativePath);
            routes.Add(new RouteEntry(pattern, componentType, relativePath));
        }

        return routes;
    }

    private string GetRelativePath(string absolutePath)
    {
        var fullPagesDir = Path.GetFullPath(_pagesDirectory);
        var fullFilePath = Path.GetFullPath(absolutePath);

        // Get relative path and normalize separators to forward slashes
        return Path.GetRelativePath(fullPagesDir, fullFilePath).Replace('\\', '/');
    }

    private static bool IsRoutableType(Type type)
    {
        if (type.IsAbstract || type.IsInterface)
        {
            return false;
        }

        return typeof(IAtollComponent).IsAssignableFrom(type)
               || typeof(IAtollEndpoint).IsAssignableFrom(type);
    }

    private static IReadOnlyDictionary<string, Type> BuildTypeMap(IEnumerable<Assembly> assemblies)
    {
        var typeMap = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.GetExportedTypes())
            {
                if (!IsRoutableType(type))
                {
                    continue;
                }

                // Map the class name to the type (e.g., "AboutPage" → typeof(AboutPage))
                typeMap.TryAdd(type.Name, type);
            }
        }

        return typeMap;
    }
}
