namespace Atoll.Lagoon.Markdown;

/// <summary>
/// Identifies the visual frame to render around a code block.
/// </summary>
internal enum CodeFrameType
{
    /// <summary>Auto-detect from language and attributes.</summary>
    Auto,

    /// <summary>Render as an editor (IDE) window with a file-tab header.</summary>
    Code,

    /// <summary>Render as a terminal window with traffic-light dots header.</summary>
    Terminal,

    /// <summary>No frame — render without any header chrome.</summary>
    None,
}

/// <summary>
/// The type of line highlight applied to a source line.
/// </summary>
internal enum LineMarkerType
{
    /// <summary>Neutral highlight (blue).</summary>
    Mark,

    /// <summary>Insertion highlight (green).</summary>
    Ins,

    /// <summary>Deletion highlight (red).</summary>
    Del,
}

/// <summary>
/// Represents a contiguous range of 1-based line numbers.
/// </summary>
/// <param name="Start">The first line in the range (1-based, inclusive).</param>
/// <param name="End">The last line in the range (1-based, inclusive).</param>
internal readonly record struct LineRange(int Start, int End)
{
    /// <summary>Returns <c>true</c> when <paramref name="lineNumber"/> falls within this range.</summary>
    internal bool Contains(int lineNumber) => lineNumber >= Start && lineNumber <= End;
}

/// <summary>
/// A line marker entry associating a set of line ranges with a highlight type.
/// </summary>
/// <param name="Type">The highlight type.</param>
/// <param name="Ranges">The 1-based line ranges to highlight.</param>
internal sealed record LineMarker(LineMarkerType Type, IReadOnlyList<LineRange> Ranges);

/// <summary>
/// An inline text marker that highlights occurrences of a literal string or a regex pattern
/// within the rendered code lines.
/// </summary>
internal sealed record InlineMarker
{
    /// <summary>
    /// <c>true</c> when this marker uses a regular expression; <c>false</c> for a literal string.
    /// </summary>
    internal bool IsRegex { get; init; }

    /// <summary>
    /// The literal text to match (when <see cref="IsRegex"/> is <c>false</c>) or
    /// the regex pattern string (when <see cref="IsRegex"/> is <c>true</c>).
    /// </summary>
    internal string Pattern { get; init; } = string.Empty;

    /// <summary>
    /// When <c>true</c> the regex pattern contains a capture group and only the first
    /// captured sub-match is highlighted (rather than the full match).
    /// </summary>
    internal bool HasCaptureGroup { get; init; }
}

/// <summary>
/// Parsed metadata extracted from a fenced code block's info string and arguments.
/// All properties are optional — missing attributes use sensible defaults.
/// </summary>
internal sealed class CodeBlockMeta
{
    /// <summary>The language identifier (e.g., <c>"csharp"</c>). May be null or empty.</summary>
    internal string? Language { get; init; }

    /// <summary>
    /// The title shown in the frame header (e.g., the file name).
    /// <c>null</c> means no title.
    /// </summary>
    internal string? Title { get; init; }

    /// <summary>
    /// The frame type to render.  Defaults to <see cref="CodeFrameType.Auto"/>.
    /// </summary>
    internal CodeFrameType Frame { get; init; } = CodeFrameType.Auto;

    /// <summary>
    /// Line markers (mark / ins / del) to apply.  Empty when none are specified.
    /// </summary>
    internal IReadOnlyList<LineMarker> LineMarkers { get; init; } = [];

    /// <summary>
    /// Inline text markers (literal or regex) to apply.  Empty when none are specified.
    /// </summary>
    internal IReadOnlyList<InlineMarker> InlineMarkers { get; init; } = [];

    /// <summary>
    /// Ranges of lines to collapse.  Empty when no collapsing is requested.
    /// </summary>
    internal IReadOnlyList<LineRange> CollapseRanges { get; init; } = [];

    /// <summary>
    /// When <c>true</c>, long lines wrap instead of causing horizontal scrolling.
    /// </summary>
    internal bool Wrap { get; init; }

    /// <summary>
    /// When <c>true</c> (the default when <see cref="Wrap"/> is set), wrapped continuation
    /// lines are indented to match the leading whitespace of the original line.
    /// </summary>
    internal bool PreserveIndent { get; init; } = true;

    /// <summary>
    /// When set, line numbers are rendered in a gutter, starting at this value (1-based).
    /// <c>null</c> means no line numbers.
    /// </summary>
    internal int? ShowLineNumbers { get; init; }

    /// <summary>
    /// For <c>diff</c> blocks: the underlying language to use for syntax highlighting
    /// after stripping the <c>+</c>/<c>-</c> diff prefix.
    /// </summary>
    internal string? DiffLang { get; init; }

    /// <summary>
    /// Returns a <see cref="CodeBlockMeta"/> with only the language set — used as a fast
    /// path when the arguments string is absent or blank.
    /// </summary>
    internal static CodeBlockMeta ForLanguage(string? language) =>
        new() { Language = language };
}
