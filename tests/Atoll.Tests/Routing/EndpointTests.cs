using System.Text;
using System.Text.Json;
using Atoll.Routing.FileSystem;
using Atoll.Routing.Matching;

namespace Atoll.Routing.Tests;

public sealed class EndpointTests
{
    // ---- IAtollEndpoint interface tests ----

    [Fact]
    public void EndpointShouldImplementInterface()
    {
        var endpoint = new StubPostsEndpoint();
        (endpoint is IAtollEndpoint).ShouldBeTrue();
    }

    // ---- EndpointRequest tests ----

    [Fact]
    public void RequestShouldStoreMethodAndUrl()
    {
        var url = new Uri("http://localhost/api/posts");
        var request = new EndpointRequest("GET", url);

        request.Method.ShouldBe("GET");
        request.Url.ShouldBe(url);
        request.Headers.ShouldBeEmpty();
        request.Body.ShouldBeNull();
    }

    [Fact]
    public void RequestShouldNormalizeMethodToUppercase()
    {
        var url = new Uri("http://localhost/api/posts");
        var request = new EndpointRequest("post", url);

        request.Method.ShouldBe("POST");
    }

    [Fact]
    public void RequestShouldStoreHeaders()
    {
        var url = new Uri("http://localhost/api/posts");
        var headers = new Dictionary<string, string>
        {
            ["Authorization"] = "Bearer token123",
            ["Content-Type"] = "application/json"
        };
        var request = new EndpointRequest("GET", url, headers);

        request.Headers["Authorization"].ShouldBe("Bearer token123");
        request.Headers["Content-Type"].ShouldBe("application/json");
        request.Body.ShouldBeNull();
    }

    [Fact]
    public void RequestShouldStoreBody()
    {
        var url = new Uri("http://localhost/api/posts");
        var headers = new Dictionary<string, string>();
        var body = new MemoryStream(Encoding.UTF8.GetBytes("{\"title\":\"New Post\"}"));
        var request = new EndpointRequest("POST", url, headers, body);

        request.Body.ShouldNotBeNull();
        request.Body.ShouldBeSameAs(body);
    }

    [Fact]
    public void RequestShouldThrowForNullMethod()
    {
        var url = new Uri("http://localhost/api/posts");
        Should.Throw<ArgumentNullException>(() => new EndpointRequest(null!, url));
    }

    [Fact]
    public void RequestShouldThrowForNullUrl()
    {
        Should.Throw<ArgumentNullException>(() => new EndpointRequest("GET", null!));
    }

    [Fact]
    public void RequestShouldThrowForNullHeaders()
    {
        var url = new Uri("http://localhost/api/posts");
        Should.Throw<ArgumentNullException>(() => new EndpointRequest("GET", url, null!));
    }

    // ---- EndpointContext tests ----

    [Fact]
    public void ContextShouldStoreParametersAndRequest()
    {
        var parameters = new Dictionary<string, string> { ["slug"] = "hello-world" };
        var request = new EndpointRequest("GET", new Uri("http://localhost/api/posts/hello-world"));
        var context = new EndpointContext(parameters, request);

        context.Parameters["slug"].ShouldBe("hello-world");
        context.Request.Method.ShouldBe("GET");
        context.Locals.ShouldBeEmpty();
    }

    [Fact]
    public void ContextShouldStoreLocals()
    {
        var parameters = new Dictionary<string, string>();
        var request = new EndpointRequest("GET", new Uri("http://localhost/api"));
        var locals = new Dictionary<string, object?> { ["userId"] = 42 };
        var context = new EndpointContext(parameters, request, locals);

        context.Locals["userId"].ShouldBe(42);
    }

    [Fact]
    public void ContextShouldCreateWithRequestOnly()
    {
        var request = new EndpointRequest("GET", new Uri("http://localhost/api"));
        var context = new EndpointContext(request);

        context.Parameters.ShouldBeEmpty();
        context.Locals.ShouldBeEmpty();
        context.Request.ShouldBe(request);
    }

