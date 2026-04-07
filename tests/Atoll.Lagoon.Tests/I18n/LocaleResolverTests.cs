using Atoll.Lagoon.I18n;

namespace Atoll.Lagoon.Tests.I18n;

public sealed class LocaleResolverTests
{
    // --- Single-language mode (null / empty locales) ---

    [Fact]
    public void ShouldReturnNullWhenLocalesIsNull()
    {
        var result = LocaleResolver.Resolve("/intro", null, "");

        result.ShouldBeNull();
    }

    [Fact]
    public void ShouldReturnNullWhenLocalesIsEmpty()
    {
        var result = LocaleResolver.Resolve("/intro", new Dictionary<string, LocaleConfig>(), "");

        result.ShouldBeNull();
    }

    // --- Root locale ---

    [Fact]
    public void ShouldMatchRootLocaleForUnprefixedPath()
    {
        var locales = new Dictionary<string, LocaleConfig>
        {
            ["root"] = new() { Label = "English", Lang = "en" },
            ["fr"] = new() { Label = "French", Lang = "fr" },
        };

        var result = LocaleResolver.Resolve("/intro", locales, "");

        result.ShouldNotBeNull();
        result.Key.ShouldBe("root");
        result.Config.Lang.ShouldBe("en");
        result.PathPrefix.ShouldBe("");
        result.ContentPath.ShouldBe("/intro");
    }

    [Fact]
    public void ShouldMatchRootLocaleForRootPath()
    {
        var locales = new Dictionary<string, LocaleConfig>
        {
            ["root"] = new() { Label = "English", Lang = "en" },
        };

        var result = LocaleResolver.Resolve("/", locales, "");

        result.ShouldNotBeNull();
        result.Key.ShouldBe("root");
        result.ContentPath.ShouldBe("/");
    }

    // --- Prefixed locale ---

    [Fact]
    public void ShouldMatchPrefixedLocale()
    {
        var locales = new Dictionary<string, LocaleConfig>
        {
            ["root"] = new() { Label = "English", Lang = "en" },
            ["fr"] = new() { Label = "French", Lang = "fr" },
        };

        var result = LocaleResolver.Resolve("/fr/intro", locales, "");

        result.ShouldNotBeNull();
        result.Key.ShouldBe("fr");
        result.Config.Lang.ShouldBe("fr");
        result.PathPrefix.ShouldBe("/fr");
        result.ContentPath.ShouldBe("/intro");
    }

    [Fact]
    public void ShouldMatchPrefixedLocaleExactPath()
    {
        var locales = new Dictionary<string, LocaleConfig>
        {
            ["root"] = new() { Label = "English", Lang = "en" },
            ["fr"] = new() { Label = "French", Lang = "fr" },
        };

        var result = LocaleResolver.Resolve("/fr", locales, "");

        result.ShouldNotBeNull();
        result.Key.ShouldBe("fr");
        result.ContentPath.ShouldBe("/");
    }

    [Fact]
    public void ShouldMatchCorrectLocaleFromMultiple()
    {
        var locales = new Dictionary<string, LocaleConfig>
        {
            ["root"] = new() { Label = "English", Lang = "en" },
            ["fr"] = new() { Label = "French", Lang = "fr" },
            ["de"] = new() { Label = "German", Lang = "de" },
            ["ja"] = new() { Label = "Japanese", Lang = "ja" },
        };

        var result = LocaleResolver.Resolve("/de/guides/setup", locales, "");

        result.ShouldNotBeNull();
        result.Key.ShouldBe("de");
        result.Config.Lang.ShouldBe("de");
        result.ContentPath.ShouldBe("/guides/setup");
    }

    // --- RTL locale ---

    [Fact]
    public void ShouldReturnRtlDirection()
    {
        var locales = new Dictionary<string, LocaleConfig>
        {
            ["root"] = new() { Label = "English", Lang = "en" },
            ["ar"] = new() { Label = "Arabic", Lang = "ar", Dir = "rtl" },
        };

        var result = LocaleResolver.Resolve("/ar/intro", locales, "");

        result.ShouldNotBeNull();
        result.Config.Dir.ShouldBe("rtl");
    }

    // --- Fallback when no root key ---

    [Fact]
    public void ShouldFallbackToFirstLocaleWhenNoRootAndNoMatch()
    {
        var locales = new Dictionary<string, LocaleConfig>
        {
            ["en"] = new() { Label = "English", Lang = "en" },
            ["fr"] = new() { Label = "French", Lang = "fr" },
        };

        var result = LocaleResolver.Resolve("/unknown/page", locales, "");

        result.ShouldNotBeNull();
        result.Key.ShouldBe("en");
    }

    // --- BCP-47 tags with hyphens ---

