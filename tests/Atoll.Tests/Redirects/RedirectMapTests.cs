using Atoll.Redirects;
using Shouldly;
using Xunit;

namespace Atoll.Tests.Redirects;

public sealed class RedirectMapTests
{
    [Fact]
    public void CreateWithEntriesShouldBuildMap()
    {
        var map = RedirectMap.Create(new Dictionary<string, string>
        {
            ["/old"] = "/new",
            ["/legacy"] = "/current",
        });

        map.Count.ShouldBe(2);
        map.Entries.ShouldContainKey("/old");
        map.Entries.ShouldContainKey("/legacy");
    }

    [Fact]
    public void TryGetRedirectShouldReturnTrueForMatchedPath()
    {
        var map = RedirectMap.Create(new Dictionary<string, string>
        {
            ["/old-page"] = "/new-page",
        });

        var result = map.TryGetRedirect("/old-page", out var target);

        result.ShouldBeTrue();
        target.ShouldBe("/new-page");
    }

    [Fact]
    public void TryGetRedirectShouldReturnFalseForUnmatchedPath()
    {
        var map = RedirectMap.Create(new Dictionary<string, string>
        {
            ["/old-page"] = "/new-page",
        });

        var result = map.TryGetRedirect("/does-not-exist", out var target);

        result.ShouldBeFalse();
        target.ShouldBeNull();
    }

    [Fact]
    public void CreateShouldNormalizeLeadingSlash()
    {
        var map = RedirectMap.Create(new Dictionary<string, string>
        {
            ["no-leading-slash"] = "no-target-slash",
        });

        map.TryGetRedirect("/no-leading-slash", out var target).ShouldBeTrue();
        target.ShouldBe("/no-target-slash");
    }

    [Fact]
    public void CreateShouldStripTrailingSlash()
    {
        var map = RedirectMap.Create(new Dictionary<string, string>
        {
            ["/old-page/"] = "/new-page/",
        });

        map.TryGetRedirect("/old-page", out var target).ShouldBeTrue();
        target.ShouldBe("/new-page");
    }

    [Fact]
    public void CreateShouldNormalizePathsToLowercase()
    {
        var map = RedirectMap.Create(new Dictionary<string, string>
        {
            ["/Old-Page"] = "/New-Page",
        });

        map.TryGetRedirect("/old-page", out var target).ShouldBeTrue();
        target.ShouldBe("/new-page");
    }

    [Fact]
    public void CreateShouldThrowOnDuplicateSourcePaths()
    {
        var entries = new[]
        {
            new KeyValuePair<string, string>("/old", "/new1"),
            new KeyValuePair<string, string>("/old", "/new2"),
        };

        Should.Throw<InvalidOperationException>(() => RedirectMap.Create(entries))
            .Message.ShouldContain("/old");
    }

    [Fact]
    public void CreateShouldThrowOnDuplicateSourcePathsDifferingOnlyInCase()
    {
        var entries = new[]
        {
            new KeyValuePair<string, string>("/old", "/new1"),
            new KeyValuePair<string, string>("/OLD", "/new2"),
        };

        Should.Throw<InvalidOperationException>(() => RedirectMap.Create(entries));
    }

    [Fact]
    public void EmptyMapShouldHaveZeroCount()
    {
        var map = RedirectMap.Create(Enumerable.Empty<KeyValuePair<string, string>>());

        map.Count.ShouldBe(0);
        map.Entries.ShouldBeEmpty();
    }

    [Fact]
    public void TryGetRedirectShouldMatchCaseInsensitively()
    {
        var map = RedirectMap.Create(new Dictionary<string, string>
        {
            ["/old-page"] = "/new-page",
        });

        map.TryGetRedirect("/OLD-PAGE", out var target).ShouldBeTrue();
        target.ShouldBe("/new-page");
    }

    [Fact]
    public void TryGetRedirectShouldMatchWithTrailingSlashInLookup()
    {
        var map = RedirectMap.Create(new Dictionary<string, string>
        {
            ["/old-page"] = "/new-page",
        });

        map.TryGetRedirect("/old-page/", out var target).ShouldBeTrue();
        target.ShouldBe("/new-page");
    }

    [Fact]
    public void EmptyStaticPropertyShouldHaveZeroCount()
    {
        RedirectMap.Empty.Count.ShouldBe(0);
        RedirectMap.Empty.Entries.ShouldBeEmpty();
    }

    [Fact]
    public void NormalizePathShouldHandleRootPath()
    {
        var result = RedirectMap.NormalizePath("/");

        // Root path "/" should remain "/" (not become "")
        result.ShouldBe("/");
    }

    [Fact]
    public void NormalizePathShouldTrimWhitespace()
    {
        var result = RedirectMap.NormalizePath("  /old-page  ");

        result.ShouldBe("/old-page");
    }
}
