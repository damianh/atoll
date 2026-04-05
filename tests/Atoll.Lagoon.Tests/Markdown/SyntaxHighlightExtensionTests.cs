using Atoll.Lagoon.Markdown;
using Shouldly;
using Xunit;

namespace Atoll.Lagoon.Tests.Markdown;

public sealed class SyntaxHighlightExtensionTests
{
    private static string RenderWithHighlighting(string markdown)
    {
        var options = new DocsMarkdownOptions { EnableSyntaxHighlighting = true };
        return DocsMarkdownRenderer.Render(markdown, options).Html;
    }

    private static string RenderWithBothExtensions(string markdown)
    {
        var options = new DocsMarkdownOptions
        {
            EnableMermaid = true,
            EnableSyntaxHighlighting = true,
        };
        return DocsMarkdownRenderer.Render(markdown, options).Html;
    }

    private static string RenderWithoutHighlighting(string markdown)
    {
        var options = new DocsMarkdownOptions { EnableSyntaxHighlighting = false };
        return DocsMarkdownRenderer.Render(markdown, options).Html;
    }

    // --- C# keyword highlighting ---

    [Fact]
    public void ShouldHighlightCSharpKeywords()
    {
        var md = "```csharp\nvar x = 1;\n```";
        var html = RenderWithHighlighting(md);

        html.ShouldContain("tm-keyword");
        html.ShouldContain("var");
    }

    [Fact]
    public void ShouldHighlightCSharpStrings()
    {
        var md = "```csharp\nvar s = \"hello\";\n```";
        var html = RenderWithHighlighting(md);

        html.ShouldContain("tm-string");
    }

    [Fact]
    public void ShouldHighlightCSharpComments()
    {
        var md = "```csharp\n// a comment\n```";
        var html = RenderWithHighlighting(md);

        html.ShouldContain("tm-comment");
    }

    // --- HTML structure ---

    [Fact]
    public void ShouldRenderPreWithHighlightClass()
    {
        var md = "```csharp\nvar x = 1;\n```";
        var html = RenderWithHighlighting(md);

        html.ShouldContain("<pre class=\"highlight\">");
    }

    [Fact]
    public void ShouldRenderCodeWithLanguageClass()
    {
        var md = "```csharp\nvar x = 1;\n```";
        var html = RenderWithHighlighting(md);

        html.ShouldContain("<code class=\"language-csharp\">");
    }

    // --- Fallback for unrecognized or absent language ---

    [Fact]
    public void ShouldFallBackForUnrecognizedLanguage()
    {
        var md = "```foobar\nsome text\n```";
        var html = RenderWithHighlighting(md);

        html.ShouldNotContain("tm-");
        html.ShouldNotContain("class=\"highlight\"");
        html.ShouldContain("some text");
    }

    [Fact]
    public void ShouldFallBackForNoLanguage()
    {
        var md = "```\nplain code\n```";
        var html = RenderWithHighlighting(md);

        html.ShouldNotContain("tm-");
        html.ShouldNotContain("class=\"highlight\"");
        html.ShouldContain("plain code");
    }

    // --- Disabled ---

    [Fact]
    public void ShouldNotHighlightWhenDisabled()
    {
        var md = "```csharp\nvar x = 1;\n```";
        var html = RenderWithoutHighlighting(md);

        html.ShouldNotContain("tm-");
        html.ShouldNotContain("class=\"highlight\"");
        html.ShouldContain("var x = 1;");
    }

    // --- Mermaid coexistence ---

    [Fact]
    public void ShouldPreserveMermaidWhenBothEnabled()
    {
        var md = "```mermaid\ngraph TD;\nA-->B;\n```";
        var html = RenderWithBothExtensions(md);

        html.ShouldContain("<pre class=\"mermaid\">");
        html.ShouldNotContain("class=\"highlight\"");
        html.ShouldNotContain("tm-");
    }

    [Fact]
    public void ShouldHighlightCodeBlocksAlongsideMermaid()
    {
        var md = "```mermaid\ngraph TD;\nA-->B;\n```\n\n```csharp\nvar x = 1;\n```";
        var html = RenderWithBothExtensions(md);

        html.ShouldContain("<pre class=\"mermaid\">");
        html.ShouldContain("tm-keyword");
        html.ShouldContain("graph TD;");
        html.ShouldContain("var");
    }

    // --- XSS safety ---

    [Fact]
    public void ShouldHtmlEncodeCodeContent()
    {
        var md = "```csharp\nif (x < 5) {}\n```";
        var html = RenderWithHighlighting(md);

        // '<' must be escaped
        html.ShouldContain("&lt;");
        html.ShouldNotContain("<5");
    }

    // --- Edge cases ---

    [Fact]
    public void ShouldHandleEmptyCodeBlock()
    {
        var md = "```csharp\n\n```";

        // Should not throw
        var html = RenderWithHighlighting(md);

        html.ShouldContain("<pre class=\"highlight\">");
    }

    // --- Language aliases ---

    [Fact]
    public void ShouldSupportCsAlias()
    {
        var md = "```cs\nvar x = 1;\n```";
        var html = RenderWithHighlighting(md);

        html.ShouldContain("tm-keyword");
        html.ShouldContain("var");
    }

    [Fact]
    public void ShouldSupportJavaScriptHighlighting()
    {
        var md = "```javascript\nconst x = 1;\n```";
        var html = RenderWithHighlighting(md);

        html.ShouldContain("tm-keyword");
    }
}
