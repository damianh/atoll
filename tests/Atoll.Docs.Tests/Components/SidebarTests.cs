using Atoll.Components;
using Atoll.Docs.Components;
using Atoll.Docs.Navigation;
using Atoll.Rendering;
using Shouldly;
using Xunit;

namespace Atoll.Docs.Tests.Components;

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

    // --- Structure ---

    [Fact]
    public async Task ShouldRenderNavWithAriaLabel()
    {
        var html = await RenderSidebarAsync([]);

        html.ShouldContain("<nav aria-label=\"Main\">");
        html.ShouldContain("</nav>");
    }

    [Fact]
    public async Task ShouldRenderEmptySidebarWithEmptyList()
    {
        var html = await RenderSidebarAsync([]);

        html.ShouldBe("<nav aria-label=\"Main\"><ul></ul></nav>");
    }

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
        var item = new ResolvedSidebarItem("New Feature", "/feature", false, "New");
        var html = await RenderSidebarAsync([item]);

        html.ShouldContain("<span class=\"badge\">New</span>");
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
    public async Task ShouldRenderGroupOpenByDefault()
    {
        var html = await RenderSidebarAsync([
            Group("Guides", [Link("Start", "/guides/start")])
        ]);

        html.ShouldContain("<details open>");
    }

    [Fact]
    public async Task ShouldRenderCollapsedGroupWithoutOpenAttribute()
    {
        var html = await RenderSidebarAsync([
            Group("Guides", [Link("Start", "/guides/start")], collapsed: true)
        ]);

        html.ShouldNotContain("<details open>");
        html.ShouldContain("<details>");
    }

    [Fact]
    public async Task ShouldRenderCollapsedButActiveGroupAsOpen()
    {
        var html = await RenderSidebarAsync([
            Group("Guides", [Link("Start", "/guides/start", isCurrent: true)], isActive: true, collapsed: true)
        ]);

        html.ShouldContain("<details open>");
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

    // --- HTML encoding ---

    [Fact]
    public async Task ShouldHtmlEncodeSpecialCharactersInLabel()
    {
        var html = await RenderSidebarAsync([Link("<script>", "/safe")]);

        html.ShouldNotContain("<script>");
        html.ShouldContain("&lt;script&gt;");
    }

    [Fact]
    public async Task ShouldHtmlEncodeSpecialCharactersInHref()
    {
        var html = await RenderSidebarAsync([Link("Page", "/path?a=1&b=2")]);

        html.ShouldContain("href=\"/path?a=1&amp;b=2\"");
    }
}
