using Atoll.Routing;
using Atoll.Routing.Matching;

namespace Atoll.Middleware.Tests;

public sealed class MiddlewareContextTests
{
    private static EndpointRequest CreateRequest(string path)
    {
        return new EndpointRequest("GET", new Uri($"http://localhost{path}"));
    }

    [Fact]
    public void ShouldStoreRoutePatternAndParametersAndRequest()
    {
        var parameters = new Dictionary<string, string> { ["slug"] = "hello" };
        var request = CreateRequest("/blog/hello");
        var context = new MiddlewareContext("/blog/[slug]", parameters, request);

        context.RoutePattern.ShouldBe("/blog/[slug]");
        context.Parameters["slug"].ShouldBe("hello");
        context.Request.ShouldBeSameAs(request);
        context.Locals.ShouldBeEmpty();
    }

    [Fact]
    public void ShouldInitializeWithEmptyLocals()
    {
        var request = CreateRequest("/about");
        var context = new MiddlewareContext("/about", new Dictionary<string, string>(), request);

        context.Locals.ShouldBeEmpty();
    }

    [Fact]
    public void ShouldAcceptExplicitLocals()
    {
        var locals = new Dictionary<string, object?> { ["user"] = "alice" };
        var request = CreateRequest("/about");
        var context = new MiddlewareContext("/about", new Dictionary<string, string>(), request, locals);

        context.Locals["user"].ShouldBe("alice");
    }

    [Fact]
    public void ShouldInitializeFromRequestOnly()
    {
        var request = CreateRequest("/");
        var context = new MiddlewareContext(request);

        context.RoutePattern.ShouldBe(string.Empty);
        context.Parameters.ShouldBeEmpty();
        context.Request.ShouldBeSameAs(request);
    }

    [Fact]
    public void UrlShouldReturnRequestUrl()
    {
        var request = CreateRequest("/about");
        var context = new MiddlewareContext("/about", new Dictionary<string, string>(), request);

        context.Url.ShouldBe(request.Url);
    }

    [Fact]
    public void ShouldThrowForNullRoutePattern()
    {
        var request = CreateRequest("/");
        Should.Throw<ArgumentNullException>(() =>
            new MiddlewareContext(null!, new Dictionary<string, string>(), request));
    }

    [Fact]
    public void ShouldThrowForNullParameters()
    {
        var request = CreateRequest("/");
        Should.Throw<ArgumentNullException>(() =>
            new MiddlewareContext("/about", null!, request));
    }

    [Fact]
    public void ShouldThrowForNullRequest()
    {
        Should.Throw<ArgumentNullException>(() =>
            new MiddlewareContext("/about", new Dictionary<string, string>(), null!));
    }

    [Fact]
    public void ShouldThrowForNullLocals()
    {
        var request = CreateRequest("/");
        Should.Throw<ArgumentNullException>(() =>
            new MiddlewareContext("/about", new Dictionary<string, string>(), request, null!));
    }

    [Fact]
    public void ShouldThrowForNullRequestOnRequestOnlyConstructor()
    {
        Should.Throw<ArgumentNullException>(() => new MiddlewareContext(null!));
    }

    // ---- Rewrite ----

    [Fact]
    public void RewriteShouldSetTargetPath()
    {
        var context = new MiddlewareContext(CreateRequest("/old"));
        context.HasRewrite.ShouldBeFalse();
        context.RewriteTarget.ShouldBeNull();

        context.Rewrite("/new");

        context.HasRewrite.ShouldBeTrue();
        context.RewriteTarget.ShouldBe("/new");
    }

    [Fact]
    public void RewriteShouldThrowForNullPath()
    {
        var context = new MiddlewareContext(CreateRequest("/old"));
        Should.Throw<ArgumentNullException>(() => context.Rewrite(null!));
    }

    [Fact]
    public void RewriteShouldOverwritePreviousTarget()
    {
        var context = new MiddlewareContext(CreateRequest("/old"));
        context.Rewrite("/first");
        context.Rewrite("/second");

        context.RewriteTarget.ShouldBe("/second");
    }

    // ---- Locals ----

    [Fact]
    public void GetLocalShouldReturnTypedValue()
    {
        var context = new MiddlewareContext(CreateRequest("/"));
        context.Locals["count"] = 42;

        context.GetLocal<int>("count").ShouldBe(42);
    }

    [Fact]
    public void GetLocalShouldThrowForMissingKey()
    {
        var context = new MiddlewareContext(CreateRequest("/"));
        Should.Throw<KeyNotFoundException>(() => context.GetLocal<string>("missing"));
    }

    [Fact]
    public void GetLocalWithDefaultShouldReturnValueWhenPresent()
    {
        var context = new MiddlewareContext(CreateRequest("/"));
        context.Locals["name"] = "alice";

        context.GetLocal("name", "default").ShouldBe("alice");
    }

    [Fact]
    public void GetLocalWithDefaultShouldReturnDefaultWhenMissing()
    {
        var context = new MiddlewareContext(CreateRequest("/"));
        context.GetLocal("missing", "fallback").ShouldBe("fallback");
    }

    [Fact]
    public void GetLocalShouldThrowForNullKey()
    {
        var context = new MiddlewareContext(CreateRequest("/"));
        Should.Throw<ArgumentNullException>(() => context.GetLocal<string>(null!));
    }

    [Fact]
    public void GetLocalWithDefaultShouldThrowForNullKey()
    {
        var context = new MiddlewareContext(CreateRequest("/"));
        Should.Throw<ArgumentNullException>(() => context.GetLocal(null!, "default"));
    }

    // ---- GetParameter ----

    [Fact]
    public void GetParameterShouldReturnValue()
    {
        var parameters = new Dictionary<string, string> { ["slug"] = "hello" };
        var context = new MiddlewareContext("/blog/[slug]", parameters, CreateRequest("/blog/hello"));

        context.GetParameter("slug").ShouldBe("hello");
    }

    [Fact]
    public void GetParameterShouldThrowForMissingParameter()
    {
        var context = new MiddlewareContext("/about", new Dictionary<string, string>(), CreateRequest("/about"));
        Should.Throw<KeyNotFoundException>(() => context.GetParameter("slug"));
    }

    [Fact]
    public void GetParameterShouldThrowForNullName()
    {
        var context = new MiddlewareContext(CreateRequest("/"));
        Should.Throw<ArgumentNullException>(() => context.GetParameter(null!));
    }
}
