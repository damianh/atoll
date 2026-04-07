using Atoll.Lagoon.Markdown;

namespace Atoll.Lagoon.Tests.Markdown;

public sealed class ExpressiveCodeDiffTests
{
    private static string Render(string markdown)
    {
        var options = new DocsMarkdownOptions { EnableSyntaxHighlighting = true };
        return DocsMarkdownRenderer.Render(markdown, options).Html;
    }

    // ── diff with lang= ───────────────────────────────────────────────────────

    [Fact]
    public void ShouldApplyInsClassToPlusLines()
    {
        var md = "```diff lang=\"csharp\"\n+var x = 1;\n```";
        var html = Render(md);

        html.ShouldContain("ec-ins");
    }

    [Fact]
    public void ShouldApplyDelClassToMinusLines()
    {
        var md = "```diff lang=\"csharp\"\n-var x = 1;\n```";
        var html = Render(md);

        html.ShouldContain("ec-del");
    }

    [Fact]
    public void ShouldNotMarkNeutralLines()
    {
        var md = "```diff lang=\"csharp\"\n var x = 1;\n```";
        var html = Render(md);

        html.ShouldNotContain("ec-ins");
        html.ShouldNotContain("ec-del");
        html.ShouldNotContain("ec-mark");
    }

    [Fact]
    public void ShouldStripDiffPrefixFromDisplayedText()
    {
        var md = "```diff lang=\"csharp\"\n+var x = 1;\n```";
        var html = Render(md);

        // The '+' prefix should NOT appear in code content — it's stripped.
        // We check by verifying the syntax-highlighted 'var' keyword is present (not just '+var').
        html.ShouldContain("tm-keyword");
    }

    [Fact]
    public void ShouldSyntaxHighlightWithUnderlyingLanguage()
    {
        var md = "```diff lang=\"csharp\"\n+var x = 1;\n```";
        var html = Render(md);

        html.ShouldContain("language-csharp");
        html.ShouldContain("tm-keyword");
    }

    [Fact]
    public void ShouldHandleMixedPlusMinusNeutralLines()
    {
        var md = "```diff lang=\"csharp\"\n var a = 1;\n+var b = 2;\n-var c = 3;\n```";
        var html = Render(md);

        html.ShouldContain("ec-ins");
        html.ShouldContain("ec-del");
    }

    // ── diff with title ───────────────────────────────────────────────────────

    [Fact]
    public void ShouldRenderDiffWithTitle()
    {
        var md = "```diff lang=\"csharp\" title=\"Changes.cs\"\n+var x = 1;\n```";
        var html = Render(md);

        html.ShouldContain("ec-ins");
        html.ShouldContain("Changes.cs");
        html.ShouldContain("ec-frame");
    }

    // ── diff without lang= ────────────────────────────────────────────────────

    [Fact]
    public void ShouldRenderPlainDiffWithoutLangAsFallback()
    {
        // Plain diff without lang= falls back to the fallback renderer (no syntax highlighting).
        var md = "```diff\n+added line\n-removed line\n```";
        var html = Render(md);

        // Should render something — no exception.
        html.ShouldNotBeNullOrEmpty();
    }

    // ── diff frame ────────────────────────────────────────────────────────────

    [Fact]
    public void ShouldRenderDefaultCodeFrameForDiff()
    {
        var md = "```diff lang=\"csharp\"\n+var x = 1;\n```";
        var html = Render(md);

        html.ShouldContain("ec-frame");
    }
}
