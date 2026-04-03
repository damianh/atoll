using Atoll.Build.Ssg;
using Atoll.Core.Components;
using Atoll.Routing;
using Shouldly;
using Xunit;

namespace Atoll.Build.Tests.Ssg;

public sealed class RouteEnumeratorTests
{
    private readonly RouteEnumerator _enumerator = new();

    [Fact]
    public async Task ShouldEnumerateStaticRoute()
    {
        var routes = new[]
        {
            new RouteEntry("/about", typeof(TestAboutPage), "about.cs"),
        };

        var result = await _enumerator.EnumerateAsync(routes);

        result.Count.ShouldBe(1);
        result[0].UrlPath.ShouldBe("/about");
        result[0].ComponentType.ShouldBe(typeof(TestAboutPage));
        result[0].Parameters.Count.ShouldBe(0);
    }

    [Fact]
    public async Task ShouldEnumerateRootRoute()
    {
        var routes = new[]
        {
            new RouteEntry("/", typeof(TestIndexPage), "index.cs"),
        };

        var result = await _enumerator.EnumerateAsync(routes);

        result.Count.ShouldBe(1);
        result[0].UrlPath.ShouldBe("/");
    }

    [Fact]
    public async Task ShouldExpandDynamicRouteViaGetStaticPaths()
    {
        var routes = new[]
        {
            new RouteEntry("/blog/[slug]", typeof(TestBlogPage), "blog/[slug].cs"),
        };

        var result = await _enumerator.EnumerateAsync(routes);

        result.Count.ShouldBe(2);
        result[0].UrlPath.ShouldBe("/blog/hello-world");
        result[0].Parameters["slug"].ShouldBe("hello-world");
        result[1].UrlPath.ShouldBe("/blog/second-post");
        result[1].Parameters["slug"].ShouldBe("second-post");
    }

    [Fact]
    public async Task ShouldPassPropsFromStaticPaths()
    {
        var routes = new[]
        {
            new RouteEntry("/blog/[slug]", typeof(TestBlogPageWithProps), "blog/[slug].cs"),
        };

        var result = await _enumerator.EnumerateAsync(routes);

        result.Count.ShouldBe(1);
        result[0].UrlPath.ShouldBe("/blog/my-post");
        result[0].Props["Title"].ShouldBe("My Post Title");
    }

    [Fact]
    public async Task ShouldExpandCatchAllRoute()
    {
        var routes = new[]
        {
            new RouteEntry("/docs/[...rest]", typeof(TestDocsPage), "docs/[...rest].cs"),
        };

        var result = await _enumerator.EnumerateAsync(routes);

        result.Count.ShouldBe(2);
        result[0].UrlPath.ShouldBe("/docs/getting-started");
        result[1].UrlPath.ShouldBe("/docs/guides/advanced");
    }

    [Fact]
    public async Task ShouldSkipEndpointRoutes()
    {
        var routes = new[]
        {
            new RouteEntry("/about", typeof(TestAboutPage), "about.cs"),
            new RouteEntry("/api/posts", typeof(TestApiEndpoint), "api/posts.cs"),
        };

        var result = await _enumerator.EnumerateAsync(routes);

        result.Count.ShouldBe(1);
        result[0].UrlPath.ShouldBe("/about");
    }

    [Fact]
    public async Task ShouldThrowForDynamicRouteWithoutStaticPathsProvider()
    {
        var routes = new[]
        {
            new RouteEntry("/blog/[slug]", typeof(TestAboutPage), "blog/[slug].cs"),
        };

        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => _enumerator.EnumerateAsync(routes));

