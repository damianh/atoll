using System.Collections.ObjectModel;
using Atoll.Cli.Commands.Dev;
using Atoll.Middleware.Server.Hosting;
using Atoll.Routing.Matching;
using Microsoft.Extensions.Logging;

namespace Atoll.Integration.Tests;

/// <summary>
/// Integration tests for <see cref="DevDistWriter"/>.
/// </summary>
public sealed class DevDistWriterTests : IDisposable
{
    private readonly List<string> _tempDirs = new();

    private static readonly IReadOnlyDictionary<string, byte[]> EmptyAssets =
        new ReadOnlyDictionary<string, byte[]>(new Dictionary<string, byte[]>());

    private static ILoggerFactory CreateLoggerFactory() =>
        LoggerFactory.Create(b => b.SetMinimumLevel(LogLevel.Warning));

    private string CreateTempDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), "atoll-dist-writer-tests-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(dir);
        _tempDirs.Add(dir);
        return dir;
    }

    private static DevDistWriter CreateWriter(string outputDir, ILoggerFactory loggerFactory)
        => new(outputDir, loggerFactory.CreateLogger<DevDistWriter>());

    private static DevDistWriter CreateWriterWithPublicDir(string outputDir, string? publicDir, ILoggerFactory loggerFactory)
        => new(outputDir, publicDir, loggerFactory.CreateLogger<DevDistWriter>());

    private static DevServerState CreateStateWithAssets(
        IReadOnlyDictionary<string, byte[]> islandAssets,
        byte[]? searchIndexJson = null,
        string globalCss = "")
    {
        var matcher = new RouteMatcher([]);
        var options = new AtollOptions();
        return new DevServerState(matcher, options, null, null, globalCss, islandAssets, searchIndexJson, null, null);
    }

    // ── Public directory assets ─────────────────────────────────────────────────

    [Fact]
    public async Task ShouldCopyPublicDirectoryAssetsToOutputDirectory()
    {
        var outputDir = CreateTempDir();
        var publicDir = CreateTempDir();
        using var loggerFactory = CreateLoggerFactory();

        // Create public/ assets: favicon.ico and a nested file
        await File.WriteAllBytesAsync(Path.Combine(publicDir, "favicon.ico"), new byte[] { 0x00, 0x00, 0x01, 0x00 });
        var fontsDir = Path.Combine(publicDir, "fonts");
        Directory.CreateDirectory(fontsDir);
        await File.WriteAllTextAsync(Path.Combine(fontsDir, "font.woff2"), "font-data");

        var writer = CreateWriterWithPublicDir(outputDir, publicDir, loggerFactory);
        var state = CreateStateWithAssets(EmptyAssets);

        await writer.WriteAsync(state, CancellationToken.None);

        File.Exists(Path.Combine(outputDir, "favicon.ico")).ShouldBeTrue();
        File.Exists(Path.Combine(outputDir, "fonts", "font.woff2")).ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldNotFailWhenPublicDirectoryIsNull()
    {
        var outputDir = CreateTempDir();
        using var loggerFactory = CreateLoggerFactory();
        var writer = CreateWriter(outputDir, loggerFactory);
        var state = CreateStateWithAssets(EmptyAssets);

        // Should complete without errors when no public directory is configured
        var exception = await Record.ExceptionAsync(() => writer.WriteAsync(state, CancellationToken.None));
        exception.ShouldBeNull();
    }

    public void Dispose()
    {
        foreach (var dir in _tempDirs)
        {
            try { Directory.Delete(dir, recursive: true); }
            catch { /* best-effort */ }
        }
    }

    // ── Clean ───────────────────────────────────────────────────────────────────

    [Fact]
    public void ShouldCreateOutputDirectoryOnClean()
    {
        var outputDir = Path.Combine(Path.GetTempPath(), "atoll-test-clean-" + Guid.NewGuid().ToString("N")[..8]);
        _tempDirs.Add(outputDir);

        using var loggerFactory = CreateLoggerFactory();
        var writer = CreateWriter(outputDir, loggerFactory);

        writer.Clean();

        Directory.Exists(outputDir).ShouldBeTrue();
    }

    [Fact]
    public void ShouldDeleteExistingFilesOnClean()
    {
        var outputDir = CreateTempDir();
        var existingFile = Path.Combine(outputDir, "existing.html");
        File.WriteAllText(existingFile, "old content");

        using var loggerFactory = CreateLoggerFactory();
        var writer = CreateWriter(outputDir, loggerFactory);

        writer.Clean();

        File.Exists(existingFile).ShouldBeFalse();
        Directory.Exists(outputDir).ShouldBeTrue();
    }

    // ── Island assets ───────────────────────────────────────────────────────────

    [Fact]
    public async Task ShouldWriteIslandAssetsToOutputDirectory()
    {
        var outputDir = CreateTempDir();
        using var loggerFactory = CreateLoggerFactory();
        var writer = CreateWriter(outputDir, loggerFactory);

        var assetBytes = "console.log('island');"u8.ToArray();
        var assets = new ReadOnlyDictionary<string, byte[]>(new Dictionary<string, byte[]>
        {
            ["scripts/theme-toggle.js"] = assetBytes,
        });
        var state = CreateStateWithAssets(assets);

        await writer.WriteAsync(state, CancellationToken.None);

        var outputPath = Path.Combine(outputDir, "scripts", "theme-toggle.js");
        File.Exists(outputPath).ShouldBeTrue();
        File.ReadAllBytes(outputPath).ShouldBe(assetBytes);
    }

    [Fact]
    public async Task ShouldWriteMultipleIslandAssetsToOutputDirectory()
    {
        var outputDir = CreateTempDir();
        using var loggerFactory = CreateLoggerFactory();
        var writer = CreateWriter(outputDir, loggerFactory);

        var assets = new ReadOnlyDictionary<string, byte[]>(new Dictionary<string, byte[]>
        {
            ["scripts/a.js"] = "a"u8.ToArray(),
            ["scripts/b.js"] = "b"u8.ToArray(),
        });
        var state = CreateStateWithAssets(assets);

        await writer.WriteAsync(state, CancellationToken.None);

        File.Exists(Path.Combine(outputDir, "scripts", "a.js")).ShouldBeTrue();
        File.Exists(Path.Combine(outputDir, "scripts", "b.js")).ShouldBeTrue();
    }

    // ── Search index ────────────────────────────────────────────────────────────

    [Fact]
    public async Task ShouldWriteSearchIndexJsonWhenPresent()
    {
        var outputDir = CreateTempDir();
        using var loggerFactory = CreateLoggerFactory();
        var writer = CreateWriter(outputDir, loggerFactory);

        var searchIndexBytes = "{\"items\":[]}"u8.ToArray();
        var state = CreateStateWithAssets(EmptyAssets, searchIndexJson: searchIndexBytes);

        await writer.WriteAsync(state, CancellationToken.None);

        var outputPath = Path.Combine(outputDir, "search-index.json");
        File.Exists(outputPath).ShouldBeTrue();
        File.ReadAllBytes(outputPath).ShouldBe(searchIndexBytes);
    }

    [Fact]
    public async Task ShouldNotWriteSearchIndexJsonWhenAbsent()
    {
        var outputDir = CreateTempDir();
        using var loggerFactory = CreateLoggerFactory();
        var writer = CreateWriter(outputDir, loggerFactory);

        var state = CreateStateWithAssets(EmptyAssets, searchIndexJson: null);

        await writer.WriteAsync(state, CancellationToken.None);

        var outputPath = Path.Combine(outputDir, "search-index.json");
        File.Exists(outputPath).ShouldBeFalse();
    }

    // ── Core Atoll scripts ──────────────────────────────────────────────────────

    [Fact]
    public async Task ShouldWriteCoreAtollScripts()
    {
        var outputDir = CreateTempDir();
        using var loggerFactory = CreateLoggerFactory();
        var writer = CreateWriter(outputDir, loggerFactory);

        var state = CreateStateWithAssets(EmptyAssets);

        await writer.WriteAsync(state, CancellationToken.None);

        File.Exists(Path.Combine(outputDir, "_atoll", "island.js")).ShouldBeTrue();
        File.Exists(Path.Combine(outputDir, "_atoll", "directives.js")).ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldWriteNonEmptyCoreAtollScripts()
    {
        var outputDir = CreateTempDir();
        using var loggerFactory = CreateLoggerFactory();
        var writer = CreateWriter(outputDir, loggerFactory);

        var state = CreateStateWithAssets(EmptyAssets);

        await writer.WriteAsync(state, CancellationToken.None);

        var islandJsContent = File.ReadAllText(Path.Combine(outputDir, "_atoll", "island.js"));
        var directivesJsContent = File.ReadAllText(Path.Combine(outputDir, "_atoll", "directives.js"));

        islandJsContent.ShouldNotBeNullOrWhiteSpace();
        directivesJsContent.ShouldNotBeNullOrWhiteSpace();
    }

    // ── Stale file cleanup ──────────────────────────────────────────────────────

    [Fact]
    public async Task ShouldRemoveStaleIslandAssetsOnSubsequentWrite()
    {
        var outputDir = CreateTempDir();
        using var loggerFactory = CreateLoggerFactory();
        var writer = CreateWriter(outputDir, loggerFactory);

        // First write — two assets
        var firstAssets = new ReadOnlyDictionary<string, byte[]>(new Dictionary<string, byte[]>
        {
            ["scripts/keep.js"] = "keep"u8.ToArray(),
            ["scripts/stale.js"] = "stale"u8.ToArray(),
        });
        await writer.WriteAsync(CreateStateWithAssets(firstAssets), CancellationToken.None);

        // Both exist after first write
        File.Exists(Path.Combine(outputDir, "scripts", "keep.js")).ShouldBeTrue();
        File.Exists(Path.Combine(outputDir, "scripts", "stale.js")).ShouldBeTrue();

        // Second write — only one asset remains
        var secondAssets = new ReadOnlyDictionary<string, byte[]>(new Dictionary<string, byte[]>
        {
            ["scripts/keep.js"] = "keep"u8.ToArray(),
        });
        await writer.WriteAsync(CreateStateWithAssets(secondAssets), CancellationToken.None);

        // Kept file still exists, stale file removed
        File.Exists(Path.Combine(outputDir, "scripts", "keep.js")).ShouldBeTrue();
        File.Exists(Path.Combine(outputDir, "scripts", "stale.js")).ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldRemoveStaleSearchIndexOnSubsequentWrite()
    {
        var outputDir = CreateTempDir();
        using var loggerFactory = CreateLoggerFactory();
        var writer = CreateWriter(outputDir, loggerFactory);

        // First write — search index present
        var firstState = CreateStateWithAssets(EmptyAssets, searchIndexJson: "{}"u8.ToArray());
        await writer.WriteAsync(firstState, CancellationToken.None);

        File.Exists(Path.Combine(outputDir, "search-index.json")).ShouldBeTrue();

        // Second write — no search index
        var secondState = CreateStateWithAssets(EmptyAssets, searchIndexJson: null);
        await writer.WriteAsync(secondState, CancellationToken.None);

        File.Exists(Path.Combine(outputDir, "search-index.json")).ShouldBeFalse();
    }

    // ── Concurrent writes ───────────────────────────────────────────────────────

    [Fact]
    public async Task ShouldSerializeConcurrentWriteOperations()
    {
        var outputDir = CreateTempDir();
        using var loggerFactory = CreateLoggerFactory();
        var writer = CreateWriter(outputDir, loggerFactory);

        var assets = new ReadOnlyDictionary<string, byte[]>(new Dictionary<string, byte[]>
        {
            ["scripts/concurrent.js"] = "data"u8.ToArray(),
        });
        var state = CreateStateWithAssets(assets);

        // Fire two concurrent writes — neither should throw or corrupt state
        var t1 = writer.WriteAsync(state, CancellationToken.None);
        var t2 = writer.WriteAsync(state, CancellationToken.None);
        await Task.WhenAll(t1, t2);

        File.Exists(Path.Combine(outputDir, "scripts", "concurrent.js")).ShouldBeTrue();
    }
}