    [Fact]
    public void ContextGetParameterShouldReturnValue()
    {
        var parameters = new Dictionary<string, string> { ["slug"] = "my-post" };
        var request = new EndpointRequest("GET", new Uri("http://localhost/api"));
        var context = new EndpointContext(parameters, request);

        context.GetParameter("slug").ShouldBe("my-post");
    }

    [Fact]
    public void ContextGetParameterShouldThrowForMissing()
    {
        var request = new EndpointRequest("GET", new Uri("http://localhost/api"));
        var context = new EndpointContext(request);

        Should.Throw<KeyNotFoundException>(() => context.GetParameter("slug"));
    }

    [Fact]
    public void ContextGetParameterShouldThrowForNull()
    {
        var request = new EndpointRequest("GET", new Uri("http://localhost/api"));
        var context = new EndpointContext(request);

        Should.Throw<ArgumentNullException>(() => context.GetParameter(null!));
    }

    [Fact]
    public void ContextGetLocalShouldReturnTypedValue()
    {
        var request = new EndpointRequest("GET", new Uri("http://localhost/api"));
        var locals = new Dictionary<string, object?> { ["userId"] = 42 };
        var context = new EndpointContext(new Dictionary<string, string>(), request, locals);

        context.GetLocal<int>("userId").ShouldBe(42);
    }

    [Fact]
    public void ContextGetLocalShouldThrowForMissing()
    {
        var request = new EndpointRequest("GET", new Uri("http://localhost/api"));
        var context = new EndpointContext(request);

        Should.Throw<KeyNotFoundException>(() => context.GetLocal<int>("userId"));
    }

    [Fact]
    public void ContextGetLocalShouldThrowForNullKey()
    {
        var request = new EndpointRequest("GET", new Uri("http://localhost/api"));
        var context = new EndpointContext(request);

        Should.Throw<ArgumentNullException>(() => context.GetLocal<int>(null!));
    }

    [Fact]
    public void ContextGetLocalWithDefaultShouldReturnValueWhenPresent()
    {
        var request = new EndpointRequest("GET", new Uri("http://localhost/api"));
        var locals = new Dictionary<string, object?> { ["userId"] = 42 };
        var context = new EndpointContext(new Dictionary<string, string>(), request, locals);

        context.GetLocal("userId", 0).ShouldBe(42);
    }

    [Fact]
    public void ContextGetLocalWithDefaultShouldReturnDefaultWhenMissing()
    {
        var request = new EndpointRequest("GET", new Uri("http://localhost/api"));
        var context = new EndpointContext(request);

        context.GetLocal("userId", -1).ShouldBe(-1);
    }

    [Fact]
    public void ContextGetLocalWithDefaultShouldReturnDefaultForTypeMismatch()
    {
        var request = new EndpointRequest("GET", new Uri("http://localhost/api"));
        var locals = new Dictionary<string, object?> { ["userId"] = "not-an-int" };
        var context = new EndpointContext(new Dictionary<string, string>(), request, locals);

        context.GetLocal("userId", -1).ShouldBe(-1);
    }

    [Fact]
    public void ContextGetLocalWithDefaultShouldThrowForNullKey()
    {
        var request = new EndpointRequest("GET", new Uri("http://localhost/api"));
        var context = new EndpointContext(request);

        Should.Throw<ArgumentNullException>(() => context.GetLocal(null!, 0));
    }

    [Fact]
    public void ContextShouldThrowForNullParameters()
    {
        var request = new EndpointRequest("GET", new Uri("http://localhost/api"));
        Should.Throw<ArgumentNullException>(() => new EndpointContext(null!, request));
    }

    [Fact]
    public void ContextShouldThrowForNullRequest()
    {
        var parameters = new Dictionary<string, string>();
        Should.Throw<ArgumentNullException>(() => new EndpointContext(parameters, null!));
    }

