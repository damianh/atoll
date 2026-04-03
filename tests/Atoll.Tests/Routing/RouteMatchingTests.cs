using Atoll.Components;
using Atoll.Routing.Matching;
using Shouldly;
using Xunit;

namespace Atoll.Routing.Tests;

public sealed class RouteMatchingTests
{
    // --- Static route matching ---

    [Fact]
    public void ShouldMatchRootRoute()
    {
        var matcher = CreateMatcher(("/", typeof(StubIndexPage)));

        var result = matcher.Match("/");

        result.ShouldNotBeNull();
        result.RouteEntry.Pattern.ShouldBe("/");
        result.Parameters.ShouldBeEmpty();
    }

    [Fact]
    public void ShouldMatchStaticRoute()
    {
        var matcher = CreateMatcher(("/about", typeof(StubAboutPage)));

        var result = matcher.Match("/about");

        result.ShouldNotBeNull();
        result.RouteEntry.Pattern.ShouldBe("/about");
        result.Parameters.ShouldBeEmpty();
    }

    [Fact]
    public void ShouldMatchNestedStaticRoute()
    {
        var matcher = CreateMatcher(("/blog/archive", typeof(StubBlogArchivePage)));

        var result = matcher.Match("/blog/archive");

        result.ShouldNotBeNull();
        result.RouteEntry.Pattern.ShouldBe("/blog/archive");
    }

    [Fact]
    public void ShouldMatchStaticRouteCaseInsensitively()
    {
        var matcher = CreateMatcher(("/About", typeof(StubAboutPage)));

        var result = matcher.Match("/about");

        result.ShouldNotBeNull();
        result.RouteEntry.Pattern.ShouldBe("/About");
    }

    [Fact]
    public void ShouldReturnNullForUnmatchedRoute()
    {
        var matcher = CreateMatcher(("/about", typeof(StubAboutPage)));

        var result = matcher.Match("/contact");

        result.ShouldBeNull();
    }

    // --- Dynamic route matching ---

    [Fact]
    public void ShouldMatchDynamicSegment()
    {
        var matcher = CreateMatcher(("/blog/[slug]", typeof(StubBlogPostPage)));

        var result = matcher.Match("/blog/hello-world");

        result.ShouldNotBeNull();
        result.RouteEntry.Pattern.ShouldBe("/blog/[slug]");
        result.Parameters["slug"].ShouldBe("hello-world");
    }

    [Fact]
    public void ShouldMatchMultipleDynamicSegments()
    {
        var matcher = CreateMatcher(("/blog/[year]/[slug]", typeof(StubBlogPostPage)));

        var result = matcher.Match("/blog/2024/hello-world");

        result.ShouldNotBeNull();
        result.Parameters["year"].ShouldBe("2024");
        result.Parameters["slug"].ShouldBe("hello-world");
    }

    [Fact]
    public void ShouldNotMatchDynamicRouteWithFewerSegments()
    {
        var matcher = CreateMatcher(("/blog/[slug]", typeof(StubBlogPostPage)));

        var result = matcher.Match("/blog");

        result.ShouldBeNull();
    }

    [Fact]
    public void ShouldNotMatchDynamicRouteWithMoreSegments()
    {
        var matcher = CreateMatcher(("/blog/[slug]", typeof(StubBlogPostPage)));

        var result = matcher.Match("/blog/hello/extra");

        result.ShouldBeNull();
    }

    // --- Catch-all route matching ---

    [Fact]
    public void ShouldMatchCatchAllRoute()
    {
        var matcher = CreateMatcher(("/docs/[...rest]", typeof(StubDocsPage)));

        var result = matcher.Match("/docs/a/b/c");

        result.ShouldNotBeNull();
        result.RouteEntry.Pattern.ShouldBe("/docs/[...rest]");
        result.Parameters["rest"].ShouldBe("a/b/c");
    }

    [Fact]
    public void ShouldMatchCatchAllWithSingleSegment()
    {
        var matcher = CreateMatcher(("/docs/[...rest]", typeof(StubDocsPage)));

        var result = matcher.Match("/docs/intro");

        result.ShouldNotBeNull();
        result.Parameters["rest"].ShouldBe("intro");
    }

    [Fact]
    public void ShouldMatchCatchAllWithEmptyRemainder()
    {
        var matcher = CreateMatcher(("/docs/[...rest]", typeof(StubDocsPage)));

        var result = matcher.Match("/docs");

        result.ShouldNotBeNull();
        result.Parameters["rest"].ShouldBe(string.Empty);
    }

    [Fact]
    public void ShouldMatchSoleCatchAllRoute()
    {
        var matcher = CreateMatcher(("/[...rest]", typeof(StubCatchAllPage)));

        var result = matcher.Match("/anything/goes/here");

        result.ShouldNotBeNull();
        result.Parameters["rest"].ShouldBe("anything/goes/here");
    }

    [Fact]
    public void ShouldMatchSoleCatchAllWithEmptyPath()
    {
        var matcher = CreateMatcher(("/[...rest]", typeof(StubCatchAllPage)));

        var result = matcher.Match("/");

        result.ShouldNotBeNull();
        result.Parameters["rest"].ShouldBe(string.Empty);
    }

