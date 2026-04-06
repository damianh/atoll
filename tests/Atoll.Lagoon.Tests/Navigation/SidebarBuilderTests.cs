using Atoll.Lagoon.Configuration;
using Atoll.Lagoon.Navigation;
using Shouldly;
using Xunit;

namespace Atoll.Lagoon.Tests.Navigation;

public sealed class SidebarBuilderTests
{
    // --- Helper builders ---

    private static SidebarEntry Entry(string label, string href, string slug, int order = 0, SidebarBadge? badge = null)
        => new SidebarEntry(label, href, slug, order, badge);

    private static SidebarEntry DraftEntry(string label, string href, string slug, int order = 0)
        => new SidebarEntry(label, href, slug, order, null, draft: true);

    private static SidebarItem LinkItem(string label, string link, SidebarBadge? badge = null)
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

    // --- Locale-aware Build overload ---

    [Fact]
    public void BuildWithLocaleShouldPrefixManualLinkHrefs()
    {
        var builder = new SidebarBuilder(
            [LinkItem("Home", "/docs/"), LinkItem("Guide", "/docs/guide")],
            []);

        var result = builder.Build("/docs/fr/", "/fr", "/docs");

        result[0].Href.ShouldBe("/docs/fr/");
        result[1].Href.ShouldBe("/docs/fr/guide");
    }

    [Fact]
    public void BuildWithLocaleShouldMarkCurrentPageWithLocalePrefix()
    {
        var builder = new SidebarBuilder(
            [LinkItem("Guide", "/docs/guide")],
            []);

        var result = builder.Build("/docs/fr/guide", "/fr", "/docs");

        result[0].IsCurrent.ShouldBeTrue();
    }

    [Fact]
    public void BuildWithLocaleShouldPrefixAutoGeneratedEntryHrefs()
    {
        var entries = new[]
        {
            Entry("Start", "/docs/guides/start", "guides/start", order: 1),
            Entry("Advanced", "/docs/guides/advanced", "guides/advanced", order: 2)
        };

        var builder = new SidebarBuilder([AutoItem("Guides", "guides")], entries);

        var result = builder.Build("/docs/fr/guides/start", "/fr", "/docs");

        result[0].Items[0].Href.ShouldBe("/docs/fr/guides/start");
        result[0].Items[1].Href.ShouldBe("/docs/fr/guides/advanced");
        result[0].Items[0].IsCurrent.ShouldBeTrue();
        result[0].Items[1].IsCurrent.ShouldBeFalse();
    }

    [Fact]
    public void BuildWithLocaleShouldMarkGroupActiveWhenChildIsCurrent()
    {
        var entries = new[]
        {
            Entry("Start", "/docs/guides/start", "guides/start")
        };

        var builder = new SidebarBuilder([AutoItem("Guides", "guides")], entries);

        var result = builder.Build("/docs/es/guides/start", "/es", "/docs");

        result[0].IsActive.ShouldBeTrue();
    }

    [Fact]
    public void BuildWithRootLocaleShouldNotPrefixHrefs()
    {
        var builder = new SidebarBuilder(
            [LinkItem("Guide", "/docs/guide")],
            []);

        var result = builder.Build("/docs/guide", "", "/docs");

        result[0].Href.ShouldBe("/docs/guide");
        result[0].IsCurrent.ShouldBeTrue();
    }

    [Fact]
    public void BuildWithLocaleShouldPrefixNestedGroupChildren()
    {
        var builder = new SidebarBuilder(
            [GroupItem("Docs", [
                GroupItem("Guides", [
                    LinkItem("Start", "/docs/guides/start")
                ])
            ])],
            []);

        var result = builder.Build("/docs/fr/guides/start", "/fr", "/docs");

        result[0].Items[0].Items[0].Href.ShouldBe("/docs/fr/guides/start");
        result[0].Items[0].Items[0].IsCurrent.ShouldBeTrue();
        result[0].Items[0].IsActive.ShouldBeTrue();
        result[0].IsActive.ShouldBeTrue();
    }

    [Fact]
    public void BuildWithLocaleNoBasePathShouldPrefixCorrectly()
    {
        var builder = new SidebarBuilder(
            [LinkItem("Home", "/"), LinkItem("Guide", "/guide")],
            []);

        var result = builder.Build("/fr/guide", "/fr", "");

        result[0].Href.ShouldBe("/fr/");
        result[1].Href.ShouldBe("/fr/guide");
        result[1].IsCurrent.ShouldBeTrue();
    }

    // --- Per-locale auto-generation ---

    [Fact]
    public void BuildWithLocaleKeyShouldFilterAutoGeneratedEntriesToLocale()
    {
        var entries = new[]
        {
            Entry("Start (EN)", "/docs/guides/start", "guides/start", order: 1),
            Entry("Start (FR)", "/docs/guides/start", "fr/guides/start", order: 1),
            Entry("Advanced (FR)", "/docs/guides/advanced", "fr/guides/advanced", order: 2),
        };

        var builder = new SidebarBuilder([AutoItem("Guides", "guides")], entries);

        var result = builder.Build("/docs/fr/guides/start", "/fr", "/docs", "fr");

        result[0].Items.Count.ShouldBe(2);
        result[0].Items[0].Label.ShouldBe("Start (FR)");
        result[0].Items[1].Label.ShouldBe("Advanced (FR)");
    }

