using Markdig;
using System.Text;
using System.Text.RegularExpressions;

namespace Atoll.Build.Content.Markdown;

/// <summary>
/// Pre-processes raw Markdown content before Markdig parsing, converting
/// <c>&lt;PascalCaseName Prop="value"&gt;markdown children&lt;/PascalCaseName&gt;</c>
/// and self-closing <c>&lt;PascalCaseName Prop="value" /&gt;</c> tags into
/// <c>&lt;!--atoll-tag:N--&gt;</c> placeholders with collected <see cref="ComponentReference"/> data.
/// </summary>
/// <remarks>
/// <para>
/// This pre-processor runs <em>before</em> Markdig because Markdig treats
/// <c>&lt;PascalCaseName&gt;</c> as raw HTML blocks — meaning inner content would not be
/// processed as Markdown. By replacing the tags with placeholders before parsing,
/// the inner Markdown is rendered correctly and component references are captured.
/// </para>
/// <para>
/// Only tag names registered in the <see cref="ComponentMap"/> (via their explicit name
/// or PascalCase type-name alias) are intercepted. Unregistered PascalCase tags and all
/// lowercase HTML tags pass through unchanged.
/// </para>
/// <para>
/// The emitted placeholder prefix is <c>atoll-tag</c> (i.e. <c>&lt;!--atoll-tag:N--&gt;</c>),
/// distinct from the <c>atoll</c> prefix used by the <c>:::</c> directive extension.
/// <see cref="MarkdownRenderer"/> merges and renumbers both placeholder sequences after rendering.
/// </para>
/// <para>
/// Known limitation: tags inside indented code blocks (4-space indent) are not detected
/// as code and will be intercepted if the tag name is registered. This is a prototype
/// limitation; only fenced code blocks (<c>```</c> or <c>~~~</c>) are excluded.
/// </para>
/// </remarks>
internal sealed class ComponentTagPreprocessor
{
    // Matches a PascalCase opening or self-closing tag: <CardGrid ...> or <Card ... />
    // Group 1: tag name (starts with uppercase, ≥2 chars)
    // Group 2: attribute string (everything between tag name and > or />)
    // Group 3: "/" if self-closing
    private static readonly Regex TagOpeningPattern = new(
        @"<([A-Z][A-Za-z0-9]+)((?:\s[^>]*?)?)(\s*/)?>",
        RegexOptions.Compiled | RegexOptions.Singleline,
        TimeSpan.FromSeconds(5));

    // Matches a PascalCase attribute key=value pair.
    // Group 1: attribute name
    // Group 2: double-quoted value
    // Group 3: single-quoted value
    // Group 4: unquoted value (stops at whitespace, > or /)
    // Group 5: boolean attribute (name only, no =)
    private static readonly Regex AttributePattern = new(
        @"([A-Za-z][A-Za-z0-9\-]*)(?:\s*=\s*(?:""([^""]*)""|'([^']*)'|([^\s>""'/]+)))?",
        RegexOptions.Compiled,
        TimeSpan.FromSeconds(5));

    private readonly ComponentMap _componentMap;
    private readonly MarkdownOptions _options;

    /// <summary>
    /// Initializes a new <see cref="ComponentTagPreprocessor"/>.
    /// </summary>
    /// <param name="componentMap">The registry used to look up component types by tag name.</param>
    /// <param name="options">
    /// The Markdown options used when rendering inner content (child Markdown within a tag).
    /// The component directive extension is intentionally excluded from child rendering.
    /// </param>
    internal ComponentTagPreprocessor(ComponentMap componentMap, MarkdownOptions options)
    {
        _componentMap = componentMap;
        _options = options;
    }

    /// <summary>
    /// Processes the given Markdown string, replacing PascalCase component tags with
    /// <c>&lt;!--atoll-tag:N--&gt;</c> placeholders and collecting component references.
    /// </summary>
    /// <param name="markdown">The raw Markdown content to process.</param>
    /// <returns>
    /// A tuple of the processed Markdown string (with component tags replaced by placeholders)
    /// and the ordered list of collected <see cref="ComponentReference"/> entries.
    /// </returns>
    internal (string Markdown, IReadOnlyList<ComponentReference> References) Process(string markdown)
    {
        var references = new List<ComponentReference>();
        var processed = ProcessCore(markdown, references);
        return (processed, references);
    }

