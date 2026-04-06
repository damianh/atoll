using Atoll.Lagoon.Versioning;
using Shouldly;
using Xunit;

namespace Atoll.Lagoon.Tests.Versioning;

public sealed class VersionPathHelperTests
{
    // --- PrefixPath ---

    [Fact]
    public void PrefixPathShouldReturnContentPathWhenNoVersionOrBasePath()
    {
        var result = VersionPathHelper.PrefixPath("/intro", "");

        result.ShouldBe("/intro");
    }

    [Fact]
    public void PrefixPathShouldPrependVersionPrefix()
    {
        var result = VersionPathHelper.PrefixPath("/intro", "/v1.0");

        result.ShouldBe("/v1.0/intro");
    }

    [Fact]
    public void PrefixPathShouldInsertVersionAfterLocalePrefix()
    {
        var result = VersionPathHelper.PrefixPath("/intro", "/v1.0", "/fr");

        result.ShouldBe("/fr/v1.0/intro");
    }

    [Fact]
    public void PrefixPathShouldInsertAllSegmentsInOrder()
    {
        var result = VersionPathHelper.PrefixPath("/intro", "/v1.0", "/fr", "/docs");

        result.ShouldBe("/docs/fr/v1.0/intro");
    }

    [Fact]
    public void PrefixPathShouldHandleCurrentVersionWithLocaleAndBasePath()
    {
        var result = VersionPathHelper.PrefixPath("/intro", "", "/fr", "/docs");

        result.ShouldBe("/docs/fr/intro");
    }

    [Fact]
    public void PrefixPathShouldHandleEmptyContentPath()
    {
        var result = VersionPathHelper.PrefixPath("/", "/v1.0", "/fr", "/docs");

        result.ShouldBe("/docs/fr/v1.0/");
    }

    [Fact]
    public void PrefixPathShouldHandleTrailingSlashOnBasePath()
    {
        var result = VersionPathHelper.PrefixPath("/intro", "/v1.0", "/fr", "/docs/");

        result.ShouldBe("/docs/fr/v1.0/intro");
    }

    [Fact]
    public void PrefixPathShouldHandleContentPathWithoutLeadingSlash()
    {
        var result = VersionPathHelper.PrefixPath("intro", "/v1.0", "/fr", "/docs");

        result.ShouldBe("/docs/fr/v1.0/intro");
    }

    [Fact]
    public void PrefixPathShouldReturnSlashForAllEmpty()
    {
        var result = VersionPathHelper.PrefixPath("", "");

        result.ShouldBe("/");
    }

    [Fact]
    public void PrefixPathShouldHandleDeepContentPath()
    {
        var result = VersionPathHelper.PrefixPath("/guides/getting-started", "/v1.0", "/fr", "/docs");

        result.ShouldBe("/docs/fr/v1.0/guides/getting-started");
    }

    // --- StripPrefix ---

    [Fact]
    public void StripPrefixShouldRemoveVersionPrefixFromPath()
    {
        var result = VersionPathHelper.StripPrefix("/v1.0/intro", "/v1.0");

        result.ShouldBe("/intro");
    }

    [Fact]
    public void StripPrefixShouldRemoveVersionAndLocalePrefix()
    {
        var result = VersionPathHelper.StripPrefix("/fr/v1.0/intro", "/v1.0", "/fr");

        result.ShouldBe("/intro");
    }

    [Fact]
    public void StripPrefixShouldRemoveAllSegments()
    {
        var result = VersionPathHelper.StripPrefix("/docs/fr/v1.0/intro", "/v1.0", "/fr", "/docs");

        result.ShouldBe("/intro");
    }

    [Fact]
    public void StripPrefixShouldReturnSlashWhenNothingRemains()
    {
        var result = VersionPathHelper.StripPrefix("/docs/fr/v1.0", "/v1.0", "/fr", "/docs");

        result.ShouldBe("/");
    }

    [Fact]
    public void StripPrefixShouldReturnPathUnchangedWhenNoPrefixMatch()
    {
        var result = VersionPathHelper.StripPrefix("/other/intro", "/v1.0", "/fr", "/docs");

        result.ShouldBe("/other/intro");
    }

    [Fact]
    public void StripPrefixShouldHandleCurrentVersion()
    {
        var result = VersionPathHelper.StripPrefix("/docs/intro", "", "", "/docs");

        result.ShouldBe("/intro");
    }

    [Fact]
    public void StripPrefixShouldHandleNoBasePathOrPrefixes()
    {
        var result = VersionPathHelper.StripPrefix("/intro", "");

        result.ShouldBe("/intro");
    }

    // --- BelongsToVersion ---

    [Fact]
    public void BelongsToVersionShouldReturnTrueForMatchingVersionPrefix()
    {
        var result = VersionPathHelper.BelongsToVersion("/docs/fr/v1.0/intro", "/v1.0", "/fr", "/docs");

        result.ShouldBeTrue();
    }

    [Fact]
    public void BelongsToVersionShouldReturnTrueForExactVersionPrefixMatch()
    {
        var result = VersionPathHelper.BelongsToVersion("/docs/fr/v1.0", "/v1.0", "/fr", "/docs");

        result.ShouldBeTrue();
    }

    [Fact]
    public void BelongsToVersionShouldReturnFalseForDifferentVersion()
    {
        var result = VersionPathHelper.BelongsToVersion("/docs/fr/v2.0/intro", "/v1.0", "/fr", "/docs");

        result.ShouldBeFalse();
    }

    [Fact]
    public void BelongsToVersionShouldReturnTrueForCurrentVersion()
    {
        var result = VersionPathHelper.BelongsToVersion("/docs/intro", "", "", "/docs");

        result.ShouldBeTrue();
    }

    [Fact]
    public void BelongsToVersionShouldReturnTrueForCurrentVersionWithNoBasePath()
    {
        var result = VersionPathHelper.BelongsToVersion("/intro", "");

        result.ShouldBeTrue();
    }

    [Fact]
    public void BelongsToVersionShouldReturnFalseForPartialPrefixMatch()
    {
        // "/v1.0-beta" should NOT match version prefix "/v1.0"
        var result = VersionPathHelper.BelongsToVersion("/v1.0-beta/intro", "/v1.0");

        result.ShouldBeFalse();
    }

    [Fact]
    public void BelongsToVersionShouldBeCaseInsensitive()
    {
        var result = VersionPathHelper.BelongsToVersion("/DOCS/FR/V1.0/intro", "/v1.0", "/fr", "/docs");

        result.ShouldBeTrue();
    }
}
