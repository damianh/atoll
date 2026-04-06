using Atoll.Components;
using Atoll.Reef.Components;
using Atoll.Rendering;
using Shouldly;
using Xunit;

namespace Atoll.Reef.Tests.Components;

public sealed class ArticleTimelineTests
{
    private static ArticleListItem MakeItem(string slug, string title, DateTime pubDate) =>
        new(title, slug, "", pubDate, null, [], null);

    private static async Task<string> RenderAsync(
        IReadOnlyList<ArticleListItem>? items = null,
        bool groupByMonth = false,
        string basePath = "/articles")
    {
        var dest = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            ["Items"] = items ?? [],
            ["GroupByMonth"] = groupByMonth,
            ["BasePath"] = basePath,
        };
        await ComponentRenderer.RenderComponentAsync<ArticleTimeline>(dest, props);
        return dest.GetOutput();
    }

    [Fact]
    public async Task ShouldGroupByYear()
    {
        var items = new[]
        {
            MakeItem("post-1", "Post One", new DateTime(2025, 3, 1)),
            MakeItem("post-2", "Post Two", new DateTime(2024, 6, 15)),
        };

        var html = await RenderAsync(items);

        html.ShouldContain(">2025<");
        html.ShouldContain(">2024<");
    }

    [Fact]
    public async Task ShouldRenderArticleTitlesAsLinks()
    {
        var items = new[]
        {
            MakeItem("my-post", "My Post Title", new DateTime(2025, 1, 1)),
        };

        var html = await RenderAsync(items, basePath: "/blog");

        html.ShouldContain("href=\"/blog/my-post\"");
        html.ShouldContain("My Post Title");
    }

    [Fact]
    public async Task ShouldRenderTimelineEntryWithDate()
    {
        var items = new[]
        {
            MakeItem("post", "Title", new DateTime(2025, 4, 5)),
        };

        var html = await RenderAsync(items);

        html.ShouldContain("datetime=\"2025-04-05\"");
        html.ShouldContain("class=\"timeline-entries\"");
    }

    [Fact]
    public async Task ShouldGroupByMonthWhenEnabled()
    {
        var items = new[]
        {
            MakeItem("post-jan", "January Post", new DateTime(2025, 1, 10)),
            MakeItem("post-mar", "March Post", new DateTime(2025, 3, 5)),
        };

        var html = await RenderAsync(items, groupByMonth: true);

        html.ShouldContain("March 2025");
        html.ShouldContain("January 2025");
    }

    [Fact]
    public async Task ShouldOrderArticlesNewestFirst()
    {
        var items = new[]
        {
            MakeItem("old", "Old Post", new DateTime(2023, 1, 1)),
            MakeItem("new", "New Post", new DateTime(2025, 1, 1)),
        };

        var html = await RenderAsync(items);

        var newPos = html.IndexOf(">2025<", StringComparison.Ordinal);
        var oldPos = html.IndexOf(">2023<", StringComparison.Ordinal);
        newPos.ShouldBeLessThan(oldPos);
    }

    [Fact]
    public async Task ShouldRenderEmptyTimelineWithNoItems()
    {
        var html = await RenderAsync([]);

        html.ShouldContain("article-timeline");
        html.ShouldNotContain("timeline-year");
    }
}