    [Fact]
    public void BuildWithLocaleKeyShouldNotIncludeEntriesFromOtherLocales()
    {
        var entries = new[]
        {
            Entry("Start (EN)", "/docs/guides/start", "guides/start", order: 1),
            Entry("Start (FR)", "/docs/guides/start", "fr/guides/start", order: 1),
            Entry("Start (ES)", "/docs/guides/start", "es/guides/start", order: 1),
        };

        var builder = new SidebarBuilder([AutoItem("Guides", "guides")], entries);

        var result = builder.Build("/docs/fr/guides/start", "/fr", "/docs", "fr");

        result[0].Items.Count.ShouldBe(1);
        result[0].Items[0].Label.ShouldBe("Start (FR)");
    }

    [Fact]
    public void BuildWithRootLocaleKeyShouldIncludeUnprefixedEntries()
    {
        var entries = new[]
        {
            Entry("Start", "/docs/guides/start", "guides/start", order: 2),
            Entry("Advanced", "/docs/guides/advanced", "guides/advanced", order: 1),
            Entry("Start (FR)", "/docs/guides/start", "fr/guides/start", order: 1),
        };

        var builder = new SidebarBuilder([AutoItem("Guides", "guides")], entries);

        var result = builder.Build("/docs/guides/start", "", "/docs", "root");

        result[0].Items.Count.ShouldBe(2);
        result[0].Items[0].Label.ShouldBe("Advanced");
        result[0].Items[1].Label.ShouldBe("Start");
    }

    [Fact]
    public void BuildWithLocaleKeyShouldPrefixAutoGeneratedHrefs()
    {
        var entries = new[]
        {
            Entry("Start", "/docs/guides/start", "fr/guides/start", order: 1),
        };

        var builder = new SidebarBuilder([AutoItem("Guides", "guides")], entries);

        var result = builder.Build("/docs/fr/guides/start", "/fr", "/docs", "fr");

        result[0].Items.Count.ShouldBe(1);
        result[0].Items[0].Href.ShouldBe("/docs/fr/guides/start");
        result[0].Items[0].IsCurrent.ShouldBeTrue();
    }

    [Fact]
    public void BuildWithLocaleKeyShouldMarkGroupActiveWhenChildIsCurrent()
    {
        var entries = new[]
        {
            Entry("Start", "/docs/guides/start", "fr/guides/start"),
            Entry("Advanced", "/docs/guides/advanced", "fr/guides/advanced"),
        };

        var builder = new SidebarBuilder([AutoItem("Guides", "guides")], entries);

        var result = builder.Build("/docs/fr/guides/start", "/fr", "/docs", "fr");

        result[0].IsActive.ShouldBeTrue();
    }

    [Fact]
    public void BuildWithEmptyLocaleKeyShouldNotFilterEntries()
    {
        var entries = new[]
        {
            Entry("Start (EN)", "/docs/guides/start", "guides/start", order: 1),
            Entry("Start (FR)", "/docs/fr/guides/start", "fr/guides/start", order: 1),
        };

        var builder = new SidebarBuilder([AutoItem("All", "")], entries);

        // Empty localeKey means no filtering.
        var result = builder.Build("/docs/guides/start", "", "/docs", "");

        result[0].Items.Count.ShouldBe(2);
    }

    [Fact]
    public void BuildWithLocaleKeyShouldHandleEmptyAutoGenerateDir()
    {
        var entries = new[]
        {
            Entry("Intro (FR)", "/docs/fr/intro", "fr/intro", order: 1),
            Entry("Intro (EN)", "/docs/intro", "intro", order: 1),
        };

        var builder = new SidebarBuilder([AutoItem("All", "")], entries);

        var result = builder.Build("/docs/fr/intro", "/fr", "/docs", "fr");

        // Empty dir with locale key should match all entries belonging to the locale.
        result[0].Items.Count.ShouldBe(1);
        result[0].Items[0].Label.ShouldBe("Intro (FR)");
    }

    // --- Draft filtering ---

    [Fact]
    public void BuildShouldExcludeDraftEntriesFromAutoGeneratedGroup()
    {
        var entries = new[]
        {
            Entry("Introduction", "/docs/intro", "guides/intro"),
            DraftEntry("Unreleased Feature", "/docs/secret", "guides/secret"),
        };
        var builder = new SidebarBuilder([AutoItem("Guides", "guides")], entries);

        var result = builder.Build("/docs/intro");

        result[0].Items.Count.ShouldBe(1);
        result[0].Items[0].Label.ShouldBe("Introduction");
    }

