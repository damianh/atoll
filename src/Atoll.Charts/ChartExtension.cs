using Markdig;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace Atoll.Charts;

/// <summary>
/// Markdig extension that intercepts fenced code blocks with language identifier
/// <c>chart</c> and renders them as a <c>&lt;div class="atoll-chart"&gt;</c> containing
/// a <c>&lt;canvas data-chart-config="..."&gt;</c> element for Chart.js client-side rendering.
/// All other code blocks are rendered normally.
/// </summary>
public sealed class ChartExtension : IMarkdownExtension
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

        // Capture the current code block renderer as the fallback so that non-chart
        // blocks continue through the existing chain (e.g., syntax highlighting → mermaid
        // → default). This mirrors the chain-of-responsibility pattern used by the other
        // code block extensions.
        // Walk the renderer list looking for the first HtmlObjectRenderer<CodeBlock>
        // that is not our own renderer.
        IMarkdownObjectRenderer? fallback = null;
        foreach (var r in htmlRenderer.ObjectRenderers)
        {
            if (r is HtmlObjectRenderer<CodeBlock> codeRenderer and not ChartCodeBlockRenderer)
            {
                fallback = codeRenderer;
                break;
            }
        }

        fallback ??= new CodeBlockRenderer();

        // Remove the captured fallback so only one dispatcher is active.
        htmlRenderer.ObjectRenderers.Remove(fallback);

        htmlRenderer.ObjectRenderers.Insert(0, new ChartCodeBlockRenderer(fallback));
    }
}
