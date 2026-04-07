namespace Atoll.Build.Pipeline;

/// <summary>
/// Copies static assets from a source directory (e.g., <c>public/</c>) to the
/// output directory (e.g., <c>dist/</c>), preserving directory structure.
/// </summary>
/// <remarks>
/// <para>
/// Static assets are files that are served as-is without processing: images, fonts,
/// favicons, <c>robots.txt</c>, etc. They are copied to the root of the output
/// directory (not into a subdirectory).
/// </para>
/// </remarks>
public sealed class StaticAssetCopier
{
    private readonly string _outputDirectory;

    /// <summary>
    /// Initializes a new <see cref="StaticAssetCopier"/> with the specified output directory.
    /// </summary>
    /// <param name="outputDirectory">The root output directory to copy assets into.</param>
    public StaticAssetCopier(string outputDirectory)
    {
        ArgumentNullException.ThrowIfNull(outputDirectory);
        _outputDirectory = outputDirectory;
    }

    /// <summary>
    /// Copies all files from the source directory to the output directory,
    /// preserving the relative directory structure.
    /// </summary>
    /// <param name="sourceDirectory">The directory containing static assets (e.g., <c>public/</c>).</param>
    /// <returns>A <see cref="CopyResult"/> listing the files that were copied.</returns>
    public CopyResult Copy(string sourceDirectory)
    {
        ArgumentNullException.ThrowIfNull(sourceDirectory);

        if (!Directory.Exists(sourceDirectory))
        {
            return new CopyResult([]);
        }

        var copiedFiles = new List<CopiedFile>();
        CopyDirectory(sourceDirectory, _outputDirectory, sourceDirectory, copiedFiles);
        return new CopyResult(copiedFiles);
    }

    /// <summary>
    /// Copies all files from the source directory to the output directory asynchronously,
    /// preserving the relative directory structure.
    /// </summary>
    /// <param name="sourceDirectory">The directory containing static assets.</param>
    /// <param name="cancellationToken">A token to cancel the copy operation.</param>
    /// <returns>A <see cref="CopyResult"/> listing the files that were copied.</returns>
    public async Task<CopyResult> CopyAsync(string sourceDirectory, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(sourceDirectory);

        if (!Directory.Exists(sourceDirectory))
        {
            return new CopyResult([]);
        }

        var copiedFiles = new List<CopiedFile>();
        await CopyDirectoryAsync(sourceDirectory, _outputDirectory, sourceDirectory, copiedFiles, cancellationToken);
        return new CopyResult(copiedFiles);
    }

    private static void CopyDirectory(
        string sourceDir,
        string destDir,
        string rootSource,
        List<CopiedFile> copiedFiles)
    {
        Directory.CreateDirectory(destDir);

        // Copy files
        foreach (var filePath in Directory.GetFiles(sourceDir))
        {
            var fileName = Path.GetFileName(filePath);
            var destPath = Path.Combine(destDir, fileName);
            File.Copy(filePath, destPath, overwrite: true);

            var relativePath = Path.GetRelativePath(rootSource, filePath);
            copiedFiles.Add(new CopiedFile(relativePath, destPath, new FileInfo(filePath).Length));
        }

        // Recurse into subdirectories
        foreach (var dirPath in Directory.GetDirectories(sourceDir))
        {
            var dirName = Path.GetFileName(dirPath);
            CopyDirectory(dirPath, Path.Combine(destDir, dirName), rootSource, copiedFiles);
        }
    }

    private static async Task CopyDirectoryAsync(
        string sourceDir,
        string destDir,
        string rootSource,
        List<CopiedFile> copiedFiles,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(destDir);

        // Copy files
        foreach (var filePath in Directory.GetFiles(sourceDir))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var fileName = Path.GetFileName(filePath);
            var destPath = Path.Combine(destDir, fileName);

            await using var sourceStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
            await using var destStream = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);
            await sourceStream.CopyToAsync(destStream, cancellationToken);

            var relativePath = Path.GetRelativePath(rootSource, filePath);
            copiedFiles.Add(new CopiedFile(relativePath, destPath, new FileInfo(filePath).Length));
        }

        // Recurse into subdirectories
        foreach (var dirPath in Directory.GetDirectories(sourceDir))
        {
            var dirName = Path.GetFileName(dirPath);
            await CopyDirectoryAsync(dirPath, Path.Combine(destDir, dirName), rootSource, copiedFiles, cancellationToken);
        }
    }
}

/// <summary>
/// Represents the result of copying static assets.
/// </summary>
public sealed class CopyResult
{
    /// <summary>
    /// Initializes a new <see cref="CopyResult"/>.
    /// </summary>
    /// <param name="files">The list of copied files.</param>
    public CopyResult(IReadOnlyList<CopiedFile> files)
    {
        ArgumentNullException.ThrowIfNull(files);
        Files = files;
    }

    /// <summary>
    /// Gets the list of files that were copied.
    /// </summary>
    public IReadOnlyList<CopiedFile> Files { get; }

    /// <summary>
    /// Gets the total number of files copied.
    /// </summary>
    public int Count => Files.Count;

    /// <summary>
    /// Gets the total size in bytes of all copied files.
    /// </summary>
    public long TotalSize => Files.Sum(f => f.Size);
}

/// <summary>
/// Represents a single file that was copied during the static asset copy.
/// </summary>
/// <param name="RelativePath">The relative path from the source directory.</param>
/// <param name="DestinationPath">The full destination path.</param>
/// <param name="Size">The file size in bytes.</param>
public sealed record CopiedFile(string RelativePath, string DestinationPath, long Size);
