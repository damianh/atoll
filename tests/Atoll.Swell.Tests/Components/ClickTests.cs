using Atoll.Components;
using Atoll.Rendering;
using Atoll.Slots;
using Atoll.Swell.Components;

namespace Atoll.Swell.Tests.Components;

public sealed class ClickTests
{
    [Fact]
    public async Task should_wrap_slot_content_in_swell_click_div()
    {
        var destination = new StringRenderDestination();
        var slotContent = RenderFragment.FromHtml("<p>Revealed content</p>");
        var slots = SlotCollection.FromDefault(slotContent);

        await ComponentRenderer.RenderComponentAsync<Click>(destination, new Dictionary<string, object?>(), slots);

        var output = destination.GetOutput();
        output.ShouldContain("<div class=\"swell-click\">");
        output.ShouldContain("<p>Revealed content</p>");
        output.ShouldContain("</div>");
    }

    [Fact]
    public async Task should_render_empty_swell_click_div_when_no_slot()
    {
        var destination = new StringRenderDestination();

        await ComponentRenderer.RenderComponentAsync<Click>(destination);

        var output = destination.GetOutput();
        output.ShouldContain("<div class=\"swell-click\">");
        output.ShouldContain("</div>");
    }
}
