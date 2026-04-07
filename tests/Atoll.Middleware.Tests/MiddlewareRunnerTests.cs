using Atoll.Components;
using Atoll.Middleware.Pipeline;
using Atoll.Routing;
using Atoll.Routing.Matching;

namespace Atoll.Middleware.Tests;

public sealed class MiddlewareRunnerTests
{
    private static EndpointRequest CreateRequest(string path)
    {
        return new EndpointRequest("GET", new Uri($"http://localhost{path}"));
    }

    private static MiddlewareContext CreateContext(string routePattern, string path)
    {
        return new MiddlewareContext(
            routePattern,
            new Dictionary<string, string>(),
            CreateRequest(path));
    }

    // ---- Basic execution ----

    [Fact]
    public async Task ShouldRunPipelineAndReturnTerminalResponse()
    {
        MiddlewareHandler passThrough = async (ctx, next) => await next();
        var runner = new MiddlewareRunner(passThrough);
        var context = CreateContext("/", "/");

        var response = await runner.RunAsync(
            context,
            _ => Task.FromResult(AtollResponse.Text("hello")));

        response.GetBodyAsString().ShouldBe("hello");
    }

    [Fact]
    public async Task ShouldPassContextToTerminalHandler()
    {
        MiddlewareHandler setLocal = async (ctx, next) =>
        {
            ctx.Locals["key"] = "value";
            return await next();
        };

        var runner = new MiddlewareRunner(setLocal);
        var context = CreateContext("/about", "/about");
        string? capturedLocal = null;

        await runner.RunAsync(
            context,
            ctx =>
            {
                capturedLocal = ctx.GetLocal<string>("key");
                return Task.FromResult(AtollResponse.Text("ok"));
            });

        capturedLocal.ShouldBe("value");
    }

    [Fact]
    public async Task ShouldSupportShortCircuitInPipeline()
    {
        MiddlewareHandler auth = (ctx, next) =>
            Task.FromResult(AtollResponse.Text("denied", 403));

        var runner = new MiddlewareRunner(auth);
        var context = CreateContext("/", "/");
        var terminalCalled = false;

        var response = await runner.RunAsync(
            context,
            _ =>
            {
                terminalCalled = true;
                return Task.FromResult(AtollResponse.Text("ok"));
            });

        terminalCalled.ShouldBeFalse();
        response.StatusCode.ShouldBe(403);
    }

    // ---- RouteMatchResult overload ----

    [Fact]
    public async Task ShouldCreateContextFromRouteMatch()
    {
        var routeEntry = new RouteEntry("/blog/[slug]", typeof(StubPage), "blog/[slug].cs");
        var parameters = new Dictionary<string, string> { ["slug"] = "hello" };
        var routeMatch = new RouteMatchResult(routeEntry, parameters);
        var request = CreateRequest("/blog/hello");

        MiddlewareHandler passThrough = async (ctx, next) =>
        {
            ctx.RoutePattern.ShouldBe("/blog/[slug]");
            ctx.GetParameter("slug").ShouldBe("hello");
            return await next();
        };

        var runner = new MiddlewareRunner(passThrough);

        var response = await runner.RunAsync(
            routeMatch,
            request,
            _ => Task.FromResult(AtollResponse.Text("ok")));

        response.GetBodyAsString().ShouldBe("ok");
    }

    // ---- Rewrite support ----

    [Fact]
    public async Task RewriteShouldReResolveRouteBeforeTerminal()
    {
        var routes = new[]
        {
            new RouteEntry("/old", typeof(StubPage), "old.cs"),
            new RouteEntry("/new", typeof(StubPage), "new.cs"),
        };
        var matcher = new RouteMatcher(routes);

        MiddlewareHandler rewriter = async (ctx, next) =>
        {
            ctx.Rewrite("/new");
            return await next();
        };

        var runner = new MiddlewareRunner(rewriter, matcher);
        var context = new MiddlewareContext(
            "/old",
            new Dictionary<string, string>(),
            CreateRequest("/old"));

        string? capturedPattern = null;
        await runner.RunAsync(
            context,
            ctx =>
            {
                capturedPattern = ctx.RoutePattern;
                return Task.FromResult(AtollResponse.Text("ok"));
            });

        capturedPattern.ShouldBe("/new");
    }

    [Fact]
    public async Task RewriteShouldUpdateParameters()
    {
        var routes = new[]
        {
            new RouteEntry("/old", typeof(StubPage), "old.cs"),
            new RouteEntry("/blog/[slug]", typeof(StubPage), "blog/[slug].cs"),
        };
        var matcher = new RouteMatcher(routes);

        MiddlewareHandler rewriter = async (ctx, next) =>
        {
            ctx.Rewrite("/blog/rewritten-post");
            return await next();
        };

        var runner = new MiddlewareRunner(rewriter, matcher);
        var context = new MiddlewareContext(
            "/old",
            new Dictionary<string, string>(),
            CreateRequest("/old"));

        string? capturedSlug = null;
        await runner.RunAsync(
            context,
            ctx =>
            {
                capturedSlug = ctx.GetParameter("slug");
                return Task.FromResult(AtollResponse.Text("ok"));
            });

        capturedSlug.ShouldBe("rewritten-post");
    }

