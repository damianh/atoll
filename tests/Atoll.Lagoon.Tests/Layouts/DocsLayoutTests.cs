using Atoll.Build.Content.Markdown;
using Atoll.Components;
using Atoll.Lagoon.Configuration;
using Atoll.Lagoon.Layouts;
using Atoll.Lagoon.Navigation;
using Atoll.Rendering;
using Atoll.Slots;
using Shouldly;
using Xunit;

namespace Atoll.Lagoon.Tests.Layouts;

public sealed class DocsLayoutTests
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
        IReadOnlyList<MarkdownHeading>? headings = null,
        IReadOnlyList<ResolvedSidebarItem>? sidebarItems = null,
        PaginationLink? previous = null,
        PaginationLink? next = null,
        IReadOnlyList<BreadcrumbItem>? breadcrumbItems = null,
        ComponentDelegate? slotContent = null)
    {
        var destination = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            ["Config"] = config,
            ["PageTitle"] = pageTitle,
            ["PageDescription"] = pageDescription,
            ["Headings"] = headings ?? [],
            ["SidebarItems"] = sidebarItems ?? [],
            ["Previous"] = previous,
            ["Next"] = next,
            ["BreadcrumbItems"] = breadcrumbItems ?? [],
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

        await ComponentRenderer.RenderComponentAsync<DocsLayout>(destination, props, slots);
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
        var html = await RenderLayoutAsync(MakeConfig("Site Title"), pageTitle: "Getting Started");

        html.ShouldContain("Getting Started");
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
    public async Task ShouldRenderSidebarAsideWithAriaLabel()
    {
        var html = await RenderLayoutAsync(MakeConfig());

        html.ShouldContain("<aside");
        html.ShouldContain("id=\"mobile-nav-menu\"");
        html.ShouldContain("aria-label=\"Site navigation\"");
    }

    [Fact]
    public async Task ShouldRenderTocAsideWithAriaLabel()
    {
        var html = await RenderLayoutAsync(MakeConfig());

        html.ShouldContain("aria-label=\"On this page\"");
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
    public async Task ShouldRenderMobileNavToggle()
    {
        var html = await RenderLayoutAsync(MakeConfig());

        html.ShouldContain("id=\"mobile-nav-toggle\"");
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

    // --- Sidebar ---

    [Fact]
    public async Task ShouldRenderSidebarNavigation()
    {
        var items = new[]
        {
            new ResolvedSidebarItem("Introduction", "/docs/intro/", false, null),
        };
        var html = await RenderLayoutAsync(MakeConfig(), sidebarItems: items);

        html.ShouldContain("Introduction");
        html.ShouldContain("/docs/intro/");
    }

    // --- Breadcrumbs ---

    [Fact]
    public async Task ShouldRenderBreadcrumbsWhenProvided()
    {
        var crumbs = new[]
        {
            new BreadcrumbItem("Home", "/", false),
            new BreadcrumbItem("Guides", "/docs/guides/", false),
            new BreadcrumbItem("Getting Started", null, true),
        };
        var html = await RenderLayoutAsync(MakeConfig(), breadcrumbItems: crumbs);

        html.ShouldContain("aria-label=\"Breadcrumbs\"");
        html.ShouldContain("Home");
        html.ShouldContain("Getting Started");
    }

    [Fact]
    public async Task ShouldNotRenderBreadcrumbsWhenEmpty()
    {
        var html = await RenderLayoutAsync(MakeConfig(), breadcrumbItems: []);

        html.ShouldNotContain("aria-label=\"Breadcrumbs\"");
    }

    // --- Pagination ---

    [Fact]
    public async Task ShouldRenderPaginationWhenBothLinksProvided()
    {
        var html = await RenderLayoutAsync(
            MakeConfig(),
            previous: new PaginationLink("Overview", "/docs/overview/"),
            next: new PaginationLink("Configuration", "/docs/config/"));

        html.ShouldContain("aria-label=\"Pagination\"");
        html.ShouldContain("Overview");
        html.ShouldContain("Configuration");
    }

    [Fact]
    public async Task ShouldNotRenderPaginationWhenNeitherLinkProvided()
    {
        var html = await RenderLayoutAsync(MakeConfig());

        html.ShouldNotContain("aria-label=\"Pagination\"");
    }

    // --- Table of contents ---

    [Fact]
    public async Task ShouldRenderTocWhenHeadingsProvided()
    {
        var headings = new[]
        {
            new MarkdownHeading(2, "Installation", "installation"),
            new MarkdownHeading(2, "Configuration", "configuration"),
        };
        var html = await RenderLayoutAsync(MakeConfig(), headings: headings);

        html.ShouldContain("aria-label=\"On this page\"");
        html.ShouldContain("Installation");
        html.ShouldContain("Configuration");
    }

    // --- Slot content ---

    [Fact]
    public async Task ShouldRenderDefaultSlotInsideArticle()
    {
        ComponentDelegate slotContent = ctx =>
        {
            ctx.WriteHtml("<p>My page content</p>");
            return Task.CompletedTask;
        };
        var html = await RenderLayoutAsync(MakeConfig(), slotContent: slotContent);

        html.ShouldContain("<article class=\"docs-article prose\">");
        html.ShouldContain("<p>My page content</p>");
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
