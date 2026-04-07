using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using LspRange = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace Atoll.Lsp.Parsing;

/// <summary>
/// Maps between absolute character offsets and (line, character) positions.
/// Lines and characters are 0-based (LSP convention).
/// </summary>
internal sealed class LineMap
{
    private readonly int[] _lineStarts;

    internal LineMap(string content)
    {
        _lineStarts = BuildLineStarts(content);
    }

    internal int LineCount => _lineStarts.Length;

    /// <summary>
    /// Converts a 0-based (line, character) position to an absolute offset.
    /// </summary>
    internal int PositionToOffset(int line, int character)
    {
        if (line < 0 || line >= _lineStarts.Length)
        {
            return 0;
        }

        return _lineStarts[line] + character;
    }

    /// <summary>
    /// Converts an absolute offset to a 0-based (line, character) position.
    /// </summary>
    internal Position OffsetToPosition(int offset)
    {
        if (offset <= 0)
        {
            return new Position(0, 0);
        }

        // Binary search for the line
        var lo = 0;
        var hi = _lineStarts.Length - 1;
        while (lo < hi)
        {
            var mid = (lo + hi + 1) / 2;
            if (_lineStarts[mid] <= offset)
            {
                lo = mid;
            }
            else
            {
                hi = mid - 1;
            }
        }

        return new Position(lo, offset - _lineStarts[lo]);
    }

    /// <summary>
    /// Creates an LSP Range from two absolute offsets.
    /// </summary>
    internal LspRange OffsetToRange(int startOffset, int endOffset)
        => new(OffsetToPosition(startOffset), OffsetToPosition(endOffset));

    private static int[] BuildLineStarts(string content)
    {
        var starts = new List<int> { 0 };
        for (var i = 0; i < content.Length; i++)
        {
            if (content[i] == '\n')
            {
                starts.Add(i + 1);
            }
        }

        return [.. starts];
    }
}
