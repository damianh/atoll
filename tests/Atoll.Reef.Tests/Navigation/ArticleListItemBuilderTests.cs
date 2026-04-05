using Atoll.Build.Content.Collections;
using Atoll.Reef.Configuration;
using Atoll.Reef.Navigation;
using Shouldly;
using Xunit;

namespace Atoll.Reef.Tests.Navigation;

public sealed class ArticleListItemBuilderTests
{
    private static ContentEntry<ArticleSchema> MakeEntry(
        string slug,
        string title = "Test Article",
        string author = "",
        string tags = "",
        int? readingTimeMinutes = null,
        string body = "",
        DateTime? pubDate = null) =>
        new(
            id: slug + ".md",
            collection: "articles",
            slug: slug,
            body: body,
            data: new ArticleSchema
            {
                Title = title,
                Description = "A description",
                PubDate = pubDate ?? new DateTime(2025, 1, 1),
                Author = author,
                Tags = tags,
                ReadingTimeMinutes = readingTimeMinutes,
            });

    [Fact]
    public void ShouldReturnOneItemPerEntry()
    {
        var entries = new[]
        {
            MakeEntry("post-1"),
            MakeEntry("post-2"),
        };

        var items = ArticleListItemBuilder.Build(entries);

        items.Count.ShouldBe(2);
    }

    [Fact]
    public void ShouldMapTitleAndSlug()
    {
        var entries = new[]
        {
            MakeEntry("my-post", title: "My Great Post"),
        };

        var items = ArticleListItemBuilder.Build(entries);

        items[0].Title.ShouldBe("My Great Post");
        items[0].Slug.ShouldBe("my-post");
    }

    [Fact]
    public void ShouldMapPubDate()
    {
        var date = new DateTime(2025, 6, 15);
        var entries = new[]
        {
            MakeEntry("post", pubDate: date),
        };

        var items = ArticleListItemBuilder.Build(entries);

        items[0].PubDate.ShouldBe(date);
    }

    [Fact]
    public void ShouldMapAuthorWhenPresent()
    {
        var entries = new[]
        {
            MakeEntry("post", author: "Alice"),
        };

        var items = ArticleListItemBuilder.Build(entries);

        items[0].Author.ShouldBe("Alice");
    }

    [Fact]
    public void ShouldMapAuthorAsNullWhenEmpty()
    {
        var entries = new[]
        {
            MakeEntry("post", author: ""),
        };

        var items = ArticleListItemBuilder.Build(entries);

        items[0].Author.ShouldBeNull();
    }

    [Fact]
    public void ShouldMapTagsFromFrontmatter()
    {
        var entries = new[]
        {
            MakeEntry("post", tags: "dotnet, csharp"),
        };

        var items = ArticleListItemBuilder.Build(entries);

        items[0].Tags.ShouldContain("dotnet");
        items[0].Tags.ShouldContain("csharp");
    }

    [Fact]
    public void ShouldUseReadingTimeFromFrontmatterWhenSet()
    {
        var entries = new[]
        {
            MakeEntry("post", readingTimeMinutes: 7, body: "short"),
        };

        var items = ArticleListItemBuilder.Build(entries);

        items[0].ReadingTimeMinutes.ShouldBe(7);
    }

    [Fact]
    public void ShouldCalculateReadingTimeWhenNotSetInFrontmatter()
    {
        // ~200 words = ~1 minute at 200 wpm
        var words = string.Join(" ", Enumerable.Repeat("word", 200));
        var entries = new[]
        {
            MakeEntry("post", readingTimeMinutes: null, body: words),
        };

        var items = ArticleListItemBuilder.Build(entries);

        items[0].ReadingTimeMinutes!.Value.ShouldBeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public void ShouldReturnEmptyListForEmptyInput()
    {
        var items = ArticleListItemBuilder.Build([]);

        items.ShouldBeEmpty();
    }

    [Fact]
    public void ShouldPreserveInputOrder()
    {
        var entries = new[]
        {
            MakeEntry("alpha"),
            MakeEntry("beta"),
            MakeEntry("gamma"),
        };

        var items = ArticleListItemBuilder.Build(entries);

        items[0].Slug.ShouldBe("alpha");
        items[1].Slug.ShouldBe("beta");
        items[2].Slug.ShouldBe("gamma");
    }
}
