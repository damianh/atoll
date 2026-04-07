using Atoll.DrawIo.Content;

namespace Atoll.DrawIo.Tests.Content;

public sealed class DrawioCollectionLoaderTests
{
    private static readonly string FixturesDir =
        Path.Combine(AppContext.BaseDirectory, "Fixtures");

    private readonly DrawioCollectionLoader _loader = new();

    [Fact]
    public void LoadCollectionFromFixturesDirShouldReturnEntries()
    {
        var entries = _loader.LoadCollection(FixturesDir, "diagrams");

        entries.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void LoadCollectionShouldOnlyReturnDrawioFiles()
    {
        var entries = _loader.LoadCollection(FixturesDir, "diagrams");

        foreach (var entry in entries)
        {
            (entry.Id.EndsWith(".drawio", StringComparison.OrdinalIgnoreCase) ||
             entry.Id.EndsWith(".dio", StringComparison.OrdinalIgnoreCase))
                .ShouldBeTrue($"Entry '{entry.Id}' is not a .drawio/.dio file");
        }
    }

    [Fact]
    public void LoadCollectionShouldPopulateSlug()
    {
        var entries = _loader.LoadCollection(FixturesDir, "diagrams");

        foreach (var entry in entries)
        {
            entry.Slug.ShouldNotBeNullOrEmpty();
            entry.Slug.ShouldNotContain(".");
        }
    }

    [Fact]
    public void LoadCollectionShouldPopulateCollectionName()
    {
        var entries = _loader.LoadCollection(FixturesDir, "my-collection");

        foreach (var entry in entries)
        {
            entry.Collection.ShouldBe("my-collection");
        }
    }

    [Fact]
    public void LoadCollectionShouldPopulateBody()
    {
        var entries = _loader.LoadCollection(FixturesDir, "diagrams");

        foreach (var entry in entries)
        {
            entry.Body.ShouldNotBeNullOrEmpty();
        }
    }

    [Fact]
    public void LoadCollectionShouldPopulatePageCount()
    {
        var entries = _loader.LoadCollection(FixturesDir, "diagrams");
        var simple = entries.FirstOrDefault(e => e.Slug == "simple");

        simple.ShouldNotBeNull();
        simple!.Data.PageCount.ShouldBe(1);
    }

    [Fact]
    public void LoadCollectionShouldPopulateMultiPageCount()
    {
        var entries = _loader.LoadCollection(FixturesDir, "diagrams");
        var multiPage = entries.FirstOrDefault(e => e.Slug == "multi-page");

        multiPage.ShouldNotBeNull();
        multiPage!.Data.PageCount.ShouldBe(3);
    }

    [Fact]
    public void LoadCollectionShouldPopulatePageNames()
    {
        var entries = _loader.LoadCollection(FixturesDir, "diagrams");
        var multiPage = entries.FirstOrDefault(e => e.Slug == "multi-page");

        multiPage.ShouldNotBeNull();
        var pageNames = multiPage!.Data.Pages.Select(p => p.Name).ToList();
        pageNames.ShouldContain("Overview");
        pageNames.ShouldContain("Details");
        pageNames.ShouldContain("Flow");
    }

    [Fact]
    public void LoadCollectionShouldPopulateLayerNames()
    {
        var entries = _loader.LoadCollection(FixturesDir, "diagrams");
        var layers = entries.FirstOrDefault(e => e.Slug == "layers");

        layers.ShouldNotBeNull();
        var layerNames = layers!.Data.Pages[0].Layers.ToList();
        layerNames.ShouldContain("Background");
        layerNames.ShouldContain("Content");
        layerNames.ShouldContain("Annotations");
    }

    [Fact]
    public void LoadCollectionFromMissingDirectoryShouldReturnEmpty()
    {
        var entries = _loader.LoadCollection(
            Path.Combine(AppContext.BaseDirectory, "NonExistentDir"),
            "diagrams");

        entries.ShouldBeEmpty();
    }

    [Fact]
    public void LoadEntryShouldReturnCorrectEntry()
    {
        var entry = _loader.LoadEntry(FixturesDir, "diagrams", "simple");

        entry.ShouldNotBeNull();
        entry!.Slug.ShouldBe("simple");
        entry.Data.PageCount.ShouldBe(1);
    }

    [Fact]
    public void LoadEntryForMissingSlugShouldReturnNull()
    {
        var entry = _loader.LoadEntry(FixturesDir, "diagrams", "does-not-exist");

        entry.ShouldBeNull();
    }

    [Fact]
    public void LoadCollectionShouldPopulateTitleFromFileName()
    {
        var entries = _loader.LoadCollection(FixturesDir, "diagrams");
        var simple = entries.FirstOrDefault(e => e.Slug == "simple");

        simple.ShouldNotBeNull();
        simple!.Data.Title.ShouldBe("simple");
    }
}
