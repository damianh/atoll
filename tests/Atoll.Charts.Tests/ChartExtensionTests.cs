using Markdig;

namespace Atoll.Charts.Tests;

public sealed class ChartExtensionTests
{
    private static string RenderDirect(string markdown) =>
        RenderDirect(markdown, enableChart: true);

    private static string RenderDirect(string markdown, bool enableChart)
    {
        var builder = new MarkdownPipelineBuilder();
        if (enableChart)
        {
            builder.Use<ChartExtension>();
        }
        var pipeline = builder.Build();
        return Markdig.Markdown.ToHtml(markdown, pipeline);
    }

    // --- Chart blocks ---

    [Fact]
    public void ShouldRenderChartBlockAsCanvasElement()
    {
        var md = "```chart\n{\"type\":\"bar\",\"data\":{}}\n```";
        var html = RenderDirect(md);

        html.ShouldContain("<canvas data-chart-config=");
    }

    [Fact]
    public void ShouldWrapChartBlockInIslandElement()
    {
        var md = "```chart\n{\"type\":\"bar\",\"data\":{}}\n```";
        var html = RenderDirect(md);

        html.ShouldContain("<atoll-island");
        html.ShouldContain("component-url=\"/scripts/atoll-charts-init.js\"");
        html.ShouldContain("component-export=\"default\"");
        html.ShouldContain("client=\"visible\"");
        html.ShouldContain("</atoll-island>");
    }

    [Fact]
    public void ShouldContainNoscriptFallback()
    {
        var md = "```chart\n{\"type\":\"bar\",\"data\":{}}\n```";
        var html = RenderDirect(md);

        html.ShouldContain("<noscript>");
        html.ShouldContain("Chart requires JavaScript to display.");
    }

    [Fact]
    public void ShouldNotContainCodeTagForChartBlock()
    {
        var md = "```chart\n{\"type\":\"bar\",\"data\":{}}\n```";
        var html = RenderDirect(md);

        html.ShouldNotContain("<code");
        html.ShouldNotContain("language-chart");
    }

    [Fact]
    public void ShouldRenderChartBlockCaseInsensitive()
    {
        var md = "```Chart\n{\"type\":\"bar\",\"data\":{}}\n```";
        var html = RenderDirect(md);

        html.ShouldContain("<canvas data-chart-config=");
    }

    // --- Non-chart blocks ---

    [Fact]
    public void ShouldRenderNonChartCodeBlockNormally()
    {
        var md = "```csharp\nvar x = 1;\n```";
        var html = RenderDirect(md);

        html.ShouldNotContain("data-chart-config");
        html.ShouldContain("<code");
        html.ShouldContain("var x = 1;");
    }

    [Fact]
    public void ShouldRenderCodeBlockWithoutLanguageNormally()
    {
        var md = "```\nplain code\n```";
        var html = RenderDirect(md);

        html.ShouldNotContain("data-chart-config");
        html.ShouldContain("plain code");
    }

    // --- Without Chart extension ---

    [Fact]
    public void ShouldRenderChartAsRegularCodeWhenExtensionDisabled()
    {
        var md = "```chart\n{\"type\":\"bar\",\"data\":{}}\n```";
        var html = RenderDirect(md, enableChart: false);

        html.ShouldNotContain("data-chart-config");
        html.ShouldContain("<code");
    }

    // --- Mixed blocks ---

    [Fact]
    public void ShouldHandleMixedChartAndCodeBlocks()
    {
        var md = "```chart\n{\"type\":\"bar\",\"data\":{}}\n```\n\n```csharp\nvar x = 1;\n```";
        var html = RenderDirect(md);

        html.ShouldContain("data-chart-config=");
        html.ShouldContain("<code");
        html.ShouldContain("var x = 1;");
    }

    // --- Extension standalone usage ---

    [Fact]
    public void ShouldBeUsableAsStandalonePipelineExtension()
    {
        var pipeline = new MarkdownPipelineBuilder()
            .Use<ChartExtension>()
            .Build();

        var md = "```chart\n{\"type\":\"bar\",\"data\":{}}\n```";
        var html = Markdig.Markdown.ToHtml(md, pipeline);

        html.ShouldContain("data-chart-config=");
    }

    // --- XSS safety ---

    [Fact]
    public void ShouldHtmlEncodeChartConfig()
    {
        // JSON with special HTML chars in string values
        var md = "```chart\n{\"type\":\"bar\",\"label\":\"A & B <test>\"}\n```";
        var html = RenderDirect(md);

        // The JSON is stored in an HTML attribute and must be attribute-encoded
        html.ShouldContain("data-chart-config=");
        // Extract the attribute value and verify & is encoded as &amp; and < as &lt;
        var attrStart = html.IndexOf("data-chart-config=\"", StringComparison.Ordinal) + "data-chart-config=\"".Length;
        var attrEnd = html.IndexOf('"', attrStart);
        var attrValue = html[attrStart..attrEnd];
        attrValue.ShouldContain("&amp;");
        attrValue.ShouldContain("&lt;");
    }

    [Fact]
    public void ShouldPreserveValidChartJsonInDataAttribute()
    {
        var originalJson = "{\"type\":\"bar\",\"data\":{\"labels\":[\"A\",\"B\"]}}";
        var md = $"```chart\n{originalJson}\n```";
        var html = RenderDirect(md);

        // Extract the attribute value and HTML-decode it to recover the original JSON
        var attrStart = html.IndexOf("data-chart-config=\"", StringComparison.Ordinal) + "data-chart-config=\"".Length;
        var attrEnd = html.IndexOf('"', attrStart);
        var encodedValue = html[attrStart..attrEnd];
        var decodedJson = System.Net.WebUtility.HtmlDecode(encodedValue);

        decodedJson.ShouldContain("\"type\":\"bar\"");
        decodedJson.ShouldContain("\"labels\"");
    }

    [Fact]
    public void ShouldHandleInvalidJsonGracefully()
    {
        var md = "```chart\nnot valid json at all\n```";
        var html = RenderDirect(md);

        // Should not throw; should render an error indicator instead of crashing
        html.ShouldNotContain("data-chart-config=");
        html.ShouldContain("atoll-chart-error");
    }

    // --- Atoll extensions pass-through ---

    [Fact]
    public void ShouldPreserveAtollLinksConfigInDataAttribute()
    {
        var json = """{"type":"bar","data":{"labels":["A","B"],"datasets":[{"data":[1,2]}]},"_atoll":{"links":[["/a","/b"]]}}""";
        var md = $"```chart\n{json}\n```";
        var html = RenderDirect(md);

        var attrStart = html.IndexOf("data-chart-config=\"", StringComparison.Ordinal) + "data-chart-config=\"".Length;
        var attrEnd = html.IndexOf('"', attrStart);
        var decoded = System.Net.WebUtility.HtmlDecode(html[attrStart..attrEnd]);
        decoded.ShouldContain("\"_atoll\"");
        decoded.ShouldContain("\"links\"");
        decoded.ShouldContain("\"/a\"");
    }
}
