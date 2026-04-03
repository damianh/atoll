using Atoll.Content.Collections;
using Atoll.Content.Markdown;
using Shouldly;
using Xunit;

namespace Atoll.Build.Tests.Content.Collections;

public sealed class ContentCollectionDefinitionTests
{
    // --- Schema types for testing ---

    private sealed class BlogPost
    {
        public string Title { get; set; } = "";
        public DateTime Date { get; set; }
    }

    private sealed class Author
    {
        public string Name { get; set; } = "";
        public string Bio { get; set; } = "";
    }

    // --- ContentCollection.Define tests ---

    [Fact]
    public void DefineShouldCreateCollectionWithName()
    {
        var collection = ContentCollection.Define<BlogPost>("blog");

        collection.Name.ShouldBe("blog");
    }

    [Fact]
    public void DefineShouldCaptureSchemaType()
    {
        var collection = ContentCollection.Define<BlogPost>("blog");

        collection.SchemaType.ShouldBe(typeof(BlogPost));
    }

    [Fact]
    public void DefineShouldThrowOnNullName()
    {
        Should.Throw<ArgumentNullException>(() => ContentCollection.Define<BlogPost>(null!));
    }

    [Fact]
    public void DefineShouldThrowOnEmptyName()
    {
        Should.Throw<ArgumentException>(() => ContentCollection.Define<BlogPost>(""));
    }

    [Fact]
    public void DefineShouldThrowOnWhitespaceName()
    {
        Should.Throw<ArgumentException>(() => ContentCollection.Define<BlogPost>("  "));
    }

    // --- CollectionConfig tests ---

    [Fact]
    public void ConfigShouldUseDefaultBaseDirectory()
    {
        var config = new CollectionConfig();

        config.BaseDirectory.ShouldBe(CollectionConfig.DefaultBaseDirectory);
    }

    [Fact]
    public void ConfigShouldAcceptCustomBaseDirectory()
    {
        var config = new CollectionConfig("content");

        config.BaseDirectory.ShouldBe("content");
    }

    [Fact]
    public void ConfigShouldThrowOnNullBaseDirectory()
    {
        Should.Throw<ArgumentNullException>(() => new CollectionConfig(null!));
    }

    [Fact]
    public void ConfigShouldThrowOnEmptyBaseDirectory()
    {
        Should.Throw<ArgumentException>(() => new CollectionConfig(""));
    }

    [Fact]
    public void ConfigShouldThrowOnWhitespaceBaseDirectory()
    {
        Should.Throw<ArgumentException>(() => new CollectionConfig("   "));
    }

    [Fact]
    public void AddCollectionShouldRegisterCollection()
    {
        var config = new CollectionConfig()
            .AddCollection(ContentCollection.Define<BlogPost>("blog"));

        config.Collections.ShouldContainKey("blog");
        config.Collections["blog"].SchemaType.ShouldBe(typeof(BlogPost));
    }

    [Fact]
    public void AddCollectionShouldSupportFluentChaining()
    {
        var config = new CollectionConfig()
            .AddCollection(ContentCollection.Define<BlogPost>("blog"))
            .AddCollection(ContentCollection.Define<Author>("authors"));

        config.Collections.Count.ShouldBe(2);
    }

    [Fact]
    public void AddCollectionShouldThrowOnDuplicateName()
    {
        var config = new CollectionConfig()
            .AddCollection(ContentCollection.Define<BlogPost>("blog"));

        Should.Throw<ArgumentException>(() =>
            config.AddCollection(ContentCollection.Define<BlogPost>("blog")));
    }

    [Fact]
    public void AddCollectionShouldBeCaseInsensitive()
    {
        var config = new CollectionConfig()
            .AddCollection(ContentCollection.Define<BlogPost>("Blog"));

        Should.Throw<ArgumentException>(() =>
            config.AddCollection(ContentCollection.Define<BlogPost>("blog")));
    }

    [Fact]
    public void AddCollectionShouldThrowOnNullCollection()
    {
        var config = new CollectionConfig();

        Should.Throw<ArgumentNullException>(() => config.AddCollection(null!));
    }