    [Fact]
    public void ContextShouldThrowForNullLocals()
    {
        var parameters = new Dictionary<string, string>();
        var request = new EndpointRequest("GET", new Uri("http://localhost/api"));
        Should.Throw<ArgumentNullException>(() => new EndpointContext(parameters, request, null!));
    }

    // ---- AtollResponse tests ----

    [Fact]
    public void ResponseShouldStoreStatusCodeAndHeaders()
    {
        var headers = new Dictionary<string, string> { ["X-Custom"] = "value" };
        var response = new AtollResponse(200, headers);

        response.StatusCode.ShouldBe(200);
        response.Headers["X-Custom"].ShouldBe("value");
        response.Body.ShouldBeNull();
    }

    [Fact]
    public void ResponseShouldStoreBody()
    {
        var body = Encoding.UTF8.GetBytes("hello");
        var headers = new Dictionary<string, string>();
        var response = new AtollResponse(200, headers, body);

        response.Body.ShouldBe(body);
        response.GetBodyAsString().ShouldBe("hello");
    }

    [Fact]
    public void ResponseGetBodyAsStringShouldReturnNullWhenNoBody()
    {
        var response = new AtollResponse(204);
        response.GetBodyAsString().ShouldBeNull();
    }

    [Fact]
    public void ResponseJsonShouldSerializeToJsonBody()
    {
        var data = new { Id = 1, Title = "Hello" };
        var response = AtollResponse.Json(data);

        response.StatusCode.ShouldBe(200);
        response.Headers["Content-Type"].ShouldBe("application/json; charset=utf-8");

        var bodyStr = response.GetBodyAsString()!;
        using var doc = JsonDocument.Parse(bodyStr);
        doc.RootElement.GetProperty("id").GetInt32().ShouldBe(1);
        doc.RootElement.GetProperty("title").GetString().ShouldBe("Hello");
    }

    [Fact]
    public void ResponseJsonShouldUseCamelCasePropertyNames()
    {
        var data = new { MyProperty = "value" };
        var response = AtollResponse.Json(data);

        var bodyStr = response.GetBodyAsString()!;
        bodyStr.ShouldContain("\"myProperty\"");
        // Verify the PascalCase version is NOT present (exact case check)
        bodyStr.IndexOf("\"MyProperty\"", StringComparison.Ordinal).ShouldBe(-1);
    }

    [Fact]
    public void ResponseJsonShouldAcceptCustomStatusCode()
    {
        var data = new { Id = 2 };
        var response = AtollResponse.Json(data, 201);

        response.StatusCode.ShouldBe(201);
    }

    [Fact]
    public void ResponseTextShouldCreateTextResponse()
    {
        var response = AtollResponse.Text("Hello World");

        response.StatusCode.ShouldBe(200);
        response.Headers["Content-Type"].ShouldBe("text/plain; charset=utf-8");
        response.GetBodyAsString().ShouldBe("Hello World");
    }

    [Fact]
    public void ResponseTextShouldAcceptCustomStatusCode()
    {
        var response = AtollResponse.Text("Error", 400);

        response.StatusCode.ShouldBe(400);
        response.GetBodyAsString().ShouldBe("Error");
    }

    [Fact]
    public void ResponseTextShouldThrowForNull()
    {
        Should.Throw<ArgumentNullException>(() => AtollResponse.Text(null!));
    }

    [Fact]
    public void ResponseHtmlShouldCreateHtmlResponse()
    {
        var response = AtollResponse.Html("<h1>Hello</h1>");

        response.StatusCode.ShouldBe(200);
        response.Headers["Content-Type"].ShouldBe("text/html; charset=utf-8");
        response.GetBodyAsString().ShouldBe("<h1>Hello</h1>");
    }

