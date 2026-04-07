using Atoll.Lagoon.Versioning;

namespace Atoll.Lagoon.Tests.Versioning;

public sealed class VersionResolverTests
{
    // --- Single-version mode (null / empty versions) ---

    [Fact]
    public void ShouldReturnNullWhenVersionsIsNull()
    {
        var result = VersionResolver.Resolve("/intro", null, "");

        result.ShouldBeNull();
    }

    [Fact]
    public void ShouldReturnNullWhenVersionsIsEmpty()
    {
        var result = VersionResolver.Resolve("/intro", new Dictionary<string, VersionConfig>(), "");

        result.ShouldBeNull();
    }

    // --- Current version ---

    [Fact]
    public void ShouldMatchCurrentVersionForUnprefixedPath()
    {
        var versions = new Dictionary<string, VersionConfig>
        {
            ["current"] = new() { Label = "Latest", Slug = "current" },
            ["v1.0"] = new() { Label = "v1.0", Slug = "v1.0" },
        };

        var result = VersionResolver.Resolve("/intro", versions, "");

        result.ShouldNotBeNull();
        result.Key.ShouldBe("current");
        result.PathPrefix.ShouldBe("");
        result.ContentPath.ShouldBe("/intro");
    }

    [Fact]
    public void ShouldMatchCurrentVersionForRootPath()
    {
        var versions = new Dictionary<string, VersionConfig>
        {
            ["current"] = new() { Label = "Latest", Slug = "current" },
        };

        var result = VersionResolver.Resolve("/", versions, "");

        result.ShouldNotBeNull();
        result.Key.ShouldBe("current");
        result.ContentPath.ShouldBe("/");
    }

    // --- Prefixed version ---

    [Fact]
    public void ShouldMatchPrefixedVersion()
    {
        var versions = new Dictionary<string, VersionConfig>
        {
            ["current"] = new() { Label = "Latest", Slug = "current" },
            ["v1.0"] = new() { Label = "v1.0", Slug = "v1.0" },
        };

        var result = VersionResolver.Resolve("/v1.0/intro", versions, "");

        result.ShouldNotBeNull();
        result.Key.ShouldBe("v1.0");
        result.PathPrefix.ShouldBe("/v1.0");
        result.ContentPath.ShouldBe("/intro");
    }

    [Fact]
    public void ShouldMatchPrefixedVersionExactPath()
    {
        var versions = new Dictionary<string, VersionConfig>
        {
            ["current"] = new() { Label = "Latest", Slug = "current" },
            ["v1.0"] = new() { Label = "v1.0", Slug = "v1.0" },
        };

        var result = VersionResolver.Resolve("/v1.0", versions, "");

        result.ShouldNotBeNull();
        result.Key.ShouldBe("v1.0");
        result.ContentPath.ShouldBe("/");
    }

    [Fact]
    public void ShouldMatchCorrectVersionFromMultiple()
    {
        var versions = new Dictionary<string, VersionConfig>
        {
            ["current"] = new() { Label = "Latest", Slug = "current" },
            ["v2.0"] = new() { Label = "v2.0", Slug = "v2.0" },
            ["v1.0"] = new() { Label = "v1.0", Slug = "v1.0" },
        };

        var result = VersionResolver.Resolve("/v1.0/guides/setup", versions, "");

        result.ShouldNotBeNull();
        result.Key.ShouldBe("v1.0");
        result.ContentPath.ShouldBe("/guides/setup");
    }

    // --- Fallback when no current key ---

    [Fact]
    public void ShouldFallbackToFirstVersionWhenNoCurrentAndNoMatch()
    {
        var versions = new Dictionary<string, VersionConfig>
        {
            ["v2.0"] = new() { Label = "v2.0", Slug = "v2.0" },
            ["v1.0"] = new() { Label = "v1.0", Slug = "v1.0" },
        };

        var result = VersionResolver.Resolve("/unknown/page", versions, "");

        result.ShouldNotBeNull();
        result.Key.ShouldBe("v2.0");
    }

    // --- BasePath handling ---

    [Fact]
    public void ShouldStripBasePathBeforeMatching()
    {
        var versions = new Dictionary<string, VersionConfig>
        {
            ["current"] = new() { Label = "Latest", Slug = "current" },
            ["v1.0"] = new() { Label = "v1.0", Slug = "v1.0" },
        };

        var result = VersionResolver.Resolve("/docs/v1.0/intro", versions, "/docs");

        result.ShouldNotBeNull();
        result.Key.ShouldBe("v1.0");
        result.ContentPath.ShouldBe("/intro");
    }

    [Fact]
    public void ShouldMatchCurrentVersionWithBasePath()
    {
        var versions = new Dictionary<string, VersionConfig>
        {
            ["current"] = new() { Label = "Latest", Slug = "current" },
            ["v1.0"] = new() { Label = "v1.0", Slug = "v1.0" },
        };

        var result = VersionResolver.Resolve("/docs/intro", versions, "/docs");

        result.ShouldNotBeNull();
        result.Key.ShouldBe("current");
        result.ContentPath.ShouldBe("/intro");
    }

    // --- Case-insensitive matching ---

    [Fact]
    public void ShouldMatchVersionKeysCaseInsensitively()
    {
        var versions = new Dictionary<string, VersionConfig>
        {
            ["current"] = new() { Label = "Latest", Slug = "current" },
            ["v1.0"] = new() { Label = "v1.0", Slug = "v1.0" },
        };

        var result = VersionResolver.Resolve("/V1.0/intro", versions, "");

        result.ShouldNotBeNull();
        result.Key.ShouldBe("v1.0");
        result.ContentPath.ShouldBe("/intro");
    }

    // --- Overload without basePath ---

    [Fact]
    public void ShouldWorkWithOverloadWithoutBasePath()
    {
        var versions = new Dictionary<string, VersionConfig>
        {
            ["current"] = new() { Label = "Latest", Slug = "current" },
            ["v1.0"] = new() { Label = "v1.0", Slug = "v1.0" },
        };

        var result = VersionResolver.Resolve("/v1.0/intro", versions);

        result.ShouldNotBeNull();
        result.Key.ShouldBe("v1.0");
        result.ContentPath.ShouldBe("/intro");
    }

    // --- Empty remaining path normalizes to "/" ---

    [Fact]
    public void ShouldNormalizeEmptyContentPathToSlash()
    {
        var versions = new Dictionary<string, VersionConfig>
        {
            ["current"] = new() { Label = "Latest", Slug = "current" },
            ["v1.0"] = new() { Label = "v1.0", Slug = "v1.0" },
        };

        var result = VersionResolver.Resolve("/docs/v1.0", versions, "/docs");

        result.ShouldNotBeNull();
        result.Key.ShouldBe("v1.0");
        result.ContentPath.ShouldBe("/");
    }

    // --- IsDeprecated config is preserved ---

    [Fact]
    public void ShouldPreserveVersionConfigProperties()
    {
        var versions = new Dictionary<string, VersionConfig>
        {
            ["current"] = new() { Label = "Latest", Slug = "current" },
            ["v1.0"] = new() { Label = "v1.0", Slug = "v1.0", IsDeprecated = true, DeprecationMessage = "Please upgrade" },
        };

        var result = VersionResolver.Resolve("/v1.0/intro", versions, "");

        result.ShouldNotBeNull();
        result.Config.IsDeprecated.ShouldBeTrue();
        result.Config.DeprecationMessage.ShouldBe("Please upgrade");
    }
}
