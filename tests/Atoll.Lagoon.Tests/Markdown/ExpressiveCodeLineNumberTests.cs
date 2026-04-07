using Atoll.Lagoon.Markdown;
using Shouldly;
using Xunit;

namespace Atoll.Lagoon.Tests.Markdown;

public sealed class ExpressiveCodeLineNumberTests
{
    private static string Render(string markdown)
    {
        var options = new DocsMarkdownOptions { EnableSyntaxHighlighting = true };
        return DocsMarkdownRenderer.Render(markdown, options).Html;
    }

    // ── data-line-numbers attribute ───────────────────────────────────────────

    [Fact]
    public void ShouldAddDataLineNumbersAttributeWhenShowLineNumbers()
    {
        var md = "```csharp showLineNumbers\nvar x = 1;\n```";
        var html = Render(md);

        html.ShouldContain("data-line-numbers");
    }

    [Fact]
    public void ShouldNotAddDataLineNumbersWhenNotSpecified()
    {
        var md = "```csharp\nvar x = 1;\n```";
        var html = Render(md);

        html.ShouldNotContain("data-line-numbers");
    }

    [Fact]
    public void ShouldNotAddCounterResetStyleWhenStartIsOne()
    {
        // showLineNumbers with default start=1 should NOT emit counter-reset style.
        var md = "```csharp showLineNumbers\nvar x = 1;\n```";
        var html = Render(md);

        html.ShouldContain("data-line-numbers");
        html.ShouldNotContain("counter-reset");
    }

    [Fact]
    public void ShouldAddCounterResetStyleWhenStartLineIsNotOne()
    {
        var md = "```csharp showLineNumbers startLineNumber=5\nvar x = 1;\n```";
        var html = Render(md);

        // counter-reset: ec-line-num 4  (start - 1 = 5 - 1 = 4)
        html.ShouldContain("data-line-numbers");
        html.ShouldContain("counter-reset:ec-line-num 4");
    }

    [Fact]
    public void ShouldAddDataLineNumbersToFigureElement()
    {
        var md = "```csharp showLineNumbers title=\"file.cs\"\nvar x = 1;\n```";
        var html = Render(md);

        html.ShouldContain("ec-frame");
        html.ShouldContain("data-line-numbers");
    }

    [Fact]
    public void ShouldAddDataLineNumbersToWrapperDivForFrameNone()
    {
        var md = "```csharp showLineNumbers frame=\"none\"\nvar x = 1;\n```";
        var html = Render(md);

        html.ShouldContain("code-block-wrapper");
        html.ShouldContain("data-line-numbers");
    }

    // ── combination with other features ──────────────────────────────────────

    [Fact]
    public void ShouldCombineLineNumbersWithLineMarkers()
    {
        var md = "```csharp showLineNumbers ins={1}\nvar x = 1;\nvar y = 2;\n```";
        var html = Render(md);

        html.ShouldContain("data-line-numbers");
        html.ShouldContain("ec-ins");
    }

    [Fact]
    public void ShouldCombineLineNumbersWithCollapse()
    {
        var md = "```csharp showLineNumbers collapse={1-2}\nvar a = 1;\nvar b = 2;\nvar c = 3;\n```";
        var html = Render(md);

        html.ShouldContain("data-line-numbers");
        html.ShouldContain("ec-collapse-group");
    }
}
