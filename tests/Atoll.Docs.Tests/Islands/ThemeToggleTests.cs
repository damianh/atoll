using Atoll.Components;
using Atoll.Docs.Islands;
using Atoll.Instructions;
using Atoll.Islands;
using Atoll.Rendering;
using Shouldly;
using Xunit;

namespace Atoll.Docs.Tests.Islands;

public sealed class ThemeToggleTests
{
    [Fact]
    public async Task ShouldRenderToggleButton()
    {
        var dest = new StringRenderDestination();
        await ComponentRenderer.RenderComponentAsync<ThemeToggle>(dest, new Dictionary<string, object?>());
        var html = dest.GetOutput();

        html.ShouldContain("<button");
        html.ShouldContain("id=\"theme-toggle\"");
    }

    [Fact]
    public async Task ShouldRenderButtonWithAriaLabel()
    {
        var dest = new StringRenderDestination();
        await ComponentRenderer.RenderComponentAsync<ThemeToggle>(dest, new Dictionary<string, object?>());
        var html = dest.GetOutput();

        html.ShouldContain("aria-label=\"Toggle theme\"");
    }

    [Fact]
    public async Task ShouldHaveClientLoadDirective()
    {
        var island = new ThemeToggle();

        var metadata = island.CreateMetadata();

        metadata.ShouldNotBeNull();
        metadata.DirectiveType.ShouldBe(ClientDirectiveType.Load);
    }

    [Fact]
    public async Task ShouldHaveCorrectClientModuleUrl()
    {
        var island = new ThemeToggle();

        island.ClientModuleUrl.ShouldBe("/scripts/atoll-docs-theme-toggle.js");
    }

    [Fact]
    public async Task ShouldRenderAsIslandWrapper()
    {
        var dest = new StringRenderDestination();
        var island = new ThemeToggle();
        await island.RenderIslandAsync(dest);
        var html = dest.GetOutput();

        html.ShouldContain("<atoll-island");
        html.ShouldContain("client=\"load\"");
        html.ShouldContain("component-url=\"/scripts/atoll-docs-theme-toggle.js\"");
        html.ShouldContain("id=\"theme-toggle\"");
    }
}
