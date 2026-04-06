using Atoll.Components;
using Atoll.Lagoon.Components;
using Atoll.Lagoon.Navigation;
using Atoll.Rendering;
using Shouldly;
using Xunit;

namespace Atoll.Lagoon.Tests.Components;

public sealed class BreadcrumbsTests
{
    private static BreadcrumbItem Crumb(string label, string href) =>
        new BreadcrumbItem(label, href, isCurrent: false);

    private static BreadcrumbItem Current(string label) =>
        new BreadcrumbItem(label, null, isCurrent: true);

    private static async Task<string> RenderBreadcrumbsAsync(IReadOnlyList<BreadcrumbItem> items)
    {
        var destination = new StringRenderDestination();
        var props = new Dictionary<string, object?> { ["Items"] = items };
        await ComponentRenderer.RenderComponentAsync<Breadcrumbs>(destination, props);
        return destination.GetOutput();
    }

    // --- Empty ---

    [Fact]
    public async Task ShouldRenderNothingForEmptyItems()
    {
        var html = await RenderBreadcrumbsAsync([]);

        html.ShouldBeEmpty();
    }

    // --- Current page ---

    [Fact]
    public async Task ShouldRenderCurrentPageWithAriaCurrent()
    {
        var html = await RenderBreadcrumbsAsync([Current("Getting Started")]);

        html.ShouldContain("aria-current=\"page\"");
        html.ShouldContain("Getting Started");
    }

    [Fact]
    public async Task ShouldRenderCurrentPageWithoutAnchorTag()
    {
        var html = await RenderBreadcrumbsAsync([Current("Current Page")]);

        html.ShouldNotContain("<a ");
        html.ShouldContain("Current Page");
    }

    // --- Linked crumbs ---

    [Fact]
    public async Task ShouldRenderLinkedCrumbWithAnchor()
    {
        var html = await RenderBreadcrumbsAsync([
            Crumb("Home", "/"),
            Current("Docs")
        ]);

        html.ShouldContain("href=\"/\"");
        html.ShouldContain("Home");
    }

    // --- Typical breadcrumb structures ---

    [Fact]
    public async Task ShouldRenderTopLevelPageBreadcrumb()
    {
        // Home → Current
        var html = await RenderBreadcrumbsAsync([
            Crumb("Home", "/"),
            Current("Introduction")
        ]);

        html.ShouldContain("href=\"/\"");
        html.ShouldContain("Home");
        html.ShouldContain("Introduction");
        html.ShouldContain("aria-current=\"page\"");
    }

    [Fact]
    public async Task ShouldRenderNestedPageBreadcrumb()
    {
        // Home → Group → Current
        var html = await RenderBreadcrumbsAsync([
            Crumb("Home", "/"),
            Crumb("Guides", "/docs/guides/"),
            Current("Getting Started")
        ]);

        html.ShouldContain("href=\"/\"");
        html.ShouldContain("href=\"/docs/guides/\"");
        html.ShouldContain("Getting Started");
    }

    // --- HTML encoding ---

    [Fact]
    public async Task ShouldHtmlEncodeCurrentPageLabel()
    {
        var html = await RenderBreadcrumbsAsync([Current("<script>xss</script>")]);

        html.ShouldNotContain("<script>");
        html.ShouldContain("&lt;script&gt;");
    }

    [Fact]
    public async Task ShouldHtmlEncodeLinkedCrumbLabel()
    {
        var html = await RenderBreadcrumbsAsync([Crumb("<b>Home</b>", "/"), Current("Page")]);

        html.ShouldNotContain("<b>Home</b>");
        html.ShouldContain("&lt;b&gt;");
    }
}
