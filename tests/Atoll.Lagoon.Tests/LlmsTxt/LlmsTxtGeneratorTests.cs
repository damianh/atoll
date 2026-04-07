using Atoll.Build.Content.Collections;
using Atoll.Lagoon.LlmsTxt;
using Shouldly;
using Xunit;

namespace Atoll.Lagoon.Tests.LlmsTxt;

public sealed class LlmsTxtGeneratorTests : IDisposable
{
    private readonly string _outputDir;

    public LlmsTxtGeneratorTests()
    {
        var id = Guid.NewGuid().ToString("N")[..8];
        _outputDir = Path.Combine(Path.GetTempPath(), "atoll-lagoon-llms-test-" + id);
    }

    public void Dispose()
    {
        if (Directory.Exists(_outputDir))
        {
            Directory.Delete(_outputDir, recursive: true);
        }
    }

    // ── ILlmsTxtConfiguration overload ──

    [Fact]
    public async Task ShouldGenerateLlmsTxtFromConfiguration()
    {
        var config = new StubLlmsTxtConfig(
            new LlmsTxtSiteInfo("My Docs", "Documentation for My Project."),
            [
                new LlmsTxtDocumentInput("Getting Started", "/docs/getting-started/"),
                new LlmsTxtDocumentInput("Components", "/docs/components/"),
            ]);
        var generator = new LlmsTxtGenerator(_outputDir);

        var result = await generator.GenerateAsync(CreateEmptyQuery(), config);

        result.DocumentCount.ShouldBe(2);
        File.Exists(result.LlmsTxtPath).ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldReturnCorrectOutputPath()
    {
        var config = new StubLlmsTxtConfig(
            new LlmsTxtSiteInfo("Docs", null),
            []);
        var generator = new LlmsTxtGenerator(_outputDir);

        var result = await generator.GenerateAsync(CreateEmptyQuery(), config);

        result.LlmsTxtPath.ShouldBe(Path.Combine(_outputDir, "llms.txt"));
    }

    [Fact]
    public async Task ShouldReturnElapsedTime()
    {
        var config = new StubLlmsTxtConfig(
            new LlmsTxtSiteInfo("Docs", null),
            [new LlmsTxtDocumentInput("Doc", "/doc/")]);
        var generator = new LlmsTxtGenerator(_outputDir);

        var result = await generator.GenerateAsync(CreateEmptyQuery(), config);

        result.Elapsed.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
    }

    [Fact]
    public async Task ShouldHandleEmptyDocumentList()
    {
        var config = new StubLlmsTxtConfig(
            new LlmsTxtSiteInfo("Empty Site", "No docs."),
            []);
        var generator = new LlmsTxtGenerator(_outputDir);

        var result = await generator.GenerateAsync(CreateEmptyQuery(), config);

        result.DocumentCount.ShouldBe(0);
        File.Exists(result.LlmsTxtPath).ShouldBeTrue();
    }

    // ── LlmsTxtSiteInfo + documents overload ──

    [Fact]
    public async Task ShouldGenerateFromExplicitDocumentList()
    {
        var siteInfo = new LlmsTxtSiteInfo("My Project", "A great project.");
        var documents = new[]
        {
            new LlmsTxtDocumentInput("Guide", "/guide/"),
            new LlmsTxtDocumentInput("API", "/api/"),
        };
        var generator = new LlmsTxtGenerator(_outputDir);

        var result = await generator.GenerateAsync(siteInfo, documents);

        result.DocumentCount.ShouldBe(2);
        File.Exists(result.LlmsTxtPath).ShouldBeTrue();
    }

    // ── llms.txt content ──

    [Fact]
    public async Task LlmsTxtShouldContainH1Title()
    {
        var siteInfo = new LlmsTxtSiteInfo("Duende IdentityServer", null);
        var generator = new LlmsTxtGenerator(_outputDir);

        await generator.GenerateAsync(siteInfo, Array.Empty<LlmsTxtDocumentInput>());

        var content = await File.ReadAllTextAsync(Path.Combine(_outputDir, "llms.txt"));
        content.ShouldStartWith("# Duende IdentityServer");
    }

    [Fact]
    public async Task LlmsTxtShouldContainBlockquoteDescription()
    {
        var siteInfo = new LlmsTxtSiteInfo("My Docs", "Developer documentation for My Project.");
        var generator = new LlmsTxtGenerator(_outputDir);

        await generator.GenerateAsync(siteInfo, Array.Empty<LlmsTxtDocumentInput>());

        var content = await File.ReadAllTextAsync(Path.Combine(_outputDir, "llms.txt"));
        content.ShouldContain("> Developer documentation for My Project.");
    }

    [Fact]
    public async Task LlmsTxtShouldOmitBlockquoteWhenDescriptionIsNull()
    {
        var siteInfo = new LlmsTxtSiteInfo("My Docs", null);
        var generator = new LlmsTxtGenerator(_outputDir);

        await generator.GenerateAsync(siteInfo, Array.Empty<LlmsTxtDocumentInput>());

        var content = await File.ReadAllTextAsync(Path.Combine(_outputDir, "llms.txt"));
        content.ShouldNotContain(">");
    }

    [Fact]
    public async Task LlmsTxtShouldContainMarkdownLinks()
    {
        var siteInfo = new LlmsTxtSiteInfo("Docs", null);
        var documents = new[]
        {
            new LlmsTxtDocumentInput("Getting Started", "/docs/getting-started/"),
        };
        var generator = new LlmsTxtGenerator(_outputDir);

        await generator.GenerateAsync(siteInfo, documents);

        var content = await File.ReadAllTextAsync(Path.Combine(_outputDir, "llms.txt"));
        content.ShouldContain("- [Getting Started](/docs/getting-started/)");
    }

    [Fact]
    public async Task LlmsTxtShouldIncludeDescriptionAfterLink()
    {
        var siteInfo = new LlmsTxtSiteInfo("Docs", null);
        var documents = new[]
        {
            new LlmsTxtDocumentInput("API Reference", "/api/")
            {
                Description = "Complete API documentation",
            },
        };
        var generator = new LlmsTxtGenerator(_outputDir);

        await generator.GenerateAsync(siteInfo, documents);

        var content = await File.ReadAllTextAsync(Path.Combine(_outputDir, "llms.txt"));
        content.ShouldContain("- [API Reference](/api/): Complete API documentation");
    }

    [Fact]
    public async Task LlmsTxtShouldGroupDocumentsBySection()
    {
        var siteInfo = new LlmsTxtSiteInfo("Docs", null);
        var documents = new[]
        {
            new LlmsTxtDocumentInput("Overview", "/overview/") { Section = "Getting Started" },
            new LlmsTxtDocumentInput("Installation", "/install/") { Section = "Getting Started" },
            new LlmsTxtDocumentInput("Auth", "/auth/") { Section = "Advanced" },
        };
        var generator = new LlmsTxtGenerator(_outputDir);

        await generator.GenerateAsync(siteInfo, documents);

        var content = await File.ReadAllTextAsync(Path.Combine(_outputDir, "llms.txt"));
        content.ShouldContain("## Getting Started");
        content.ShouldContain("## Advanced");
        // Section order should be preserved (Getting Started before Advanced)
        content.IndexOf("## Getting Started").ShouldBeLessThan(content.IndexOf("## Advanced"));
    }

    [Fact]
    public async Task LlmsTxtShouldUseDefaultSectionForUngroupedDocuments()
    {
        var siteInfo = new LlmsTxtSiteInfo("Docs", null);
        var documents = new[]
        {
            new LlmsTxtDocumentInput("Intro", "/intro/"),
        };
        var generator = new LlmsTxtGenerator(_outputDir);

        await generator.GenerateAsync(siteInfo, documents);

        var content = await File.ReadAllTextAsync(Path.Combine(_outputDir, "llms.txt"));
        content.ShouldContain("## Documentation");
    }

    // ── llms-full.txt ──

    [Fact]
    public async Task ShouldGenerateLlmsFullTxtWhenMarkdownBodyPresent()
    {
        var siteInfo = new LlmsTxtSiteInfo("Docs", null);
        var documents = new[]
        {
            new LlmsTxtDocumentInput("Guide", "/guide/")
            {
                MarkdownBody = "This is the guide content.\n\n## Section\n\nMore content here.",
            },
        };
        var generator = new LlmsTxtGenerator(_outputDir);

        var result = await generator.GenerateAsync(siteInfo, documents);

        result.LlmsFullTxtPath.ShouldNotBeNull();
        File.Exists(result.LlmsFullTxtPath).ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldNotGenerateLlmsFullTxtWhenNoMarkdownBody()
    {
        var siteInfo = new LlmsTxtSiteInfo("Docs", null);
        var documents = new[]
        {
            new LlmsTxtDocumentInput("Guide", "/guide/"),
        };
        var generator = new LlmsTxtGenerator(_outputDir);

        var result = await generator.GenerateAsync(siteInfo, documents);

        result.LlmsFullTxtPath.ShouldBeNull();
    }

    [Fact]
    public async Task LlmsFullTxtShouldContainFullMarkdownBody()
    {
        var siteInfo = new LlmsTxtSiteInfo("Docs", "Full content export.");
        var documents = new[]
        {
            new LlmsTxtDocumentInput("Guide", "/guide/")
            {
                MarkdownBody = "This is the full guide content.",
            },
        };
        var generator = new LlmsTxtGenerator(_outputDir);

        var result = await generator.GenerateAsync(siteInfo, documents);

        var content = await File.ReadAllTextAsync(result.LlmsFullTxtPath!);
        content.ShouldContain("# Docs");
        content.ShouldContain("> Full content export.");
        content.ShouldContain("## Guide");
        content.ShouldContain("This is the full guide content.");
    }

    [Fact]
    public async Task LlmsFullTxtShouldIncludeDescriptionBeforeBody()
    {
        var siteInfo = new LlmsTxtSiteInfo("Docs", null);
        var documents = new[]
        {
            new LlmsTxtDocumentInput("API", "/api/")
            {
                Description = "API reference docs.",
                MarkdownBody = "## Endpoints\n\nGET /users",
            },
        };
        var generator = new LlmsTxtGenerator(_outputDir);

        var result = await generator.GenerateAsync(siteInfo, documents);

        var content = await File.ReadAllTextAsync(result.LlmsFullTxtPath!);
        var descriptionIdx = content.IndexOf("API reference docs.");
        var bodyIdx = content.IndexOf("## Endpoints");
        descriptionIdx.ShouldBeLessThan(bodyIdx);
    }

    [Fact]
    public async Task LlmsFullTxtPathShouldBeCorrect()
    {
        var siteInfo = new LlmsTxtSiteInfo("Docs", null);
        var documents = new[]
        {
            new LlmsTxtDocumentInput("Doc", "/doc/") { MarkdownBody = "Content." },
        };
        var generator = new LlmsTxtGenerator(_outputDir);

        var result = await generator.GenerateAsync(siteInfo, documents);

        result.LlmsFullTxtPath.ShouldBe(Path.Combine(_outputDir, "llms-full.txt"));
    }

    // ── BuildLlmsTxt internals ──

    [Fact]
    public void BuildLlmsTxtShouldPreserveSectionInsertionOrder()
    {
        var siteInfo = new LlmsTxtSiteInfo("Docs", null);
        var documents = new LlmsTxtDocumentInput[]
        {
            new("A", "/a/") { Section = "Zebra" },
            new("B", "/b/") { Section = "Alpha" },
            new("C", "/c/") { Section = "Zebra" },
        };

        var content = LlmsTxtGenerator.BuildLlmsTxt(siteInfo, documents);

        // Zebra should come before Alpha because it appeared first
        content.IndexOf("## Zebra").ShouldBeLessThan(content.IndexOf("## Alpha"));
    }

    [Fact]
    public void BuildLlmsTxtShouldHandleMixedSectionedAndUnsectioned()
    {
        var siteInfo = new LlmsTxtSiteInfo("Docs", null);
        var documents = new LlmsTxtDocumentInput[]
        {
            new("Ungrouped", "/ungrouped/"),
            new("Grouped", "/grouped/") { Section = "Guides" },
        };

        var content = LlmsTxtGenerator.BuildLlmsTxt(siteInfo, documents);

        content.ShouldContain("## Documentation");
        content.ShouldContain("## Guides");
        content.ShouldContain("- [Ungrouped](/ungrouped/)");
        content.ShouldContain("- [Grouped](/grouped/)");
    }

    // ── GroupBySection ──

    [Fact]
    public void GroupBySectionShouldReturnEmptyForNoDocuments()
    {
        var result = LlmsTxtGenerator.GroupBySection([]);

        result.ShouldBeEmpty();
    }

    [Fact]
    public void GroupBySectionShouldGroupAllUnsectionedUnderDefault()
    {
        var documents = new LlmsTxtDocumentInput[]
        {
            new("A", "/a/"),
            new("B", "/b/"),
        };

        var result = LlmsTxtGenerator.GroupBySection(documents);

        result.Count.ShouldBe(1);
        result[0].Section.ShouldBe("Documentation");
        result[0].Documents.Count.ShouldBe(2);
    }

    [Fact]
    public void GroupBySectionShouldPreserveInsertionOrder()
    {
        var documents = new LlmsTxtDocumentInput[]
        {
            new("A", "/a/") { Section = "Second" },
            new("B", "/b/") { Section = "First" },
            new("C", "/c/") { Section = "Second" },
        };

        var result = LlmsTxtGenerator.GroupBySection(documents);

        result.Count.ShouldBe(2);
        result[0].Section.ShouldBe("Second");
        result[0].Documents.Count.ShouldBe(2);
        result[1].Section.ShouldBe("First");
        result[1].Documents.Count.ShouldBe(1);
    }

    // ── Helpers ──

    private static CollectionQuery CreateEmptyQuery()
    {
        var config = new CollectionConfig("content");
        var provider = new InMemoryFileProvider();
        var loader = new CollectionLoader(config, provider);
        return new CollectionQuery(loader);
    }

    private sealed class StubLlmsTxtConfig : ILlmsTxtConfiguration
    {
        private readonly LlmsTxtSiteInfo _siteInfo;
        private readonly IReadOnlyList<LlmsTxtDocumentInput> _documents;

        public StubLlmsTxtConfig(LlmsTxtSiteInfo siteInfo, IReadOnlyList<LlmsTxtDocumentInput> documents)
        {
            _siteInfo = siteInfo;
            _documents = documents;
        }

        public LlmsTxtSiteInfo GetSiteInfo() => _siteInfo;

        public IEnumerable<LlmsTxtDocumentInput> GetDocuments(CollectionQuery query) => _documents;
    }
}
