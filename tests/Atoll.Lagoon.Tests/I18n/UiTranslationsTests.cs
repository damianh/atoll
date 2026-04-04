using Atoll.Components;
using Atoll.Lagoon.Components;
using Atoll.Lagoon.I18n;
using Atoll.Lagoon.Islands;
using Atoll.Lagoon.Navigation;
using Atoll.Rendering;
using Shouldly;
using Xunit;

namespace Atoll.Lagoon.Tests.I18n;

public sealed class UiTranslationsTests
{
    // --- Default values ---

    [Fact]
    public void DefaultShouldReturnEnglishDefaults()
    {
        var t = UiTranslations.Default;

        t.SkipLinkLabel.ShouldBe("Skip to content");
        t.SearchLabel.ShouldBe("Search");
        t.SearchPlaceholder.ShouldBe("Search docs...");
        t.SearchDialogLabel.ShouldBe("Search docs");
        t.SearchCloseLabel.ShouldBe("Close search");
        t.SearchResultsLabel.ShouldBe("Search results");
        t.SearchNoResults.ShouldBe("No results found.");
        t.ThemeToggleLabel.ShouldBe("Toggle theme");
        t.ThemeSwitchToLight.ShouldBe("Switch to light theme");
        t.ThemeSwitchToDark.ShouldBe("Switch to dark theme");
        t.SidebarNavLabel.ShouldBe("Main");
        t.SiteNavigationLabel.ShouldBe("Site navigation");
        t.MobileNavOpenLabel.ShouldBe("Open navigation");
        t.BreadcrumbsLabel.ShouldBe("Breadcrumbs");
        t.PaginationLabel.ShouldBe("Pagination");
        t.PaginationPrevious.ShouldBe("Previous");
        t.PaginationNext.ShouldBe("Next");
        t.TocLabel.ShouldBe("On this page");
        t.BuiltWithLabel.ShouldBe("Built with");
        t.LanguageSelectLabel.ShouldBe("Select language");
    }

    [Fact]
    public void DefaultShouldBeSingletonInstance()
    {
        UiTranslations.Default.ShouldBeSameAs(UiTranslations.Default);
    }

    // --- With overrides ---

    [Fact]
    public void WithExpressionShouldOverrideSingleProperty()
    {
        var french = UiTranslations.Default with { PaginationNext = "Suivant" };

        french.PaginationNext.ShouldBe("Suivant");
        french.PaginationPrevious.ShouldBe("Previous"); // unchanged
    }

    [Fact]
    public void WithExpressionShouldOverrideMultipleProperties()
    {
        var custom = UiTranslations.Default with
        {
            PaginationNext = "Suivant",
            PaginationPrevious = "Précédent",
            TocLabel = "Sur cette page",
        };

        custom.PaginationNext.ShouldBe("Suivant");
        custom.PaginationPrevious.ShouldBe("Précédent");
        custom.TocLabel.ShouldBe("Sur cette page");
        custom.SearchLabel.ShouldBe("Search"); // unchanged
    }

    // --- Custom translations flow through to rendered components ---

    [Fact]
    public async Task PaginationShouldRenderCustomTranslations()
    {
        var translations = UiTranslations.Default with
        {
            PaginationLabel = "Navigation",
            PaginationPrevious = "Précédent",
            PaginationNext = "Suivant",
        };

        var destination = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            ["Previous"] = new PaginationLink("Intro", "/intro/"),
            ["Next"] = new PaginationLink("Advanced", "/advanced/"),
            ["Translations"] = translations,
        };
        await ComponentRenderer.RenderComponentAsync<Pagination>(destination, props);
        var html = destination.GetOutput();

