using Atoll.Build.Content.Collections;

namespace Atoll.Build.Tests.Content.Collections;

public sealed class RecursiveFileProviderTests
{
    // CollectionConfig("") uses "" as base dir, collection "content" → directory "content".
    // For tests using "/content" as the direct scan path we set baseDir="/" and collection="content".
    // Path.Combine("/", "content") = "/content" on all platforms.
    private static CollectionConfig CreateConfig() =>
        new CollectionConfig("/")
            .AddCollection(ContentCollection.Define<TestSchema>("content"));

    // --- InMemoryFileProvider recursive tests ---

    [Fact]
    public void InMemoryGetMarkdownFilesRecursiveShouldReturnFilesFromSubdirectories()
    {
        var provider = new InMemoryFileProvider()
            .AddFile("/content", "page.md", "top level")
            .AddFile("/content/guides", "getting-started.md", "guide content")
            .AddFile("/content/reference", "api.md", "api content");

        var files = provider.GetMarkdownFiles("/content", recursive: true);

        files.Count.ShouldBe(3);
        files.Select(f => f.RelativePath).ShouldContain("page.md");
        files.Select(f => f.RelativePath).ShouldContain("guides/getting-started.md");
        files.Select(f => f.RelativePath).ShouldContain("reference/api.md");
    }

    [Fact]
    public void InMemoryGetMarkdownFilesNonRecursiveShouldNotReturnSubdirectoryFiles()
    {
        var provider = new InMemoryFileProvider()
            .AddFile("/content", "page.md", "top level")
            .AddFile("/content/guides", "getting-started.md", "guide content");

        var files = provider.GetMarkdownFiles("/content", recursive: false);

        files.Count.ShouldBe(1);
        files[0].RelativePath.ShouldBe("page.md");
    }

    [Fact]
    public void InMemoryGetMarkdownFilesNonRecursiveShouldMatchNonRecursiveOverload()
    {
        var provider = new InMemoryFileProvider()
            .AddFile("/content", "page.md", "top level")
            .AddFile("/content/guides", "guide.md", "guide");

        var explicit_ = provider.GetMarkdownFiles("/content", recursive: false);
        var implicit_ = provider.GetMarkdownFiles("/content");

        explicit_.Count.ShouldBe(implicit_.Count);
        explicit_.Select(f => f.RelativePath).ShouldBe(implicit_.Select(f => f.RelativePath));
    }

    [Fact]
    public void InMemoryGetMarkdownFilesRecursiveShouldIncludeSubdirectoryInRelativePath()
    {
        var provider = new InMemoryFileProvider()
            .AddFile("/content/guides", "getting-started.md", "# Getting Started");

        var files = provider.GetMarkdownFiles("/content", recursive: true);

        files.Count.ShouldBe(1);
        files[0].RelativePath.ShouldBe("guides/getting-started.md");
    }

    [Fact]
    public void InMemoryGetMarkdownFilesRecursiveShouldReturnEmptyWhenNoFiles()
    {
        var provider = new InMemoryFileProvider();

        var files = provider.GetMarkdownFiles("/content", recursive: true);

        files.ShouldBeEmpty();
    }

    [Fact]
    public void InMemoryGetMarkdownFilesRecursiveShouldExcludeNonMdFiles()
    {
        var provider = new InMemoryFileProvider()
            .AddFile("/content", "page.md", "markdown")
            .AddFile("/content/guides", "style.css", "css content");

        var files = provider.GetMarkdownFiles("/content", recursive: true);

        files.Count.ShouldBe(1);
        files[0].RelativePath.ShouldBe("page.md");
    }

    [Fact]
    public void InMemoryGetMarkdownFilesRecursiveShouldSupportDeeplyNestedPaths()
    {
        var provider = new InMemoryFileProvider()
            .AddFile("/content/guides/advanced", "deep-topic.md", "deep content");

        var files = provider.GetMarkdownFiles("/content", recursive: true);

        files.Count.ShouldBe(1);
        files[0].RelativePath.ShouldBe("guides/advanced/deep-topic.md");
    }

    // --- Slug generation for nested paths ---

    [Fact]
    public void SlugForTopLevelFileShouldBeFileNameWithoutExtension()
    {
        var provider = new InMemoryFileProvider()
            .AddFile("/content", "my-post.md", "---\ntitle: My Post\n---\nbody");

        var config = CreateConfig();
        var loader = new CollectionLoader(config, provider);

        var entries = loader.LoadCollection<TestSchema>("content");

        entries[0].Slug.ShouldBe("my-post");
    }

    [Fact]
    public void SlugForNestedFileShouldIncludeSubdirectory()
    {
        var provider = new InMemoryFileProvider()
            .AddFile("/content/guides", "getting-started.md", "---\ntitle: Getting Started\n---\nbody");

        var config = CreateConfig();
        var loader = new CollectionLoader(config, provider);

        var entries = loader.LoadCollection<TestSchema>("content", recursive: true);

        entries[0].Slug.ShouldBe("guides/getting-started");
    }

    [Fact]
    public void LoadCollectionRecursiveShouldReturnEntriesFromAllSubdirectories()
    {
        var provider = new InMemoryFileProvider()
            .AddFile("/content", "index.md", "---\ntitle: Index\n---\ncontent")
            .AddFile("/content/guides", "intro.md", "---\ntitle: Intro\n---\ncontent")
            .AddFile("/content/reference", "api.md", "---\ntitle: API\n---\ncontent");

        var config = CreateConfig();
        var loader = new CollectionLoader(config, provider);

        var entries = loader.LoadCollection<TestSchema>("content", recursive: true);

        entries.Count.ShouldBe(3);
        entries.Select(e => e.Slug).ShouldContain("index");
        entries.Select(e => e.Slug).ShouldContain("guides/intro");
        entries.Select(e => e.Slug).ShouldContain("reference/api");
    }

    [Fact]
    public void LoadCollectionNonRecursiveShouldNotBreakExistingBehavior()
    {
        var provider = new InMemoryFileProvider()
            .AddFile("/content", "page.md", "---\ntitle: Page\n---\nbody")
            .AddFile("/content/sub", "hidden.md", "---\ntitle: Hidden\n---\nbody");

        var config = CreateConfig();
        var loader = new CollectionLoader(config, provider);

        // The original non-recursive overload should not see subdirectory files.
        var entries = loader.LoadCollection<TestSchema>("content");

        entries.Count.ShouldBe(1);
        entries[0].Slug.ShouldBe("page");
    }

    private sealed class TestSchema
    {
        public string Title { get; set; } = "";
    }
}
