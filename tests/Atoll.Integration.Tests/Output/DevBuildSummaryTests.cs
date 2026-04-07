using Atoll.Cli.Output;
using Shouldly;
using Xunit;

namespace Atoll.Integration.Tests.Output;

public sealed class DevBuildSummaryTests
{
    [Fact]
    public void ShouldIncludeRouteCount()
    {
        var result = DevBuildSummary.Format(
            routeCount: 12,
            islandAssetCount: 0,
            hasGlobalCss: false,
            hasSearchIndex: false,
            elapsedMilliseconds: 100);

        result.ShouldContain("12 discovered");
    }

    [Fact]
    public void ShouldIncludeElapsedTime()
    {
        var result = DevBuildSummary.Format(
            routeCount: 5,
            islandAssetCount: 0,
            hasGlobalCss: false,
            hasSearchIndex: false,
            elapsedMilliseconds: 342);

        result.ShouldContain("342ms");
    }

    [Fact]
    public void ShouldIncludeIslandCountWhenPresent()
    {
        var result = DevBuildSummary.Format(
            routeCount: 5,
            islandAssetCount: 3,
            hasGlobalCss: false,
            hasSearchIndex: false,
            elapsedMilliseconds: 100);

        result.ShouldContain("3 JS asset(s)");
    }

    [Fact]
    public void ShouldNotIncludeIslandLineWhenNone()
    {
        var result = DevBuildSummary.Format(
            routeCount: 5,
            islandAssetCount: 0,
            hasGlobalCss: false,
            hasSearchIndex: false,
            elapsedMilliseconds: 100);

        result.ShouldNotContain("Islands");
    }

    [Fact]
    public void ShouldIncludeCssLineWhenGlobalCssPresent()
    {
        var result = DevBuildSummary.Format(
            routeCount: 5,
            islandAssetCount: 0,
            hasGlobalCss: true,
            hasSearchIndex: false,
            elapsedMilliseconds: 100);

        result.ShouldContain("global styles loaded");
    }

    [Fact]
    public void ShouldNotIncludeCssLineWhenNoCss()
    {
        var result = DevBuildSummary.Format(
            routeCount: 5,
            islandAssetCount: 0,
            hasGlobalCss: false,
            hasSearchIndex: false,
            elapsedMilliseconds: 100);

        result.ShouldNotContain("CSS");
    }

    [Fact]
    public void ShouldIncludeSearchLineWhenSearchIndexPresent()
    {
        var result = DevBuildSummary.Format(
            routeCount: 5,
            islandAssetCount: 0,
            hasGlobalCss: false,
            hasSearchIndex: true,
            elapsedMilliseconds: 100);

        result.ShouldContain("index generated");
    }

    [Fact]
    public void ShouldNotIncludeSearchLineWhenNoSearchIndex()
    {
        var result = DevBuildSummary.Format(
            routeCount: 5,
            islandAssetCount: 0,
            hasGlobalCss: false,
            hasSearchIndex: false,
            elapsedMilliseconds: 100);

        result.ShouldNotContain("Search");
    }

    [Fact]
    public void ShouldFormatFullSummaryWithAllOptions()
    {
        var result = DevBuildSummary.Format(
            routeCount: 15,
            islandAssetCount: 2,
            hasGlobalCss: true,
            hasSearchIndex: true,
            elapsedMilliseconds: 500);

        result.ShouldContain("15 discovered");
        result.ShouldContain("2 JS asset(s)");
        result.ShouldContain("global styles loaded");
        result.ShouldContain("index generated");
        result.ShouldContain("500ms");
    }

    [Fact]
    public void ShouldHandleZeroRoutes()
    {
        var result = DevBuildSummary.Format(
            routeCount: 0,
            islandAssetCount: 0,
            hasGlobalCss: false,
            hasSearchIndex: false,
            elapsedMilliseconds: 50);

        result.ShouldContain("0 discovered");
    }
}
