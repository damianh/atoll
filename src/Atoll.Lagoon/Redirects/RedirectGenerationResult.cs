namespace Atoll.Lagoon.Redirects;

/// <summary>
/// Represents the outcome of a redirect rules generation pass.
/// </summary>
public sealed class RedirectGenerationResult
{
    /// <summary>
    /// Gets the number of redirect rules written to the <c>_redirects</c> file.
    /// </summary>
    public int RuleCount { get; }

    /// <summary>
    /// Gets the time taken to enumerate redirect rules and write the output file.
    /// </summary>
    public TimeSpan Elapsed { get; }

    /// <summary>
    /// Initializes a new instance with the given rule count and elapsed time.
    /// </summary>
    /// <param name="ruleCount">The number of redirect rules written.</param>
    /// <param name="elapsed">The time taken to generate the file.</param>
    public RedirectGenerationResult(int ruleCount, TimeSpan elapsed)
    {
        RuleCount = ruleCount;
        Elapsed = elapsed;
    }
}
