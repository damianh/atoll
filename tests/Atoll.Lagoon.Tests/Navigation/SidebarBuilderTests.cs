using Atoll.Lagoon.Configuration;
using Atoll.Lagoon.Navigation;
using Shouldly;
using Xunit;

namespace Atoll.Lagoon.Tests.Navigation;

public sealed class SidebarBuilderTests
{
    // --- Helper builders ---

    private static SidebarEntry Entry(string label, string href, string slug, int order = 0, string? badge = null)
        => new SidebarEntry(label, href, slug, order, badge);

    private static SidebarItem LinkItem(string label, string link, string? badge = null)
        => new SidebarItem { Label = label, Link = link, Badge = badge };

    private static SidebarItem GroupItem(string label, IReadOnlyList<SidebarItem> items, bool collapsed = false)
        => new SidebarItem { Label = label, Items = items, Collapsed = collapsed };

    private static SidebarItem AutoItem(string label, string dir, bool collapsed = false)
        => new SidebarItem { Label = label, AutoGenerate = dir, Collapsed = collapsed };

    // --- Manual sidebar ---

    [Fact]
    public void BuildShouldReturnEmptyWhenConfigIsEmpty()
    {
        var builder = new SidebarBuilder([], []);

        var result = builder.Build("/docs/");

        result.ShouldBeEmpty();
    }

    [Fact]
    public void BuildShouldResolveSingleLeafLinkItem()
    {
        var builder = new SidebarBuilder(
            [LinkItem("Home", "/docs/")],
            []);

        var result = builder.Build("/other/");

        result.Count.ShouldBe(1);
        result[0].Label.ShouldBe("Home");
        result[0].Href.ShouldBe("/docs/");
        result[0].IsGroup.ShouldBeFalse();
        result[0].IsCurrent.ShouldBeFalse();
        result[0].IsActive.ShouldBeFalse();
    }

    [Fact]
    public void BuildShouldMarkCurrentPageAsCurrentAndActive()
    {
        var builder = new SidebarBuilder(
            [LinkItem("Home", "/docs/")],
            []);

        var result = builder.Build("/docs/");

        result[0].IsCurrent.ShouldBeTrue();
        result[0].IsActive.ShouldBeTrue();
    }

    [Fact]
    public void BuildShouldMatchCurrentPageCaseInsensitively()
    {
        var builder = new SidebarBuilder(
            [LinkItem("Page", "/Docs/Guide/")],
            []);

        var result = builder.Build("/docs/guide/");

        result[0].IsCurrent.ShouldBeTrue();
    }

    [Fact]
    public void BuildShouldMatchCurrentPageIgnoringTrailingSlash()
    {
        var builder = new SidebarBuilder(
            [LinkItem("Page", "/docs/guide")],
            []);

        var result = builder.Build("/docs/guide/");

        result[0].IsCurrent.ShouldBeTrue();
    }

    [Fact]
    public void BuildShouldPreserveItemBadge()
    {
        var builder = new SidebarBuilder(
            [LinkItem("New Feature", "/docs/new", "New")],
            []);

        var result = builder.Build("/other/");

        result[0].Badge.ShouldBe("New");
    }

    // --- Group items ---

    [Fact]
    public void BuildShouldResolveManualGroupWithChildren()
    {
        var builder = new SidebarBuilder(
            [GroupItem("Guides", [
                LinkItem("Getting Started", "/docs/guides/start"),
                LinkItem("Advanced", "/docs/guides/advanced")
            ])],
            []);

        var result = builder.Build("/other/");

        result.Count.ShouldBe(1);
        result[0].IsGroup.ShouldBeTrue();
        result[0].Label.ShouldBe("Guides");
        result[0].Items.Count.ShouldBe(2);
        result[0].IsActive.ShouldBeFalse();
    }

    [Fact]
    public void BuildShouldMarkGroupAsActiveWhenChildIsCurrent()
    {
        var builder = new SidebarBuilder(
            [GroupItem("Guides", [
                LinkItem("Getting Started", "/docs/guides/start"),
                LinkItem("Advanced", "/docs/guides/advanced")
            ])],
            []);

        var result = builder.Build("/docs/guides/start");

        result[0].IsActive.ShouldBeTrue();
        result[0].Items[0].IsCurrent.ShouldBeTrue();
        result[0].Items[1].IsCurrent.ShouldBeFalse();
    }

    [Fact]
    public void BuildShouldSupportNestedGroups()
    {
        var builder = new SidebarBuilder(
            [GroupItem("Docs", [
                GroupItem("Guides", [
                    LinkItem("Start", "/docs/guides/start")
                ])
            ])],
            []);

        var result = builder.Build("/docs/guides/start");

        result[0].IsActive.ShouldBeTrue();
        result[0].Items[0].IsActive.ShouldBeTrue();
        result[0].Items[0].Items[0].IsCurrent.ShouldBeTrue();
    }

    [Fact]
    public void BuildShouldPreserveGroupCollapsedState()
    {
        var builder = new SidebarBuilder(
            [GroupItem("Guides", [LinkItem("Start", "/docs/start")], collapsed: true)],
            []);

        var result = builder.Build("/other/");

        result[0].Collapsed.ShouldBeTrue();
    }

    // --- Auto-generate sidebar ---