    /// <summary>
    /// Core recursive processing method that scans <paramref name="markdown"/> for component
    /// tags and replaces them with placeholders, appending references to the shared
    /// <paramref name="references"/> accumulator.
    /// </summary>
    private string ProcessCore(string markdown, List<ComponentReference> references)
    {
        if (string.IsNullOrEmpty(markdown))
        {
            return markdown;
        }

        // Build the set of character ranges that are inside fenced code blocks.
        // Tags in these regions must be skipped.
        var codeBlockRanges = FindFencedCodeBlockRanges(markdown);

        var result = new StringBuilder(markdown.Length);
        var pos = 0;

        while (pos < markdown.Length)
        {
            var match = TagOpeningPattern.Match(markdown, pos);
            if (!match.Success)
            {
                // No more tags — append the rest verbatim.
                result.Append(markdown, pos, markdown.Length - pos);
                break;
            }

            var matchStart = match.Index;

            // Skip tags that are inside a fenced code block.
            if (IsInsideCodeBlock(matchStart, codeBlockRanges))
            {
                // Advance past the match start character (to avoid infinite loop) and keep scanning.
                result.Append(markdown, pos, matchStart - pos + 1);
                pos = matchStart + 1;
                continue;
            }

            var tagName = match.Groups[1].Value;
            var attrString = match.Groups[2].Value;
            var isSelfClosing = match.Groups[3].Success && match.Groups[3].Value.Contains('/');

            // Only intercept tags registered in the component map.
            if (!_componentMap.TryResolve(tagName, out var componentType) || componentType is null)
            {
                // Not a registered component — append up to and including this tag verbatim.
                result.Append(markdown, pos, match.Index - pos + match.Length);
                pos = match.Index + match.Length;
                continue;
            }

            // Append content before this tag.
            result.Append(markdown, pos, matchStart - pos);

            var props = ParseAttributes(attrString);
            string? childHtml = null;

            if (isSelfClosing)
            {
                // Self-closing: no children.
                pos = match.Index + match.Length;
            }
            else
            {
                // Find the matching closing tag, accounting for nested same-name tags.
                var contentStart = match.Index + match.Length;
                var closingTagIndex = FindClosingTag(markdown, tagName, contentStart, codeBlockRanges);

                if (closingTagIndex < 0)
                {
                    // No matching closing tag found — treat as unrecognised, append verbatim.
                    result.Append(markdown, matchStart, match.Length);
                    pos = matchStart + match.Length;
                    continue;
                }

                var closingTag = $"</{tagName}>";
                var innerContent = markdown.Substring(contentStart, closingTagIndex - contentStart);

                // Recursively process inner content first (inside-out, for nested tags).
                var processedInner = ProcessCore(innerContent, references);

                // Render the processed inner content as Markdown.
                childHtml = RenderChildMarkdown(processedInner);
                if (string.IsNullOrEmpty(childHtml))
                {
                    childHtml = null;
                }

                pos = closingTagIndex + closingTag.Length;
            }

            var index = references.Count;
            references.Add(new ComponentReference(componentType, props, childHtml));
            result.Append($"<!--atoll-tag:{index}-->");
        }

        return result.ToString();
    }

    /// <summary>
    /// Finds the position of the closing <c>&lt;/TagName&gt;</c> tag that matches the opening tag,
    /// handling nesting of the same tag name.
    /// </summary>
    /// <returns>The start index of the closing tag, or -1 if not found.</returns>
    private static int FindClosingTag(
        string markdown,
        string tagName,
        int searchFrom,
        IReadOnlyList<(int Start, int End)> codeBlockRanges)
    {
        var openingTag = $"<{tagName}";
        var closingTag = $"</{tagName}>";
        var depth = 1; // We've already consumed one opening tag.
        var pos = searchFrom;

        while (pos < markdown.Length)
        {
            var nextOpen = markdown.IndexOf(openingTag, pos, StringComparison.OrdinalIgnoreCase);
            var nextClose = markdown.IndexOf(closingTag, pos, StringComparison.OrdinalIgnoreCase);

            if (nextClose < 0)
            {
                // No closing tag found.
                return -1;
            }

            if (nextOpen >= 0 && nextOpen < nextClose)
            {
                // Opening comes before closing.
                if (!IsInsideCodeBlock(nextOpen, codeBlockRanges))
                {
                    // Verify it's a proper opening tag (next char after tag name is space, > or /).
                    var charAfter = nextOpen + openingTag.Length < markdown.Length
                        ? markdown[nextOpen + openingTag.Length]
                        : '\0';

                    if (charAfter == '>' || charAfter == '/' || char.IsWhiteSpace(charAfter))
                    {
                        depth++;
                    }
                }

                pos = nextOpen + openingTag.Length;
            }
            else
            {
                // Closing comes next.
                if (!IsInsideCodeBlock(nextClose, codeBlockRanges))
                {
                    depth--;
                    if (depth == 0)
                    {
                        return nextClose;
                    }
                }

                pos = nextClose + closingTag.Length;
            }
        }

        return -1;
    }

