using System.Text.Json;
using Atoll.Charts.Islands;
using Atoll.Components;
using Atoll.Instructions;
using Atoll.Rendering;

namespace Atoll.Charts.Tests.Islands;

public sealed class ChartIslandTests
{
    private static readonly string SampleConfig = "{\"type\":\"bar\",\"data\":{\"labels\":[\"A\",\"B\"],\"datasets\":[]}}";

    private static async Task<string> RenderAsync(Dictionary<string, object?> props)
    {
        var dest = new StringRenderDestination();
        await ComponentRenderer.RenderComponentAsync<ChartIsland>(dest, props);
        return dest.GetOutput();
    }

    // ─── HTML structure ───────────────────────────────────────────────────

    [Fact]
    public async Task RenderShouldContainAtollChartDiv()
    {
        var html = await RenderAsync(new Dictionary<string, object?>
        {
            ["ConfigJson"] = SampleConfig,
        });

        html.ShouldContain("<div class=\"atoll-chart\"");
    }

    [Fact]
    public async Task RenderShouldContainCanvasElement()
    {
        var html = await RenderAsync(new Dictionary<string, object?>
        {
            ["ConfigJson"] = SampleConfig,
        });

        html.ShouldContain("<canvas");
    }

    [Fact]
    public async Task RenderShouldContainDataChartConfigAttribute()
    {
        var html = await RenderAsync(new Dictionary<string, object?>
        {
            ["ConfigJson"] = SampleConfig,
        });

        html.ShouldContain("data-chart-config=");
    }

    [Fact]
    public async Task RenderShouldContainNoscriptFallback()
    {
        var html = await RenderAsync(new Dictionary<string, object?>
        {
            ["ConfigJson"] = SampleConfig,
        });

        html.ShouldContain("<noscript>");
        html.ShouldContain("Chart requires JavaScript to display.");
    }

    [Fact]
    public async Task RenderShouldEmbedConfigJsonInDataAttribute()
    {
        var html = await RenderAsync(new Dictionary<string, object?>
        {
            ["ConfigJson"] = SampleConfig,
        });

        // Extract and decode the data-chart-config attribute value
        var config = RequireConfig(html);
        config.TryGetProperty("type", out var typeProp).ShouldBeTrue();
        typeProp.GetString().ShouldBe("bar");
    }

    // ─── Accessibility (Alt) ──────────────────────────────────────────────

    [Fact]
    public async Task RenderWithAltShouldAddRoleAndAriaLabel()
    {
        var html = await RenderAsync(new Dictionary<string, object?>
        {
            ["ConfigJson"] = SampleConfig,
            ["Alt"] = "Monthly sales chart",
        });

        html.ShouldContain("role=\"img\"");
        html.ShouldContain("aria-label=\"Monthly sales chart\"");
    }

    [Fact]
    public async Task RenderWithoutAltShouldNotAddRoleOrAriaLabel()
    {
        var html = await RenderAsync(new Dictionary<string, object?>
        {
            ["ConfigJson"] = SampleConfig,
        });

        html.ShouldNotContain("role=");
        html.ShouldNotContain("aria-label=");
    }

    [Fact]
    public async Task RenderAltShouldHtmlEncodeSpecialCharacters()
    {
        var html = await RenderAsync(new Dictionary<string, object?>
        {
            ["ConfigJson"] = SampleConfig,
            ["Alt"] = "A & B <chart>",
        });

        html.ShouldContain("aria-label=\"A &amp; B &lt;chart>\"");
    }

    // ─── Width / Height ───────────────────────────────────────────────────

    [Fact]
    public async Task RenderWithWidthAndHeightShouldSetCanvasAttributes()
    {
        var html = await RenderAsync(new Dictionary<string, object?>
        {
            ["ConfigJson"] = SampleConfig,
            ["Width"] = (int?)600,
            ["Height"] = (int?)400,
        });

        html.ShouldContain("width=\"600\"");
        html.ShouldContain("height=\"400\"");
    }

