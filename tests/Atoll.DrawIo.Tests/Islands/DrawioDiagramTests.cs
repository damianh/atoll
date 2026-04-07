using System.Text.Json;
using Atoll.Components;
using Atoll.DrawIo.Islands;
using Atoll.Instructions;
using Atoll.Rendering;

namespace Atoll.DrawIo.Tests.Islands;

public sealed class DrawioDiagramTests
{
    private static readonly string FixturesDir =
        Path.Combine(AppContext.BaseDirectory, "Fixtures");

    private static string FixturePath(string name) =>
        Path.Combine(FixturesDir, name);

    private static async Task<string> RenderAsync(Dictionary<string, object?> props)
    {
        var dest = new StringRenderDestination();
        await ComponentRenderer.RenderComponentAsync<DrawioDiagram>(dest, props);
        return dest.GetOutput();
    }

    // ─── HTML structure ───────────────────────────────────────────────────

    [Fact]
    public async Task RenderShouldContainDrawioDiagramDiv()
    {
        var html = await RenderAsync(new Dictionary<string, object?>
        {
            ["FilePath"] = FixturePath("simple.drawio"),
        });

        html.ShouldContain("<div class=\"drawio-diagram\"");
    }

    [Fact]
    public async Task RenderShouldContainDataMxgraphAttribute()
    {
        var html = await RenderAsync(new Dictionary<string, object?>
        {
            ["FilePath"] = FixturePath("simple.drawio"),
        });

        html.ShouldContain("data-mxgraph=");
    }

    [Fact]
    public async Task RenderShouldNotContainSvgElement()
    {
        var html = await RenderAsync(new Dictionary<string, object?>
        {
            ["FilePath"] = FixturePath("simple.drawio"),
        });

        html.ShouldNotContain("<svg");
    }

    [Fact]
    public async Task RenderShouldContainNoscriptFallback()
    {
        var html = await RenderAsync(new Dictionary<string, object?>
        {
            ["FilePath"] = FixturePath("simple.drawio"),
        });

        html.ShouldContain("<noscript>");
        html.ShouldContain("Enable JavaScript to view this diagram.");
    }

    // ─── data-mxgraph JSON config ─────────────────────────────────────────

    [Fact]
    public async Task RenderShouldEmbedRawFileXmlInDataMxgraph()
    {
        var filePath = FixturePath("simple.drawio");
        var html = await RenderAsync(new Dictionary<string, object?>
        {
            ["FilePath"] = filePath,
        });

        var config = RequireConfig(html);
        config.TryGetProperty("xml", out var xmlProp).ShouldBeTrue();
        xmlProp.GetString().ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task RenderShouldDefaultToPageZero()
    {
        var html = await RenderAsync(new Dictionary<string, object?>
        {
            ["FilePath"] = FixturePath("simple.drawio"),
        });

        var config = RequireConfig(html);
        config.TryGetProperty("page", out var pageProp).ShouldBeTrue();
        pageProp.GetInt32().ShouldBe(0);
    }

    [Fact]
    public async Task RenderWithPageIndexShouldSetPageInConfig()
    {
        var html = await RenderAsync(new Dictionary<string, object?>
        {
            ["FilePath"] = FixturePath("multi-page.drawio"),
            ["Page"] = (int?)1,
        });

        var config = RequireConfig(html);
        config.TryGetProperty("page", out var pageProp).ShouldBeTrue();
        pageProp.GetInt32().ShouldBe(1);
    }

    [Fact]
    public async Task RenderWithPageNameShouldResolveToPageIndex()
    {
        var html = await RenderAsync(new Dictionary<string, object?>
        {
            ["FilePath"] = FixturePath("multi-page.drawio"),
            ["PageName"] = "Details",
        });

        var config = RequireConfig(html);
        // "Details" is the second page (index 1) in multi-page.drawio
        config.TryGetProperty("page", out var pageProp).ShouldBeTrue();
        pageProp.GetInt32().ShouldBe(1);
    }

    // ─── Accessibility (Alt) ──────────────────────────────────────────────

    [Fact]
    public async Task RenderWithAltShouldAddRoleAndAriaLabel()
    {
        var html = await RenderAsync(new Dictionary<string, object?>
        {
            ["FilePath"] = FixturePath("simple.drawio"),
            ["Alt"] = "System architecture",
        });

        html.ShouldContain("role=\"img\"");
        html.ShouldContain("aria-label=\"System architecture\"");
    }

    [Fact]
    public async Task RenderWithoutAltShouldNotAddRoleOrAriaLabel()
    {
        var html = await RenderAsync(new Dictionary<string, object?>
        {
            ["FilePath"] = FixturePath("simple.drawio"),
        });

        html.ShouldNotContain("role=");
        html.ShouldNotContain("aria-label=");
    }

    [Fact]
    public async Task RenderAltShouldHtmlEncodeSpecialCharacters()
    {
        var html = await RenderAsync(new Dictionary<string, object?>
        {
            ["FilePath"] = FixturePath("simple.drawio"),
            ["Alt"] = "A & B <diagram>",
        });

        html.ShouldContain("aria-label=\"A &amp; B &lt;diagram>\"");
    }

    // ─── Island contract ──────────────────────────────────────────────────

    [Fact]
    public void ShouldHaveClientVisibleDirective()
    {
        var island = new DrawioDiagram();

        var metadata = island.CreateMetadata();

        metadata.ShouldNotBeNull();
        metadata!.DirectiveType.ShouldBe(ClientDirectiveType.Visible);
    }

    [Fact]
    public void ClientModuleUrlShouldPointToViewerInitScript()
    {
        var island = new DrawioDiagram();

        island.ClientModuleUrl.ShouldBe("/scripts/atoll-drawio-viewer-init.js");
    }

    [Fact]
    public async Task RenderShouldWrapInAtollIslandElement()
    {
        var html = await RenderAsync(new Dictionary<string, object?>
        {
            ["FilePath"] = FixturePath("simple.drawio"),
        });

        html.ShouldContain("<atoll-island");
        html.ShouldContain("</atoll-island>");
    }

    // ─── Helpers ──────────────────────────────────────────────────────────

    private static JsonElement? ExtractMxgraphConfig(string html)
    {
        const string marker = "data-mxgraph=\"";
        var start = html.IndexOf(marker, StringComparison.Ordinal);
        if (start < 0) return null;

        start += marker.Length;
        var end = html.IndexOf('"', start);
        if (end < 0) return null;

        // The JSON is HTML-attribute-encoded; decode it before parsing.
        var encoded = html[start..end];
        var jsonText = System.Net.WebUtility.HtmlDecode(encoded);

        return JsonSerializer.Deserialize<JsonElement>(jsonText);
    }

    private static JsonElement RequireConfig(string html)
    {
        var config = ExtractMxgraphConfig(html);
        config.ShouldNotBeNull("data-mxgraph attribute not found in output");
        return config!.Value;
    }
}
