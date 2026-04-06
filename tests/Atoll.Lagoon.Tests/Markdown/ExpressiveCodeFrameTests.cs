using Atoll.Build.Content.Markdown;
using Atoll.Lagoon.Markdown;
using Shouldly;
using Xunit;

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
    public void ShouldRenderEditorTabInCodeFrame()
    {
        var md = "```csharp\nvar x = 1;\n```";
        var html = Render(md);

        html.ShouldContain("class=\"ec-tab\"");
    }

    [Fact]
    public void ShouldRenderFrameHeaderForDefaultCodeBlock()
    {
        var md = "```csharp\nvar x = 1;\n```";
        var html = Render(md);

        html.ShouldContain("class=\"ec-header\"");
    }

    // ── Title in editor frame ─────────────────────────────────────────────────

    [Fact]
    public void ShouldRenderTitleInEditorFrame()
    {
        var md = "```csharp title=\"Program.cs\"\nvar x = 1;\n```";
        var html = Render(md);

        html.ShouldContain("data-frame=\"code\"");
        html.ShouldContain("class=\"ec-title\"");
        html.ShouldContain("Program.cs");
    }

    [Fact]
    public void ShouldHtmlEscapeTitleContent()
    {
        var md = "```csharp title=\"a<b>c\"\nvar x = 1;\n```";
        var html = Render(md);

        html.ShouldContain("a&lt;b&gt;c");
        html.ShouldNotContain("a<b>c");
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
        html.ShouldContain("class=\"ec-terminal-dots\"");
    }

    [Fact]
    public void ShouldRenderTerminalDotsInTerminalFrame()
    {
        var md = "```bash\necho hello\n```";
        var html = Render(md);

        html.ShouldContain("ec-terminal-dots");
        html.ShouldNotContain("ec-tab");
    }

    [Fact]
    public void ShouldRenderTitleInTerminalFrame()
    {
        // A title forces code (editor) frame even on terminal languages — title takes precedence.
        var md = "```bash title=\"Deploy\"\necho hello\n```";
        var html = Render(md);

        html.ShouldContain("data-frame=\"code\"");
        html.ShouldContain("class=\"ec-title\"");
        html.ShouldContain("Deploy");
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
        html.ShouldContain("ec-terminal-dots");
    }

    [Fact]
    public void ShouldRenderFramelessWhenFrameNone()
    {
        var md = "```csharp frame=\"none\"\nvar x = 1;\n```";
        var html = Render(md);

        html.ShouldNotContain("class=\"ec-frame\"");
        html.ShouldNotContain("class=\"ec-header\"");
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
    public void ShouldUseFigcaptionForHeader()
    {
        var md = "```csharp\nvar x = 1;\n```";
        var html = Render(md);

        html.ShouldContain("<figcaption");
        html.ShouldContain("</figcaption>");
    }

    [Fact]
    public void ShouldPlaceCopyButtonInsideFrameHeader()
    {
        var md = "```csharp\nvar x = 1;\n```";
        var html = Render(md);

        // figcaption should contain the copy button
        var figcaptionStart = html.IndexOf("<figcaption", StringComparison.Ordinal);
        var figcaptionEnd = html.IndexOf("</figcaption>", StringComparison.Ordinal);
        var header = html[figcaptionStart..figcaptionEnd];

        header.ShouldContain("code-copy-btn");
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
        // Even if language would normally be "none" detection case,
        // having a title should force code frame.
        var md = "```csharp title=\"Program.cs\"\nvar x = 1;\n```";
        var html = Render(md);

        html.ShouldContain("data-frame=\"code\"");
    }
}
