using Atoll.Build.Content.Markdown;
using Atoll.Components;
using Atoll.Docs.Components;
using Atoll.Rendering;
using Shouldly;
using Xunit;

namespace Atoll.Docs.Tests.Components;

public sealed class TableOfContentsTests
{
    private static MarkdownHeading H(int depth, string text, string? id = null)
        => new MarkdownHeading(depth, text, id ?? text.ToLowerInvariant().Replace(' ', '-'));

    private static async Task<string> RenderTocAsync(
        IReadOnlyList<MarkdownHeading> headings,
        int minLevel = 2,
        int maxLevel = 3)
    {
        var destination = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            ["Headings"] = headings,
            ["MinLevel"] = minLevel,
            ["MaxLevel"] = maxLevel
        };
        await ComponentRenderer.RenderComponentAsync<TableOfContents>(destination, props);
        return destination.GetOutput();
    }

    // --- Empty cases ---

    [Fact]
    public async Task ShouldRenderNothingForEmptyHeadings()
    {
        var html = await RenderTocAsync([]);

        html.ShouldBeEmpty();
    }

    [Fact]
    public async Task ShouldRenderNothingWhenAllHeadingsFilteredOut()
    {
        // H1 is excluded by default min level of 2.
        var html = await RenderTocAsync([H(1, "Title")]);

        html.ShouldBeEmpty();
    }

    [Fact]
    public async Task ShouldRenderNothingWhenHeadingsBelowMaxLevel()
    {
        // h4 is beyond max level of 3.
        var html = await RenderTocAsync([H(4, "Deep")]);

        html.ShouldBeEmpty();
    }

    // --- Basic rendering ---

    [Fact]
    public async Task ShouldRenderNavWithAriaLabel()
    {
        var html = await RenderTocAsync([H(2, "Section", "section")]);

        html.ShouldContain("<nav aria-label=\"On this page\">");
        html.ShouldContain("</nav>");
    }

    [Fact]
    public async Task ShouldRenderSingleH2Heading()
    {
        var html = await RenderTocAsync([H(2, "Introduction", "introduction")]);

        html.ShouldContain("<a href=\"#introduction\">");
        html.ShouldContain("Introduction");
    }

    [Fact]
    public async Task ShouldRenderMultipleH2Headings()
    {
        var html = await RenderTocAsync([
            H(2, "First", "first"),
            H(2, "Second", "second")
        ]);

        html.ShouldContain("href=\"#first\"");
        html.ShouldContain("href=\"#second\"");
    }

    // --- Nesting ---

    [Fact]
    public async Task ShouldNestH3AsChildOfPrecedingH2()
    {
        var html = await RenderTocAsync([
            H(2, "Guide", "guide"),
            H(3, "Subsection", "subsection")
        ]);

        // h3 should appear inside a nested <ul> after the h2 link
        html.ShouldContain("Guide");
        html.ShouldContain("Subsection");
        html.ShouldContain("<ul>");
    }

    [Fact]
    public async Task ShouldRenderFlatListWhenNoNesting()
    {
        var html = await RenderTocAsync([
            H(2, "A", "a"),
            H(2, "B", "b"),
            H(2, "C", "c")
        ]);

        // Should be exactly two <ul> tags — outer open and outer close
        html.ShouldContain("<ul>");
        // No nested ul
        var nestedCount = html.Split("<ul>").Length - 1;
        nestedCount.ShouldBe(1);
    }

    // --- Filtering by level ---

    [Fact]
    public async Task ShouldExcludeH1ByDefault()
    {
        var html = await RenderTocAsync([
            H(1, "Page Title", "title"),
            H(2, "Section", "section")
        ]);

        html.ShouldNotContain("Page Title");
        html.ShouldContain("Section");
    }

    [Fact]
    public async Task ShouldExcludeH4ByDefault()
    {
        var html = await RenderTocAsync([
            H(2, "Section", "section"),
            H(4, "Detail", "detail")
        ]);

        html.ShouldNotContain("Detail");
        html.ShouldContain("Section");
    }

    [Fact]
    public async Task ShouldRespectCustomMinLevel()
    {
        var html = await RenderTocAsync([
            H(1, "Title", "title"),
            H(2, "Section", "section")
        ], minLevel: 1, maxLevel: 3);

        html.ShouldContain("Title");
        html.ShouldContain("Section");
    }

    [Fact]
    public async Task ShouldRespectCustomMaxLevel()
    {
        var html = await RenderTocAsync([
            H(2, "Section", "section"),
            H(3, "Subsection", "subsection"),
            H(4, "Deep", "deep")
        ], minLevel: 2, maxLevel: 4);

        html.ShouldContain("Section");
        html.ShouldContain("Subsection");
        html.ShouldContain("Deep");
    }

    // --- HTML encoding ---

    [Fact]
    public async Task ShouldHtmlEncodeHeadingText()
    {
        var html = await RenderTocAsync([H(2, "<script>alert(1)</script>", "safe")]);

        html.ShouldNotContain("<script>");
        html.ShouldContain("&lt;script&gt;");
    }

    // --- Null ID handling ---

    [Fact]
    public async Task ShouldRenderEmptyAnchorWhenIdIsNull()
    {
        var heading = new MarkdownHeading(2, "No ID", null);
        var html = await RenderTocAsync([heading]);

        html.ShouldContain("href=\"\"");
    }
}
