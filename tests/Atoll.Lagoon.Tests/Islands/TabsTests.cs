using Atoll.Components;
using Atoll.Instructions;
using Atoll.Islands;
using Atoll.Lagoon.Components;
using Atoll.Lagoon.Islands;
using Atoll.Rendering;
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
}
