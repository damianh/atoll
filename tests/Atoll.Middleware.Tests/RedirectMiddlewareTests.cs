using Atoll.Redirects;
using Atoll.Routing;
using Shouldly;
using Xunit;

namespace Atoll.Middleware.Tests;

public sealed class RedirectMiddlewareTests
{
    private static MiddlewareContext CreateContext(string path, string? query = null)
    {
        var url = query is not null
            ? $"http://localhost{path}{query}"
            : $"http://localhost{path}";
        var request = new EndpointRequest("GET", new Uri(url));
        return new MiddlewareContext(request);
    }

    private static RedirectMap CreateMap(params (string from, string to)[] entries)
    {
        return RedirectMap.Create(entries.Select(e => new KeyValuePair<string, string>(e.from, e.to)));
    }

    [Fact]
    public async Task ShouldReturn301ForMatchedRedirectPath()
    {
        var map = CreateMap(("/old-page", "/new-page"));
        var middleware = RedirectMiddleware.Create(map);
        var context = CreateContext("/old-page");

        var response = await middleware(context, () => throw new InvalidOperationException("next should not be called"));

        response.StatusCode.ShouldBe(301);
    }

    [Fact]
    public async Task ShouldPassThroughForUnmatchedPath()
    {
        var map = CreateMap(("/old-page", "/new-page"));
        var middleware = RedirectMiddleware.Create(map);
        var context = CreateContext("/other-page");
        var downstream = new AtollResponse(200);

        var response = await middleware(context, () => Task.FromResult(downstream));

        response.ShouldBeSameAs(downstream);
    }

    [Fact]
    public async Task ShouldNormalizeTrailingSlashBeforeLookup()
    {
        var map = CreateMap(("/old-page", "/new-page"));
        var middleware = RedirectMiddleware.Create(map);
        var context = CreateContext("/old-page/");

        var response = await middleware(context, () => throw new InvalidOperationException("next should not be called"));

        response.StatusCode.ShouldBe(301);
    }

    [Fact]
    public async Task ShouldNormalizeCaseBeforeLookup()
    {
        var map = CreateMap(("/old-page", "/new-page"));
        var middleware = RedirectMiddleware.Create(map);
        var context = CreateContext("/OLD-PAGE");

        var response = await middleware(context, () => throw new InvalidOperationException("next should not be called"));

        response.StatusCode.ShouldBe(301);
    }

    [Fact]
    public async Task ShouldSetLocationHeaderToTarget()
    {
        var map = CreateMap(("/old-page", "/new-page"));
        var middleware = RedirectMiddleware.Create(map);
        var context = CreateContext("/old-page");

        var response = await middleware(context, () => throw new InvalidOperationException("next should not be called"));

        response.Headers.ContainsKey("Location").ShouldBeTrue();
        response.Headers["Location"].ShouldBe("/new-page");
    }

    [Fact]
    public async Task ShouldNotCallNextOnMatch()
    {
        var map = CreateMap(("/old-page", "/new-page"));
        var middleware = RedirectMiddleware.Create(map);
        var context = CreateContext("/old-page");
        var nextCalled = false;

        await middleware(context, () =>
        {
            nextCalled = true;
            return Task.FromResult(new AtollResponse(200));
        });

        nextCalled.ShouldBeFalse();
    }

    [Fact]
    public void ShouldThrowForNullRedirectMap()
    {
        Should.Throw<ArgumentNullException>(() => RedirectMiddleware.Create(null!));
    }

    [Fact]
    public async Task ShouldSupportCustomStatusCode()
    {
        var map = CreateMap(("/old-page", "/new-page"));
        var middleware = RedirectMiddleware.Create(map, 302);
        var context = CreateContext("/old-page");

        var response = await middleware(context, () => throw new InvalidOperationException("next should not be called"));

        response.StatusCode.ShouldBe(302);
    }

    [Fact]
    public async Task ShouldPreserveQueryStringOnRedirect()
    {
        var map = CreateMap(("/old-page", "/new-page"));
        var middleware = RedirectMiddleware.Create(map);
        var context = CreateContext("/old-page", "?foo=bar&baz=qux");

        var response = await middleware(context, () => throw new InvalidOperationException("next should not be called"));

        response.Headers["Location"].ShouldBe("/new-page?foo=bar&baz=qux");
    }

    [Fact]
    public async Task ShouldNotAppendQueryStringWhenNonePresent()
    {
        var map = CreateMap(("/old-page", "/new-page"));
        var middleware = RedirectMiddleware.Create(map);
        var context = CreateContext("/old-page");

        var response = await middleware(context, () => throw new InvalidOperationException("next should not be called"));

        response.Headers["Location"].ShouldBe("/new-page");
    }
}