    // --- Priority / specificity tests ---

    [Fact]
    public void ShouldPreferStaticOverDynamic()
    {
        var matcher = CreateMatcher(
            ("/blog/[slug]", typeof(StubBlogPostPage)),
            ("/blog/archive", typeof(StubBlogArchivePage)));

        var result = matcher.Match("/blog/archive");

        result.ShouldNotBeNull();
        result.RouteEntry.ComponentType.ShouldBe(typeof(StubBlogArchivePage));
    }

    [Fact]
    public void ShouldPreferDynamicOverCatchAll()
    {
        var matcher = CreateMatcher(
            ("/docs/[...rest]", typeof(StubDocsPage)),
            ("/docs/[id]", typeof(StubDocPage)));

        var result = matcher.Match("/docs/intro");

        result.ShouldNotBeNull();
        result.RouteEntry.ComponentType.ShouldBe(typeof(StubDocPage));
    }

    [Fact]
    public void ShouldPreferStaticOverCatchAll()
    {
        var matcher = CreateMatcher(
            ("/docs/[...rest]", typeof(StubDocsPage)),
            ("/docs/intro", typeof(StubDocIntroPage)));

        var result = matcher.Match("/docs/intro");

        result.ShouldNotBeNull();
        result.RouteEntry.ComponentType.ShouldBe(typeof(StubDocIntroPage));
    }

    [Fact]
    public void ShouldFallThroughToCatchAllWhenDynamicDoesNotMatch()
    {
        var matcher = CreateMatcher(
            ("/docs/[id]", typeof(StubDocPage)),
            ("/docs/[...rest]", typeof(StubDocsPage)));

        var result = matcher.Match("/docs/a/b/c");

        result.ShouldNotBeNull();
        result.RouteEntry.ComponentType.ShouldBe(typeof(StubDocsPage));
        result.Parameters["rest"].ShouldBe("a/b/c");
    }

    [Fact]
    public void ShouldPreferRootOverCatchAll()
    {
        var matcher = CreateMatcher(
            ("/[...rest]", typeof(StubCatchAllPage)),
            ("/", typeof(StubIndexPage)));

        var result = matcher.Match("/");

        result.ShouldNotBeNull();
        result.RouteEntry.ComponentType.ShouldBe(typeof(StubIndexPage));
    }

    [Fact]
    public void ShouldMatchWithTrailingSlash()
    {
        var matcher = CreateMatcher(("/about", typeof(StubAboutPage)));

        var result = matcher.Match("/about/");

        // Trailing slash should still match
        result.ShouldNotBeNull();
        result.RouteEntry.Pattern.ShouldBe("/about");
    }

    // --- Edge cases ---

    [Fact]
    public void ShouldReturnNullForEmptyRouteTable()
    {
        var matcher = new RouteMatcher([]);

        var result = matcher.Match("/anything");

        result.ShouldBeNull();
    }

    [Fact]
    public void ShouldThrowForNullPath()
    {
        var matcher = new RouteMatcher([]);
        Should.Throw<ArgumentNullException>(() => matcher.Match(null!));
    }

    [Fact]
    public void ShouldThrowForNullRoutes()
    {
        Should.Throw<ArgumentNullException>(() => new RouteMatcher(null!));
    }

    [Fact]
    public void ShouldExtractParametersCaseInsensitively()
    {
        var matcher = CreateMatcher(("/blog/[slug]", typeof(StubBlogPostPage)));

        var result = matcher.Match("/blog/Hello-World");

        result.ShouldNotBeNull();
        result.Parameters["slug"].ShouldBe("Hello-World");
        // Parameter name lookup should be case-insensitive
        result.Parameters["Slug"].ShouldBe("Hello-World");
    }

    // --- Helper methods ---

    private static RouteMatcher CreateMatcher(params (string Pattern, Type ComponentType)[] routes)
    {
        var entries = routes.Select(r =>
            new RouteEntry(r.Pattern, r.ComponentType, r.Pattern + ".cs")).ToList();
        return new RouteMatcher(entries);
    }

    // --- Stub types ---

    private sealed class StubIndexPage : IAtollComponent
    {
        public Task RenderAsync(RenderContext context) => Task.CompletedTask;
    }

    private sealed class StubAboutPage : IAtollComponent
    {
        public Task RenderAsync(RenderContext context) => Task.CompletedTask;
    }

    private sealed class StubBlogPostPage : IAtollComponent
    {
        public Task RenderAsync(RenderContext context) => Task.CompletedTask;
    }

    private sealed class StubBlogArchivePage : IAtollComponent
    {
        public Task RenderAsync(RenderContext context) => Task.CompletedTask;
    }

    private sealed class StubDocsPage : IAtollComponent
    {
        public Task RenderAsync(RenderContext context) => Task.CompletedTask;
    }

    private sealed class StubDocPage : IAtollComponent
    {
        public Task RenderAsync(RenderContext context) => Task.CompletedTask;
    }

    private sealed class StubDocIntroPage : IAtollComponent
    {
        public Task RenderAsync(RenderContext context) => Task.CompletedTask;
    }

    private sealed class StubCatchAllPage : IAtollComponent
    {
        public Task RenderAsync(RenderContext context) => Task.CompletedTask;
    }
}
