using Atoll.DrawIo.Islands;
using Atoll.Instructions;
using Atoll.Islands;
using Atoll.Rendering;
using Shouldly;
using Xunit;

namespace Atoll.DrawIo.Tests.Islands;

public sealed class InteractiveDrawioDiagramTests
{
    private static readonly string FixturesDir =
        Path.Combine(AppContext.BaseDirectory, "Fixtures");

    private static string FixturePath(string name) =>
        Path.Combine(FixturesDir, name);

    [Fact]
    public void ClientModuleUrlShouldBeCorrect()
    {
        var island = new InteractiveDrawioDiagram();

        island.ClientModuleUrl.ShouldBe("/scripts/atoll-drawio-interactive.js");
    }

    [Fact]
    public void ShouldHaveClientVisibleDirective()
    {
        var island = new InteractiveDrawioDiagram();

        var metadata = island.CreateMetadata();

        metadata.ShouldNotBeNull();
        metadata.DirectiveType.ShouldBe(ClientDirectiveType.Visible);
    }

    [Fact]
    public async Task RenderIslandShouldWrapInAtollIslandElement()
    {
        var dest = new StringRenderDestination();
        var props = new Dictionary<string, object?> { ["FilePath"] = FixturePath("simple.drawio") };
        var island = new InteractiveDrawioDiagram();

        await island.RenderIslandAsync(dest, props);
        var html = dest.GetOutput();

        html.ShouldContain("<atoll-island");
        html.ShouldContain("</atoll-island>");
    }

    [Fact]
    public async Task RenderIslandShouldContainClientVisibleAttribute()
    {
        var dest = new StringRenderDestination();
        var props = new Dictionary<string, object?> { ["FilePath"] = FixturePath("simple.drawio") };
        var island = new InteractiveDrawioDiagram();

        await island.RenderIslandAsync(dest, props);
        var html = dest.GetOutput();

        html.ShouldContain("client=\"visible\"");
    }

    [Fact]
    public async Task RenderIslandShouldContainComponentUrl()
    {
        var dest = new StringRenderDestination();
        var props = new Dictionary<string, object?> { ["FilePath"] = FixturePath("simple.drawio") };
        var island = new InteractiveDrawioDiagram();

        await island.RenderIslandAsync(dest, props);
        var html = dest.GetOutput();

        html.ShouldContain("component-url=\"/scripts/atoll-drawio-interactive.js\"");
    }

    [Fact]
    public async Task RenderShouldContainSvgElement()
    {
        var dest = new StringRenderDestination();
        var props = new Dictionary<string, object?> { ["FilePath"] = FixturePath("simple.drawio") };
        var island = new InteractiveDrawioDiagram();

        await island.RenderIslandAsync(dest, props);
        var html = dest.GetOutput();

        html.ShouldContain("<svg");
    }

    [Fact]
    public async Task RenderShouldContainInteractiveContainer()
    {
        var dest = new StringRenderDestination();
        var props = new Dictionary<string, object?> { ["FilePath"] = FixturePath("simple.drawio") };
        var island = new InteractiveDrawioDiagram();

        await island.RenderIslandAsync(dest, props);
        var html = dest.GetOutput();

        html.ShouldContain("class=\"drawio-interactive\"");
    }

    [Fact]
    public async Task RenderWithPanZoomShouldContainDataAttribute()
    {
        var dest = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            ["FilePath"] = FixturePath("simple.drawio"),
            ["EnablePanZoom"] = true,
        };
        var island = new InteractiveDrawioDiagram();

        await island.RenderIslandAsync(dest, props);
        var html = dest.GetOutput();

        html.ShouldContain("data-pan-zoom=\"true\"");
    }

    [Fact]
    public async Task RenderWithPanZoomDisabledShouldNotContainDataAttribute()
    {
        var dest = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            ["FilePath"] = FixturePath("simple.drawio"),
            ["EnablePanZoom"] = false,
        };
        var island = new InteractiveDrawioDiagram();

        await island.RenderIslandAsync(dest, props);
        var html = dest.GetOutput();

        html.ShouldNotContain("data-pan-zoom=");
    }

    [Fact]
    public async Task RenderWithLayersShouldContainLayerButtons()
    {
        var dest = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            ["FilePath"] = FixturePath("layers.drawio"),
            ["ShowLayerControls"] = true,
        };
        var island = new InteractiveDrawioDiagram();

        await island.RenderIslandAsync(dest, props);
        var html = dest.GetOutput();

        html.ShouldContain("drawio-layer-btn");
        html.ShouldContain("drawio-layer-controls");
    }

    [Fact]
    public async Task RenderWithLayerControlsDisabledShouldNotContainButtons()
    {
        var dest = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            ["FilePath"] = FixturePath("layers.drawio"),
            ["ShowLayerControls"] = false,
        };
        var island = new InteractiveDrawioDiagram();

        await island.RenderIslandAsync(dest, props);
        var html = dest.GetOutput();

        html.ShouldNotContain("drawio-layer-btn");
    }
}
