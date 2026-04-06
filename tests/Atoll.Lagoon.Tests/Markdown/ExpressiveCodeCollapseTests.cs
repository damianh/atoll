using Atoll.Build.Content.Markdown;
using Atoll.Lagoon.Markdown;
using Shouldly;
using Xunit;

namespace Atoll.Lagoon.Tests.Markdown;

public sealed class ExpressiveCodeCollapseTests
{
    private static string Render(string markdown)
    {
        var options = new DocsMarkdownOptions { EnableSyntaxHighlighting = true };
        return DocsMarkdownRenderer.Render(markdown, options).Html;
    }

    // ── single collapse range ─────────────────────────────────────────────────

    [Fact]
    public void ShouldRenderDetailsElementForCollapseRange()
    {
        var md = "```csharp collapse={1-3}\nvar a = 1;\nvar b = 2;\nvar c = 3;\n```";
        var html = Render(md);

        html.ShouldContain("ec-collapse-group");
        html.ShouldContain("ec-collapse-summary");
    }

    [Fact]
    public void ShouldShowCorrectLineCountInSummary()
    {
        var md = "```csharp collapse={1-5}\nvar a = 1;\nvar b = 2;\nvar c = 3;\nvar d = 4;\nvar e = 5;\n```";
        var html = Render(md);

        html.ShouldContain("5 collapsed lines");
    }

    [Fact]
    public void ShouldUseSingularLabelWhenOneLineCollapsed()
    {
        var md = "```csharp collapse={2-2}\nvar a = 1;\nvar b = 2;\nvar c = 3;\n```";
        var html = Render(md);

        html.ShouldContain("1 collapsed line");
        html.ShouldNotContain("1 collapsed lines");
    }

    [Fact]
    public void ShouldWrapCollapsedLinesInCollapseContent()
    {
        var md = "```csharp collapse={1-2}\nvar a = 1;\nvar b = 2;\nvar c = 3;\n```";
        var html = Render(md);

        html.ShouldContain("ec-collapse-content");
        // The third line should NOT be inside the collapse content.
        // Verify details closes before remaining lines.
        var detailsStart = html.IndexOf("<details", StringComparison.Ordinal);
        var detailsEnd = html.IndexOf("</details>", StringComparison.Ordinal);
        detailsStart.ShouldBeGreaterThan(-1);
        detailsEnd.ShouldBeGreaterThan(detailsStart);
    }

    [Fact]
    public void ShouldRenderLinesOutsideCollapseNormally()
    {
        var md = "```csharp collapse={1-1}\nvar a = 1;\nvar b = 2;\n```";
        var html = Render(md);

        // Second line is outside collapse — should have a normal ec-line div after </details>
        var detailsEnd = html.IndexOf("</details>", StringComparison.Ordinal);
        var afterDetails = html[(detailsEnd + 10)..];
        afterDetails.ShouldContain("ec-line");
    }

    // ── multiple collapse ranges ───────────────────────────────────────────────

    [Fact]
    public void ShouldRenderMultipleCollapseRanges()
    {
        var md = "```csharp collapse={1-2,4-5}\nvar a = 1;\nvar b = 2;\nvar c = 3;\nvar d = 4;\nvar e = 5;\n```";
        var html = Render(md);

        // Should have two separate details elements.
        var firstDetails = html.IndexOf("<details", StringComparison.Ordinal);
        var secondDetails = html.IndexOf("<details", firstDetails + 1);
        firstDetails.ShouldBeGreaterThan(-1);
        secondDetails.ShouldBeGreaterThan(firstDetails);
    }

    [Fact]
    public void ShouldShowCorrectCountForEachRange()
    {
        var md = "```csharp collapse={1-2,4-6}\nvar a = 1;\nvar b = 2;\nvar c = 3;\nvar d = 4;\nvar e = 5;\nvar f = 6;\n```";
        var html = Render(md);

        html.ShouldContain("2 collapsed lines");
        html.ShouldContain("3 collapsed lines");
    }

    // ── collapse combined with other features ─────────────────────────────────

    [Fact]
    public void ShouldRenderCollapseWithTitle()
    {
        var md = "```csharp title=\"example.cs\" collapse={1-2}\nvar a = 1;\nvar b = 2;\nvar c = 3;\n```";
        var html = Render(md);

        html.ShouldContain("ec-frame");
        html.ShouldContain("example.cs");
        html.ShouldContain("ec-collapse-group");
        html.ShouldContain("2 collapsed lines");
    }

    [Fact]
    public void ShouldRenderCollapseWithLineMarkersOnCollapsedLines()
    {
        var md = "```csharp collapse={1-2} ins={1}\nvar a = 1;\nvar b = 2;\nvar c = 3;\n```";
        var html = Render(md);

        // Collapsed content should still have the ins marker class on the first line.
        html.ShouldContain("ec-collapse-group");
        html.ShouldContain("ec-ins");
    }

    [Fact]
    public void ShouldNotRenderCollapseGroupWhenNoCollapseAttribute()
    {
        var md = "```csharp\nvar a = 1;\n```";
        var html = Render(md);

        html.ShouldNotContain("ec-collapse-group");
        html.ShouldNotContain("ec-collapse-summary");
    }
}
