namespace Atoll.Islands;

/// <summary>
/// Represents the result of an <see cref="IslandAssetWriter.WriteAsync"/> operation.
/// </summary>
public sealed class IslandAssetWriteResult
{
    /// <summary>
    /// Initializes a new <see cref="IslandAssetWriteResult"/>.
    /// </summary>
    /// <param name="fileCount">The number of files written.</param>
    /// <param name="writtenPaths">The absolute paths of all files written.</param>
    public IslandAssetWriteResult(int fileCount, IReadOnlyList<string> writtenPaths)
    {
        ArgumentNullException.ThrowIfNull(writtenPaths);
        FileCount = fileCount;
        WrittenPaths = writtenPaths;
    }

    /// <summary>
    /// Gets the number of island asset files written.
    /// </summary>
    public int FileCount { get; }

    /// <summary>
    /// Gets the absolute paths of all files written to the output directory.
    /// </summary>
    public IReadOnlyList<string> WrittenPaths { get; }
}

/// <summary>
/// Writes island asset files to the SSG output directory by reading them from
/// embedded resources described by <see cref="IslandAssetDescriptor"/> objects.
/// </summary>
public sealed class IslandAssetWriter
{
    private readonly string _outputDirectory;

    /// <summary>
    /// Initializes a new <see cref="IslandAssetWriter"/> targeting the specified output directory.
    /// </summary>
    /// <param name="outputDirectory">
    /// The root output directory. Asset output paths are resolved relative to this directory.
    /// </param>
    public IslandAssetWriter(string outputDirectory)
    {
        ArgumentNullException.ThrowIfNull(outputDirectory);
        _outputDirectory = outputDirectory;
    }

    /// <summary>
    /// Reads each asset's embedded resource and writes it to the resolved output path.
    /// Parent directories are created automatically.
    /// </summary>
    /// <param name="assets">The asset descriptors to write.</param>
    /// <returns>An <see cref="IslandAssetWriteResult"/> with the count and paths of written files.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when an embedded resource specified by a descriptor cannot be found.
    /// </exception>
    public async Task<IslandAssetWriteResult> WriteAsync(IEnumerable<IslandAssetDescriptor> assets)
    {
        ArgumentNullException.ThrowIfNull(assets);

        var writtenPaths = new List<string>();

        foreach (var descriptor in assets)
        {
            var stream = descriptor.ResourceAssembly.GetManifestResourceStream(descriptor.ResourceName);
            if (stream is null)
            {
                throw new InvalidOperationException(
                    $"Embedded resource '{descriptor.ResourceName}' was not found in assembly " +
                    $"'{descriptor.ResourceAssembly.GetName().Name}'. " +
                    $"Ensure the file is included as an EmbeddedResource in the project file.");
            }

            await using (stream)
            {
                var fullOutputPath = Path.Combine(_outputDirectory, descriptor.OutputPath);
                var parentDir = Path.GetDirectoryName(fullOutputPath);
                if (parentDir is not null && !Directory.Exists(parentDir))
                {
                    Directory.CreateDirectory(parentDir);
                }

                await using var fileStream = new FileStream(
                    fullOutputPath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 4096,
                    useAsync: true);

                await stream.CopyToAsync(fileStream);
                writtenPaths.Add(fullOutputPath);
            }
        }

        return new IslandAssetWriteResult(writtenPaths.Count, writtenPaths.AsReadOnly());
    }
}
