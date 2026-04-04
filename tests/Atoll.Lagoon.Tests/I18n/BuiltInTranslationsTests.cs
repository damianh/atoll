using Atoll.Lagoon.I18n;
using Shouldly;
using Xunit;

namespace Atoll.Lagoon.Tests.I18n;

public sealed class BuiltInTranslationsTests
{
    // --- All properties populated for each language ---

    [Theory]
    [InlineData("English")]
    [InlineData("French")]
    [InlineData("Spanish")]
    [InlineData("German")]
    [InlineData("Japanese")]
    [InlineData("Arabic")]
    [InlineData("ChineseSimplified")]
    [InlineData("Portuguese")]
    public void AllPropertiesShouldBeNonNullAndNonEmpty(string languageName)
    {
        var translations = GetByName(languageName);

        translations.SkipLinkLabel.ShouldNotBeNullOrEmpty();
        translations.SearchLabel.ShouldNotBeNullOrEmpty();
        translations.SearchPlaceholder.ShouldNotBeNullOrEmpty();
        translations.SearchDialogLabel.ShouldNotBeNullOrEmpty();
        translations.SearchCloseLabel.ShouldNotBeNullOrEmpty();
        translations.SearchResultsLabel.ShouldNotBeNullOrEmpty();
        translations.SearchNoResults.ShouldNotBeNullOrEmpty();
        translations.ThemeToggleLabel.ShouldNotBeNullOrEmpty();
        translations.ThemeSwitchToLight.ShouldNotBeNullOrEmpty();
        translations.ThemeSwitchToDark.ShouldNotBeNullOrEmpty();
        translations.SidebarNavLabel.ShouldNotBeNullOrEmpty();
        translations.SiteNavigationLabel.ShouldNotBeNullOrEmpty();
        translations.MobileNavOpenLabel.ShouldNotBeNullOrEmpty();
        translations.BreadcrumbsLabel.ShouldNotBeNullOrEmpty();
        translations.PaginationLabel.ShouldNotBeNullOrEmpty();
        translations.PaginationPrevious.ShouldNotBeNullOrEmpty();
        translations.PaginationNext.ShouldNotBeNullOrEmpty();
        translations.TocLabel.ShouldNotBeNullOrEmpty();
        translations.BuiltWithLabel.ShouldNotBeNullOrEmpty();
        translations.LanguageSelectLabel.ShouldNotBeNullOrEmpty();
        translations.UntranslatedContentNotice.ShouldNotBeNullOrEmpty();
    }

    // --- ForLanguage returns correct instance ---

    [Fact]
    public void ForLanguageShouldReturnEnglishForEn()
    {
        BuiltInTranslations.ForLanguage("en").ShouldBeSameAs(BuiltInTranslations.English);
    }

    [Fact]
    public void ForLanguageShouldReturnFrenchForFr()
    {
        BuiltInTranslations.ForLanguage("fr").ShouldBeSameAs(BuiltInTranslations.French);
    }

    [Fact]
    public void ForLanguageShouldReturnSpanishForEs()
    {
        BuiltInTranslations.ForLanguage("es").ShouldBeSameAs(BuiltInTranslations.Spanish);
    }

    [Fact]
    public void ForLanguageShouldReturnGermanForDe()
    {
        BuiltInTranslations.ForLanguage("de").ShouldBeSameAs(BuiltInTranslations.German);
    }

    [Fact]
    public void ForLanguageShouldReturnJapaneseForJa()
    {
        BuiltInTranslations.ForLanguage("ja").ShouldBeSameAs(BuiltInTranslations.Japanese);
    }

    [Fact]
    public void ForLanguageShouldReturnArabicForAr()
    {
        BuiltInTranslations.ForLanguage("ar").ShouldBeSameAs(BuiltInTranslations.Arabic);
    }

    [Fact]
    public void ForLanguageShouldReturnChineseSimplifiedForZhCN()
    {
        BuiltInTranslations.ForLanguage("zh-CN").ShouldBeSameAs(BuiltInTranslations.ChineseSimplified);
    }

    [Fact]
    public void ForLanguageShouldReturnPortugueseForPtBR()
    {
        BuiltInTranslations.ForLanguage("pt-BR").ShouldBeSameAs(BuiltInTranslations.Portuguese);
    }

