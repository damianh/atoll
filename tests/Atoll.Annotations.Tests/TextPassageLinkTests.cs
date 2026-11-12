using Atoll.Components;
using Atoll.Instructions;
using Atoll.Rendering;

namespace Atoll.Annotations.Tests;

public sealed class TextPassageLinkTests
{
    [Fact]
    public async Task ShouldRenderLinkOnlyIslandWithoutRepository()
    {
        var destination = new StringRenderDestination();
        await ComponentRenderer.RenderComponentAsync<TextPassageLink>(destination, new Dictionary<string, object?>());

        var html = destination.GetOutput();
        html.ShouldContain("<div class=\"atoll-passage-link\" data-content-selector=\"article\"");
        html.ShouldNotContain("data-repo=");
    }

    [Fact]
    public async Task ShouldEncodeCustomContentSelector()
    {
        var destination = new StringRenderDestination();
        await ComponentRenderer.RenderComponentAsync<TextPassageLink>(destination, new Dictionary<string, object?>
        {
            [nameof(TextPassageLink.ContentSelector)] = "article[data-kind=\"docs\"]",
        });

        destination.GetOutput().ShouldContain("data-content-selector=\"article[data-kind=&quot;docs&quot;]\"");
    }

    [Fact]
    public void ShouldUseSharedIdleHydratedModule()
    {
        var island = new TextPassageLink();

        island.ClientModuleUrl.ShouldBe("/scripts/atoll-annotations.js");
        var metadata = island.CreateMetadata();
        metadata.ShouldNotBeNull();
        metadata.DirectiveType.ShouldBe(ClientDirectiveType.Idle);
    }

    [Fact]
    public async Task ShouldRenderIdleIslandWrapperWithoutRepository()
    {
        var destination = new StringRenderDestination();
        await new TextPassageLink().RenderIslandAsync(destination, new Dictionary<string, object?>());

        var html = destination.GetOutput();
        html.ShouldContain("<atoll-island");
        html.ShouldContain("client=\"idle\"");
        html.ShouldContain("component-url=\"/scripts/atoll-annotations.js\"");
        html.ShouldContain("<div class=\"atoll-passage-link\"");
    }
}