    [Fact]
    public void ResponseHtmlShouldAcceptCustomStatusCode()
    {
        var response = AtollResponse.Html("<h1>Error</h1>", 500);

        response.StatusCode.ShouldBe(500);
    }

    [Fact]
    public void ResponseHtmlShouldThrowForNull()
    {
        Should.Throw<ArgumentNullException>(() => AtollResponse.Html(null!));
    }

    [Fact]
    public void ResponseRedirectShouldSetLocationHeader()
    {
        var response = AtollResponse.Redirect("/new-page");

        response.StatusCode.ShouldBe(302);
        response.Headers["Location"].ShouldBe("/new-page");
        response.Body.ShouldBeNull();
    }

    [Fact]
    public void ResponseRedirectShouldAcceptCustomStatusCode()
    {
        var response = AtollResponse.Redirect("/permanent", 301);

        response.StatusCode.ShouldBe(301);
        response.Headers["Location"].ShouldBe("/permanent");
    }

    [Fact]
    public void ResponseRedirectShouldThrowForNull()
    {
        Should.Throw<ArgumentNullException>(() => AtollResponse.Redirect(null!));
    }

    [Fact]
    public void ResponseEmptyShouldCreateBodylessResponse()
    {
        var response = AtollResponse.Empty(204);

        response.StatusCode.ShouldBe(204);
        response.Body.ShouldBeNull();
    }

    [Fact]
    public void ResponseMethodNotAllowedShouldSetAllowHeader()
    {
        var allowed = new List<string> { "GET", "POST" };
        var response = AtollResponse.MethodNotAllowed(allowed);

        response.StatusCode.ShouldBe(405);
        response.Headers["Allow"].ShouldBe("GET, POST");
        response.Body.ShouldBeNull();
    }

    [Fact]
    public void ResponseMethodNotAllowedShouldThrowForNull()
    {
        Should.Throw<ArgumentNullException>(() => AtollResponse.MethodNotAllowed(null!));
    }

    [Fact]
    public void ResponseNotFoundShouldReturn404()
    {
        var response = AtollResponse.NotFound();

        response.StatusCode.ShouldBe(404);
        response.Body.ShouldBeNull();
    }

    [Fact]
    public void ResponseShouldThrowForNullHeaders()
    {
        Should.Throw<ArgumentNullException>(() => new AtollResponse(200, null!));
    }

    // ---- EndpointDispatcher tests ----

    [Fact]
    public async Task DispatcherShouldRouteGetToGetAsync()
    {
        var endpoint = new StubPostsEndpoint();
        var context = CreateContext("GET", "/api/posts");

        var response = await EndpointDispatcher.DispatchAsync(endpoint, context);

        response.StatusCode.ShouldBe(200);
        response.Headers["Content-Type"].ShouldBe("application/json; charset=utf-8");
        var body = response.GetBodyAsString()!;
        using var doc = JsonDocument.Parse(body);
        doc.RootElement.GetArrayLength().ShouldBe(2);
    }

    [Fact]
    public async Task DispatcherShouldRoutePostToPostAsync()
    {
        var endpoint = new StubPostsEndpoint();
        var context = CreateContext("POST", "/api/posts");

        var response = await EndpointDispatcher.DispatchAsync(endpoint, context);

        response.StatusCode.ShouldBe(201);
    }

    [Fact]
    public async Task DispatcherShouldReturn405ForUnsupportedMethod()
    {
        var endpoint = new StubGetOnlyEndpoint();
        var context = CreateContext("DELETE", "/api/items");

        var response = await EndpointDispatcher.DispatchAsync(endpoint, context);

        response.StatusCode.ShouldBe(405);
        response.Headers["Allow"].ShouldContain("GET");
    }

    [Fact]
    public async Task DispatcherShouldReturn405ForUnknownMethod()
    {
        var endpoint = new StubGetOnlyEndpoint();
        var context = CreateContext("TRACE", "/api/items");

        var response = await EndpointDispatcher.DispatchAsync(endpoint, context);

        response.StatusCode.ShouldBe(405);
    }

