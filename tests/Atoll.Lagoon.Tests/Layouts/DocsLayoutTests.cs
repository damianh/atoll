using Atoll.Build.Content.Markdown;
using Atoll.Components;
using Atoll.Lagoon.Configuration;
using Atoll.Lagoon.I18n;
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
        ComponentDelegate? slotContent = null,
        string currentPath = "/")
    {
        return await RenderLayoutCoreAsync(
            config, pageTitle, pageDescription, headings, sidebarItems,
            previous, next, breadcrumbItems, slotContent, currentPath,
            isUntranslatedContent: false, pageHeadContent: null);
    }

    private static async Task<string> RenderLayoutWithHeadContentAsync(
        DocsConfig config,
        string pageTitle = "",
        string? pageDescription = null,
        IReadOnlyList<MarkdownHeading>? headings = null,
        IReadOnlyList<ResolvedSidebarItem>? sidebarItems = null,
        PaginationLink? previous = null,
        PaginationLink? next = null,
        IReadOnlyList<BreadcrumbItem>? breadcrumbItems = null,
        ComponentDelegate? slotContent = null,
        string? pageHeadContent = null,
        string currentPath = "/")
    {
        return await RenderLayoutCoreAsync(
            config, pageTitle, pageDescription, headings, sidebarItems,
            previous, next, breadcrumbItems, slotContent, currentPath,
            isUntranslatedContent: false, pageHeadContent: pageHeadContent);
    }

    private static async Task<string> RenderLayoutCoreAsync(
        DocsConfig config,
        string pageTitle,
        string? pageDescription,
        IReadOnlyList<MarkdownHeading>? headings,
        IReadOnlyList<ResolvedSidebarItem>? sidebarItems,
        PaginationLink? previous,
        PaginationLink? next,
        IReadOnlyList<BreadcrumbItem>? breadcrumbItems,
        ComponentDelegate? slotContent,
        string currentPath,
        bool isUntranslatedContent,
        string? pageHeadContent = null)
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
            ["PageHeadContent"] = pageHeadContent,
            ["CurrentPath"] = currentPath,
            ["IsUntranslatedContent"] = isUntranslatedContent,
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

        html.ShouldContain("<html lang=\"en\" dir=\"ltr\">");
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

        // When LogoSrc is null, the default Atoll logo is rendered instead.
        html.ShouldContain("<img src=\"/_atoll/logo.png\"");
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

    // --- Per-page head content ---

    [Fact]
    public async Task ShouldPassPageHeadContentThroughToHead()
    {
        var html = await RenderLayoutWithHeadContentAsync(
            MakeConfig(),
            pageHeadContent: "<script src=\"/analytics.js\"></script>");

        var headStart = html.IndexOf("<head>", StringComparison.Ordinal);
        var headEnd = html.IndexOf("</head>", StringComparison.Ordinal);
        var headSection = html.Substring(headStart, headEnd - headStart + "</head>".Length);
        headSection.ShouldContain("<script src=\"/analytics.js\"></script>");
    }

    // --- Locale resolution ---

    [Fact]
    public async Task ShouldRenderDefaultLangAndDirWithoutLocales()
    {
        var html = await RenderLayoutAsync(MakeConfig());

        html.ShouldContain("<html lang=\"en\" dir=\"ltr\">");
    }

    [Fact]
    public async Task ShouldRenderResolvedLangForFrenchLocale()
    {
        var config = new DocsConfig
        {
            Title = "Docs",
            Locales = new Dictionary<string, LocaleConfig>
            {
                ["root"] = new() { Label = "English", Lang = "en" },
                ["fr"] = new() { Label = "French", Lang = "fr" },
            },
        };

        var html = await RenderLayoutAsync(config, currentPath: "/fr/intro");

        html.ShouldContain("<html lang=\"fr\" dir=\"ltr\">");
    }

    [Fact]
    public async Task ShouldRenderRtlDirectionForArabicLocale()
    {
        var config = new DocsConfig
        {
            Title = "Docs",
            Locales = new Dictionary<string, LocaleConfig>
            {
                ["root"] = new() { Label = "English", Lang = "en" },
                ["ar"] = new() { Label = "Arabic", Lang = "ar", Dir = "rtl" },
            },
        };

        var html = await RenderLayoutAsync(config, currentPath: "/ar/intro");

        html.ShouldContain("<html lang=\"ar\" dir=\"rtl\">");
    }

    [Fact]
    public async Task ShouldRenderRootLocaleForUnprefixedPath()
    {
        var config = new DocsConfig
        {
            Title = "Docs",
            Locales = new Dictionary<string, LocaleConfig>
            {
                ["root"] = new() { Label = "English", Lang = "en" },
                ["fr"] = new() { Label = "French", Lang = "fr" },
            },
        };

        var html = await RenderLayoutAsync(config, currentPath: "/intro");

        html.ShouldContain("<html lang=\"en\" dir=\"ltr\">");
    }

    [Fact]
    public async Task ShouldUsePerLocaleTranslations()
    {
        var frenchTranslations = UiTranslations.Default with
        {
            SiteNavigationLabel = "Navigation du site",
        };
        var config = new DocsConfig
        {
            Title = "Docs",
            Locales = new Dictionary<string, LocaleConfig>
            {
                ["root"] = new() { Label = "English", Lang = "en" },
                ["fr"] = new() { Label = "French", Lang = "fr", Translations = frenchTranslations },
            },
        };

        var html = await RenderLayoutAsync(config, currentPath: "/fr/intro");

        html.ShouldContain("aria-label=\"Navigation du site\"");
        html.ShouldNotContain("aria-label=\"Site navigation\"");
    }

    [Fact]
    public async Task ShouldResolveLocaleWithBasePath()
    {
        var config = new DocsConfig
        {
            Title = "Docs",
            BasePath = "/docs",
            Locales = new Dictionary<string, LocaleConfig>
            {
                ["root"] = new() { Label = "English", Lang = "en" },
                ["fr"] = new() { Label = "French", Lang = "fr" },
            },
        };

        var html = await RenderLayoutAsync(config, currentPath: "/docs/fr/intro");

        html.ShouldContain("<html lang=\"fr\" dir=\"ltr\">");
    }

    // --- Search index URL ---

    [Fact]
    public async Task ShouldRenderDefaultSearchIndexUrlWithoutLocales()
    {
        var html = await RenderLayoutAsync(MakeConfig());

        html.ShouldContain("data-index-url=\"/search-index.json\"");
    }

    [Fact]
    public async Task ShouldRenderBasePathPrefixedSearchIndexUrl()
    {
        var config = new DocsConfig { Title = "Docs", BasePath = "/docs" };

        var html = await RenderLayoutAsync(config);

        html.ShouldContain("data-index-url=\"/docs/search-index.json\"");
    }

    [Fact]
    public async Task ShouldRenderLocalePrefixedSearchIndexUrl()
    {
        var config = new DocsConfig
        {
            Title = "Docs",
            Locales = new Dictionary<string, LocaleConfig>
            {
                ["root"] = new() { Label = "English", Lang = "en" },
                ["fr"] = new() { Label = "French", Lang = "fr" },
            },
        };

        var html = await RenderLayoutAsync(config, currentPath: "/fr/intro");

        html.ShouldContain("data-index-url=\"/fr/search-index.json\"");
    }

    [Fact]
    public async Task ShouldRenderLocalePrefixedSearchIndexUrlWithBasePath()
    {
        var config = new DocsConfig
        {
            Title = "Docs",
            BasePath = "/docs",
            Locales = new Dictionary<string, LocaleConfig>
            {
                ["root"] = new() { Label = "English", Lang = "en" },
                ["fr"] = new() { Label = "French", Lang = "fr" },
            },
        };

        var html = await RenderLayoutAsync(config, currentPath: "/docs/fr/intro");

        html.ShouldContain("data-index-url=\"/docs/fr/search-index.json\"");
    }

    [Fact]
    public async Task ShouldRenderRootLocaleSearchIndexUrlWithBasePath()
    {
        var config = new DocsConfig
        {
            Title = "Docs",
            BasePath = "/docs",
            Locales = new Dictionary<string, LocaleConfig>
            {
                ["root"] = new() { Label = "English", Lang = "en" },
                ["fr"] = new() { Label = "French", Lang = "fr" },
            },
        };

        var html = await RenderLayoutAsync(config, currentPath: "/docs/intro");

        html.ShouldContain("data-index-url=\"/docs/search-index.json\"");
    }

    // --- Untranslated content notice ---

    [Fact]
    public async Task ShouldRenderUntranslatedNoticeWhenFlagIsSetAndLocalesConfigured()
    {
        var config = new DocsConfig
        {
            Title = "Docs",
            Locales = new Dictionary<string, LocaleConfig>
            {
                ["root"] = new() { Label = "English", Lang = "en" },
                ["fr"] = new() { Label = "French", Lang = "fr" },
            },
        };

        var html = await RenderLayoutCoreAsync(
            config, "", null, null, null, null, null, null, null, "/fr/intro", true);

        html.ShouldContain("class=\"untranslated-notice\"");
        // French locale auto-resolves to French built-in translations
        html.ShouldContain("Cette page n&#39;a pas encore été traduite.");
    }

    [Fact]
    public async Task ShouldNotRenderUntranslatedNoticeWhenFlagIsFalse()
    {
        var config = new DocsConfig
        {
            Title = "Docs",
            Locales = new Dictionary<string, LocaleConfig>
            {
                ["root"] = new() { Label = "English", Lang = "en" },
                ["fr"] = new() { Label = "French", Lang = "fr" },
            },
        };

        var html = await RenderLayoutCoreAsync(
            config, "", null, null, null, null, null, null, null, "/fr/intro", false);

        html.ShouldNotContain("untranslated-notice");
    }

    [Fact]
    public async Task ShouldNotRenderUntranslatedNoticeWithoutLocales()
    {
        var config = MakeConfig();

        var html = await RenderLayoutCoreAsync(
            config, "", null, null, null, null, null, null, null, "/", true);

        html.ShouldNotContain("untranslated-notice");
    }

    [Fact]
    public async Task ShouldRenderCustomUntranslatedNoticeFromTranslations()
    {
        var frenchTranslations = UiTranslations.Default with
        {
            UntranslatedContentNotice = "Pas traduite",
        };
        var config = new DocsConfig
        {
            Title = "Docs",
            Locales = new Dictionary<string, LocaleConfig>
            {
                ["root"] = new() { Label = "English", Lang = "en" },
                ["fr"] = new() { Label = "French", Lang = "fr", Translations = frenchTranslations },
            },
        };

        var html = await RenderLayoutCoreAsync(
            config, "", null, null, null, null, null, null, null, "/fr/intro", true);

        html.ShouldContain("class=\"untranslated-notice\"");
        html.ShouldContain("Pas traduite");
        html.ShouldNotContain("This page has not been translated yet.");
    }

    [Fact]
    public async Task ShouldRenderSidebarStateScriptTag()
    {
        var config = MakeConfig();

        var html = await RenderLayoutAsync(config);

        html.ShouldContain("atoll-sidebar-state.js");
    }
}
