using Atoll.Lagoon.Markdown;
using Shouldly;
using Xunit;

namespace Atoll.Lagoon.Tests.Markdown;

public sealed class CodeBlockMetaParserTests
{
    // ── Language-only ──────────────────────────────────────────────────────────

    [Fact]
    public void ShouldReturnLanguageWhenNoArguments()
    {
        var meta = CodeBlockMetaParser.Parse("csharp", null);

        meta.Language.ShouldBe("csharp");
        meta.Title.ShouldBeNull();
        meta.Frame.ShouldBe(CodeFrameType.Auto);
        meta.LineMarkers.ShouldBeEmpty();
        meta.InlineMarkers.ShouldBeEmpty();
        meta.CollapseRanges.ShouldBeEmpty();
        meta.Wrap.ShouldBeFalse();
        meta.ShowLineNumbers.ShouldBeNull();
        meta.DiffLang.ShouldBeNull();
    }

    [Fact]
    public void ShouldHandleNullLanguageAndNullArguments()
    {
        var meta = CodeBlockMetaParser.Parse(null, null);

        meta.Language.ShouldBeNull();
    }

    [Fact]
    public void ShouldHandleEmptyArguments()
    {
        var meta = CodeBlockMetaParser.Parse("js", "");

        meta.Language.ShouldBe("js");
        meta.Title.ShouldBeNull();
    }

    [Fact]
    public void ShouldHandleWhitespaceOnlyArguments()
    {
        var meta = CodeBlockMetaParser.Parse("js", "   ");

        meta.Language.ShouldBe("js");
        meta.Title.ShouldBeNull();
    }

    // ── title= ────────────────────────────────────────────────────────────────

    [Fact]
    public void ShouldParseTitleAttribute()
    {
        var meta = CodeBlockMetaParser.Parse("csharp", """title="Program.cs" """);

        meta.Title.ShouldBe("Program.cs");
    }

    [Fact]
    public void ShouldParseTitleWithSpaces()
    {
        var meta = CodeBlockMetaParser.Parse("csharp", """title="My File.cs" """);

        meta.Title.ShouldBe("My File.cs");
    }

    [Fact]
    public void ShouldParseTitleWithPath()
    {
        var meta = CodeBlockMetaParser.Parse("csharp", """title="src/MyApp/Program.cs" """);

        meta.Title.ShouldBe("src/MyApp/Program.cs");
    }

    // ── frame= ────────────────────────────────────────────────────────────────

    [Fact]
    public void ShouldParseFrameCode()
    {
        var meta = CodeBlockMetaParser.Parse("bash", "frame=\"code\"");

        meta.Frame.ShouldBe(CodeFrameType.Code);
    }

    [Fact]
    public void ShouldParseFrameTerminal()
    {
        var meta = CodeBlockMetaParser.Parse("csharp", "frame=\"terminal\"");

        meta.Frame.ShouldBe(CodeFrameType.Terminal);
    }

    [Fact]
    public void ShouldParseFrameNone()
    {
        var meta = CodeBlockMetaParser.Parse("csharp", "frame=\"none\"");

        meta.Frame.ShouldBe(CodeFrameType.None);
    }

    [Fact]
    public void ShouldDefaultFrameToAutoForUnknownValue()
    {
        var meta = CodeBlockMetaParser.Parse("csharp", "frame=\"weird\"");

        meta.Frame.ShouldBe(CodeFrameType.Auto);
    }

    // ── Frame auto-detection ──────────────────────────────────────────────────

    [Theory]
    [InlineData("bash")]
    [InlineData("sh")]
    [InlineData("shell")]
    [InlineData("zsh")]
    [InlineData("powershell")]
    [InlineData("pwsh")]
    [InlineData("ps1")]
    [InlineData("cmd")]
    [InlineData("terminal")]
    [InlineData("console")]
    public void ShouldAutoDetectTerminalLanguages(string lang)
    {
        var meta = CodeBlockMetaParser.Parse(lang, null);

        CodeBlockMetaParser.ResolveFrameType(meta).ShouldBe(CodeFrameType.Terminal);
    }

    [Fact]
    public void ShouldAutoDetectEditorFrameForCSharp()
    {
        var meta = CodeBlockMetaParser.Parse("csharp", null);

        CodeBlockMetaParser.ResolveFrameType(meta).ShouldBe(CodeFrameType.Code);
    }

    [Fact]
    public void ShouldAutoDetectEditorFrameWhenTitleSet()
    {
        // Even terminal language gets code frame when title is provided via auto
        var meta = CodeBlockMetaParser.Parse("bash", """title="script.sh" """);

        CodeBlockMetaParser.ResolveFrameType(meta).ShouldBe(CodeFrameType.Code);
    }

    [Fact]
    public void ShouldRespectExplicitFrameOverAutoDetect()
    {
        var meta = CodeBlockMetaParser.Parse("bash", "frame=\"code\"");

        CodeBlockMetaParser.ResolveFrameType(meta).ShouldBe(CodeFrameType.Code);
    }

