using System.Text;
using System.Text.RegularExpressions;

namespace Atoll.Lagoon.Markdown;

/// <summary>
/// Applies inline text markers to a line of syntax-highlighted HTML.
/// Markers can be literal strings or regular expressions; they wrap matching
/// text in <c>&lt;mark class="ec-text-marker"&gt;</c> elements, even when the
/// match spans multiple syntax-highlighting token boundaries.
/// </summary>
internal static class InlineMarkerApplier
{
    /// <summary>
    /// Applies all inline markers to the given line's plain text and rendered HTML tokens,
    /// returning the HTML with <c>&lt;mark&gt;</c> wrappers inserted at match positions.
    /// </summary>
    /// <param name="plainText">The raw source text of the line (no HTML).</param>
    /// <param name="tokenHtml">
    /// A list of (startIndex, endIndex, html) tuples representing the HTML rendering
    /// of each token.  <c>html</c> is the already-escaped/span-wrapped fragment.
    /// <c>startIndex</c>/<c>endIndex</c> are character offsets in <paramref name="plainText"/>.
    /// </param>
    /// <param name="markers">The inline markers to apply.</param>
    /// <returns>The assembled HTML for the line content.</returns>
    internal static string Apply(
        string plainText,
        IReadOnlyList<TokenFragment> tokenHtml,
        IReadOnlyList<InlineMarker> markers)
    {
        if (markers.Count == 0 || tokenHtml.Count == 0)
        {
            return BuildPlainHtml(tokenHtml);
        }

        // Collect all match intervals (start inclusive, end exclusive) in plain-text space.
        var intervals = CollectMatchIntervals(plainText, markers);

        if (intervals.Count == 0)
        {
            return BuildPlainHtml(tokenHtml);
        }

        // Merge overlapping intervals.
        intervals = MergeIntervals(intervals);

        // Re-render tokens with <mark> wrappers inserted.
        return RenderWithMarks(plainText, tokenHtml, intervals);
    }

    /// <summary>
    /// Collects all match intervals for all markers in priority order.
    /// Each interval is (start inclusive, end exclusive) in plain-text char offsets.
    /// </summary>
    private static List<(int Start, int End)> CollectMatchIntervals(
        string text,
        IReadOnlyList<InlineMarker> markers)
    {
        var result = new List<(int Start, int End)>();

        foreach (var marker in markers)
        {
            if (marker.IsRegex)
            {
                CollectRegexMatches(text, marker, result);
            }
            else
            {
                CollectLiteralMatches(text, marker.Pattern, result);
            }
        }

        return result;
    }

    private static void CollectLiteralMatches(
        string text,
        string pattern,
        List<(int Start, int End)> result)
    {
        if (string.IsNullOrEmpty(pattern))
        {
            return;
        }

        var idx = 0;
        while (true)
        {
            var pos = text.IndexOf(pattern, idx, StringComparison.Ordinal);
            if (pos < 0)
            {
                break;
            }

            result.Add((pos, pos + pattern.Length));
            idx = pos + pattern.Length;
            if (idx >= text.Length)
            {
                break;
            }
        }
    }

    private static void CollectRegexMatches(
        string text,
        InlineMarker marker,
        List<(int Start, int End)> result)
    {
        Regex regex;
        try
        {
            regex = new Regex(marker.Pattern, RegexOptions.None, TimeSpan.FromSeconds(1));
        }
        catch (ArgumentException)
        {
            // Invalid regex — skip silently.
            return;
        }

        foreach (Match match in regex.Matches(text))
        {
            if (!match.Success)
            {
                continue;
            }

            if (marker.HasCaptureGroup && match.Groups.Count > 1)
            {
                // Highlight only the first capture group.
                var group = match.Groups[1];
                if (group.Success)
                {
                    result.Add((group.Index, group.Index + group.Length));
                }
            }
            else
            {
                result.Add((match.Index, match.Index + match.Length));
            }
        }
    }

    private static List<(int Start, int End)> MergeIntervals(List<(int Start, int End)> intervals)
    {
        if (intervals.Count <= 1)
        {
            return intervals;
        }

        intervals.Sort((a, b) => a.Start.CompareTo(b.Start));

        var merged = new List<(int Start, int End)> { intervals[0] };
        for (var i = 1; i < intervals.Count; i++)
        {
            var (s, e) = intervals[i];
            var last = merged[^1];
            if (s < last.End)
            {
                merged[^1] = (last.Start, Math.Max(last.End, e));
            }
            else
            {
                merged.Add((s, e));
            }
        }

        return merged;
    }