    [Fact]
    public void BuildShouldAutoGenerateGroupFromEntries()
    {
        var entries = new[]
        {
            Entry("Getting Started", "/docs/guides/start", "guides/start"),
            Entry("Advanced", "/docs/guides/advanced", "guides/advanced")
        };

        var builder = new SidebarBuilder([AutoItem("Guides", "guides")], entries);

        var result = builder.Build("/other/");

        result.Count.ShouldBe(1);
        result[0].IsGroup.ShouldBeTrue();
        result[0].Label.ShouldBe("Guides");
        result[0].Items.Count.ShouldBe(2);
    }

    [Fact]
    public void BuildShouldAutoGenerateAndMarkCurrentPage()
    {
        var entries = new[]
        {
            Entry("Getting Started", "/docs/guides/start", "guides/start"),
            Entry("Advanced", "/docs/guides/advanced", "guides/advanced")
        };

        var builder = new SidebarBuilder([AutoItem("Guides", "guides")], entries);

        var result = builder.Build("/docs/guides/start");

        result[0].IsActive.ShouldBeTrue();
        result[0].Items.Single(i => i.Href == "/docs/guides/start").IsCurrent.ShouldBeTrue();
        result[0].Items.Single(i => i.Href == "/docs/guides/advanced").IsCurrent.ShouldBeFalse();
    }

    [Fact]
    public void BuildShouldAutoGenerateAndSortByOrder()
    {
        var entries = new[]
        {
            Entry("Second", "/docs/guides/second", "guides/second", order: 2),
            Entry("First", "/docs/guides/first", "guides/first", order: 1),
            Entry("Third", "/docs/guides/third", "guides/third", order: 3)
        };

        var builder = new SidebarBuilder([AutoItem("Guides", "guides")], entries);

        var result = builder.Build("/other/");

        result[0].Items[0].Label.ShouldBe("First");
        result[0].Items[1].Label.ShouldBe("Second");
        result[0].Items[2].Label.ShouldBe("Third");
    }

    [Fact]
    public void BuildShouldAutoGenerateWithEmptyDirMatchesAllEntries()
    {
        var entries = new[]
        {
            Entry("Page A", "/docs/a", "a"),
            Entry("Page B", "/docs/b", "b")
        };

        var builder = new SidebarBuilder([AutoItem("All", "")], entries);

        var result = builder.Build("/other/");

        result[0].Items.Count.ShouldBe(2);
    }

    [Fact]
    public void BuildShouldAutoGenerateOnlyMatchEntriesWithinDirectory()
    {
        var entries = new[]
        {
            Entry("Guide", "/docs/guides/guide", "guides/guide"),
            Entry("Reference", "/docs/reference/ref", "reference/ref")
        };

        var builder = new SidebarBuilder([AutoItem("Guides", "guides")], entries);

        var result = builder.Build("/other/");

        result[0].Items.Count.ShouldBe(1);
        result[0].Items[0].Label.ShouldBe("Guide");
    }

    // --- Mixed manual + auto-generate ---

    [Fact]
    public void BuildShouldSupportMixedManualAndAutoGenerateItems()
    {
        var entries = new[]
        {
            Entry("Guide", "/docs/guides/guide", "guides/guide")
        };

        var config = new SidebarItem[]
        {
            LinkItem("Home", "/docs/"),
            AutoItem("Guides", "guides"),
            GroupItem("Reference", [LinkItem("API", "/docs/reference/api")])
        };

        var builder = new SidebarBuilder(config, entries);

        var result = builder.Build("/docs/");

        result.Count.ShouldBe(3);
        result[0].Label.ShouldBe("Home");
        result[1].Label.ShouldBe("Guides");
        result[2].Label.ShouldBe("Reference");
    }

    // --- Flatten ---

    [Fact]
    public void FlattenShouldReturnAllLinkItemsInOrder()
    {
        var items = new ResolvedSidebarItem[]
        {
            new ResolvedSidebarItem("Home", "/", false, null),
            new ResolvedSidebarItem("Guides", false, null, false, [
                new ResolvedSidebarItem("Start", "/guides/start", false, null),
                new ResolvedSidebarItem("Advanced", "/guides/advanced", false, null)
            ]),
            new ResolvedSidebarItem("API", "/api", false, null)
        };

        var flat = SidebarBuilder.Flatten(items);

        flat.Count.ShouldBe(4);
        flat[0].Href.ShouldBe("/");
        flat[1].Href.ShouldBe("/guides/start");
        flat[2].Href.ShouldBe("/guides/advanced");
        flat[3].Href.ShouldBe("/api");
    }

    [Fact]
    public void FlattenShouldReturnEmptyForEmptyInput()
    {
        var flat = SidebarBuilder.Flatten([]);

        flat.ShouldBeEmpty();
    }

    [Fact]
    public void FlattenShouldSkipGroupHeaders()
    {
        var items = new ResolvedSidebarItem[]
        {
            new ResolvedSidebarItem("Group", false, null, false, [
                new ResolvedSidebarItem("Child", "/child", false, null)
            ])
        };

        var flat = SidebarBuilder.Flatten(items);

        flat.Count.ShouldBe(1);
        flat[0].IsGroup.ShouldBeFalse();
    }
}
