using Atoll.Core.Components;
using Atoll.Routing.FileSystem;
using Atoll.Routing.Matching;
using Shouldly;
using Xunit;

namespace Atoll.Routing.Tests;

public sealed class PageTypeTests
{
    // --- IAtollPage marker interface tests ---

    [Fact]
    public void PageShouldImplementIAtollComponent()
    {
        var page = new StubStaticPage();
        (page is IAtollComponent).ShouldBeTrue();
        (page is IAtollPage).ShouldBeTrue();
    }

    [Fact]
    public async Task PageShouldBeRenderableViaIAtollComponent()
    {
        IAtollComponent component = new StubStaticPage();
        var destination = new Atoll.Core.Rendering.StringRenderDestination();
        var context = new RenderContext(destination);
        await component.RenderAsync(context);

        destination.GetOutput().ShouldBe("<h1>About</h1>");
    }

    // --- IStaticPathsProvider tests ---

    [Fact]
    public async Task StaticPathsProviderShouldReturnPaths()
    {
        IStaticPathsProvider provider = new StubDynamicPage();
        var paths = await provider.GetStaticPathsAsync();

        paths.Count.ShouldBe(3);
        paths[0].Parameters["slug"].ShouldBe("hello-world");
        paths[1].Parameters["slug"].ShouldBe("getting-started");
        paths[2].Parameters["slug"].ShouldBe("advanced-tips");
    }

    [Fact]
    public async Task StaticPathsProviderShouldReturnPathsWithProps()
    {
        IStaticPathsProvider provider = new StubDynamicPageWithProps();
        var paths = await provider.GetStaticPathsAsync();

        paths.Count.ShouldBe(2);

        paths[0].Parameters["slug"].ShouldBe("first-post");
        paths[0].Props["Title"].ShouldBe("First Post");
        paths[0].Props["Draft"].ShouldBe(false);

        paths[1].Parameters["slug"].ShouldBe("second-post");
        paths[1].Props["Title"].ShouldBe("Second Post");
        paths[1].Props["Draft"].ShouldBe(true);
    }

    [Fact]
    public async Task StaticPathsProviderShouldReturnEmptyForNoContent()
    {
        IStaticPathsProvider provider = new StubEmptyPathsPage();
        var paths = await provider.GetStaticPathsAsync();

        paths.ShouldBeEmpty();
    }

    // --- StaticPath tests ---

    [Fact]
    public void StaticPathShouldStoreParameters()
    {
        var parameters = new Dictionary<string, string> { ["slug"] = "my-post" };
        var path = new StaticPath(parameters);

        path.Parameters["slug"].ShouldBe("my-post");
        path.Props.ShouldBeEmpty();
    }

    [Fact]
    public void StaticPathShouldStoreParametersAndProps()
    {
        var parameters = new Dictionary<string, string> { ["slug"] = "my-post" };
        var props = new Dictionary<string, object?> { ["Title"] = "My Post", ["Count"] = 42 };
        var path = new StaticPath(parameters, props);

        path.Parameters["slug"].ShouldBe("my-post");
        path.Props["Title"].ShouldBe("My Post");
        path.Props["Count"].ShouldBe(42);
    }

    [Fact]
    public void StaticPathShouldSupportMultipleParameters()
    {
        var parameters = new Dictionary<string, string>
        {
            ["category"] = "tech",
            ["slug"] = "my-post"
        };
        var path = new StaticPath(parameters);

        path.Parameters["category"].ShouldBe("tech");
        path.Parameters["slug"].ShouldBe("my-post");
    }

    [Fact]
    public void StaticPathShouldSupportNullPropValues()
    {
        var parameters = new Dictionary<string, string> { ["slug"] = "test" };
        var props = new Dictionary<string, object?> { ["Subtitle"] = null };
        var path = new StaticPath(parameters, props);

        path.Props.ContainsKey("Subtitle").ShouldBeTrue();
        path.Props["Subtitle"].ShouldBeNull();
    }

    [Fact]
    public void StaticPathShouldThrowForNullParameters()
    {
        Should.Throw<ArgumentNullException>(() => new StaticPath(null!));
    }

    [Fact]
    public void StaticPathShouldThrowForNullParametersWithProps()
    {
        var props = new Dictionary<string, object?>();
        Should.Throw<ArgumentNullException>(() => new StaticPath(null!, props));
    }

    [Fact]
    public void StaticPathShouldThrowForNullProps()
    {
        var parameters = new Dictionary<string, string>();
        Should.Throw<ArgumentNullException>(() => new StaticPath(parameters, null!));
    }

    // --- Integration: Route discovery with IAtollPage types ---

    [Fact]
    public void RouteDiscoveryShouldFindPageTypes()
    {
        var entries = new List<(string RelativeFilePath, Type ComponentType)>
        {
            ("index.cs", typeof(StubStaticPage)),
            ("blog/[slug].cs", typeof(StubDynamicPage)),
        };

        var routes = RouteDiscovery.DiscoverRoutesFromEntries(entries);

        routes.Count.ShouldBe(2);
        routes[0].Pattern.ShouldBe("/");
        routes[0].ComponentType.ShouldBe(typeof(StubStaticPage));
        routes[1].Pattern.ShouldBe("/blog/[slug]");
        routes[1].ComponentType.ShouldBe(typeof(StubDynamicPage));
    }