    /// <summary>
    /// Parses HTML-style attributes from an attribute string.
    /// Supports double-quoted, single-quoted, unquoted, and boolean attributes.
    /// </summary>
    private static IReadOnlyDictionary<string, object?> ParseAttributes(string attrString)
    {
        if (string.IsNullOrWhiteSpace(attrString))
        {
            return EmptyProps;
        }

        var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        foreach (Match m in AttributePattern.Matches(attrString))
        {
            var key = m.Groups[1].Value;

            string value;
            if (m.Groups[2].Success)
            {
                value = m.Groups[2].Value; // double-quoted
            }
            else if (m.Groups[3].Success)
            {
                value = m.Groups[3].Value; // single-quoted
            }
            else if (m.Groups[4].Success)
            {
                value = m.Groups[4].Value; // unquoted
            }
            else
            {
                value = "true"; // boolean attribute (key only)
            }

            dict[key] = value;
        }

        return dict;
    }

    /// <summary>
    /// Renders the given (already-preprocessed) inner Markdown string to HTML.
    /// Uses the caller's <see cref="MarkdownOptions"/> without the component directive extension,
    /// so inner Markdown gets the same table/autolink/etc. support as the outer document.
    /// </summary>
    private string RenderChildMarkdown(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return string.Empty;
        }

        // Build a pipeline matching the caller's options but without component directives.
        // (MarkdownRenderer.BuildPipeline is the public overload that excludes the directive extension.)
        var pipeline = MarkdownRenderer.BuildPipeline(_options);
        var document = Markdig.Markdown.Parse(markdown, pipeline);
        return document.ToHtml(pipeline);
    }

    /// <summary>
    /// Finds all fenced code block character ranges in the Markdown string.
    /// Ranges are [start, end] inclusive character positions.
    /// </summary>
    private static List<(int Start, int End)> FindFencedCodeBlockRanges(string markdown)
    {
        var ranges = new List<(int Start, int End)>();
        var lines = markdown.Split('\n');
        var pos = 0;
        string? fenceChar = null; // null = not in a fence, "```" or "~~~" = current fence
        int? fenceStart = null;
        var minFenceLength = 3;
        var currentFenceLength = 0;

        foreach (var line in lines)
        {
            var trimmed = line.TrimStart();
            var isFenceStart = trimmed.StartsWith("```", StringComparison.Ordinal)
                || trimmed.StartsWith("~~~", StringComparison.Ordinal);

            if (isFenceStart)
            {
                var fc = trimmed.StartsWith("```", StringComparison.Ordinal) ? "`" : "~";
                var len = 0;
                for (var i = 0; i < trimmed.Length && trimmed[i].ToString() == fc; i++)
                {
                    len++;
                }

                if (fenceChar is null)
                {
                    // Opening a new fence.
                    fenceChar = fc;
                    fenceStart = pos;
                    currentFenceLength = len;
                }
                else if (fc == fenceChar && len >= currentFenceLength)
                {
                    // Closing the fence (same char, at least as many characters).
                    var end = pos + line.Length;
                    ranges.Add((fenceStart!.Value, end));
                    fenceChar = null;
                    fenceStart = null;
                    currentFenceLength = minFenceLength;
                }
                // else: different fence char or length — treat as content inside the outer fence
            }

            pos += line.Length + 1; // +1 for the '\n' that was split on
        }

        // Unclosed fence — treat rest of document as code block.
        if (fenceStart.HasValue)
        {
            ranges.Add((fenceStart.Value, markdown.Length));
        }

        return ranges;
    }

    /// <summary>
    /// Returns <c>true</c> if <paramref name="position"/> falls within any of the
    /// fenced code block ranges.
    /// </summary>
    private static bool IsInsideCodeBlock(
        int position,
        IReadOnlyList<(int Start, int End)> codeBlockRanges)
    {
        foreach (var (start, end) in codeBlockRanges)
        {
            if (position >= start && position <= end)
            {
                return true;
            }
        }

        return false;
    }

    private static readonly IReadOnlyDictionary<string, object?> EmptyProps =
        new Dictionary<string, object?>();
}
