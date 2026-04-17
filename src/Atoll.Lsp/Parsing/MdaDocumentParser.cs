using System.Text.RegularExpressions;
using Atoll.Build.Content.Frontmatter;
using Atoll.Lsp.Documents;
using OmniSharp.Extensions.LanguageServer.Protocol;
using LspRange = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace Atoll.Lsp.Parsing;

/// <summary>
/// Parses an MDA document into structured model without running the full Markdig pipeline.
/// Uses <see cref="FrontmatterParser"/> for frontmatter and regex for directive/tag detection.
/// </summary>
internal static class MdaDocumentParser
{
    // Matches :::name or :::name{props} at the start of a line.
    // Group 1: name, Group 2: props string inside {} (may be empty/absent)
    private static readonly Regex DirectiveOpenPattern =
        new(@"^:::([a-zA-Z][a-zA-Z0-9-]*)(?:\{([^}]*)\})?\s*$",
            RegexOptions.Multiline | RegexOptions.Compiled);

    // Matches the closing ::: on its own line
    private static readonly Regex DirectiveClosePattern =
        new(@"^:::\s*$", RegexOptions.Multiline | RegexOptions.Compiled);

    // Matches <PascalCase ...> or <PascalCase ... /> opening tags.
    // Group 1: tag name, Group 2: attribute string, Group 3: "/" if self-closing
    private static readonly Regex TagOpenPattern =
        new(@"<([A-Z][A-Za-z0-9]+)((?:\s[^>]*?)?)(\s*/)?>",
            RegexOptions.Compiled);

    // Matches individual attributes: Key="val", Key='val', Key=val, or boolean Key
    private static readonly Regex AttributePattern =
        new(@"([A-Za-z][A-Za-z0-9\-]*)(?:\s*=\s*(?:""([^""]*)""|'([^']*)'|([^\s>""'/]+)))?",
            RegexOptions.Compiled);

    // Matches fenced code blocks (``` or ~~~)
    private static readonly Regex FencedCodePattern =
        new(@"^(`{3,}|~{3,})[^\n]*\n.*?\n\1\s*$",
            RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);

    // Matches Markdown headings
    private static readonly Regex HeadingPattern =
        new(@"^(#{1,6})\s+(.+)$", RegexOptions.Multiline | RegexOptions.Compiled);

    internal static MdaDocument Parse(DocumentUri uri, string content, int version)
    {
        var lineMap = new LineMap(content);
        var parseResult = FrontmatterParser.Parse(content);

        FrontmatterRegion? frontmatter = null;
        int bodyOffset = 0;

        // FrontmatterParser.HasFrontmatter is false for empty frontmatter (---\n---),
        // but the LSP needs to treat that as "frontmatter present but empty" so that
        // missing-required-field diagnostics fire. Detect delimiters directly.
        var hasFrontmatterDelimiters = parseResult.HasFrontmatter || HasEmptyFrontmatter(content);

        if (hasFrontmatterDelimiters)
        {
            // Frontmatter starts at offset 0 (---) and ends just before body
            // Body begins after the closing --- line
            bodyOffset = content.Length - parseResult.Body.Length;

            var fmEndOffset = bodyOffset;
            var fmStartPos = lineMap.OffsetToPosition(0);
            var fmEndPos = lineMap.OffsetToPosition(fmEndOffset);
            frontmatter = new FrontmatterRegion(
                parseResult.RawFrontmatter,
                new LspRange(fmStartPos, fmEndPos),
                bodyOffset);
        }
        else
        {
            bodyOffset = 0;
        }

        var body = parseResult.Body;

        // Build a set of fenced code block ranges to skip
        var codeRanges = BuildCodeRanges(body, bodyOffset);

        var directives = ParseDirectives(body, bodyOffset, lineMap, codeRanges);
        var tags = ParseTags(body, bodyOffset, lineMap, codeRanges);
        var headings = ParseHeadings(body, bodyOffset, lineMap, codeRanges);

        return new MdaDocument(uri, content, version, lineMap, frontmatter, directives, tags, headings);
    }

    private static List<(int Start, int End)> BuildCodeRanges(string body, int bodyOffset)
    {
        var ranges = new List<(int Start, int End)>();
        foreach (Match m in FencedCodePattern.Matches(body))
        {
            ranges.Add((bodyOffset + m.Index, bodyOffset + m.Index + m.Length));
        }
        return ranges;
    }

