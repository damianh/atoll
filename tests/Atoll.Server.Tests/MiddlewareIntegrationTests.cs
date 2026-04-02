using System.Collections.ObjectModel;
using System.Net;
using System.Text.Json;
using Atoll.Core.Components;
using Atoll.Routing;
using Atoll.Server.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

namespace Atoll.Server.Tests;

public sealed class MiddlewareIntegrationTests
{
    // ----------------------------------------------------------------
    // Test page components
    // ----------------------------------------------------------------

    private sealed class HomePage : AtollComponent, IAtollPage
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<html><head><title>Home</title></head><body><h1>Welcome</h1></body></html>");
            return Task.CompletedTask;
        }
    }

    private sealed class AboutPage : AtollComponent, IAtollPage
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<html><head><title>About</title></head><body><h1>About Us</h1></body></html>");
            return Task.CompletedTask;
        }
    }

    private sealed class BlogPostPage : AtollComponent, IAtollPage
    {
        [Parameter(Required = true)]
        public string Slug { get; set; } = "";

        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml($"<html><head><title>{Slug}</title></head><body><h1>Post: {Slug}</h1></body></html>");
            return Task.CompletedTask;
        }
    }

    private sealed class DocsPage : AtollComponent, IAtollPage
    {
        [Parameter]
        public string Rest { get; set; } = "";

        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml($"<html><body><h1>Docs: {Rest}</h1></body></html>");
            return Task.CompletedTask;
        }
    }

    // ----------------------------------------------------------------
    // Test layout components
    // ----------------------------------------------------------------

    private sealed class BaseLayout : AtollComponent, IAtollComponent
    {
        protected override async Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<html><head><title>Site</title></head><body><nav>Nav</nav><main>");
            await RenderSlotAsync();
            WriteHtml("</main><footer>Footer</footer></body></html>");
        }
    }

    [Layout(typeof(BaseLayout))]
    private sealed class LayoutPage : AtollComponent, IAtollPage
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<h1>Page with Layout</h1>");
            return Task.CompletedTask;
        }
    }

    // ----------------------------------------------------------------
    // Test endpoint components
    // ----------------------------------------------------------------

    private sealed class PostsEndpoint : IAtollEndpoint
    {
        public Task<AtollResponse> GetAsync(EndpointContext context)
        {
            var posts = new[] { new { Id = 1, Title = "Hello" }, new { Id = 2, Title = "World" } };
            return Task.FromResult(AtollResponse.Json(posts));
        }

        public Task<AtollResponse> PostAsync(EndpointContext context)
        {
            return Task.FromResult(AtollResponse.Json(new { Id = 3, Title = "Created" }, 201));
        }
    }

    private sealed class EchoEndpoint : IAtollEndpoint
    {
        public Task<AtollResponse> GetAsync(EndpointContext context)
        {
            var slug = context.GetParameter("slug");
            return Task.FromResult(AtollResponse.Json(new { Slug = slug }));
        }
    }

    private sealed class TextEndpoint : IAtollEndpoint
    {
        public Task<AtollResponse> GetAsync(EndpointContext context)
        {
            return Task.FromResult(AtollResponse.Text("Hello, plain text!"));
        }
    }

    private sealed class RedirectEndpoint : IAtollEndpoint
    {
        public Task<AtollResponse> GetAsync(EndpointContext context)
        {
            return Task.FromResult(AtollResponse.Redirect("/new-location"));
        }
    }

    // ----------------------------------------------------------------
    // Helper: create test host with routes
    // ----------------------------------------------------------------

    private static HttpClient CreateTestClient(
        params (string RelativeFilePath, Type ComponentType)[] routes)
    {
        var builder = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost.UseTestServer();
                webHost.ConfigureServices(services =>
                {
                    services.AddAtoll(options =>
                    {
                        foreach (var route in routes)
                        {
                            options.RouteEntries.Add(route);
                        }
                    });
                    services.AddLogging();
                });
                webHost.Configure(app =>
                {
                    app.UseAtoll();
                });
            });

        var host = builder.Start();
        return host.GetTestClient();
    }

    private static HttpClient CreateTestClientWithBasePath(
        string basePath,
        params (string RelativeFilePath, Type ComponentType)[] routes)
    {
        var builder = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost.UseTestServer();
                webHost.ConfigureServices(services =>
                {
                    services.AddAtoll(options =>
                    {
                        options.BasePath = basePath;
                        foreach (var route in routes)
                        {
                            options.RouteEntries.Add(route);
                        }
                    });
                    services.AddLogging();
                });
                webHost.Configure(app =>
                {
                    app.UseAtoll();
                });
            });

        var host = builder.Start();
        return host.GetTestClient();
    }

    // ================================================================
    // Page Rendering Tests
    // ================================================================

    [Fact]
    public async Task ShouldRenderHomePageAtRoot()
    {
        using var client = CreateTestClient(("index.cs", typeof(HomePage)));

        var response = await client.GetAsync("/");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.ShouldBe("text/html");
        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain("<!DOCTYPE html>");
        body.ShouldContain("<h1>Welcome</h1>");
    }

    [Fact]
    public async Task ShouldRenderStaticPageAtPath()
    {
        using var client = CreateTestClient(
            ("index.cs", typeof(HomePage)),
            ("about.cs", typeof(AboutPage)));

        var response = await client.GetAsync("/about");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain("<h1>About Us</h1>");
    }

    [Fact]
    public async Task ShouldRenderDynamicPageWithRouteParams()
    {
        using var client = CreateTestClient(("blog/[slug].cs", typeof(BlogPostPage)));

        var response = await client.GetAsync("/blog/hello-world");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain("<h1>Post: hello-world</h1>");
        body.ShouldContain("<title>hello-world</title>");
    }

    [Fact]
    public async Task ShouldRenderCatchAllPageWithRouteParams()
    {
        using var client = CreateTestClient(("docs/[...rest].cs", typeof(DocsPage)));

        var response = await client.GetAsync("/docs/getting-started/install");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain("<h1>Docs: getting-started/install</h1>");
    }

    [Fact]
    public async Task ShouldAutoPrependDoctype()
    {
        using var client = CreateTestClient(("index.cs", typeof(HomePage)));

        var response = await client.GetAsync("/");
        var body = await response.Content.ReadAsStringAsync();

        body.ShouldStartWith("<!DOCTYPE html>");
    }

    [Fact]
    public async Task ShouldRenderPageWithLayout()
    {
        using var client = CreateTestClient(("layout-page.cs", typeof(LayoutPage)));

        var response = await client.GetAsync("/layout-page");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain("<nav>Nav</nav>");
        body.ShouldContain("<h1>Page with Layout</h1>");
        body.ShouldContain("<footer>Footer</footer>");
    }

    // ================================================================
    // Endpoint Dispatch Tests
    // ================================================================

    [Fact]
    public async Task ShouldDispatchGetToEndpoint()
    {
        using var client = CreateTestClient(("api/posts.cs", typeof(PostsEndpoint)));

        var response = await client.GetAsync("/api/posts");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.ShouldBe("application/json");

        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body);
        json.RootElement.GetArrayLength().ShouldBe(2);
        json.RootElement[0].GetProperty("id").GetInt32().ShouldBe(1);
        json.RootElement[0].GetProperty("title").GetString().ShouldBe("Hello");
    }

    [Fact]
    public async Task ShouldDispatchPostToEndpoint()
    {
        using var client = CreateTestClient(("api/posts.cs", typeof(PostsEndpoint)));

        var response = await client.PostAsync("/api/posts", null);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body);
        json.RootElement.GetProperty("id").GetInt32().ShouldBe(3);
    }

    [Fact]
    public async Task ShouldReturn405ForUnsupportedMethod()
    {
        using var client = CreateTestClient(("api/posts.cs", typeof(PostsEndpoint)));

        var response = await client.DeleteAsync("/api/posts");

        response.StatusCode.ShouldBe(HttpStatusCode.MethodNotAllowed);
        // The Allow header is classified as a content header by HttpClient
        response.Content.Headers.TryGetValues("Allow", out var allowValues).ShouldBeTrue();
        var allow = string.Join(", ", allowValues!);
        allow.ShouldContain("GET");
        allow.ShouldContain("POST");
    }

    [Fact]
    public async Task ShouldPassRouteParamsToEndpoint()
    {
        using var client = CreateTestClient(("api/echo/[slug].cs", typeof(EchoEndpoint)));

        var response = await client.GetAsync("/api/echo/test-slug");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body);
        json.RootElement.GetProperty("slug").GetString().ShouldBe("test-slug");
    }

    [Fact]
    public async Task ShouldReturnTextResponse()
    {
        using var client = CreateTestClient(("api/text.cs", typeof(TextEndpoint)));

        var response = await client.GetAsync("/api/text");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.ShouldBe("text/plain");
        var body = await response.Content.ReadAsStringAsync();
        body.ShouldBe("Hello, plain text!");
    }

    [Fact]
    public async Task ShouldReturnRedirectResponse()
    {
        var builder = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost.UseTestServer();
                webHost.ConfigureServices(services =>
                {
                    services.AddAtoll(options =>
                    {
                        options.RouteEntries.Add(("api/redirect.cs", typeof(RedirectEndpoint)));
                    });
                    services.AddLogging();
                });
                webHost.Configure(app =>
                {
                    app.UseAtoll();
                });
            });

        var host = builder.Start();
        var client = host.GetTestClient();
        // Disable auto-follow redirects to inspect the redirect response
        var handler = host.GetTestServer().CreateHandler();
        using var noRedirectClient = new HttpClient(handler) { BaseAddress = client.BaseAddress };

        var response = await noRedirectClient.GetAsync("/api/redirect");

        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        response.Headers.Location!.ToString().ShouldBe("/new-location");
    }

    // ================================================================
    // Route Matching / 404 Tests
    // ================================================================

    [Fact]
    public async Task ShouldReturn404ForUnmatchedRoute()
    {
        using var client = CreateTestClient(("index.cs", typeof(HomePage)));

        var response = await client.GetAsync("/nonexistent");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldPassThroughToNextMiddlewareForUnmatchedRoute()
    {
        var builder = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost.UseTestServer();
                webHost.ConfigureServices(services =>
                {
                    services.AddAtoll(options =>
                    {
                        options.RouteEntries.Add(("index.cs", typeof(HomePage)));
                    });
                    services.AddLogging();
                });
                webHost.Configure(app =>
                {
                    app.UseAtoll();
                    // Fallback middleware that catches unmatched requests
                    app.Run(async ctx =>
                    {
                        ctx.Response.StatusCode = 418; // I'm a teapot
                        await ctx.Response.WriteAsync("Fallback handler");
                    });
                });
            });

        var host = builder.Start();
        using var client = host.GetTestClient();

        var response = await client.GetAsync("/not-an-atoll-route");

        response.StatusCode.ShouldBe((HttpStatusCode)418);
        var body = await response.Content.ReadAsStringAsync();
        body.ShouldBe("Fallback handler");
    }

    // ================================================================
    // Multiple Routes Tests
    // ================================================================

    [Fact]
    public async Task ShouldHandleMultipleRoutes()
    {
        using var client = CreateTestClient(
            ("index.cs", typeof(HomePage)),
            ("about.cs", typeof(AboutPage)),
            ("blog/[slug].cs", typeof(BlogPostPage)),
            ("api/posts.cs", typeof(PostsEndpoint)));

        var homeResponse = await client.GetAsync("/");
        homeResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await homeResponse.Content.ReadAsStringAsync()).ShouldContain("<h1>Welcome</h1>");

        var aboutResponse = await client.GetAsync("/about");
        aboutResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await aboutResponse.Content.ReadAsStringAsync()).ShouldContain("<h1>About Us</h1>");

        var blogResponse = await client.GetAsync("/blog/my-post");
        blogResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await blogResponse.Content.ReadAsStringAsync()).ShouldContain("<h1>Post: my-post</h1>");

        var apiResponse = await client.GetAsync("/api/posts");
        apiResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var apiBody = await apiResponse.Content.ReadAsStringAsync();
        apiBody.ShouldContain("Hello");
    }

    // ================================================================
    // Base Path Tests
    // ================================================================

    [Fact]
    public async Task ShouldRespectBasePath()
    {
        using var client = CreateTestClientWithBasePath(
            "/docs",
            ("index.cs", typeof(HomePage)),
            ("about.cs", typeof(AboutPage)));

        var response = await client.GetAsync("/docs");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).ShouldContain("<h1>Welcome</h1>");
    }

    [Fact]
    public async Task ShouldRenderSubPageUnderBasePath()
    {
        using var client = CreateTestClientWithBasePath(
            "/docs",
            ("index.cs", typeof(HomePage)),
            ("about.cs", typeof(AboutPage)));

        var response = await client.GetAsync("/docs/about");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).ShouldContain("<h1>About Us</h1>");
    }

    [Fact]
    public async Task ShouldNotMatchRoutesOutsideBasePath()
    {
        var builder = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost.UseTestServer();
                webHost.ConfigureServices(services =>
                {
                    services.AddAtoll(options =>
                    {
                        options.BasePath = "/docs";
                        options.RouteEntries.Add(("index.cs", typeof(HomePage)));
                    });
                    services.AddLogging();
                });
                webHost.Configure(app =>
                {
                    app.UseAtoll();
                    app.Run(async ctx =>
                    {
                        ctx.Response.StatusCode = 418;
                        await ctx.Response.WriteAsync("Not Atoll");
                    });
                });
            });

        var host = builder.Start();
        using var client = host.GetTestClient();

        // Request outside base path should fall through
        var response = await client.GetAsync("/other");

        response.StatusCode.ShouldBe((HttpStatusCode)418);
    }

    // ================================================================
    // Content-Type Tests
    // ================================================================

    [Fact]
    public async Task ShouldSetHtmlContentTypeForPages()
    {
        using var client = CreateTestClient(("index.cs", typeof(HomePage)));

        var response = await client.GetAsync("/");

        response.Content.Headers.ContentType!.MediaType.ShouldBe("text/html");
        response.Content.Headers.ContentType!.CharSet.ShouldBe("utf-8");
    }

    [Fact]
    public async Task ShouldSetJsonContentTypeForJsonEndpoints()
    {
        using var client = CreateTestClient(("api/posts.cs", typeof(PostsEndpoint)));

        var response = await client.GetAsync("/api/posts");

        response.Content.Headers.ContentType!.MediaType.ShouldBe("application/json");
    }

    // ================================================================
    // Edge Case Tests
    // ================================================================

    [Fact]
    public async Task ShouldHandleTrailingSlash()
    {
        using var client = CreateTestClient(("about.cs", typeof(AboutPage)));

        // URL with trailing slash should still match
        var response = await client.GetAsync("/about/");

        // Route matcher may or may not match trailing slash — check that it doesn't crash
        // The current implementation strips trailing slashes in SplitPath
        response.StatusCode.ShouldBeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldHandleEmptyRouteTable()
    {
        var builder = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost.UseTestServer();
                webHost.ConfigureServices(services =>
                {
                    services.AddAtoll(options => { });
                    services.AddLogging();
                });
                webHost.Configure(app =>
                {
                    app.UseAtoll();
                    app.Run(async ctx =>
                    {
                        ctx.Response.StatusCode = 418;
                        await ctx.Response.WriteAsync("Fallback");
                    });
                });
            });

        var host = builder.Start();
        using var client = host.GetTestClient();

        var response = await client.GetAsync("/anything");

        response.StatusCode.ShouldBe((HttpStatusCode)418);
    }

    [Fact]
    public async Task ShouldHandleConcurrentRequests()
    {
        using var client = CreateTestClient(
            ("index.cs", typeof(HomePage)),
            ("blog/[slug].cs", typeof(BlogPostPage)));

        var tasks = Enumerable.Range(0, 20).Select(i =>
        {
            if (i % 2 == 0)
            {
                return client.GetAsync("/");
            }
            return client.GetAsync($"/blog/post-{i}");
        });

        var responses = await Task.WhenAll(tasks);

        foreach (var response in responses)
        {
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }
    }

    [Fact]
    public async Task ShouldRenderDifferentDynamicSlugValues()
    {
        using var client = CreateTestClient(("blog/[slug].cs", typeof(BlogPostPage)));

        var response1 = await client.GetAsync("/blog/first-post");
        var response2 = await client.GetAsync("/blog/second-post");

        (await response1.Content.ReadAsStringAsync()).ShouldContain("<h1>Post: first-post</h1>");
        (await response2.Content.ReadAsStringAsync()).ShouldContain("<h1>Post: second-post</h1>");
    }

    // ================================================================
    // Coexistence with existing ASP.NET Core endpoints
    // ================================================================

    [Fact]
    public async Task ShouldCoexistWithAspNetCoreEndpoints()
    {
        var builder = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost.UseTestServer();
                webHost.ConfigureServices(services =>
                {
                    services.AddAtoll(options =>
                    {
                        options.RouteEntries.Add(("index.cs", typeof(HomePage)));
                    });
                    services.AddRouting();
                    services.AddLogging();
                });
                webHost.Configure(app =>
                {
                    app.UseRouting();
                    app.UseAtoll();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapGet("/aspnet-route", async ctx =>
                        {
                            await ctx.Response.WriteAsync("ASP.NET Core endpoint");
                        });
                    });
                });
            });

        var host = builder.Start();
        using var client = host.GetTestClient();

        // Atoll page
        var atollResponse = await client.GetAsync("/");
        atollResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await atollResponse.Content.ReadAsStringAsync()).ShouldContain("<h1>Welcome</h1>");

        // ASP.NET Core endpoint
        var aspnetResponse = await client.GetAsync("/aspnet-route");
        aspnetResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await aspnetResponse.Content.ReadAsStringAsync()).ShouldBe("ASP.NET Core endpoint");
    }

    // ================================================================
    // AddAtoll configuration tests
    // ================================================================

    [Fact]
    public async Task ShouldConfigureRouteEntriesViaOptions()
    {
        using var client = CreateTestClient(("custom/path.cs", typeof(HomePage)));

        var response = await client.GetAsync("/custom/path");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).ShouldContain("<h1>Welcome</h1>");
    }

    [Fact]
    public async Task ShouldConfigureSiteUrl()
    {
        var builder = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost.UseTestServer();
                webHost.ConfigureServices(services =>
                {
                    services.AddAtoll(options =>
                    {
                        options.SiteUrl = new Uri("https://example.com");
                        options.RouteEntries.Add(("index.cs", typeof(HomePage)));
                    });
                    services.AddLogging();
                });
                webHost.Configure(app =>
                {
                    app.UseAtoll();
                });
            });

        var host = builder.Start();
        using var client = host.GetTestClient();

        // Verify the app still works (SiteUrl is stored for future use)
        var response = await client.GetAsync("/");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    // ================================================================
    // Response Headers Tests
    // ================================================================

    [Fact]
    public async Task ShouldForwardEndpointResponseHeaders()
    {
        using var client = CreateTestClient(("api/posts.cs", typeof(PostsEndpoint)));

        var response = await client.GetAsync("/api/posts");

        response.Content.Headers.ContentType!.ToString().ShouldContain("application/json");
    }

    [Fact]
    public async Task ShouldForwardCustomHeadersFromEndpoint()
    {
        using var client = CreateTestClient(("api/custom.cs", typeof(CustomHeaderEndpoint)));

        var response = await client.GetAsync("/api/custom");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Headers.TryGetValues("X-Custom-Header", out var values).ShouldBeTrue();
        string.Join("", values!).ShouldBe("custom-value");
    }

    // ================================================================
    // Async Page Through Middleware Tests
    // ================================================================

    [Fact]
    public async Task ShouldRenderAsyncPageThroughMiddleware()
    {
        using var client = CreateTestClient(("async/[id].cs", typeof(AsyncDataPage)));

        var response = await client.GetAsync("/async/42");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain("<p>Loaded item: 42</p>");
    }

    // ================================================================
    // Nested Layout Through Middleware Tests
    // ================================================================

    [Fact]
    public async Task ShouldRenderPageWithNestedLayoutsThroughMiddleware()
    {
        using var client = CreateTestClient(("nested.cs", typeof(NestedLayoutPage)));

        var response = await client.GetAsync("/nested");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain("<!DOCTYPE html>");
        body.ShouldContain("<header>Outer Header</header>");
        body.ShouldContain("<aside>Inner Sidebar</aside>");
        body.ShouldContain("<article>Nested Content</article>");
        body.ShouldContain("</aside>"); // Inner layout close
        body.ShouldContain("<footer>Outer Footer</footer>");
    }

    // ================================================================
    // Catch-All With Empty Remainder Tests
    // ================================================================

    [Fact]
    public async Task ShouldRenderCatchAllWithEmptyRemainder()
    {
        using var client = CreateTestClient(("docs/[...rest].cs", typeof(DocsPage)));

        var response = await client.GetAsync("/docs");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain("<h1>Docs: </h1>");
    }

    [Fact]
    public async Task ShouldRenderCatchAllWithMultiSegmentRemainder()
    {
        using var client = CreateTestClient(("docs/[...rest].cs", typeof(DocsPage)));

        var response = await client.GetAsync("/docs/guides/advanced/topics");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain("<h1>Docs: guides/advanced/topics</h1>");
    }

    // ================================================================
    // Additional Phase 2 test page/endpoint stubs
    // ================================================================

    private sealed class CustomHeaderEndpoint : IAtollEndpoint
    {
        public Task<AtollResponse> GetAsync(EndpointContext context)
        {
            var headers = new Dictionary<string, string>
            {
                ["Content-Type"] = "application/json; charset=utf-8",
                ["X-Custom-Header"] = "custom-value"
            };
            var body = System.Text.Encoding.UTF8.GetBytes("{\"ok\":true}");
            return Task.FromResult(new AtollResponse(200, new ReadOnlyDictionary<string, string>(headers), body));
        }
    }

    private sealed class AsyncDataPage : AtollComponent, IAtollPage
    {
        [Parameter(Required = true)]
        public string Id { get; set; } = "";

        protected override async Task RenderCoreAsync(RenderContext context)
        {
            // Force async code path
            await Task.Yield();
            WriteHtml($"<html><body><p>Loaded item: {Id}</p></body></html>");
        }
    }

    private sealed class OuterLayout : AtollComponent
    {
        protected override async Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<html><body><header>Outer Header</header>");
            await RenderSlotAsync();
            WriteHtml("<footer>Outer Footer</footer></body></html>");
        }
    }

    [Layout(typeof(OuterLayout))]
    private sealed class InnerLayout : AtollComponent
    {
        protected override async Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<aside>Inner Sidebar</aside><div>");
            await RenderSlotAsync();
            WriteHtml("</div>");
        }
    }

    [Layout(typeof(InnerLayout))]
    private sealed class NestedLayoutPage : AtollComponent, IAtollPage
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<article>Nested Content</article>");
            return Task.CompletedTask;
        }
    }

    // ================================================================
    // PUT/PATCH/HEAD/OPTIONS Through Middleware Tests
    // ================================================================

    [Fact]
    public async Task ShouldDispatchPutToEndpoint()
    {
        using var client = CreateTestClient(("api/items.cs", typeof(CrudEndpoint)));

        var response = await client.PutAsync("/api/items", null);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.ShouldBe("updated");
    }

    [Fact]
    public async Task ShouldDispatchPatchToEndpoint()
    {
        using var client = CreateTestClient(("api/items.cs", typeof(CrudEndpoint)));

        var request = new HttpRequestMessage(HttpMethod.Patch, "/api/items");
        var response = await client.SendAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.ShouldBe("patched");
    }

    [Fact]
    public async Task ShouldDispatchHeadToEndpoint()
    {
        using var client = CreateTestClient(("api/health.cs", typeof(HeadOptionsEndpoint)));

        var request = new HttpRequestMessage(HttpMethod.Head, "/api/health");
        var response = await client.SendAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ShouldDispatchOptionsToEndpoint()
    {
        using var client = CreateTestClient(("api/health.cs", typeof(HeadOptionsEndpoint)));

        var request = new HttpRequestMessage(HttpMethod.Options, "/api/health");
        var response = await client.SendAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    // ================================================================
    // Base Path With Dynamic Routes Tests
    // ================================================================

    [Fact]
    public async Task ShouldRenderDynamicPageUnderBasePath()
    {
        using var client = CreateTestClientWithBasePath(
            "/app",
            ("blog/[slug].cs", typeof(BlogPostPage)));

        var response = await client.GetAsync("/app/blog/my-post");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain("<h1>Post: my-post</h1>");
    }

    [Fact]
    public async Task ShouldDispatchEndpointUnderBasePath()
    {
        using var client = CreateTestClientWithBasePath(
            "/api/v2",
            ("posts.cs", typeof(PostsEndpoint)));

        var response = await client.GetAsync("/api/v2/posts");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.ShouldBe("application/json");
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body);
        json.RootElement.GetArrayLength().ShouldBe(2);
    }

    [Fact]
    public async Task ShouldNotMatchDynamicRouteOutsideBasePath()
    {
        var builder = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost.UseTestServer();
                webHost.ConfigureServices(services =>
                {
                    services.AddAtoll(options =>
                    {
                        options.BasePath = "/app";
                        options.RouteEntries.Add(("blog/[slug].cs", typeof(BlogPostPage)));
                    });
                    services.AddLogging();
                });
                webHost.Configure(app =>
                {
                    app.UseAtoll();
                    app.Run(async ctx =>
                    {
                        ctx.Response.StatusCode = 418;
                        await ctx.Response.WriteAsync("Not Atoll");
                    });
                });
            });

        var host = builder.Start();
        using var client = host.GetTestClient();

        // Request outside base path should fall through
        var response = await client.GetAsync("/blog/my-post");

        response.StatusCode.ShouldBe((HttpStatusCode)418);
    }

    // ================================================================
    // Multiple Dynamic Segments Through Middleware Tests
    // ================================================================

    [Fact]
    public async Task ShouldRenderPageWithMultipleDynamicSegments()
    {
        using var client = CreateTestClient(("blog/[year]/[slug].cs", typeof(YearSlugPage)));

        var response = await client.GetAsync("/blog/2024/my-post");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain("Year: 2024");
        body.ShouldContain("Post: my-post");
    }

    // ================================================================
    // Endpoint Returning Different Response Types Through Middleware
    // ================================================================

    [Fact]
    public async Task ShouldReturnHtmlResponseFromEndpoint()
    {
        using var client = CreateTestClient(("api/html.cs", typeof(HtmlEndpoint)));

        var response = await client.GetAsync("/api/html");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.ShouldBe("text/html");
        var body = await response.Content.ReadAsStringAsync();
        body.ShouldBe("<h1>Hello HTML</h1>");
    }

    [Fact]
    public async Task ShouldReturnEmptyResponseFromEndpoint()
    {
        using var client = CreateTestClient(("api/empty.cs", typeof(EmptyEndpoint)));

        var response = await client.DeleteAsync("/api/empty");

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ShouldReturnNotFoundFromEndpoint()
    {
        using var client = CreateTestClient(("api/missing.cs", typeof(NotFoundEndpoint)));

        var response = await client.GetAsync("/api/missing");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    // ================================================================
    // Additional stubs for new tests
    // ================================================================

    private sealed class CrudEndpoint : IAtollEndpoint
    {
        public Task<AtollResponse> GetAsync(EndpointContext context)
        {
            return Task.FromResult(AtollResponse.Json(new[] { "item1" }));
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

    private sealed class HeadOptionsEndpoint : IAtollEndpoint
    {
        public Task<AtollResponse> HeadAsync(EndpointContext context)
        {
            return Task.FromResult(AtollResponse.Empty(200));
        }

        public Task<AtollResponse> OptionsAsync(EndpointContext context)
        {
            return Task.FromResult(AtollResponse.Empty(204));
        }
    }

    private sealed class YearSlugPage : AtollComponent, IAtollPage
    {
        [Parameter(Required = true)]
        public string Year { get; set; } = "";

        [Parameter(Required = true)]
        public string Slug { get; set; } = "";

        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml($"<html><body><p>Year: {Year}, Post: {Slug}</p></body></html>");
            return Task.CompletedTask;
        }
    }

    private sealed class HtmlEndpoint : IAtollEndpoint
    {
        public Task<AtollResponse> GetAsync(EndpointContext context)
        {
            return Task.FromResult(AtollResponse.Html("<h1>Hello HTML</h1>"));
        }
    }

    private sealed class EmptyEndpoint : IAtollEndpoint
    {
        public Task<AtollResponse> DeleteAsync(EndpointContext context)
        {
            return Task.FromResult(AtollResponse.Empty(204));
        }
    }

    private sealed class NotFoundEndpoint : IAtollEndpoint
    {
        public Task<AtollResponse> GetAsync(EndpointContext context)
        {
            return Task.FromResult(AtollResponse.NotFound());
        }
    }
}
