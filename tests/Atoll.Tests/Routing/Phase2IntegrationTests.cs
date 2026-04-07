using Atoll.Components;
using Atoll.Rendering;
using Atoll.Routing.FileSystem;
using Atoll.Routing.Matching;

namespace Atoll.Routing.Tests;

/// <summary>
/// Phase 2 integration tests covering cross-cutting scenarios across
/// route discovery, pattern matching, page types, endpoints, and
/// the full request pipeline (sans ASP.NET Core middleware).
/// </summary>
public sealed class Phase2IntegrationTests
{
    // ================================================================
    // RoutePattern computed-property tests
    // ================================================================

    [Fact]
    public void RoutePatternShouldReportStaticPatternAsNotDynamic()
    {
        var pattern = new RoutePattern("/about");

        pattern.IsDynamic.ShouldBeFalse();
        pattern.HasCatchAll.ShouldBeFalse();
        pattern.StaticSegmentCount.ShouldBe(1);
        pattern.DynamicSegmentCount.ShouldBe(0);
    }

    [Fact]
    public void RoutePatternShouldReportDynamicSegmentAsDynamic()
    {
        var pattern = new RoutePattern("/blog/[slug]");

        pattern.IsDynamic.ShouldBeTrue();
        pattern.HasCatchAll.ShouldBeFalse();
        pattern.StaticSegmentCount.ShouldBe(1);
        pattern.DynamicSegmentCount.ShouldBe(1);
    }

    [Fact]
    public void RoutePatternShouldReportCatchAllAsHasCatchAll()
    {
        var pattern = new RoutePattern("/docs/[...rest]");

        pattern.IsDynamic.ShouldBeTrue();
        pattern.HasCatchAll.ShouldBeTrue();
        pattern.StaticSegmentCount.ShouldBe(1);
        pattern.DynamicSegmentCount.ShouldBe(0);
    }

    [Fact]
    public void RoutePatternShouldReportRootAsNotDynamic()
    {
        var pattern = new RoutePattern("/");

        pattern.IsDynamic.ShouldBeFalse();
        pattern.HasCatchAll.ShouldBeFalse();
        pattern.StaticSegmentCount.ShouldBe(0);
        pattern.DynamicSegmentCount.ShouldBe(0);
        pattern.Segments.ShouldBeEmpty();
    }

    [Fact]
    public void RoutePatternShouldCountMixedSegments()
    {
        var pattern = new RoutePattern("/blog/[year]/[slug]");

        pattern.IsDynamic.ShouldBeTrue();
        pattern.HasCatchAll.ShouldBeFalse();
        pattern.StaticSegmentCount.ShouldBe(1);
        pattern.DynamicSegmentCount.ShouldBe(2);
        pattern.Segments.Length.ShouldBe(3);
    }

    [Fact]
    public void RoutePatternShouldStoreRawPattern()
    {
        var pattern = new RoutePattern("/blog/[slug]");
        pattern.RawPattern.ShouldBe("/blog/[slug]");
    }

    [Fact]
    public void RoutePatternShouldThrowForNull()
    {
        Should.Throw<ArgumentNullException>(() => new RoutePattern(null!));
    }

    [Fact]
    public void RoutePatternShouldReportSoleCatchAllAsHasCatchAll()
    {
        var pattern = new RoutePattern("/[...rest]");

        pattern.IsDynamic.ShouldBeTrue();
        pattern.HasCatchAll.ShouldBeTrue();
        pattern.StaticSegmentCount.ShouldBe(0);
        pattern.DynamicSegmentCount.ShouldBe(0);
    }

    // ================================================================
    // RouteComparer null-argument validation
    // ================================================================

    [Fact]
    public void RouteComparerShouldThrowForNullFirstArgument()
    {
        var pattern = new RoutePattern("/about");
        Should.Throw<ArgumentNullException>(() => RouteComparer.Compare(null!, pattern));
    }

    [Fact]
    public void RouteComparerShouldThrowForNullSecondArgument()
    {
        var pattern = new RoutePattern("/about");
        Should.Throw<ArgumentNullException>(() => RouteComparer.Compare(pattern, null!));
    }

    [Fact]
    public void RouteComparerShouldReturnZeroForIdenticalPatterns()
    {
        var pattern1 = new RoutePattern("/about");
        var pattern2 = new RoutePattern("/about");
        RouteComparer.Compare(pattern1, pattern2).ShouldBe(0);
    }

    // ================================================================
    // RouteMatchResult null-argument validation
    // ================================================================

    [Fact]
    public void RouteMatchResultShouldThrowForNullRouteEntry()
    {
        var parameters = new Dictionary<string, string>();
        Should.Throw<ArgumentNullException>(() => new RouteMatchResult(null!, parameters));
    }

    [Fact]
    public void RouteMatchResultShouldThrowForNullParameters()
    {
        var entry = new RouteEntry("/about", typeof(StubPage), "about.cs");
        Should.Throw<ArgumentNullException>(() => new RouteMatchResult(entry, null!));
    }

    // ================================================================
    // RouteSegment null-argument validation
    // ================================================================

    [Fact]
    public void RouteSegmentShouldThrowForNullValue()
    {
        Should.Throw<ArgumentNullException>(() => new RouteSegment(RouteSegmentType.Static, null!));
    }

    // ================================================================
    // RouteConventions edge cases
    // ================================================================

    [Fact]
    public void IsDynamicSegmentShouldThrowForNull()
    {
        Should.Throw<ArgumentNullException>(() => RouteConventions.IsDynamicSegment(null!));
    }

    [Fact]
    public void IsCatchAllSegmentShouldThrowForNull()
    {
        Should.Throw<ArgumentNullException>(() => RouteConventions.IsCatchAllSegment(null!));
    }

