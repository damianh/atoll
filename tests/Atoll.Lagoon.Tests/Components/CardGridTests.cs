using Atoll.Components;
using Atoll.Lagoon.Components;
using Atoll.Rendering;
using Atoll.Slots;

namespace Atoll.Lagoon.Tests.Components;

public sealed class CardGridTests
{
    private static async Task<string> RenderCardGridAsync(
        bool stagger = false,
        string slotContent = "<div>item</div>")
    {
        var destination = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            ["Stagger"] = stagger,
        };
        var slots = SlotCollection.FromDefault(RenderFragment.FromHtml(slotContent));
        await ComponentRenderer.RenderComponentAsync<CardGrid>(destination, props, slots);
        return destination.GetOutput();
    }

    [Fact]
    public async Task ShouldApplyStaggerClassWhenTrue()
    {
        var html = await RenderCardGridAsync(stagger: true);

        html.ShouldContain("card-grid-stagger");
    }

    [Fact]
    public async Task ShouldOmitStaggerClassWhenFalse()
    {
        var html = await RenderCardGridAsync(stagger: false);

        html.ShouldNotContain("card-grid-stagger");
    }

    [Fact]
    public async Task ShouldRenderSlotContent()
    {
        var html = await RenderCardGridAsync(slotContent: "<div>my card</div>");

        html.ShouldContain("<div>my card</div>");
    }
}
