using Atoll.Components;
using Atoll.Lagoon.Components;
using Atoll.Rendering;
using Atoll.Slots;
using Shouldly;
using Xunit;

namespace Atoll.Lagoon.Tests.Components;

public sealed class TabItemTests
{
    private static Task<string> RenderTabItemAsync(string label)
        => RenderTabItemAsync(label, null, "<p>Content</p>");

    private static Task<string> RenderTabItemAsync(string label, IconName? iconName)
        => RenderTabItemAsync(label, iconName, "<p>Content</p>");

    private static async Task<string> RenderTabItemAsync(
        string label,
        IconName? iconName,
        string slotContent)
    {
        var destination = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            ["Label"] = label,
            ["IconName"] = iconName,
        };
        var slots = SlotCollection.FromDefault(RenderFragment.FromHtml(slotContent));
        await ComponentRenderer.RenderComponentAsync<TabItem>(destination, props, slots);
        return destination.GetOutput();
    }

    [Fact]
    public async Task ShouldRenderSectionElement()
    {
        var html = await RenderTabItemAsync("npm");

        html.ShouldContain("<section");
        html.ShouldContain("</section>");
    }

    [Fact]
    public async Task ShouldRenderTabPanelClass()
    {
        var html = await RenderTabItemAsync("npm");

        html.ShouldContain("class=\"tab-panel\"");
    }

    [Fact]
    public async Task ShouldRenderDataTabLabelAttribute()
    {
        var html = await RenderTabItemAsync("npm");

        html.ShouldContain("data-tab-label=\"npm\"");
    }

    [Fact]
    public async Task ShouldRenderSlotContentInsideSection()
    {
        var html = await RenderTabItemAsync("npm", null, "<p>Install with npm</p>");

        html.ShouldContain("<p>Install with npm</p>");
        // Slot content must be inside the section element.
        var sectionStart = html.IndexOf("<section", StringComparison.Ordinal);
        var sectionEnd = html.IndexOf("</section>", StringComparison.Ordinal);
        var contentIndex = html.IndexOf("<p>Install with npm</p>", StringComparison.Ordinal);
        contentIndex.ShouldBeGreaterThan(sectionStart);
        contentIndex.ShouldBeLessThan(sectionEnd);
    }

    [Fact]
    public async Task ShouldRenderDataTabIconWhenIconNameProvided()
    {
        var html = await RenderTabItemAsync("npm", IconName.Star);

        html.ShouldContain("data-tab-icon=");
    }

    [Fact]
    public async Task ShouldOmitDataTabIconWhenIconNameIsNull()
    {
        var html = await RenderTabItemAsync("npm");

        html.ShouldNotContain("data-tab-icon");
    }

    [Fact]
    public async Task ShouldHtmlEncodeLabelInDataAttribute()
    {
        var html = await RenderTabItemAsync("A <b>label</b>");

        html.ShouldContain("data-tab-label=\"A &lt;b&gt;label&lt;/b&gt;\"");
    }
}
