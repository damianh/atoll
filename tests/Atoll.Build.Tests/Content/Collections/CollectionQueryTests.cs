using System.ComponentModel.DataAnnotations;
using Atoll.Content.Collections;
using Atoll.Content.Frontmatter;
using Shouldly;
using Xunit;

namespace Atoll.Build.Tests.Content.Collections;

public sealed class CollectionQueryTests
{
    private sealed class BlogPost
    {
        [Required]
        public string Title { get; set; } = "";

        public string Description { get; set; } = "";
        public DateTime PubDate { get; set; }
        public List<string> Tags { get; set; } = [];
        public bool Draft { get; set; }
    }

    private sealed class Author
    {
        [Required]
        public string Name { get; set; } = "";

        public string Bio { get; set; } = "";
    }

    private static readonly string BlogDir = Path.Combine("content", "blog");
    private static readonly string AuthorsDir = Path.Combine("content", "authors");

    private static InMemoryFileProvider CreateBlogProvider()
    {
        return new InMemoryFileProvider()
            .AddFile(BlogDir, "first-post.md",
                "---\ntitle: First Post\ndescription: My first post\npubDate: 2026-01-01\ntags:\n  - intro\n---\n# First Post\n\nHello world!")
            .AddFile(BlogDir, "second-post.md",
                "---\ntitle: Second Post\ndescription: A follow-up\npubDate: 2026-02-15\ntags:\n  - csharp\n  - dotnet\ndraft: true\n---\n# Second Post\n\nMore content here.")
            .AddFile(BlogDir, "third-post.md",
                "---\ntitle: Third Post\ndescription: Final post\npubDate: 2026-03-20\ntags:\n  - web\n---\n# Third Post\n\nThe last one.");
    }

    private static CollectionConfig CreateBlogConfig()
    {
        return new CollectionConfig("content")
            .AddCollection(ContentCollection.Define<BlogPost>("blog"));
    }

    private static CollectionQuery CreateBlogQuery()
    {
        var config = CreateBlogConfig();
        var loader = new CollectionLoader(config, CreateBlogProvider());
        return new CollectionQuery(loader);
    }

    // --- GetCollection tests ---

    [Fact]
    public void GetCollectionShouldReturnAllEntries()
    {
        var query = CreateBlogQuery();
        var entries = query.GetCollection<BlogPost>("blog");

        entries.Count.ShouldBe(3);
    }

    [Fact]
    public void GetCollectionShouldReturnEntriesSortedByFileName()
    {
        var query = CreateBlogQuery();
        var entries = query.GetCollection<BlogPost>("blog");

        entries[0].Slug.ShouldBe("first-post");
        entries[1].Slug.ShouldBe("second-post");
        entries[2].Slug.ShouldBe("third-post");
    }

    [Fact]
    public void GetCollectionShouldParseFrontmatterData()
    {
        var query = CreateBlogQuery();
        var entries = query.GetCollection<BlogPost>("blog");

        var first = entries[0];
        first.Data.Title.ShouldBe("First Post");
        first.Data.Description.ShouldBe("My first post");
        first.Data.PubDate.ShouldBe(new DateTime(2026, 1, 1));
        first.Data.Tags.ShouldBe(["intro"]);
        first.Data.Draft.ShouldBeFalse();
    }

    [Fact]
    public void GetCollectionShouldParseBody()
    {
        var query = CreateBlogQuery();
        var entries = query.GetCollection<BlogPost>("blog");

        entries[0].Body.ShouldContain("# First Post");
        entries[0].Body.ShouldContain("Hello world!");
    }

    [Fact]
    public void GetCollectionShouldSetIdWithCollectionPrefix()
    {
        var query = CreateBlogQuery();
        var entries = query.GetCollection<BlogPost>("blog");

        entries[0].Id.ShouldBe("blog/first-post.md");
        entries[0].Collection.ShouldBe("blog");
    }

    [Fact]
    public void GetCollectionWithPredicateShouldFilter()
    {
        var query = CreateBlogQuery();
        var published = query.GetCollection<BlogPost>("blog", e => !e.Data.Draft);

        published.Count.ShouldBe(2);
        published.ShouldAllBe(e => !e.Data.Draft);
    }

    [Fact]
    public void GetCollectionShouldReturnEmptyForEmptyDirectory()
    {
        var config = new CollectionConfig("content")
            .AddCollection(ContentCollection.Define<BlogPost>("blog"));
        var provider = new InMemoryFileProvider(); // No files added
        var loader = new CollectionLoader(config, provider);
        var query = new CollectionQuery(loader);

        var entries = query.GetCollection<BlogPost>("blog");

        entries.ShouldBeEmpty();
    }

    [Fact]
    public void GetCollectionShouldThrowForUnknownCollection()
    {
        var query = CreateBlogQuery();

        Should.Throw<KeyNotFoundException>(() => query.GetCollection<BlogPost>("unknown"));
    }

    [Fact]
    public void GetCollectionShouldThrowForTypeMismatch()
    {
        var query = CreateBlogQuery();

        Should.Throw<InvalidOperationException>(() => query.GetCollection<Author>("blog"));
    }

