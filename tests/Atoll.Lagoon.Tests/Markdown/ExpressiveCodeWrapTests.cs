using Atoll.Lagoon.Markdown;

namespace Atoll.Lagoon.Tests.Markdown;

public sealed class ExpressiveCodeWrapTests
{
    private static string Render(string markdown)
    {
        var options = new DocsMarkdownOptions { EnableSyntaxHighlighting = true };
        return DocsMarkdownRenderer.Render(markdown, options).Html;
    }

    // ── data-wrap attribute ───────────────────────────────────────────────────

    [Fact]
    public void ShouldAddDataWrapAttributeWhenWrapSpecified()
    {
        var md = "```csharp wrap\nvar x = 1;\n```";
        var html = Render(md);

        html.ShouldContain("data-wrap");
    }

    [Fact]
    public void ShouldNotAddDataWrapWhenWrapNotSpecified()
    {
        var md = "```csharp\nvar x = 1;\n```";
        var html = Render(md);

        html.ShouldNotContain("data-wrap");
    }

    [Fact]
    public void ShouldAddDataWrapToFigureForFramedBlock()
    {
        var md = "```csharp wrap title=\"file.cs\"\nvar x = 1;\n```";
        var html = Render(md);

        // The figure element should carry the data-wrap attribute.
        html.ShouldContain("ec-frame");
        html.ShouldContain("data-wrap");
    }

    [Fact]
    public void ShouldAddDataWrapToWrapperDivForFrameNone()
    {
        var md = "```csharp wrap frame=\"none\"\nvar x = 1;\n```";
        var html = Render(md);

        html.ShouldContain("code-block-wrapper");
        html.ShouldContain("data-wrap");
    }

    // ── preserveIndent ────────────────────────────────────────────────────────

    [Fact]
    public void ShouldAddIndentStyleWhenPreserveIndentIsTrue()
    {
        // wrap defaults to preserveIndent=true.
        var md = "```csharp wrap\n    var x = 1;\n```";
        var html = Render(md);

        // 4 leading spaces → --ec-indent:4ch
        html.ShouldContain("--ec-indent:4ch");
    }

    [Fact]
    public void ShouldNotAddIndentStyleWhenPreserveIndentIsFalse()
    {
        var md = "```csharp wrap preserveIndent=false\n    var x = 1;\n```";
        var html = Render(md);

        html.ShouldNotContain("--ec-indent");
    }

    [Fact]
    public void ShouldSetZeroIndentForLineWithNoLeadingWhitespace()
    {
        var md = "```csharp wrap\nvar x = 1;\n```";
        var html = Render(md);

        html.ShouldContain("--ec-indent:0ch");
    }

    [Fact]
    public void ShouldComputeIndentForTabCharacter()
    {
        // A single tab counts as 1 character for --ec-indent.
        var md = "```csharp wrap\n\tvar x = 1;\n```";
        var html = Render(md);

        html.ShouldContain("--ec-indent:1ch");
    }
}
