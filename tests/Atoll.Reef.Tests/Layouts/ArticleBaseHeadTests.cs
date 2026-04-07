using Atoll.Components;
using Atoll.Reef.Configuration;
using Atoll.Reef.Layouts;
using Atoll.Rendering;

namespace Atoll.Reef.Tests.Layouts;

public sealed class ArticleBaseHeadTests
{
    private static ReefConfig MakeConfig(
        string title = "My Blog",
        string? description = null,
        string? faviconHref = null,
        bool rssEnabled = false,
        string basePath = "") =>
        new()
        {
            Title = title,
            Description = description ?? "",
            FaviconHref = faviconHref ?? "",
            RssEnabled = rssEnabled,
            BasePath = basePath,
        };

    private static async Task<string> RenderAsync(
        ReefConfig config,
        string pageTitle = "",
        string? pageDescription = null,
        string? pageHeadContent = null)
    {
        var destination = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            [nameof(ArticleBaseHead.Config)] = config,
            [nameof(ArticleBaseHead.PageTitle)] = pageTitle,
            [nameof(ArticleBaseHead.PageDescription)] = pageDescription,
            [nameof(ArticleBaseHead.PageHeadContent)] = pageHeadContent,
        };
        await ComponentRenderer.RenderComponentAsync<ArticleBaseHead>(destination, props);
        return destination.GetOutput();
    }

    [Fact]
    public async Task ShouldRenderHeadElement()
    {
        var html = await RenderAsync(MakeConfig());

        html.ShouldStartWith("<head>");
        html.ShouldContain("</head>");
    }

    [Fact]
    public async Task ShouldRenderCharsetMeta()
    {
        var html = await RenderAsync(MakeConfig());

        html.ShouldContain("charset=\"utf-8\"");
    }

    [Fact]
    public async Task ShouldRenderViewportMeta()
    {
        var html = await RenderAsync(MakeConfig());

        html.ShouldContain("name=\"viewport\"");
        html.ShouldContain("width=device-width");
    }

    [Fact]
    public async Task ShouldRenderSiteTitle()
    {
        var html = await RenderAsync(MakeConfig(title: "Acme Blog"));

        html.ShouldContain("<title>");
        html.ShouldContain("Acme Blog");
    }

    [Fact]
    public async Task ShouldPrefixPageTitleWithSeparator()
    {
        var html = await RenderAsync(MakeConfig(title: "My Blog"), pageTitle: "First Post");

        html.ShouldContain("First Post");
        html.ShouldContain(" | ");
        html.ShouldContain("My Blog");
    }

    [Fact]
    public async Task ShouldOmitSeparatorWhenPageTitleEmpty()
    {
        var html = await RenderAsync(MakeConfig(title: "My Blog"), pageTitle: "");

        html.ShouldNotContain(" | ");
        html.ShouldContain("My Blog");
    }

    [Fact]
    public async Task ShouldRenderPageDescriptionWhenProvided()
    {
        var html = await RenderAsync(MakeConfig(), pageDescription: "A specific page description");

        html.ShouldContain("name=\"description\"");
        html.ShouldContain("A specific page description");
    }

    [Fact]
    public async Task ShouldFallbackToConfigDescriptionWhenPageDescriptionNull()
    {
        var html = await RenderAsync(MakeConfig(description: "Site description"), pageDescription: null);

        html.ShouldContain("Site description");
    }

    [Fact]
    public async Task ShouldRenderFaviconLinkWhenSet()
    {
        var html = await RenderAsync(MakeConfig(faviconHref: "/favicon.svg"));

        html.ShouldContain("rel=\"icon\"");
        html.ShouldContain("/favicon.svg");
    }

    [Fact]
    public async Task ShouldOmitFaviconLinkWhenNotSet()
    {
        var html = await RenderAsync(MakeConfig(faviconHref: ""));

        html.ShouldNotContain("rel=\"icon\"");
    }

    [Fact]
    public async Task ShouldRenderRssLinkWhenEnabled()
    {
        var html = await RenderAsync(MakeConfig(rssEnabled: true, basePath: "/blog"));

        html.ShouldContain("rel=\"alternate\"");
        html.ShouldContain("application/rss+xml");
        html.ShouldContain("/blog/feed.xml");
    }

    [Fact]
    public async Task ShouldOmitRssLinkWhenDisabled()
    {
        var html = await RenderAsync(MakeConfig(rssEnabled: false));

        html.ShouldNotContain("application/rss+xml");
    }

    [Fact]
    public async Task ShouldTrimTrailingSlashFromBasePathInRssLink()
    {
        var html = await RenderAsync(MakeConfig(rssEnabled: true, basePath: "/blog/"));

        html.ShouldContain("/blog/feed.xml");
        html.ShouldNotContain("/blog//feed.xml");
    }

    [Fact]
    public async Task ShouldRenderThemeToggleScript()
    {
        var html = await RenderAsync(MakeConfig());

        html.ShouldContain("<script>");
        html.ShouldContain("atoll-theme");
    }

    [Fact]
    public async Task ShouldRenderPageHeadContentWhenProvided()
    {
        var html = await RenderAsync(MakeConfig(), pageHeadContent: "<meta property=\"og:title\" content=\"Test\" />");

        html.ShouldContain("og:title");
        html.ShouldContain("Test");
    }

    [Fact]
    public async Task ShouldOmitPageHeadContentWhenNull()
    {
        var html = await RenderAsync(MakeConfig(), pageHeadContent: null);

        html.ShouldNotContain("og:title");
    }

    [Fact]
    public async Task ShouldHtmlEncodeSpecialCharsInTitle()
    {
        var html = await RenderAsync(MakeConfig(title: "A & B"));

        html.ShouldContain("A &amp; B");
    }

    [Fact]
    public async Task ShouldRenderCustomCssLinks()
    {
        var config = MakeConfig();
        config.CustomCss = ["/styles/custom.css"];
        var html = await RenderAsync(config);

        html.ShouldContain("rel=\"stylesheet\"");
        html.ShouldContain("/styles/custom.css");
    }
}
