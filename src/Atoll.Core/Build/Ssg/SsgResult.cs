namespace Atoll.Build.Ssg;

/// <summary>
/// Represents the result of a complete static site generation run.
/// </summary>
public sealed class SsgResult
{
    /// <summary>
    /// Initializes a new <see cref="SsgResult"/> with the specified page results and total elapsed time.
    /// </summary>
    /// <param name="pageResults">The individual page render results.</param>
    /// <param name="totalElapsed">The total time for the entire SSG run.</param>
    public SsgResult(IReadOnlyList<SsgPageResult> pageResults, TimeSpan totalElapsed)
    {
        ArgumentNullException.ThrowIfNull(pageResults);
        PageResults = pageResults;
        TotalElapsed = totalElapsed;
    }

    /// <summary>
    /// Gets the individual page render results.
    /// </summary>
    public IReadOnlyList<SsgPageResult> PageResults { get; }

    /// <summary>
    /// Gets the total time for the entire SSG run.
    /// </summary>
    public TimeSpan TotalElapsed { get; }

    /// <summary>
    /// Gets the number of pages that were successfully rendered.
    /// </summary>
    public int SuccessCount => PageResults.Count(r => r.IsSuccess);

    /// <summary>
    /// Gets the number of pages that failed to render.
    /// </summary>
    public int FailureCount => PageResults.Count(r => !r.IsSuccess);

    /// <summary>
    /// Gets the total number of pages that were processed.
    /// </summary>
    public int TotalCount => PageResults.Count;

    /// <summary>
    /// Gets a value indicating whether all pages were rendered successfully.
    /// </summary>
    public bool IsSuccess => PageResults.All(r => r.IsSuccess);

    /// <summary>
    /// Gets the page results that failed.
    /// </summary>
    public IReadOnlyList<SsgPageResult> Failures => PageResults.Where(r => !r.IsSuccess).ToList();
}
