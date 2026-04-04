using Atoll.Components;
using Atoll.Lagoon.Configuration;
using Atoll.Lagoon.Layouts;
using Atoll.Rendering;
using Atoll.Slots;
using Shouldly;
using Xunit;

namespace Atoll.Lagoon.Tests.Layouts;

public sealed class SplashLayoutTests
{
    private static DocsConfig MakeConfig(
        string title = "My Docs",
        string description = "",
        bool enableMermaid = false)
    {
        return new DocsConfig
        {
            Title = title,
            Description = description,
            EnableMermaid = enableMermaid,
        };
    }

    private static async Task<string> RenderLayoutAsync(
        DocsConfig config,
        string pageTitle = "",
        string? pageDescription = null,
        ComponentDelegate? slotContent = null)
    {
        var destination = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            ["Config"] = config,
            ["PageTitle"] = pageTitle,
            ["PageDescription"] = pageDescription,
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

        await ComponentRenderer.RenderComponentAsync<SplashLayout>(destination, props, slots);
        return destination.GetOutput();
    }

    // --- Document structure ---

    [Fact]
    public async Task ShouldRenderHtml5Doctype()
    {
        var html = await RenderLayoutAsync(MakeConfig());

        html.ShouldStartWith("<!DOCTYPE html>");
    }

    [Fact]
    public async Task ShouldRenderHtmlElement()
    {
        var html = await RenderLayoutAsync(MakeConfig());

        html.ShouldContain("<html lang=\"en\">");
    }

    [Fact]
    public async Task ShouldRenderHeadSection()
    {
        var html = await RenderLayoutAsync(MakeConfig("Site Title"));

        html.ShouldContain("<head>");
        html.ShouldContain("</head>");
        html.ShouldContain("<title>");
        html.ShouldContain("Site Title");
    }

    [Fact]
    public async Task ShouldRenderPageTitleWithSeparator()
    {
        var html = await RenderLayoutAsync(MakeConfig("Site Title"), pageTitle: "Welcome");

        html.ShouldContain("Welcome");
        html.ShouldContain("Site Title");
        html.ShouldContain(" | ");
    }

    [Fact]
    public async Task ShouldRenderBodyElement()
    {
        var html = await RenderLayoutAsync(MakeConfig());

        html.ShouldContain("<body>");
        html.ShouldContain("</body>");
    }

    // --- ARIA landmarks ---

    [Fact]
    public async Task ShouldRenderHeaderLandmark()
    {
        var html = await RenderLayoutAsync(MakeConfig());

        html.ShouldContain("<header");
    }

    [Fact]
    public async Task ShouldRenderMainLandmark()
    {
        var html = await RenderLayoutAsync(MakeConfig());

        html.ShouldContain("<main");
        html.ShouldContain("id=\"main-content\"");
    }

    [Fact]
    public async Task ShouldRenderFooterElement()
    {
        var html = await RenderLayoutAsync(MakeConfig());

        html.ShouldContain("<footer");
        html.ShouldContain("</footer>");
    }

    // --- Header content ---

    [Fact]
    public async Task ShouldRenderBrandLinkWithSiteTitle()
    {
        var html = await RenderLayoutAsync(MakeConfig("Atoll Framework"));

        html.ShouldContain("Atoll Framework");
        html.ShouldContain("docs-brand");
    }

    [Fact]
    public async Task ShouldRenderLogoImageWhenLogoSrcSet()
    {
        var config = new DocsConfig { Title = "Docs", LogoSrc = "/logo.svg", LogoAlt = "Logo" };
        var html = await RenderLayoutAsync(config);

        html.ShouldContain("<img src=\"/logo.svg\"");
        html.ShouldContain("alt=\"Logo\"");
    }

    [Fact]
    public async Task ShouldNotRenderLogoImageWhenLogoSrcNull()
    {
        var html = await RenderLayoutAsync(MakeConfig());

        html.ShouldNotContain("<img src=");
    }

    [Fact]
    public async Task ShouldRenderThemeToggleIsland()
    {
        var html = await RenderLayoutAsync(MakeConfig());

        html.ShouldContain("id=\"theme-toggle\"");
    }

    [Fact]
    public async Task ShouldRenderSearchDialogTrigger()
    {
        var html = await RenderLayoutAsync(MakeConfig());

        html.ShouldContain("id=\"search-trigger\"");
    }