    // ── Line markers: bare {range-list} → mark ────────────────────────────────

    [Fact]
    public void ShouldParseSingleLineMarkRange()
    {
        var meta = CodeBlockMetaParser.Parse("csharp", "{3}");

        meta.LineMarkers.ShouldHaveSingleItem()
            .Type.ShouldBe(LineMarkerType.Mark);
        meta.LineMarkers[0].Ranges.ShouldHaveSingleItem()
            .ShouldBe(new LineRange(3, 3));
    }

    [Fact]
    public void ShouldParseLineRangeWithDash()
    {
        var meta = CodeBlockMetaParser.Parse("csharp", "{7-8}");

        meta.LineMarkers.ShouldHaveSingleItem()
            .Ranges.ShouldHaveSingleItem()
            .ShouldBe(new LineRange(7, 8));
    }

    [Fact]
    public void ShouldParseMultipleRangesInSingleBraces()
    {
        var meta = CodeBlockMetaParser.Parse("csharp", "{1, 4, 7-8}");

        var ranges = meta.LineMarkers.ShouldHaveSingleItem().Ranges;
        ranges.Count.ShouldBe(3);
        ranges[0].ShouldBe(new LineRange(1, 1));
        ranges[1].ShouldBe(new LineRange(4, 4));
        ranges[2].ShouldBe(new LineRange(7, 8));
    }

    // ── ins= and del= ─────────────────────────────────────────────────────────

    [Fact]
    public void ShouldParseInsRange()
    {
        var meta = CodeBlockMetaParser.Parse("csharp", "ins={3-4}");

        meta.LineMarkers.ShouldHaveSingleItem()
            .Type.ShouldBe(LineMarkerType.Ins);
        meta.LineMarkers[0].Ranges.ShouldHaveSingleItem()
            .ShouldBe(new LineRange(3, 4));
    }

    [Fact]
    public void ShouldParseDelRange()
    {
        var meta = CodeBlockMetaParser.Parse("csharp", "del={2}");

        meta.LineMarkers.ShouldHaveSingleItem()
            .Type.ShouldBe(LineMarkerType.Del);
        meta.LineMarkers[0].Ranges.ShouldHaveSingleItem()
            .ShouldBe(new LineRange(2, 2));
    }

    [Fact]
    public void ShouldParseCombinedMarkInsDelMarkers()
    {
        var meta = CodeBlockMetaParser.Parse("csharp", "{1} ins={3} del={5}");

        meta.LineMarkers.Count.ShouldBe(3);

        var mark = meta.LineMarkers.Single(m => m.Type == LineMarkerType.Mark);
        mark.Ranges.ShouldHaveSingleItem().ShouldBe(new LineRange(1, 1));

        var ins = meta.LineMarkers.Single(m => m.Type == LineMarkerType.Ins);
        ins.Ranges.ShouldHaveSingleItem().ShouldBe(new LineRange(3, 3));

        var del = meta.LineMarkers.Single(m => m.Type == LineMarkerType.Del);
        del.Ranges.ShouldHaveSingleItem().ShouldBe(new LineRange(5, 5));
    }

    // ── Inline text markers ───────────────────────────────────────────────────

    [Fact]
    public void ShouldParseLiteralInlineMarker()
    {
        var meta = CodeBlockMetaParser.Parse("csharp", "\"hello\"");

        meta.InlineMarkers.ShouldHaveSingleItem();
        var marker = meta.InlineMarkers[0];
        marker.IsRegex.ShouldBeFalse();
        marker.Pattern.ShouldBe("hello");
    }

    [Fact]
    public void ShouldParseMultipleLiteralInlineMarkers()
    {
        var meta = CodeBlockMetaParser.Parse("csharp", "\"foo\" \"bar\"");

        meta.InlineMarkers.Count.ShouldBe(2);
        meta.InlineMarkers[0].Pattern.ShouldBe("foo");
        meta.InlineMarkers[1].Pattern.ShouldBe("bar");
    }

    // ── Inline regex markers ──────────────────────────────────────────────────

    [Fact]
    public void ShouldParseRegexMarkerWithoutCaptureGroup()
    {
        var meta = CodeBlockMetaParser.Parse("csharp", @"/\d+/");

        meta.InlineMarkers.ShouldHaveSingleItem();
        var marker = meta.InlineMarkers[0];
        marker.IsRegex.ShouldBeTrue();
        marker.Pattern.ShouldBe(@"\d+");
        marker.HasCaptureGroup.ShouldBeFalse();
    }

    [Fact]
    public void ShouldParseRegexMarkerWithCaptureGroup()
    {
        var meta = CodeBlockMetaParser.Parse("csharp", @"/total: (\d+)/");

        var marker = meta.InlineMarkers.ShouldHaveSingleItem();
        marker.IsRegex.ShouldBeTrue();
        marker.Pattern.ShouldBe(@"total: (\d+)");
        marker.HasCaptureGroup.ShouldBeTrue();
    }

