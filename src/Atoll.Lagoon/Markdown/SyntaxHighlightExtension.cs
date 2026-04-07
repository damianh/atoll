using Atoll.Mermaid;
using Markdig;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace Atoll.Lagoon.Markdown;

/// <summary>
/// Markdig extension that intercepts fenced code blocks with recognized language
/// identifiers and renders them with server-side syntax highlighting using TextMate
/// grammars, producing <c>&lt;span&gt;</c> elements with semantic CSS class names.
/// Code blocks with unrecognized or absent language identifiers are forwarded to the
/// previously registered <see cref="CodeBlockRenderer"/> (or a Mermaid-aware renderer
/// when <c>MermaidExtension</c> is also active).
/// </summary>
public sealed class SyntaxHighlightExtension : IMarkdownExtension
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

        // Capture the current code block renderer as the fallback. When MermaidExtension
        // was registered before this extension, the fallback will be MermaidCodeBlockRenderer,
        // which itself delegates non-mermaid blocks to CodeBlockRenderer. This forms a
        // chain-of-responsibility without either extension needing to know about the other.
        IMarkdownObjectRenderer fallback =
            htmlRenderer.ObjectRenderers.FindExact<MermaidCodeBlockRenderer>() ??
            (IMarkdownObjectRenderer?)htmlRenderer.ObjectRenderers.FindExact<CodeBlockRenderer>() ??
            new CodeBlockRenderer();

        // Remove the existing renderer so only one dispatcher is active.
        var existing = htmlRenderer.ObjectRenderers.FindExact<MermaidCodeBlockRenderer>();
        if (existing is not null)
        {
            htmlRenderer.ObjectRenderers.Remove(existing);
        }
        else
        {
            var defaultRenderer = htmlRenderer.ObjectRenderers.FindExact<CodeBlockRenderer>();
            if (defaultRenderer is not null)
            {
                htmlRenderer.ObjectRenderers.Remove(defaultRenderer);
            }
        }

        htmlRenderer.ObjectRenderers.Insert(0, new SyntaxHighlightCodeBlockRenderer(fallback));
    }
}
