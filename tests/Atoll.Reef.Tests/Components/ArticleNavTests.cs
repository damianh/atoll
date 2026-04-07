using Atoll.Components;
using Atoll.Reef.Components;
using Atoll.Rendering;
using Atoll.Slots;

namespace Atoll.Reef.Tests.Components;

public sealed class ArticleNavTests
{
    private static async Task<string> RenderAsync(
        ArticleNavLink? previous = null,
        ArticleNavLink? next = null)
    {
        var destination = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            [nameof(ArticleNav.Previous)] = previous,
            [nameof(ArticleNav.Next)] = next,
        };
        await ComponentRenderer.RenderComponentAsync<ArticleNav>(destination, props, SlotCollection.Empty);
        return destination.GetOutput();
    }

    [Fact]
    public async Task ShouldRenderNothingWhenBothLinksAreNull()
    {
        var html = await RenderAsync();
        html.ShouldBeEmpty();
    }

    [Fact]
    public async Task ShouldRenderPreviousLink()
    {
        var html = await RenderAsync(previous: new ArticleNavLink("Prev Post", "/blog/prev"));
        html.ShouldContain("article-nav-prev");
        html.ShouldContain("href=\"/blog/prev\"");
        html.ShouldContain("Prev Post");
    }

    [Fact]
    public async Task ShouldRenderNextLink()
    {
        var html = await RenderAsync(next: new ArticleNavLink("Next Post", "/blog/next"));
        html.ShouldContain("article-nav-next");
        html.ShouldContain("href=\"/blog/next\"");
        html.ShouldContain("Next Post");
    }

    [Fact]
    public async Task ShouldOmitPreviousWhenNull()
    {
        var html = await RenderAsync(next: new ArticleNavLink("Next", "/next"));
        html.ShouldNotContain("article-nav-prev");
    }

    [Fact]
    public async Task ShouldOmitNextWhenNull()
    {
        var html = await RenderAsync(previous: new ArticleNavLink("Prev", "/prev"));
        html.ShouldNotContain("article-nav-next");
    }

    [Fact]
    public async Task ShouldHtmlEncodePreviousTitle()
    {
        var html = await RenderAsync(previous: new ArticleNavLink("<b>Prev</b>", "/prev"));
        html.ShouldContain("&lt;b&gt;Prev&lt;/b&gt;");
    }

    [Fact]
    public async Task ShouldHtmlEncodeNextTitle()
    {
        var html = await RenderAsync(next: new ArticleNavLink("<b>Next</b>", "/next"));
        html.ShouldContain("&lt;b&gt;Next&lt;/b&gt;");
    }

}