    // --- ForLanguage returns null for unknown languages ---

    [Fact]
    public void ForLanguageShouldReturnNullForUnknownLanguage()
    {
        BuiltInTranslations.ForLanguage("xx").ShouldBeNull();
    }

    [Fact]
    public void ForLanguageShouldReturnNullForEmptyString()
    {
        BuiltInTranslations.ForLanguage("").ShouldBeNull();
    }

    [Fact]
    public void ForLanguageShouldReturnNullForWhitespace()
    {
        BuiltInTranslations.ForLanguage("  ").ShouldBeNull();
    }

    // --- Case-insensitive lookup ---

    [Fact]
    public void ForLanguageShouldBeCaseInsensitiveUppercase()
    {
        BuiltInTranslations.ForLanguage("FR").ShouldBeSameAs(BuiltInTranslations.French);
    }

    [Fact]
    public void ForLanguageShouldBeCaseInsensitiveMixedCase()
    {
        BuiltInTranslations.ForLanguage("De").ShouldBeSameAs(BuiltInTranslations.German);
    }

    [Fact]
    public void ForLanguageShouldBeCaseInsensitiveZhCn()
    {
        BuiltInTranslations.ForLanguage("ZH-cn").ShouldBeSameAs(BuiltInTranslations.ChineseSimplified);
    }

    // --- Primary subtag matching ---

    [Fact]
    public void ForLanguageShouldMatchFrenchForFrCA()
    {
        BuiltInTranslations.ForLanguage("fr-CA").ShouldBeSameAs(BuiltInTranslations.French);
    }

    [Fact]
    public void ForLanguageShouldMatchFrenchForFrBE()
    {
        BuiltInTranslations.ForLanguage("fr-BE").ShouldBeSameAs(BuiltInTranslations.French);
    }

    [Fact]
    public void ForLanguageShouldMatchPortugueseForPt()
    {
        BuiltInTranslations.ForLanguage("pt").ShouldBeSameAs(BuiltInTranslations.Portuguese);
    }

    [Fact]
    public void ForLanguageShouldMatchChineseForZh()
    {
        BuiltInTranslations.ForLanguage("zh").ShouldBeSameAs(BuiltInTranslations.ChineseSimplified);
    }

    [Fact]
    public void ForLanguageShouldMatchSpanishForEsMX()
    {
        BuiltInTranslations.ForLanguage("es-MX").ShouldBeSameAs(BuiltInTranslations.Spanish);
    }

    [Fact]
    public void ForLanguageShouldMatchGermanForDeAT()
    {
        BuiltInTranslations.ForLanguage("de-AT").ShouldBeSameAs(BuiltInTranslations.German);
    }

    // --- English is the default singleton ---

    [Fact]
    public void EnglishShouldBeSameAsDefault()
    {
        BuiltInTranslations.English.ShouldBeSameAs(UiTranslations.Default);
    }

    // --- Non-English languages differ from English ---

    [Theory]
    [InlineData("French")]
    [InlineData("Spanish")]
    [InlineData("German")]
    [InlineData("Japanese")]
    [InlineData("Arabic")]
    [InlineData("ChineseSimplified")]
    [InlineData("Portuguese")]
    public void NonEnglishLanguageShouldDifferFromEnglish(string languageName)
    {
        var translations = GetByName(languageName);

        translations.ShouldNotBeSameAs(BuiltInTranslations.English);
        translations.PaginationNext.ShouldNotBe(BuiltInTranslations.English.PaginationNext);
    }

    // --- Auto-resolution in LocaleResolver ---

    [Fact]
    public void ResolverShouldAutoResolveFrenchTranslationsWhenNoExplicitTranslations()
    {
        var locales = new Dictionary<string, LocaleConfig>
        {
            ["root"] = new() { Label = "English", Lang = "en" },
            ["fr"] = new() { Label = "Français", Lang = "fr" },
        };

        var result = LocaleResolver.Resolve("/fr/intro", locales, "");

        result.ShouldNotBeNull();
        result.Config.Translations.ShouldBeSameAs(BuiltInTranslations.French);
        result.Config.Translations.PaginationNext.ShouldBe("Suivant");
    }

