using Markdig;

namespace Atoll.Mermaid.Tests;

public sealed class MermaidExtensionTests
{
    private static string RenderDirect(string markdown, bool enableMermaid = true)
    {
        var builder = new MarkdownPipelineBuilder();
        if (enableMermaid)
        {
            builder.Use<MermaidExtension>();
        }
        var pipeline = builder.Build();
        return Markdig.Markdown.ToHtml(markdown, pipeline);
    }

    // --- Mermaid blocks ---

    [Fact]
    public void ShouldRenderMermaidBlockAsMermaidPre()
    {
        var md = "```mermaid\ngraph TD;\nA-->B;\n```";
        var html = RenderDirect(md);

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
        var html = RenderDirect(md);

        html.ShouldNotContain("<code");
        html.ShouldNotContain("language-mermaid");
    }

    [Fact]
    public void ShouldRenderMermaidBlockCaseInsensitive()
    {
        var md = "```Mermaid\ngraph TD;\nA-->B;\n```";
        var html = RenderDirect(md);

        html.ShouldContain("<pre class=\"mermaid\">");
    }

    // --- Non-mermaid blocks ---

    [Fact]
    public void ShouldRenderNonMermaidCodeBlockNormally()
    {
        var md = "```csharp\nvar x = 1;\n```";
        var html = RenderDirect(md);

        html.ShouldNotContain("class=\"mermaid\"");
        html.ShouldContain("<code");
        html.ShouldContain("var x = 1;");
    }

    [Fact]
    public void ShouldRenderCodeBlockWithoutLanguageNormally()
    {
        var md = "```\nplain code\n```";
        var html = RenderDirect(md);

        html.ShouldNotContain("class=\"mermaid\"");
        html.ShouldContain("plain code");
    }

    // --- Without Mermaid extension ---

    [Fact]
    public void ShouldRenderMermaidAsRegularCodeWhenExtensionDisabled()
    {
        var md = "```mermaid\ngraph TD;\nA-->B;\n```";
        var html = RenderDirect(md, enableMermaid: false);

        html.ShouldNotContain("<pre class=\"mermaid\">");
        html.ShouldContain("<code");
    }

    // --- Multiple blocks ---

    [Fact]
    public void ShouldHandleMixedMermaidAndCodeBlocks()
    {
        var md = "```mermaid\ngraph TD;\nA-->B;\n```\n\n```csharp\nvar x = 1;\n```";
        var html = RenderDirect(md);

        html.ShouldContain("<pre class=\"mermaid\">");
        html.ShouldContain("<code");
        html.ShouldContain("graph TD;");
        html.ShouldContain("var x = 1;");
    }

    // --- Extension standalone usage ---

    [Fact]
    public void ShouldBeUsableAsStandalonePipelineExtension()
    {
        var pipeline = new MarkdownPipelineBuilder()
            .Use<MermaidExtension>()
            .Build();

        var md = "```mermaid\ngraph TD;\nA-->B;\n```";
        var html = Markdig.Markdown.ToHtml(md, pipeline);

        html.ShouldContain("<pre class=\"mermaid\">");
    }

    // --- XSS safety ---

    [Fact]
    public void ShouldHtmlEncodeMermaidContent()
    {
        var md = "```mermaid\ngraph TD;\nA-->\">B;\n```";
        var html = RenderDirect(md);

        // Both " and > in diagram source must be HTML-encoded for XSS safety.
        // Mermaid JS reads textContent, so entities are decoded before rendering.
        html.ShouldContain("&quot;");
        html.ShouldContain("&gt;");
        // The raw unencoded characters must not appear inside the mermaid pre content.
        // Extract content between the pre tags to avoid matching HTML attribute syntax.
        var preStart = html.IndexOf("<pre class=\"mermaid\">", StringComparison.Ordinal) + "<pre class=\"mermaid\">".Length;
        var preEnd = html.IndexOf("</pre>", preStart, StringComparison.Ordinal);
        var preContent = html[preStart..preEnd];
        preContent.ShouldNotContain("\"");
        preContent.ShouldNotContain(">");
    }
}