    private static bool IsInCodeRange(int offset, List<(int Start, int End)> codeRanges)
    {
        foreach (var (start, end) in codeRanges)
        {
            if (offset >= start && offset < end)
            {
                return true;
            }
        }
        return false;
    }

    private static List<DirectiveUsage> ParseDirectives(
        string body,
        int bodyOffset,
        LineMap lineMap,
        List<(int Start, int End)> codeRanges)
    {
        var directives = new List<DirectiveUsage>();

        foreach (Match m in DirectiveOpenPattern.Matches(body))
        {
            var absoluteOffset = bodyOffset + m.Index;
            if (IsInCodeRange(absoluteOffset, codeRanges))
            {
                continue;
            }

            var name = m.Groups[1].Value;
            var propsString = m.Groups[2].Success ? m.Groups[2].Value : string.Empty;

            // Name range: starts at offset+3 (after :::), length of name
            var nameStart = absoluteOffset + 3;
            var nameEnd = nameStart + name.Length;

            // Full range: entire opening line
            var lineEnd = absoluteOffset + m.Length;

            // Detect if there's a matching close :::
            var afterOpen = m.Index + m.Length;
            var closeMatch = DirectiveClosePattern.Match(body, afterOpen);
            var isBlock = closeMatch.Success;

            directives.Add(new DirectiveUsage(
                name,
                propsString,
                lineMap.OffsetToRange(nameStart, nameEnd),
                lineMap.OffsetToRange(absoluteOffset, lineEnd),
                isBlock));
        }

        return directives;
    }

    private static List<TagUsage> ParseTags(
        string body,
        int bodyOffset,
        LineMap lineMap,
        List<(int Start, int End)> codeRanges)
    {
        var tags = new List<TagUsage>();

        foreach (Match m in TagOpenPattern.Matches(body))
        {
            var absoluteOffset = bodyOffset + m.Index;
            if (IsInCodeRange(absoluteOffset, codeRanges))
            {
                continue;
            }

            var name = m.Groups[1].Value;
            var attrString = m.Groups[2].Value;
            var isSelfClosing = m.Groups[3].Success && m.Groups[3].Value.Contains('/');

            // Name range: starts at offset+1 (after <), length of name
            var nameStart = absoluteOffset + 1;
            var nameEnd = nameStart + name.Length;

            var fullEnd = absoluteOffset + m.Length;

            var attributes = ParseAttributes(attrString);

            tags.Add(new TagUsage(
                name,
                attributes,
                lineMap.OffsetToRange(nameStart, nameEnd),
                lineMap.OffsetToRange(absoluteOffset, fullEnd),
                isSelfClosing));
        }

        return tags;
    }

    private static Dictionary<string, string?> ParseAttributes(string attrString)
    {
        var result = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(attrString))
        {
            return result;
        }

        foreach (Match m in AttributePattern.Matches(attrString))
        {
            var key = m.Groups[1].Value;
            string? value = null;
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
            // else boolean attribute (null value)

            result[key] = value;
        }

        return result;
    }

    private static List<HeadingInfo> ParseHeadings(
        string body,
        int bodyOffset,
        LineMap lineMap,
        List<(int Start, int End)> codeRanges)
    {
        var headings = new List<HeadingInfo>();

        foreach (Match m in HeadingPattern.Matches(body))
        {
            var absoluteOffset = bodyOffset + m.Index;
            if (IsInCodeRange(absoluteOffset, codeRanges))
            {
                continue;
            }

            var level = m.Groups[1].Value.Length;
            var text = m.Groups[2].Value.Trim();
            var lineEnd = absoluteOffset + m.Length;

            headings.Add(new HeadingInfo(
                level,
                text,
                lineMap.OffsetToRange(absoluteOffset, lineEnd)));
        }

        return headings;
    }

    /// <summary>
    /// Detects the pattern <c>---\n---</c> at the start of the file, which
    /// <see cref="FrontmatterParser"/> treats as "no frontmatter" because the
    /// raw YAML between the delimiters is empty.
    /// </summary>
    private static bool HasEmptyFrontmatter(string content)
    {
        // Match: ---\n--- or ---\r\n--- at the very start
        var span = content.AsSpan();
        if (!span.StartsWith("---"))
        {
            return false;
        }

        var i = 3;
        if (i < span.Length && span[i] == '\r')
        {
            i++;
        }

        if (i >= span.Length || span[i] != '\n')
        {
            return false;
        }

        i++;

        return i + 3 <= span.Length && span.Slice(i, 3).SequenceEqual("---");
    }
}
