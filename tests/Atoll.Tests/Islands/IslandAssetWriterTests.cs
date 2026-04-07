using Atoll.Islands;

namespace Atoll.Tests.Islands;

public sealed class IslandAssetWriterTests : IDisposable
{
    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;
    private readonly string _outputDir;

    public IslandAssetWriterTests()
    {
        var id = Guid.NewGuid().ToString("N")[..8];
        _outputDir = Path.Combine(Path.GetTempPath(), "atoll-island-writer-" + id);
    }

    public void Dispose()
    {
        if (Directory.Exists(_outputDir))
        {
            Directory.Delete(_outputDir, recursive: true);
        }
    }

    // ── Write tests ──

    [Fact]
    public async Task WriteAsyncShouldWriteEmbeddedResourceToOutputDirectory()
    {
        var assembly = typeof(IslandAssetWriter).Assembly;
        var descriptor = new IslandAssetDescriptor(
            "scripts/atoll-island.js",
            "Atoll.Islands.Assets.atoll-island.js",
            assembly);

        var writer = new IslandAssetWriter(_outputDir);
        var result = await writer.WriteAsync([descriptor], _ct);

        result.FileCount.ShouldBe(1);
        var expectedPath = Path.Combine(_outputDir, "scripts", "atoll-island.js");
        File.Exists(expectedPath).ShouldBeTrue();
    }

    [Fact]
    public async Task WriteAsyncShouldCreateParentDirectories()
    {
        var assembly = typeof(IslandAssetWriter).Assembly;
        var descriptor = new IslandAssetDescriptor(
            "deep/sub/dir/atoll-island.js",
            "Atoll.Islands.Assets.atoll-island.js",
            assembly);

        var writer = new IslandAssetWriter(_outputDir);
        await writer.WriteAsync([descriptor], _ct);

        var expectedPath = Path.Combine(_outputDir, "deep", "sub", "dir", "atoll-island.js");
        File.Exists(expectedPath).ShouldBeTrue();
    }

    [Fact]
    public async Task WriteAsyncShouldReturnCorrectFileCount()
    {
        var assembly = typeof(IslandAssetWriter).Assembly;
        var descriptors = new[]
        {
            new IslandAssetDescriptor(
                "scripts/atoll-island.js",
                "Atoll.Islands.Assets.atoll-island.js",
                assembly),
            new IslandAssetDescriptor(
                "scripts/atoll-directives.js",
                "Atoll.Islands.Assets.atoll-directives.js",
                assembly),
        };

        var writer = new IslandAssetWriter(_outputDir);
        var result = await writer.WriteAsync(descriptors, _ct);

        result.FileCount.ShouldBe(2);
        result.WrittenPaths.Count.ShouldBe(2);
    }

    [Fact]
    public async Task WriteAsyncShouldWriteNonEmptyContent()
    {
        var assembly = typeof(IslandAssetWriter).Assembly;
        var descriptor = new IslandAssetDescriptor(
            "scripts/atoll-island.js",
            "Atoll.Islands.Assets.atoll-island.js",
            assembly);

        var writer = new IslandAssetWriter(_outputDir);
        await writer.WriteAsync([descriptor], _ct);

        var content = await File.ReadAllTextAsync(Path.Combine(_outputDir, "scripts", "atoll-island.js"));
        content.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task WriteAsyncShouldThrowForMissingResource()
    {
        var assembly = typeof(IslandAssetWriter).Assembly;
        var descriptor = new IslandAssetDescriptor(
            "scripts/does-not-exist.js",
            "Atoll.Islands.Assets.does-not-exist.js",
            assembly);

        var writer = new IslandAssetWriter(_outputDir);

        await Should.ThrowAsync<InvalidOperationException>(
            async () => await writer.WriteAsync([descriptor], _ct));
    }

    [Fact]
    public async Task WriteAsyncShouldThrowOnNullAssets()
    {
        var writer = new IslandAssetWriter(_outputDir);

        await Should.ThrowAsync<ArgumentNullException>(
            async () => await writer.WriteAsync(null!, _ct));
    }

    // ── Constructor ──

    [Fact]
    public void ConstructorShouldThrowOnNullOutputDirectory()
    {
        Should.Throw<ArgumentNullException>(() => new IslandAssetWriter(null!));
    }
}
