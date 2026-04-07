using Atoll.Lagoon.Markdown;
using Shouldly;
using Xunit;

namespace Atoll.Lagoon.Tests.Markdown;

/// <summary>
/// End-to-end integration tests that verify combinations of Expressive Code features
/// working together, backward compatibility, and mermaid coexistence.
/// </summary>
public sealed class ExpressiveCodeIntegrationTests
{
    private static string Render(string markdown)
    {
        var options = new DocsMarkdownOptions { EnableSyntaxHighlighting = true };
        return DocsMarkdownRenderer.Render(markdown, options).Html;
    }

    // ── backward compatibility ────────────────────────────────────────────────

    [Fact]
    public void ShouldRenderCodeBlockWithNoAttributesAsDefaultCodeFrame()
    {
        var md = "```csharp\nvar x = 1;\n```";
        var html = Render(md);

        // Should get a default code frame.
        html.ShouldContain("ec-frame");
        html.ShouldContain("data-frame=\"code\"");
        html.ShouldContain("ec-line");
        html.ShouldContain("language-csharp");
    }

    [Fact]
    public void ShouldRenderMermaidDiagramsWithoutExpressiveCodeFeatures()
    {
        var md = "```mermaid\ngraph TD; A-->B;\n```";
        var options = new DocsMarkdownOptions
        {
            EnableSyntaxHighlighting = true,
            EnableMermaid = true,
        };
        var html = DocsMarkdownRenderer.Render(md, options).Html;

        // Mermaid blocks should not gain ec-frame wrappers.
        html.ShouldContain("mermaid");
        html.ShouldNotContain("ec-frame");
    }

    // ── feature combinations ──────────────────────────────────────────────────

    [Fact]
    public void ShouldRenderFrameWithLineMarkersAndInlineMarkersAndLineNumbers()
    {
        var md = "```csharp title=\"app.cs\" ins={1} showLineNumbers\nvar x = 1;\nvar y = 2;\n```";
        var html = Render(md);

        html.ShouldContain("ec-frame");
        html.ShouldContain("app.cs");
        html.ShouldContain("ec-ins");
        html.ShouldContain("data-line-numbers");
    }

    [Fact]
    public void ShouldRenderDiffWithCollapse()
    {
        var md = "```diff lang=\"csharp\" collapse={3-4}\n var a = 1;\n var b = 2;\n var c = 3;\n var d = 4;\n```";
        var html = Render(md);

        html.ShouldContain("ec-collapse-group");
        html.ShouldContain("ec-frame");
    }

    [Fact]
    public void ShouldRenderTerminalFrameWithWrap()
    {
        var md = "```bash wrap\necho 'a very long command that might wrap on narrow screens'\n```";
        var html = Render(md);

        html.ShouldContain("data-frame=\"terminal\"");
        html.ShouldContain("data-wrap");
    }

    [Fact]
    public void ShouldRenderAllFeaturesSimultaneously()
    {
        var md = """
            ```csharp title="demo.cs" ins={1} del={2} collapse={3-4} showLineNumbers wrap
            var a = 1;
            var b = 2;
            var c = 3;
            var d = 4;
            var e = 5;
            ```
            """;
        var html = Render(md);

        html.ShouldContain("ec-frame");
        html.ShouldContain("demo.cs");
        html.ShouldContain("ec-ins");
        html.ShouldContain("ec-del");
        html.ShouldContain("ec-collapse-group");
        html.ShouldContain("data-line-numbers");
        html.ShouldContain("data-wrap");
    }

    // ── no regressions in existing tests ─────────────────────────────────────

    [Fact]
    public void ShouldStillContainEcLineWrappersOnAllLines()
    {
        var md = "```csharp\nline1;\nline2;\nline3;\n```";
        var html = Render(md);

        // Three lines → three ec-line divs.
        var count = 0;
        var idx = 0;
        while ((idx = html.IndexOf("ec-line", idx, StringComparison.Ordinal)) >= 0)
        {
            // Only count opening divs (class="ec-line"), not the content divs.
            var tagStart = html.LastIndexOf('<', idx);
            if (html[tagStart..idx].Contains("div"))
            {
                count++;
            }

            idx++;
        }

        count.ShouldBeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public void ShouldRenderCopyButtonInFrameHeader()
    {
        var md = "```csharp title=\"file.cs\"\nvar x = 1;\n```";
        var html = Render(md);

        html.ShouldContain("ec-header");
        html.ShouldContain("code-copy-btn");

        // Copy button should be inside the header (before </figcaption>).
        var headerEnd = html.IndexOf("</figcaption>", StringComparison.Ordinal);
        var copyBtnPos = html.IndexOf("code-copy-btn", StringComparison.Ordinal);
        copyBtnPos.ShouldBeLessThan(headerEnd);
    }

    [Fact]
    public void ShouldRenderCopyButtonAfterPreForFrameNone()
    {
        var md = "```csharp frame=\"none\"\nvar x = 1;\n```";
        var html = Render(md);

        html.ShouldContain("code-copy-btn");
        html.ShouldNotContain("ec-header");

        // For frame=none, button should appear after </pre>.
        var preEnd = html.IndexOf("</pre>", StringComparison.Ordinal);
        var copyBtnPos = html.IndexOf("code-copy-btn", StringComparison.Ordinal);
        copyBtnPos.ShouldBeGreaterThan(preEnd);
    }
}