        exception.Message.ShouldContain("IStaticPathsProvider");
        exception.Message.ShouldContain("/blog/[slug]");
    }

    [Fact]
    public async Task ShouldEnumerateMixedStaticAndDynamicRoutes()
    {
        var routes = new[]
        {
            new RouteEntry("/", typeof(TestIndexPage), "index.cs"),
            new RouteEntry("/about", typeof(TestAboutPage), "about.cs"),
            new RouteEntry("/blog/[slug]", typeof(TestBlogPage), "blog/[slug].cs"),
        };

        var result = await _enumerator.EnumerateAsync(routes);

        result.Count.ShouldBe(4); // 1 + 1 + 2 dynamic
        result[0].UrlPath.ShouldBe("/");
        result[1].UrlPath.ShouldBe("/about");
        result[2].UrlPath.ShouldBe("/blog/hello-world");
        result[3].UrlPath.ShouldBe("/blog/second-post");
    }

    [Fact]
    public async Task ShouldHandleEmptyRouteEntries()
    {
        var result = await _enumerator.EnumerateAsync([]);

        result.Count.ShouldBe(0);
    }

    [Fact]
    public async Task ShouldHandleEmptyStaticPaths()
    {
        var routes = new[]
        {
            new RouteEntry("/blog/[slug]", typeof(TestEmptyStaticPathsPage), "blog/[slug].cs"),
        };

        var result = await _enumerator.EnumerateAsync(routes);

        result.Count.ShouldBe(0);
    }

    [Fact]
    public void ShouldThrowOnNullRoutes()
    {
        Should.ThrowAsync<ArgumentNullException>(
            () => _enumerator.EnumerateAsync(null!));
    }

    [Fact]
    public async Task ShouldResolveUrlPathWithDynamicSegment()
    {
        // Test URL resolution indirectly through EnumerateAsync
        var routes = new[]
        {
            new RouteEntry("/blog/[slug]", typeof(TestBlogPage), "blog/[slug].cs"),
        };

        var result = await _enumerator.EnumerateAsync(routes);

        // The first path returned by TestBlogPage is slug=hello-world
        result[0].UrlPath.ShouldBe("/blog/hello-world");
    }

    [Fact]
    public async Task ShouldResolveUrlPathWithCatchAll()
    {
        var routes = new[]
        {
            new RouteEntry("/docs/[...rest]", typeof(TestDocsPage), "docs/[...rest].cs"),
        };

        var result = await _enumerator.EnumerateAsync(routes);

        result[0].UrlPath.ShouldBe("/docs/getting-started");
        result[1].UrlPath.ShouldBe("/docs/guides/advanced");
    }

    [Fact]
    public async Task ShouldResolveMultipleDynamicSegments()
    {
        var routes = new[]
        {
            new RouteEntry("/archive/[year]/[month]", typeof(TestArchivePage), "archive/[year]/[month].cs"),
        };

        var result = await _enumerator.EnumerateAsync(routes);

        result[0].UrlPath.ShouldBe("/archive/2026/04");
    }

    // ── Test helpers ─────────────────────────────────────────────

    private sealed class TestIndexPage : AtollComponent, IAtollPage
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            context.WriteHtml("<h1>Home</h1>");
            return Task.CompletedTask;
        }
    }

    private sealed class TestAboutPage : AtollComponent, IAtollPage
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            context.WriteHtml("<h1>About</h1>");
            return Task.CompletedTask;
        }
    }

    private sealed class TestBlogPage : AtollComponent, IAtollPage, IStaticPathsProvider
    {
        [Parameter]
        public string Slug { get; set; } = "";

        public Task<IReadOnlyList<StaticPath>> GetStaticPathsAsync()
        {
            IReadOnlyList<StaticPath> paths = new[]
            {
                new StaticPath(new Dictionary<string, string> { ["slug"] = "hello-world" }),
                new StaticPath(new Dictionary<string, string> { ["slug"] = "second-post" }),
            };
            return Task.FromResult(paths);
        }

        protected override Task RenderCoreAsync(RenderContext context)
        {
            context.WriteHtml($"<h1>Blog: {Slug}</h1>");
            return Task.CompletedTask;
        }
    }

    private sealed class TestBlogPageWithProps : AtollComponent, IAtollPage, IStaticPathsProvider
    {
        [Parameter]
        public string Title { get; set; } = "";

        public Task<IReadOnlyList<StaticPath>> GetStaticPathsAsync()
        {
            IReadOnlyList<StaticPath> paths = new[]
            {
                new StaticPath(
                    new Dictionary<string, string> { ["slug"] = "my-post" },
                    new Dictionary<string, object?> { ["Title"] = "My Post Title" }),
            };
            return Task.FromResult(paths);
        }

        protected override Task RenderCoreAsync(RenderContext context)
        {
            context.WriteHtml($"<h1>{Title}</h1>");
            return Task.CompletedTask;
        }
    }

    private sealed class TestDocsPage : AtollComponent, IAtollPage, IStaticPathsProvider
    {
        [Parameter]
        public string Rest { get; set; } = "";

        public Task<IReadOnlyList<StaticPath>> GetStaticPathsAsync()
        {
            IReadOnlyList<StaticPath> paths = new[]
            {
                new StaticPath(new Dictionary<string, string> { ["rest"] = "getting-started" }),
                new StaticPath(new Dictionary<string, string> { ["rest"] = "guides/advanced" }),
            };
            return Task.FromResult(paths);
        }

        protected override Task RenderCoreAsync(RenderContext context)
        {
            context.WriteHtml($"<h1>Docs: {Rest}</h1>");
            return Task.CompletedTask;
        }
    }

    private sealed class TestEmptyStaticPathsPage : AtollComponent, IAtollPage, IStaticPathsProvider
    {
        public Task<IReadOnlyList<StaticPath>> GetStaticPathsAsync()
        {
            IReadOnlyList<StaticPath> paths = [];
            return Task.FromResult(paths);
        }

        protected override Task RenderCoreAsync(RenderContext context)
        {
            context.WriteHtml("<h1>Empty</h1>");
            return Task.CompletedTask;
        }
    }

    private sealed class TestApiEndpoint : IAtollEndpoint
    {
        public Task<AtollResponse> GetAsync(EndpointContext context)
        {
            return Task.FromResult(AtollResponse.Json(new { data = "test" }));
        }
    }

    private sealed class TestArchivePage : AtollComponent, IAtollPage, IStaticPathsProvider
    {
        [Parameter]
        public string Year { get; set; } = "";

        [Parameter]
        public string Month { get; set; } = "";

        public Task<IReadOnlyList<StaticPath>> GetStaticPathsAsync()
        {
            IReadOnlyList<StaticPath> paths = new[]
            {
                new StaticPath(new Dictionary<string, string>
                {
                    ["year"] = "2026",
                    ["month"] = "04",
                }),
            };
            return Task.FromResult(paths);
        }

        protected override Task RenderCoreAsync(RenderContext context)
        {
            context.WriteHtml($"<h1>Archive: {Year}/{Month}</h1>");
            return Task.CompletedTask;
        }
    }
}