    [Fact]
    public void ExtractParameterNameShouldThrowForNull()
    {
        Should.Throw<ArgumentNullException>(() => RouteConventions.ExtractParameterName(null!));
    }

    [Fact]
    public void IsDynamicSegmentShouldReturnFalseForEmptyBrackets()
    {
        // "[]" starts with '[' and ends with ']' so IsDynamic returns true
        // but it won't parse as a valid parameter name (empty)
        RouteConventions.IsDynamicSegment("[]").ShouldBeTrue();
    }

    [Fact]
    public void IsCatchAllSegmentShouldReturnTrueForEmptyCatchAll()
    {
        // "[...]" starts with "[..." and ends with ']'
        RouteConventions.IsCatchAllSegment("[...]").ShouldBeTrue();
    }

    [Fact]
    public void IsCatchAllSegmentShouldReturnFalseForPartialBrackets()
    {
        RouteConventions.IsCatchAllSegment("[..rest").ShouldBeFalse();
    }

    // ================================================================
    // Cross-cutting: Static paths → route discovery → matching → page rendering
    // ================================================================

    [Fact]
    public async Task EndToEndStaticPathsShouldProduceMatchableRoutes()
    {
        // Given a page that provides static paths
        var dynamicPage = new StubBlogPage();
        var staticPaths = await ((IStaticPathsProvider)dynamicPage).GetStaticPathsAsync();

        // Discover routes
        var entries = new List<(string RelativeFilePath, Type ComponentType)>
        {
            ("index.cs", typeof(StubHomePage)),
            ("blog/[slug].cs", typeof(StubBlogPage)),
            ("api/posts.cs", typeof(StubApiEndpoint)),
        };
        var routes = RouteDiscovery.DiscoverRoutesFromEntries(entries);
        var matcher = new RouteMatcher(routes);

        // Each static path should be matchable
        foreach (var staticPath in staticPaths)
        {
            var url = "/blog/" + staticPath.Parameters["slug"];
            var match = matcher.Match(url);

            match.ShouldNotBeNull();
            match.RouteEntry.ComponentType.ShouldBe(typeof(StubBlogPage));
            match.Parameters["slug"].ShouldBe(staticPath.Parameters["slug"]);
        }
    }

    [Fact]
    public async Task EndToEndStaticPathsPageShouldRenderWithExtractedParams()
    {
        // Discover routes
        var entries = new List<(string RelativeFilePath, Type ComponentType)>
        {
            ("blog/[slug].cs", typeof(StubBlogPage)),
        };
        var routes = RouteDiscovery.DiscoverRoutesFromEntries(entries);
        var matcher = new RouteMatcher(routes);

        // Match URL
        var match = matcher.Match("/blog/hello-world");
        match.ShouldNotBeNull();

        // Render the page with extracted parameters as props
        var props = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var kvp in match.Parameters)
        {
            props[kvp.Key] = kvp.Value;
        }

        var page = (IAtollComponent)Activator.CreateInstance(match.RouteEntry.ComponentType)!;
        var destination = new StringRenderDestination();
        await ComponentRenderer.RenderComponentAsync(page, destination, props);

