using Atoll.Lagoon.Versioning;
using Shouldly;
using Xunit;

namespace Atoll.Lagoon.Tests.Versioning;

public sealed class VersionContentResolverTests
{
    // --- GetVersionContentPath ---

    [Fact]
    public void GetVersionContentPathShouldReturnUnchangedPathForCurrentVersion()
    {
        var result = VersionContentResolver.GetVersionContentPath("guides/intro", "current");

        result.ShouldBe("guides/intro");
    }

    [Fact]
    public void GetVersionContentPathShouldReturnUnchangedPathForCurrentVersionCaseInsensitive()
    {
        var result = VersionContentResolver.GetVersionContentPath("guides/intro", "CURRENT");

        result.ShouldBe("guides/intro");
    }

    [Fact]
    public void GetVersionContentPathShouldReturnUnchangedPathForEmptyVersionKey()
    {
        var result = VersionContentResolver.GetVersionContentPath("guides/intro", "");

        result.ShouldBe("guides/intro");
    }

    [Fact]
    public void GetVersionContentPathShouldPrefixWithVersionKey()
    {
        var result = VersionContentResolver.GetVersionContentPath("guides/intro", "v1.0");

        result.ShouldBe("v1.0/guides/intro");
    }

    [Fact]
    public void GetVersionContentPathShouldPrefixWithVersionKeyForNestedPath()
    {
        var result = VersionContentResolver.GetVersionContentPath("guides/advanced/config", "v2.0");

        result.ShouldBe("v2.0/guides/advanced/config");
    }

    [Fact]
    public void GetVersionContentPathShouldHandleEmptyContentPath()
    {
        var result = VersionContentResolver.GetVersionContentPath("", "v1.0");

        result.ShouldBe("v1.0");
    }

    [Fact]
    public void GetVersionContentPathShouldHandleEmptyContentPathForCurrentVersion()
    {
        var result = VersionContentResolver.GetVersionContentPath("", "current");

        result.ShouldBe("");
    }

    [Fact]
    public void GetVersionContentPathShouldStripLeadingSlashFromContentPath()
    {
        var result = VersionContentResolver.GetVersionContentPath("/guides/intro", "v1.0");

        result.ShouldBe("v1.0/guides/intro");
    }

    [Fact]
    public void GetVersionContentPathShouldStripLeadingSlashForCurrentVersion()
    {
        var result = VersionContentResolver.GetVersionContentPath("/guides/intro", "current");

        result.ShouldBe("guides/intro");
    }

    [Fact]
    public void GetVersionContentPathShouldHandleBackslashesInContentPath()
    {
        var result = VersionContentResolver.GetVersionContentPath("guides\\intro", "v1.0");

        result.ShouldBe("v1.0/guides/intro");
    }

    // --- StripVersionFromSlug ---

    [Fact]
    public void StripVersionFromSlugShouldReturnSlugUnchangedForCurrentVersion()
    {
        var result = VersionContentResolver.StripVersionFromSlug("guides/intro", "current");

        result.ShouldBe("guides/intro");
    }

    [Fact]
    public void StripVersionFromSlugShouldReturnSlugUnchangedForEmptyVersionKey()
    {
        var result = VersionContentResolver.StripVersionFromSlug("guides/intro", "");

        result.ShouldBe("guides/intro");
    }

    [Fact]
    public void StripVersionFromSlugShouldStripVersionPrefixFromSlug()
    {
        var result = VersionContentResolver.StripVersionFromSlug("v1.0/guides/intro", "v1.0");

        result.ShouldBe("guides/intro");
    }

    [Fact]
    public void StripVersionFromSlugShouldStripVersionPrefixCaseInsensitively()
    {
        var result = VersionContentResolver.StripVersionFromSlug("V1.0/guides/intro", "v1.0");

        result.ShouldBe("guides/intro");
    }

    [Fact]
    public void StripVersionFromSlugShouldReturnEmptyWhenSlugIsJustVersionKey()
    {
        var result = VersionContentResolver.StripVersionFromSlug("v1.0", "v1.0");

        result.ShouldBe("");
    }

    [Fact]
    public void StripVersionFromSlugShouldReturnSlugUnchangedWhenNoMatch()
    {
        var result = VersionContentResolver.StripVersionFromSlug("guides/intro", "v1.0");

        result.ShouldBe("guides/intro");
    }

    [Fact]
    public void StripVersionFromSlugShouldHandleLeadingSlash()
    {
        var result = VersionContentResolver.StripVersionFromSlug("/v1.0/guides/intro", "v1.0");

        result.ShouldBe("guides/intro");
    }

    [Fact]
    public void StripVersionFromSlugShouldNotStripPartialMatch()
    {
        // "v1.0-beta" starts with "v1.0" but should not be stripped
        var result = VersionContentResolver.StripVersionFromSlug("v1.0-beta/guides/intro", "v1.0");

        result.ShouldBe("v1.0-beta/guides/intro");
    }

    [Fact]
    public void StripVersionFromSlugShouldHandleVersionKeyWithDots()
    {
        var result = VersionContentResolver.StripVersionFromSlug("v2.1.0/guides/intro", "v2.1.0");

        result.ShouldBe("guides/intro");
    }
}
