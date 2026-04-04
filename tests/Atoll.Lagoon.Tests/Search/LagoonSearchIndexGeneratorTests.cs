using Atoll.Build.Content.Collections;
using Atoll.Lagoon.Search;
using Shouldly;
using Xunit;

namespace Atoll.Lagoon.Tests.Search;

public sealed class LagoonSearchIndexGeneratorTests : IDisposable
{
    private readonly string _outputDir;

    public LagoonSearchIndexGeneratorTests()
    {
        var id = Guid.NewGuid().ToString("N")[..8];
        _outputDir = Path.Combine(Path.GetTempPath(), "atoll-lagoon-gen-test-" + id);
    }

    public void Dispose()
    {
        if (Directory.Exists(_outputDir))
        {
            Directory.Delete(_outputDir, recursive: true);
        }
    }

    private static string SearchIndexPath(string dir) => Path.Combine(dir, "search-index.json");

    // ── ISearchIndexConfiguration overload ──

    [Fact]
    public async Task ShouldGenerateSearchIndexFromConfiguration()
    {
        var config = new StubSearchConfig(
        [
            new SearchDocumentInput("Getting Started", "/docs/getting-started/"),
            new SearchDocumentInput("Components", "/docs/components/"),
            new SearchDocumentInput("Collections", "/docs/collections/"),
        ]);
        var query = CreateEmptyQuery();
        var generator = new LagoonSearchIndexGenerator(_outputDir);

        var result = await generator.GenerateAsync(query, config);

        result.EntryCount.ShouldBe(3);
        File.Exists(SearchIndexPath(_outputDir)).ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldReturnCorrectEntryCountFromConfiguration()
    {
        var config = new StubSearchConfig(
        [
            new SearchDocumentInput("Doc 1", "/docs/doc-1/"),
            new SearchDocumentInput("Doc 2", "/docs/doc-2/"),
        ]);
        var generator = new LagoonSearchIndexGenerator(_outputDir);

        var result = await generator.GenerateAsync(CreateEmptyQuery(), config);

        result.EntryCount.ShouldBe(2);
    }

    [Fact]
    public async Task ShouldHandleEmptyConfigurationCollection()
    {
        var config = new StubSearchConfig([]);
        var generator = new LagoonSearchIndexGenerator(_outputDir);

        var result = await generator.GenerateAsync(CreateEmptyQuery(), config);

        result.EntryCount.ShouldBe(0);
        File.Exists(SearchIndexPath(_outputDir)).ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldReturnOutputPathInResult()
    {
        var config = new StubSearchConfig([]);
        var generator = new LagoonSearchIndexGenerator(_outputDir);

        var result = await generator.GenerateAsync(CreateEmptyQuery(), config);

        result.OutputPath.ShouldBe(SearchIndexPath(_outputDir));
    }

    [Fact]
    public async Task ShouldReturnElapsedTimeInResult()
    {
        var config = new StubSearchConfig([new SearchDocumentInput("Doc", "/docs/doc/")]);
        var generator = new LagoonSearchIndexGenerator(_outputDir);

        var result = await generator.GenerateAsync(CreateEmptyQuery(), config);

        result.Elapsed.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
    }

    // ── IEnumerable<SearchDocumentInput> overload ──

    [Fact]
    public async Task ShouldGenerateSearchIndexFromDocuments()
    {
        var documents = new[]
        {
            new SearchDocumentInput("Doc A", "/a/") { HtmlBody = "<p>Hello world</p>" },
            new SearchDocumentInput("Doc B", "/b/") { HtmlBody = "<p>Another doc</p>" },
        };
        var generator = new LagoonSearchIndexGenerator(_outputDir);

        var result = await generator.GenerateAsync(documents);

        result.EntryCount.ShouldBe(2);
        File.Exists(SearchIndexPath(_outputDir)).ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldStripHtmlFromRenderedBodyInOutput()
    {
        var documents = new[]
        {
            new SearchDocumentInput("Doc", "/doc/") { HtmlBody = "<h2>Intro</h2><p>Hello <strong>world</strong></p>" },
        };
        var generator = new LagoonSearchIndexGenerator(_outputDir);

        await generator.GenerateAsync(documents);

        var json = await File.ReadAllTextAsync(SearchIndexPath(_outputDir));
        json.ShouldNotContain("<p>");
        json.ShouldNotContain("<strong>");
        json.ShouldContain("Hello");
        json.ShouldContain("world");
    }

    [Fact]
    public async Task ShouldHandleEmptyDocumentList()
    {
        var generator = new LagoonSearchIndexGenerator(_outputDir);

        var result = await generator.GenerateAsync(Array.Empty<SearchDocumentInput>());

        result.EntryCount.ShouldBe(0);
        File.Exists(SearchIndexPath(_outputDir)).ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldUseCustomSelectorFieldsInOutput()
    {
        var documents = new[]
        {
            new SearchDocumentInput("My Title", "/my-path/")
            {
                Description = "My description",
                Section = "My Section",
            },
        };
        var generator = new LagoonSearchIndexGenerator(_outputDir);

        await generator.GenerateAsync(documents);

        var json = await File.ReadAllTextAsync(SearchIndexPath(_outputDir));
        json.ShouldContain("My Title");
        json.ShouldContain("/my-path/");
        json.ShouldContain("My description");
        json.ShouldContain("My Section");
    }

    [Fact]
    public async Task ShouldWriteValidJsonWithEntriesArray()
    {
        var documents = new[]
        {
            new SearchDocumentInput("Doc", "/doc/"),
        };
        var generator = new LagoonSearchIndexGenerator(_outputDir);

        await generator.GenerateAsync(documents);

        var json = await File.ReadAllTextAsync(SearchIndexPath(_outputDir));
        json.ShouldContain("\"entries\"");
        json.ShouldContain("\"generatedAt\"");
    }

    // ── Helpers ──

    private static CollectionQuery CreateEmptyQuery()
    {
        var config = new CollectionConfig("content");
        var provider = new InMemoryFileProvider();
        var loader = new CollectionLoader(config, provider);
        return new CollectionQuery(loader);
    }

    private sealed class StubSearchConfig : ISearchIndexConfiguration
    {
        private readonly IReadOnlyList<SearchDocumentInput> _documents;

        public StubSearchConfig(IReadOnlyList<SearchDocumentInput> documents)
        {
            _documents = documents;
        }

        public IEnumerable<SearchDocumentInput> GetDocuments(CollectionQuery query) => _documents;
    }
}
