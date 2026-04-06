using Atoll.Components;
using Atoll.Lagoon.Components;
using Atoll.Lagoon.Configuration;
using Atoll.Lagoon.Navigation;
using Atoll.Rendering;
using Shouldly;
using Xunit;
namespace Atoll.Lagoon.Tests.Components;

public sealed class SidebarTests
{
    private static ResolvedSidebarItem Link(string label, string href, bool isCurrent = false)
        => new ResolvedSidebarItem(label, href, isCurrent, null);

    private static ResolvedSidebarItem Group(
        string label,
        IReadOnlyList<ResolvedSidebarItem> items,
        bool isActive = false,
        bool collapsed = false)
        => new ResolvedSidebarItem(label, isActive, null, collapsed, items);

    private static async Task<string> RenderSidebarAsync(IReadOnlyList<ResolvedSidebarItem> items)
    {
        var destination = new StringRenderDestination();
        var props = new Dictionary<string, object?> { ["Items"] = items };
        await ComponentRenderer.RenderComponentAsync<Sidebar>(destination, props);
        return destination.GetOutput();
    }

    private static async Task<string> RenderSidebarAsync(
        IReadOnlyList<ResolvedSidebarItem> items,
        SidebarChevronPosition chevronPosition)
    {
        var destination = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            ["Items"] = items,
            ["ChevronPosition"] = chevronPosition,
        };
        await ComponentRenderer.RenderComponentAsync<Sidebar>(destination, props);
        return destination.GetOutput();
    }

    // --- Structure ---

    [Fact]
    public async Task ShouldRenderSingleLinkItem()
    {
        var html = await RenderSidebarAsync([Link("Home", "/")]);

        html.ShouldContain("<a href=\"/\">");
        html.ShouldContain("Home");
        html.ShouldContain("</a>");
    }

    [Fact]
    public async Task ShouldRenderMultipleLinkItems()
    {
        var html = await RenderSidebarAsync([
            Link("Home", "/"),
            Link("About", "/about")
        ]);

        html.ShouldContain("href=\"/\"");
        html.ShouldContain("href=\"/about\"");
    }

    // --- Active state ---

    [Fact]
    public async Task ShouldAddAriaCurentPageForCurrentItem()
    {
        var html = await RenderSidebarAsync([Link("Home", "/", isCurrent: true)]);

        html.ShouldContain("aria-current=\"page\"");
    }

    [Fact]
    public async Task ShouldNotAddAriaCurentForNonCurrentItem()
    {
        var html = await RenderSidebarAsync([Link("Home", "/", isCurrent: false)]);

        html.ShouldNotContain("aria-current");
    }

    [Fact]
    public async Task ShouldAddActiveCssClassForActiveItem()
    {
        var html = await RenderSidebarAsync([Link("Home", "/", isCurrent: true)]);

        html.ShouldContain("class=\"active\"");
    }

    // --- Badge ---

    [Fact]
    public async Task ShouldRenderBadgeOnLinkItem()
    {
        var item = new ResolvedSidebarItem("New Feature", "/feature", false, new SidebarBadge("New"));
        var html = await RenderSidebarAsync([item]);

        html.ShouldContain("<span class=\"sidebar-badge\">New</span>");
    }

    [Fact]
    public async Task ShouldRenderBadgeWithSuccessVariantClass()
    {
        var item = new ResolvedSidebarItem("New Feature", "/feature", false, new SidebarBadge("New", BadgeVariant.Success));
        var html = await RenderSidebarAsync([item]);

        html.ShouldContain("class=\"sidebar-badge sidebar-badge-success\"");
        html.ShouldContain("New");
    }

    [Fact]
    public async Task ShouldRenderBadgeWithDefaultVariantWithoutVariantSuffix()
    {
        var item = new ResolvedSidebarItem("New Feature", "/feature", false, new SidebarBadge("New", BadgeVariant.Default));
        var html = await RenderSidebarAsync([item]);

        html.ShouldContain("class=\"sidebar-badge\"");
        html.ShouldNotContain("sidebar-badge-default");
    }

    [Fact]
    public async Task ShouldRenderBadgeViaImplicitStringConversion()
    {
        SidebarBadge badge = "OSS";
        var item = new ResolvedSidebarItem("Feature", "/feature", false, badge);
        var html = await RenderSidebarAsync([item]);

        html.ShouldContain("<span class=\"sidebar-badge\">OSS</span>");
    }

    // --- Groups ---

    [Fact]
    public async Task ShouldRenderGroupWithDetailsAndSummary()
    {
        var html = await RenderSidebarAsync([
            Group("Guides", [Link("Start", "/guides/start")])
        ]);

        html.ShouldContain("<details");
        html.ShouldContain("<summary>");
        html.ShouldContain("Guides");
        html.ShouldContain("</summary>");
        html.ShouldContain("</details>");
    }

    [Fact]
    public async Task ShouldRenderGroupCollapsedByDefault()
    {
        var html = await RenderSidebarAsync([
            Group("Guides", [Link("Start", "/guides/start")])
        ]);

        html.ShouldNotContain(" open>");
    }

    [Fact]
    public async Task ShouldRenderCollapsedGroupWithoutOpenAttribute()
    {
        var html = await RenderSidebarAsync([
            Group("Guides", [Link("Start", "/guides/start")], collapsed: true)
        ]);

        html.ShouldNotContain(" open>");
    }

    [Fact]
    public async Task ShouldRenderActiveGroupAsOpen()
    {
        var html = await RenderSidebarAsync([
            Group("Guides", [Link("Start", "/guides/start", isCurrent: true)], isActive: true)
        ]);

        html.ShouldContain(" open>");
    }

    [Fact]
    public async Task ShouldRenderCollapsedButActiveGroupAsOpen()
    {
        var html = await RenderSidebarAsync([
            Group("Guides", [Link("Start", "/guides/start", isCurrent: true)], isActive: true, collapsed: true)
        ]);

        html.ShouldContain(" open>");
    }

    [Fact]
    public async Task ShouldRenderNestedGroupsRecursively()
    {
        var html = await RenderSidebarAsync([
            Group("Docs", [
                Group("Guides", [Link("Start", "/guides/start")])
            ])
        ]);

        html.ShouldContain("Docs");
        html.ShouldContain("Guides");
        html.ShouldContain("href=\"/guides/start\"");
    }

    // --- data-index ---

    [Fact]
    public async Task ShouldRenderDataIndexOnDetailsElements()
    {
        var html = await RenderSidebarAsync([
            Group("Alpha", [Link("A", "/a")]),
            Group("Beta", [Link("B", "/b")]),
        ]);

        html.ShouldContain("data-index=\"0\"");
        html.ShouldContain("data-index=\"1\"");
    }

    [Fact]
    public async Task ShouldAssignSequentialIndicesForNestedGroups()
    {
        var html = await RenderSidebarAsync([
            Group("Outer", [
                Group("Inner", [Link("Start", "/start")])
            ])
        ]);

        html.ShouldContain("data-index=\"0\"");
        html.ShouldContain("data-index=\"1\"");
    }

    [Fact]
    public async Task ShouldRenderSidebarRestoreCustomElement()
    {
        var html = await RenderSidebarAsync([
            Group("Guides", [Link("Start", "/guides/start")])
        ]);

        html.ShouldContain("<sl-sidebar-restore data-index=\"0\"></sl-sidebar-restore>");
    }

    // --- data-active ---

    [Fact]
    public async Task ShouldRenderDataActiveOnActiveGroup()
    {
        var html = await RenderSidebarAsync([
            Group("Guides", [Link("Start", "/guides/start")], isActive: true)
        ]);

        html.ShouldContain("data-active");
    }

    [Fact]
    public async Task ShouldNotRenderDataActiveOnInactiveGroup()
    {
        var html = await RenderSidebarAsync([
            Group("Guides", [Link("Start", "/guides/start")], isActive: false)
        ]);

        // The <details> element must not carry data-active; the attribute only appears in
        // the inline JS string as a literal (hasAttribute('data-active')), so we verify
        // the details tag itself doesn't include the attribute.
        html.ShouldNotContain("<details class=\"sidebar-chevron-end\" data-index=\"0\" data-active");
    }

    // --- Hash computation ---

    [Fact]
    public async Task ShouldComputeStableHash()
    {
        var items = new[] { Group("Alpha", [Link("A", "/a")]) };
        var html1 = await RenderSidebarAsync(items);
        var html2 = await RenderSidebarAsync(items);

        // Extract hash values and confirm they match
        var hash1 = ExtractDataHash(html1);
        var hash2 = ExtractDataHash(html2);
        hash1.ShouldNotBeNullOrEmpty();
        hash1.ShouldBe(hash2);
    }

    [Fact]
    public async Task ShouldComputeDifferentHashForDifferentStructure()
    {
        var html1 = await RenderSidebarAsync([Group("Alpha", [Link("A", "/a")])]);
        var html2 = await RenderSidebarAsync([Group("Beta", [Link("B", "/b")])]);

        ExtractDataHash(html1).ShouldNotBe(ExtractDataHash(html2));
    }

    private static string ExtractDataHash(string html)
    {
        var marker = "data-hash=\"";
        var start = html.IndexOf(marker, StringComparison.Ordinal);
        if (start < 0) return string.Empty;
        start += marker.Length;
        var end = html.IndexOf('"', start);
        return end < 0 ? string.Empty : html[start..end];
    }

    // --- HTML encoding ---

    [Fact]
    public async Task ShouldHtmlEncodeSpecialCharactersInLabel()
    {
        var html = await RenderSidebarAsync([Link("<script>", "/safe")]);

        html.ShouldNotContain("<a href=\"/safe\"><script>");
        html.ShouldContain("&lt;script&gt;");
    }

    [Fact]
    public async Task ShouldHtmlEncodeSpecialCharactersInHref()
    {
        var html = await RenderSidebarAsync([Link("Page", "/path?a=1&b=2")]);

        html.ShouldContain("href=\"/path?a=1&amp;b=2\"");
    }

    // --- Chevron ---

    [Fact]
    public async Task ShouldRenderEndChevronPositionClassByDefault()
    {
        var html = await RenderSidebarAsync([
            Group("Guides", [Link("Start", "/guides/start")])
        ]);

        html.ShouldContain("class=\"sidebar-chevron-end\"");
    }

    [Fact]
    public async Task ShouldRenderStartChevronPositionClassWhenConfigured()
    {
        var html = await RenderSidebarAsync(
            [Group("Guides", [Link("Start", "/guides/start")])],
            SidebarChevronPosition.Start);

        html.ShouldContain("class=\"sidebar-chevron-start\"");
    }

    [Fact]
    public async Task ShouldPropagateChevronPositionToNestedGroups()
    {
        var html = await RenderSidebarAsync(
            [Group("Docs", [Group("Guides", [Link("Start", "/guides/start")])])],
            SidebarChevronPosition.Start);

        // Both the outer and inner group should use start position
        var count = html.Split("sidebar-chevron-start").Length - 1;
        count.ShouldBe(2);
    }
}
