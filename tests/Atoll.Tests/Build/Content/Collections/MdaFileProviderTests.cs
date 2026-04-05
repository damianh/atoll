using Atoll.Build.Content.Collections;
using Shouldly;
using Xunit;

namespace Atoll.Build.Tests.Content.Collections;

/// <summary>
/// Tests for <c>.mda</c> (Markdown Atoll) file extension support in
/// <see cref="InMemoryFileProvider"/> and <see cref="CollectionLoader"/>.
/// </summary>
public sealed class MdaFileProviderTests
{
    private static CollectionConfig CreateConfig() =>
        new CollectionConfig("/")
            .AddCollection(ContentCollection.Define<MdaTestSchema>("content"));

    // ── InMemoryFileProvider — non-recursive ──

    [Fact]
    public void InMemoryGetMarkdownFilesShouldIncludeMdaFiles()
    {
        var provider = new InMemoryFileProvider()
            .AddFile("/content", "standard.md", "md content")
            .AddFile("/content", "atoll.mda", "mda content");

        var files = provider.GetMarkdownFiles("/content");

        files.Count.ShouldBe(2);
        files.Select(f => f.RelativePath).ShouldContain("standard.md");
        files.Select(f => f.RelativePath).ShouldContain("atoll.mda");
    }

    [Fact]
    public void InMemoryGetMarkdownFilesShouldExcludeOtherExtensionsAlongsideMda()
    {
        var provider = new InMemoryFileProvider()
            .AddFile("/content", "page.mda", "mda content")
            .AddFile("/content", "style.css", "css content")
            .AddFile("/content", "data.json", "json content");

        var files = provider.GetMarkdownFiles("/content");

        files.Count.ShouldBe(1);
        files[0].RelativePath.ShouldBe("page.mda");
    }

    // ── InMemoryFileProvider — recursive ──

    [Fact]
    public void InMemoryGetMarkdownFilesRecursiveShouldIncludeMdaFiles()
    {
        var provider = new InMemoryFileProvider()
            .AddFile("/content", "index.md", "top level")
            .AddFile("/content/guides", "guide.mda", "guide mda content");

        var files = provider.GetMarkdownFiles("/content", recursive: true);

        files.Count.ShouldBe(2);
        files.Select(f => f.RelativePath).ShouldContain("index.md");
        files.Select(f => f.RelativePath).ShouldContain("guides/guide.mda");
    }

    [Fact]
    public void InMemoryGetMarkdownFilesRecursiveShouldIncludeMdaRelativePath()
    {
        var provider = new InMemoryFileProvider()
            .AddFile("/content/guides", "topic.mda", "mda content");

        var files = provider.GetMarkdownFiles("/content", recursive: true);

        files.Count.ShouldBe(1);
        files[0].RelativePath.ShouldBe("guides/topic.mda");
    }

    // ── InMemoryFileProvider — GetMarkdownFile fallback ──

    [Fact]
    public void InMemoryGetMarkdownFileShouldFallBackToMda()
    {
        var provider = new InMemoryFileProvider()
            .AddFile("/content", "atoll.mda", "mda content");

        var file = provider.GetMarkdownFile("/content", "atoll");

        file.ShouldNotBeNull();
        file!.RelativePath.ShouldBe("atoll.mda");
        file.Content.ShouldBe("mda content");
    }

    [Fact]
    public void InMemoryGetMarkdownFileShouldPreferMdOverMda()
    {
        var provider = new InMemoryFileProvider()
            .AddFile("/content", "page.md", "md wins")
            .AddFile("/content", "page.mda", "mda loses");

        var file = provider.GetMarkdownFile("/content", "page");

        file.ShouldNotBeNull();
        file!.RelativePath.ShouldBe("page.md");
        file.Content.ShouldBe("md wins");
    }

    [Fact]
    public void InMemoryGetMarkdownFileShouldReturnNullWhenNeitherExtensionExists()
    {
        var provider = new InMemoryFileProvider()
            .AddFile("/content", "other.txt", "not markdown");

        var file = provider.GetMarkdownFile("/content", "other");

        file.ShouldBeNull();
    }

    // ── CollectionLoader integration — .mda slug derivation ──

    [Fact]
    public void SlugForTopLevelMdaFileShouldBeFileNameWithoutExtension()
    {
        var provider = new InMemoryFileProvider()
            .AddFile("/content", "my-page.mda", "---\ntitle: My Page\n---\nbody");

        var config = CreateConfig();
        var loader = new CollectionLoader(config, provider);

        var entries = loader.LoadCollection<MdaTestSchema>("content");

        entries.Count.ShouldBe(1);
        entries[0].Slug.ShouldBe("my-page");
    }

