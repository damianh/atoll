using Atoll.Components;
using Atoll.Rendering;
using Atoll.Swell.Components;

namespace Atoll.Swell.Tests.Components;

public sealed class SwellDeckTests
{
    private static async Task<string> RenderAsync(
        string src = "/slides/",
        string title = "Slide deck",
        string aspectRatio = "16/9")
    {
        var destination = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            [nameof(SwellDeck.Src)] = src,
            [nameof(SwellDeck.Title)] = title,
            [nameof(SwellDeck.AspectRatio)] = aspectRatio,
        };

        await ComponentRenderer.RenderComponentAsync<SwellDeck>(destination, props);
        return destination.GetOutput();
    }

    [Fact]
    public async Task should_render_iframe_with_correct_src()
    {
        var html = await RenderAsync(src: "/my-talk/");

        html.ShouldContain("src=\"/my-talk/\"");
        html.ShouldContain("<iframe");
    }

    [Fact]
    public async Task should_render_iframe_with_correct_title()
    {
        var html = await RenderAsync(title: "My Conference Talk");

        html.ShouldContain("title=\"My Conference Talk\"");
    }

    [Fact]
    public async Task should_render_aspect_ratio_container()
    {
        var html = await RenderAsync(aspectRatio: "4/3");

        html.ShouldContain("aspect-ratio:4/3");
    }

    [Fact]
    public async Task should_render_fullscreen_breakout_link()
    {
        var html = await RenderAsync(src: "/slides/");

        // Should include a link to open the deck in a new tab
        html.ShouldContain("target=\"_blank\"");
        html.ShouldContain("href=\"/slides/\"");
        html.ShouldContain("Fullscreen");
    }

    [Fact]
    public async Task should_default_to_16x9_aspect_ratio()
    {
        var html = await RenderAsync();

        html.ShouldContain("aspect-ratio:16/9");
    }
}
