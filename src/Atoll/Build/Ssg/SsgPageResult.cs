namespace Atoll.Build.Ssg;

/// <summary>
/// Represents the result of rendering a single page during static site generation.
/// </summary>
public sealed class SsgPageResult
{
    /// <summary>
    /// Initializes a new <see cref="SsgPageResult"/> for a successfully rendered page.
    /// </summary>
    /// <param name="route">The SSG route that was rendered.</param>
    /// <param name="outputPath">The full file path where the HTML was written.</param>
    /// <param name="html">The rendered HTML content.</param>
    /// <param name="elapsed">The time taken to render the page.</param>
    public SsgPageResult(SsgRoute route, string outputPath, string html, TimeSpan elapsed)
    {
        ArgumentNullException.ThrowIfNull(route);
        ArgumentNullException.ThrowIfNull(outputPath);
        ArgumentNullException.ThrowIfNull(html);
        Route = route;
        OutputPath = outputPath;
        Html = html;
        Elapsed = elapsed;
        IsSuccess = true;
        IsSkipped = false;
    }

    /// <summary>
    /// Initializes a new <see cref="SsgPageResult"/> for a failed page render.
    /// </summary>
    /// <param name="route">The SSG route that failed.</param>
    /// <param name="error">The exception that occurred.</param>
    /// <param name="elapsed">The time taken before the error occurred.</param>
    public SsgPageResult(SsgRoute route, Exception error, TimeSpan elapsed)
    {
        ArgumentNullException.ThrowIfNull(route);
        ArgumentNullException.ThrowIfNull(error);
        Route = route;
        OutputPath = "";
        Html = "";
        Error = error;
        Elapsed = elapsed;
        IsSuccess = false;
        IsSkipped = false;
    }

    /// <summary>
    /// Initializes a new <see cref="SsgPageResult"/> for a skipped page (incremental build cache hit).
    /// The output file already exists from a previous build and the inputs have not changed.
    /// </summary>
    /// <param name="route">The SSG route that was skipped.</param>
    /// <param name="outputPath">The full file path of the existing HTML output.</param>
    /// <param name="elapsed">The time taken to determine the page should be skipped.</param>
    public SsgPageResult(SsgRoute route, string outputPath, TimeSpan elapsed)
    {
        ArgumentNullException.ThrowIfNull(route);
        ArgumentNullException.ThrowIfNull(outputPath);
        Route = route;
        OutputPath = outputPath;
        Html = "";
        Elapsed = elapsed;
        IsSuccess = true;
        IsSkipped = true;
    }

    /// <summary>
    /// Gets the SSG route that was rendered.
    /// </summary>
    public SsgRoute Route { get; }

    /// <summary>
    /// Gets the full file path where the HTML was written.
    /// Empty if the render failed.
    /// </summary>
    public string OutputPath { get; }

    /// <summary>
    /// Gets the rendered HTML content. Empty if the render failed or the page was skipped.
    /// </summary>
    public string Html { get; }

    /// <summary>
    /// Gets the time taken to render the page.
    /// </summary>
    public TimeSpan Elapsed { get; }

    /// <summary>
    /// Gets a value indicating whether the page was rendered successfully (or skipped due to caching).
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the page was skipped because the output file already exists
    /// and all inputs are unchanged since the last build.
    /// </summary>
    public bool IsSkipped { get; }

    /// <summary>
    /// Gets the exception that occurred during rendering, or <c>null</c> if the render succeeded.
    /// </summary>
    public Exception? Error { get; }
}
