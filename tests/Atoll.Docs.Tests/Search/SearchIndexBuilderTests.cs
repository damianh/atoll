using Atoll.Build.Content.Collections;
using Atoll.Docs.Search;
using Shouldly;
using Xunit;

namespace Atoll.Docs.Tests.Search;

public sealed class SearchIndexBuilderTests
{
    private sealed class DocSchema
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    private static ContentEntry<DocSchema> MakeEntry(string slug, string title, string body, string? description = null)
    {
        var data = new DocSchema { Title = title, Description = description };
        return new ContentEntry<DocSchema>(slug + ".md", "docs", slug, body, data);
    }

    // --- Empty collection ---

    [Fact]
    public void ShouldBuildEmptyIndexFromNoDocuments()
    {
        var index = new SearchIndexBuilder().Build();

        index.Entries.ShouldBeEmpty();
    }

    [Fact]
    public void ShouldBuildEmptyIndexFromEmptyCollection()
    {
        var entries = Array.Empty<ContentEntry<DocSchema>>();
        var index = new SearchIndexBuilder()
            .AddCollection(entries, e => new SearchDocumentInput(e.Data.Title, $"/docs/{e.Slug}/"))
            .Build();

        index.Entries.ShouldBeEmpty();
    }

    // --- Basic indexing ---

    [Fact]
    public void ShouldIndexSingleDocument()
    {
        var input = new SearchDocumentInput("Getting Started", "/docs/getting-started/");
        var index = new SearchIndexBuilder().Add(input).Build();

        index.Entries.Count.ShouldBe(1);
        index.Entries[0].Title.ShouldBe("Getting Started");
        index.Entries[0].Href.ShouldBe("/docs/getting-started/");
    }

    [Fact]
    public void ShouldIndexMultipleDocuments()
    {
        var index = new SearchIndexBuilder()
            .Add(new SearchDocumentInput("Intro", "/docs/intro/"))
            .Add(new SearchDocumentInput("Advanced", "/docs/advanced/"))
            .Build();

        index.Entries.Count.ShouldBe(2);
    }

    // --- HTML stripping ---

    [Fact]
    public void ShouldStripHtmlFromBody()
    {
        var input = new SearchDocumentInput("Page", "/docs/page/")
        {
            HtmlBody = "<p>Hello <strong>world</strong></p>"
        };
        var index = new SearchIndexBuilder().Add(input).Build();

        index.Entries[0].Body.ShouldNotContain("<p>");
        index.Entries[0].Body.ShouldContain("Hello");
        index.Entries[0].Body.ShouldContain("world");
    }

    [Fact]
    public void ShouldTruncateBodyToMaxLength()
    {
        var longBody = new string('x', 1000);
        var input = new SearchDocumentInput("Page", "/docs/page/")
        {
            HtmlBody = longBody,
            MaxBodyLength = 100
        };
        var index = new SearchIndexBuilder().Add(input).Build();

        index.Entries[0].Body.Length.ShouldBeLessThanOrEqualTo(100);
    }

    [Fact]
    public void ShouldUseDefaultMaxBodyLengthOf500()
    {
        var longBody = new string('x', 600);
        var input = new SearchDocumentInput("Page", "/docs/page/")
        {
            HtmlBody = longBody
        };
        var index = new SearchIndexBuilder().Add(input).Build();

        index.Entries[0].Body.Length.ShouldBeLessThanOrEqualTo(500);
    }

    [Fact]
    public void ShouldUsePlainBodyOverHtmlBody()
    {
        var input = new SearchDocumentInput("Page", "/docs/page/")
        {
            HtmlBody = "<p>HTML content</p>",
            PlainBody = "Plain content"
        };
        var index = new SearchIndexBuilder().Add(input).Build();

        index.Entries[0].Body.ShouldBe("Plain content");
        index.Entries[0].Body.ShouldNotContain("HTML content");
    }

    // --- Metadata ---

    [Fact]
    public void ShouldPreserveDescriptionAndSection()
    {
        var input = new SearchDocumentInput("Guide", "/docs/guide/")
        {
            Description = "A helpful guide",
            Section = "Getting Started"
        };
        var index = new SearchIndexBuilder().Add(input).Build();

        index.Entries[0].Description.ShouldBe("A helpful guide");
        index.Entries[0].Section.ShouldBe("Getting Started");
    }

    [Fact]
    public void ShouldUseProvidedHeadings()
    {
        var input = new SearchDocumentInput("Page", "/docs/page/")
        {
            Headings = ["Introduction", "Setup", "Usage"]
        };
        var index = new SearchIndexBuilder().Add(input).Build();

        index.Entries[0].Headings.ShouldBe(["Introduction", "Setup", "Usage"]);
    }

    [Fact]
    public void ShouldExtractHeadingsFromHtmlBody()
    {
        var input = new SearchDocumentInput("Page", "/docs/page/")
        {
            HtmlBody = "<h2>Introduction</h2><p>Text</p><h3>Sub-topic</h3>"
        };
        var index = new SearchIndexBuilder().Add(input).Build();

        index.Entries[0].Headings.ShouldContain("Introduction");
        index.Entries[0].Headings.ShouldContain("Sub-topic");
    }

    // --- Timestamp ---

    [Fact]
    public void ShouldEmbedGenerationTimestamp()
    {
        var timestamp = new DateTimeOffset(2025, 1, 15, 12, 0, 0, TimeSpan.Zero);
        var index = new SearchIndexBuilder().Build(timestamp);

        index.GeneratedAt.ShouldBe(timestamp);
    }

    // --- Collection helper ---

    [Fact]
    public void ShouldAddAllEntriesFromCollection()
    {
        var entries = new[]
        {
            MakeEntry("intro", "Introduction", "Content here"),
            MakeEntry("setup", "Setup Guide", "Install and configure")
        };
        var index = new SearchIndexBuilder()
            .AddCollection(entries, e => new SearchDocumentInput(e.Data.Title, $"/docs/{e.Slug}/"))
            .Build();

        index.Entries.Count.ShouldBe(2);
        index.Entries.Select(e => e.Title).ShouldBe(["Introduction", "Setup Guide"]);
    }

    // --- StripHtml static helper ---

    [Fact]
    public void StripHtmlShouldHandleEmptyString()
    {
        var result = SearchIndexBuilder.StripHtml(string.Empty);

        result.ShouldBeEmpty();
    }

    [Fact]
    public void StripHtmlShouldRemoveAllTags()
    {
        var result = SearchIndexBuilder.StripHtml("<h1>Title</h1><p>Body <em>text</em>.</p>");

        result.ShouldNotContain("<");
        result.ShouldContain("Title");
        result.ShouldContain("Body");
        result.ShouldContain("text");
    }
}
