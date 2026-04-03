using Atoll.Docs.Navigation;
using Shouldly;
using Xunit;

namespace Atoll.Docs.Tests.Navigation;

public sealed class BreadcrumbBuilderTests
{
    private static ResolvedSidebarItem Link(string label, string href)
        => new ResolvedSidebarItem(label, href, false, null);

    private static ResolvedSidebarItem Group(string label, IReadOnlyList<ResolvedSidebarItem> items)
        => new ResolvedSidebarItem(label, false, null, false, items);

    // --- Not found ---

    [Fact]
    public void BuildShouldReturnEmptyWhenCurrentPageNotFound()
    {
        var builder = new BreadcrumbBuilder([Link("Home", "/")]);

        var result = builder.Build("/not-found");

        result.ShouldBeEmpty();
    }

    [Fact]
    public void BuildShouldReturnEmptyForEmptySidebar()
    {
        var builder = new BreadcrumbBuilder([]);

        var result = builder.Build("/docs/page");

        result.ShouldBeEmpty();
    }

    // --- Top-level page ---

    [Fact]
    public void BuildShouldReturnSingleCrumbForTopLevelPage()
    {
        var builder = new BreadcrumbBuilder([Link("Home", "/")]);

        var result = builder.Build("/");

        result.Count.ShouldBe(1);
        result[0].Label.ShouldBe("Home");
        result[0].IsCurrent.ShouldBeTrue();
        result[0].Href.ShouldBeNull();
    }

    [Fact]
    public void BuildShouldReturnSingleCrumbForTopLevelPageAmongMultiple()
    {
        var builder = new BreadcrumbBuilder([
            Link("Home", "/"),
            Link("About", "/about")
        ]);

        var result = builder.Build("/about");

        result.Count.ShouldBe(1);
        result[0].Label.ShouldBe("About");
        result[0].IsCurrent.ShouldBeTrue();
    }

    // --- Nested page ---

    [Fact]
    public void BuildShouldReturnGroupThenPageForNestedPage()
    {
        var builder = new BreadcrumbBuilder([
            Group("Guides", [
                Link("Getting Started", "/guides/start")
            ])
        ]);

        var result = builder.Build("/guides/start");

        result.Count.ShouldBe(2);
        result[0].Label.ShouldBe("Guides");
        result[0].IsCurrent.ShouldBeFalse();
        result[0].Href.ShouldBeNull();
        result[1].Label.ShouldBe("Getting Started");
        result[1].IsCurrent.ShouldBeTrue();
        result[1].Href.ShouldBeNull();
    }

    [Fact]
    public void BuildShouldHandleDeeplyNestedGroups()
    {
        var builder = new BreadcrumbBuilder([
            Group("Docs", [
                Group("Guides", [
                    Link("Start", "/docs/guides/start")
                ])
            ])
        ]);

        var result = builder.Build("/docs/guides/start");

        result.Count.ShouldBe(3);
        result[0].Label.ShouldBe("Docs");
        result[1].Label.ShouldBe("Guides");
        result[2].Label.ShouldBe("Start");
        result[2].IsCurrent.ShouldBeTrue();
    }

    // --- Trailing slash normalization ---

    [Fact]
    public void BuildShouldMatchHrefIgnoringTrailingSlash()
    {
        var builder = new BreadcrumbBuilder([Link("Page", "/page/")]);

        var result = builder.Build("/page");

        result.Count.ShouldBe(1);
        result[0].IsCurrent.ShouldBeTrue();
    }

    // --- Case-insensitive matching ---

    [Fact]
    public void BuildShouldMatchHrefCaseInsensitively()
    {
        var builder = new BreadcrumbBuilder([
            Group("Guides", [
                Link("Start", "/Guides/Start")
            ])
        ]);

        var result = builder.Build("/guides/start");

        result.Count.ShouldBe(2);
        result[1].IsCurrent.ShouldBeTrue();
    }

    // --- Non-matching sibling groups not included ---

    [Fact]
    public void BuildShouldNotIncludeNonMatchingGroups()
    {
        var builder = new BreadcrumbBuilder([
            Group("Reference", [
                Link("API", "/reference/api")
            ]),
            Group("Guides", [
                Link("Start", "/guides/start")
            ])
        ]);

        var result = builder.Build("/guides/start");

        result.Count.ShouldBe(2);
        result[0].Label.ShouldBe("Guides");
        result[1].Label.ShouldBe("Start");
    }
}
