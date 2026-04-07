using Atoll.Components;
using Atoll.Instructions;
using Atoll.Reef.Configuration;
using Atoll.Reef.Islands;
using Atoll.Rendering;
using Shouldly;
using Xunit;

namespace Atoll.Reef.Tests.Islands;

public sealed class ViewToggleTests
{
    private static async Task<string> RenderViewToggleAsync(DefaultView view = DefaultView.List)
    {
        var dest = new StringRenderDestination();
        var props = new Dictionary<string, object?> { ["CurrentView"] = view };
        await ComponentRenderer.RenderComponentAsync<ViewToggle>(dest, props);
        return dest.GetOutput();
    }

    [Fact]
    public async Task ShouldRenderToggleContainer()
    {
        var html = await RenderViewToggleAsync();

        html.ShouldContain("data-view-toggle");
        html.ShouldContain("class=\"view-toggle\"");
    }

    [Fact]
    public async Task ShouldRenderThreeViewButtons()
    {
        var html = await RenderViewToggleAsync();

        html.ShouldContain("data-view-btn=\"list\"");
        html.ShouldContain("data-view-btn=\"grid\"");
        html.ShouldContain("data-view-btn=\"table\"");
    }

    [Fact]
    public async Task ShouldSetActiveButtonForListView()
    {
        var html = await RenderViewToggleAsync(DefaultView.List);

        html.ShouldContain("data-view-btn=\"list\" aria-pressed=\"true\"");
        html.ShouldContain("data-view-btn=\"grid\" aria-pressed=\"false\"");
        html.ShouldContain("data-view-btn=\"table\" aria-pressed=\"false\"");
    }

    [Fact]
    public async Task ShouldSetActiveButtonForGridView()
    {
        var html = await RenderViewToggleAsync(DefaultView.Grid);

        html.ShouldContain("data-view-btn=\"list\" aria-pressed=\"false\"");
        html.ShouldContain("data-view-btn=\"grid\" aria-pressed=\"true\"");
        html.ShouldContain("data-view-btn=\"table\" aria-pressed=\"false\"");
    }

    [Fact]
    public async Task ShouldSetActiveButtonForTableView()
    {
        var html = await RenderViewToggleAsync(DefaultView.Table);

        html.ShouldContain("data-view-btn=\"list\" aria-pressed=\"false\"");
        html.ShouldContain("data-view-btn=\"grid\" aria-pressed=\"false\"");
        html.ShouldContain("data-view-btn=\"table\" aria-pressed=\"true\"");
    }

    [Fact]
    public async Task ShouldAddActiveCssClassToCurrentView()
    {
        var html = await RenderViewToggleAsync(DefaultView.Grid);

        html.ShouldContain("view-toggle__btn--active");
    }

    [Fact]
    public void ShouldHaveClientLoadDirective()
    {
        var island = new ViewToggle();

        var metadata = island.CreateMetadata();

        metadata.ShouldNotBeNull();
        metadata.DirectiveType.ShouldBe(ClientDirectiveType.Load);
    }

    [Fact]
    public void ShouldHaveCorrectClientModuleUrl()
    {
        var island = new ViewToggle();

        island.ClientModuleUrl.ShouldBe("/scripts/atoll-reef-view-toggle.js");
    }

    [Fact]
    public async Task ShouldRenderAsIslandWrapper()
    {
        var dest = new StringRenderDestination();
        var island = new ViewToggle();
        await island.RenderIslandAsync(dest);
        var html = dest.GetOutput();

        html.ShouldContain("<atoll-island");
        html.ShouldContain("client=\"load\"");
        html.ShouldContain("component-url=\"/scripts/atoll-reef-view-toggle.js\"");
    }

    [Fact]
    public async Task ShouldHaveAriaLabelOnGroup()
    {
        var html = await RenderViewToggleAsync();

        html.ShouldContain("aria-label=\"Switch article view\"");
    }
}
