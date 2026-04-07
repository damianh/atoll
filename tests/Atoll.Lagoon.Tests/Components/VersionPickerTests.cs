using Atoll.Components;
using Atoll.Lagoon.Components;
using Atoll.Lagoon.I18n;
using Atoll.Lagoon.Versioning;
using Atoll.Rendering;

namespace Atoll.Lagoon.Tests.Components;

public sealed class VersionPickerTests
{
    private static async Task<string> RenderPickerAsync(
        IReadOnlyDictionary<string, VersionConfig>? versions,
        string currentVersionKey = "current",
        string currentContentPath = "/",
        string localePrefix = "",
        string basePath = "",
        UiTranslations? translations = null)
    {
        var destination = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            ["Versions"] = versions,
            ["CurrentVersionKey"] = currentVersionKey,
            ["CurrentContentPath"] = currentContentPath,
            ["LocalePrefix"] = localePrefix,
            ["BasePath"] = basePath,
            ["Translations"] = translations ?? UiTranslations.Default,
        };
        await ComponentRenderer.RenderComponentAsync<VersionPicker>(destination, props);
        return destination.GetOutput();
    }

    // --- Empty / no-render cases ---

    [Fact]
    public async Task ShouldRenderNothingWhenVersionsIsNull()
    {
        var html = await RenderPickerAsync(null);

        html.ShouldBeEmpty();
    }

    [Fact]
    public async Task ShouldRenderNothingWhenVersionsHasSingleEntry()
    {
        var versions = new Dictionary<string, VersionConfig>
        {
            ["current"] = new() { Label = "Latest", Slug = "current" },
        };

        var html = await RenderPickerAsync(versions);

        html.ShouldBeEmpty();
    }

    // --- Renders select with options ---

    [Fact]
    public async Task ShouldRenderSelectWithOptionsForMultipleVersions()
    {
        var versions = new Dictionary<string, VersionConfig>
        {
            ["current"] = new() { Label = "Latest", Slug = "current" },
            ["v1.0"] = new() { Label = "v1.0", Slug = "v1.0" },
        };

        var html = await RenderPickerAsync(versions);

        html.ShouldContain("<nav class=\"version-picker\"");
        html.ShouldContain("<select");
        html.ShouldContain("</select>");
        html.ShouldContain("</nav>");
        html.ShouldContain("Latest");
        html.ShouldContain("v1.0");
    }

    [Fact]
    public async Task ShouldRenderOnchangeHandler()
    {
        var versions = new Dictionary<string, VersionConfig>
        {
            ["current"] = new() { Label = "Latest", Slug = "current" },
            ["v1.0"] = new() { Label = "v1.0", Slug = "v1.0" },
        };

        var html = await RenderPickerAsync(versions);

        html.ShouldContain("onchange=\"window.location.href=this.value\"");
    }

    // --- Selected option ---

    [Fact]
    public async Task ShouldMarkCurrentVersionAsSelected()
    {
        var versions = new Dictionary<string, VersionConfig>
        {
            ["current"] = new() { Label = "Latest", Slug = "current" },
            ["v1.0"] = new() { Label = "v1.0", Slug = "v1.0" },
        };

        var html = await RenderPickerAsync(versions, currentVersionKey: "v1.0", currentContentPath: "/intro");

        html.ShouldContain("<option value=\"/v1.0/intro\" selected>");
        html.ShouldContain("<option value=\"/intro\">");
        html.ShouldNotContain("<option value=\"/intro\" selected>");
    }

    [Fact]
    public async Task ShouldMarkCurrentVersionKeyAsSelectedWhenCurrent()
    {
        var versions = new Dictionary<string, VersionConfig>
        {
            ["current"] = new() { Label = "Latest", Slug = "current" },
            ["v1.0"] = new() { Label = "v1.0", Slug = "v1.0" },
        };

        var html = await RenderPickerAsync(versions, currentVersionKey: "current", currentContentPath: "/intro");

        html.ShouldContain("<option value=\"/intro\" selected>");
        html.ShouldContain("<option value=\"/v1.0/intro\">");
        html.ShouldNotContain("<option value=\"/v1.0/intro\" selected>");
    }

    // --- URL building ---

    [Fact]
    public async Task ShouldBuildCorrectUrlsWithBasePath()
    {
        var versions = new Dictionary<string, VersionConfig>
        {
            ["current"] = new() { Label = "Latest", Slug = "current" },
            ["v1.0"] = new() { Label = "v1.0", Slug = "v1.0" },
        };

        var html = await RenderPickerAsync(versions, currentVersionKey: "current", currentContentPath: "/intro", basePath: "/docs");

        html.ShouldContain("value=\"/docs/intro\"");
        html.ShouldContain("value=\"/docs/v1.0/intro\"");
    }

    [Fact]
    public async Task ShouldBuildCorrectUrlsWithLocalePrefix()
    {
        var versions = new Dictionary<string, VersionConfig>
        {
            ["current"] = new() { Label = "Latest", Slug = "current" },
            ["v1.0"] = new() { Label = "v1.0", Slug = "v1.0" },
        };

        var html = await RenderPickerAsync(versions, currentVersionKey: "current", currentContentPath: "/intro", localePrefix: "/fr", basePath: "/docs");

        html.ShouldContain("value=\"/docs/fr/intro\"");
        html.ShouldContain("value=\"/docs/fr/v1.0/intro\"");
    }

    [Fact]
    public async Task ShouldBuildCorrectUrlsForMultipleVersions()
    {
        var versions = new Dictionary<string, VersionConfig>
        {
            ["current"] = new() { Label = "Latest", Slug = "current" },
            ["v2.0"] = new() { Label = "v2.0", Slug = "v2.0" },
            ["v1.0"] = new() { Label = "v1.0", Slug = "v1.0" },
        };

        var html = await RenderPickerAsync(versions, currentVersionKey: "current", currentContentPath: "/guides/start");

        html.ShouldContain("value=\"/guides/start\"");
        html.ShouldContain("value=\"/v2.0/guides/start\"");
        html.ShouldContain("value=\"/v1.0/guides/start\"");
    }

    // --- Translations ---

    [Fact]
    public async Task ShouldUseTranslatedAriaLabel()
    {
        var versions = new Dictionary<string, VersionConfig>
        {
            ["current"] = new() { Label = "Latest", Slug = "current" },
            ["v1.0"] = new() { Label = "v1.0", Slug = "v1.0" },
        };
        var translations = UiTranslations.Default with
        {
            VersionSelectLabel = "Choisir la version",
        };

        var html = await RenderPickerAsync(versions, translations: translations);

        html.ShouldContain("aria-label=\"Choisir la version\"");
        html.ShouldNotContain("aria-label=\"Select version\"");
    }

    [Fact]
    public async Task ShouldUseDefaultAriaLabel()
    {
        var versions = new Dictionary<string, VersionConfig>
        {
            ["current"] = new() { Label = "Latest", Slug = "current" },
            ["v1.0"] = new() { Label = "v1.0", Slug = "v1.0" },
        };

        var html = await RenderPickerAsync(versions);

        html.ShouldContain("aria-label=\"Select version\"");
    }

    // --- HTML encoding ---

    [Fact]
    public async Task ShouldHtmlEncodeLabels()
    {
        var versions = new Dictionary<string, VersionConfig>
        {
            ["current"] = new() { Label = "Latest", Slug = "current" },
            ["xss"] = new() { Label = "<script>alert('xss')</script>", Slug = "xss" },
        };

        var html = await RenderPickerAsync(versions);

        html.ShouldNotContain("<script>alert");
        html.ShouldContain("&lt;script&gt;");
    }
}
