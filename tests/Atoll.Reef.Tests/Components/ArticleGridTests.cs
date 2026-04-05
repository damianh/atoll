using Atoll.Components;
using Atoll.Reef.Components;
using Atoll.Rendering;
using Atoll.Slots;
using Shouldly;
using Xunit;

namespace Atoll.Reef.Tests.Components;

public sealed class ArticleGridTests
{
    private static ArticleListItem MakeItem(string title = "Title", string slug = "slug") =>
        new(title, slug, "Desc", new DateTime(2026, 1, 15), null, [], null);

    private static async Task<string> RenderAsync(
        IReadOnlyList<ArticleListItem> items,
        int columns = 3,
        string basePath = "")
    {
        var destination = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            [nameof(ArticleGrid.Items)] = items,
            [nameof(ArticleGrid.Columns)] = columns,
            [nameof(ArticleGrid.BasePath)] = basePath,
        };
        await ComponentRenderer.RenderComponentAsync<ArticleGrid>(destination, props, SlotCollection.Empty);
        return destination.GetOutput();
    }

    [Fact]
    public async Task ShouldRenderGridContainer()
    {
        var html = await RenderAsync([MakeItem()]);
        html.ShouldContain("class=\"article-grid\"");
    }

    [Fact]
    public async Task ShouldSetGridColsCustomProperty()
    {
        var html = await RenderAsync([MakeItem()], columns: 2);
        html.ShouldContain("--grid-cols:2");
    }

    [Fact]
    public async Task ShouldDefaultToThreeColumns()
    {
        var html = await RenderAsync([MakeItem()]);
        html.ShouldContain("--grid-cols:3");
    }

    [Fact]
    public async Task ShouldRenderEachItemAsCard()
    {
        var html = await RenderAsync([MakeItem("Alpha"), MakeItem("Beta")]);
        html.ShouldContain("Alpha");
        html.ShouldContain("Beta");
        html.ShouldContain("article-card");
    }

    [Fact]
    public async Task ShouldRenderEmptyGridWhenNoItems()
    {
        var html = await RenderAsync([]);
        html.ShouldContain("class=\"article-grid\"");
        html.ShouldNotContain("article-card");
    }

    [Fact]
    public async Task ShouldForwardBasePathToCards()
    {
        var html = await RenderAsync([MakeItem(slug: "my-post")], basePath: "/blog");
        html.ShouldContain("href=\"/blog/my-post\"");
    }
}
