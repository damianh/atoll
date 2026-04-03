using Atoll.Routing.FileSystem;
using Shouldly;
using Xunit;

namespace Atoll.Routing.Tests;

public sealed class RouteConventionsTests
{
    // --- FilePathToPattern tests ---

    [Fact]
    public void ShouldMapIndexFileToRoot()
    {
        RouteConventions.FilePathToPattern("index.cs").ShouldBe("/");
    }

    [Fact]
    public void ShouldMapSimpleFileToRoute()
    {
        RouteConventions.FilePathToPattern("about.cs").ShouldBe("/about");
    }

    [Fact]
    public void ShouldMapNestedFileToRoute()
    {
        RouteConventions.FilePathToPattern("blog/post.cs").ShouldBe("/blog/post");
    }

    [Fact]
    public void ShouldMapNestedIndexToDirectory()
    {
        RouteConventions.FilePathToPattern("blog/index.cs").ShouldBe("/blog");
    }

    [Fact]
    public void ShouldMapDynamicSegment()
    {
        RouteConventions.FilePathToPattern("blog/[slug].cs").ShouldBe("/blog/[slug]");
    }

    [Fact]
    public void ShouldMapCatchAllSegment()
    {
        RouteConventions.FilePathToPattern("[...rest].cs").ShouldBe("/[...rest]");
    }

    [Fact]
    public void ShouldMapNestedCatchAll()
    {
        RouteConventions.FilePathToPattern("docs/[...rest].cs").ShouldBe("/docs/[...rest]");
    }

    [Fact]
    public void ShouldMapDeeplyNestedFile()
    {
        RouteConventions.FilePathToPattern("a/b/c/page.cs").ShouldBe("/a/b/c/page");
    }

    [Fact]
    public void ShouldMapDeeplyNestedIndex()
    {
        RouteConventions.FilePathToPattern("a/b/c/index.cs").ShouldBe("/a/b/c");
    }

    [Fact]
    public void ShouldNormalizeBackslashesToForwardSlashes()
    {
        RouteConventions.FilePathToPattern("blog\\[slug].cs").ShouldBe("/blog/[slug]");
    }

    [Fact]
    public void ShouldThrowForNonCsFile()
    {
        var ex = Should.Throw<ArgumentException>(() => RouteConventions.FilePathToPattern("about.txt"));
        ex.ParamName.ShouldBe("relativeFilePath");
    }

    [Fact]
    public void ShouldThrowForNullFilePath()
    {
        Should.Throw<ArgumentNullException>(() => RouteConventions.FilePathToPattern(null!));
    }

    [Fact]
    public void ShouldMapMultipleDynamicSegments()
    {
        RouteConventions.FilePathToPattern("blog/[year]/[slug].cs").ShouldBe("/blog/[year]/[slug]");
    }

    // --- IsDynamicSegment / IsCatchAllSegment tests ---

    [Fact]
    public void ShouldIdentifyDynamicSegment()
    {
        RouteConventions.IsDynamicSegment("[slug]").ShouldBeTrue();
    }

    [Fact]
    public void ShouldNotIdentifyStaticAsDynamic()
    {
        RouteConventions.IsDynamicSegment("about").ShouldBeFalse();
    }

    [Fact]
    public void ShouldIdentifyCatchAllSegment()
    {
        RouteConventions.IsCatchAllSegment("[...rest]").ShouldBeTrue();
    }

    [Fact]
    public void ShouldNotIdentifyDynamicAsCatchAll()
    {
        RouteConventions.IsCatchAllSegment("[slug]").ShouldBeFalse();
    }

    [Fact]
    public void ShouldNotIdentifyStaticAsCatchAll()
    {
        RouteConventions.IsCatchAllSegment("about").ShouldBeFalse();
    }

    // --- ExtractParameterName tests ---

    [Fact]
    public void ShouldExtractDynamicParameterName()
    {
        RouteConventions.ExtractParameterName("[slug]").ShouldBe("slug");
    }

    [Fact]
    public void ShouldExtractCatchAllParameterName()
    {
        RouteConventions.ExtractParameterName("[...rest]").ShouldBe("rest");
    }

    [Fact]
    public void ShouldThrowForStaticExtractParameterName()
    {
        Should.Throw<ArgumentException>(() => RouteConventions.ExtractParameterName("about"));
    }

    // --- ParseSegments tests ---

    [Fact]
    public void ShouldParseRootAsEmptySegments()
    {
        RouteConventions.ParseSegments("/").ShouldBeEmpty();
    }

    [Fact]
    public void ShouldParseSingleStaticSegment()
    {
        var segments = RouteConventions.ParseSegments("/about");
        segments.Length.ShouldBe(1);
        segments[0].SegmentType.ShouldBe(RouteSegmentType.Static);
        segments[0].Value.ShouldBe("about");
    }

    [Fact]
    public void ShouldParseDynamicSegment()
    {
        var segments = RouteConventions.ParseSegments("/blog/[slug]");
        segments.Length.ShouldBe(2);
        segments[0].SegmentType.ShouldBe(RouteSegmentType.Static);
        segments[0].Value.ShouldBe("blog");
        segments[1].SegmentType.ShouldBe(RouteSegmentType.Dynamic);
        segments[1].Value.ShouldBe("slug");
    }

    [Fact]
    public void ShouldParseCatchAllSegment()
    {
        var segments = RouteConventions.ParseSegments("/docs/[...rest]");
        segments.Length.ShouldBe(2);
        segments[0].SegmentType.ShouldBe(RouteSegmentType.Static);
        segments[0].Value.ShouldBe("docs");
        segments[1].SegmentType.ShouldBe(RouteSegmentType.CatchAll);
        segments[1].Value.ShouldBe("rest");
    }

    [Fact]
    public void ShouldParseMultipleDynamicSegments()
    {
        var segments = RouteConventions.ParseSegments("/blog/[year]/[slug]");
        segments.Length.ShouldBe(3);
        segments[0].SegmentType.ShouldBe(RouteSegmentType.Static);
        segments[0].Value.ShouldBe("blog");
        segments[1].SegmentType.ShouldBe(RouteSegmentType.Dynamic);
        segments[1].Value.ShouldBe("year");
        segments[2].SegmentType.ShouldBe(RouteSegmentType.Dynamic);
        segments[2].Value.ShouldBe("slug");
    }

    [Fact]
    public void ShouldThrowWhenCatchAllIsNotLastSegment()
    {
        Should.Throw<ArgumentException>(() =>
            RouteConventions.ParseSegments("/[...rest]/more"));
    }

    [Fact]
    public void ShouldParseSoleCatchAll()
    {
        var segments = RouteConventions.ParseSegments("/[...rest]");
        segments.Length.ShouldBe(1);
        segments[0].SegmentType.ShouldBe(RouteSegmentType.CatchAll);
        segments[0].Value.ShouldBe("rest");
    }

    [Fact]
    public void ShouldThrowForNullPattern()
    {
        Should.Throw<ArgumentNullException>(() => RouteConventions.ParseSegments(null!));
    }
}