    // --- Integration: Route matching with page that provides static paths ---

    [Fact]
    public async Task RouteMatchShouldProvideParamsMatchingStaticPaths()
    {
        var entries = new List<(string RelativeFilePath, Type ComponentType)>
        {
            ("blog/[slug].cs", typeof(StubDynamicPage)),
        };

        var routes = RouteDiscovery.DiscoverRoutesFromEntries(entries);
        var matcher = new RouteMatcher(routes);

        // Get static paths from the page
        var page = new StubDynamicPage();
        var staticPaths = await page.GetStaticPathsAsync();

        // Each static path's parameters should be matchable by the route matcher
        foreach (var staticPath in staticPaths)
        {
            var url = "/blog/" + staticPath.Parameters["slug"];
            var match = matcher.Match(url);

            match.ShouldNotBeNull();
            match.Parameters["slug"].ShouldBe(staticPath.Parameters["slug"]);
            match.RouteEntry.ComponentType.ShouldBe(typeof(StubDynamicPage));
        }
    }

    // --- Integration: Catch-all route with static paths ---

    [Fact]
    public async Task CatchAllPageShouldProvideStaticPaths()
    {
        IStaticPathsProvider provider = new StubCatchAllPage();
        var paths = await provider.GetStaticPathsAsync();

        paths.Count.ShouldBe(2);
        paths[0].Parameters["rest"].ShouldBe("guide/getting-started");
        paths[1].Parameters["rest"].ShouldBe("api/core/rendering");
    }

    [Fact]
    public async Task CatchAllRouteMatchShouldAlignWithStaticPaths()
    {
        var entries = new List<(string RelativeFilePath, Type ComponentType)>
        {
            ("docs/[...rest].cs", typeof(StubCatchAllPage)),
        };

        var routes = RouteDiscovery.DiscoverRoutesFromEntries(entries);
        var matcher = new RouteMatcher(routes);

        var page = new StubCatchAllPage();
        var staticPaths = await page.GetStaticPathsAsync();

        foreach (var staticPath in staticPaths)
        {
            var url = "/docs/" + staticPath.Parameters["rest"];
            var match = matcher.Match(url);

            match.ShouldNotBeNull();
            match.Parameters["rest"].ShouldBe(staticPath.Parameters["rest"]);
        }
    }

    // --- Stub page types ---

    private sealed class StubStaticPage : IAtollPage
    {
        public Task RenderAsync(RenderContext context)
        {
            context.WriteHtml("<h1>About</h1>");
            return Task.CompletedTask;
        }
    }

    private sealed class StubDynamicPage : IAtollPage, IStaticPathsProvider
    {
        [Parameter(Required = true)]
        public string Slug { get; set; } = "";

        public Task RenderAsync(RenderContext context)
        {
            context.WriteHtml("<h1>Blog: " + Slug + "</h1>");
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<StaticPath>> GetStaticPathsAsync()
        {
            IReadOnlyList<StaticPath> paths = new List<StaticPath>
            {
                new(new Dictionary<string, string> { ["slug"] = "hello-world" }),
                new(new Dictionary<string, string> { ["slug"] = "getting-started" }),
                new(new Dictionary<string, string> { ["slug"] = "advanced-tips" }),
            };
            return Task.FromResult(paths);
        }
    }

    private sealed class StubDynamicPageWithProps : IAtollPage, IStaticPathsProvider
    {
        public Task RenderAsync(RenderContext context)
        {
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<StaticPath>> GetStaticPathsAsync()
        {
            IReadOnlyList<StaticPath> paths = new List<StaticPath>
            {
                new(
                    new Dictionary<string, string> { ["slug"] = "first-post" },
                    new Dictionary<string, object?> { ["Title"] = "First Post", ["Draft"] = false }),
                new(
                    new Dictionary<string, string> { ["slug"] = "second-post" },
                    new Dictionary<string, object?> { ["Title"] = "Second Post", ["Draft"] = true }),
            };
            return Task.FromResult(paths);
        }
    }

    private sealed class StubEmptyPathsPage : IAtollPage, IStaticPathsProvider
    {
        public Task RenderAsync(RenderContext context)
        {
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<StaticPath>> GetStaticPathsAsync()
        {
            IReadOnlyList<StaticPath> paths = new List<StaticPath>();
            return Task.FromResult(paths);
        }
    }

    private sealed class StubCatchAllPage : IAtollPage, IStaticPathsProvider
    {
        public Task RenderAsync(RenderContext context)
        {
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<StaticPath>> GetStaticPathsAsync()
        {
            IReadOnlyList<StaticPath> paths = new List<StaticPath>
            {
                new(new Dictionary<string, string> { ["rest"] = "guide/getting-started" }),
                new(new Dictionary<string, string> { ["rest"] = "api/core/rendering" }),
            };
            return Task.FromResult(paths);
        }
    }
}
