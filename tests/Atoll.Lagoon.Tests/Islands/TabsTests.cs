using Atoll.Components;
using Atoll.Instructions;
using Atoll.Islands;
using Atoll.Lagoon.Components;
using Atoll.Lagoon.Islands;
using Atoll.Rendering;
using Atoll.Slots;
using Shouldly;
using Xunit;

namespace Atoll.Lagoon.Tests.Islands;

public sealed class TabsTests
{
    private static IReadOnlyList<TabItemData> MakeTabs(params (string label, string content)[] items) =>
        items.Select(t => new TabItemData(t.label, RenderFragment.FromHtml(t.content))).ToList();

    private static async Task<string> RenderTabsAsync(
        IReadOnlyList<TabItemData> tabItems,
        string? syncKey = null)
    {
        var destination = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            ["TabItems"] = tabItems,
            ["SyncKey"] = syncKey,
        };
        await ComponentRenderer.RenderComponentAsync<Tabs>(destination, props);
        return destination.GetOutput();
    }

    [Fact]
    public async Task ShouldRenderTabButtonsWithRoleTab()
    {
        var html = await RenderTabsAsync(MakeTabs(("Tab A", "<p>A</p>"), ("Tab B", "<p>B</p>")));

        html.ShouldContain("role=\"tab\"");
    }

    [Fact]
    public async Task ShouldRenderTabPanelsWithRoleTabpanel()
    {
        var html = await RenderTabsAsync(MakeTabs(("Tab A", "<p>A</p>"), ("Tab B", "<p>B</p>")));

        html.ShouldContain("role=\"tabpanel\"");
    }

    [Fact]
    public async Task ShouldSetFirstTabAriaSelectedTrue()
    {
        var html = await RenderTabsAsync(MakeTabs(("Tab A", "<p>A</p>"), ("Tab B", "<p>B</p>")));

        html.ShouldContain("aria-selected=\"true\"");
    }

    [Fact]
    public async Task ShouldSetSubsequentTabsAriaSelectedFalse()
    {
        var html = await RenderTabsAsync(MakeTabs(("Tab A", "<p>A</p>"), ("Tab B", "<p>B</p>")));

        html.ShouldContain("aria-selected=\"false\"");
    }

    [Fact]
    public async Task ShouldHideSecondPanelWithHiddenAttribute()
    {
        var html = await RenderTabsAsync(MakeTabs(("Tab A", "<p>A</p>"), ("Tab B", "<p>B</p>")));

        html.ShouldContain("hidden");
    }

    [Fact]
    public async Task ShouldRenderSyncKeyDataAttribute()
    {
        var html = await RenderTabsAsync(MakeTabs(("Tab A", "<p>A</p>")), syncKey: "os");

        html.ShouldContain("data-sync-key=\"os\"");
    }

    [Fact]
    public async Task ShouldOmitSyncKeyWhenNull()
    {
        var html = await RenderTabsAsync(MakeTabs(("Tab A", "<p>A</p>")), syncKey: null);

        html.ShouldNotContain("data-sync-key");
    }

    [Fact]
    public async Task ShouldHtmlEncodeTabLabel()
    {
        var html = await RenderTabsAsync(MakeTabs(("<b>Bold</b>", "<p>content</p>")));

        html.ShouldContain("&lt;b&gt;Bold&lt;/b&gt;");
        html.ShouldNotContain("<b>Bold</b>");
    }

    [Fact]
    public async Task ShouldHaveClientLoadDirective()
    {
        var island = new Tabs();
        var metadata = island.CreateMetadata();

        metadata.ShouldNotBeNull();
        metadata!.DirectiveType.ShouldBe(ClientDirectiveType.Load);
    }

    [Fact]
    public async Task ShouldHaveCorrectClientModuleUrl()
    {
        var island = new Tabs();

        island.ClientModuleUrl.ShouldBe("/scripts/atoll-docs-tabs.js");
    }

    [Fact]
    public async Task ShouldRenderPanelContent()
    {
        var html = await RenderTabsAsync(MakeTabs(("Tab A", "<p>Hello from A</p>")));

        html.ShouldContain("<p>Hello from A</p>");
    }

    [Fact]
    public async Task ShouldRenderIconInTabButtonWhenProvided()
    {
        var tabItems = new List<TabItemData>
        {
            new TabItemData("Stars", RenderFragment.FromHtml("<p>content</p>"), IconName.Star),
        };

        var html = await RenderTabsAsync(tabItems);

        html.ShouldContain("<svg ");
    }

    // ── Slot-based rendering mode ──

    private static async Task<string> RenderTabsWithSlotAsync(
        string slotContent,
        string? syncKey = null)
    {
        var destination = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            ["SyncKey"] = syncKey,
        };
        var slots = SlotCollection.FromDefault(RenderFragment.FromHtml(slotContent));
        await ComponentRenderer.RenderComponentAsync<Tabs>(destination, props, slots);
        return destination.GetOutput();
    }

    [Fact]
    public async Task ShouldRenderWrapperDivWithTabsClassInSlotMode()
    {
        var html = await RenderTabsWithSlotAsync(
            "<section class=\"tab-panel\" data-tab-label=\"A\">Content A</section>");

        html.ShouldContain("class=\"tabs\"");
    }

    [Fact]
    public async Task ShouldRenderSyncKeyOnWrapperInSlotMode()
    {
        var html = await RenderTabsWithSlotAsync(
            "<section class=\"tab-panel\" data-tab-label=\"A\">Content A</section>",
            syncKey: "pkg");

        html.ShouldContain("data-sync-key=\"pkg\"");
    }

    [Fact]
    public async Task ShouldOmitSyncKeyOnWrapperInSlotModeWhenNull()
    {
        var html = await RenderTabsWithSlotAsync(
            "<section class=\"tab-panel\" data-tab-label=\"A\">Content A</section>");

        html.ShouldNotContain("data-sync-key");
    }

    [Fact]
    public async Task ShouldRenderSlotContentInsideWrapperInSlotMode()
    {
        const string slotContent = "<section class=\"tab-panel\" data-tab-label=\"A\">Content A</section>";

        var html = await RenderTabsWithSlotAsync(slotContent);

        html.ShouldContain(slotContent);
        // Slot content must be inside the tabs div.
        var tabsStart = html.IndexOf("<div class=\"tabs\"", StringComparison.Ordinal);
        var tabsEnd = html.IndexOf("</div>", tabsStart, StringComparison.Ordinal);
        var contentIndex = html.IndexOf(slotContent, StringComparison.Ordinal);
        contentIndex.ShouldBeGreaterThan(tabsStart);
        contentIndex.ShouldBeLessThan(tabsEnd);
    }

    [Fact]
    public async Task ShouldNotRenderTablistInSlotMode()
    {
        var html = await RenderTabsWithSlotAsync(
            "<section class=\"tab-panel\" data-tab-label=\"A\">Content A</section>");

        // In slot mode the JS builds the tablist; server should not render it.
        html.ShouldNotContain("role=\"tablist\"");
    }

    [Fact]
    public async Task ShouldPreserveIslandMetadataInSlotMode()
    {
        var island = new Tabs();

        island.ClientModuleUrl.ShouldBe("/scripts/atoll-docs-tabs.js");
        island.CreateMetadata()!.DirectiveType.ShouldBe(ClientDirectiveType.Load);
    }
}
