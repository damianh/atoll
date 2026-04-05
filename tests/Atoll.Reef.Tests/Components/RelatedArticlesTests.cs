using Atoll.Components;
using Atoll.Reef.Components;
using Atoll.Rendering;
using Shouldly;
using Xunit;

namespace Atoll.Reef.Tests.Components;

public sealed class RelatedArticlesTests
{
    private static ArticleNavLink Link(string title, string href) => new(title, href);

    private static async Task<string> RenderAsync(
        IReadOnlyList<ArticleNavLink>? articles = null,
        string heading = "Related Articles",
        int maxItems = 3)
    {
        var dest = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            [nameof(RelatedArticles.Articles)] = articles ?? [],
            [nameof(RelatedArticles.Heading)] = heading,
            [nameof(RelatedArticles.MaxItems)] = maxItems,
        };
        await ComponentRenderer.RenderComponentAsync<RelatedArticles>(dest, props);
        return dest.GetOutput();
    }

    [Fact]
    public async Task ShouldRenderNothingWhenNoArticles()
    {
        var html = await RenderAsync([]);

        html.ShouldBeEmpty();
    }

    [Fact]
    public async Task ShouldRenderAsideWhenArticlesPresent()
    {
        var html = await RenderAsync([Link("Article One", "/blog/one")]);

        html.ShouldContain("<aside");
        html.ShouldContain("class=\"related-articles\"");
    }

    [Fact]
    public async Task ShouldRenderHeading()
    {
        var html = await RenderAsync(
            [Link("Article One", "/blog/one")],
            heading: "You Might Also Like");

        html.ShouldContain("You Might Also Like");
    }

    [Fact]
    public async Task ShouldRenderDefaultHeading()
    {
        var html = await RenderAsync([Link("Article", "/blog/a")]);

        html.ShouldContain("Related Articles");
    }

    [Fact]
    public async Task ShouldRenderArticleLinks()
    {
        var articles = new[]
        {
            Link("First Article", "/blog/first"),
            Link("Second Article", "/blog/second"),
        };

        var html = await RenderAsync(articles);

        html.ShouldContain("href=\"/blog/first\"");
        html.ShouldContain("First Article");
        html.ShouldContain("href=\"/blog/second\"");
        html.ShouldContain("Second Article");
    }

    [Fact]
    public async Task ShouldRespectMaxItems()
    {
        var articles = new[]
        {
            Link("Article One", "/blog/one"),
            Link("Article Two", "/blog/two"),
            Link("Article Three", "/blog/three"),
            Link("Article Four", "/blog/four"),
        };

        var html = await RenderAsync(articles, maxItems: 2);

        html.ShouldContain("Article One");
        html.ShouldContain("Article Two");
        html.ShouldNotContain("Article Three");
        html.ShouldNotContain("Article Four");
    }

    [Fact]
    public async Task ShouldHtmlEncodeTitles()
    {
        var html = await RenderAsync([Link("<script>xss</script>", "/blog/post")]);

        html.ShouldContain("&lt;script&gt;xss&lt;/script&gt;");
        html.ShouldNotContain("<script>xss</script>");
    }

    [Fact]
    public async Task ShouldHtmlEncodeHrefs()
    {
        var html = await RenderAsync([Link("Safe Title", "/blog/post?q=a&b=c")]);

        html.ShouldContain("&amp;");
        html.ShouldNotContain("href=\"/blog/post?q=a&b=c\"");
    }

    [Fact]
    public async Task ShouldRenderUnorderedList()
    {
        var html = await RenderAsync([Link("Article", "/blog/a")]);

        html.ShouldContain("<ul");
        html.ShouldContain("<li");
    }
}
