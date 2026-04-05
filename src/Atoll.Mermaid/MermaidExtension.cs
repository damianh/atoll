using Markdig;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace Atoll.Mermaid;

/// <summary>
/// Markdig extension that intercepts fenced code blocks with language identifier
/// <c>mermaid</c> and renders them as <c>&lt;pre class="mermaid"&gt;</c> instead of
/// the default <c>&lt;pre&gt;&lt;code class="language-mermaid"&gt;</c>.
/// This is the format expected by the Mermaid JS library for client-side rendering.
/// All other code blocks are rendered normally.
/// </summary>
public sealed class MermaidExtension : IMarkdownExtension
{
    void IMarkdownExtension.Setup(MarkdownPipelineBuilder pipeline)
    {
        // Ensure fenced code blocks are enabled.
        if (!pipeline.BlockParsers.Contains<FencedCodeBlockParser>())
        {
            pipeline.BlockParsers.Insert(0, new FencedCodeBlockParser());
        }
    }

    void IMarkdownExtension.Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
        if (renderer is not HtmlRenderer htmlRenderer)
        {
            return;
        }

        // Replace the existing CodeBlockRenderer with our Mermaid-aware version.
        var existing = htmlRenderer.ObjectRenderers.FindExact<CodeBlockRenderer>();
        if (existing is not null)
        {
            htmlRenderer.ObjectRenderers.Remove(existing);
        }

        htmlRenderer.ObjectRenderers.Insert(0, new MermaidCodeBlockRenderer());
    }
}
