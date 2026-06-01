using Atoll.Components;
using Atoll.Rendering;
using Atoll.Swell.Layouts;
using Atoll.Swell.Markdown;

namespace Atoll.Swell.Tests.Layouts;

public sealed class PresenterLayoutTests
{
    private static async Task<string> RenderAsync(
        DeckConfig? config = null,
        IReadOnlyList<RenderedSlideEntry>? slides = null)
    {
        config ??= new DeckConfig { Title = "Test Talk" };
        slides ??= [];

        var destination = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            [nameof(PresenterLayout.Config)] = config,
            [nameof(PresenterLayout.Slides)] = slides,
        };

        await ComponentRenderer.RenderComponentAsync<PresenterLayout>(destination, props);
        return destination.GetOutput();
    }

    [Fact]
    public async Task should_render_html5_doctype()
    {
        var html = await RenderAsync();

        html.ShouldStartWith("<!DOCTYPE html>");
    }

    [Fact]
    public async Task should_include_deck_title_in_page_title()
    {
        var html = await RenderAsync(new DeckConfig { Title = "My Talk" });

        html.ShouldContain("Presenter — My Talk");
    }

    [Fact]
    public async Task should_render_presenter_ui_elements()
    {
        var html = await RenderAsync();

        html.ShouldContain("id=\"swell-presenter-current\"");
        html.ShouldContain("id=\"swell-presenter-next\"");
        html.ShouldContain("id=\"swell-presenter-notes\"");
        html.ShouldContain("id=\"swell-presenter-elapsed\"");
        html.ShouldContain("id=\"swell-presenter-clock\"");
        html.ShouldContain("id=\"swell-presenter-counter\"");
    }

    [Fact]
    public async Task should_include_presenter_script_reference()
    {
        var html = await RenderAsync();

        html.ShouldContain("atoll-swell-presenter.js");
    }

    [Fact]
    public async Task should_inject_slide_notes_json()
    {
        var slides = new List<RenderedSlideEntry>
        {
            new(0, new SlideConfig(), "First slide note"),
            new(1, new SlideConfig(), "Second slide note"),
        };

        var html = await RenderAsync(slides: slides);

        html.ShouldContain("window.swellSlides");
        html.ShouldContain("First slide note");
        html.ShouldContain("Second slide note");
    }

    [Fact]
    public async Task should_show_correct_total_slide_count()
    {
        var slides = new List<RenderedSlideEntry>
        {
            new(0, new SlideConfig(), ""),
            new(1, new SlideConfig(), ""),
            new(2, new SlideConfig(), ""),
        };

        var html = await RenderAsync(slides: slides);

        html.ShouldContain("/ 3");
    }
}
