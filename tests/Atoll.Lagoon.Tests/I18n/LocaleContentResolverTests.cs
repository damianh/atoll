using Atoll.Lagoon.I18n;
using Shouldly;
using Xunit;

namespace Atoll.Lagoon.Tests.I18n;

public sealed class LocaleContentResolverTests
{
    // --- GetLocaleContentPath ---

    [Fact]
    public void GetLocaleContentPathShouldReturnUnchangedPathForRootLocale()
    {
        var result = LocaleContentResolver.GetLocaleContentPath("guides/intro", "root");

        result.ShouldBe("guides/intro");
    }

    [Fact]
    public void GetLocaleContentPathShouldReturnUnchangedPathForRootLocaleCaseInsensitive()
    {
        var result = LocaleContentResolver.GetLocaleContentPath("guides/intro", "ROOT");

        result.ShouldBe("guides/intro");
    }

    [Fact]
    public void GetLocaleContentPathShouldReturnUnchangedPathForEmptyLocaleKey()
    {
        var result = LocaleContentResolver.GetLocaleContentPath("guides/intro", "");

        result.ShouldBe("guides/intro");
    }

    [Fact]
    public void GetLocaleContentPathShouldPrefixWithLocaleKey()
    {
        var result = LocaleContentResolver.GetLocaleContentPath("guides/intro", "fr");

        result.ShouldBe("fr/guides/intro");
    }

    [Fact]
    public void GetLocaleContentPathShouldPrefixWithLocaleKeyForNestedPath()
    {
        var result = LocaleContentResolver.GetLocaleContentPath("guides/advanced/config", "es");

        result.ShouldBe("es/guides/advanced/config");
    }

    [Fact]
    public void GetLocaleContentPathShouldHandleEmptyContentPath()
    {
        var result = LocaleContentResolver.GetLocaleContentPath("", "fr");

        result.ShouldBe("fr");
    }

    [Fact]
    public void GetLocaleContentPathShouldHandleEmptyContentPathForRootLocale()
    {
        var result = LocaleContentResolver.GetLocaleContentPath("", "root");

        result.ShouldBe("");
    }

    [Fact]
    public void GetLocaleContentPathShouldStripLeadingSlashFromContentPath()
    {
        var result = LocaleContentResolver.GetLocaleContentPath("/guides/intro", "fr");

        result.ShouldBe("fr/guides/intro");
    }

    [Fact]
    public void GetLocaleContentPathShouldStripLeadingSlashForRootLocale()
    {
        var result = LocaleContentResolver.GetLocaleContentPath("/guides/intro", "root");

        result.ShouldBe("guides/intro");
    }

    [Fact]
    public void GetLocaleContentPathShouldHandleBackslashesInContentPath()
    {
        var result = LocaleContentResolver.GetLocaleContentPath("guides\\intro", "fr");

        result.ShouldBe("fr/guides/intro");
    }

    // --- StripLocaleFromSlug ---

    [Fact]
    public void StripLocaleFromSlugShouldReturnSlugUnchangedForRootLocale()
    {
        var result = LocaleContentResolver.StripLocaleFromSlug("guides/intro", "root");

        result.ShouldBe("guides/intro");
    }

    [Fact]
    public void StripLocaleFromSlugShouldReturnSlugUnchangedForEmptyLocaleKey()
    {
        var result = LocaleContentResolver.StripLocaleFromSlug("guides/intro", "");

        result.ShouldBe("guides/intro");
    }

    [Fact]
    public void StripLocaleFromSlugShouldStripLocalePrefixFromSlug()
    {
        var result = LocaleContentResolver.StripLocaleFromSlug("fr/guides/intro", "fr");

        result.ShouldBe("guides/intro");
    }

    [Fact]
    public void StripLocaleFromSlugShouldStripLocalePrefixCaseInsensitively()
    {
        var result = LocaleContentResolver.StripLocaleFromSlug("FR/guides/intro", "fr");

        result.ShouldBe("guides/intro");
    }

    [Fact]
    public void StripLocaleFromSlugShouldReturnEmptyWhenSlugIsJustLocaleKey()
    {
        var result = LocaleContentResolver.StripLocaleFromSlug("fr", "fr");

        result.ShouldBe("");
    }

    [Fact]
    public void StripLocaleFromSlugShouldReturnSlugUnchangedWhenNoMatch()
    {
        var result = LocaleContentResolver.StripLocaleFromSlug("guides/intro", "fr");

        result.ShouldBe("guides/intro");
    }

    [Fact]
    public void StripLocaleFromSlugShouldHandleLeadingSlash()
    {
        var result = LocaleContentResolver.StripLocaleFromSlug("/fr/guides/intro", "fr");

        result.ShouldBe("guides/intro");
    }

    [Fact]
    public void StripLocaleFromSlugShouldNotStripPartialMatch()
    {
        // "french" starts with "fr" but should not be stripped
        var result = LocaleContentResolver.StripLocaleFromSlug("french/guides/intro", "fr");

        result.ShouldBe("french/guides/intro");
    }

    [Fact]
    public void StripLocaleFromSlugShouldHandleHyphenatedLocaleKey()
    {
        var result = LocaleContentResolver.StripLocaleFromSlug("zh-cn/guides/intro", "zh-cn");

        result.ShouldBe("guides/intro");
    }
}
