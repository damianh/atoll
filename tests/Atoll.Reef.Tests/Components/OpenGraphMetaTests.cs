using Atoll.Components;
using Atoll.Reef.Components;
using Atoll.Rendering;
using Shouldly;
using Xunit;

namespace Atoll.Reef.Tests.Components;

public sealed class OpenGraphMetaTests
{
    private static async Task<string> RenderAsync(
        string title = "My Article",
        string? description = null,
        string? imageUrl = null,
        string? url = null,
        string? author = null,
        DateTime? pubDate = null,
        string siteName = "")
    {
        var dest = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            [nameof(OpenGraphMeta.Title)] = title,
            [nameof(OpenGraphMeta.Description)] = description,
            [nameof(OpenGraphMeta.ImageUrl)] = imageUrl,
            [nameof(OpenGraphMeta.Url)] = url,
            [nameof(OpenGraphMeta.Author)] = author,
            [nameof(OpenGraphMeta.PubDate)] = pubDate,
            [nameof(OpenGraphMeta.SiteName)] = siteName,
        };
        await ComponentRenderer.RenderComponentAsync<OpenGraphMeta>(dest, props);
        return dest.GetOutput();
    }

    [Fact]
    public async Task ShouldRenderOgDescription()
    {
        var html = await RenderAsync(description: "Article description");

        html.ShouldContain("og:description");
        html.ShouldContain("Article description");
    }

    [Fact]
    public async Task ShouldNotRenderOgDescriptionWhenNull()
    {
        var html = await RenderAsync(description: null);

        html.ShouldNotContain("og:description");
    }

    [Fact]
    public async Task ShouldRenderOgImage()
    {
        var html = await RenderAsync(imageUrl: "https://example.com/image.jpg");

        html.ShouldContain("og:image");
        html.ShouldContain("https://example.com/image.jpg");
    }

    [Fact]
    public async Task ShouldRenderOgUrl()
    {
        var html = await RenderAsync(url: "https://example.com/post");

        html.ShouldContain("og:url");
        html.ShouldContain("https://example.com/post");
    }

    [Fact]
    public async Task ShouldRenderOgSiteName()
    {
        var html = await RenderAsync(siteName: "My Blog");

        html.ShouldContain("og:site_name");
        html.ShouldContain("My Blog");
    }

    [Fact]
    public async Task ShouldRenderArticleAuthor()
    {
        var html = await RenderAsync(author: "Alice");

        html.ShouldContain("article:author");
        html.ShouldContain("Alice");
    }

    [Fact]
    public async Task ShouldRenderArticlePublishedTime()
    {
        var date = new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        var html = await RenderAsync(pubDate: date);

        html.ShouldContain("article:published_time");
        html.ShouldContain("2025-01-15");
    }

    [Fact]
    public async Task ShouldRenderTwitterCardWithNameAttribute()
    {
        var html = await RenderAsync();

        html.ShouldContain("<meta name=\"twitter:card\"");
        html.ShouldContain("summary_large_image");
    }

    [Fact]
    public async Task ShouldRenderTwitterTitleWithNameAttribute()
    {
        var html = await RenderAsync(title: "My Post");

        html.ShouldContain("<meta name=\"twitter:title\"");
        html.ShouldContain("My Post");
    }

    [Fact]
    public async Task ShouldRenderTwitterImageWithNameAttribute()
    {
        var html = await RenderAsync(imageUrl: "https://example.com/img.jpg");

        html.ShouldContain("<meta name=\"twitter:image\"");
        html.ShouldContain("https://example.com/img.jpg");
    }

    [Fact]
    public async Task ShouldRenderTwitterDescriptionWithNameAttribute()
    {
        var html = await RenderAsync(description: "A description");

        html.ShouldContain("<meta name=\"twitter:description\"");
        html.ShouldContain("A description");
    }

}
