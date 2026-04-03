namespace Atoll.Build.Content.Frontmatter;

/// <summary>
/// The result of parsing a Markdown file with frontmatter.
/// Contains the raw YAML frontmatter string and the Markdown body.
/// </summary>
public sealed class FrontmatterParseResult
{
    /// <summary>
    /// Initializes a new instance of <see cref="FrontmatterParseResult"/>.
    /// </summary>
    /// <param name="rawFrontmatter">The raw YAML frontmatter string (without delimiters), or empty if none.</param>
    /// <param name="body">The Markdown body content (after the closing frontmatter delimiter).</param>
    public FrontmatterParseResult(string rawFrontmatter, string body)
    {
        ArgumentNullException.ThrowIfNull(rawFrontmatter);
        ArgumentNullException.ThrowIfNull(body);
        RawFrontmatter = rawFrontmatter;
        Body = body;
    }

    /// <summary>
    /// Gets the raw YAML frontmatter string (without the <c>---</c> delimiters).
    /// Empty if the file contained no frontmatter.
    /// </summary>
    public string RawFrontmatter { get; }

    /// <summary>
    /// Gets the Markdown body content (everything after the closing frontmatter delimiter).
    /// If no frontmatter was present, this is the entire file content.
    /// </summary>
    public string Body { get; }

    /// <summary>
    /// Gets a value indicating whether frontmatter was present in the parsed content.
    /// </summary>
    public bool HasFrontmatter => RawFrontmatter.Length > 0;
}

/// <summary>
/// Extracts YAML frontmatter from Markdown content. Frontmatter is delimited by
/// <c>---</c> on its own line at the start of the file and a closing <c>---</c>.
/// </summary>
/// <remarks>
/// <para>
/// This follows the standard frontmatter convention used by Astro, Jekyll, Hugo, and other
/// static site generators. The opening <c>---</c> must be the very first line of the file.
/// The closing <c>---</c> must appear on its own line.
/// </para>
/// </remarks>
public static class FrontmatterParser
{
    private const string Delimiter = "---";

    /// <summary>
    /// Parses the specified content, extracting frontmatter and body.
    /// </summary>
    /// <param name="content">The full file content to parse.</param>
    /// <returns>A <see cref="FrontmatterParseResult"/> containing the raw YAML and body.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="content"/> is <c>null</c>.</exception>
    public static FrontmatterParseResult Parse(string content)
    {
        ArgumentNullException.ThrowIfNull(content);

        if (content.Length == 0)
        {
            return new FrontmatterParseResult("", "");
        }

        var span = content.AsSpan();

        // The opening delimiter must be at the very start of the file
        if (!StartsWithDelimiter(span))
        {
            return new FrontmatterParseResult("", content);
        }

        // Find the end of the opening delimiter line
        var openingEnd = FindLineEnd(span, 0);

        // Search for the closing delimiter
        var searchStart = openingEnd;
        while (searchStart < span.Length)
        {
            var lineStart = searchStart;
            var lineEnd = FindLineEnd(span, lineStart);
            var line = span[lineStart..lineEnd].TrimEnd("\r\n".AsSpan());

            if (line.SequenceEqual(Delimiter.AsSpan()))
            {
                // Found closing delimiter
                var rawFrontmatter = content[openingEnd..lineStart].TrimEnd('\r', '\n');
                var bodyStart = lineEnd;
                var body = bodyStart < content.Length ? content[bodyStart..].TrimStart('\r', '\n') : "";
                return new FrontmatterParseResult(rawFrontmatter, body);
            }

            searchStart = lineEnd;
        }

        // No closing delimiter found — treat entire content as body (no frontmatter)
        return new FrontmatterParseResult("", content);
    }

    private static bool StartsWithDelimiter(ReadOnlySpan<char> span)
    {
        if (span.Length < Delimiter.Length)
        {
            return false;
        }

        if (!span.StartsWith(Delimiter.AsSpan()))
        {
            return false;
        }

        // Must be exactly "---" followed by end of line or end of content
        if (span.Length == Delimiter.Length)
        {
            return true;
        }

        var afterDelimiter = span[Delimiter.Length];
        return afterDelimiter == '\n' || afterDelimiter == '\r';
    }

    private static int FindLineEnd(ReadOnlySpan<char> span, int start)
    {
        for (var i = start; i < span.Length; i++)
        {
            if (span[i] == '\n')
            {
                return i + 1;
            }
        }

        return span.Length;
    }
}