    /// <summary>
    /// Renders token fragments with <c>&lt;mark&gt;</c> wrappers inserted at
    /// the given plain-text intervals.
    /// </summary>
    private static string RenderWithMarks(
        string plainText,
        IReadOnlyList<TokenFragment> tokens,
        List<(int Start, int End)> intervals)
    {
        var sb = new StringBuilder();
        var intervalIdx = 0;
        var insideMark = false;

        foreach (var token in tokens)
        {
            var tStart = token.Start;
            var tEnd = token.End;

            if (tStart >= tEnd)
            {
                continue;
            }

            // We need to potentially split this token around mark boundaries.
            // Walk character by character within the token's plain-text range.
            var pos = tStart;

            while (pos < tEnd)
            {
                // Advance interval index past expired intervals.
                while (intervalIdx < intervals.Count && intervals[intervalIdx].End <= pos)
                {
                    intervalIdx++;
                    if (insideMark)
                    {
                        sb.Append("</mark>");
                        insideMark = false;
                    }
                }

                // Determine the next boundary within this token.
                int segEnd;
                bool segInMark;

                if (intervalIdx < intervals.Count)
                {
                    var (iStart, iEnd) = intervals[intervalIdx];

                    if (pos < iStart)
                    {
                        // Before the next interval.
                        segEnd = Math.Min(tEnd, iStart);
                        segInMark = false;
                    }
                    else if (pos >= iStart && pos < iEnd)
                    {
                        // Inside the interval.
                        segEnd = Math.Min(tEnd, iEnd);
                        segInMark = true;
                    }
                    else
                    {
                        segEnd = tEnd;
                        segInMark = false;
                    }
                }
                else
                {
                    segEnd = tEnd;
                    segInMark = false;
                }

                // Open/close mark as needed.
                if (segInMark && !insideMark)
                {
                    sb.Append("<mark class=\"ec-text-marker\">");
                    insideMark = true;
                }
                else if (!segInMark && insideMark)
                {
                    sb.Append("</mark>");
                    insideMark = false;
                }

                // Emit the segment. We must respect the token's HTML wrapper (span vs. plain).
                // Since the segment may be a sub-range of the token, we re-emit the HTML wrapper
                // around only the sub-text, escaping as needed.
                var segText = plainText[pos..segEnd];
                EmitSegment(sb, segText, token.CssClass);

                pos = segEnd;
            }
        }

        // Close any open mark.
        if (insideMark)
        {
            sb.Append("</mark>");
        }

        return sb.ToString();
    }

    private static void EmitSegment(StringBuilder sb, string text, string? cssClass)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        if (cssClass is not null)
        {
            sb.Append("<span class=\"");
            sb.Append(cssClass);
            sb.Append("\">");
            AppendEscaped(sb, text);
            sb.Append("</span>");
        }
        else
        {
            AppendEscaped(sb, text);
        }
    }

    private static void AppendEscaped(StringBuilder sb, string text)
    {
        foreach (var c in text)
        {
            sb.Append(c switch
            {
                '<' => "&lt;",
                '>' => "&gt;",
                '&' => "&amp;",
                '"' => "&quot;",
                '\'' => "&#39;",
                _ => c.ToString(),
            });
        }
    }

    private static string BuildPlainHtml(IReadOnlyList<TokenFragment> tokens)
    {
        var sb = new StringBuilder();
        foreach (var token in tokens)
        {
            if (token.Start >= token.End)
            {
                continue;
            }

            if (token.CssClass is not null)
            {
                sb.Append("<span class=\"");
                sb.Append(token.CssClass);
                sb.Append("\">");
                AppendEscaped(sb, token.Text);
                sb.Append("</span>");
            }
            else
            {
                AppendEscaped(sb, token.Text);
            }
        }

        return sb.ToString();
    }
}

/// <summary>
/// Represents one TextMate token's plain text and its resolved CSS class (if any).
/// Start/End are character offsets in the original line.
/// </summary>
/// <param name="Start">Inclusive start offset in the source line.</param>
/// <param name="End">Exclusive end offset in the source line.</param>
/// <param name="Text">The plain text content of this token.</param>
/// <param name="CssClass">The CSS class to wrap the token in, or <c>null</c> for unstyled text.</param>
internal readonly record struct TokenFragment(int Start, int End, string Text, string? CssClass);
