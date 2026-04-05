using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Text.Json;

namespace Atoll.Cli.Commands;

/// <summary>
/// Resolves managed and native assemblies for a user project loaded into an isolated
/// <see cref="AssemblyLoadContext"/> by reading the project's <c>.deps.json</c> file
/// and probing the NuGet global packages folder.
/// <para>
/// <see cref="AssemblyDependencyResolver"/> does not work for class library projects because
/// they lack a <c>.runtimeconfig.json</c> file that tells the resolver where NuGet packages live.
/// This class fills that gap by parsing <c>.deps.json</c> directly and constructing full paths
/// into the NuGet cache.
/// </para>
/// </summary>
internal sealed class DepsJsonAssemblyResolver
{
    private readonly string _assemblyDir;
    private readonly Dictionary<string, string> _managedAssemblyPaths;
    private readonly Dictionary<string, string> _nativeLibraryPaths;

    private DepsJsonAssemblyResolver(
        string assemblyDir,
        Dictionary<string, string> managedAssemblyPaths,
        Dictionary<string, string> nativeLibraryPaths)
    {
        _assemblyDir = assemblyDir;
        _managedAssemblyPaths = managedAssemblyPaths;
        _nativeLibraryPaths = nativeLibraryPaths;
    }

    /// <summary>
    /// Creates a resolver by parsing the <c>.deps.json</c> adjacent to the given assembly path.
    /// Returns <c>null</c> if the deps file does not exist or cannot be parsed.
    /// </summary>
    public static DepsJsonAssemblyResolver? Create(string assemblyPath)
    {
        var fullPath = Path.GetFullPath(assemblyPath);
        var assemblyDir = Path.GetDirectoryName(fullPath)!;
        var depsPath = Path.Combine(
            assemblyDir,
            Path.GetFileNameWithoutExtension(fullPath) + ".deps.json");

        if (!File.Exists(depsPath))
        {
            return null;
        }

        var nugetPackagesDir = GetNuGetPackagesDirectory();
        if (nugetPackagesDir is null || !Directory.Exists(nugetPackagesDir))
        {
            return null;
        }

        var (managed, native) = ParseDepsJson(depsPath, nugetPackagesDir);
        return new DepsJsonAssemblyResolver(assemblyDir, managed, native);
    }

    /// <summary>
    /// Resolves a managed assembly name to a file path.
    /// Returns <c>null</c> if the assembly is not found.
    /// </summary>
    public string? ResolveAssemblyToPath(AssemblyName assemblyName)
    {
        var name = assemblyName.Name;
        if (name is null)
        {
            return null;
        }

        // First check the NuGet package map built from deps.json.
        if (_managedAssemblyPaths.TryGetValue(name, out var nugetPath) && File.Exists(nugetPath))
        {
            return nugetPath;
        }

        // Fallback: probe the assembly's output directory for project references.
        var candidate = Path.Combine(_assemblyDir, name + ".dll");
        return File.Exists(candidate) ? candidate : null;
    }

    /// <summary>
    /// Resolves a native library name to a file path.
    /// Returns <c>null</c> if the library is not found.
    /// </summary>
    public string? ResolveUnmanagedDllToPath(string unmanagedDllName)
    {
        if (_nativeLibraryPaths.TryGetValue(unmanagedDllName, out var nativePath) && File.Exists(nativePath))
        {
            return nativePath;
        }

        return null;
    }

    /// <summary>
    /// Wires this resolver into an <see cref="AssemblyLoadContext"/>'s
    /// <c>Resolving</c> and <c>ResolvingUnmanagedDll</c> events.
    /// </summary>
    public void Attach(AssemblyLoadContext loadContext)
    {
        loadContext.Resolving += (context, assemblyName) =>
        {
            var resolvedPath = ResolveAssemblyToPath(assemblyName);
            return resolvedPath is not null
                ? context.LoadFromAssemblyPath(resolvedPath)
                : null;
        };

        loadContext.ResolvingUnmanagedDll += (_, unmanagedDllName) =>
        {
            var resolvedPath = ResolveUnmanagedDllToPath(unmanagedDllName);
            return resolvedPath is not null
                ? NativeLibrary.Load(resolvedPath)
                : IntPtr.Zero;
        };
    }

    private static string? GetNuGetPackagesDirectory()
    {
        // NUGET_PACKAGES env var takes precedence (CI, custom setups).
        var envPath = Environment.GetEnvironmentVariable("NUGET_PACKAGES");
        if (!string.IsNullOrEmpty(envPath))
        {
            return envPath;
        }

        // Default: ~/.nuget/packages
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return string.IsNullOrEmpty(userProfile)
            ? null
            : Path.Combine(userProfile, ".nuget", "packages");
    }

