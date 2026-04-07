using Atoll.Components;
using Atoll.Reef.Components;
using Atoll.Rendering;

namespace Atoll.Reef.Tests.Components;

public sealed class ArticleSeriesTests
{
    private static SeriesPart Part(string title, string href) => new(title, href);

    private static async Task<string> RenderAsync(
        string seriesName = "My Series",
        IReadOnlyList<SeriesPart>? parts = null,
        int currentPart = 1)
    {
        parts ??= [Part("Part One", "/blog/part-1"), Part("Part Two", "/blog/part-2")];
        var dest = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            [nameof(ArticleSeries.SeriesName)] = seriesName,
            [nameof(ArticleSeries.Parts)] = parts,
            [nameof(ArticleSeries.CurrentPart)] = currentPart,
        };
        await ComponentRenderer.RenderComponentAsync<ArticleSeries>(dest, props);
        return dest.GetOutput();
    }

    [Fact]
    public async Task ShouldRenderSeriesHeader()
    {
        var html = await RenderAsync(seriesName: "Deep Dives", currentPart: 2);

        html.ShouldContain("Part 2 of 2");
        html.ShouldContain("Deep Dives");
    }

    [Fact]
    public async Task ShouldRenderAllPartsAsList()
    {
        var parts = new[]
        {
            Part("Introduction", "/s/intro"),
            Part("Advanced Topics", "/s/advanced"),
            Part("Conclusion", "/s/conclusion"),
        };

        var html = await RenderAsync(parts: parts, currentPart: 1);

        html.ShouldContain("class=\"series-parts\"");
        html.ShouldContain("Introduction");
        html.ShouldContain("Advanced Topics");
        html.ShouldContain("Conclusion");
    }

    [Fact]
    public async Task ShouldMarkCurrentPartWithAriaCurrent()
    {
        var parts = new[]
        {
            Part("Part One", "/blog/part-1"),
            Part("Part Two", "/blog/part-2"),
        };

        var html = await RenderAsync(parts: parts, currentPart: 2);

        html.ShouldContain("aria-current=\"page\"");
        html.ShouldContain("series-part--current");
    }

    [Fact]
    public async Task ShouldNotMarkNonCurrentPartsWithAriaCurrent()
    {
        var parts = new[]
        {
            Part("Part One", "/blog/part-1"),
            Part("Part Two", "/blog/part-2"),
        };

        var html = await RenderAsync(parts: parts, currentPart: 1);

        var currentCount = CountOccurrences(html, "aria-current");
        currentCount.ShouldBe(1);
    }

    [Fact]
    public async Task ShouldRenderPartsWithLinks()
    {
        var html = await RenderAsync();

        html.ShouldContain("href=\"/blog/part-1\"");
        html.ShouldContain("href=\"/blog/part-2\"");
    }

    [Fact]
    public async Task ShouldHtmlEncodeSeriesName()
    {
        var html = await RenderAsync(seriesName: "<script>alert(1)</script>");

        html.ShouldContain("&lt;script&gt;alert(1)&lt;/script&gt;");
        html.ShouldNotContain("<script>");
    }

    [Fact]
    public async Task ShouldHtmlEncodePartTitles()
    {
        var parts = new[] { Part("<b>Bold</b>", "/part") };

        var html = await RenderAsync(parts: parts, currentPart: 1);

        html.ShouldContain("&lt;b&gt;Bold&lt;/b&gt;");
        html.ShouldNotContain("<b>Bold</b>");
    }

    private static int CountOccurrences(string text, string pattern)
    {
        var count = 0;
        var index = 0;
        while ((index = text.IndexOf(pattern, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += pattern.Length;
        }
        return count;
    }
}
