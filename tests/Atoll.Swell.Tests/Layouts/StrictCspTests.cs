using Atoll.Components;
using Atoll.Rendering;
using Atoll.Slots;
using Atoll.Swell.Layouts;
using Atoll.Swell.Markdown;
using Atoll.Tests.Shared;

namespace Atoll.Swell.Tests.Layouts;

/// <summary>
/// Verifies Swell output works under a strict Content-Security-Policy (<c>script-src 'self'</c>).
/// </summary>
public sealed class StrictCspTests
{
    // Notes are JSON-encoded in the presenter data block, so a </script> in a note cannot break out.
    private static readonly IReadOnlyList<RenderedSlideEntry> Slides =
    [
        new(0, new SlideConfig(), "Note with </script><script>alert(1)</script> inside"),
        new(1, new SlideConfig(), "Second"),
    ];

    [Fact]
    public async Task PresenterLayoutShouldHaveNoInlineScriptsOrHandlers()
    {
        var destination = new StringRenderDestination();
        await ComponentRenderer.RenderComponentAsync<PresenterLayout>(destination, new Dictionary<string, object?>
        {
            [nameof(PresenterLayout.Config)] = new DeckConfig { Title = "Talk" },
            [nameof(PresenterLayout.Slides)] = Slides,
        });
        var html = destination.GetOutput();

        html.ShouldContain("<script type=\"application/json\" id=\"swell-slides\">");
        html.ShouldNotContain("</script><script>alert(1)");
        html.ShouldBeStrictCspCompatible();
    }

    [Fact]
    public async Task DeckLayoutShouldHaveNoInlineScriptsOrHandlers()
    {
        var destination = new StringRenderDestination();
        var slot = RenderFragment.FromHtml("<section data-slide-index=\"0\"><h1>Hi</h1></section>");
        await ComponentRenderer.RenderComponentAsync<SwellDeckLayout>(
            destination,
            new Dictionary<string, object?>
            {
                [nameof(SwellDeckLayout.Config)] = new DeckConfig { Title = "Talk" },
                [nameof(SwellDeckLayout.Slides)] = new List<RenderedSlideEntry> { new(0, new SlideConfig(), "Note") },
            },
            SlotCollection.FromDefault(slot));

        destination.GetOutput().ShouldBeStrictCspCompatible();
    }
}
