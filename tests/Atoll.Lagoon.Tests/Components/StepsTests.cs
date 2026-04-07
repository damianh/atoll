using Atoll.Components;
using Atoll.Lagoon.Components;
using Atoll.Rendering;
using Atoll.Slots;

namespace Atoll.Lagoon.Tests.Components;

public sealed class StepsTests
{
    private static async Task<string> RenderStepsAsync(string slotContent = "<ol><li>Step one</li><li>Step two</li></ol>")
    {
        var destination = new StringRenderDestination();
        var props = new Dictionary<string, object?>();
        var slots = SlotCollection.FromDefault(RenderFragment.FromHtml(slotContent));
        await ComponentRenderer.RenderComponentAsync<Steps>(destination, props, slots);
        return destination.GetOutput();
    }

    [Fact]
    public async Task ShouldRenderSlotContent()
    {
        var html = await RenderStepsAsync("<ol><li>First step</li></ol>");

        html.ShouldContain("<ol><li>First step</li></ol>");
    }

    [Fact]
    public async Task ShouldRenderSlotContentInsideStepsDiv()
    {
        var html = await RenderStepsAsync("<ol><li>A</li></ol>");

        var stepsStart = html.IndexOf("<div class=\"steps\">");
        var stepsEnd = html.IndexOf("</div>");
        var slotPos = html.IndexOf("<ol>");

        slotPos.ShouldBeGreaterThan(stepsStart);
        slotPos.ShouldBeLessThan(stepsEnd);
    }
}
