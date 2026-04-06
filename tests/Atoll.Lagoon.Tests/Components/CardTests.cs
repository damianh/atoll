using Atoll.Components;
using Atoll.Lagoon.Components;
using Atoll.Rendering;
using Atoll.Slots;
using Shouldly;
using Xunit;

namespace Atoll.Lagoon.Tests.Components;

public sealed class CardTests
{
    private static async Task<string> RenderCardAsync(
        string title,
        IconName? iconName = null,
        string slotContent = "<p>Body content</p>")
    {
        var destination = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            ["Title"] = title,
            ["IconName"] = iconName,
        };
        var slots = SlotCollection.FromDefault(RenderFragment.FromHtml(slotContent));
        await ComponentRenderer.RenderComponentAsync<Card>(destination, props, slots);
        return destination.GetOutput();
    }

    [Fact]
    public async Task ShouldRenderIconWhenProvided()
    {
        var html = await RenderCardAsync("My Card", iconName: IconName.Star);

        html.ShouldContain("<svg");
    }

    [Fact]
    public async Task ShouldOmitIconWhenNull()
    {
        var html = await RenderCardAsync("My Card");

        html.ShouldNotContain("<svg");
    }

    [Fact]
    public async Task ShouldRenderSlotContent()
    {
        var html = await RenderCardAsync("My Card", slotContent: "<p>Hello body</p>");

        html.ShouldContain("<p>Hello body</p>");
        html.ShouldContain("card-body");
    }

    [Fact]
    public async Task ShouldHtmlEncodeTitleText()
    {
        var html = await RenderCardAsync("A <script>xss</script>");

        html.ShouldContain("A &lt;script&gt;xss&lt;/script&gt;");
    }
}
