using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Atoll.Swell.Markdown;

/// <summary>
/// Parses a Swell Markdown file into a <see cref="SlideDeck"/>.
/// </summary>
/// <remarks>
/// <para>
/// A Swell Markdown file has the following structure:
/// </para>
/// <list type="bullet">
///   <item>
///     An optional YAML headmatter block at the very top (delimited by <c>---</c>) that
///     configures the entire deck (<see cref="DeckConfig"/>).
///   </item>
///   <item>
///     One or more slide chunks separated by <c>\n\n---\n\n</c> (a <c>---</c> line
///     preceded and followed by a blank line). A bare <c>---</c> without surrounding blank
///     lines is treated as a Markdown horizontal rule.
///   </item>
///   <item>
///     Each slide chunk may begin with its own YAML frontmatter block
///     (<see cref="SlideConfig"/>) and end with HTML comment blocks
///     (<c>&lt;!-- … --&gt;</c>) that are extracted as presenter notes.
///   </item>
/// </list>
/// </remarks>
public static class SlideParser
{
    // Matches the slide-separator: a blank line, then "---", then a blank line.
    // Uses \r?\n to handle both CRLF and LF line endings.
    private static readonly Regex SlideSeparatorPattern =
        new(@"\r?\n\r?\n---\r?\n\r?\n", RegexOptions.Compiled, TimeSpan.FromSeconds(5));

    // Matches one or more HTML comment blocks (<!-- ... -->) at the END of slide content.
    // Capture group 1 is the concatenated comment content.
    // Uses [\s\S] to match across newlines; non-greedy to avoid over-capture.
    private static readonly Regex TrailingNotesPattern =
        new(@"(\s*<!--[\s\S]*?-->)+\s*$", RegexOptions.Compiled, TimeSpan.FromSeconds(5));

    // Matches a single HTML comment block for content extraction.
    private static readonly Regex HtmlCommentContentPattern =
        new(@"<!--([\s\S]*?)-->", RegexOptions.Compiled, TimeSpan.FromSeconds(5));

    private static readonly IDeserializer YamlDeserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .WithTypeConverter(new AspectRatioYamlConverter())
        .WithTypeConverter(new TransitionTypeYamlConverter())
        .Build();

    /// <summary>
    /// Parses the specified Swell Markdown content into a <see cref="SlideDeck"/>.
    /// </summary>
    /// <param name="content">The full content of the <c>.md</c> file.</param>
    /// <returns>A <see cref="SlideDeck"/> containing deck config and slides.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="content"/> is <c>null</c>.</exception>
    public static SlideDeck Parse(string content)
    {
        ArgumentNullException.ThrowIfNull(content);

        var (deckConfig, body) = ExtractHeadmatter(content);
        var slideChunks = SplitIntoChunks(body);
        var slides = BuildSlides(slideChunks);

        return new SlideDeck(deckConfig, slides);
    }

    private static (DeckConfig Config, string Body) ExtractHeadmatter(string content)
    {
        var normalized = content.ReplaceLineEndings("\n").TrimStart('\n');

        // Headmatter is a YAML block at the very start: ---\n...\n---\n
        if (!normalized.StartsWith("---\n", StringComparison.Ordinal))
        {
            return (new DeckConfig(), normalized);
        }

        // Find the closing ---
        var closingIndex = normalized.IndexOf("\n---\n", 4, StringComparison.Ordinal);
        if (closingIndex < 0)
        {
            // No closing delimiter — no headmatter
            return (new DeckConfig(), normalized);
        }

        var rawYaml = normalized[4..closingIndex];
        var remainingBody = normalized[(closingIndex + 5)..].TrimStart('\n');

        var deckConfig = ParseYaml<DeckConfig>(rawYaml);
        return (deckConfig, remainingBody);
    }

    private static IReadOnlyList<string> SplitIntoChunks(string body)
    {
        var chunks = SlideSeparatorPattern.Split(body);
        var result = new List<string>(chunks.Length);

        foreach (var chunk in chunks)
        {
            var trimmed = chunk.Trim('\n', '\r');
            if (trimmed.Length > 0)
            {
                result.Add(trimmed);
            }
        }

        // If no separator was found, treat the whole body as a single slide.
        if (result.Count == 0 && body.Trim().Length > 0)
        {
            result.Add(body.Trim());
        }

        return result;
    }

    private static IReadOnlyList<SlideData> BuildSlides(IReadOnlyList<string> chunks)
    {
        var slides = new List<SlideData>(chunks.Count);

        for (var i = 0; i < chunks.Count; i++)
        {
            var chunk = chunks[i];
            var (slideConfig, markdown) = ExtractSlideFrontmatter(chunk);
            var (notes, body) = ExtractPresenterNotes(markdown);
            slides.Add(new SlideData(i, slideConfig, body, notes));
        }

        return slides;
    }

    private static (SlideConfig Config, string Markdown) ExtractSlideFrontmatter(string chunk)
    {
        if (!chunk.StartsWith("---\n", StringComparison.Ordinal))
        {
            return (new SlideConfig(), chunk);
        }

        var closingIndex = chunk.IndexOf("\n---\n", 4, StringComparison.Ordinal);
        if (closingIndex < 0)
        {
            // Try closing at end of string: ---\n...\n---
            if (chunk.EndsWith("\n---", StringComparison.Ordinal))
            {
                closingIndex = chunk.LastIndexOf("\n---", StringComparison.Ordinal);
                var rawYaml = chunk[4..closingIndex];
                var config = ParseYaml<SlideConfig>(rawYaml);
                return (config, "");
            }

            return (new SlideConfig(), chunk);
        }

        var yaml = chunk[4..closingIndex];
        var body = chunk[(closingIndex + 5)..].TrimStart('\n');
        return (ParseYaml<SlideConfig>(yaml), body);
    }

    private static (string Notes, string Body) ExtractPresenterNotes(string markdown)
    {
        var match = TrailingNotesPattern.Match(markdown);
        if (!match.Success)
        {
            return ("", markdown);
        }

        var notesSection = match.Value;
        var body = markdown[..match.Index].TrimEnd();

        // Extract text content from all comment blocks.
        var commentMatches = HtmlCommentContentPattern.Matches(notesSection);
        var notesParts = new List<string>(commentMatches.Count);
        foreach (System.Text.RegularExpressions.Match cm in commentMatches)
        {
            var text = cm.Groups[1].Value.Trim();
            if (text.Length > 0)
            {
                notesParts.Add(text);
            }
        }

        return (string.Join("\n\n", notesParts), body);
    }

    private static T ParseYaml<T>(string yaml) where T : new()
    {
        if (string.IsNullOrWhiteSpace(yaml))
        {
            return new T();
        }

        try
        {
            return YamlDeserializer.Deserialize<T>(yaml) ?? new T();
        }
        catch (YamlDotNet.Core.YamlException)
        {
            return new T();
        }
    }
}
