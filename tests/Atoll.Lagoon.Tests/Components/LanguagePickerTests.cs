using Atoll.Components;
using Atoll.Lagoon.Components;
using Atoll.Lagoon.I18n;
using Atoll.Rendering;
using Shouldly;
using Xunit;

namespace Atoll.Lagoon.Tests.Components;

public sealed class LanguagePickerTests
{
    private static async Task<string> RenderPickerAsync(
        IReadOnlyDictionary<string, LocaleConfig>? locales,
        string currentLocaleKey = "root",
        string currentContentPath = "/",
        string basePath = "",
        UiTranslations? translations = null)
    {
        var destination = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            ["Locales"] = locales,
            ["CurrentLocaleKey"] = currentLocaleKey,
            ["CurrentContentPath"] = currentContentPath,
            ["BasePath"] = basePath,
            ["Translations"] = translations ?? UiTranslations.Default,
        };
        await ComponentRenderer.RenderComponentAsync<LanguagePicker>(destination, props);
        return destination.GetOutput();
    }

    // --- Empty / no-render cases ---

    [Fact]
    public async Task ShouldRenderNothingWhenLocalesIsNull()
    {
        var html = await RenderPickerAsync(null);

        html.ShouldBeEmpty();
    }

    [Fact]
    public async Task ShouldRenderNothingWhenLocalesHasSingleEntry()
    {
        var locales = new Dictionary<string, LocaleConfig>
        {
            ["root"] = new() { Label = "English", Lang = "en" },
        };

        var html = await RenderPickerAsync(locales);

        html.ShouldBeEmpty();
    }

    // --- Renders select with options ---

    [Fact]
    public async Task ShouldRenderSelectWithOptionsForMultipleLocales()
    {
        var locales = new Dictionary<string, LocaleConfig>
        {
            ["root"] = new() { Label = "English", Lang = "en" },
            ["fr"] = new() { Label = "Français", Lang = "fr" },
        };

        var html = await RenderPickerAsync(locales);

        html.ShouldContain("<nav class=\"language-picker\"");
        html.ShouldContain("<select");
        html.ShouldContain("</select>");
        html.ShouldContain("</nav>");
        html.ShouldContain("English");
        html.ShouldContain("Français");
    }

    [Fact]
    public async Task ShouldRenderOnchangeHandler()
    {
        var locales = new Dictionary<string, LocaleConfig>
        {
            ["root"] = new() { Label = "English", Lang = "en" },
            ["fr"] = new() { Label = "Français", Lang = "fr" },
        };

        var html = await RenderPickerAsync(locales);

        html.ShouldContain("onchange=\"window.location.href=this.value\"");
    }

    // --- Selected option ---

    [Fact]
    public async Task ShouldMarkCurrentLocaleAsSelected()
    {
        var locales = new Dictionary<string, LocaleConfig>
        {
            ["root"] = new() { Label = "English", Lang = "en" },
            ["fr"] = new() { Label = "Français", Lang = "fr" },
        };

        var html = await RenderPickerAsync(locales, currentLocaleKey: "fr", currentContentPath: "/intro");

        html.ShouldContain("<option value=\"/fr/intro\" selected>");
        html.ShouldContain("<option value=\"/intro\">");
        html.ShouldNotContain("<option value=\"/intro\" selected>");
    }

    [Fact]
    public async Task ShouldMarkRootLocaleAsSelectedWhenCurrent()
    {
        var locales = new Dictionary<string, LocaleConfig>
        {
            ["root"] = new() { Label = "English", Lang = "en" },
            ["fr"] = new() { Label = "Français", Lang = "fr" },
        };

        var html = await RenderPickerAsync(locales, currentLocaleKey: "root", currentContentPath: "/intro");

        html.ShouldContain("<option value=\"/intro\" selected>");
        html.ShouldContain("<option value=\"/fr/intro\">");
        html.ShouldNotContain("<option value=\"/fr/intro\" selected>");
    }

    // --- URL building ---

    [Fact]
    public async Task ShouldBuildCorrectUrlsWithBasePath()
    {
        var locales = new Dictionary<string, LocaleConfig>
        {
            ["root"] = new() { Label = "English", Lang = "en" },
            ["fr"] = new() { Label = "Français", Lang = "fr" },
        };

        var html = await RenderPickerAsync(locales, currentLocaleKey: "root", currentContentPath: "/intro", basePath: "/docs");

        html.ShouldContain("value=\"/docs/intro\"");
        html.ShouldContain("value=\"/docs/fr/intro\"");
    }

    [Fact]
    public async Task ShouldBuildCorrectUrlsForRootLocale()
    {
        var locales = new Dictionary<string, LocaleConfig>
        {
            ["root"] = new() { Label = "English", Lang = "en" },
            ["fr"] = new() { Label = "Français", Lang = "fr" },
            ["de"] = new() { Label = "Deutsch", Lang = "de" },
        };

        var html = await RenderPickerAsync(locales, currentLocaleKey: "root", currentContentPath: "/guides/start");

        html.ShouldContain("value=\"/guides/start\"");
        html.ShouldContain("value=\"/fr/guides/start\"");
        html.ShouldContain("value=\"/de/guides/start\"");
    }

    // --- Translations ---

    [Fact]
    public async Task ShouldUseTranslatedAriaLabel()
    {
        var locales = new Dictionary<string, LocaleConfig>
        {
            ["root"] = new() { Label = "English", Lang = "en" },
            ["fr"] = new() { Label = "Français", Lang = "fr" },
        };
        var translations = UiTranslations.Default with
        {
            LanguageSelectLabel = "Choisir la langue",
        };

        var html = await RenderPickerAsync(locales, translations: translations);

        html.ShouldContain("aria-label=\"Choisir la langue\"");
        html.ShouldNotContain("aria-label=\"Select language\"");
    }

    [Fact]
    public async Task ShouldUseDefaultAriaLabel()
    {
        var locales = new Dictionary<string, LocaleConfig>
        {
            ["root"] = new() { Label = "English", Lang = "en" },
            ["fr"] = new() { Label = "Français", Lang = "fr" },
        };

        var html = await RenderPickerAsync(locales);

        html.ShouldContain("aria-label=\"Select language\"");
    }

    // --- HTML encoding ---

    [Fact]
    public async Task ShouldHtmlEncodeLabels()
    {
        var locales = new Dictionary<string, LocaleConfig>
        {
            ["root"] = new() { Label = "English", Lang = "en" },
            ["xss"] = new() { Label = "<script>alert('xss')</script>", Lang = "xx" },
        };

        var html = await RenderPickerAsync(locales);

        html.ShouldNotContain("<script>alert");
        html.ShouldContain("&lt;script&gt;");
    }
}
