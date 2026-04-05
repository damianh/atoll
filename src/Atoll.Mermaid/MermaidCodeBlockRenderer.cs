using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace Atoll.Mermaid;

/// <summary>
/// HTML renderer for code blocks that handles <c>mermaid</c> language blocks specially.
/// Renders <c>```mermaid</c> fenced blocks as <c>&lt;pre class="mermaid"&gt;</c>
/// (without a nested <c>&lt;code&gt;</c> element) so the Mermaid JS library can process them.
/// All other code blocks are rendered using the default Markdig behaviour.
/// </summary>
public sealed class MermaidCodeBlockRenderer : HtmlObjectRenderer<CodeBlock>
{
    private readonly CodeBlockRenderer _fallback = new CodeBlockRenderer();

    /// <inheritdoc />
    protected override void Write(HtmlRenderer renderer, CodeBlock block)
    {
        if (block is FencedCodeBlock fenced &&
            string.Equals(fenced.Info, "mermaid", StringComparison.OrdinalIgnoreCase))
        {
            WriteMermaidBlock(renderer, fenced);
        }
        else
        {
            _fallback.Write(renderer, block);
        }
    }

    private static void WriteMermaidBlock(HtmlRenderer renderer, FencedCodeBlock block)
    {
        renderer.Write("<pre class=\"mermaid\">");

        // HTML-encode the code content so that mermaid diagram text cannot inject raw HTML.
        // The Mermaid JS library reads the element's textContent, so encoded content is
        // functionally identical to unencoded content for diagram rendering purposes.
        var lines = block.Lines;
        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines.Lines[i];
            var slice = line.Slice;
            renderer.WriteEscape(slice.Text, slice.Start, slice.Length);
            if (i < lines.Count - 1)
            {
                renderer.WriteLine();
            }
        }

        renderer.WriteLine("</pre>");
    }
}