    [Fact]
    public void GetCollectionShouldReturnRegisteredCollection()
    {
        var config = new CollectionConfig()
            .AddCollection(ContentCollection.Define<BlogPost>("blog"));

        var collection = config.GetCollection("blog");

        collection.Name.ShouldBe("blog");
        collection.SchemaType.ShouldBe(typeof(BlogPost));
    }

    [Fact]
    public void GetCollectionShouldBeCaseInsensitive()
    {
        var config = new CollectionConfig()
            .AddCollection(ContentCollection.Define<BlogPost>("Blog"));

        var collection = config.GetCollection("blog");

        collection.ShouldNotBeNull();
    }

    [Fact]
    public void GetCollectionShouldThrowOnUnknownName()
    {
        var config = new CollectionConfig()
            .AddCollection(ContentCollection.Define<BlogPost>("blog"));

        Should.Throw<KeyNotFoundException>(() => config.GetCollection("unknown"));
    }

    [Fact]
    public void GetCollectionShouldThrowOnNullName()
    {
        var config = new CollectionConfig();

        Should.Throw<ArgumentNullException>(() => config.GetCollection(null!));
    }

    [Fact]
    public void GetCollectionDirectoryShouldCombineBaseDirAndName()
    {
        var config = new CollectionConfig("content");

        var dir = config.GetCollectionDirectory("blog");

        dir.ShouldBe(Path.Combine("content", "blog"));
    }

    [Fact]
    public void GetCollectionDirectoryShouldThrowOnNullName()
    {
        var config = new CollectionConfig("content");

        Should.Throw<ArgumentNullException>(() => config.GetCollectionDirectory(null!));
    }

    // --- ContentEntry tests ---

    [Fact]
    public void ContentEntryShouldStoreAllProperties()
    {
        var data = new BlogPost { Title = "Hello", Date = new DateTime(2026, 1, 1) };
        var entry = new ContentEntry<BlogPost>("blog/hello.md", "blog", "hello", "# Hello", data);

        entry.Id.ShouldBe("blog/hello.md");
        entry.Collection.ShouldBe("blog");
        entry.Slug.ShouldBe("hello");
        entry.Body.ShouldBe("# Hello");
        entry.Data.ShouldBeSameAs(data);
    }

    [Fact]
    public void ContentEntryShouldThrowOnNullId()
    {
        Should.Throw<ArgumentNullException>(() =>
            new ContentEntry<BlogPost>(null!, "blog", "hello", "body", new BlogPost()));
    }

    [Fact]
    public void ContentEntryShouldThrowOnNullCollection()
    {
        Should.Throw<ArgumentNullException>(() =>
            new ContentEntry<BlogPost>("id", null!, "hello", "body", new BlogPost()));
    }

    [Fact]
    public void ContentEntryShouldThrowOnNullSlug()
    {
        Should.Throw<ArgumentNullException>(() =>
            new ContentEntry<BlogPost>("id", "blog", null!, "body", new BlogPost()));
    }

    [Fact]
    public void ContentEntryShouldThrowOnNullBody()
    {
        Should.Throw<ArgumentNullException>(() =>
            new ContentEntry<BlogPost>("id", "blog", "hello", null!, new BlogPost()));
    }

    [Fact]
    public void ContentEntryShouldThrowOnNullData()
    {
        Should.Throw<ArgumentNullException>(() =>
            new ContentEntry<BlogPost>("id", "blog", "hello", "body", null!));
    }
}

public sealed class RenderedContentTests
{
    [Fact]
    public void ShouldStoreHtmlAndHeadings()
    {
        var headings = new List<MarkdownHeading> { new(1, "Title", "title") };
        var rendered = new RenderedContent("<h1>Title</h1>", headings);

        rendered.Html.ShouldBe("<h1>Title</h1>");
        rendered.Headings.Count.ShouldBe(1);
        rendered.Headings[0].Text.ShouldBe("Title");
    }

    [Fact]
    public void ShouldThrowOnNullHtml()
    {
        Should.Throw<ArgumentNullException>(() =>
            new RenderedContent(null!, []));
    }

    [Fact]
    public void ShouldThrowOnNullHeadings()
    {
        Should.Throw<ArgumentNullException>(() =>
            new RenderedContent("<p>test</p>", null!));
    }

    [Fact]
    public void ShouldSupportEmptyHtmlAndHeadings()
    {
        var rendered = new RenderedContent("", []);

        rendered.Html.ShouldBeEmpty();
        rendered.Headings.ShouldBeEmpty();
    }
}

