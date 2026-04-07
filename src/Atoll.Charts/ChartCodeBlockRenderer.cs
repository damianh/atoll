using System.Text.Json;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace Atoll.Charts;

/// <summary>
/// HTML renderer for code blocks that handles <c>chart</c> language blocks specially.
/// Renders <c>```chart</c> fenced blocks as a <c>&lt;div class="atoll-chart"&gt;</c>
/// containing a <c>&lt;canvas data-chart-config="..."&gt;</c> element for Chart.js
/// client-side rendering. All other code blocks are rendered using the default
/// Markdig behaviour.
/// </summary>
internal sealed class ChartCodeBlockRenderer : HtmlObjectRenderer<CodeBlock>
{
    private readonly CodeBlockRenderer _fallback = new();

    /// <inheritdoc />
    protected override void Write(HtmlRenderer renderer, CodeBlock block)
    {
        if (block is FencedCodeBlock fenced &&
            string.Equals(fenced.Info, "chart", StringComparison.OrdinalIgnoreCase))
        {
            WriteChartBlock(renderer, fenced);
        }
        else
        {
            _fallback.Write(renderer, block);
        }
    }

    private static void WriteChartBlock(HtmlRenderer renderer, FencedCodeBlock block)
    {
        // Extract raw chart config JSON from the code block content.
        var rawJson = ExtractContent(block);

        // Validate that the content is parseable JSON before embedding.
        if (!IsValidJson(rawJson))
        {
            renderer.Write("<div class=\"atoll-chart atoll-chart-error\">");
            renderer.WriteEscape("Invalid chart configuration: JSON could not be parsed.");
            renderer.Write("</div>");
            return;
        }

        // HTML-attribute-encode the JSON for safe embedding in a data attribute.
        var encodedJson = System.Web.HttpUtility.HtmlAttributeEncode(rawJson);

        // Wrap in an <atoll-island> element so the island runtime discovers
        // this element, loads chart-init.js, and initialises Chart.js.
        var optsJson = System.Web.HttpUtility.HtmlAttributeEncode(
            JsonSerializer.Serialize(new { name = "ChartCodeBlock", value = "" }));

        renderer.Write("<atoll-island" +
            " component-url=\"/scripts/atoll-charts-init.js\"" +
            " component-export=\"default\"" +
            " client=\"visible\"" +
            " props=\"{}\"" +
            $" opts=\"{optsJson}\"" +
            " ssr>");
        renderer.Write("<div class=\"atoll-chart\">");
        renderer.Write($"<canvas data-chart-config=\"{encodedJson}\"></canvas>");
        renderer.Write("<noscript>Chart requires JavaScript to display.</noscript>");
        renderer.Write("</div>");
        renderer.WriteLine("</atoll-island>");
    }

    private static string ExtractContent(FencedCodeBlock block)
    {
        var lines = block.Lines;
        var builder = new System.Text.StringBuilder();
        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines.Lines[i];
            var slice = line.Slice;
            if (slice.Text is not null)
            {
                builder.Append(slice.Text, slice.Start, slice.Length);
            }
            if (i < lines.Count - 1)
            {
                builder.Append('\n');
            }
        }
        return builder.ToString();
    }

    private static bool IsValidJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
