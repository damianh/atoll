using Atoll.Components;
using Atoll.Reef.Configuration;
using Atoll.Reef.Layouts;
using Atoll.Rendering;
using Atoll.Slots;
using Shouldly;
using Xunit;

namespace Atoll.Reef.Tests.Layouts;

public sealed class ArticleListLayoutTests
{
    private static ReefConfig MakeConfig(string title = "My Articles", string basePath = "") =>
        new() { Title = title, BasePath = basePath };

    private static async Task<string> RenderAsync(
        ReefConfig config,
        string pageTitle = "",
        string? pageDescription = null,
        ComponentDelegate? slotContent = null)
    {
        var destination = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            [nameof(ArticleListLayout.Config)] = config,
            [nameof(ArticleListLayout.PageTitle)] = pageTitle,
            [nameof(ArticleListLayout.PageDescription)] = pageDescription,
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

        await ComponentRenderer.RenderComponentAsync<ArticleListLayout>(destination, props, slots);
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
    public async Task ShouldRenderHeaderWithBannerRole()
    {
        var html = await RenderAsync(MakeConfig());
        html.ShouldContain("role=\"banner\"");
        html.ShouldContain("<header");
    }

    [Fact]
    public async Task ShouldRenderMainWithListingClass()
    {
        var html = await RenderAsync(MakeConfig());
        html.ShouldContain("article-listing");
        html.ShouldContain("role=\"main\"");
    }

    [Fact]
    public async Task ShouldRenderFooterWithContentInfo()
    {
        var html = await RenderAsync(MakeConfig());
        html.ShouldContain("role=\"contentinfo\"");
        html.ShouldContain("<footer");
    }

    [Fact]
    public async Task ShouldRenderPageTitleAsHeading()
    {
        var html = await RenderAsync(MakeConfig(), pageTitle: "All Articles");
        html.ShouldContain("<h1");
        html.ShouldContain("All Articles");
    }

    [Fact]
    public async Task ShouldOmitHeadingWhenPageTitleIsEmpty()
    {
        var html = await RenderAsync(MakeConfig(), pageTitle: "");
        html.ShouldNotContain("<h1");
    }

    [Fact]
    public async Task ShouldRenderSiteTitleInHeader()
    {
        var html = await RenderAsync(MakeConfig(title: "My Blog"));
        html.ShouldContain("My Blog");
    }

    [Fact]
    public async Task ShouldRenderSlotContent()
    {
        var html = await RenderAsync(MakeConfig(), slotContent: ctx =>
        {
            ctx.WriteHtml("<div class=\"custom-slot-content\">articles here</div>");
            return Task.CompletedTask;
        });
        html.ShouldContain("custom-slot-content");
        html.ShouldContain("articles here");
    }

    [Fact]
    public async Task ShouldContainMainContentId()
    {
        var html = await RenderAsync(MakeConfig());
        html.ShouldContain("id=\"main-content\"");
    }
}
