using Atoll.Components;
using Atoll.Lagoon.Components;
using Atoll.Lagoon.Navigation;
using Atoll.Rendering;

namespace Atoll.Lagoon.Tests.Components;

public sealed class PaginationTests
{
    private static PaginationLink Link(string label, string href) => new PaginationLink(label, href);

    private static async Task<string> RenderPaginationAsync(PaginationLink? previous, PaginationLink? next)
    {
        var destination = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            ["Previous"] = previous,
            ["Next"] = next
        };
        await ComponentRenderer.RenderComponentAsync<Pagination>(destination, props);
        return destination.GetOutput();
    }

    // --- Empty cases ---

    [Fact]
    public async Task ShouldRenderNothingWhenNoPreviousOrNext()
    {
        var html = await RenderPaginationAsync(null, null);

        html.ShouldBeEmpty();
    }

    // --- Previous link ---

    [Fact]
    public async Task ShouldRenderPreviousLinkWithRelPrev()
    {
        var html = await RenderPaginationAsync(Link("Getting Started", "/docs/getting-started/"), null);

        html.ShouldContain("rel=\"prev\"");
        html.ShouldContain("href=\"/docs/getting-started/\"");
    }

    [Fact]
    public async Task ShouldNotRenderPreviousWhenNull()
    {
        var html = await RenderPaginationAsync(null, Link("Next Page", "/docs/next/"));

        html.ShouldNotContain("rel=\"prev\"");
        html.ShouldNotContain("Previous");
    }

    // --- Next link ---

    [Fact]
    public async Task ShouldRenderNextLinkWithRelNext()
    {
        var html = await RenderPaginationAsync(null, Link("Advanced", "/docs/advanced/"));

        html.ShouldContain("rel=\"next\"");
        html.ShouldContain("href=\"/docs/advanced/\"");
    }

    [Fact]
    public async Task ShouldNotRenderNextWhenNull()
    {
        var html = await RenderPaginationAsync(Link("Previous Page", "/docs/prev/"), null);

        html.ShouldNotContain("rel=\"next\"");
        html.ShouldNotContain("Next");
    }

    // --- Both directions ---

    [Fact]
    public async Task ShouldRenderBothPreviousAndNext()
    {
        var html = await RenderPaginationAsync(
            Link("Intro", "/docs/intro/"),
            Link("Advanced", "/docs/advanced/"));

        html.ShouldContain("rel=\"prev\"");
        html.ShouldContain("rel=\"next\"");
        html.ShouldContain("Intro");
        html.ShouldContain("Advanced");
    }

    // --- HTML encoding ---

    [Fact]
    public async Task ShouldHtmlEncodePreviousLabel()
    {
        var html = await RenderPaginationAsync(Link("<script>xss</script>", "/docs/page/"), null);

        html.ShouldNotContain("<script>");
        html.ShouldContain("&lt;script&gt;");
    }

    [Fact]
    public async Task ShouldHtmlEncodeNextLabel()
    {
        var html = await RenderPaginationAsync(null, Link("<b>Bold</b>", "/docs/page/"));

        html.ShouldNotContain("<b>Bold</b>");
        html.ShouldContain("&lt;b&gt;");
    }
}
