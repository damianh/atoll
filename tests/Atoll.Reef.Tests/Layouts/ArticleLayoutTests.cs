using Atoll.Components;
using Atoll.Reef.Configuration;
using Atoll.Reef.Layouts;
using Atoll.Rendering;
using Atoll.Slots;
using Shouldly;
using Xunit;

namespace Atoll.Reef.Tests.Layouts;

public sealed class ArticleLayoutTests
{
    private static ReefConfig MakeConfig(
        string title = "My Blog",
        string basePath = "",
        bool rssEnabled = false) =>
        new() { Title = title, BasePath = basePath, RssEnabled = rssEnabled };

    private static async Task<string> RenderAsync(
        ReefConfig config,
        string pageTitle = "",
        string? pageDescription = null,
        string? pageHeadContent = null,
        ComponentDelegate? slotContent = null)
    {
        var destination = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            [nameof(ArticleLayout.Config)] = config,
            [nameof(ArticleLayout.PageTitle)] = pageTitle,
            [nameof(ArticleLayout.PageDescription)] = pageDescription,
            [nameof(ArticleLayout.PageHeadContent)] = pageHeadContent,
        };

        SlotCollection slots;
        if (slotContent is not null)
        {
            var slotFragment = RenderFragment.FromAsync(async dest =>
            {
                var ctx = new RenderContext(dest, new Dictionary<string, object?>(), SlotCollection.Empty);
                await slotContent(ctx);
            });
            slots = SlotCollection.FromDefault(slotFragment);
        }
        else
        {
            slots = SlotCollection.Empty;
        }

        await ComponentRenderer.RenderComponentAsync<ArticleLayout>(destination, props, slots);
        return destination.GetOutput();
    }

    [Fact]
    public async Task ShouldRenderHtml5Doctype()
    {
        var html = await RenderAsync(MakeConfig());

        html.ShouldStartWith("<!DOCTYPE html>");
    }

    [Fact]
    public async Task ShouldRenderHtmlElement()
    {
        var html = await RenderAsync(MakeConfig());

        html.ShouldContain("<html lang=\"en\">");
    }

    [Fact]
    public async Task ShouldRenderHeadSection()
    {
        var html = await RenderAsync(MakeConfig("My Blog"));

        html.ShouldContain("<head>");
        html.ShouldContain("</head>");
        html.ShouldContain("<title>");
        html.ShouldContain("My Blog");
    }

    [Fact]
    public async Task ShouldRenderPageTitleWithSeparator()
    {
        var html = await RenderAsync(MakeConfig("My Blog"), pageTitle: "Getting Started");

        html.ShouldContain("Getting Started");
        html.ShouldContain("My Blog");
        html.ShouldContain(" | ");
    }

    [Fact]
    public async Task ShouldRenderBodyElement()
    {
        var html = await RenderAsync(MakeConfig());

        html.ShouldContain("<body>");
        html.ShouldContain("</body>");
    }

    [Fact]
    public async Task ShouldRenderHeaderWithBannerRole()
    {
        var html = await RenderAsync(MakeConfig());

        html.ShouldContain("role=\"banner\"");
        html.ShouldContain("<header");
    }

    [Fact]
    public async Task ShouldRenderMainWithArticleRole()
    {
        var html = await RenderAsync(MakeConfig());

        html.ShouldContain("role=\"main\"");
        html.ShouldContain("<main");
    }

    [Fact]
    public async Task ShouldRenderArticleElementInsideMain()
    {
        var html = await RenderAsync(MakeConfig());

        html.ShouldContain("reef-article");
        html.ShouldContain("<article");
    }

    [Fact]
    public async Task ShouldRenderFooterWithContentInfo()
    {
        var html = await RenderAsync(MakeConfig());

        html.ShouldContain("role=\"contentinfo\"");
        html.ShouldContain("<footer");
    }

    [Fact]
    public async Task ShouldRenderSiteTitleInHeader()
    {
        var html = await RenderAsync(MakeConfig(title: "Acme Blog"));

        html.ShouldContain("Acme Blog");
    }

    [Fact]
    public async Task ShouldRenderMainContentId()
    {
        var html = await RenderAsync(MakeConfig());

        html.ShouldContain("id=\"main-content\"");
    }

    [Fact]
    public async Task ShouldRenderSlotContent()
    {
        var html = await RenderAsync(MakeConfig(), slotContent: ctx =>
        {
            ctx.WriteHtml("<p class=\"article-body\">Hello world</p>");
            return Task.CompletedTask;
        });

        html.ShouldContain("article-body");
        html.ShouldContain("Hello world");
    }

    [Fact]
    public async Task ShouldRenderPageHeadContent()
    {
        var html = await RenderAsync(MakeConfig(), pageHeadContent: "<meta name=\"custom\" content=\"value\" />");

        html.ShouldContain("<meta name=\"custom\" content=\"value\" />");
    }

    [Fact]
    public async Task ShouldRenderDescriptionMetaWhenProvided()
    {
        var html = await RenderAsync(MakeConfig(), pageDescription: "An interesting article");

        html.ShouldContain("name=\"description\"");
        html.ShouldContain("An interesting article");
    }

    [Fact]
    public async Task ShouldRenderRssLinkWhenEnabled()
    {
        var html = await RenderAsync(MakeConfig(rssEnabled: true));

        html.ShouldContain("application/rss+xml");
        html.ShouldContain("feed.xml");
    }

    [Fact]
    public async Task ShouldNotRenderRssLinkWhenDisabled()
    {
        var html = await RenderAsync(MakeConfig(rssEnabled: false));

        html.ShouldNotContain("application/rss+xml");
    }

    [Fact]
    public async Task ShouldRenderSocialLinksWhenPresent()
    {
        var config = MakeConfig();
        config.Social = [new SocialLink("GitHub", "https://github.com/example", SocialIcon.GitHub)];
        var html = await RenderAsync(config);

        html.ShouldContain("GitHub");
        html.ShouldContain("https://github.com/example");
    }
}