    [Fact]
    public async Task DispatcherShouldRoutePutToPutAsync()
    {
        var endpoint = new StubFullCrudEndpoint();
        var context = CreateContext("PUT", "/api/items/1");

        var response = await EndpointDispatcher.DispatchAsync(endpoint, context);

        response.StatusCode.ShouldBe(200);
        response.GetBodyAsString().ShouldBe("updated");
    }

    [Fact]
    public async Task DispatcherShouldRouteDeleteToDeleteAsync()
    {
        var endpoint = new StubFullCrudEndpoint();
        var context = CreateContext("DELETE", "/api/items/1");

        var response = await EndpointDispatcher.DispatchAsync(endpoint, context);

        response.StatusCode.ShouldBe(204);
    }

    [Fact]
    public async Task DispatcherShouldRoutePatchToPatchAsync()
    {
        var endpoint = new StubFullCrudEndpoint();
        var context = CreateContext("PATCH", "/api/items/1");

        var response = await EndpointDispatcher.DispatchAsync(endpoint, context);

        response.StatusCode.ShouldBe(200);
        response.GetBodyAsString().ShouldBe("patched");
    }

    [Fact]
    public async Task DispatcherShouldRouteHeadToHeadAsync()
    {
        var endpoint = new StubHeadOptionsEndpoint();
        var context = CreateContext("HEAD", "/api/health");

        var response = await EndpointDispatcher.DispatchAsync(endpoint, context);

        response.StatusCode.ShouldBe(200);
    }

    [Fact]
    public async Task DispatcherShouldRouteOptionsToOptionsAsync()
    {
        var endpoint = new StubHeadOptionsEndpoint();
        var context = CreateContext("OPTIONS", "/api/health");

        var response = await EndpointDispatcher.DispatchAsync(endpoint, context);

        response.StatusCode.ShouldBe(204);
    }

    [Fact]
    public void DispatcherShouldThrowForNullEndpoint()
    {
        var context = CreateContext("GET", "/api/posts");
        Should.ThrowAsync<ArgumentNullException>(
            () => EndpointDispatcher.DispatchAsync(null!, context));
    }

    [Fact]
    public void DispatcherShouldThrowForNullContext()
    {
        var endpoint = new StubPostsEndpoint();
        Should.ThrowAsync<ArgumentNullException>(
            () => EndpointDispatcher.DispatchAsync(endpoint, null!));
    }

    // ---- GetSupportedMethods tests ----

    [Fact]
    public void GetSupportedMethodsShouldReturnImplementedMethods()
    {
        var methods = EndpointDispatcher.GetSupportedMethods(typeof(StubPostsEndpoint));

        methods.ShouldContain("GET");
        methods.ShouldContain("POST");
        methods.Count.ShouldBe(2);
    }

    [Fact]
    public void GetSupportedMethodsShouldReturnGetOnlyForGetOnlyEndpoint()
    {
        var methods = EndpointDispatcher.GetSupportedMethods(typeof(StubGetOnlyEndpoint));

        methods.ShouldContain("GET");
        methods.Count.ShouldBe(1);
    }

    [Fact]
    public void GetSupportedMethodsShouldReturnAllForFullCrudEndpoint()
    {
        var methods = EndpointDispatcher.GetSupportedMethods(typeof(StubFullCrudEndpoint));

        methods.ShouldContain("GET");
        methods.ShouldContain("POST");
        methods.ShouldContain("PUT");
        methods.ShouldContain("DELETE");
        methods.ShouldContain("PATCH");
        methods.Count.ShouldBe(5);
    }