        html.ShouldContain("aria-label=\"Navigation\"");
        html.ShouldContain("Précédent");
        html.ShouldContain("Suivant");
        html.ShouldNotContain("Previous");
        html.ShouldNotContain(">Next<");
    }

    [Fact]
    public async Task BreadcrumbsShouldRenderCustomTranslations()
    {
        var translations = UiTranslations.Default with
        {
            BreadcrumbsLabel = "Fil d'Ariane",
        };

        var destination = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            ["Items"] = new[] { new BreadcrumbItem("Home", null, true) },
            ["Translations"] = translations,
        };
        await ComponentRenderer.RenderComponentAsync<Breadcrumbs>(destination, props);
        var html = destination.GetOutput();

        html.ShouldContain("aria-label=\"Fil d&#39;Ariane\"");
    }

    [Fact]
    public async Task SidebarShouldRenderCustomTranslations()
    {
        var translations = UiTranslations.Default with
        {
            SidebarNavLabel = "Principal",
        };

        var destination = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            ["Items"] = Array.Empty<ResolvedSidebarItem>(),
            ["Translations"] = translations,
        };
        await ComponentRenderer.RenderComponentAsync<Sidebar>(destination, props);
        var html = destination.GetOutput();

        html.ShouldContain("aria-label=\"Principal\"");
        html.ShouldNotContain("aria-label=\"Main\"");
    }

    [Fact]
    public async Task ThemeToggleShouldRenderCustomTranslations()
    {
        var translations = UiTranslations.Default with
        {
            ThemeToggleLabel = "Theme toggle",
            ThemeSwitchToLight = "Light mode",
            ThemeSwitchToDark = "Dark mode",
        };

        var destination = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            ["Translations"] = translations,
        };
        await ComponentRenderer.RenderComponentAsync<ThemeToggle>(destination, props);
        var html = destination.GetOutput();

        html.ShouldContain("aria-label=\"Theme toggle\"");
        html.ShouldContain("data-label-light=\"Light mode\"");
        html.ShouldContain("data-label-dark=\"Dark mode\"");
    }

    [Fact]
    public async Task MobileNavShouldRenderCustomTranslations()
    {
        var translations = UiTranslations.Default with
        {
            MobileNavOpenLabel = "Ouvrir le menu",
        };

        var destination = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            ["Translations"] = translations,
        };
        await ComponentRenderer.RenderComponentAsync<MobileNav>(destination, props);
        var html = destination.GetOutput();

        html.ShouldContain("aria-label=\"Ouvrir le menu\"");
        html.ShouldNotContain("aria-label=\"Open navigation\"");
    }

    [Fact]
    public async Task SearchDialogShouldRenderCustomTranslations()
    {
        var translations = UiTranslations.Default with
        {
            SearchLabel = "Find",
            SearchDialogLabel = "Find in docs",
            SearchCloseLabel = "Close",
            SearchResultsLabel = "Results",
            SearchNoResults = "Nothing found.",
            SearchPlaceholder = "Type to search...",
        };

        var destination = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            ["Translations"] = translations,
        };
        await ComponentRenderer.RenderComponentAsync<SearchDialog>(destination, props);
        var html = destination.GetOutput();

        html.ShouldContain("aria-label=\"Find\"");
        html.ShouldContain("aria-label=\"Find in docs\"");
        html.ShouldContain("aria-label=\"Close\"");
        html.ShouldContain("aria-label=\"Results\"");
        html.ShouldContain("data-no-results=\"Nothing found.\"");
        html.ShouldContain("Type to search...");
    }

    [Fact]
    public async Task SearchDialogShouldUseCustomPlaceholderOverTranslations()
    {
        var translations = UiTranslations.Default with
        {
            SearchPlaceholder = "Type here...",
        };

        var destination = new StringRenderDestination();
#pragma warning disable CS0618 // Obsolete member usage is intentional for testing backward compatibility
        var props = new Dictionary<string, object?>
        {
            ["Placeholder"] = "Find articles...",
            ["Translations"] = translations,
        };
#pragma warning restore CS0618
        await ComponentRenderer.RenderComponentAsync<SearchDialog>(destination, props);
        var html = destination.GetOutput();

        // Custom Placeholder should take precedence over translations
        html.ShouldContain("Find articles...");
        html.ShouldNotContain("Type here...");
    }

    [Fact]
    public async Task SearchDialogShouldRenderNoResultsDataAttribute()
    {
        var destination = new StringRenderDestination();
        await ComponentRenderer.RenderComponentAsync<SearchDialog>(destination, new Dictionary<string, object?>());
        var html = destination.GetOutput();

        html.ShouldContain("data-no-results=\"No results found.\"");
    }

    [Fact]
    public async Task ThemeToggleShouldRenderDataLabelAttributes()
    {
        var destination = new StringRenderDestination();
        await ComponentRenderer.RenderComponentAsync<ThemeToggle>(destination, new Dictionary<string, object?>());
        var html = destination.GetOutput();

        html.ShouldContain("data-label-light=\"Switch to light theme\"");
        html.ShouldContain("data-label-dark=\"Switch to dark theme\"");
    }
}
