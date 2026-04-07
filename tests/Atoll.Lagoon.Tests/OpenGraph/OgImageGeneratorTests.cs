using Atoll.Build.Content.Collections;
using Atoll.Lagoon.Configuration;
using Atoll.Lagoon.OpenGraph;

namespace Atoll.Lagoon.Tests.OpenGraph;

public sealed class OgImageGeneratorTests : IDisposable
{
    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;
    private readonly string _tempDir;

    public OgImageGeneratorTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "atoll-og-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }

    private sealed class TestOgConfig : IOgImageConfiguration
    {
        public OpenGraphConfig GetOpenGraphConfig() => new()
        {
            Categories = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["docs"] = "Docs",
            },
        };

        public IEnumerable<OgImageInput> GetDocuments(CollectionQuery query)
        {
            yield return new OgImageInput("Getting Started", "/docs/getting-started", "How to get started.", null);
            yield return new OgImageInput("Advanced Usage", "/docs/advanced", "Advanced patterns and tips.", null);
            yield return new OgImageInput("API Reference", "/docs/api-reference", null, null);
        }
    }

    [Fact]
    public async Task ShouldGeneratePngFilesAtExpectedPaths()
    {
        var generator = new OgImageGenerator(_tempDir, _tempDir);
        var config = new TestOgConfig();
        var query = CreateEmptyQuery();

        var result = await generator.GenerateAsync(query, config, _ct);

        result.ImageCount.ShouldBe(3);
        File.Exists(Path.Combine(_tempDir, "og", "docs", "getting-started.png")).ShouldBeTrue();
        File.Exists(Path.Combine(_tempDir, "og", "docs", "advanced.png")).ShouldBeTrue();
        File.Exists(Path.Combine(_tempDir, "og", "docs", "api-reference.png")).ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldWriteValidPngFiles()
    {
        var generator = new OgImageGenerator(_tempDir, _tempDir);
        var config = new TestOgConfig();
        var query = CreateEmptyQuery();

        await generator.GenerateAsync(query, config, _ct);

        var pngPath = Path.Combine(_tempDir, "og", "docs", "getting-started.png");
        var bytes = await File.ReadAllBytesAsync(pngPath);

        // Verify PNG magic bytes: 89 50 4E 47 0D 0A 1A 0A
        bytes[0].ShouldBe((byte)0x89);
        bytes[1].ShouldBe((byte)0x50);
        bytes[2].ShouldBe((byte)0x4E);
        bytes[3].ShouldBe((byte)0x47);
    }

    [Fact]
    public async Task ShouldReportCorrectImageCountInResult()
    {
        var generator = new OgImageGenerator(_tempDir, _tempDir);
        var config = new TestOgConfig();
        var query = CreateEmptyQuery();

        var result = await generator.GenerateAsync(query, config, _ct);

        result.ImageCount.ShouldBe(3);
        result.Elapsed.ShouldBeGreaterThan(TimeSpan.Zero);
        result.OutputDirectory.ShouldBe(Path.Combine(_tempDir, "og"));
    }

    [Fact]
    public async Task ShouldCreateOutputDirectoryIfNotExists()
    {
        var outputDir = Path.Combine(_tempDir, "new-output");
        var generator = new OgImageGenerator(outputDir, _tempDir);
        var config = new TestOgConfig();
        var query = CreateEmptyQuery();

        await generator.GenerateAsync(query, config, _ct);

        Directory.Exists(Path.Combine(outputDir, "og")).ShouldBeTrue();
    }

    private static CollectionQuery CreateEmptyQuery()
    {
        // Create a minimal CollectionQuery with no actual content
        var collectionConfig = new CollectionConfig(Path.GetTempPath());
        var fileProvider = new PhysicalFileProvider();
        var loader = new CollectionLoader(collectionConfig, fileProvider);
        return new CollectionQuery(loader);
    }
}

