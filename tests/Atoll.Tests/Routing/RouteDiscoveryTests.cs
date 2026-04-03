using Atoll.Components;
using Atoll.Routing.FileSystem;
using Shouldly;
using Xunit;

namespace Atoll.Routing.Tests;

public sealed class RouteDiscoveryTests : IDisposable
{
    private readonly string _tempDir;

    public RouteDiscoveryTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "atoll-route-tests-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

    // --- DiscoverRoutesFromEntries tests ---

    [Fact]
    public void ShouldDiscoverRoutesFromExplicitEntries()
    {
        var entries = new List<(string RelativeFilePath, Type ComponentType)>
        {
            ("index.cs", typeof(StubIndexPage)),
            ("about.cs", typeof(StubAboutPage)),
            ("blog/[slug].cs", typeof(StubBlogPostPage)),
        };

        var routes = RouteDiscovery.DiscoverRoutesFromEntries(entries);

        routes.Count.ShouldBe(3);
        routes[0].Pattern.ShouldBe("/");
        routes[0].ComponentType.ShouldBe(typeof(StubIndexPage));
        routes[0].RelativeFilePath.ShouldBe("index.cs");
        routes[1].Pattern.ShouldBe("/about");
        routes[1].ComponentType.ShouldBe(typeof(StubAboutPage));
        routes[2].Pattern.ShouldBe("/blog/[slug]");
        routes[2].ComponentType.ShouldBe(typeof(StubBlogPostPage));
    }

    [Fact]
    public void ShouldDiscoverCatchAllRouteFromEntries()
    {
        var entries = new List<(string RelativeFilePath, Type ComponentType)>
        {
            ("docs/[...rest].cs", typeof(StubDocsPage)),
        };

        var routes = RouteDiscovery.DiscoverRoutesFromEntries(entries);

        routes.Count.ShouldBe(1);
        routes[0].Pattern.ShouldBe("/docs/[...rest]");
        routes[0].ComponentType.ShouldBe(typeof(StubDocsPage));
    }

    [Fact]
    public void ShouldDiscoverNestedIndexRouteFromEntries()
    {
        var entries = new List<(string RelativeFilePath, Type ComponentType)>
        {
            ("blog/index.cs", typeof(StubBlogIndexPage)),
        };

        var routes = RouteDiscovery.DiscoverRoutesFromEntries(entries);

        routes.Count.ShouldBe(1);
        routes[0].Pattern.ShouldBe("/blog");
    }

    [Fact]
    public void ShouldReturnEmptyForEmptyEntries()
    {
        var routes = RouteDiscovery.DiscoverRoutesFromEntries([]);
        routes.ShouldBeEmpty();
    }

    [Fact]
    public void ShouldThrowForNullEntries()
    {
        Should.Throw<ArgumentNullException>(
            () => RouteDiscovery.DiscoverRoutesFromEntries(null!));
    }

    // --- DiscoverRoutes with type map + filesystem integration tests ---

    [Fact]
    public void ShouldDiscoverRoutesFromFileSystem()
    {
        CreatePagesFile("index.cs");
        CreatePagesFile("about.cs");
        CreatePagesFile(Path.Combine("blog", "[slug].cs"));

        var typeMap = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            ["index"] = typeof(StubIndexPage),
            ["about"] = typeof(StubAboutPage),
            ["[slug]"] = typeof(StubBlogPostPage),
        };

        var discovery = new RouteDiscovery(_tempDir);
        var routes = discovery.DiscoverRoutes(typeMap);

        routes.Count.ShouldBe(3);

