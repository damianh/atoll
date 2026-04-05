using Atoll.Build.Content.Collections;
using Atoll.Reef.Configuration;
using Atoll.Reef.Navigation;
using Shouldly;
using Xunit;

namespace Atoll.Reef.Tests.Navigation;

public sealed class SeriesResolverTests
{
    private static ContentEntry<ArticleSchema> MakeEntry(
        string slug,
        string seriesName,
        int? seriesOrder = null,
        DateTime? pubDate = null) =>
        new(
            id: slug + ".md",
            collection: "articles",
            slug: slug,
            body: "body",
            data: new ArticleSchema
            {
                Title = "Title for " + slug,
                Description = "Description",
                PubDate = pubDate ?? new DateTime(2025, 1, 1),
                Series = seriesName,
                SeriesOrder = seriesOrder,
            });

    [Fact]
    public void ShouldReturnPartsOrderedBySeriesOrder()
    {
        var entries = new[]
        {
            MakeEntry("part-3", "My Series", seriesOrder: 3),
            MakeEntry("part-1", "My Series", seriesOrder: 1),
            MakeEntry("part-2", "My Series", seriesOrder: 2),
        };

        var (parts, _) = SeriesResolver.Resolve("My Series", "part-1", entries, "/blog");

        parts[0].Title.ShouldBe("Title for part-1");
        parts[1].Title.ShouldBe("Title for part-2");
        parts[2].Title.ShouldBe("Title for part-3");
    }

    [Fact]
    public void ShouldReturnCurrentPartIndex()
    {
        var entries = new[]
        {
            MakeEntry("part-1", "My Series", seriesOrder: 1),
            MakeEntry("part-2", "My Series", seriesOrder: 2),
            MakeEntry("part-3", "My Series", seriesOrder: 3),
        };

        var (_, currentPart) = SeriesResolver.Resolve("My Series", "part-2", entries, "/blog");

        currentPart.ShouldBe(2);
    }

    [Fact]
    public void ShouldReturnZeroCurrentPartWhenSlugNotFound()
    {
        var entries = new[]
        {
            MakeEntry("part-1", "My Series", seriesOrder: 1),
        };

        var (_, currentPart) = SeriesResolver.Resolve("My Series", "not-in-series", entries, "/blog");

        currentPart.ShouldBe(0);
    }

    [Fact]
    public void ShouldFilterBySeriesNameCaseInsensitively()
    {
        var entries = new[]
        {
            MakeEntry("part-1", "my series", seriesOrder: 1),
            MakeEntry("other", "other series", seriesOrder: 1),
        };

        var (parts, _) = SeriesResolver.Resolve("MY SERIES", "part-1", entries, "/blog");

        parts.Count.ShouldBe(1);
        parts[0].Title.ShouldBe("Title for part-1");
    }

    [Fact]
    public void ShouldBuildCorrectPartHrefs()
    {
        var entries = new[]
        {
            MakeEntry("intro", "Series A", seriesOrder: 1),
        };

        var (parts, _) = SeriesResolver.Resolve("Series A", "intro", entries, "/blog");

        parts[0].Href.ShouldBe("/blog/intro");
    }

    [Fact]
    public void ShouldTrimTrailingSlashOnBasePath()
    {
        var entries = new[]
        {
            MakeEntry("intro", "Series A", seriesOrder: 1),
        };

        var (parts, _) = SeriesResolver.Resolve("Series A", "intro", entries, "/blog/");

        parts[0].Href.ShouldBe("/blog/intro");
    }

    [Fact]
    public void ShouldFallbackToPublicationDateWhenSeriesOrderNull()
    {
        var entries = new[]
        {
            MakeEntry("second", "Series B", pubDate: new DateTime(2025, 6, 1)),
            MakeEntry("first", "Series B", pubDate: new DateTime(2025, 1, 1)),
        };

        var (parts, _) = SeriesResolver.Resolve("Series B", "first", entries, "/blog");

        parts[0].Title.ShouldBe("Title for first");
        parts[1].Title.ShouldBe("Title for second");
    }

    [Fact]
    public void ShouldReturnEmptyWhenNoEntriesMatchSeries()
    {
        var entries = new[]
        {
            MakeEntry("post", "Different Series", seriesOrder: 1),
        };

        var (parts, currentPart) = SeriesResolver.Resolve("My Series", "post", entries, "/blog");

        parts.ShouldBeEmpty();
        currentPart.ShouldBe(0);
    }
}