    [Fact]
    public async Task RenderWithoutWidthAndHeightShouldNotSetCanvasAttributes()
    {
        var html = await RenderAsync(new Dictionary<string, object?>
        {
            ["ConfigJson"] = SampleConfig,
        });

        html.ShouldNotContain("width=");
        html.ShouldNotContain("height=");
    }

    // ─── MaxWidth / MaxHeight ─────────────────────────────────────────────

    [Fact]
    public async Task RenderWithMaxWidthShouldSetContainerStyle()
    {
        var html = await RenderAsync(new Dictionary<string, object?>
        {
            ["ConfigJson"] = SampleConfig,
            ["MaxWidth"] = "600px",
        });

        html.ShouldContain("style=\"max-width:600px\"");
    }

    [Fact]
    public async Task RenderWithMaxHeightShouldSetContainerStyle()
    {
        var html = await RenderAsync(new Dictionary<string, object?>
        {
            ["ConfigJson"] = SampleConfig,
            ["MaxHeight"] = "400px",
        });

        html.ShouldContain("style=\"max-height:400px\"");
    }

    [Fact]
    public async Task RenderWithMaxWidthAndMaxHeightShouldSetBothStyles()
    {
        var html = await RenderAsync(new Dictionary<string, object?>
        {
            ["ConfigJson"] = SampleConfig,
            ["MaxWidth"] = "600px",
            ["MaxHeight"] = "400px",
        });

        html.ShouldContain("style=\"max-width:600px;max-height:400px\"");
    }

    [Fact]
    public async Task RenderWithoutMaxDimensionsShouldNotSetStyleAttribute()
    {
        var html = await RenderAsync(new Dictionary<string, object?>
        {
            ["ConfigJson"] = SampleConfig,
        });

        html.ShouldNotContain("style=");
    }

    // ─── Island contract ──────────────────────────────────────────────────

    [Fact]
    public void ShouldHaveClientVisibleDirective()
    {
        var island = new ChartIsland();

        var metadata = island.CreateMetadata();

        metadata.ShouldNotBeNull();
        metadata!.DirectiveType.ShouldBe(ClientDirectiveType.Visible);
    }

    [Fact]
    public void ClientModuleUrlShouldPointToChartInitScript()
    {
        var island = new ChartIsland();

        island.ClientModuleUrl.ShouldBe("/scripts/atoll-charts-init.js");
    }

    [Fact]
    public async Task RenderShouldWrapInAtollIslandElement()
    {
        var html = await RenderAsync(new Dictionary<string, object?>
        {
            ["ConfigJson"] = SampleConfig,
        });

        html.ShouldContain("<atoll-island");
        html.ShouldContain("</atoll-island>");
    }

    // ─── Atoll extensions pass-through ───────────────────────────────────

    [Fact]
    public async Task RenderShouldPreserveAtollLinksConfig()
    {
        var configWithLinks = """{"type":"bar","data":{},"_atoll":{"links":[["/a","/b"]]}}""";
        var html = await RenderAsync(new Dictionary<string, object?>
        {
            ["ConfigJson"] = configWithLinks,
        });

        var config = RequireConfig(html);
        config.TryGetProperty("_atoll", out var atoll).ShouldBeTrue();
        atoll.TryGetProperty("links", out var links).ShouldBeTrue();
        links.GetArrayLength().ShouldBe(1);
    }

    // ─── Helpers ──────────────────────────────────────────────────────────

    private static JsonElement? ExtractChartConfig(string html)
    {
        const string marker = "data-chart-config=\"";
        var start = html.IndexOf(marker, StringComparison.Ordinal);
        if (start < 0) return null;

        start += marker.Length;
        var end = html.IndexOf('"', start);
        if (end < 0) return null;

        var encoded = html[start..end];
        var jsonText = System.Net.WebUtility.HtmlDecode(encoded);

        return JsonSerializer.Deserialize<JsonElement>(jsonText);
    }

    private static JsonElement RequireConfig(string html)
    {
        var config = ExtractChartConfig(html);
        config.ShouldNotBeNull("data-chart-config attribute not found in output");
        return config!.Value;
    }
}
