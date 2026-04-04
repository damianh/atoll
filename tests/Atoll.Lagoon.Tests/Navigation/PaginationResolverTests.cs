using Atoll.Lagoon.Navigation;
using Shouldly;
using Xunit;

namespace Atoll.Lagoon.Tests.Navigation;

public sealed class PaginationResolverTests
{
    private static ResolvedSidebarItem Link(string label, string href)
        => new ResolvedSidebarItem(label, href, false, null);

    private static ResolvedSidebarItem Group(string label, IReadOnlyList<ResolvedSidebarItem> items)
        => new ResolvedSidebarItem(label, false, null, false, items);

    // --- Basic cases ---

    [Fact]
    public void ResolveShouldReturnNullsForEmptyList()
    {
        var resolver = new PaginationResolver([]);

        var result = resolver.Resolve("/docs/page");

        result.Previous.ShouldBeNull();
        result.Next.ShouldBeNull();
    }

    [Fact]
    public void ResolveShouldReturnNullsWhenCurrentPageNotFound()
    {
        var resolver = new PaginationResolver([Link("A", "/a"), Link("B", "/b")]);

        var result = resolver.Resolve("/not-found");

        result.Previous.ShouldBeNull();
        result.Next.ShouldBeNull();
    }

    [Fact]
    public void ResolveShouldHaveNoPreviousOnFirstPage()
    {
        var resolver = new PaginationResolver([
            Link("First", "/first"),
            Link("Second", "/second"),
            Link("Third", "/third")
        ]);

        var result = resolver.Resolve("/first");

        result.Previous.ShouldBeNull();
        result.Next.ShouldNotBeNull();
        result.Next!.Label.ShouldBe("Second");
        result.Next.Href.ShouldBe("/second");
    }

    [Fact]
    public void ResolveShouldHaveNoNextOnLastPage()
    {
        var resolver = new PaginationResolver([
            Link("First", "/first"),
            Link("Second", "/second"),
            Link("Third", "/third")
        ]);

        var result = resolver.Resolve("/third");

        result.Next.ShouldBeNull();
        result.Previous.ShouldNotBeNull();
        result.Previous!.Label.ShouldBe("Second");
        result.Previous.Href.ShouldBe("/second");
    }

    [Fact]
    public void ResolveShouldHaveBothPreviousAndNextForMiddlePage()
    {
        var resolver = new PaginationResolver([
            Link("First", "/first"),
            Link("Second", "/second"),
            Link("Third", "/third")
        ]);

        var result = resolver.Resolve("/second");

        result.Previous.ShouldNotBeNull();
        result.Previous!.Label.ShouldBe("First");
        result.Next.ShouldNotBeNull();
        result.Next!.Label.ShouldBe("Third");
    }

    // --- Single-item list ---

    [Fact]
    public void ResolveShouldReturnNullsForSingleItemList()
    {
        var resolver = new PaginationResolver([Link("Only", "/only")]);

        var result = resolver.Resolve("/only");

        result.Previous.ShouldBeNull();
        result.Next.ShouldBeNull();
    }

    // --- Trailing slash normalization ---

    [Fact]
    public void ResolveShouldMatchHrefIgnoringTrailingSlash()
    {
        var resolver = new PaginationResolver([
            Link("A", "/a/"),
            Link("B", "/b"),
            Link("C", "/c/")
        ]);

        var result = resolver.Resolve("/b/");

        result.Previous!.Label.ShouldBe("A");
        result.Next!.Label.ShouldBe("C");
    }

    // --- Case-insensitive matching ---

    [Fact]
    public void ResolveShouldMatchHrefCaseInsensitively()
    {
        var resolver = new PaginationResolver([
            Link("A", "/Docs/A"),
            Link("B", "/Docs/B"),
            Link("C", "/Docs/C")
        ]);

        var result = resolver.Resolve("/docs/b");

        result.Previous!.Label.ShouldBe("A");
        result.Next!.Label.ShouldBe("C");
    }

    // --- Group headers are skipped ---

    [Fact]
    public void ResolveShouldSkipGroupHeadersInFlattenedList()
    {
        var items = new ResolvedSidebarItem[]
        {
            Link("Home", "/"),
            Group("Guides", [
                Link("Start", "/guides/start"),
                Link("Advanced", "/guides/advanced")
            ]),
            Link("API", "/api")
        };

        var resolver = new PaginationResolver(items, flatten: true);

        var result = resolver.Resolve("/guides/start");

        result.Previous!.Label.ShouldBe("Home");
        result.Next!.Label.ShouldBe("Advanced");
    }

    [Fact]
    public void ResolveShouldSkipNestedGroupHeaders()
    {
        var items = new ResolvedSidebarItem[]
        {
            Group("Outer", [
                Group("Inner", [
                    Link("Deep", "/deep")
                ])
            ]),
            Link("After", "/after")
        };

        var resolver = new PaginationResolver(items, flatten: true);
        var flat = SidebarBuilder.Flatten(items);

        flat.Count.ShouldBe(2);

        var result = resolver.Resolve("/deep");

        result.Previous.ShouldBeNull();
        result.Next!.Label.ShouldBe("After");
    }

    // --- Both constructors produce consistent results ---

    [Fact]
    public void FlatItemsAndFlattenOverloadShouldProduceSameResult()
    {
        var items = new ResolvedSidebarItem[]
        {
            Link("A", "/a"),
            Group("G", [Link("B", "/b"), Link("C", "/c")]),
            Link("D", "/d")
        };

        var flat = SidebarBuilder.Flatten(items);
        var resolverFlat = new PaginationResolver(flat);
        var resolverTree = new PaginationResolver(items, flatten: true);

        var resultFlat = resolverFlat.Resolve("/b");
        var resultTree = resolverTree.Resolve("/b");

        resultFlat.Previous!.Href.ShouldBe(resultTree.Previous!.Href);
        resultFlat.Next!.Href.ShouldBe(resultTree.Next!.Href);
    }
}