public sealed class ContentFileTests
{
    [Fact]
    public void ShouldStoreRelativePathAndContent()
    {
        var file = new ContentFile("my-post.md", "# Hello");

        file.RelativePath.ShouldBe("my-post.md");
        file.Content.ShouldBe("# Hello");
    }

    [Fact]
    public void ShouldThrowOnNullRelativePath()
    {
        Should.Throw<ArgumentNullException>(() => new ContentFile(null!, "content"));
    }

    [Fact]
    public void ShouldThrowOnNullContent()
    {
        Should.Throw<ArgumentNullException>(() => new ContentFile("file.md", null!));
    }
}

public sealed class InMemoryFileProviderTests
{
    [Fact]
    public void ShouldReturnEmptyListForUnknownDirectory()
    {
        var provider = new InMemoryFileProvider();
        var files = provider.GetMarkdownFiles("nonexistent");

        files.ShouldBeEmpty();
    }

    [Fact]
    public void ShouldReturnNullForMissingSlug()
    {
        var provider = new InMemoryFileProvider();
        var file = provider.GetMarkdownFile("dir", "missing");

        file.ShouldBeNull();
    }

    [Fact]
    public void ShouldReturnNullForSlugInUnknownDirectory()
    {
        var provider = new InMemoryFileProvider()
            .AddFile("dir1", "post.md", "content");

        var file = provider.GetMarkdownFile("dir2", "post");

        file.ShouldBeNull();
    }

    [Fact]
    public void ShouldFilterToMarkdownFilesOnly()
    {
        var provider = new InMemoryFileProvider()
            .AddFile("dir", "post.md", "# Post")
            .AddFile("dir", "image.png", "binary data")
            .AddFile("dir", "readme.txt", "readme");

        var files = provider.GetMarkdownFiles("dir");

        files.Count.ShouldBe(1);
        files[0].RelativePath.ShouldBe("post.md");
    }

    [Fact]
    public void ShouldNormalizePathSeparators()
    {
        var provider = new InMemoryFileProvider()
            .AddFile("content\\blog", "post.md", "# Post");

        var files = provider.GetMarkdownFiles("content/blog");

        files.Count.ShouldBe(1);
    }

    [Fact]
    public void ShouldHandleTrailingSlash()
    {
        var provider = new InMemoryFileProvider()
            .AddFile("content/blog/", "post.md", "# Post");

        var files = provider.GetMarkdownFiles("content/blog");

        files.Count.ShouldBe(1);
    }

    [Fact]
    public void ShouldFindSlugCaseInsensitively()
    {
        var provider = new InMemoryFileProvider()
            .AddFile("dir", "MyPost.md", "# Content");

        var file = provider.GetMarkdownFile("dir", "mypost");

        file.ShouldNotBeNull();
    }

    [Fact]
    public void AddFileShouldThrowOnNullDirectory()
    {
        var provider = new InMemoryFileProvider();

        Should.Throw<ArgumentNullException>(() => provider.AddFile(null!, "file.md", "content"));
    }

    [Fact]
    public void AddFileShouldThrowOnNullRelativePath()
    {
        var provider = new InMemoryFileProvider();

        Should.Throw<ArgumentNullException>(() => provider.AddFile("dir", null!, "content"));
    }

    [Fact]
    public void AddFileShouldThrowOnNullContent()
    {
        var provider = new InMemoryFileProvider();

        Should.Throw<ArgumentNullException>(() => provider.AddFile("dir", "file.md", null!));
    }

    [Fact]
    public void GetMarkdownFilesShouldThrowOnNullDirectory()
    {
        var provider = new InMemoryFileProvider();

        Should.Throw<ArgumentNullException>(() => provider.GetMarkdownFiles(null!));
    }

    [Fact]
    public void GetMarkdownFileShouldThrowOnNullDirectory()
    {
        var provider = new InMemoryFileProvider();

        Should.Throw<ArgumentNullException>(() => provider.GetMarkdownFile(null!, "slug"));
    }

    [Fact]
    public void GetMarkdownFileShouldThrowOnNullSlug()
    {
        var provider = new InMemoryFileProvider();

        Should.Throw<ArgumentNullException>(() => provider.GetMarkdownFile("dir", null!));
    }