    /// <summary>
    /// Parses a <c>.deps.json</c> file and builds maps of:
    /// <list type="bullet">
    /// <item>Managed assembly name → full file path (from <c>runtime</c> assets)</item>
    /// <item>Native library name → full file path (from <c>runtimeTargets</c> assets matching the current RID)</item>
    /// </list>
    /// </summary>
    private static (Dictionary<string, string> Managed, Dictionary<string, string> Native) ParseDepsJson(
        string depsPath,
        string nugetPackagesDir)
    {
        var managed = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var native = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var currentRid = RuntimeInformation.RuntimeIdentifier;

        try
        {
            using var stream = File.OpenRead(depsPath);
            using var doc = JsonDocument.Parse(stream);

            var root = doc.RootElement;

            // Determine the runtime target (e.g. ".NETCoreApp,Version=v10.0").
            if (!root.TryGetProperty("runtimeTarget", out var runtimeTarget))
            {
                return (managed, native);
            }

            var targetName = runtimeTarget.TryGetProperty("name", out var nameElement)
                ? nameElement.GetString()
                : null;

            if (targetName is null
                || !root.TryGetProperty("targets", out var targets)
                || !targets.TryGetProperty(targetName, out var targetEntries)
                || !root.TryGetProperty("libraries", out var libraries))
            {
                return (managed, native);
            }

            // Build a set of RIDs to accept, from most specific to least.
            var acceptableRids = GetRidFallbacks(currentRid);

            // Iterate all entries in the target.
            foreach (var entry in targetEntries.EnumerateObject())
            {
                // Only process NuGet packages (skip project references).
                if (!libraries.TryGetProperty(entry.Name, out var libEntry))
                {
                    continue;
                }

                if (!libEntry.TryGetProperty("type", out var typeElement)
                    || typeElement.GetString() != "package")
                {
                    continue;
                }

                if (!libEntry.TryGetProperty("path", out var pathElement))
                {
                    continue;
                }

                var packagePath = pathElement.GetString();
                if (packagePath is null)
                {
                    continue;
                }

                // Extract managed runtime assets.
                if (entry.Value.TryGetProperty("runtime", out var runtimeAssets))
                {
                    foreach (var asset in runtimeAssets.EnumerateObject())
                    {
                        var assetRelativePath = asset.Name;
                        var assemblyFileName = Path.GetFileNameWithoutExtension(assetRelativePath);
                        var fullAssetPath = Path.GetFullPath(
                            Path.Combine(nugetPackagesDir, packagePath, assetRelativePath));

                        managed[assemblyFileName] = fullAssetPath;
                    }
                }

                // Extract native runtime targets (RID-specific native libraries).
                if (entry.Value.TryGetProperty("runtimeTargets", out var runtimeTargets))
                {
                    foreach (var asset in runtimeTargets.EnumerateObject())
                    {
                        // Only process native assets.
                        if (!asset.Value.TryGetProperty("assetType", out var assetType)
                            || assetType.GetString() != "native")
                        {
                            continue;
                        }

                        if (!asset.Value.TryGetProperty("rid", out var ridElement))
                        {
                            continue;
                        }

                        var rid = ridElement.GetString();
                        if (rid is null || !acceptableRids.Contains(rid))
                        {
                            continue;
                        }

                        var assetRelativePath = asset.Name;
                        var nativeFileName = Path.GetFileNameWithoutExtension(assetRelativePath);
                        var fullNativePath = Path.GetFullPath(
                            Path.Combine(nugetPackagesDir, packagePath, assetRelativePath));

                        // Only take the first (most specific) RID match per library name.
                        native.TryAdd(nativeFileName, fullNativePath);
                    }
                }
            }
        }
        catch (JsonException)
        {
            // Malformed deps.json — return whatever we've collected so far.
        }

        return (managed, native);
    }

    /// <summary>
    /// Returns a set of RIDs that are compatible with the given RID, ordered from
    /// most specific to least specific. For example, <c>win-x64</c> produces
    /// <c>{ "win-x64", "win" }</c>.
    /// </summary>
    private static HashSet<string> GetRidFallbacks(string rid)
    {
        var rids = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { rid };

        // Simple fallback: strip the architecture suffix (e.g. "win-x64" → "win").
        var dashIndex = rid.LastIndexOf('-');
        if (dashIndex > 0)
        {
            rids.Add(rid[..dashIndex]);
        }

        return rids;
    }
}