    [Fact]
    public void GetSupportedMethodsShouldReturnHeadAndOptionsForHeadOptionsEndpoint()
    {
        var methods = EndpointDispatcher.GetSupportedMethods(typeof(StubHeadOptionsEndpoint));

        methods.ShouldContain("HEAD");
        methods.ShouldContain("OPTIONS");
        methods.Count.ShouldBe(2);
    }

    [Fact]
    public void GetSupportedMethodsShouldReturnEmptyForEmptyEndpoint()
    {
        var methods = EndpointDispatcher.GetSupportedMethods(typeof(StubEmptyEndpoint));

        methods.ShouldBeEmpty();
    }

    [Fact]
    public void GetSupportedMethodsShouldThrowForNullType()
    {
        Should.Throw<ArgumentNullException>(() => EndpointDispatcher.GetSupportedMethods(null!));
    }

    // ---- Integration: Route discovery with IAtollEndpoint types ----

    [Fact]
    public void RouteDiscoveryShouldFindEndpointTypes()
    {
        var entries = new List<(string RelativeFilePath, Type ComponentType)>
        {
            ("api/posts.cs", typeof(StubPostsEndpoint)),
            ("api/posts/[slug].cs", typeof(StubDynamicEndpoint)),
        };

        var routes = RouteDiscovery.DiscoverRoutesFromEntries(entries);

        routes.Count.ShouldBe(2);
        routes[0].Pattern.ShouldBe("/api/posts");
        routes[0].ComponentType.ShouldBe(typeof(StubPostsEndpoint));
        routes[1].Pattern.ShouldBe("/api/posts/[slug]");
        routes[1].ComponentType.ShouldBe(typeof(StubDynamicEndpoint));
    }

    // ---- Integration: Route matching with endpoint dispatch ----

    [Fact]
    public async Task EndToEndEndpointShouldReturnJsonOnGet()
    {
        var entries = new List<(string RelativeFilePath, Type ComponentType)>
        {
            ("api/posts.cs", typeof(StubPostsEndpoint)),
        };

        var routes = RouteDiscovery.DiscoverRoutesFromEntries(entries);
        var matcher = new RouteMatcher(routes);

        var match = matcher.Match("/api/posts");
        match.ShouldNotBeNull();
        match.RouteEntry.ComponentType.ShouldBe(typeof(StubPostsEndpoint));

        var endpoint = (IAtollEndpoint)Activator.CreateInstance(match.RouteEntry.ComponentType)!;
        var context = new EndpointContext(
            match.Parameters,
            new EndpointRequest("GET", new Uri("http://localhost/api/posts")));

        var response = await EndpointDispatcher.DispatchAsync(endpoint, context);

        response.StatusCode.ShouldBe(200);
        response.Headers["Content-Type"].ShouldBe("application/json; charset=utf-8");

        var body = response.GetBodyAsString()!;
        using var doc = JsonDocument.Parse(body);
        doc.RootElement.GetArrayLength().ShouldBe(2);
    }

    [Fact]
    public async Task EndToEndDynamicEndpointShouldExtractSlugParameter()
    {
        var entries = new List<(string RelativeFilePath, Type ComponentType)>
        {
            ("api/posts/[slug].cs", typeof(StubDynamicEndpoint)),
        };

        var routes = RouteDiscovery.DiscoverRoutesFromEntries(entries);
        var matcher = new RouteMatcher(routes);

        var match = matcher.Match("/api/posts/hello-world");
        match.ShouldNotBeNull();
        match.Parameters["slug"].ShouldBe("hello-world");

        var endpoint = (IAtollEndpoint)Activator.CreateInstance(match.RouteEntry.ComponentType)!;
        var context = new EndpointContext(
            match.Parameters,
            new EndpointRequest("GET", new Uri("http://localhost/api/posts/hello-world")));

        var response = await EndpointDispatcher.DispatchAsync(endpoint, context);

        response.StatusCode.ShouldBe(200);
        var body = response.GetBodyAsString()!;
        using var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("slug").GetString().ShouldBe("hello-world");
    }

