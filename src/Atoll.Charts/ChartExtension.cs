using Markdig;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Renderers.Html;

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

        // Replace the existing CodeBlockRenderer with our Chart-aware version.
        var existing = htmlRenderer.ObjectRenderers.FindExact<CodeBlockRenderer>();
        if (existing is not null)
        {
            htmlRenderer.ObjectRenderers.Remove(existing);
        }

        htmlRenderer.ObjectRenderers.Insert(0, new ChartCodeBlockRenderer());
    }
}