    [Fact]
    public void ShouldSupportMultipleFilesInSameDirectory()
    {
        var provider = new InMemoryFileProvider()
            .AddFile("dir", "a.md", "# A")
            .AddFile("dir", "b.md", "# B")
            .AddFile("dir", "c.md", "# C");

        var files = provider.GetMarkdownFiles("dir");

        files.Count.ShouldBe(3);
    }
}

public sealed class CollectionLoaderTests
{
    private sealed class SimpleSchema
    {
        public string Title { get; set; } = "";
    }

    private sealed class OtherSchema
    {
        public string Name { get; set; } = "";
    }

    [Fact]
    public void ShouldThrowOnNullConfigInConstructor()
    {
        Should.Throw<ArgumentNullException>(() =>
            new CollectionLoader(null!, new InMemoryFileProvider()));
    }

    [Fact]
    public void ShouldThrowOnNullFileProviderInConstructor()
    {
        Should.Throw<ArgumentNullException>(() =>
            new CollectionLoader(new CollectionConfig("content"), null!));
    }

    [Fact]
    public void LoadCollectionShouldThrowOnNullCollectionName()
    {
        var config = new CollectionConfig("content")
            .AddCollection(ContentCollection.Define<SimpleSchema>("blog"));
        var loader = new CollectionLoader(config, new InMemoryFileProvider());

        Should.Throw<ArgumentNullException>(() => loader.LoadCollection<SimpleSchema>(null!));
    }

    [Fact]
    public void LoadEntryShouldThrowOnNullCollectionName()
    {
        var config = new CollectionConfig("content")
            .AddCollection(ContentCollection.Define<SimpleSchema>("blog"));
        var loader = new CollectionLoader(config, new InMemoryFileProvider());

        Should.Throw<ArgumentNullException>(() => loader.LoadEntry<SimpleSchema>(null!, "slug"));
    }

    [Fact]
    public void LoadEntryShouldThrowOnNullSlug()
    {
        var config = new CollectionConfig("content")
            .AddCollection(ContentCollection.Define<SimpleSchema>("blog"));
        var loader = new CollectionLoader(config, new InMemoryFileProvider());

        Should.Throw<ArgumentNullException>(() => loader.LoadEntry<SimpleSchema>("blog", null!));
    }

    [Fact]
    public void LoadCollectionShouldThrowOnSchemaTypeMismatch()
    {
        var config = new CollectionConfig("content")
            .AddCollection(ContentCollection.Define<SimpleSchema>("blog"));
        var loader = new CollectionLoader(config, new InMemoryFileProvider());

        Should.Throw<InvalidOperationException>(() => loader.LoadCollection<OtherSchema>("blog"));
    }

    [Fact]
    public void LoadEntryShouldThrowOnSchemaTypeMismatch()
    {
        var config = new CollectionConfig("content")
            .AddCollection(ContentCollection.Define<SimpleSchema>("blog"));
        var loader = new CollectionLoader(config, new InMemoryFileProvider());

        Should.Throw<InvalidOperationException>(() => loader.LoadEntry<OtherSchema>("blog", "slug"));
    }

    [Fact]
    public void LoadEntryShouldReturnNullWhenFileNotFound()
    {
        var config = new CollectionConfig("content")
            .AddCollection(ContentCollection.Define<SimpleSchema>("blog"));
        var loader = new CollectionLoader(config, new InMemoryFileProvider());

        var entry = loader.LoadEntry<SimpleSchema>("blog", "nonexistent");

        entry.ShouldBeNull();
    }

    [Fact]
    public void LoadEntryShouldLoadSingleEntry()
    {
        var config = new CollectionConfig("content")
            .AddCollection(ContentCollection.Define<SimpleSchema>("blog"));
        var provider = new InMemoryFileProvider()
            .AddFile(Path.Combine("content", "blog"), "my-post.md",
                "---\ntitle: My Post\n---\n# Content");
        var loader = new CollectionLoader(config, provider);

        var entry = loader.LoadEntry<SimpleSchema>("blog", "my-post");

        entry.ShouldNotBeNull();
        entry.Slug.ShouldBe("my-post");
        entry.Data.Title.ShouldBe("My Post");
        entry.Body.ShouldContain("# Content");
    }

