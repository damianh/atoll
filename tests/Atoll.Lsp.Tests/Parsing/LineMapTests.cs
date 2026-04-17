using Atoll.Lsp.Parsing;
using OmniSharp.Extensions.LanguageServer.Protocol;

namespace Atoll.Lsp.Tests.Parsing;

public sealed class LineMapTests
{
    [Fact]
    public void ShouldReturnZeroPositionForEmptyContent()
    {
        var map = new LineMap(string.Empty);
        var pos = map.OffsetToPosition(0);
        pos.Line.ShouldBe(0);
        pos.Character.ShouldBe(0);
    }

    [Fact]
    public void ShouldMapFirstLineOffsets()
    {
        var map = new LineMap("hello world");
        var pos = map.OffsetToPosition(6);
        pos.Line.ShouldBe(0);
        pos.Character.ShouldBe(6);
    }

    [Fact]
    public void ShouldMapSecondLineOffset()
    {
        var map = new LineMap("line1\nline2\nline3");
        var pos = map.OffsetToPosition(6); // Start of "line2"
        pos.Line.ShouldBe(1);
        pos.Character.ShouldBe(0);
    }

    [Fact]
    public void ShouldMapOffsetInMiddleOfSecondLine()
    {
        var map = new LineMap("line1\nline2\nline3");
        var pos = map.OffsetToPosition(8); // "ne2" in line2
        pos.Line.ShouldBe(1);
        pos.Character.ShouldBe(2);
    }

    [Fact]
    public void ShouldRoundTripPositionToOffset()
    {
        var content = "hello\nworld\nfoo";
        var map = new LineMap(content);
        var offset = map.PositionToOffset(1, 3); // line 1, col 3 = "l" in "world"
        offset.ShouldBe(9);
    }

    [Fact]
    public void ShouldCreateRangeFromOffsets()
    {
        var map = new LineMap("hello\nworld");
        var range = map.OffsetToRange(0, 5); // "hello"
        range.Start.Line.ShouldBe(0);
        range.Start.Character.ShouldBe(0);
        range.End.Line.ShouldBe(0);
        range.End.Character.ShouldBe(5);
    }

    [Fact]
    public void ShouldHandleWindowsLineEndings()
    {
        var map = new LineMap("line1\r\nline2");
        // line2 starts at offset 7 (5 + \r + \n = 7)
        var pos = map.OffsetToPosition(7);
        pos.Line.ShouldBe(1);
        pos.Character.ShouldBe(0);
    }

    [Fact]
    public void ShouldReturnZeroLineCountForEmptyString()
    {
        var map = new LineMap(string.Empty);
        map.LineCount.ShouldBe(1); // Always at least one line (line 0)
    }
}
