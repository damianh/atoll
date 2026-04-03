namespace Atoll.Build.Ssg;

/// <summary>
/// Configuration options for static site generation.
/// </summary>
public sealed class SsgOptions
{
    /// <summary>
    /// Initializes a new <see cref="SsgOptions"/> with the specified output directory.
    /// </summary>
    /// <param name="outputDirectory">The absolute path to the output directory (e.g., <c>dist/</c>).</param>
    public SsgOptions(string outputDirectory)
    {
        ArgumentNullException.ThrowIfNull(outputDirectory);
        OutputDirectory = outputDirectory;
    }

    /// <summary>
    /// Gets the absolute path to the output directory where generated files will be written.
    /// </summary>
    public string OutputDirectory { get; }

    /// <summary>
    /// Gets or sets the base URL for the site (e.g., <c>https://example.com</c>).
    /// Used for generating canonical URLs in sitemaps and meta tags.
    /// </summary>
    public string BaseUrl { get; set; } = "";

    /// <summary>
    /// Gets or sets the base path prefix for the site (e.g., <c>/docs</c>).
    /// All generated URLs are relative to this path.
    /// </summary>
    public string BasePath { get; set; } = "";

    /// <summary>
    /// Gets or sets the maximum degree of parallelism for rendering pages.
    /// A value of 1 disables parallel rendering. A value of -1 uses the system default.
    /// </summary>
    public int MaxConcurrency { get; set; } = -1;

    /// <summary>
    /// Gets or sets a value indicating whether to clean the output directory before generating.
    /// </summary>
    public bool CleanOutputDirectory { get; set; } = true;
}
