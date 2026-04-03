using Atoll.Components;
using Atoll.Middleware.Pipeline;
using Atoll.Routing;
using Atoll.Routing.Matching;
using Shouldly;
using Xunit;

namespace Atoll.Middleware.Tests;

public sealed class MiddlewareIntegrationTests
{
    private static EndpointRequest CreateRequest(string method, string path)
    {
        return new EndpointRequest(method, new Uri($"http://localhost{path}"));
    }

    private static EndpointRequest CreateGetRequest(string path)
    {
        return CreateRequest("GET", path);
    }

    // ---- End-to-end: middleware → route match → dispatch ----

    [Fact]
    public async Task ShouldRunFullPipelineWithAuthAndLogging()
    {
        var routes = new[]
        {
            new RouteEntry("/", typeof(StubPage), "index.cs"),
            new RouteEntry("/admin", typeof(StubPage), "admin.cs"),
        };
        var matcher = new RouteMatcher(routes);

        var log = new List<string>();

        MiddlewareHandler logging = async (ctx, next) =>
        {
            log.Add($"request:{ctx.Url.AbsolutePath}");
            var response = await next();
            log.Add($"response:{response.StatusCode}");
            return response;
        };

        MiddlewareHandler auth = async (ctx, next) =>
        {
            if (ctx.RoutePattern == "/admin" && !ctx.Locals.ContainsKey("user"))
            {
                return AtollResponse.Text("Forbidden", 403);
            }
            return await next();
        };

        var pipeline = MiddlewareSequencer.Sequence(logging, auth);
        var runner = new MiddlewareRunner(pipeline, matcher);

        // Unauthenticated request to /admin
        var match = matcher.Match("/admin")!;
        var response = await runner.RunAsync(
            match,
            CreateGetRequest("/admin"),
            ctx => Task.FromResult(AtollResponse.Text("Admin Page")));

        response.StatusCode.ShouldBe(403);
        log.ShouldContain("request:/admin");
        log.ShouldContain("response:403");
    }

    [Fact]
    public async Task ShouldRunFullPipelineWithAuthenticatedUser()
    {
        var routes = new[]
        {
            new RouteEntry("/admin", typeof(StubPage), "admin.cs"),
        };
        var matcher = new RouteMatcher(routes);

        MiddlewareHandler auth = async (ctx, next) =>
        {
            // Simulate auth — set user in locals
            ctx.Locals["user"] = "admin";
            return await next();
        };

        var pipeline = MiddlewareSequencer.Sequence(auth);
        var runner = new MiddlewareRunner(pipeline, matcher);

        var match = matcher.Match("/admin")!;
        var response = await runner.RunAsync(
            match,
            CreateGetRequest("/admin"),
            ctx =>
            {
                var user = ctx.GetLocal<string>("user");
                return Task.FromResult(AtollResponse.Text($"Welcome, {user}!"));
            });

        response.StatusCode.ShouldBe(200);
        response.GetBodyAsString().ShouldBe("Welcome, admin!");
    }

    // ---- Rewrite inside middleware chain ----

    [Fact]
    public async Task ShouldRewriteOldPathToNewPathInMiddleware()
    {
        var routes = new[]
        {
            new RouteEntry("/old-page", typeof(StubPage), "old-page.cs"),
            new RouteEntry("/new-page", typeof(StubPage), "new-page.cs"),
        };
        var matcher = new RouteMatcher(routes);

        MiddlewareHandler redirectMiddleware = async (ctx, next) =>
        {
            if (ctx.RoutePattern == "/old-page")
            {
                ctx.Rewrite("/new-page");
            }
            return await next();
        };

        var pipeline = MiddlewareSequencer.Sequence(redirectMiddleware);
        var runner = new MiddlewareRunner(pipeline, matcher);

        var match = matcher.Match("/old-page")!;
        string? capturedPattern = null;

        var response = await runner.RunAsync(
            match,
            CreateGetRequest("/old-page"),
            ctx =>
            {
                capturedPattern = ctx.RoutePattern;
                return Task.FromResult(AtollResponse.Text("new page content"));
            });

        capturedPattern.ShouldBe("/new-page");
        response.GetBodyAsString().ShouldBe("new page content");
    }

    // ---- Middleware reading route parameters ----

    [Fact]
    public async Task ShouldAccessRouteParametersInMiddleware()
    {
        var routes = new[]
        {
            new RouteEntry("/blog/[slug]", typeof(StubPage), "blog/[slug].cs"),
        };
        var matcher = new RouteMatcher(routes);

        string? capturedSlug = null;
        MiddlewareHandler paramReader = async (ctx, next) =>
        {
            capturedSlug = ctx.GetParameter("slug");
            return await next();
        };

        var pipeline = MiddlewareSequencer.Sequence(paramReader);
        var runner = new MiddlewareRunner(pipeline, matcher);

        var match = matcher.Match("/blog/hello-world")!;
        await runner.RunAsync(
            match,
            CreateGetRequest("/blog/hello-world"),
            _ => Task.FromResult(AtollResponse.Text("ok")));

        capturedSlug.ShouldBe("hello-world");
    }

