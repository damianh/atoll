using Atoll.Components;
using Atoll.DrawIo.Components;
using Atoll.Rendering;
using Shouldly;
using Xunit;

namespace Atoll.DrawIo.Tests.Components;

public sealed class DrawioDiagramTests
{
    private static readonly string FixturesDir =
        Path.Combine(AppContext.BaseDirectory, "Fixtures");

    private static string FixturePath(string name) =>
        Path.Combine(FixturesDir, name);

    [Fact]
    public async Task RenderShouldContainSvgElement()
    {
        var dest = new StringRenderDestination();
        var props = new Dictionary<string, object?> { ["FilePath"] = FixturePath("simple.drawio") };

        await ComponentRenderer.RenderComponentAsync<DrawioDiagram>(dest, props);
        var html = dest.GetOutput();

        html.ShouldContain("<svg");
    }

    [Fact]
    public async Task RenderShouldWrapInDiagramDiv()
    {
        var dest = new StringRenderDestination();
        var props = new Dictionary<string, object?> { ["FilePath"] = FixturePath("simple.drawio") };

        await ComponentRenderer.RenderComponentAsync<DrawioDiagram>(dest, props);
        var html = dest.GetOutput();

        html.ShouldStartWith("<div class=\"drawio-diagram\">");
        html.ShouldEndWith("</div>");
    }

    [Fact]
    public async Task RenderWithAltShouldAddRoleAndAriaLabel()
    {
        var dest = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            ["FilePath"] = FixturePath("simple.drawio"),
            ["Alt"] = "System architecture",
        };

        await ComponentRenderer.RenderComponentAsync<DrawioDiagram>(dest, props);
        var html = dest.GetOutput();

        html.ShouldContain("role=\"img\"");
        html.ShouldContain("aria-label=\"System architecture\"");
    }

    [Fact]
    public async Task RenderWithoutAltShouldNotAddRoleOrAriaLabel()
    {
        var dest = new StringRenderDestination();
        var props = new Dictionary<string, object?> { ["FilePath"] = FixturePath("simple.drawio") };

        await ComponentRenderer.RenderComponentAsync<DrawioDiagram>(dest, props);
        var html = dest.GetOutput();

        html.ShouldNotContain("role=");
        html.ShouldNotContain("aria-label=");
    }

    [Fact]
    public async Task RenderWithPageIndexShouldRenderSpecificPage()
    {
        var dest = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            ["FilePath"] = FixturePath("multi-page.drawio"),
            ["Page"] = (int?)1,
        };

        await ComponentRenderer.RenderComponentAsync<DrawioDiagram>(dest, props);
        var html = dest.GetOutput();

        // Page 1 ("Details") has an ellipse
        html.ShouldContain("<ellipse");
    }

    [Fact]
    public async Task RenderWithPageNameShouldRenderSpecificPage()
    {
        var dest = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            ["FilePath"] = FixturePath("multi-page.drawio"),
            ["PageName"] = "Details",
        };

        await ComponentRenderer.RenderComponentAsync<DrawioDiagram>(dest, props);
        var html = dest.GetOutput();

        html.ShouldContain("<ellipse");
    }

    [Fact]
    public async Task RenderWithWidthShouldSetWidthAttribute()
    {
        var dest = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            ["FilePath"] = FixturePath("simple.drawio"),
            ["Width"] = "800px",
        };

        await ComponentRenderer.RenderComponentAsync<DrawioDiagram>(dest, props);
        var html = dest.GetOutput();

        html.ShouldContain("width=\"800px\"");
    }

    [Fact]
    public async Task RenderAltShouldHtmlEncodeSpecialCharacters()
    {
        var dest = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            ["FilePath"] = FixturePath("simple.drawio"),
            ["Alt"] = "A & B <diagram>",
        };

        await ComponentRenderer.RenderComponentAsync<DrawioDiagram>(dest, props);
        var html = dest.GetOutput();

        html.ShouldContain("aria-label=\"A &amp; B &lt;diagram&gt;\"");
    }
}
