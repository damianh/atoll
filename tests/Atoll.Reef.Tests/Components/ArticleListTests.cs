using Atoll.Components;
using Atoll.Reef.Components;
using Atoll.Rendering;
using Atoll.Slots;
using Shouldly;
using Xunit;

namespace Atoll.Reef.Tests.Components;

public sealed class ArticleListTests
{
    private static ArticleListItem MakeItem(string title = "Title", string slug = "slug", string description = "Desc") =>
        new(title, slug, description, new DateTime(2026, 1, 15), null, [], null);

    private static async Task<string> RenderAsync(
        IReadOnlyList<ArticleListItem> items,
        string basePath = "")
    {
        var destination = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            [nameof(ArticleList.Items)] = items,
            [nameof(ArticleList.BasePath)] = basePath,
        };
        await ComponentRenderer.RenderComponentAsync<ArticleList>(destination, props, SlotCollection.Empty);
        return destination.GetOutput();
    }

    [Fact]
    public async Task ShouldRenderEachItemAsArticle()
    {
        var html = await RenderAsync([MakeItem("Alpha"), MakeItem("Beta")]);
        html.ShouldContain("Alpha");
        html.ShouldContain("Beta");
        html.ShouldContain("article-list-item");
    }

    [Fact]
    public async Task ShouldRenderItemTitleAsLink()
    {
        var html = await RenderAsync([MakeItem(title: "Post", slug: "post-slug")], basePath: "/blog");
        html.ShouldContain("href=\"/blog/post-slug\"");
    }

    [Fact]
    public async Task ShouldRenderItemDescription()
    {
        var html = await RenderAsync([MakeItem(description: "A great post")]);
        html.ShouldContain("A great post");
    }

    [Fact]
    public async Task ShouldOmitDescriptionWhenEmpty()
    {
        var html = await RenderAsync([MakeItem(description: "")]);
        html.ShouldNotContain("article-list-item-description");
    }

    [Fact]
    public async Task ShouldRenderEmptyListWhenNoItems()
    {
        var html = await RenderAsync([]);
        html.ShouldContain("class=\"article-list\"");
        html.ShouldNotContain("article-list-item");
    }

    [Fact]
    public async Task ShouldHtmlEncodeTitleText()
    {
        var html = await RenderAsync([MakeItem(title: "<b>Injected</b>")]);
        html.ShouldContain("&lt;b&gt;Injected&lt;/b&gt;");
    }
}