    [Fact]
    public void ShouldNotTreatNonCapturingGroupAsCaptureGroup()
    {
        // (?:...) is non-capturing
        var meta = CodeBlockMetaParser.Parse("csharp", @"/(?:foo|bar)/");

        var marker = meta.InlineMarkers.ShouldHaveSingleItem();
        marker.HasCaptureGroup.ShouldBeFalse();
    }

    // ── collapse= ────────────────────────────────────────────────────────────

    [Fact]
    public void ShouldParseCollapseRange()
    {
        var meta = CodeBlockMetaParser.Parse("csharp", "collapse={1-5}");

        meta.CollapseRanges.ShouldHaveSingleItem()
            .ShouldBe(new LineRange(1, 5));
    }

    [Fact]
    public void ShouldParseMultipleCollapseRanges()
    {
        var meta = CodeBlockMetaParser.Parse("csharp", "collapse={1-5, 12-14}");

        meta.CollapseRanges.Count.ShouldBe(2);
        meta.CollapseRanges[0].ShouldBe(new LineRange(1, 5));
        meta.CollapseRanges[1].ShouldBe(new LineRange(12, 14));
    }

    // ── wrap ─────────────────────────────────────────────────────────────────

    [Fact]
    public void ShouldParseWrapFlag()
    {
        var meta = CodeBlockMetaParser.Parse("csharp", "wrap");

        meta.Wrap.ShouldBeTrue();
    }

    [Fact]
    public void ShouldParseWrapFalse()
    {
        var meta = CodeBlockMetaParser.Parse("csharp", "wrap=false");

        meta.Wrap.ShouldBeFalse();
    }

    [Fact]
    public void ShouldPreserveIndentDefaultToTrue()
    {
        var meta = CodeBlockMetaParser.Parse("csharp", "wrap");

        meta.PreserveIndent.ShouldBeTrue();
    }

    [Fact]
    public void ShouldParsePreserveIndentFalse()
    {
        var meta = CodeBlockMetaParser.Parse("csharp", "preserveIndent=false");

        meta.PreserveIndent.ShouldBeFalse();
    }

    // ── showLineNumbers / startLineNumber ────────────────────────────────────

    [Fact]
    public void ShouldParseShowLineNumbers()
    {
        var meta = CodeBlockMetaParser.Parse("csharp", "showLineNumbers");

        meta.ShowLineNumbers.ShouldBe(1);
    }

    [Fact]
    public void ShouldParseStartLineNumber()
    {
        var meta = CodeBlockMetaParser.Parse("csharp", "startLineNumber=5");

        meta.ShowLineNumbers.ShouldBe(5);
    }

    // ── lang= (diff language) ────────────────────────────────────────────────

    [Fact]
    public void ShouldParseDiffLangAttribute()
    {
        var meta = CodeBlockMetaParser.Parse("diff", "lang=\"csharp\"");

        meta.DiffLang.ShouldBe("csharp");
    }

    [Fact]
    public void ShouldParseLangWithoutQuotes()
    {
        var meta = CodeBlockMetaParser.Parse("diff", "lang=js");

        meta.DiffLang.ShouldBe("js");
    }

    // ── LineRange.Contains ───────────────────────────────────────────────────

    [Fact]
    public void LineRangeShouldContainLineInRange()
    {
        var range = new LineRange(3, 7);

        range.Contains(3).ShouldBeTrue();
        range.Contains(5).ShouldBeTrue();
        range.Contains(7).ShouldBeTrue();
    }

    [Fact]
    public void LineRangeShouldNotContainLineOutsideRange()
    {
        var range = new LineRange(3, 7);

        range.Contains(2).ShouldBeFalse();
        range.Contains(8).ShouldBeFalse();
    }

    // ── Unknown attributes silently ignored ──────────────────────────────────

    [Fact]
    public void ShouldIgnoreUnknownAttributes()
    {
        // Should not throw
        var meta = CodeBlockMetaParser.Parse("csharp", "unknownAttr=\"value\" {1}");

        meta.LineMarkers.ShouldHaveSingleItem();
    }

    // ── Combined attributes ───────────────────────────────────────────────────

    [Fact]
    public void ShouldParseCombinedAttributes()
    {
        var meta = CodeBlockMetaParser.Parse(
            "csharp",
            """title="Program.cs" frame="code" {5-7} ins={12-14} wrap showLineNumbers""");

        meta.Title.ShouldBe("Program.cs");
        meta.Frame.ShouldBe(CodeFrameType.Code);
        meta.Wrap.ShouldBeTrue();
        meta.ShowLineNumbers.ShouldNotBeNull();

        var markMarker = meta.LineMarkers.Single(m => m.Type == LineMarkerType.Mark);
        markMarker.Ranges.ShouldHaveSingleItem().ShouldBe(new LineRange(5, 7));

        var insMarker = meta.LineMarkers.Single(m => m.Type == LineMarkerType.Ins);
        insMarker.Ranges.ShouldHaveSingleItem().ShouldBe(new LineRange(12, 14));
    }
}