    [Fact]
    public void BuildShouldIncludeNonDraftEntriesAlongDraftOnes()
    {
        var entries = new[]
        {
            Entry("Page A", "/docs/a", "guides/a"),
            DraftEntry("Page B (draft)", "/docs/b", "guides/b"),
            Entry("Page C", "/docs/c", "guides/c"),
        };
        var builder = new SidebarBuilder([AutoItem("Guides", "guides")], entries);

        var result = builder.Build("/docs/a");

        result[0].Items.Count.ShouldBe(2);
        result[0].Items.Select(i => i.Label).ShouldBe(["Page A", "Page C"]);
    }

    [Fact]
    public void BuildShouldExcludeAllEntriesWhenAllAreDraft()
    {
        var entries = new[]
        {
            DraftEntry("Draft 1", "/docs/d1", "guides/d1"),
            DraftEntry("Draft 2", "/docs/d2", "guides/d2"),
        };
        var builder = new SidebarBuilder([AutoItem("Guides", "guides")], entries);

        var result = builder.Build("/docs/d1");

        result[0].Items.ShouldBeEmpty();
    }

    [Fact]
    public void BuildShouldExcludeDraftEntriesWithLocaleAwareBuild()
    {
        var entries = new[]
        {
            Entry("Intro (FR)", "/docs/fr/intro", "fr/intro", order: 1),
            DraftEntry("Secret (FR)", "/docs/fr/secret", "fr/secret", order: 2),
        };
        var builder = new SidebarBuilder([AutoItem("All", "")], entries);

        var result = builder.Build("/docs/fr/intro", "/fr", "/docs", "fr");

        result[0].Items.Count.ShouldBe(1);
        result[0].Items[0].Label.ShouldBe("Intro (FR)");
    }

    // --- Version-aware sidebar ---

    [Fact]
    public void BuildWithVersionShouldPrefixManualLinkHrefs()
    {
        var builder = new SidebarBuilder(
            [LinkItem("Intro", "/intro")],
            []);

        var result = builder.Build("/v1.0/intro", "", "", "", "/v1.0", "v1.0");

        result[0].Href.ShouldBe("/v1.0/intro");
    }

    [Fact]
    public void BuildWithVersionShouldMarkCurrentPageWithVersionPrefix()
    {
        var builder = new SidebarBuilder(
            [LinkItem("Intro", "/intro"), LinkItem("Guide", "/guide")],
            []);

        var result = builder.Build("/v1.0/intro", "", "", "", "/v1.0", "v1.0");

        result[0].IsCurrent.ShouldBeTrue();
        result[1].IsCurrent.ShouldBeFalse();
    }

    [Fact]
    public void BuildWithVersionShouldFilterAutoGeneratedEntriesToVersion()
    {
        var entries = new[]
        {
            Entry("Intro v1", "/v1.0/intro", "v1.0/intro", order: 1),
            Entry("Intro current", "/intro", "intro", order: 1),
        };
        var builder = new SidebarBuilder([AutoItem("All", "")], entries);

        var result = builder.Build("/v1.0/intro", "", "", "", "/v1.0", "v1.0");

        result[0].Items.Count.ShouldBe(1);
        result[0].Items[0].Label.ShouldBe("Intro v1");
    }

    [Fact]
    public void BuildWithVersionAndLocaleShouldPrefixCorrectly()
    {
        var builder = new SidebarBuilder(
            [LinkItem("Intro", "/intro")],
            []);

        var result = builder.Build("/docs/fr/v1.0/intro", "/fr", "/docs", "fr", "/v1.0", "v1.0");

        result[0].Href.ShouldBe("/docs/fr/v1.0/intro");
    }

    [Fact]
    public void BuildWithVersionShouldNotIncludeEntriesFromOtherVersions()
    {
        var entries = new[]
        {
            Entry("Intro v1", "/v1.0/intro", "v1.0/intro", order: 1),
            Entry("Intro v2", "/v2.0/intro", "v2.0/intro", order: 1),
        };
        var builder = new SidebarBuilder([AutoItem("All", "")], entries);

        var result = builder.Build("/v1.0/intro", "", "", "", "/v1.0", "v1.0");

        result[0].Items.Count.ShouldBe(1);
        result[0].Items[0].Label.ShouldBe("Intro v1");
    }

    [Fact]
    public void BuildWithCurrentVersionKeyShouldIncludeUnprefixedEntries()
    {
        var entries = new[]
        {
            Entry("Intro", "/intro", "intro", order: 1),
            Entry("Guide", "/guide", "guide", order: 2),
        };
        var builder = new SidebarBuilder([AutoItem("All", "")], entries);

        var result = builder.Build("/intro", "", "", "", "", "current");

        result[0].Items.Count.ShouldBe(2);
    }

    [Fact]
    public void BuildWithEmptyVersionKeyShouldNotFilterEntries()
    {
        var entries = new[]
        {
            Entry("Intro", "/intro", "intro", order: 1),
            Entry("Guide", "/guide", "guide", order: 2),
        };
        var builder = new SidebarBuilder([AutoItem("All", "")], entries);

        var result = builder.Build("/intro", "", "", "", "", "");

        result[0].Items.Count.ShouldBe(2);
    }
}
