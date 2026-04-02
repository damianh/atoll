using Atoll.Core.Components;
using Atoll.Middleware.Pipeline;
using Atoll.Routing;
using Atoll.Routing.Matching;
using Shouldly;
using Xunit;

namespace Atoll.Middleware.Tests;

public sealed class RewriteHandlerTests
{
    private static EndpointRequest CreateRequest(string path)
    {
        return new EndpointRequest("GET", new Uri($"http://localhost{path}"));
    }

    private static RouteMatcher CreateMatcher(params RouteEntry[] routes)
    {
        return new RouteMatcher(routes);
    }

    // ---- TryResolve ----

    [Fact]
    public void TryResolveShouldReturnMatchForValidPath()
    {
        var routes = new[] { new RouteEntry("/about", typeof(StubPage), "about.cs") };
        var handler = new RewriteHandler(CreateMatcher(routes));

        var match = handler.TryResolve("/about");

        match.ShouldNotBeNull();
        match.RouteEntry.Pattern.ShouldBe("/about");
    }

    [Fact]
    public void TryResolveShouldReturnNullForUnmatchedPath()
    {
        var routes = new[] { new RouteEntry("/about", typeof(StubPage), "about.cs") };
        var handler = new RewriteHandler(CreateMatcher(routes));

        var match = handler.TryResolve("/missing");

        match.ShouldBeNull();
    }

    [Fact]
    public void TryResolveShouldThrowForNullPath()
    {
        var handler = new RewriteHandler(CreateMatcher());
        Should.Throw<ArgumentNullException>(() => handler.TryResolve(null!));
    }

    // ---- Resolve ----

    [Fact]
    public void ResolveShouldReturnMatchForValidPath()
    {
        var routes = new[] { new RouteEntry("/about", typeof(StubPage), "about.cs") };
        var handler = new RewriteHandler(CreateMatcher(routes));

        var match = handler.Resolve("/about");

        match.RouteEntry.Pattern.ShouldBe("/about");
    }

    [Fact]
    public void ResolveShouldThrowForUnmatchedPath()
    {
        var routes = new[] { new RouteEntry("/about", typeof(StubPage), "about.cs") };
        var handler = new RewriteHandler(CreateMatcher(routes));

        var ex = Should.Throw<InvalidOperationException>(() => handler.Resolve("/missing"));
        ex.Message.ShouldContain("/missing");
        ex.Message.ShouldContain("does not match");
    }

    [Fact]
    public void ResolveShouldThrowForNullPath()
    {
        var handler = new RewriteHandler(CreateMatcher());
        Should.Throw<ArgumentNullException>(() => handler.Resolve(null!));
    }

    // ---- CreateHandler ----

    [Fact]
    public async Task HandlerShouldPassThroughWhenNoRewrite()
    {
        var routes = new[] { new RouteEntry("/about", typeof(StubPage), "about.cs") };
        var rewriteHandler = new RewriteHandler(CreateMatcher(routes));
        var middleware = rewriteHandler.CreateHandler();

        var context = new MiddlewareContext(
            "/about",
            new Dictionary<string, string>(),
            CreateRequest("/about"));

        var response = await middleware(context, () =>
            Task.FromResult(AtollResponse.Text("ok")));

        response.GetBodyAsString().ShouldBe("ok");
        context.RoutePattern.ShouldBe("/about");
    }

    [Fact]
    public async Task HandlerShouldApplyRewriteAndUpdateContext()
    {
        var routes = new[]
        {
            new RouteEntry("/old", typeof(StubPage), "old.cs"),
            new RouteEntry("/new", typeof(StubPage), "new.cs"),
        };
        var rewriteHandler = new RewriteHandler(CreateMatcher(routes));
        var middleware = rewriteHandler.CreateHandler();

        var context = new MiddlewareContext(
            "/old",
            new Dictionary<string, string>(),
            CreateRequest("/old"));
        context.Rewrite("/new");

        var response = await middleware(context, () =>
            Task.FromResult(AtollResponse.Text("ok")));

        context.RoutePattern.ShouldBe("/new");
        context.HasRewrite.ShouldBeFalse();
    }

    [Fact]
    public async Task HandlerShouldExtractParametersFromRewriteTarget()
    {
        var routes = new[]
        {
            new RouteEntry("/old", typeof(StubPage), "old.cs"),
            new RouteEntry("/blog/[slug]", typeof(StubPage), "blog/[slug].cs"),
        };
        var rewriteHandler = new RewriteHandler(CreateMatcher(routes));
        var middleware = rewriteHandler.CreateHandler();

        var context = new MiddlewareContext(
            "/old",
            new Dictionary<string, string>(),
            CreateRequest("/old"));
        context.Rewrite("/blog/my-post");

        await middleware(context, () =>
            Task.FromResult(AtollResponse.Text("ok")));

        context.RoutePattern.ShouldBe("/blog/[slug]");
        context.Parameters["slug"].ShouldBe("my-post");
    }

    [Fact]
    public async Task HandlerShouldThrowForUnmatchedRewriteTarget()
    {
        var routes = new[]
        {
            new RouteEntry("/old", typeof(StubPage), "old.cs"),
        };
        var rewriteHandler = new RewriteHandler(CreateMatcher(routes));
        var middleware = rewriteHandler.CreateHandler();

        var context = new MiddlewareContext(
            "/old",
            new Dictionary<string, string>(),
            CreateRequest("/old"));
        context.Rewrite("/nonexistent");

        await Should.ThrowAsync<InvalidOperationException>(() =>
            middleware(context, () =>
                Task.FromResult(AtollResponse.Text("ok"))));
    }

    // ---- Constructor null validation ----

    [Fact]
    public void ShouldThrowForNullRouteMatcher()
    {
        Should.Throw<ArgumentNullException>(() => new RewriteHandler(null!));
    }

    // ---- Dynamic parameters on rewrite ----

    [Fact]
    public async Task HandlerShouldResolveToCorrectDynamicRouteOnRewrite()
    {
        var routes = new[]
        {
            new RouteEntry("/", typeof(StubPage), "index.cs"),
            new RouteEntry("/docs/[...rest]", typeof(StubPage), "docs/[...rest].cs"),
        };
        var rewriteHandler = new RewriteHandler(CreateMatcher(routes));
        var middleware = rewriteHandler.CreateHandler();

        var context = new MiddlewareContext(
            "/",
            new Dictionary<string, string>(),
            CreateRequest("/"));
        context.Rewrite("/docs/getting-started/install");

        await middleware(context, () =>
            Task.FromResult(AtollResponse.Text("ok")));

        context.RoutePattern.ShouldBe("/docs/[...rest]");
        context.Parameters["rest"].ShouldBe("getting-started/install");
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
