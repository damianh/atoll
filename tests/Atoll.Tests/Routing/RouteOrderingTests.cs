using Atoll.Components;
using Atoll.Routing.Matching;
using Shouldly;
using Xunit;

namespace Atoll.Routing.Tests;

public sealed class RouteOrderingTests
{
    [Fact]
    public void ShouldOrderStaticBeforeDynamic()
    {
        var matcher = CreateMatcher(
            ("/blog/[slug]", typeof(StubDynamicPage)),
            ("/blog/archive", typeof(StubStaticPage)));

        var sorted = matcher.SortedRoutes;
        sorted[0].Pattern.ShouldBe("/blog/archive");
        sorted[1].Pattern.ShouldBe("/blog/[slug]");
    }

    [Fact]
    public void ShouldOrderDynamicBeforeCatchAll()
    {
        var matcher = CreateMatcher(
            ("/docs/[...rest]", typeof(StubCatchAllPage)),
            ("/docs/[id]", typeof(StubDynamicPage)));

        var sorted = matcher.SortedRoutes;
        sorted[0].Pattern.ShouldBe("/docs/[id]");
        sorted[1].Pattern.ShouldBe("/docs/[...rest]");
    }

    [Fact]
    public void ShouldOrderStaticBeforeCatchAll()
    {
        var matcher = CreateMatcher(
            ("/docs/[...rest]", typeof(StubCatchAllPage)),
            ("/docs/intro", typeof(StubStaticPage)));

        var sorted = matcher.SortedRoutes;
        sorted[0].Pattern.ShouldBe("/docs/intro");
        sorted[1].Pattern.ShouldBe("/docs/[...rest]");
    }

    [Fact]
    public void ShouldOrderMoreSpecificDynamicFirst()
    {
        var matcher = CreateMatcher(
            ("/[slug]", typeof(StubDynamicPage)),
            ("/blog/[slug]", typeof(StubDynamic2Page)));

        var sorted = matcher.SortedRoutes;
        sorted[0].Pattern.ShouldBe("/blog/[slug]");
        sorted[1].Pattern.ShouldBe("/[slug]");
    }

    [Fact]
    public void ShouldOrderRootCorrectly()
    {
        var matcher = CreateMatcher(
            ("/[...rest]", typeof(StubCatchAllPage)),
            ("/about", typeof(StubStaticPage)),
            ("/", typeof(StubIndexPage)));

        var sorted = matcher.SortedRoutes;
        sorted[0].Pattern.ShouldBe("/about");
        sorted[1].Pattern.ShouldBe("/");
        sorted[2].Pattern.ShouldBe("/[...rest]");
    }

    [Fact]
    public void ShouldOrderComplexRouteTable()
    {
        var matcher = CreateMatcher(
            ("/[...rest]", typeof(StubCatchAllPage)),
            ("/blog/[slug]", typeof(StubDynamic2Page)),
            ("/", typeof(StubIndexPage)),
            ("/blog/archive", typeof(StubStaticPage)),
            ("/about", typeof(StubDynamicPage)),
            ("/blog/[year]/[slug]", typeof(StubDynamic3Page)));

        var sorted = matcher.SortedRoutes;

        // Most specific routes first (by segment count, then static count):
        // 1. /blog/[year]/[slug] (3 non-catch-all segments, 1 static)
        // 2. /blog/archive (2 non-catch-all segments, 2 static)
        // 3. /about (1 non-catch-all segment, 1 static) — alphabetically before /blog/[slug]
        // 4. /blog/[slug] (2 non-catch-all segments, 1 static)
        // 5. / (root, 0 segments)
        // 6. /[...rest] (catch-all, always last)
        //
        // Note: More segments = more specific (tried first).
        // Routes with different segment counts can't conflict in matching.
        // /blog/[year]/[slug] is tried before /blog/archive but won't match
        // 2-segment paths, so /blog/archive still matches /blog/archive correctly.

        sorted[0].Pattern.ShouldBe("/blog/[year]/[slug]");
        sorted[1].Pattern.ShouldBe("/blog/archive");
        sorted[2].Pattern.ShouldBe("/blog/[slug]");
        sorted[3].Pattern.ShouldBe("/about");
        sorted[4].Pattern.ShouldBe("/");
        sorted[5].Pattern.ShouldBe("/[...rest]");
    }

    [Fact]
    public void ShouldMaintainDeterministicOrderForEqualPriority()
    {
        // Two routes with identical structure should be sorted alphabetically
        var matcher = CreateMatcher(
            ("/zebra", typeof(StubStaticPage)),
            ("/alpha", typeof(StubDynamicPage)));

        var sorted = matcher.SortedRoutes;
        sorted[0].Pattern.ShouldBe("/alpha");
        sorted[1].Pattern.ShouldBe("/zebra");
    }

    [Fact]
    public void ShouldOrderCatchAllWithMoreStaticPrefixFirst()
    {
        var matcher = CreateMatcher(
            ("/[...rest]", typeof(StubCatchAllPage)),
            ("/docs/[...rest]", typeof(StubDynamic2Page)));

        var sorted = matcher.SortedRoutes;
        sorted[0].Pattern.ShouldBe("/docs/[...rest]");
        sorted[1].Pattern.ShouldBe("/[...rest]");
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

    private sealed class StubStaticPage : IAtollComponent
    {
        public Task RenderAsync(RenderContext context) => Task.CompletedTask;
    }

    private sealed class StubDynamicPage : IAtollComponent
    {
        public Task RenderAsync(RenderContext context) => Task.CompletedTask;
    }

    private sealed class StubDynamic2Page : IAtollComponent
    {
        public Task RenderAsync(RenderContext context) => Task.CompletedTask;
    }

    private sealed class StubDynamic3Page : IAtollComponent
    {
        public Task RenderAsync(RenderContext context) => Task.CompletedTask;
    }

    private sealed class StubCatchAllPage : IAtollComponent
    {
        public Task RenderAsync(RenderContext context) => Task.CompletedTask;
    }
}
