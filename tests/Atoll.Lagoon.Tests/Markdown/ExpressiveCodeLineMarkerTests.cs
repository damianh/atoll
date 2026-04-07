using Atoll.Lagoon.Markdown;
using Shouldly;
using Xunit;

namespace Atoll.Lagoon.Tests.Markdown;

public sealed class ExpressiveCodeLineMarkerTests
{
    private static string Render(string markdown)
    {
        var options = new DocsMarkdownOptions { EnableSyntaxHighlighting = true };
        return DocsMarkdownRenderer.Render(markdown, options).Html;
    }

    // ── Single line mark ──────────────────────────────────────────────────────

    [Fact]
    public void ShouldMarkSingleLine()
    {
        var md = "```csharp {2}\nvar x = 1;\nvar y = 2;\nvar z = 3;\n```";
        var html = Render(md);

        html.ShouldContain("ec-mark");
    }

    [Fact]
    public void ShouldNotMarkUnmarkedLines()
    {
        var md = "```csharp {2}\nvar x = 1;\nvar y = 2;\nvar z = 3;\n```";
        var html = Render(md);

        // Only line 2 is marked — verify ec-mark appears exactly once for the line div.
        // Count occurrences: just check the class is present and not on all lines.
        var firstLinePos = html.IndexOf("ec-line", StringComparison.Ordinal);
        var firstLineHtml = html[firstLinePos..];
        // First ec-line should NOT have ec-mark (that's line 1).
        firstLineHtml.ShouldStartWith("ec-line\"");
    }

    // ── Range mark ───────────────────────────────────────────────────────────

    [Fact]
    public void ShouldMarkLineRange()
    {
        var md = "```csharp {1-2}\nvar x = 1;\nvar y = 2;\nvar z = 3;\n```";
        var html = Render(md);

        html.ShouldContain("ec-mark");
    }

    // ── Multiple ranges ───────────────────────────────────────────────────────

    [Fact]
    public void ShouldMarkMultipleRanges()
    {
        var md = "```csharp {1, 3}\nvar x = 1;\nvar y = 2;\nvar z = 3;\n```";
        var html = Render(md);

        html.ShouldContain("ec-mark");
    }

    // ── Insertion markers ─────────────────────────────────────────────────────

    [Fact]
    public void ShouldMarkInsertionLines()
    {
        var md = "```csharp ins={2}\nvar x = 1;\nvar y = 2;\nvar z = 3;\n```";
        var html = Render(md);

        html.ShouldContain("ec-ins");
        html.ShouldNotContain("ec-mark");
    }

    [Fact]
    public void ShouldMarkInsertionRange()
    {
        var md = "```csharp ins={1-2}\nvar x = 1;\nvar y = 2;\nvar z = 3;\n```";
        var html = Render(md);

        html.ShouldContain("ec-ins");
    }

    // ── Deletion markers ──────────────────────────────────────────────────────

    [Fact]
    public void ShouldMarkDeletionLines()
    {
        var md = "```csharp del={1}\nvar x = 1;\nvar y = 2;\nvar z = 3;\n```";
        var html = Render(md);

        html.ShouldContain("ec-del");
        html.ShouldNotContain("ec-mark");
    }

    // ── Combined markers ──────────────────────────────────────────────────────

    [Fact]
    public void ShouldSupportCombinedMarkInsDelMarkers()
    {
        var md = "```csharp {1} ins={2} del={3}\nvar x = 1;\nvar y = 2;\nvar z = 3;\n```";
        var html = Render(md);

        html.ShouldContain("ec-mark");
        html.ShouldContain("ec-ins");
        html.ShouldContain("ec-del");
    }

    // ── Precedence: ins > del > mark ──────────────────────────────────────────

    [Fact]
    public void InsShouldTakePrecedenceOverMark()
    {
        // Line 1 appears in both mark {1} and ins={1} — ins should win.
        var md = "```csharp {1} ins={1}\nvar x = 1;\n```";
        var html = Render(md);

        html.ShouldContain("ec-ins");
        // ec-mark should not appear as a separate class on the same line
        html.ShouldNotContain("ec-mark");
    }

    [Fact]
    public void DelShouldTakePrecedenceOverMark()
    {
        // Line 1 appears in both mark {1} and del={1} — del should win.
        var md = "```csharp {1} del={1}\nvar x = 1;\n```";
        var html = Render(md);

        html.ShouldContain("ec-del");
        html.ShouldNotContain("ec-mark");
    }

    // ── Out-of-range lines ignored ────────────────────────────────────────────

    [Fact]
    public void ShouldIgnoreOutOfRangeLineMarkers()
    {
        // Code has 2 lines, marker targets line 99.
        var md = "```csharp {99}\nvar x = 1;\nvar y = 2;\n```";
        var html = Render(md);

        html.ShouldNotContain("ec-mark");
        html.ShouldNotContain("ec-ins");
        html.ShouldNotContain("ec-del");
    }

    // ── Marker with title ─────────────────────────────────────────────────────

    [Fact]
    public void ShouldSupportLineMarkersWithTitle()
    {
        var md = "```csharp title=\"Program.cs\" {2}\nvar x = 1;\nvar y = 2;\n```";
        var html = Render(md);

        html.ShouldContain("ec-mark");
        html.ShouldContain("Program.cs");
        html.ShouldContain("ec-frame");
    }

    // ── Empty marker ranges ───────────────────────────────────────────────────

    [Fact]
    public void ShouldHandleCodeBlockWithNoMarkers()
    {
        var md = "```csharp\nvar x = 1;\n```";
        var html = Render(md);

        // No marker classes at all.
        html.ShouldNotContain("ec-mark");
        html.ShouldNotContain("ec-ins");
        html.ShouldNotContain("ec-del");
        // But normal ec-line structure present.
        html.ShouldContain("ec-line");
    }
}
