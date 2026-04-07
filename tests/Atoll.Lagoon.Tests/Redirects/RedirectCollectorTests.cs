using Atoll.Lagoon.Redirects;
using Shouldly;
using Xunit;

namespace Atoll.Lagoon.Tests.Redirects;

public sealed class RedirectCollectorTests
{
    [Fact]
    public void CollectShouldReturnEmptyMapWhenNoRedirects()
    {
        var collector = new RedirectCollector();

        var map = collector.Collect([]);

        map.Count.ShouldBe(0);
    }

    [Fact]
    public void CollectShouldHandleNullConfigRedirects()
    {
        var collector = new RedirectCollector(null);
        var sources = new[]
        {
            new RedirectSource("/docs/new-page", ["/docs/old-page"]),
        };

        var map = collector.Collect(sources);

        map.Count.ShouldBe(1);
        map.TryGetRedirect("/docs/old-page", out var target).ShouldBeTrue();
        target.ShouldBe("/docs/new-page");
    }

    [Fact]
    public void CollectShouldMergeConfigAndFrontmatterRedirects()
    {
        var configRedirects = new Dictionary<string, string>
        {
            ["/config-old"] = "/config-new",
        };
        var collector = new RedirectCollector(configRedirects);
        var sources = new[]
        {
            new RedirectSource("/frontmatter-new", ["/frontmatter-old"]),
        };

        var map = collector.Collect(sources);

        map.Count.ShouldBe(2);
        map.TryGetRedirect("/config-old", out var configTarget).ShouldBeTrue();
        configTarget.ShouldBe("/config-new");
        map.TryGetRedirect("/frontmatter-old", out var fmTarget).ShouldBeTrue();
        fmTarget.ShouldBe("/frontmatter-new");
    }

    [Fact]
    public void CollectShouldHandleEmptyRedirectFromLists()
    {
        var collector = new RedirectCollector();
        var sources = new[]
        {
            new RedirectSource("/page", []),
        };

        var map = collector.Collect(sources);

        map.Count.ShouldBe(0);
    }

    [Fact]
    public void CollectShouldThrowOnConflictBetweenFrontmatterEntries()
    {
        var collector = new RedirectCollector();
        var sources = new[]
        {
            new RedirectSource("/page-a", ["/shared-old"]),
            new RedirectSource("/page-b", ["/shared-old"]),
        };

        Should.Throw<RedirectConflictException>(() => collector.Collect(sources))
            .Message.ShouldContain("/shared-old");
    }

    [Fact]
    public void CollectShouldThrowOnConflictBetweenConfigAndFrontmatter()
    {
        var configRedirects = new Dictionary<string, string>
        {
            ["/shared-old"] = "/config-target",
        };
        var collector = new RedirectCollector(configRedirects);
        var sources = new[]
        {
            new RedirectSource("/frontmatter-page", ["/shared-old"]),
        };

        Should.Throw<RedirectConflictException>(() => collector.Collect(sources))
            .Message.ShouldContain("/shared-old");
    }

    [Fact]
    public void CollectShouldThrowWhenSourcePathMatchesCanonicalUrl()
    {
        var collector = new RedirectCollector();
        var sources = new[]
        {
            new RedirectSource("/page-a", ["/page-b"]),
            new RedirectSource("/page-b", []),  // /page-b is a real page
        };

        Should.Throw<RedirectConflictException>(() => collector.Collect(sources))
            .Message.ShouldContain("/page-b");
    }

    [Fact]
    public void CollectShouldThrowWhenConfigSourceMatchesCanonicalUrl()
    {
        var configRedirects = new Dictionary<string, string>
        {
            ["/existing-page"] = "/somewhere",
        };
        var collector = new RedirectCollector(configRedirects);
        var sources = new[]
        {
            new RedirectSource("/existing-page", []),
        };

        Should.Throw<RedirectConflictException>(() => collector.Collect(sources))
            .Message.ShouldContain("/existing-page");
    }

    [Fact]
    public void CollectShouldNormalizePathsBeforeConflictDetection()
    {
        var collector = new RedirectCollector();
        var sources = new[]
        {
            new RedirectSource("/page-a", ["/Shared-OLD/"]),
            new RedirectSource("/page-b", ["/shared-old"]),
        };

        // Both frontmatter entries normalize to the same source → conflict
        Should.Throw<RedirectConflictException>(() => collector.Collect(sources));
    }

    [Fact]
    public void CollectShouldSupportMultipleRedirectFromPerEntry()
    {
        var collector = new RedirectCollector();
        var sources = new[]
        {
            new RedirectSource("/new-page", ["/old-v1", "/old-v2", "/legacy"]),
        };

        var map = collector.Collect(sources);

        map.Count.ShouldBe(3);
        map.TryGetRedirect("/old-v1", out var t1).ShouldBeTrue();
        t1.ShouldBe("/new-page");
        map.TryGetRedirect("/old-v2", out var t2).ShouldBeTrue();
        t2.ShouldBe("/new-page");
        map.TryGetRedirect("/legacy", out var t3).ShouldBeTrue();
        t3.ShouldBe("/new-page");
    }
}