    [Fact]
    public async Task ShouldRenderSocialLinks()
    {
        var config = new DocsConfig
        {
            Title = "Docs",
            Social = [new SocialLink("GitHub", "https://github.com/example/repo", SocialIcon.GitHub)],
        };

        var html = await RenderLayoutAsync(config);

        html.ShouldContain("https://github.com/example/repo");
        html.ShouldContain("GitHub");
    }

    // --- Splash-specific — elements that must NOT be present ---

    [Fact]
    public async Task ShouldNotRenderMobileNavToggle()
    {
        var html = await RenderLayoutAsync(MakeConfig());

        html.ShouldNotContain("id=\"mobile-nav-toggle\"");
    }

    [Fact]
    public async Task ShouldNotRenderSidebar()
    {
        var html = await RenderLayoutAsync(MakeConfig());

        html.ShouldNotContain("docs-sidebar");
        html.ShouldNotContain("id=\"mobile-nav-menu\"");
        html.ShouldNotContain("aria-label=\"Site navigation\"");
    }

    [Fact]
    public async Task ShouldNotRenderTableOfContents()
    {
        var html = await RenderLayoutAsync(MakeConfig());

        html.ShouldNotContain("docs-toc");
        html.ShouldNotContain("aria-label=\"On this page\"");
    }

    [Fact]
    public async Task ShouldNotRenderBreadcrumbs()
    {
        var html = await RenderLayoutAsync(MakeConfig());

        html.ShouldNotContain("aria-label=\"Breadcrumbs\"");
    }

    [Fact]
    public async Task ShouldNotRenderPagination()
    {
        var html = await RenderLayoutAsync(MakeConfig());

        html.ShouldNotContain("aria-label=\"Pagination\"");
    }

    // --- Splash-specific — elements that MUST be present ---

    [Fact]
    public async Task ShouldRenderSplashMainContainer()
    {
        var html = await RenderLayoutAsync(MakeConfig());

        html.ShouldContain("class=\"splash-main\"");
    }

    [Fact]
    public async Task ShouldRenderSplashContentArticle()
    {
        var html = await RenderLayoutAsync(MakeConfig());

        html.ShouldContain("class=\"splash-content\"");
    }

    [Fact]
    public async Task ShouldNotRenderDocsBodyGrid()
    {
        var html = await RenderLayoutAsync(MakeConfig());

        html.ShouldNotContain("class=\"docs-body\"");
    }

    [Fact]
    public async Task ShouldNotRenderDocsArticle()
    {
        var html = await RenderLayoutAsync(MakeConfig());

        html.ShouldNotContain("class=\"docs-article");
    }

    // --- Slot content ---

    [Fact]
    public async Task ShouldRenderDefaultSlotInsideSplashContent()
    {
        ComponentDelegate slotContent = ctx =>
        {
            ctx.WriteHtml("<p>Landing page content</p>");
            return Task.CompletedTask;
        };
        var html = await RenderLayoutAsync(MakeConfig(), slotContent: slotContent);

        html.ShouldContain("<article class=\"splash-content\">");
        html.ShouldContain("<p>Landing page content</p>");
    }

    // --- Mermaid ---

    [Fact]
    public async Task ShouldInjectMermaidScriptWhenEnabled()
    {
        var html = await RenderLayoutAsync(MakeConfig(enableMermaid: true));

        html.ShouldContain("atoll-docs-mermaid-init.js");
    }

    [Fact]
    public async Task ShouldNotInjectMermaidScriptWhenDisabled()
    {
        var html = await RenderLayoutAsync(MakeConfig(enableMermaid: false));

        html.ShouldNotContain("atoll-docs-mermaid-init.js");
    }

    // --- Theme FOUC prevention ---

    [Fact]
    public async Task ShouldIncludeFoucPreventionScript()
    {
        var html = await RenderLayoutAsync(MakeConfig());

        html.ShouldContain("atoll-theme");
    }

    // --- HTML encoding ---

    [Fact]
    public async Task ShouldHtmlEncodeLogoSrc()
    {
        var config = new DocsConfig { Title = "Docs", LogoSrc = "/img?a=1&b=2", LogoAlt = "" };
        var html = await RenderLayoutAsync(config);

        html.ShouldContain("src=\"/img?a=1&amp;b=2\"");
    }
}