    [Fact]
    public void ResolverShouldAutoResolveGermanTranslationsForRootLocale()
    {
        var locales = new Dictionary<string, LocaleConfig>
        {
            ["root"] = new() { Label = "Deutsch", Lang = "de" },
        };

        var result = LocaleResolver.Resolve("/intro", locales, "");

        result.ShouldNotBeNull();
        result.Config.Translations.ShouldBeSameAs(BuiltInTranslations.German);
    }

    [Fact]
    public void ResolverShouldNotOverrideExplicitUserTranslations()
    {
        var customTranslations = new UiTranslations
        {
            PaginationNext = "Mon suivant",
        };

        var locales = new Dictionary<string, LocaleConfig>
        {
            ["root"] = new() { Label = "English", Lang = "en" },
            ["fr"] = new() { Label = "Français", Lang = "fr", Translations = customTranslations },
        };

        var result = LocaleResolver.Resolve("/fr/intro", locales, "");

        result.ShouldNotBeNull();
        result.Config.Translations.ShouldBeSameAs(customTranslations);
        result.Config.Translations.PaginationNext.ShouldBe("Mon suivant");
    }

    [Fact]
    public void ResolverShouldKeepDefaultTranslationsForUnknownLanguage()
    {
        var locales = new Dictionary<string, LocaleConfig>
        {
            ["root"] = new() { Label = "English", Lang = "en" },
            ["xx"] = new() { Label = "Unknown", Lang = "xx" },
        };

        var result = LocaleResolver.Resolve("/xx/intro", locales, "");

        result.ShouldNotBeNull();
        result.Config.Translations.ShouldBeSameAs(UiTranslations.Default);
    }

    [Fact]
    public void ResolverShouldAutoResolveEnglishAndKeepDefaultTranslations()
    {
        var locales = new Dictionary<string, LocaleConfig>
        {
            ["root"] = new() { Label = "English", Lang = "en" },
            ["fr"] = new() { Label = "Français", Lang = "fr" },
        };

        var result = LocaleResolver.Resolve("/intro", locales, "");

        result.ShouldNotBeNull();
        // English auto-resolve should still return the Default (since English IS the Default)
        result.Config.Translations.ShouldBeSameAs(UiTranslations.Default);
    }

    // --- User override via with expression on built-in translations (Task 44) ---

    [Fact]
    public void WithExpressionShouldOverrideSinglePropertyOnBuiltInTranslation()
    {
        var custom = BuiltInTranslations.French with { SearchPlaceholder = "Mon texte" };

        custom.SearchPlaceholder.ShouldBe("Mon texte");
        custom.PaginationNext.ShouldBe("Suivant"); // unchanged from French
        custom.PaginationPrevious.ShouldBe("Précédent"); // unchanged from French
    }

    [Fact]
    public void WithExpressionShouldPreserveAllOtherPropertiesOnBuiltInTranslation()
    {
        var custom = BuiltInTranslations.Spanish with { TocLabel = "Mi contenido" };

        custom.TocLabel.ShouldBe("Mi contenido");
        custom.SkipLinkLabel.ShouldBe("Ir al contenido"); // unchanged from Spanish
        custom.SearchLabel.ShouldBe("Buscar"); // unchanged from Spanish
        custom.PaginationNext.ShouldBe("Siguiente"); // unchanged from Spanish
    }

    [Fact]
    public void WithExpressionShouldCreateNewInstanceNotMutateOriginal()
    {
        var original = BuiltInTranslations.German;
        var custom = original with { PaginationNext = "Custom" };

        custom.PaginationNext.ShouldBe("Custom");
        original.PaginationNext.ShouldBe("Weiter"); // original unchanged
        BuiltInTranslations.German.PaginationNext.ShouldBe("Weiter"); // static unchanged
    }

    private static UiTranslations GetByName(string name)
    {
        return name switch
        {
            "English" => BuiltInTranslations.English,
            "French" => BuiltInTranslations.French,
            "Spanish" => BuiltInTranslations.Spanish,
            "German" => BuiltInTranslations.German,
            "Japanese" => BuiltInTranslations.Japanese,
            "Arabic" => BuiltInTranslations.Arabic,
            "ChineseSimplified" => BuiltInTranslations.ChineseSimplified,
            "Portuguese" => BuiltInTranslations.Portuguese,
            _ => throw new ArgumentException($"Unknown language: {name}"),
        };
    }
}
