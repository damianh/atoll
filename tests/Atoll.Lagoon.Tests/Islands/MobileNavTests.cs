using Atoll.Components;
using Atoll.Lagoon.Islands;
using Atoll.Instructions;
using Atoll.Rendering;

namespace Atoll.Lagoon.Tests.Islands;

public sealed class MobileNavTests
{
    [Fact]
    public async Task ShouldRenderHamburgerButton()
    {
        var dest = new StringRenderDestination();
        await ComponentRenderer.RenderComponentAsync<MobileNav>(dest, new Dictionary<string, object?>());
        var html = dest.GetOutput();

        html.ShouldContain("<button");
        html.ShouldContain("id=\"mobile-nav-toggle\"");
    }

    [Fact]
    public async Task ShouldRenderButtonWithAriaLabel()
    {
        var dest = new StringRenderDestination();
        await ComponentRenderer.RenderComponentAsync<MobileNav>(dest, new Dictionary<string, object?>());
        var html = dest.GetOutput();

        html.ShouldContain("aria-label=\"Open navigation\"");
    }

    [Fact]
    public async Task ShouldRenderButtonWithAriaExpanded()
    {
        var dest = new StringRenderDestination();
        await ComponentRenderer.RenderComponentAsync<MobileNav>(dest, new Dictionary<string, object?>());
        var html = dest.GetOutput();

        html.ShouldContain("aria-expanded=\"false\"");
    }

    [Fact]
    public async Task ShouldRenderButtonWithAriaControls()
    {
        var dest = new StringRenderDestination();
        await ComponentRenderer.RenderComponentAsync<MobileNav>(dest, new Dictionary<string, object?>());
        var html = dest.GetOutput();

        html.ShouldContain("aria-controls=\"mobile-nav-menu\"");
    }

    [Fact]
    public void ShouldHaveClientMediaDirective()
    {
        var island = new MobileNav();

        var metadata = island.CreateMetadata();

        metadata.ShouldNotBeNull();
        metadata.DirectiveType.ShouldBe(ClientDirectiveType.Media);
        metadata.DirectiveValue.ShouldBe("(max-width: 768px)");
    }

    [Fact]
    public void ShouldHaveCorrectClientModuleUrl()
    {
        var island = new MobileNav();

        island.ClientModuleUrl.ShouldBe("/scripts/atoll-docs-mobile-nav.js");
    }

    [Fact]
    public async Task ShouldRenderAsIslandWrapper()
    {
        var dest = new StringRenderDestination();
        var island = new MobileNav();
        await island.RenderIslandAsync(dest);
        var html = dest.GetOutput();

        html.ShouldContain("<atoll-island");
        html.ShouldContain("client=\"media\"");
        html.ShouldContain("component-url=\"/scripts/atoll-docs-mobile-nav.js\"");
        html.ShouldContain("id=\"mobile-nav-toggle\"");
    }
}
