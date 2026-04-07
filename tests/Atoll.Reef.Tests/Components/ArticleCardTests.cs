using Atoll.Components;
using Atoll.Reef.Components;
using Atoll.Rendering;
using Atoll.Slots;

namespace Atoll.Reef.Tests.Components;

public sealed class ArticleCardTests
{
    private static async Task<string> RenderAsync(
        string title = "My Article",
        string slug = "my-article",
        string description = "A description",
        DateTime pubDate = default,
        string? author = null,
        string[] tags = null!,
        string? imageSrc = null,
        string imageAlt = "",
        int? readingTimeMinutes = null,
        string basePath = "")
    {
        var destination = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            [nameof(ArticleCard.Title)] = title,
            [nameof(ArticleCard.Slug)] = slug,
            [nameof(ArticleCard.Description)] = description,
            [nameof(ArticleCard.PubDate)] = pubDate == default ? new DateTime(2026, 1, 1) : pubDate,
            [nameof(ArticleCard.Author)] = author,
            [nameof(ArticleCard.Tags)] = tags ?? [],
            [nameof(ArticleCard.ImageSrc)] = imageSrc,
            [nameof(ArticleCard.ImageAlt)] = imageAlt,
            [nameof(ArticleCard.ReadingTimeMinutes)] = readingTimeMinutes,
            [nameof(ArticleCard.BasePath)] = basePath,
        };
        await ComponentRenderer.RenderComponentAsync<ArticleCard>(destination, props, SlotCollection.Empty);
        return destination.GetOutput();
    }

    [Fact]
    public async Task ShouldRenderTitleAsLink()
    {
        var html = await RenderAsync(title: "Test", slug: "test-slug", basePath: "/blog");
        html.ShouldContain("href=\"/blog/test-slug\"");
    }

    [Fact]
    public async Task ShouldRenderImageWhenProvided()
    {
        var html = await RenderAsync(imageSrc: "/images/cover.jpg", imageAlt: "Cover");
        html.ShouldContain("/images/cover.jpg");
        html.ShouldContain("article-card-image");
        html.ShouldContain("Cover");
    }

    [Fact]
    public async Task ShouldOmitImageWhenNotProvided()
    {
        var html = await RenderAsync();
        html.ShouldNotContain("article-card-image");
    }

    [Fact]
    public async Task ShouldRenderTagsWhenProvided()
    {
        var html = await RenderAsync(tags: ["atoll", "dotnet"]);
        html.ShouldContain("atoll");
        html.ShouldContain("dotnet");
    }

    [Fact]
    public async Task ShouldHtmlEncodeTitle()
    {
        var html = await RenderAsync(title: "<script>xss</script>");
        html.ShouldContain("&lt;script&gt;xss&lt;/script&gt;");
    }

    [Fact]
    public async Task ShouldOmitDescriptionWhenEmpty()
    {
        var html = await RenderAsync(description: "");
        html.ShouldNotContain("article-card-description");
    }
}