        var patterns = routes.Select(r => r.Pattern).OrderBy(p => p).ToList();
        patterns.ShouldContain("/");
        patterns.ShouldContain("/about");
        patterns.ShouldContain("/blog/[slug]");
    }

    [Fact]
    public void ShouldSkipUnderscoreFiles()
    {
        CreatePagesFile("index.cs");
        CreatePagesFile("_Layout.cs");

        var typeMap = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            ["index"] = typeof(StubIndexPage),
            ["_Layout"] = typeof(StubLayoutComponent),
        };

        var discovery = new RouteDiscovery(_tempDir);
        var routes = discovery.DiscoverRoutes(typeMap);

        routes.Count.ShouldBe(1);
        routes[0].Pattern.ShouldBe("/");
    }

    [Fact]
    public void ShouldReturnEmptyWhenDirectoryDoesNotExist()
    {
        var discovery = new RouteDiscovery(Path.Combine(_tempDir, "nonexistent"));
        var typeMap = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        var routes = discovery.DiscoverRoutes(typeMap);

        routes.ShouldBeEmpty();
    }

    [Fact]
    public void ShouldSkipFilesWithNoMatchingType()
    {
        CreatePagesFile("index.cs");
        CreatePagesFile("orphan.cs");

        var typeMap = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            ["index"] = typeof(StubIndexPage),
            // "orphan" is NOT in the type map
        };

        var discovery = new RouteDiscovery(_tempDir);
        var routes = discovery.DiscoverRoutes(typeMap);

        routes.Count.ShouldBe(1);
        routes[0].Pattern.ShouldBe("/");
    }

    [Fact]
    public void ShouldDiscoverDeeplyNestedRoutes()
    {
        CreatePagesFile(Path.Combine("a", "b", "c", "page.cs"));

        var typeMap = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            ["page"] = typeof(StubIndexPage),
        };

        var discovery = new RouteDiscovery(_tempDir);
        var routes = discovery.DiscoverRoutes(typeMap);

        routes.Count.ShouldBe(1);
        routes[0].Pattern.ShouldBe("/a/b/c/page");
    }

    [Fact]
    public void ShouldDiscoverCatchAllFromFileSystem()
    {
        CreatePagesFile(Path.Combine("docs", "[...rest].cs"));

        var typeMap = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            ["[...rest]"] = typeof(StubDocsPage),
        };

        var discovery = new RouteDiscovery(_tempDir);
        var routes = discovery.DiscoverRoutes(typeMap);

        routes.Count.ShouldBe(1);
        routes[0].Pattern.ShouldBe("/docs/[...rest]");
    }

    [Fact]
    public void ShouldSetCorrectRelativeFilePath()
    {
        CreatePagesFile(Path.Combine("blog", "post.cs"));

        var typeMap = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            ["post"] = typeof(StubBlogPostPage),
        };

        var discovery = new RouteDiscovery(_tempDir);
        var routes = discovery.DiscoverRoutes(typeMap);

        routes.Count.ShouldBe(1);
        routes[0].RelativeFilePath.ShouldBe("blog/post.cs");
    }

    [Fact]
    public void ShouldThrowForNullPagesDirectory()
    {
        Should.Throw<ArgumentNullException>(() => new RouteDiscovery(null!));
    }

    [Fact]
    public void ShouldThrowForNullTypeMap()
    {
        var discovery = new RouteDiscovery(_tempDir);
        Should.Throw<ArgumentNullException>(() => discovery.DiscoverRoutes((IReadOnlyDictionary<string, Type>)null!));
    }

    // --- RouteEntry tests ---

    [Fact]
    public void ShouldCreateRouteEntryWithPrerender()
    {
        var entry = new RouteEntry("/about", typeof(StubAboutPage), "about.cs", true);

        entry.Pattern.ShouldBe("/about");
        entry.ComponentType.ShouldBe(typeof(StubAboutPage));
        entry.RelativeFilePath.ShouldBe("about.cs");
        entry.Prerender.ShouldBeTrue();
    }

    [Fact]
    public void ShouldCreateRouteEntryWithDefaultPrerender()
    {
        var entry = new RouteEntry("/about", typeof(StubAboutPage), "about.cs");

        entry.Prerender.ShouldBeFalse();
    }

    [Fact]
    public void ShouldThrowForNullRouteEntryPattern()
    {
        Should.Throw<ArgumentNullException>(
            () => new RouteEntry(null!, typeof(StubAboutPage), "about.cs"));
    }

    [Fact]
    public void ShouldThrowForNullRouteEntryComponentType()
    {
        Should.Throw<ArgumentNullException>(
            () => new RouteEntry("/about", null!, "about.cs"));
    }

    [Fact]
    public void ShouldThrowForNullRouteEntryRelativeFilePath()
    {
        Should.Throw<ArgumentNullException>(
            () => new RouteEntry("/about", typeof(StubAboutPage), null!));
    }

    // --- Helper methods ---

    private void CreatePagesFile(string relativePath)
    {
        var fullPath = Path.Combine(_tempDir, relativePath);
        var directory = Path.GetDirectoryName(fullPath)!;
        Directory.CreateDirectory(directory);
        File.WriteAllText(fullPath, "// placeholder");
    }

    // --- Stub types for tests ---

    private sealed class StubIndexPage : IAtollComponent
    {
        public Task RenderAsync(RenderContext context) => Task.CompletedTask;
    }

    private sealed class StubAboutPage : IAtollComponent
    {
        public Task RenderAsync(RenderContext context) => Task.CompletedTask;
    }

    private sealed class StubBlogPostPage : IAtollComponent
    {
        public Task RenderAsync(RenderContext context) => Task.CompletedTask;
    }

    private sealed class StubBlogIndexPage : IAtollComponent
    {
        public Task RenderAsync(RenderContext context) => Task.CompletedTask;
    }

    private sealed class StubDocsPage : IAtollComponent
    {
        public Task RenderAsync(RenderContext context) => Task.CompletedTask;
    }

    private sealed class StubLayoutComponent : IAtollComponent
    {
        public Task RenderAsync(RenderContext context) => Task.CompletedTask;
    }
}
