using System.Text.Json;

namespace Atoll.Build.Ssg;

/// <summary>
/// Atomically writes a <see cref="BuildCache"/> to disk.
/// </summary>
internal static class BuildCacheWriter
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = false,
    };

    /// <summary>
    /// Writes <paramref name="cache"/> to <paramref name="cachePath"/> atomically
    /// (write to a temporary file, then rename). Creates the parent directory if needed.
    /// </summary>
    public static async Task WriteAsync(BuildCache cache, string cachePath, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(cachePath);

        var directory = Path.GetDirectoryName(cachePath);
        if (directory is { Length: > 0 })
        {
            Directory.CreateDirectory(directory);
        }

        var tempPath = cachePath + ".tmp";
        try
        {
            var json = JsonSerializer.Serialize(cache, SerializerOptions);
            await File.WriteAllTextAsync(tempPath, json, cancellationToken);
            File.Move(tempPath, cachePath, overwrite: true);
        }
        catch
        {
            try { File.Delete(tempPath); } catch { /* ignore cleanup failure */ }
            throw;
        }
    }
}
