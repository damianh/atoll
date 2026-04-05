namespace Atoll.Reef.Navigation;

/// <summary>
/// Calculates estimated reading time from a Markdown body based on average reading speed.
/// </summary>
public static class ReadingTimeCalculator
{
    /// <summary>Average words per minute for a typical reader.</summary>
    private const int WordsPerMinute = 200;

    /// <summary>
    /// Estimates the reading time in minutes for the given Markdown body text.
    /// </summary>
    /// <param name="markdownBody">The raw Markdown body text (excluding frontmatter).</param>
    /// <returns>
    /// The estimated reading time in minutes. Returns a minimum of 1 minute,
    /// even for very short content.
    /// </returns>
    public static int Calculate(string markdownBody)
    {
        if (string.IsNullOrWhiteSpace(markdownBody))
        {
            return 1;
        }

        var wordCount = markdownBody.Split(
            (char[]?)null,
            StringSplitOptions.RemoveEmptyEntries).Length;

        var minutes = (int)Math.Ceiling((double)wordCount / WordsPerMinute);
        return Math.Max(1, minutes);
    }
}
