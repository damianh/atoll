using System.Text;
using Atoll.Build.Pipeline;
using Atoll.Components;
using Atoll.Routing;

namespace Atoll.Build.Ssg;

/// <summary>
/// Computes input hashes used for incremental build cache comparisons.
/// Reuses <see cref="AssetFingerprinter"/> for SHA-256 computation.
/// </summary>
internal static class InputHasher
{
    private const int HashLength = 16;

    /// <summary>
    /// Computes a hash of the assembly DLL file bytes.
    /// Returns an empty string if the file does not exist or cannot be read.
    /// </summary>
    public static string HashAssembly(string assemblyPath)
    {
        ArgumentNullException.ThrowIfNull(assemblyPath);

        if (!File.Exists(assemblyPath))
        {
            return "";
        }

        try
        {
            var bytes = File.ReadAllBytes(assemblyPath);
            return AssetFingerprinter.ComputeHash(bytes, HashLength);
        }
        catch
        {
            return "";
        }
    }

    /// <summary>
    /// Computes a coarse-grained hash of all files matching <paramref name="searchPattern"/>
    /// under <paramref name="directoryPath"/>. Returns an empty string if the directory does
    /// not exist or contains no matching files.
    /// </summary>
    /// <remarks>
    /// Uses file paths, sizes, and last-write timestamps rather than file content
    /// for speed. Any content change will cause a mtime change on most filesystems.
    /// </remarks>
    public static string HashDirectory(string directoryPath, string searchPattern = "*.md")
    {
        ArgumentNullException.ThrowIfNull(directoryPath);
        ArgumentNullException.ThrowIfNull(searchPattern);

        if (!Directory.Exists(directoryPath))
        {
            return "";
        }

        var files = Directory.GetFiles(directoryPath, searchPattern, SearchOption.AllDirectories);
        if (files.Length == 0)
        {
            return "";
        }

        Array.Sort(files, StringComparer.OrdinalIgnoreCase);

        var sb = new StringBuilder();
        foreach (var file in files)
        {
            var info = new FileInfo(file);
            sb.Append(file);
            sb.Append('|');
            sb.Append(info.Length);
            sb.Append('|');
            sb.Append(info.LastWriteTimeUtc.Ticks);
            sb.Append('\n');
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return AssetFingerprinter.ComputeHash(bytes, HashLength);
    }

    /// <summary>
    /// Computes a hash of the layout chain for <paramref name="componentType"/>.
    /// The hash covers the full names of all types in the <c>[Layout]</c> chain.
    /// Changes to the layout chain (adding, removing, or renaming layout types)
    /// will produce a different hash.
    /// </summary>
    public static string HashLayoutChain(Type componentType)
    {
        ArgumentNullException.ThrowIfNull(componentType);

        var chain = new StringBuilder();
        chain.Append(componentType.FullName ?? componentType.Name);

        var current = componentType;
        while (true)
        {
            var attr = current.GetCustomAttributes(inherit: false)
                .OfType<LayoutAttribute>()
                .FirstOrDefault();

            if (attr is null)
            {
                break;
            }

            chain.Append('|');
            chain.Append(attr.LayoutType.FullName ?? attr.LayoutType.Name);
            current = attr.LayoutType;
        }

        var bytes = Encoding.UTF8.GetBytes(chain.ToString());
        return AssetFingerprinter.ComputeHash(bytes, HashLength);
    }

    /// <summary>
    /// Determines whether a route component type is dynamic — i.e., it implements
    /// <see cref="IStaticPathsProvider"/> and therefore may return different pages
    /// depending on current content.
    /// </summary>
    public static bool IsDynamicRoute(Type componentType)
    {
        ArgumentNullException.ThrowIfNull(componentType);
        return typeof(IStaticPathsProvider).IsAssignableFrom(componentType);
    }
}