    [Fact]
    public void ShouldMatchHyphenatedLocaleKey()
    {
        var locales = new Dictionary<string, LocaleConfig>
        {
            ["root"] = new() { Label = "English", Lang = "en" },
            ["zh-cn"] = new() { Label = "Chinese", Lang = "zh-CN" },
        };

        var result = LocaleResolver.Resolve("/zh-cn/intro", locales, "");

        result.ShouldNotBeNull();
        result.Key.ShouldBe("zh-cn");
        result.Config.Lang.ShouldBe("zh-CN");
        result.ContentPath.ShouldBe("/intro");
    }

    // --- BasePath handling ---

    [Fact]
    public void ShouldStripBasePathBeforeMatching()
    {
        var locales = new Dictionary<string, LocaleConfig>
        {
            ["root"] = new() { Label = "English", Lang = "en" },
            ["fr"] = new() { Label = "French", Lang = "fr" },
        };

        var result = LocaleResolver.Resolve("/docs/fr/intro", locales, "/docs");

        result.ShouldNotBeNull();
        result.Key.ShouldBe("fr");
        result.ContentPath.ShouldBe("/intro");
    }

    [Fact]
    public void ShouldMatchRootLocaleWithBasePath()
    {
        var locales = new Dictionary<string, LocaleConfig>
        {
            ["root"] = new() { Label = "English", Lang = "en" },
            ["fr"] = new() { Label = "French", Lang = "fr" },
        };

        var result = LocaleResolver.Resolve("/docs/intro", locales, "/docs");

        result.ShouldNotBeNull();
        result.Key.ShouldBe("root");
        result.ContentPath.ShouldBe("/intro");
    }

    [Fact]
    public void ShouldHandleBasePathAtRoot()
    {
        var locales = new Dictionary<string, LocaleConfig>
        {
            ["root"] = new() { Label = "English", Lang = "en" },
        };

        var result = LocaleResolver.Resolve("/docs", locales, "/docs");

        result.ShouldNotBeNull();
        result.Key.ShouldBe("root");
        result.ContentPath.ShouldBe("/");
    }

    // --- Per-locale translations ---

    [Fact]
    public void ShouldReturnPerLocaleTranslations()
    {
        var frenchTranslations = UiTranslations.Default with { PaginationNext = "Suivant" };
        var locales = new Dictionary<string, LocaleConfig>
        {
            ["root"] = new() { Label = "English", Lang = "en" },
            ["fr"] = new() { Label = "French", Lang = "fr", Translations = frenchTranslations },
        };

        var result = LocaleResolver.Resolve("/fr/intro", locales, "");

        result.ShouldNotBeNull();
        result.Config.Translations.PaginationNext.ShouldBe("Suivant");
    }

    // --- Case-insensitive matching ---

    [Fact]
    public void ShouldMatchLocaleKeysCaseInsensitively()
    {
        var locales = new Dictionary<string, LocaleConfig>
        {
            ["root"] = new() { Label = "English", Lang = "en" },
            ["fr"] = new() { Label = "French", Lang = "fr" },
        };

        var result = LocaleResolver.Resolve("/FR/intro", locales, "");

        result.ShouldNotBeNull();
        result.Key.ShouldBe("fr");
        result.Config.Lang.ShouldBe("fr");
    }

    // --- Overload without basePath ---

    [Fact]
    public void ShouldWorkWithOverloadWithoutBasePath()
    {
        var locales = new Dictionary<string, LocaleConfig>
        {
            ["root"] = new() { Label = "English", Lang = "en" },
            ["fr"] = new() { Label = "French", Lang = "fr" },
        };

        var result = LocaleResolver.Resolve("/fr/intro", locales);

        result.ShouldNotBeNull();
        result.Key.ShouldBe("fr");
        result.ContentPath.ShouldBe("/intro");
    }

    // --- Empty remaining path normalizes to "/" ---

    [Fact]
    public void ShouldNormalizeEmptyContentPathToSlash()
    {
        var locales = new Dictionary<string, LocaleConfig>
        {
            ["root"] = new() { Label = "English", Lang = "en" },
            ["fr"] = new() { Label = "French", Lang = "fr" },
        };

        var result = LocaleResolver.Resolve("/docs/fr", locales, "/docs");

        result.ShouldNotBeNull();
        result.Key.ShouldBe("fr");
        result.ContentPath.ShouldBe("/");
    }

    // --- No prefix match and no "root" key falls back to first ---

    [Fact]
    public void ShouldFallbackToFirstLocaleWhenPathDoesNotMatchAnyPrefixAndNoRoot()
    {
        var locales = new Dictionary<string, LocaleConfig>
        {
            ["en"] = new() { Label = "English", Lang = "en" },
            ["fr"] = new() { Label = "French", Lang = "fr" },
        };

        var result = LocaleResolver.Resolve("/intro", locales, "");

        result.ShouldNotBeNull();
        // "en" starts with "/en" — "/intro" doesn't match "/en" prefix, no root key
        result.Key.ShouldBe("en");
    }
}
