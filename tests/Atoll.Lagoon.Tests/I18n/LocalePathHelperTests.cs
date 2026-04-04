using Atoll.Lagoon.I18n;
using Shouldly;
using Xunit;

namespace Atoll.Lagoon.Tests.I18n;

public sealed class LocalePathHelperTests
{
    // --- PrefixPath ---

    [Fact]
    public void PrefixPathShouldReturnContentPathWhenNoLocaleOrBasePath()
    {
        var result = LocalePathHelper.PrefixPath("/intro", "");

        result.ShouldBe("/intro");
    }

    [Fact]
    public void PrefixPathShouldPrependLocalePrefix()
    {
        var result = LocalePathHelper.PrefixPath("/intro", "/fr");

        result.ShouldBe("/fr/intro");
    }

    [Fact]
    public void PrefixPathShouldInsertLocalePrefixAfterBasePath()
    {
        var result = LocalePathHelper.PrefixPath("/intro", "/fr", "/docs");

        result.ShouldBe("/docs/fr/intro");
    }

    [Fact]
    public void PrefixPathShouldHandleRootLocaleWithBasePath()
    {
        var result = LocalePathHelper.PrefixPath("/intro", "", "/docs");

        result.ShouldBe("/docs/intro");
    }

    [Fact]
    public void PrefixPathShouldHandleEmptyContentPath()
    {
        var result = LocalePathHelper.PrefixPath("/", "/fr", "/docs");

        result.ShouldBe("/docs/fr/");
    }

    [Fact]
    public void PrefixPathShouldHandleTrailingSlashOnBasePath()
    {
        var result = LocalePathHelper.PrefixPath("/intro", "/fr", "/docs/");

        result.ShouldBe("/docs/fr/intro");
    }

    [Fact]
    public void PrefixPathShouldHandleContentPathWithoutLeadingSlash()
    {
        var result = LocalePathHelper.PrefixPath("intro", "/fr", "/docs");

        result.ShouldBe("/docs/fr/intro");
    }

    [Fact]
    public void PrefixPathShouldReturnSlashForAllEmpty()
    {
        var result = LocalePathHelper.PrefixPath("", "");

        result.ShouldBe("/");
    }

    [Fact]
    public void PrefixPathShouldHandleDeepContentPath()
    {
        var result = LocalePathHelper.PrefixPath("/guides/getting-started", "/fr", "/docs");

        result.ShouldBe("/docs/fr/guides/getting-started");
    }

    // --- StripPrefix ---

    [Fact]
    public void StripPrefixShouldRemoveLocalePrefixFromPath()
    {
        var result = LocalePathHelper.StripPrefix("/fr/intro", "/fr");

        result.ShouldBe("/intro");
    }

    [Fact]
    public void StripPrefixShouldRemoveBasePathAndLocalePrefix()
    {
        var result = LocalePathHelper.StripPrefix("/docs/fr/intro", "/fr", "/docs");

        result.ShouldBe("/intro");
    }

    [Fact]
    public void StripPrefixShouldReturnSlashWhenNothingRemains()
    {
        var result = LocalePathHelper.StripPrefix("/docs/fr", "/fr", "/docs");

        result.ShouldBe("/");
    }

    [Fact]
    public void StripPrefixShouldReturnPathUnchangedWhenNoPrefixMatch()
    {
        var result = LocalePathHelper.StripPrefix("/other/intro", "/fr", "/docs");

        result.ShouldBe("/other/intro");
    }

    [Fact]
    public void StripPrefixShouldHandleRootLocale()
    {
        var result = LocalePathHelper.StripPrefix("/docs/intro", "", "/docs");

        result.ShouldBe("/intro");
    }

    [Fact]
    public void StripPrefixShouldHandleNoBasePathOrLocale()
    {
        var result = LocalePathHelper.StripPrefix("/intro", "");

        result.ShouldBe("/intro");
    }

    // --- BelongsToLocale ---

    [Fact]
    public void BelongsToLocaleShouldReturnTrueForMatchingLocalePrefix()
    {
        var result = LocalePathHelper.BelongsToLocale("/docs/fr/intro", "/fr", "/docs");

        result.ShouldBeTrue();
    }

    [Fact]
    public void BelongsToLocaleShouldReturnTrueForExactLocalePrefixMatch()
    {
        var result = LocalePathHelper.BelongsToLocale("/docs/fr", "/fr", "/docs");

        result.ShouldBeTrue();
    }

    [Fact]
    public void BelongsToLocaleShouldReturnFalseForDifferentLocale()
    {
        var result = LocalePathHelper.BelongsToLocale("/docs/es/intro", "/fr", "/docs");

        result.ShouldBeFalse();
    }

    [Fact]
    public void BelongsToLocaleShouldReturnTrueForRootLocale()
    {
        var result = LocalePathHelper.BelongsToLocale("/docs/intro", "", "/docs");

        result.ShouldBeTrue();
    }

    [Fact]
    public void BelongsToLocaleShouldReturnTrueForRootLocaleWithNoBasePath()
    {
        var result = LocalePathHelper.BelongsToLocale("/intro", "");

        result.ShouldBeTrue();
    }

    [Fact]
    public void BelongsToLocaleShouldReturnFalseForPartialPrefixMatch()
    {
        // "/french" should NOT match locale prefix "/fr"
        var result = LocalePathHelper.BelongsToLocale("/french/intro", "/fr");

        result.ShouldBeFalse();
    }

    [Fact]
    public void BelongsToLocaleShouldBeCaseInsensitive()
    {
        var result = LocalePathHelper.BelongsToLocale("/DOCS/FR/intro", "/fr", "/docs");

        result.ShouldBeTrue();
    }

    [Fact]
    public void BelongsToLocaleShouldReturnFalseWhenBasePathDoesNotMatch()
    {
        var result = LocalePathHelper.BelongsToLocale("/other/fr/intro", "/fr", "/docs");

        result.ShouldBeFalse();
    }
}