    // ---- CORS-like middleware ----

    [Fact]
    public async Task ShouldAddCorsHeadersViaMiddleware()
    {
        MiddlewareHandler cors = async (ctx, next) =>
        {
            var response = await next();
            var headers = new Dictionary<string, string>(response.Headers)
            {
                ["Access-Control-Allow-Origin"] = "*",
                ["Access-Control-Allow-Methods"] = "GET, POST"
            };
            return new AtollResponse(response.StatusCode, headers, response.Body);
        };

        var pipeline = MiddlewareSequencer.Sequence(cors);
        var runner = new MiddlewareRunner(pipeline);
        var context = new MiddlewareContext(
            "/api/data",
            new Dictionary<string, string>(),
            CreateGetRequest("/api/data"));

        var response = await runner.RunAsync(
            context,
            _ => Task.FromResult(AtollResponse.Json(new { value = 1 })));

        response.Headers["Access-Control-Allow-Origin"].ShouldBe("*");
        response.Headers["Access-Control-Allow-Methods"].ShouldBe("GET, POST");
    }

    // ---- Rate limiting pattern ----

    [Fact]
    public async Task ShouldShortCircuitOnRateLimit()
    {
        var requestCount = 0;
        const int limit = 2;

        MiddlewareHandler rateLimiter = (ctx, next) =>
        {
            requestCount++;
            if (requestCount > limit)
            {
                return Task.FromResult(AtollResponse.Text("Too Many Requests", 429));
            }
            return next();
        };

        var pipeline = MiddlewareSequencer.Sequence(rateLimiter);
        var runner = new MiddlewareRunner(pipeline);
        var context = new MiddlewareContext(
            "/",
            new Dictionary<string, string>(),
            CreateGetRequest("/"));

        // First two requests should succeed
        var r1 = await runner.RunAsync(context, _ => Task.FromResult(AtollResponse.Text("ok")));
        var r2 = await runner.RunAsync(context, _ => Task.FromResult(AtollResponse.Text("ok")));
        // Third should be rate limited
        var r3 = await runner.RunAsync(context, _ => Task.FromResult(AtollResponse.Text("ok")));

        r1.StatusCode.ShouldBe(200);
        r2.StatusCode.ShouldBe(200);
        r3.StatusCode.ShouldBe(429);
    }

    // ---- Middleware composability ----

    [Fact]
    public async Task ShouldComposeNestedSequences()
    {
        var log = new List<string>();

        MiddlewareHandler a = async (ctx, next) =>
        {
            log.Add("a");
            return await next();
        };

        MiddlewareHandler b = async (ctx, next) =>
        {
            log.Add("b");
            return await next();
        };

        MiddlewareHandler c = async (ctx, next) =>
        {
            log.Add("c");
            return await next();
        };

        // Compose nested: Sequence(a, Sequence(b, c))
        var inner = MiddlewareSequencer.Sequence(b, c);
        var outer = MiddlewareSequencer.Sequence(a, inner);

        var runner = new MiddlewareRunner(outer);
        var context = new MiddlewareContext(
            "/",
            new Dictionary<string, string>(),
            CreateGetRequest("/"));

        await runner.RunAsync(context, _ =>
        {
            log.Add("terminal");
            return Task.FromResult(AtollResponse.Text("ok"));
        });

        log.ShouldBe(new[] { "a", "b", "c", "terminal" });
    }

    // ---- Rewrite with subsequent middleware seeing new route ----

    [Fact]
    public async Task MiddlewareAfterRewriteShouldSeeOriginalContext()
    {
        var routes = new[]
        {
            new RouteEntry("/old", typeof(StubPage), "old.cs"),
            new RouteEntry("/new", typeof(StubPage), "new.cs"),
        };
        var matcher = new RouteMatcher(routes);

        var log = new List<string>();

        MiddlewareHandler rewriter = async (ctx, next) =>
        {
            ctx.Rewrite("/new");
            log.Add($"rewriter:pattern={ctx.RoutePattern}");
            return await next();
        };

        MiddlewareHandler logger = async (ctx, next) =>
        {
            // Before rewrite is applied (happens at terminal), still see /old
            log.Add($"logger:pattern={ctx.RoutePattern}");
            return await next();
        };

        var pipeline = MiddlewareSequencer.Sequence(rewriter, logger);
        var runner = new MiddlewareRunner(pipeline, matcher);

        var match = matcher.Match("/old")!;
        string? terminalPattern = null;

        await runner.RunAsync(
            match,
            CreateGetRequest("/old"),
            ctx =>
            {
                terminalPattern = ctx.RoutePattern;
                return Task.FromResult(AtollResponse.Text("ok"));
            });

        // Rewrite is applied at terminal boundary
        terminalPattern.ShouldBe("/new");
        log.ShouldContain("rewriter:pattern=/old");
    }

    // ---- Multiple locals from different middleware ----