    [Fact]
    public void LoadCollectionShouldReturnEntriesSortedByFileName()
    {
        var config = new CollectionConfig("content")
            .AddCollection(ContentCollection.Define<SimpleSchema>("blog"));
        var dir = Path.Combine("content", "blog");
        var provider = new InMemoryFileProvider()
            .AddFile(dir, "charlie.md", "---\ntitle: C\n---\nC")
            .AddFile(dir, "alpha.md", "---\ntitle: A\n---\nA")
            .AddFile(dir, "bravo.md", "---\ntitle: B\n---\nB");
        var loader = new CollectionLoader(config, provider);

        var entries = loader.LoadCollection<SimpleSchema>("blog");

        entries.Count.ShouldBe(3);
        entries[0].Slug.ShouldBe("alpha");
        entries[1].Slug.ShouldBe("bravo");
        entries[2].Slug.ShouldBe("charlie");
    }
}

public sealed class CollectionQueryConstructorTests
{
    [Fact]
    public void ShouldThrowOnNullLoader()
    {
        Should.Throw<ArgumentNullException>(() => new CollectionQuery(null!));
    }

    [Fact]
    public void ShouldThrowOnNullMarkdownOptions()
    {
        var config = new CollectionConfig("content")
            .AddCollection(ContentCollection.Define<SimpleQuerySchema>("blog"));
        var loader = new CollectionLoader(config, new InMemoryFileProvider());

        Should.Throw<ArgumentNullException>(() => new CollectionQuery(loader, null!));
    }

    [Fact]
    public void ShouldAcceptCustomMarkdownOptions()
    {
        var config = new CollectionConfig("content")
            .AddCollection(ContentCollection.Define<SimpleQuerySchema>("blog"));
        var dir = Path.Combine("content", "blog");
        var provider = new InMemoryFileProvider()
            .AddFile(dir, "post.md", "---\ntitle: Post\n---\n# Heading");
        var loader = new CollectionLoader(config, provider);
        var options = new MarkdownOptions { EnableAutoIdentifiers = false };

        var query = new CollectionQuery(loader, options);
        var entry = query.GetEntry<SimpleQuerySchema>("blog", "post");
        var rendered = query.Render(entry!);

        // With autoidentifiers disabled, headings should not have IDs
        rendered.Headings[0].Id.ShouldBeNull();
    }

    [Fact]
    public void GetCollectionWithNullPredicateShouldThrow()
    {
        var config = new CollectionConfig("content")
            .AddCollection(ContentCollection.Define<SimpleQuerySchema>("blog"));
        var loader = new CollectionLoader(config, new InMemoryFileProvider());
        var query = new CollectionQuery(loader);

        Should.Throw<ArgumentNullException>(() =>
            query.GetCollection<SimpleQuerySchema>("blog", null!));
    }

    private sealed class SimpleQuerySchema
    {
        public string Title { get; set; } = "";
    }
}

public sealed class MarkdownHeadingTests
{
    [Fact]
    public void ShouldStoreDepthTextAndId()
    {
        var heading = new MarkdownHeading(2, "Introduction", "introduction");

        heading.Depth.ShouldBe(2);
        heading.Text.ShouldBe("Introduction");
        heading.Id.ShouldBe("introduction");
    }

    [Fact]
    public void ShouldSupportNullId()
    {
        var heading = new MarkdownHeading(1, "Title", null);

        heading.Id.ShouldBeNull();
    }
}

public sealed class MarkdownRenderResultTests
{
    [Fact]
    public void ShouldThrowOnNullHtml()
    {
        Should.Throw<ArgumentNullException>(() =>
            new MarkdownRenderResult(null!, []));
    }

    [Fact]
    public void ShouldThrowOnNullHeadings()
    {
        Should.Throw<ArgumentNullException>(() =>
            new MarkdownRenderResult("<p>test</p>", null!));
    }

    [Fact]
    public void ShouldStoreHtmlAndHeadings()
    {
        var headings = new List<MarkdownHeading> { new(1, "Title", null) };
        var result = new MarkdownRenderResult("<h1>Title</h1>", headings);

        result.Html.ShouldBe("<h1>Title</h1>");
        result.Headings.Count.ShouldBe(1);
    }
}
