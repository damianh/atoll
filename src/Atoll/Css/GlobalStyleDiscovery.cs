using System.Reflection;

namespace Atoll.Css;

/// <summary>
/// Scans assemblies for component types decorated with both <see cref="GlobalStyleAttribute"/>
/// and <see cref="StylesAttribute"/>, suitable for contributing global (unscoped) CSS to
/// the build pipeline via <see cref="CssAggregator.Add(Type)"/>.
/// </summary>
public static class GlobalStyleDiscovery
{
    /// <summary>
    /// Scans the specified assembly for types that have both
    /// <see cref="GlobalStyleAttribute"/> and <see cref="StylesAttribute"/> applied.
    /// Abstract types and interfaces are excluded.
    /// </summary>
    /// <param name="assembly">The assembly to scan.</param>
    /// <returns>
    /// A read-only list of discovered types. Returns an empty list if no matching types are found.
    /// </returns>
    public static IReadOnlyList<Type> DiscoverGlobalStyles(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        return DiscoverFromAssemblies([assembly]);
    }

    /// <summary>
    /// Scans the specified assemblies for types that have both
    /// <see cref="GlobalStyleAttribute"/> and <see cref="StylesAttribute"/> applied.
    /// Abstract types and interfaces are excluded. Results are deduplicated by type.
    /// </summary>
    /// <param name="assemblies">The assemblies to scan.</param>
    /// <returns>
    /// A read-only list of discovered types. Returns an empty list if no matching types are found.
    /// </returns>
    public static IReadOnlyList<Type> DiscoverGlobalStyles(IEnumerable<Assembly> assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);

        return DiscoverFromAssemblies(assemblies);
    }

    private static IReadOnlyList<Type> DiscoverFromAssemblies(IEnumerable<Assembly> assemblies)
    {
        var seen = new HashSet<Type>();
        var result = new List<Type>();

        foreach (var assembly in assemblies)
        {
            if (assembly is null)
            {
                throw new ArgumentException("Assemblies collection must not contain null entries.", nameof(assemblies));
            }

            foreach (var type in GetExportedTypes(assembly))
            {
                if (type.IsAbstract || type.IsInterface)
                {
                    continue;
                }

                if (!StyleScoper.HasStyles(type) || !StyleScoper.IsGlobal(type))
                {
                    continue;
                }

                if (seen.Add(type))
                {
                    result.Add(type);
                }
            }
        }

        return result.AsReadOnly();
    }

    private static IEnumerable<Type> GetExportedTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            // Return only the types that loaded successfully
            return ex.Types.Where(t => t is not null).Cast<Type>();
        }
    }
}