    [Fact]
    public void SlugForNestedMdaFileShouldIncludeSubdirectory()
    {
        var provider = new InMemoryFileProvider()
            .AddFile("/content/guides", "intro.mda", "---\ntitle: Intro\n---\nbody");

        var config = CreateConfig();
        var loader = new CollectionLoader(config, provider);

        var entries = loader.LoadCollection<MdaTestSchema>("content", recursive: true);

        entries.Count.ShouldBe(1);
        entries[0].Slug.ShouldBe("guides/intro");
    }

    [Fact]
    public void LoadCollectionShouldReturnBothMdAndMdaEntries()
    {
        var provider = new InMemoryFileProvider()
            .AddFile("/content", "standard.md", "---\ntitle: Standard\n---\nbody")
            .AddFile("/content", "enhanced.mda", "---\ntitle: Enhanced\n---\nbody");

        var config = CreateConfig();
        var loader = new CollectionLoader(config, provider);

        var entries = loader.LoadCollection<MdaTestSchema>("content");

        entries.Count.ShouldBe(2);
        entries.Select(e => e.Slug).ShouldContain("standard");
        entries.Select(e => e.Slug).ShouldContain("enhanced");
    }

    [Fact]
    public void LoadCollectionRecursiveShouldReturnMdaEntriesFromSubdirectories()
    {
        var provider = new InMemoryFileProvider()
            .AddFile("/content", "index.md", "---\ntitle: Index\n---\ncontent")
            .AddFile("/content/guides", "topic.mda", "---\ntitle: Topic\n---\ncontent");

        var config = CreateConfig();
        var loader = new CollectionLoader(config, provider);

        var entries = loader.LoadCollection<MdaTestSchema>("content", recursive: true);

        entries.Count.ShouldBe(2);
        entries.Select(e => e.Slug).ShouldContain("index");
        entries.Select(e => e.Slug).ShouldContain("guides/topic");
    }

    [Fact]
    public void LoadEntryShouldResolveMdaFileBySlug()
    {
        var provider = new InMemoryFileProvider()
            .AddFile("/content", "atoll-page.mda", "---\ntitle: Atoll Page\n---\nbody");

        var config = CreateConfig();
        var loader = new CollectionLoader(config, provider);

        var entry = loader.LoadEntry<MdaTestSchema>("content", "atoll-page");

        entry.ShouldNotBeNull();
        entry!.Slug.ShouldBe("atoll-page");
        entry.Data.Title.ShouldBe("Atoll Page");
    }

    // ── Coexistence — .md takes priority over .mda in listings ──

    [Fact]
    public void GetMarkdownFilesShouldDeduplicateWhenBothMdAndMdaExist()
    {
        var provider = new InMemoryFileProvider()
            .AddFile("/content", "page.md", "md content")
            .AddFile("/content", "page.mda", "mda content");

        var files = provider.GetMarkdownFiles("/content");

        files.Count.ShouldBe(1);
        files[0].RelativePath.ShouldBe("page.md");
        files[0].Content.ShouldBe("md content");
    }

    [Fact]
    public void GetMarkdownFilesRecursiveShouldDeduplicateWhenBothMdAndMdaExist()
    {
        var provider = new InMemoryFileProvider()
            .AddFile("/content/guides", "intro.md", "md content")
            .AddFile("/content/guides", "intro.mda", "mda content")
            .AddFile("/content", "unique.mda", "unique mda");

        var files = provider.GetMarkdownFiles("/content", recursive: true);

        files.Count.ShouldBe(2);
        files.Select(f => f.RelativePath).ShouldContain("guides/intro.md");
        files.Select(f => f.RelativePath).ShouldContain("unique.mda");
    }

    [Fact]
    public void LoadCollectionShouldDeduplicateWhenBothMdAndMdaExist()
    {
        var provider = new InMemoryFileProvider()
            .AddFile("/content", "page.md", "---\ntitle: MD Wins\n---\nbody")
            .AddFile("/content", "page.mda", "---\ntitle: MDA Loses\n---\nbody")
            .AddFile("/content", "other.mda", "---\ntitle: Other\n---\nbody");

        var config = CreateConfig();
        var loader = new CollectionLoader(config, provider);

        var entries = loader.LoadCollection<MdaTestSchema>("content");

        entries.Count.ShouldBe(2);
        var pageEntry = entries.Single(e => e.Slug == "page");
        pageEntry.Data.Title.ShouldBe("MD Wins");
        entries.Select(e => e.Slug).ShouldContain("other");
    }

    private sealed class MdaTestSchema
    {
        public string Title { get; set; } = "";
    }
}