    [Fact]
    public async Task EndToEndEndpointShouldReturn405ForUnsupportedMethodOnMatchedRoute()
    {
        var entries = new List<(string RelativeFilePath, Type ComponentType)>
        {
            ("api/items.cs", typeof(StubGetOnlyEndpoint)),
        };

        var routes = RouteDiscovery.DiscoverRoutesFromEntries(entries);
        var matcher = new RouteMatcher(routes);

        var match = matcher.Match("/api/items");
        match.ShouldNotBeNull();

        var endpoint = (IAtollEndpoint)Activator.CreateInstance(match.RouteEntry.ComponentType)!;
        var context = new EndpointContext(
            match.Parameters,
            new EndpointRequest("POST", new Uri("http://localhost/api/items")));

        var response = await EndpointDispatcher.DispatchAsync(endpoint, context);

        response.StatusCode.ShouldBe(405);
        response.Headers["Allow"].ShouldBe("GET");
    }

    // ---- Integration: Endpoint with middleware locals ----

    [Fact]
    public async Task EndpointShouldAccessMiddlewareLocals()
    {
        var request = new EndpointRequest("GET", new Uri("http://localhost/api/secure"));
        var locals = new Dictionary<string, object?> { ["userId"] = "user-42" };
        var context = new EndpointContext(new Dictionary<string, string>(), request, locals);

        var endpoint = new StubSecureEndpoint();
        var response = await EndpointDispatcher.DispatchAsync(endpoint, context);

        response.StatusCode.ShouldBe(200);
        var body = response.GetBodyAsString()!;
        body.ShouldContain("user-42");
    }

    // ---- Helpers ----

    private static EndpointContext CreateContext(string method, string path)
    {
        var request = new EndpointRequest(method, new Uri("http://localhost" + path));
        return new EndpointContext(request);
    }

    // ---- Stub endpoints ----

    private sealed class StubPostsEndpoint : IAtollEndpoint
    {
        public Task<AtollResponse> GetAsync(EndpointContext context)
        {
            var posts = new[]
            {
                new { Id = 1, Title = "Hello World" },
                new { Id = 2, Title = "Getting Started" }
            };
            return Task.FromResult(AtollResponse.Json(posts));
        }

        public Task<AtollResponse> PostAsync(EndpointContext context)
        {
            return Task.FromResult(AtollResponse.Json(new { Id = 3, Title = "New Post" }, 201));
        }
    }

    private sealed class StubGetOnlyEndpoint : IAtollEndpoint
    {
        public Task<AtollResponse> GetAsync(EndpointContext context)
        {
            return Task.FromResult(AtollResponse.Json(new[] { "item1", "item2" }));
        }
    }

    private sealed class StubFullCrudEndpoint : IAtollEndpoint
    {
        public Task<AtollResponse> GetAsync(EndpointContext context)
        {
            return Task.FromResult(AtollResponse.Text("get"));
        }

        public Task<AtollResponse> PostAsync(EndpointContext context)
        {
            return Task.FromResult(AtollResponse.Text("created", 201));
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

    private sealed class StubHeadOptionsEndpoint : IAtollEndpoint
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

    private sealed class StubEmptyEndpoint : IAtollEndpoint
    {
        // No methods implemented — all will 405
    }

    private sealed class StubDynamicEndpoint : IAtollEndpoint
    {
        public Task<AtollResponse> GetAsync(EndpointContext context)
        {
            var slug = context.GetParameter("slug");
            return Task.FromResult(AtollResponse.Json(new { Slug = slug, Title = "Post: " + slug }));
        }
    }

    private sealed class StubSecureEndpoint : IAtollEndpoint
    {
        public Task<AtollResponse> GetAsync(EndpointContext context)
        {
            var userId = context.GetLocal<string>("userId");
            return Task.FromResult(AtollResponse.Json(new { UserId = userId }));
        }
    }
}