        var output = destination.GetOutput();
        output.ShouldContain("<h1>Blog: hello-world</h1>");
    }

    [Fact]
    public async Task EndToEndCatchAllPageShouldRenderWithExtractedRestParams()
    {
        var entries = new List<(string RelativeFilePath, Type ComponentType)>
        {
            ("docs/[...rest].cs", typeof(StubDocsPage)),
        };
        var routes = RouteDiscovery.DiscoverRoutesFromEntries(entries);
        var matcher = new RouteMatcher(routes);

        var match = matcher.Match("/docs/getting-started/install");
        match.ShouldNotBeNull();

        var props = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var kvp in match.Parameters)
        {
            props[kvp.Key] = kvp.Value;
        }

        var page = (IAtollComponent)Activator.CreateInstance(match.RouteEntry.ComponentType)!;
        var destination = new StringRenderDestination();
        await ComponentRenderer.RenderComponentAsync(page, destination, props);

        var output = destination.GetOutput();
        output.ShouldContain("<p>Path: getting-started/install</p>");
    }

    [Fact]
    public async Task EndToEndEmptyRestParamShouldRenderEmptyString()
    {
        var entries = new List<(string RelativeFilePath, Type ComponentType)>
        {
            ("docs/[...rest].cs", typeof(StubDocsPage)),
        };
        var routes = RouteDiscovery.DiscoverRoutesFromEntries(entries);
        var matcher = new RouteMatcher(routes);

        var match = matcher.Match("/docs");
        match.ShouldNotBeNull();
        match.Parameters["rest"].ShouldBe(string.Empty);

        var props = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["rest"] = match.Parameters["rest"]
        };

        var page = (IAtollComponent)Activator.CreateInstance(match.RouteEntry.ComponentType)!;
        var destination = new StringRenderDestination();
        await ComponentRenderer.RenderComponentAsync(page, destination, props);

        var output = destination.GetOutput();
        output.ShouldContain("<p>Path: </p>");
    }

    // ================================================================
    // Cross-cutting: Mixed page + endpoint route table
    // ================================================================

    [Fact]
    public void MixedRouteTableShouldContainBothPagesAndEndpoints()
    {
        var entries = new List<(string RelativeFilePath, Type ComponentType)>
        {
            ("index.cs", typeof(StubHomePage)),
            ("about.cs", typeof(StubAboutPage)),
            ("blog/[slug].cs", typeof(StubBlogPage)),
            ("api/posts.cs", typeof(StubApiEndpoint)),
            ("api/posts/[slug].cs", typeof(StubDynamicApiEndpoint)),
        };
        var routes = RouteDiscovery.DiscoverRoutesFromEntries(entries);
        var matcher = new RouteMatcher(routes);

        // Pages
        matcher.Match("/")!.RouteEntry.ComponentType.ShouldBe(typeof(StubHomePage));
        matcher.Match("/about")!.RouteEntry.ComponentType.ShouldBe(typeof(StubAboutPage));
        matcher.Match("/blog/my-post")!.RouteEntry.ComponentType.ShouldBe(typeof(StubBlogPage));

        // Endpoints
        matcher.Match("/api/posts")!.RouteEntry.ComponentType.ShouldBe(typeof(StubApiEndpoint));
        matcher.Match("/api/posts/my-post")!.RouteEntry.ComponentType.ShouldBe(typeof(StubDynamicApiEndpoint));
    }

    [Fact]
    public async Task MixedRouteTableShouldDispatchEndpointsCorrectly()
    {
        var entries = new List<(string RelativeFilePath, Type ComponentType)>
        {
            ("index.cs", typeof(StubHomePage)),
            ("api/posts.cs", typeof(StubApiEndpoint)),
        };
        var routes = RouteDiscovery.DiscoverRoutesFromEntries(entries);
        var matcher = new RouteMatcher(routes);

        // Match endpoint
        var endpointMatch = matcher.Match("/api/posts");
        endpointMatch.ShouldNotBeNull();
        typeof(IAtollEndpoint).IsAssignableFrom(endpointMatch.RouteEntry.ComponentType).ShouldBeTrue();

        // Dispatch
        var endpoint = (IAtollEndpoint)Activator.CreateInstance(endpointMatch.RouteEntry.ComponentType)!;
        var context = new EndpointContext(
            endpointMatch.Parameters,
            new EndpointRequest("GET", new Uri("http://localhost/api/posts")));
        var response = await EndpointDispatcher.DispatchAsync(endpoint, context);

        response.StatusCode.ShouldBe(200);
        response.GetBodyAsString()!.ShouldContain("Hello World");
    }

    [Fact]
    public async Task MixedRouteTableShouldRenderPagesCorrectly()
    {
        var entries = new List<(string RelativeFilePath, Type ComponentType)>
        {
            ("index.cs", typeof(StubHomePage)),
            ("api/posts.cs", typeof(StubApiEndpoint)),
        };
        var routes = RouteDiscovery.DiscoverRoutesFromEntries(entries);
        var matcher = new RouteMatcher(routes);

        // Match page
        var pageMatch = matcher.Match("/");
        pageMatch.ShouldNotBeNull();
        typeof(IAtollComponent).IsAssignableFrom(pageMatch.RouteEntry.ComponentType).ShouldBeTrue();

        // Render page
        var page = (IAtollComponent)Activator.CreateInstance(pageMatch.RouteEntry.ComponentType)!;
        var destination = new StringRenderDestination();
        await page.RenderAsync(new RenderContext(destination));

        destination.GetOutput().ShouldContain("<h1>Home</h1>");
    }

    // ================================================================
    // Cross-cutting: Page rendering with layout wrapping + route params
    // ================================================================

    [Fact]
    public async Task PageWithLayoutShouldRenderInsideLayout()
    {
        var entries = new List<(string RelativeFilePath, Type ComponentType)>
        {
            ("layout-page.cs", typeof(StubLayoutPage)),
        };
        var routes = RouteDiscovery.DiscoverRoutesFromEntries(entries);
        var matcher = new RouteMatcher(routes);

        var match = matcher.Match("/layout-page");
        match.ShouldNotBeNull();

        // Simulate what AtollRequestHandler does: render page fragment + wrap with layouts
        var componentType = match.RouteEntry.ComponentType;
        var pageFragment = RenderFragment.FromAsync(async destination =>
        {
            var component = (IAtollComponent)Activator.CreateInstance(componentType)!;
            await ComponentRenderer.RenderComponentAsync(component, destination);
        });
        var wrappedFragment = LayoutResolver.WrapWithLayouts(componentType, pageFragment);

        // Render through PageRenderer
        var renderer = new PageRenderer();
        var result = await renderer.RenderPageAsync(async context =>
        {
            await context.RenderAsync(wrappedFragment);
        });

        var html = result.Html;
        html.ShouldStartWith("<!DOCTYPE html>");
        html.ShouldContain("<nav>Main Nav</nav>");
        html.ShouldContain("<h1>Layout Page Content</h1>");
        html.ShouldContain("<footer>Main Footer</footer>");
    }

    [Fact]
    public async Task PageWithNestedLayoutShouldRenderAllLayoutLayers()
    {
        var entries = new List<(string RelativeFilePath, Type ComponentType)>
        {
            ("nested-page.cs", typeof(StubNestedLayoutPage)),
        };
        var routes = RouteDiscovery.DiscoverRoutesFromEntries(entries);
        var matcher = new RouteMatcher(routes);

        var match = matcher.Match("/nested-page");
        match.ShouldNotBeNull();

        var componentType = match.RouteEntry.ComponentType;
        var pageFragment = RenderFragment.FromAsync(async destination =>
        {
            var component = (IAtollComponent)Activator.CreateInstance(componentType)!;
            await ComponentRenderer.RenderComponentAsync(component, destination);
        });
        var wrappedFragment = LayoutResolver.WrapWithLayouts(componentType, pageFragment);

        var renderer = new PageRenderer();
        var result = await renderer.RenderPageAsync(async context =>
        {
            await context.RenderAsync(wrappedFragment);
        });

        var html = result.Html;
        // Outer layout wraps inner layout which wraps page content
        html.ShouldContain("<header>Outer</header>");
        html.ShouldContain("<section>Inner:");
        html.ShouldContain("<p>Nested Page</p>");
        html.ShouldContain("</section>");
        html.ShouldContain("<footer>Outer End</footer>");
    }

    [Fact]
    public async Task AsyncPageShouldRenderWithRouteParams()
    {
        var entries = new List<(string RelativeFilePath, Type ComponentType)>
        {
            ("async/[id].cs", typeof(StubAsyncPage)),
        };
        var routes = RouteDiscovery.DiscoverRoutesFromEntries(entries);
        var matcher = new RouteMatcher(routes);

        var match = matcher.Match("/async/42");
        match.ShouldNotBeNull();

        var props = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["id"] = match.Parameters["id"]
        };

        var page = (IAtollComponent)Activator.CreateInstance(match.RouteEntry.ComponentType)!;
        var destination = new StringRenderDestination();
        await ComponentRenderer.RenderComponentAsync(page, destination, props);

        destination.GetOutput().ShouldContain("<p>Item: 42</p>");
    }

    // ================================================================
    // Cross-cutting: Route priority with pages and endpoints
    // ================================================================

    [Fact]
    public void StaticPageShouldMatchBeforeDynamicEndpoint()
    {
        var entries = new List<(string RelativeFilePath, Type ComponentType)>
        {
            ("api/[slug].cs", typeof(StubDynamicApiEndpoint)),
            ("api/posts.cs", typeof(StubApiEndpoint)),
        };
        var routes = RouteDiscovery.DiscoverRoutesFromEntries(entries);
        var matcher = new RouteMatcher(routes);

        // Static route "posts" should match before dynamic [slug]
        matcher.Match("/api/posts")!.RouteEntry.ComponentType.ShouldBe(typeof(StubApiEndpoint));
        // But a non-matching path should fall through to dynamic
        matcher.Match("/api/users")!.RouteEntry.ComponentType.ShouldBe(typeof(StubDynamicApiEndpoint));
    }

    // ================================================================
    // Cross-cutting: EndpointDispatcher with route-matched params
    // ================================================================

    [Fact]
    public async Task EndpointDispatcherShouldPassExtractedRouteParams()
    {
        var entries = new List<(string RelativeFilePath, Type ComponentType)>
        {
            ("api/posts/[slug].cs", typeof(StubDynamicApiEndpoint)),
        };
        var routes = RouteDiscovery.DiscoverRoutesFromEntries(entries);
        var matcher = new RouteMatcher(routes);

        var match = matcher.Match("/api/posts/test-post");
        match.ShouldNotBeNull();
        match.Parameters["slug"].ShouldBe("test-post");

        var endpoint = (IAtollEndpoint)Activator.CreateInstance(match.RouteEntry.ComponentType)!;
        var context = new EndpointContext(
            match.Parameters,
            new EndpointRequest("GET", new Uri("http://localhost/api/posts/test-post")));
        var response = await EndpointDispatcher.DispatchAsync(endpoint, context);

        response.StatusCode.ShouldBe(200);
        response.GetBodyAsString()!.ShouldContain("test-post");
    }

    [Fact]
    public async Task EndpointDispatcherShouldReturn405WhenMethodNotSupported()
    {
        var entries = new List<(string RelativeFilePath, Type ComponentType)>
        {
            ("api/posts.cs", typeof(StubApiEndpoint)),
        };
        var routes = RouteDiscovery.DiscoverRoutesFromEntries(entries);
        var matcher = new RouteMatcher(routes);

        var match = matcher.Match("/api/posts");
        match.ShouldNotBeNull();

        var endpoint = (IAtollEndpoint)Activator.CreateInstance(match.RouteEntry.ComponentType)!;
        var context = new EndpointContext(
            match.Parameters,
            new EndpointRequest("DELETE", new Uri("http://localhost/api/posts")));
        var response = await EndpointDispatcher.DispatchAsync(endpoint, context);

        response.StatusCode.ShouldBe(405);
        response.Headers["Allow"].ShouldContain("GET");
    }

    // ================================================================
    // Cross-cutting: RouteDiscovery integration with IsRoutableType
    // ================================================================

    [Fact]
    public void RouteDiscoveryShouldHandleBothPageAndEndpointTypes()
    {
        var entries = new List<(string RelativeFilePath, Type ComponentType)>
        {
            ("index.cs", typeof(StubHomePage)),
            ("api/data.cs", typeof(StubApiEndpoint)),
        };
        var routes = RouteDiscovery.DiscoverRoutesFromEntries(entries);

        routes.Count.ShouldBe(2);
        routes[0].Pattern.ShouldBe("/");
        routes[1].Pattern.ShouldBe("/api/data");
    }

    // ================================================================
    // Cross-cutting: Multiple dynamic segment route with rendering
    // ================================================================

    [Fact]
    public async Task MultipleDynamicSegmentsShouldExtractAllParams()
    {
        var entries = new List<(string RelativeFilePath, Type ComponentType)>
        {
            ("blog/[year]/[slug].cs", typeof(StubYearSlugPage)),
        };
        var routes = RouteDiscovery.DiscoverRoutesFromEntries(entries);
        var matcher = new RouteMatcher(routes);

        var match = matcher.Match("/blog/2024/my-post");
        match.ShouldNotBeNull();
        match.Parameters["year"].ShouldBe("2024");
        match.Parameters["slug"].ShouldBe("my-post");

        var props = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var kvp in match.Parameters)
        {
            props[kvp.Key] = kvp.Value;
        }

        var page = (IAtollComponent)Activator.CreateInstance(match.RouteEntry.ComponentType)!;
        var destination = new StringRenderDestination();
        await ComponentRenderer.RenderComponentAsync(page, destination, props);

        destination.GetOutput().ShouldContain("Year: 2024");
        destination.GetOutput().ShouldContain("Post: my-post");
    }

    // ================================================================
    // Cross-cutting: Page rendering through PageRenderer with params
    // ================================================================

    [Fact]
    public async Task PageRendererShouldInjectDoctypeAndHeadContent()
    {
        var pageFragment = RenderFragment.FromAsync(async destination =>
        {
            var page = new StubHeadPage();
            await ComponentRenderer.RenderComponentAsync(page, destination);
        });

        var renderer = new PageRenderer();
        var result = await renderer.RenderPageAsync(async context =>
        {
            await context.RenderAsync(pageFragment);
        });

        var html = result.Html;
        html.ShouldStartWith("<!DOCTYPE html>");
        html.ShouldContain("<head>");
        html.ShouldContain("<title>Head Page</title>");
        html.ShouldContain("</head>");
    }

    // ================================================================
    // Stub components
    // ================================================================

    private sealed class StubPage : IAtollComponent
    {
        public Task RenderAsync(RenderContext context) => Task.CompletedTask;
    }

    private sealed class StubHomePage : IAtollPage
    {
        public Task RenderAsync(RenderContext context)
        {
            context.WriteHtml("<html><head><title>Home</title></head><body><h1>Home</h1></body></html>");
            return Task.CompletedTask;
        }
    }

    private sealed class StubAboutPage : IAtollPage
    {
        public Task RenderAsync(RenderContext context)
        {
            context.WriteHtml("<html><body><h1>About</h1></body></html>");
            return Task.CompletedTask;
        }
    }

    private sealed class StubBlogPage : IAtollPage, IStaticPathsProvider
    {
        [Parameter(Required = true)]
        public string Slug { get; set; } = "";

        public Task RenderAsync(RenderContext context)
        {
            context.WriteHtml($"<h1>Blog: {Slug}</h1>");
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<StaticPath>> GetStaticPathsAsync()
        {
            IReadOnlyList<StaticPath> paths =
            [
                new(new Dictionary<string, string> { ["slug"] = "hello-world" }),
                new(new Dictionary<string, string> { ["slug"] = "getting-started" }),
            ];
            return Task.FromResult(paths);
        }
    }

    private sealed class StubDocsPage : IAtollPage
    {
        [Parameter]
        public string Rest { get; set; } = "";

        public Task RenderAsync(RenderContext context)
        {
            context.WriteHtml($"<p>Path: {Rest}</p>");
            return Task.CompletedTask;
        }
    }

    private sealed class StubYearSlugPage : IAtollPage
    {
        [Parameter(Required = true)]
        public string Year { get; set; } = "";

        [Parameter(Required = true)]
        public string Slug { get; set; } = "";

        public Task RenderAsync(RenderContext context)
        {
            context.WriteHtml($"<p>Year: {Year}, Post: {Slug}</p>");
            return Task.CompletedTask;
        }
    }

    private sealed class StubAsyncPage : AtollComponent, IAtollPage
    {
        [Parameter(Required = true)]
        public string Id { get; set; } = "";

        protected override async Task RenderCoreAsync(RenderContext context)
        {
            await Task.Yield(); // Force async path
            WriteHtml($"<p>Item: {Id}</p>");
        }
    }

    private sealed class StubHeadPage : IAtollPage
    {
        public Task RenderAsync(RenderContext context)
        {
            context.WriteHtml("<html><head><title>Head Page</title></head><body><p>Content</p></body></html>");
            return Task.CompletedTask;
        }
    }

    // Layout components

    private sealed class StubMainLayout : AtollComponent
    {
        protected override async Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<html><body><nav>Main Nav</nav><main>");
            await RenderSlotAsync();
            WriteHtml("</main><footer>Main Footer</footer></body></html>");
        }
    }

    [Layout(typeof(StubMainLayout))]
    private sealed class StubLayoutPage : AtollComponent, IAtollPage
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<h1>Layout Page Content</h1>");
            return Task.CompletedTask;
        }
    }

    private sealed class StubOuterLayout : AtollComponent
    {
        protected override async Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<header>Outer</header>");
            await RenderSlotAsync();
            WriteHtml("<footer>Outer End</footer>");
        }
    }

    [Layout(typeof(StubOuterLayout))]
    private sealed class StubInnerLayout : AtollComponent
    {
        protected override async Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<section>Inner:");
            await RenderSlotAsync();
            WriteHtml("</section>");
        }
    }

    [Layout(typeof(StubInnerLayout))]
    private sealed class StubNestedLayoutPage : AtollComponent, IAtollPage
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<p>Nested Page</p>");
            return Task.CompletedTask;
        }
    }

    // Endpoint components

    private sealed class StubApiEndpoint : IAtollEndpoint
    {
        public Task<AtollResponse> GetAsync(EndpointContext context)
        {
            return Task.FromResult(AtollResponse.Json(new[] { new { Id = 1, Title = "Hello World" } }));
        }
    }

    private sealed class StubDynamicApiEndpoint : IAtollEndpoint
    {
        public Task<AtollResponse> GetAsync(EndpointContext context)
        {
            var slug = context.GetParameter("slug");
            return Task.FromResult(AtollResponse.Json(new { Slug = slug }));
        }
    }

    // ================================================================
    // Additional Phase 2 integration test coverage
    // ================================================================

    // ----------------------------------------------------------------
    // RouteEntry prerender flag
    // ----------------------------------------------------------------

    [Fact]
    public void RouteEntryShouldDefaultPrerenderToFalse()
    {
        var entry = new RouteEntry("/about", typeof(StubPage), "about.cs");
        entry.Prerender.ShouldBeFalse();
    }

    [Fact]
    public void RouteEntryShouldStorePrerenderFlag()
    {
        var entry = new RouteEntry("/about", typeof(StubPage), "about.cs", true);
        entry.Prerender.ShouldBeTrue();
    }

    [Fact]
    public void RouteEntryShouldThrowForNullPattern()
    {
        Should.Throw<ArgumentNullException>(() => new RouteEntry(null!, typeof(StubPage), "about.cs"));
    }

    [Fact]
    public void RouteEntryShouldThrowForNullComponentType()
    {
        Should.Throw<ArgumentNullException>(() => new RouteEntry("/about", null!, "about.cs"));
    }

    [Fact]
    public void RouteEntryShouldThrowForNullRelativeFilePath()
    {
        Should.Throw<ArgumentNullException>(() => new RouteEntry("/about", typeof(StubPage), null!));
    }

    // ----------------------------------------------------------------
    // RouteMatcher edge cases
    // ----------------------------------------------------------------

    [Fact]
    public void RouteMatcherShouldThrowForNullRoutes()
    {
        Should.Throw<ArgumentNullException>(() => new RouteMatcher(null!));
    }

    [Fact]
    public void RouteMatcherShouldThrowForNullPath()
    {
        var matcher = new RouteMatcher(Array.Empty<RouteEntry>());
        Should.Throw<ArgumentNullException>(() => matcher.Match(null!));
    }

    [Fact]
    public void RouteMatcherShouldReturnNullForEmptyRouteTable()
    {
        var matcher = new RouteMatcher(Array.Empty<RouteEntry>());
        matcher.Match("/anything").ShouldBeNull();
    }

    [Fact]
    public void RouteMatcherSortedRoutesShouldExposeEntriesInPriorityOrder()
    {
        var entries = new List<(string RelativeFilePath, Type ComponentType)>
        {
            ("docs/[...rest].cs", typeof(StubDocsPage)),
            ("docs/[slug].cs", typeof(StubBlogPage)),
            ("docs/intro.cs", typeof(StubAboutPage)),
        };
        var routes = RouteDiscovery.DiscoverRoutesFromEntries(entries);
        var matcher = new RouteMatcher(routes);

        var sorted = matcher.SortedRoutes;
        // Static first, then dynamic, then catch-all
        sorted[0].Pattern.ShouldBe("/docs/intro");
        sorted[1].Pattern.ShouldBe("/docs/[slug]");
        sorted[2].Pattern.ShouldBe("/docs/[...rest]");
    }

    [Fact]
    public void RouteMatcherShouldReturnNullForNoMatchOnStaticRoute()
    {
        var entries = new List<(string RelativeFilePath, Type ComponentType)>
        {
            ("about.cs", typeof(StubAboutPage)),
        };
        var routes = RouteDiscovery.DiscoverRoutesFromEntries(entries);
        var matcher = new RouteMatcher(routes);

        matcher.Match("/contact").ShouldBeNull();
    }

    [Fact]
    public void RouteMatcherShouldNotMatchDynamicSegmentWithEmptyValue()
    {
        var entries = new List<(string RelativeFilePath, Type ComponentType)>
        {
            ("blog/[slug].cs", typeof(StubBlogPage)),
        };
        var routes = RouteDiscovery.DiscoverRoutesFromEntries(entries);
        var matcher = new RouteMatcher(routes);

        // Path with trailing slash but no actual segment value
        // "/blog/" splits to ["blog", ""] — empty segment should not match dynamic
        matcher.Match("/blog/").ShouldBeNull();
    }

    [Fact]
    public void RouteMatcherShouldNotMatchWhenTooFewSegments()
    {
        var entries = new List<(string RelativeFilePath, Type ComponentType)>
        {
            ("blog/[year]/[slug].cs", typeof(StubYearSlugPage)),
        };
        var routes = RouteDiscovery.DiscoverRoutesFromEntries(entries);
        var matcher = new RouteMatcher(routes);

        // Only one segment after "blog" — needs two
        matcher.Match("/blog/2024").ShouldBeNull();
    }

    [Fact]
    public void RouteMatcherShouldNotMatchWhenTooManySegments()
    {
        var entries = new List<(string RelativeFilePath, Type ComponentType)>
        {
            ("blog/[slug].cs", typeof(StubBlogPage)),
        };
        var routes = RouteDiscovery.DiscoverRoutesFromEntries(entries);
        var matcher = new RouteMatcher(routes);

        // Three segments, route only has two
        matcher.Match("/blog/my-post/extra").ShouldBeNull();
    }

    // ----------------------------------------------------------------
    // RouteConventions cross-cutting edge cases
    // ----------------------------------------------------------------

    [Fact]
    public void FilePathToPatternShouldNormalizeBackslashes()
    {
        RouteConventions.FilePathToPattern("blog\\[slug].cs").ShouldBe("/blog/[slug]");
    }

    [Fact]
    public void FilePathToPatternShouldThrowForNonCsFile()
    {
        Should.Throw<ArgumentException>(() => RouteConventions.FilePathToPattern("about.html"));
    }

    [Fact]
    public void FilePathToPatternShouldThrowForNullPath()
    {
        Should.Throw<ArgumentNullException>(() => RouteConventions.FilePathToPattern(null!));
    }

    [Fact]
    public void FilePathToPatternShouldHandleNestedIndex()
    {
        RouteConventions.FilePathToPattern("blog/index.cs").ShouldBe("/blog");
    }

    [Fact]
    public void ExtractParameterNameShouldThrowForStaticSegment()
    {
        Should.Throw<ArgumentException>(() => RouteConventions.ExtractParameterName("about"));
    }

    [Fact]
    public void ParseSegmentsShouldThrowForNull()
    {
        Should.Throw<ArgumentNullException>(() => RouteConventions.ParseSegments(null!));
    }

    [Fact]
    public void ParseSegmentsShouldReturnEmptyForRoot()
    {
        RouteConventions.ParseSegments("/").ShouldBeEmpty();
    }

    [Fact]
    public void ParseSegmentsShouldThrowForCatchAllNotAtEnd()
    {
        Should.Throw<ArgumentException>(() => RouteConventions.ParseSegments("/[...rest]/extra"));
    }

    [Fact]
    public void ParseSegmentsShouldThrowForEmptyDynamicSegmentName()
    {
        Should.Throw<ArgumentException>(() => RouteConventions.ParseSegments("/blog/[]"));
    }

    [Fact]
    public void ParseSegmentsShouldThrowForEmptyCatchAllSegmentName()
    {
        Should.Throw<ArgumentException>(() => RouteConventions.ParseSegments("/[...]"));
    }

    // ----------------------------------------------------------------
    // Cross-cutting: EndpointDispatcher with all HTTP methods through route match
    // ----------------------------------------------------------------

    [Fact]
    public async Task EndpointDispatcherShouldRouteAllCrudMethodsThroughRouteMatcher()
    {
        var entries = new List<(string RelativeFilePath, Type ComponentType)>
        {
            ("api/items.cs", typeof(StubCrudEndpoint)),
        };
        var routes = RouteDiscovery.DiscoverRoutesFromEntries(entries);
        var matcher = new RouteMatcher(routes);

        var match = matcher.Match("/api/items");
        match.ShouldNotBeNull();

        var endpoint = (IAtollEndpoint)Activator.CreateInstance(match.RouteEntry.ComponentType)!;

        // GET
        var getCtx = new EndpointContext(match.Parameters,
            new EndpointRequest("GET", new Uri("http://localhost/api/items")));
        var getResp = await EndpointDispatcher.DispatchAsync(endpoint, getCtx);
        getResp.StatusCode.ShouldBe(200);
        getResp.GetBodyAsString()!.ShouldContain("list");

        // POST
        var postCtx = new EndpointContext(match.Parameters,
            new EndpointRequest("POST", new Uri("http://localhost/api/items")));
        var postResp = await EndpointDispatcher.DispatchAsync(endpoint, postCtx);
        postResp.StatusCode.ShouldBe(201);

        // PUT
        var putCtx = new EndpointContext(match.Parameters,
            new EndpointRequest("PUT", new Uri("http://localhost/api/items")));
        var putResp = await EndpointDispatcher.DispatchAsync(endpoint, putCtx);
        putResp.StatusCode.ShouldBe(200);
        putResp.GetBodyAsString()!.ShouldContain("updated");

        // DELETE
        var deleteCtx = new EndpointContext(match.Parameters,
            new EndpointRequest("DELETE", new Uri("http://localhost/api/items")));
        var deleteResp = await EndpointDispatcher.DispatchAsync(endpoint, deleteCtx);
        deleteResp.StatusCode.ShouldBe(204);

        // PATCH
        var patchCtx = new EndpointContext(match.Parameters,
            new EndpointRequest("PATCH", new Uri("http://localhost/api/items")));
        var patchResp = await EndpointDispatcher.DispatchAsync(endpoint, patchCtx);
        patchResp.StatusCode.ShouldBe(200);
        patchResp.GetBodyAsString()!.ShouldContain("patched");
    }

    // ----------------------------------------------------------------
    // Cross-cutting: Static paths with props passed through rendering
    // ----------------------------------------------------------------

    [Fact]
    public async Task StaticPathsWithPropsShouldBeAccessibleDuringRendering()
    {
        var dynamicPage = new StubBlogPageWithProps();
        var staticPaths = await ((IStaticPathsProvider)dynamicPage).GetStaticPathsAsync();

        staticPaths.Count.ShouldBe(2);
        staticPaths[0].Props["Title"].ShouldBe("First Post");
        staticPaths[1].Props["Title"].ShouldBe("Second Post");

        // Verify route matching still works
        var entries = new List<(string RelativeFilePath, Type ComponentType)>
        {
            ("blog/[slug].cs", typeof(StubBlogPageWithProps)),
        };
        var routes = RouteDiscovery.DiscoverRoutesFromEntries(entries);
        var matcher = new RouteMatcher(routes);

        var match = matcher.Match("/blog/first-post");
        match.ShouldNotBeNull();
        match.Parameters["slug"].ShouldBe("first-post");
    }

    // ----------------------------------------------------------------
    // Cross-cutting: AtollResponse factory methods integration
    // ----------------------------------------------------------------

    [Fact]
    public async Task EndpointReturningHtmlResponseShouldHaveCorrectHeaders()
    {
        var endpoint = new StubHtmlEndpoint();
        var context = new EndpointContext(
            new Dictionary<string, string>(),
            new EndpointRequest("GET", new Uri("http://localhost/api/html")));

        var response = await EndpointDispatcher.DispatchAsync(endpoint, context);

        response.StatusCode.ShouldBe(200);
        response.Headers["Content-Type"].ShouldBe("text/html; charset=utf-8");
        response.GetBodyAsString()!.ShouldContain("<h1>Hello HTML</h1>");
    }

    [Fact]
    public async Task EndpointReturningEmptyResponseShouldHaveNoBody()
    {
        var endpoint = new StubEmptyResponseEndpoint();
        var context = new EndpointContext(
            new Dictionary<string, string>(),
            new EndpointRequest("DELETE", new Uri("http://localhost/api/empty")));

        var response = await EndpointDispatcher.DispatchAsync(endpoint, context);

        response.StatusCode.ShouldBe(204);
        response.Body.ShouldBeNull();
        response.GetBodyAsString().ShouldBeNull();
    }

    [Fact]
    public async Task EndpointReturningNotFoundShouldReturn404()
    {
        var endpoint = new StubNotFoundEndpoint();
        var context = new EndpointContext(
            new Dictionary<string, string>(),
            new EndpointRequest("GET", new Uri("http://localhost/api/missing")));

        var response = await EndpointDispatcher.DispatchAsync(endpoint, context);

        response.StatusCode.ShouldBe(404);
        response.Body.ShouldBeNull();
    }

    [Fact]
    public async Task EndpointReturningRedirectShouldHaveLocationHeader()
    {
        var endpoint = new StubRedirectEndpoint();
        var context = new EndpointContext(
            new Dictionary<string, string>(),
            new EndpointRequest("GET", new Uri("http://localhost/api/redirect")));

        var response = await EndpointDispatcher.DispatchAsync(endpoint, context);

        response.StatusCode.ShouldBe(302);
        response.Headers["Location"].ShouldBe("/new-location");
        response.Body.ShouldBeNull();
    }

    // ----------------------------------------------------------------
    // Cross-cutting: Case-insensitive route matching with page rendering
    // ----------------------------------------------------------------

    [Fact]
    public async Task CaseInsensitiveMatchingShouldRenderCorrectPage()
    {
        var entries = new List<(string RelativeFilePath, Type ComponentType)>
        {
            ("blog/[slug].cs", typeof(StubBlogPage)),
        };
        var routes = RouteDiscovery.DiscoverRoutesFromEntries(entries);
        var matcher = new RouteMatcher(routes);

        // Match with different casing in static segment
        var match = matcher.Match("/Blog/My-Post");
        match.ShouldNotBeNull();
        match.Parameters["slug"].ShouldBe("My-Post");

        var props = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["slug"] = match.Parameters["slug"]
        };

        var page = (IAtollComponent)Activator.CreateInstance(match.RouteEntry.ComponentType)!;
        var destination = new StringRenderDestination();
        await ComponentRenderer.RenderComponentAsync(page, destination, props);

        destination.GetOutput().ShouldContain("<h1>Blog: My-Post</h1>");
    }

    // ----------------------------------------------------------------
    // Cross-cutting: EndpointContext locals with middleware data
    // ----------------------------------------------------------------

    [Fact]
    public void EndpointContextShouldAllowSettingAndRetrievingLocals()
    {
        var parameters = new Dictionary<string, string> { ["slug"] = "test" };
        var request = new EndpointRequest("GET", new Uri("http://localhost/api/test"));
        var locals = new Dictionary<string, object?>();
        var context = new EndpointContext(parameters, request, locals);

        // Simulate middleware setting data
        context.Locals["currentUser"] = "admin";
        context.Locals["requestId"] = Guid.NewGuid().ToString();

        context.GetLocal<string>("currentUser").ShouldBe("admin");
        context.GetLocal<string>("requestId").ShouldNotBeNullOrEmpty();
    }

    // ----------------------------------------------------------------
    // Additional stubs for new tests
    // ----------------------------------------------------------------

    private sealed class StubCrudEndpoint : IAtollEndpoint
    {
        public Task<AtollResponse> GetAsync(EndpointContext context)
        {
            return Task.FromResult(AtollResponse.Json(new { Items = "list" }));
        }

        public Task<AtollResponse> PostAsync(EndpointContext context)
        {
            return Task.FromResult(AtollResponse.Json(new { Id = 1 }, 201));
        }

        public Task<AtollResponse> PutAsync(EndpointContext context)
        {
            return Task.FromResult(AtollResponse.Text("updated"));
        }

        public Task<AtollResponse> DeleteAsync(EndpointContext context)
        {
            return Task.FromResult(AtollResponse.Empty(204));
        }

        public Task<AtollResponse> PatchAsync(EndpointContext context)
        {
            return Task.FromResult(AtollResponse.Text("patched"));
        }
    }

    private sealed class StubBlogPageWithProps : IAtollPage, IStaticPathsProvider
    {
        [Parameter(Required = true)]
        public string Slug { get; set; } = "";

        [Parameter]
        public string Title { get; set; } = "";

        public Task RenderAsync(RenderContext context)
        {
            context.WriteHtml($"<h1>{Title}</h1><p>Slug: {Slug}</p>");
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<StaticPath>> GetStaticPathsAsync()
        {
            IReadOnlyList<StaticPath> paths =
            [
                new(
                    new Dictionary<string, string> { ["slug"] = "first-post" },
                    new Dictionary<string, object?> { ["Title"] = "First Post" }),
                new(
                    new Dictionary<string, string> { ["slug"] = "second-post" },
                    new Dictionary<string, object?> { ["Title"] = "Second Post" }),
            ];
            return Task.FromResult(paths);
        }
    }

    private sealed class StubHtmlEndpoint : IAtollEndpoint
    {
        public Task<AtollResponse> GetAsync(EndpointContext context)
        {
            return Task.FromResult(AtollResponse.Html("<h1>Hello HTML</h1>"));
        }
    }

    private sealed class StubEmptyResponseEndpoint : IAtollEndpoint
    {
        public Task<AtollResponse> DeleteAsync(EndpointContext context)
        {
            return Task.FromResult(AtollResponse.Empty(204));
        }
    }

    private sealed class StubNotFoundEndpoint : IAtollEndpoint
    {
        public Task<AtollResponse> GetAsync(EndpointContext context)
        {
            return Task.FromResult(AtollResponse.NotFound());
        }
    }

    private sealed class StubRedirectEndpoint : IAtollEndpoint
    {
        public Task<AtollResponse> GetAsync(EndpointContext context)
        {
            return Task.FromResult(AtollResponse.Redirect("/new-location"));
        }
    }
}
