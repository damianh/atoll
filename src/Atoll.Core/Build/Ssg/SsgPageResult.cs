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
    /// Gets the rendered HTML content. Empty if the render failed.
    /// </summary>
    public string Html { get; }

    /// <summary>
    /// Gets the time taken to render the page.
    /// </summary>
    public TimeSpan Elapsed { get; }

    /// <summary>
    /// Gets a value indicating whether the page was rendered successfully.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets the exception that occurred during rendering, or <c>null</c> if the render succeeded.
    /// </summary>
    public Exception? Error { get; }
}
