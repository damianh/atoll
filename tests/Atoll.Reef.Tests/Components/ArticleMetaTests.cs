using Atoll.Components;
using Atoll.Reef.Components;
using Atoll.Rendering;
using Atoll.Slots;
using Shouldly;
using Xunit;

namespace Atoll.Reef.Tests.Components;

public sealed class ArticleMetaTests
{
    private static async Task<string> RenderAsync(
        DateTime pubDate,
        string? author = null,
        int? readingTimeMinutes = null,
        string[] tags = null!,
        string basePath = "")
    {
        var destination = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            [nameof(ArticleMeta.PubDate)] = pubDate,
            [nameof(ArticleMeta.Author)] = author,
            [nameof(ArticleMeta.ReadingTimeMinutes)] = readingTimeMinutes,
            [nameof(ArticleMeta.Tags)] = tags ?? [],
            [nameof(ArticleMeta.BasePath)] = basePath,
        };
        await ComponentRenderer.RenderComponentAsync<ArticleMeta>(destination, props, SlotCollection.Empty);
        return destination.GetOutput();
    }

    [Fact]
    public async Task ShouldRenderPublicationDate()
    {
        var html = await RenderAsync(new DateTime(2026, 1, 15));
        html.ShouldContain("2026-01-15");
        html.ShouldContain("January 15, 2026");
    }

    [Fact]
    public async Task ShouldRenderAuthorWhenProvided()
    {
        var html = await RenderAsync(new DateTime(2026, 1, 15), author: "Alice");
        html.ShouldContain("Alice");
        html.ShouldContain("article-author");
    }

    [Fact]
    public async Task ShouldOmitAuthorWhenNull()
    {
        var html = await RenderAsync(new DateTime(2026, 1, 15));
        html.ShouldNotContain("article-author");
    }

    [Fact]
    public async Task ShouldRenderReadingTimeWhenProvided()
    {
        var html = await RenderAsync(new DateTime(2026, 1, 15), readingTimeMinutes: 5);
        html.ShouldContain("5 min read");
    }

    [Fact]
    public async Task ShouldOmitReadingTimeWhenNull()
    {
        var html = await RenderAsync(new DateTime(2026, 1, 15));
        html.ShouldNotContain("min read");
    }

    [Fact]
    public async Task ShouldRenderTagsAsPills()
    {
        var html = await RenderAsync(new DateTime(2026, 1, 15), tags: ["atoll", "tutorial"]);
        html.ShouldContain("atoll");
        html.ShouldContain("tutorial");
        html.ShouldContain("tag-pill");
    }

    [Fact]
    public async Task ShouldRenderTagLinksWithBasePath()
    {
        var html = await RenderAsync(new DateTime(2026, 1, 15), tags: ["dotnet"], basePath: "/blog");
        html.ShouldContain("/blog/tag/dotnet");
    }

    [Fact]
    public async Task ShouldOmitTagsListWhenEmpty()
    {
        var html = await RenderAsync(new DateTime(2026, 1, 15), tags: []);
        html.ShouldNotContain("article-tags");
    }

    [Fact]
    public async Task ShouldHtmlEncodeAuthor()
    {
        var html = await RenderAsync(new DateTime(2026, 1, 15), author: "<b>Bob</b>");
        html.ShouldContain("&lt;b&gt;Bob&lt;/b&gt;");
    }
}