    [Fact]
    public async Task MultipleMiddlewareShouldAccumulateLocals()
    {
        MiddlewareHandler setUser = async (ctx, next) =>
        {
            ctx.Locals["user"] = "alice";
            return await next();
        };

        MiddlewareHandler setRole = async (ctx, next) =>
        {
            ctx.Locals["role"] = "admin";
            return await next();
        };

        MiddlewareHandler setTimestamp = async (ctx, next) =>
        {
            ctx.Locals["timestamp"] = 1234567890L;
            return await next();
        };

        var pipeline = MiddlewareSequencer.Sequence(setUser, setRole, setTimestamp);
        var runner = new MiddlewareRunner(pipeline);
        var context = new MiddlewareContext(
            "/",
            new Dictionary<string, string>(),
            CreateGetRequest("/"));

        await runner.RunAsync(context, ctx =>
        {
            ctx.GetLocal<string>("user").ShouldBe("alice");
            ctx.GetLocal<string>("role").ShouldBe("admin");
            ctx.GetLocal<long>("timestamp").ShouldBe(1234567890L);
            return Task.FromResult(AtollResponse.Text("ok"));
        });
    }

    // ---- Conditional middleware ----

    [Fact]
    public async Task ShouldConditionallyApplyMiddlewareBasedOnRoute()
    {
        MiddlewareHandler apiOnly = async (ctx, next) =>
        {
            if (ctx.RoutePattern.StartsWith("/api"))
            {
                ctx.Locals["api"] = true;
            }
            return await next();
        };

        var pipeline = MiddlewareSequencer.Sequence(apiOnly);
        var runner = new MiddlewareRunner(pipeline);

        // API route
        var apiContext = new MiddlewareContext(
            "/api/users",
            new Dictionary<string, string>(),
            CreateGetRequest("/api/users"));

        await runner.RunAsync(apiContext, ctx =>
        {
            ctx.Locals.ContainsKey("api").ShouldBeTrue();
            return Task.FromResult(AtollResponse.Text("ok"));
        });

        // Non-API route
        var pageContext = new MiddlewareContext(
            "/about",
            new Dictionary<string, string>(),
            CreateGetRequest("/about"));

        await runner.RunAsync(pageContext, ctx =>
        {
            ctx.Locals.ContainsKey("api").ShouldBeFalse();
            return Task.FromResult(AtollResponse.Text("ok"));
        });
    }

    // ---- Middleware exception handling ----

    [Fact]
    public async Task ExceptionInMiddlewareShouldPropagate()
    {
        MiddlewareHandler faulty = (ctx, next) =>
        {
            throw new InvalidOperationException("middleware error");
        };

        var pipeline = MiddlewareSequencer.Sequence(faulty);
        var runner = new MiddlewareRunner(pipeline);
        var context = new MiddlewareContext(
            "/",
            new Dictionary<string, string>(),
            CreateGetRequest("/"));

        var ex = await Should.ThrowAsync<InvalidOperationException>(() =>
            runner.RunAsync(context, _ => Task.FromResult(AtollResponse.Text("ok"))));

        ex.Message.ShouldBe("middleware error");
    }

    [Fact]
    public async Task ExceptionInTerminalShouldPropagateBackThroughMiddleware()
    {
        var afterNextCalled = false;

        MiddlewareHandler wrapper = async (ctx, next) =>
        {
            try
            {
                return await next();
            }
            catch
            {
                afterNextCalled = true;
                throw;
            }
        };

        var pipeline = MiddlewareSequencer.Sequence(wrapper);
        var runner = new MiddlewareRunner(pipeline);
        var context = new MiddlewareContext(
            "/",
            new Dictionary<string, string>(),
            CreateGetRequest("/"));

        await Should.ThrowAsync<InvalidOperationException>(() =>
            runner.RunAsync(context, _ => throw new InvalidOperationException("terminal error")));

        afterNextCalled.ShouldBeTrue();
    }

    // ---- Error handling middleware pattern ----

    [Fact]
    public async Task ShouldCatchExceptionAndReturnErrorResponse()
    {
        MiddlewareHandler errorHandler = async (ctx, next) =>
        {
            try
            {
                return await next();
            }
            catch (Exception ex)
            {
                return AtollResponse.Text($"Error: {ex.Message}", 500);
            }
        };

        MiddlewareHandler faulty = (ctx, next) =>
        {
            throw new InvalidOperationException("something broke");
        };

        var pipeline = MiddlewareSequencer.Sequence(errorHandler, faulty);
        var runner = new MiddlewareRunner(pipeline);
        var context = new MiddlewareContext(
            "/",
            new Dictionary<string, string>(),
            CreateGetRequest("/"));

        var response = await runner.RunAsync(context,
            _ => Task.FromResult(AtollResponse.Text("ok")));

        response.StatusCode.ShouldBe(500);
        response.GetBodyAsString().ShouldBe("Error: something broke");
    }

    // ---- Stub types ----

    private sealed class StubPage : IAtollComponent
    {
        public Task RenderAsync(RenderContext context)
        {
            context.WriteHtml("<p>Stub</p>");
            return Task.CompletedTask;
        }
    }
}