    // --- GetEntry tests ---

    [Fact]
    public void GetEntryShouldReturnMatchingEntry()
    {
        var query = CreateBlogQuery();
        var entry = query.GetEntry<BlogPost>("blog", "second-post");

        entry.ShouldNotBeNull();
        entry.Slug.ShouldBe("second-post");
        entry.Data.Title.ShouldBe("Second Post");
        entry.Data.Draft.ShouldBeTrue();
    }

    [Fact]
    public void GetEntryShouldReturnNullForMissingSlug()
    {
        var query = CreateBlogQuery();
        var entry = query.GetEntry<BlogPost>("blog", "nonexistent");

        entry.ShouldBeNull();
    }

    [Fact]
    public void GetEntryShouldThrowForUnknownCollection()
    {
        var query = CreateBlogQuery();

        Should.Throw<KeyNotFoundException>(() => query.GetEntry<BlogPost>("unknown", "slug"));
    }

    // --- Render tests ---

    [Fact]
    public void RenderShouldProduceHtml()
    {
        var query = CreateBlogQuery();
        var entry = query.GetEntry<BlogPost>("blog", "first-post")!;
        var rendered = query.Render(entry);

        rendered.Html.ShouldContain("<h1");
        rendered.Html.ShouldContain("First Post");
        rendered.Html.ShouldContain("<p>Hello world!</p>");
    }

    [Fact]
    public void RenderShouldExtractHeadings()
    {
        var query = CreateBlogQuery();
        var entry = query.GetEntry<BlogPost>("blog", "first-post")!;
        var rendered = query.Render(entry);

        rendered.Headings.Count.ShouldBe(1);
        rendered.Headings[0].Depth.ShouldBe(1);
        rendered.Headings[0].Text.ShouldBe("First Post");
    }

    [Fact]
    public void RenderShouldThrowOnNullEntry()
    {
        var query = CreateBlogQuery();

        Should.Throw<ArgumentNullException>(() => query.Render<BlogPost>(null!));
    }

    // --- Validation tests ---

    [Fact]
    public void ShouldThrowValidationExceptionForInvalidFrontmatter()
    {
        var config = new CollectionConfig("content")
            .AddCollection(ContentCollection.Define<BlogPost>("blog"));
        var provider = new InMemoryFileProvider()
            .AddFile(BlogDir, "bad-post.md", "---\ndescription: No title\n---\nBody");
        var loader = new CollectionLoader(config, provider);
        var query = new CollectionQuery(loader);

        Should.Throw<FrontmatterValidationException>(() => query.GetCollection<BlogPost>("blog"));
    }

    // --- Multiple collections ---

    [Fact]
    public void ShouldSupportMultipleCollections()
    {
        var config = new CollectionConfig("content")
            .AddCollection(ContentCollection.Define<BlogPost>("blog"))
            .AddCollection(ContentCollection.Define<Author>("authors"));

        var provider = CreateBlogProvider()
            .AddFile(AuthorsDir, "alice.md", "---\nname: Alice\nbio: Developer\n---\nAlice's bio.");

        var loader = new CollectionLoader(config, provider);
        var query = new CollectionQuery(loader);

        var posts = query.GetCollection<BlogPost>("blog");
        var authors = query.GetCollection<Author>("authors");

        posts.Count.ShouldBe(3);
        authors.Count.ShouldBe(1);
        authors[0].Data.Name.ShouldBe("Alice");
    }

    // --- End-to-end round-trip ---

    [Fact]
    public void ShouldPerformFullRoundTrip()
    {
        var config = new CollectionConfig("content")
            .AddCollection(ContentCollection.Define<BlogPost>("blog"));

        var provider = new InMemoryFileProvider()
            .AddFile(BlogDir, "my-post.md", """
                ---
                title: My Amazing Post
                description: An in-depth guide
                pubDate: 2026-06-15
                tags:
                  - guide
                  - tutorial
                ---
                # My Amazing Post

                ## Getting Started

                Welcome to this **tutorial**. Here's a code example:

                ```csharp
                var x = 42;
                ```

                ## Conclusion

                Thanks for reading!
                """);

        var loader = new CollectionLoader(config, provider);
        var query = new CollectionQuery(loader);

        // Load
        var entry = query.GetEntry<BlogPost>("blog", "my-post")!;
        entry.Data.Title.ShouldBe("My Amazing Post");
        entry.Data.Tags.Count.ShouldBe(2);

        // Render
        var rendered = query.Render(entry);
        rendered.Html.ShouldContain("<h1");
        rendered.Html.ShouldContain("<h2");
        rendered.Html.ShouldContain("<strong>tutorial</strong>");
        rendered.Html.ShouldContain("<pre>");
        rendered.Html.ShouldContain("language-csharp");

        // Headings for TOC
        rendered.Headings.Count.ShouldBe(3);
        rendered.Headings[0].Text.ShouldBe("My Amazing Post");
        rendered.Headings[1].Text.ShouldBe("Getting Started");
        rendered.Headings[2].Text.ShouldBe("Conclusion");
    }
}
