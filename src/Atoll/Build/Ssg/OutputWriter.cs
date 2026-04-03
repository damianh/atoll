using System.Text;

namespace Atoll.Build.Ssg;

/// <summary>
/// Writes rendered HTML pages to the output directory, creating the appropriate
/// directory structure for clean URLs (e.g., <c>/about</c> becomes <c>about/index.html</c>).
/// </summary>
public sealed class OutputWriter
{
    private readonly string _outputDirectory;
    private readonly Encoding _encoding;

    /// <summary>
    /// Initializes a new <see cref="OutputWriter"/> with the specified output directory
    /// and UTF-8 encoding.
    /// </summary>
    /// <param name="outputDirectory">The root output directory.</param>
    public OutputWriter(string outputDirectory)
        : this(outputDirectory, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false))
    {
    }

    /// <summary>
    /// Initializes a new <see cref="OutputWriter"/> with the specified output directory
    /// and encoding.
    /// </summary>
    /// <param name="outputDirectory">The root output directory.</param>
    /// <param name="encoding">The encoding to use when writing files.</param>
    public OutputWriter(string outputDirectory, Encoding encoding)
    {
        ArgumentNullException.ThrowIfNull(outputDirectory);
        ArgumentNullException.ThrowIfNull(encoding);
        _outputDirectory = outputDirectory;
        _encoding = encoding;
    }

    /// <summary>
    /// Writes a rendered page to the output directory. Creates the file at the
    /// appropriate path for clean URLs.
    /// </summary>
    /// <param name="urlPath">The URL path (e.g., <c>/about</c> or <c>/blog/my-post</c>).</param>
    /// <param name="html">The rendered HTML content.</param>
    /// <returns>The full file path that was written.</returns>
    /// <remarks>
    /// <para>URL path mapping:</para>
    /// <list type="bullet">
    /// <item><description><c>/</c> → <c>index.html</c></description></item>
    /// <item><description><c>/about</c> → <c>about/index.html</c></description></item>
    /// <item><description><c>/blog/my-post</c> → <c>blog/my-post/index.html</c></description></item>
    /// </list>
    /// </remarks>
    public async Task<string> WritePageAsync(string urlPath, string html)
    {
        ArgumentNullException.ThrowIfNull(urlPath);
        ArgumentNullException.ThrowIfNull(html);

        var relativePath = UrlPathToFilePath(urlPath);
        var fullPath = Path.Combine(_outputDirectory, relativePath);

        var directory = Path.GetDirectoryName(fullPath);
        if (directory is not null)
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(fullPath, html, _encoding);
        return fullPath;
    }

    /// <summary>
    /// Writes raw content (non-HTML) to the specified relative path in the output directory.
    /// </summary>
    /// <param name="relativePath">The relative file path (e.g., <c>assets/style.css</c>).</param>
    /// <param name="content">The file content.</param>
    /// <returns>The full file path that was written.</returns>
    public async Task<string> WriteFileAsync(string relativePath, string content)
    {
        ArgumentNullException.ThrowIfNull(relativePath);
        ArgumentNullException.ThrowIfNull(content);

        var fullPath = Path.Combine(_outputDirectory, relativePath);

        var directory = Path.GetDirectoryName(fullPath);
        if (directory is not null)
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(fullPath, content, _encoding);
        return fullPath;
    }

    /// <summary>
    /// Writes raw binary content to the specified relative path in the output directory.
    /// </summary>
    /// <param name="relativePath">The relative file path.</param>
    /// <param name="content">The binary content.</param>
    /// <returns>The full file path that was written.</returns>
    public async Task<string> WriteBinaryFileAsync(string relativePath, byte[] content)
    {
        ArgumentNullException.ThrowIfNull(relativePath);
        ArgumentNullException.ThrowIfNull(content);

        var fullPath = Path.Combine(_outputDirectory, relativePath);

        var directory = Path.GetDirectoryName(fullPath);
        if (directory is not null)
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllBytesAsync(fullPath, content);
        return fullPath;
    }

    /// <summary>
    /// Cleans the output directory by deleting all contents.
    /// Creates the directory if it does not exist.
    /// </summary>
    public void Clean()
    {
        if (Directory.Exists(_outputDirectory))
        {
            Directory.Delete(_outputDirectory, recursive: true);
        }

        Directory.CreateDirectory(_outputDirectory);
    }

    /// <summary>
    /// Converts a URL path to a file system path for clean URLs.
    /// </summary>
    /// <param name="urlPath">The URL path.</param>
    /// <returns>The relative file path.</returns>
    internal static string UrlPathToFilePath(string urlPath)
    {
        var trimmed = urlPath.Trim('/');

        if (trimmed.Length == 0)
        {
            return "index.html";
        }

        // Replace forward slashes with platform-specific directory separators
        var segments = trimmed.Split('/');
        var combined = Path.Combine(segments);
        return Path.Combine(combined, "index.html");
    }
}
