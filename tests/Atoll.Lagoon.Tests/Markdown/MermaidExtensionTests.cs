using Atoll.Lagoon.Markdown;

namespace Atoll.Lagoon.Tests.Markdown;

public sealed class MermaidExtensionTests
{
    private static string RenderWithMermaid(string markdown)
    {
        var options = new DocsMarkdownOptions { EnableMermaid = true };
        var result = DocsMarkdownRenderer.Render(markdown, options);
        return result.Html;
    }

    private static string RenderWithoutMermaid(string markdown)
    {
        var options = new DocsMarkdownOptions { EnableMermaid = false };
        var result = DocsMarkdownRenderer.Render(markdown, options);
        return result.Html;
    }

    // --- Mermaid blocks ---

    [Fact]
    public void ShouldRenderMermaidBlockAsMermaidPre()
    {
        var md = "```mermaid\ngraph TD;\nA-->B;\n```";
        var html = RenderWithMermaid(md);

        html.ShouldContain("<pre class=\"mermaid\">");
        html.ShouldContain("graph TD;");
        // '>' is HTML-encoded to '&gt;' in the mermaid pre block for XSS safety.
        // Mermaid JS reads textContent, so &gt; is decoded back to > before diagram rendering.
        html.ShouldContain("A--&gt;B;");
    }

    [Fact]
    public void ShouldNotContainCodeTagForMermaidBlock()
    {
        var md = "```mermaid\ngraph TD;\nA-->B;\n```";
        var html = RenderWithMermaid(md);

        html.ShouldNotContain("<code");
        html.ShouldNotContain("language-mermaid");
    }

    [Fact]
    public void ShouldRenderMermaidBlockCaseInsensitive()
    {
        var md = "```Mermaid\ngraph TD;\nA-->B;\n```";
        var html = RenderWithMermaid(md);

        html.ShouldContain("<pre class=\"mermaid\">");
    }

    // --- Non-mermaid blocks ---

    [Fact]
    public void ShouldRenderNonMermaidCodeBlockNormally()
    {
        var md = "```csharp\nvar x = 1;\n```";
        var html = RenderWithMermaid(md);

        html.ShouldNotContain("class=\"mermaid\"");
        html.ShouldContain("<code");
        html.ShouldContain("var x = 1;");
    }

    [Fact]
    public void ShouldRenderCodeBlockWithoutLanguageNormally()
    {
        var md = "```\nplain code\n```";
        var html = RenderWithMermaid(md);

        html.ShouldNotContain("class=\"mermaid\"");
        html.ShouldContain("plain code");
    }

    // --- Without Mermaid extension ---

    [Fact]
    public void ShouldRenderMermaidAsRegularCodeWhenExtensionDisabled()
    {
        var md = "```mermaid\ngraph TD;\nA-->B;\n```";
        var html = RenderWithoutMermaid(md);

        html.ShouldNotContain("<pre class=\"mermaid\">");
        html.ShouldContain("<code");
    }

    // --- Multiple blocks ---

    [Fact]
    public void ShouldHandleMixedMermaidAndCodeBlocks()
    {
        var md = "```mermaid\ngraph TD;\nA-->B;\n```\n\n```csharp\nvar x = 1;\n```";
        var html = RenderWithMermaid(md);

        html.ShouldContain("<pre class=\"mermaid\">");
        html.ShouldContain("<code");
        html.ShouldContain("graph TD;");
        html.ShouldContain("var x = 1;");
    }
}
