using Atoll.Components;
using Atoll.Reef.Components;
using Atoll.Rendering;
using Shouldly;
using Xunit;

namespace Atoll.Reef.Tests.Components;

public sealed class ArticleTableTests
{
    private static ArticleListItem MakeItem(
        string slug = "post",
        string title = "Post Title",
        string? author = "Alice",
        string[] tags = null!,
        int? readingTime = 5) =>
        new(title, slug, "Description", new DateTime(2025, 1, 1), author, tags ?? ["dotnet"], readingTime);

    private static async Task<string> RenderAsync(
        IReadOnlyList<ArticleListItem>? items = null,
        bool showAuthor = true,
        bool showTags = true,
        bool showReadingTime = true,
        string basePath = "/articles")
    {
        var dest = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            ["Items"] = items ?? [MakeItem()],
            ["ShowAuthor"] = showAuthor,
            ["ShowTags"] = showTags,
            ["ShowReadingTime"] = showReadingTime,
            ["BasePath"] = basePath,
        };
        await ComponentRenderer.RenderComponentAsync<ArticleTable>(dest, props);
        return dest.GetOutput();
    }

    [Fact]
    public async Task ShouldRenderAuthorColumnWhenEnabled()
    {
        var html = await RenderAsync(showAuthor: true);

        html.ShouldContain("<th scope=\"col\">Author</th>");
        html.ShouldContain("Alice");
    }

    [Fact]
    public async Task ShouldHideAuthorColumnWhenDisabled()
    {
        var html = await RenderAsync(showAuthor: false);

        html.ShouldNotContain("<th scope=\"col\">Author</th>");
    }

    [Fact]
    public async Task ShouldRenderTagsColumnWhenEnabled()
    {
        var html = await RenderAsync(showTags: true);

        html.ShouldContain("<th scope=\"col\">Tags</th>");
        html.ShouldContain("dotnet");
    }

    [Fact]
    public async Task ShouldHideTagsColumnWhenDisabled()
    {
        var html = await RenderAsync(showTags: false);

        html.ShouldNotContain("<th scope=\"col\">Tags</th>");
    }

    [Fact]
    public async Task ShouldRenderReadingTimeColumnWhenEnabled()
    {
        var html = await RenderAsync(showReadingTime: true);

        html.ShouldContain("<th scope=\"col\">Reading Time</th>");
        html.ShouldContain("5 min");
    }

    [Fact]
    public async Task ShouldHideReadingTimeColumnWhenDisabled()
    {
        var html = await RenderAsync(showReadingTime: false);

        html.ShouldNotContain("<th scope=\"col\">Reading Time</th>");
    }

    [Fact]
    public async Task ShouldRenderTitleAsLink()
    {
        var items = new[] { MakeItem(slug: "my-article") };

        var html = await RenderAsync(items, basePath: "/blog");

        html.ShouldContain("href=\"/blog/my-article\"");
    }
}
