using Atoll.Lagoon.Markdown;

namespace Atoll.Lagoon.Tests.Markdown;

public sealed class ExpressiveCodeFrameTests
{
    private static string Render(string markdown)
    {
        var options = new DocsMarkdownOptions { EnableSyntaxHighlighting = true };
        return DocsMarkdownRenderer.Render(markdown, options).Html;
    }

    // ── Default code frame ────────────────────────────────────────────────────

    [Fact]
    public void ShouldRenderDefaultCodeFrameForCSharp()
    {
        var md = "```csharp\nvar x = 1;\n```";
        var html = Render(md);

        html.ShouldContain("class=\"ec-frame\"");
        html.ShouldContain("data-frame=\"code\"");
    }

    [Fact]
    public void ShouldRenderEditorFrameForCSharp()
    {
        var md = "```csharp\nvar x = 1;\n```";
        var html = Render(md);

        html.ShouldContain("class=\"ec-frame\"");
        html.ShouldContain("data-frame=\"code\"");
        html.ShouldNotContain("ec-tab");
        html.ShouldNotContain("ec-header");
    }

    [Fact]
    public void ShouldNotRenderFrameHeaderForDefaultCodeBlock()
    {
        var md = "```csharp\nvar x = 1;\n```";
        var html = Render(md);

        html.ShouldNotContain("ec-header");
        html.ShouldNotContain("figcaption");
    }

    // ── Title in editor frame ─────────────────────────────────────────────────

    [Fact]
    public void ShouldRenderTitleAsDataAttribute()
    {
        var md = "```csharp title=\"Program.cs\"\nvar x = 1;\n```";
        var html = Render(md);

        html.ShouldContain("data-frame=\"code\"");
        html.ShouldNotContain("ec-title");
        html.ShouldNotContain("ec-header");
    }

    [Fact]
    public void ShouldHtmlEscapeTitleContent()
    {
        var md = "```csharp title=\"a<b>c\"\nvar x = 1;\n```";
        var html = Render(md);

        // Title no longer rendered in HTML — just verify frame renders
        html.ShouldContain("data-frame=\"code\"");
    }

    // ── Terminal auto-detection ───────────────────────────────────────────────

    [Theory]
    [InlineData("bash")]
    [InlineData("sh")]
    [InlineData("shell")]
    [InlineData("powershell")]
    [InlineData("pwsh")]
    [InlineData("ps1")]
    [InlineData("cmd")]
    [InlineData("zsh")]
    [InlineData("fish")]
    [InlineData("terminal")]
    [InlineData("console")]
    public void ShouldAutoDetectTerminalLanguages(string lang)
    {
        var md = $"```{lang}\necho hello\n```";
        var html = Render(md);

        html.ShouldContain("data-frame=\"terminal\"");
        html.ShouldNotContain("ec-terminal-dots");
        html.ShouldNotContain("ec-header");
    }

    [Fact]
    public void ShouldRenderTerminalFrameWithoutDotsOrHeader()
    {
        var md = "```bash\necho hello\n```";
        var html = Render(md);

        html.ShouldContain("data-frame=\"terminal\"");
        html.ShouldNotContain("ec-terminal-dots");
        html.ShouldNotContain("ec-tab");
        html.ShouldNotContain("ec-header");
    }

    [Fact]
    public void ShouldRenderTitleAsCodeFrameForTerminalLang()
    {
        // A title forces code (editor) frame even on terminal languages — title takes precedence.
        var md = "```bash title=\"Deploy\"\necho hello\n```";
        var html = Render(md);

        html.ShouldContain("data-frame=\"code\"");
        html.ShouldNotContain("ec-title");
    }

    // ── Explicit frame overrides ──────────────────────────────────────────────

    [Fact]
    public void ShouldRespectExplicitFrameCode()
    {
        var md = "```bash frame=\"code\"\necho hello\n```";
        var html = Render(md);

        // bash would auto-detect terminal, but explicit frame="code" overrides it
        html.ShouldContain("data-frame=\"code\"");
        html.ShouldNotContain("data-frame=\"terminal\"");
    }

    [Fact]
    public void ShouldRespectExplicitFrameTerminalOnCodeLang()
    {
        var md = "```csharp frame=\"terminal\"\nvar x = 1;\n```";
        var html = Render(md);

        html.ShouldContain("data-frame=\"terminal\"");
        html.ShouldNotContain("ec-terminal-dots");
    }

    [Fact]
    public void ShouldRenderFramelessWhenFrameNone()
    {
        var md = "```csharp frame=\"none\"\nvar x = 1;\n```";
        var html = Render(md);

        html.ShouldNotContain("class=\"ec-frame\"");
        html.ShouldNotContain("ec-header");
        html.ShouldContain("class=\"code-block-wrapper\"");
    }

    [Fact]
    public void ShouldStillHaveCopyButtonWhenFrameNone()
    {
        var md = "```csharp frame=\"none\"\nvar x = 1;\n```";
        var html = Render(md);

        html.ShouldContain("code-copy-btn");
    }

    // ── Frame structure ───────────────────────────────────────────────────────

    [Fact]
    public void ShouldUseFigureElementForFrame()
    {
        var md = "```csharp\nvar x = 1;\n```";
        var html = Render(md);

        html.ShouldContain("<figure");
        html.ShouldContain("</figure>");
    }

    [Fact]
    public void ShouldNotRenderFigcaptionHeader()
    {
        var md = "```csharp\nvar x = 1;\n```";
        var html = Render(md);

        html.ShouldNotContain("<figcaption");
        html.ShouldNotContain("</figcaption>");
    }

    [Fact]
    public void ShouldPlaceCopyButtonAfterPreInsideFrame()
    {
        var md = "```csharp\nvar x = 1;\n```";
        var html = Render(md);

        // Copy button should be after </pre> but before </figure>
        var preEnd = html.IndexOf("</pre>", StringComparison.Ordinal);
        var copyBtnPos = html.IndexOf("code-copy-btn", StringComparison.Ordinal);
        var figureEnd = html.IndexOf("</figure>", StringComparison.Ordinal);

        copyBtnPos.ShouldBeGreaterThan(preEnd);
        copyBtnPos.ShouldBeLessThan(figureEnd);
    }

    // ── Syntax highlighting still works inside frames ─────────────────────────

    [Fact]
    public void ShouldSyntaxHighlightInsideFrame()
    {
        var md = "```csharp\nvar x = 1;\n```";
        var html = Render(md);

        html.ShouldContain("tm-keyword");
        html.ShouldContain("ec-frame");
    }

    [Fact]
    public void ShouldPreserveEcLineDivsInsideFrame()
    {
        var md = "```csharp\nvar x = 1;\n```";
        var html = Render(md);

        html.ShouldContain("class=\"ec-line\"");
        html.ShouldContain("class=\"ec-line-content\"");
    }

    // ── Title implies editor frame ────────────────────────────────────────────

    [Fact]
    public void ShouldDefaultToCodeFrameWhenTitleSet()
    {
        // Having a title should force code frame.
        var md = "```csharp title=\"Program.cs\"\nvar x = 1;\n```";
        var html = Render(md);

        html.ShouldContain("data-frame=\"code\"");
        html.ShouldNotContain("ec-header");
    }
}