    [Fact]
    public async Task RewriteToUnmatchedRouteShouldThrow()
    {
        var routes = new[]
        {
            new RouteEntry("/old", typeof(StubPage), "old.cs"),
        };
        var matcher = new RouteMatcher(routes);

        MiddlewareHandler rewriter = async (ctx, next) =>
        {
            ctx.Rewrite("/nonexistent");
            return await next();
        };

        var runner = new MiddlewareRunner(rewriter, matcher);
        var context = new MiddlewareContext(
            "/old",
            new Dictionary<string, string>(),
            CreateRequest("/old"));

        var ex = await Should.ThrowAsync<InvalidOperationException>(() =>
            runner.RunAsync(
                context,
                _ => Task.FromResult(AtollResponse.Text("ok"))));

        ex.Message.ShouldContain("/nonexistent");
        ex.Message.ShouldContain("does not match");
    }

    [Fact]
    public async Task RewriteWithoutRouteMatcherShouldThrow()
    {
        MiddlewareHandler rewriter = async (ctx, next) =>
        {
            ctx.Rewrite("/new");
            return await next();
        };

        var runner = new MiddlewareRunner(rewriter);
        var context = CreateContext("/old", "/old");

        var ex = await Should.ThrowAsync<InvalidOperationException>(() =>
            runner.RunAsync(
                context,
                _ => Task.FromResult(AtollResponse.Text("ok"))));

        ex.Message.ShouldContain("RouteMatcher");
    }

    // ---- Null validation ----

    [Fact]
    public void ShouldThrowForNullPipeline()
    {
        Should.Throw<ArgumentNullException>(() =>
            new MiddlewareRunner(null!));
    }

    [Fact]
    public void ShouldThrowForNullPipelineWithMatcher()
    {
        var matcher = new RouteMatcher(Array.Empty<RouteEntry>());
        Should.Throw<ArgumentNullException>(() =>
            new MiddlewareRunner(null!, matcher));
    }

    [Fact]
    public void ShouldThrowForNullRouteMatcher()
    {
        MiddlewareHandler handler = async (ctx, next) => await next();
        Should.Throw<ArgumentNullException>(() =>
            new MiddlewareRunner(handler, null!));
    }

    [Fact]
    public async Task RunAsyncShouldThrowForNullContext()
    {
        MiddlewareHandler handler = async (ctx, next) => await next();
        var runner = new MiddlewareRunner(handler);

        await Should.ThrowAsync<ArgumentNullException>(() =>
            runner.RunAsync(null!, _ => Task.FromResult(AtollResponse.Text("ok"))));
    }

    [Fact]
    public async Task RunAsyncShouldThrowForNullTerminalHandler()
    {
        MiddlewareHandler handler = async (ctx, next) => await next();
        var runner = new MiddlewareRunner(handler);
        var context = CreateContext("/", "/");

        await Should.ThrowAsync<ArgumentNullException>(() =>
            runner.RunAsync(context, null!));
    }

    [Fact]
    public async Task RunAsyncWithRouteMatchShouldThrowForNullRouteMatch()
    {
        MiddlewareHandler handler = async (ctx, next) => await next();
        var runner = new MiddlewareRunner(handler);
        var request = CreateRequest("/");

        await Should.ThrowAsync<ArgumentNullException>(() =>
            runner.RunAsync(null!, request, _ => Task.FromResult(AtollResponse.Text("ok"))));
    }

    [Fact]
    public async Task RunAsyncWithRouteMatchShouldThrowForNullRequest()
    {
        MiddlewareHandler handler = async (ctx, next) => await next();
        var runner = new MiddlewareRunner(handler);
        var routeEntry = new RouteEntry("/", typeof(StubPage), "index.cs");
        var match = new RouteMatchResult(routeEntry, new Dictionary<string, string>());

        await Should.ThrowAsync<ArgumentNullException>(() =>
            runner.RunAsync(match, null!, _ => Task.FromResult(AtollResponse.Text("ok"))));
    }

    [Fact]
    public async Task RunAsyncWithRouteMatchShouldThrowForNullTerminal()
    {
        MiddlewareHandler handler = async (ctx, next) => await next();
        var runner = new MiddlewareRunner(handler);
        var routeEntry = new RouteEntry("/", typeof(StubPage), "index.cs");
        var match = new RouteMatchResult(routeEntry, new Dictionary<string, string>());
        var request = CreateRequest("/");

        await Should.ThrowAsync<ArgumentNullException>(() =>
            runner.RunAsync(match, request, null!));
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
