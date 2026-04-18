using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Atoll.Build.Ssg;

/// <summary>
/// Reads a <see cref="BuildCache"/> from disk, returning <c>null</c> when the
/// cache is missing, corrupt, or incompatible with the current Atoll version.
/// </summary>
internal static class BuildCacheReader
{
    /// <summary>
    /// Returns the path to the build cache file for the given project root and output directory.
    /// A truncated hash of the output directory path is embedded in the filename to prevent
    /// collisions between multiple Atoll sites sharing the same project root (e.g. monorepos)
    /// or builds with different <c>--output</c> directories.
    /// </summary>
    public static string GetCachePath(string projectRoot, string outputDir)
    {
        ArgumentNullException.ThrowIfNull(projectRoot);
        ArgumentNullException.ThrowIfNull(outputDir);

        var normalizedOutputDir = Path.GetFullPath(outputDir);
        var bytes = Encoding.UTF8.GetBytes(normalizedOutputDir);
        var hash = SHA256.HashData(bytes);
        var shortHash = Convert.ToHexStringLower(hash)[..8];
        return Path.Combine(projectRoot, ".atoll", $"build-cache-{shortHash}.json");
    }

    /// <summary>
    /// Attempts to load the build cache from <paramref name="cachePath"/>.
    /// Returns <c>null</c> if the file does not exist, cannot be parsed,
    /// or has an incompatible <see cref="BuildCache.CacheVersion"/> or Atoll version.
    /// </summary>
    public static BuildCache? TryLoad(string cachePath, string currentAtollVersion)
    {
        ArgumentNullException.ThrowIfNull(cachePath);
        ArgumentNullException.ThrowIfNull(currentAtollVersion);

        if (!File.Exists(cachePath))
        {
            return null;
        }

        try
        {
            var json = File.ReadAllText(cachePath);
            var cache = JsonSerializer.Deserialize<BuildCache>(json);

            if (cache is null)
            {
                return null;
            }

            if (cache.CacheVersion != BuildCache.CurrentCacheVersion)
            {
                return null;
            }

            if (cache.AtollVersion != currentAtollVersion)
            {
                return null;
            }

            return cache;
        }
        catch
        {
            // Corrupt or unreadable cache — treat as a cache miss.
            return null;
        }
    }
}
