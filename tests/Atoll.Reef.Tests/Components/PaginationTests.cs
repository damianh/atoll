using Atoll.Components;
using Atoll.Reef.Components;
using Atoll.Reef.Navigation;
using Atoll.Rendering;
using Atoll.Slots;
using Shouldly;
using Xunit;

namespace Atoll.Reef.Tests.Components;

public sealed class PaginationTests
{
    private static async Task<string> RenderAsync(PaginationInfo info)
    {
        var destination = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            [nameof(Pagination.Info)] = info,
        };
        await ComponentRenderer.RenderComponentAsync<Pagination>(destination, props, SlotCollection.Empty);
        return destination.GetOutput();
    }

    [Fact]
    public async Task ShouldRenderNavElement()
    {
        var html = await RenderAsync(new PaginationInfo(1, 3, "/blog"));
        html.ShouldContain("<nav");
        html.ShouldContain("pagination");
    }

    [Fact]
    public async Task ShouldRenderPageNumbers()
    {
        var html = await RenderAsync(new PaginationInfo(2, 3, "/blog"));
        html.ShouldContain(">1<");
        html.ShouldContain(">2<");
        html.ShouldContain(">3<");
    }

    [Fact]
    public async Task ShouldMarkCurrentPage()
    {
        var html = await RenderAsync(new PaginationInfo(2, 3, "/blog"));
        html.ShouldContain("pagination-current");
        html.ShouldContain("aria-current=\"page\"");
    }

    [Fact]
    public async Task ShouldRenderPreviousLinkWhenNotFirstPage()
    {
        var html = await RenderAsync(new PaginationInfo(2, 3, "/blog"));
        html.ShouldContain("pagination-prev");
        html.ShouldContain("href=\"/blog\"");
    }

    [Fact]
    public async Task ShouldDisablePreviousOnFirstPage()
    {
        var html = await RenderAsync(new PaginationInfo(1, 3, "/blog"));
        html.ShouldContain("pagination-disabled");
        html.ShouldNotContain("href=\"/blog\"");
    }

    [Fact]
    public async Task ShouldRenderNextLinkWhenNotLastPage()
    {
        var html = await RenderAsync(new PaginationInfo(2, 3, "/blog"));
        html.ShouldContain("pagination-next");
        html.ShouldContain("href=\"/blog/page/3\"");
    }

    [Fact]
    public async Task ShouldDisableNextOnLastPage()
    {
        var html = await RenderAsync(new PaginationInfo(3, 3, "/blog"));
        html.ShouldContain("pagination-next");
        html.ShouldContain("pagination-disabled");
    }

    [Fact]
    public async Task ShouldRenderNothingWhenOnlyOnePage()
    {
        var html = await RenderAsync(new PaginationInfo(1, 1, "/blog"));
        html.ShouldBeEmpty();
    }

    [Fact]
    public async Task ShouldRenderEllipsisForLargePageCount()
    {
        var html = await RenderAsync(new PaginationInfo(6, 12, "/blog"));
        html.ShouldContain("pagination-ellipsis");
    }

    [Fact]
    public async Task ShouldAlwaysIncludeFirstAndLastPage()
    {
        var html = await RenderAsync(new PaginationInfo(6, 12, "/blog"));
        html.ShouldContain(">1<");
        html.ShouldContain(">12<");
    }
}
