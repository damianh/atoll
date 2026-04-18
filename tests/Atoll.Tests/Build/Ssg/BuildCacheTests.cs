using Atoll.Build.Ssg;
using Shouldly;

namespace Atoll.Build.Tests.Ssg;

/// <summary>
/// Tests for <see cref="BuildCache"/>, <see cref="BuildCacheReader"/>,
/// and <see cref="BuildCacheWriter"/>.
/// </summary>
public sealed class BuildCacheTests : IDisposable
{
    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;
    private readonly string _tempDir;

    public BuildCacheTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "atoll-cache-test-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }

    // ── BuildCacheReader.GetCachePath ──────────────────────────────────────────

    [Fact]
    public void GetCachePath_ShouldReturnConsistentPath()
    {
        var path1 = BuildCacheReader.GetCachePath(_tempDir, @"C:\project\dist");
        var path2 = BuildCacheReader.GetCachePath(_tempDir, @"C:\project\dist");

        path1.ShouldBe(path2);
    }

    [Fact]
    public void GetCachePath_ShouldProduceDifferentPathsForDifferentOutputDirs()
    {
        var path1 = BuildCacheReader.GetCachePath(_tempDir, @"C:\project\dist");
        var path2 = BuildCacheReader.GetCachePath(_tempDir, @"C:\project\output");

        path1.ShouldNotBe(path2);
    }

    [Fact]
    public void GetCachePath_ShouldPlaceCacheInDotAtollSubdirectory()
    {
        var path = BuildCacheReader.GetCachePath(_tempDir, @"C:\project\dist");

        var expectedDir = Path.Combine(_tempDir, ".atoll");
        path.ShouldStartWith(expectedDir);
        path.ShouldEndWith(".json");
    }

    // ── BuildCacheReader.TryLoad ──────────────────────────────────────────────

    [Fact]
    public void TryLoad_ShouldReturnNullWhenFileDoesNotExist()
    {
        var missingPath = Path.Combine(_tempDir, "nonexistent.json");

        var result = BuildCacheReader.TryLoad(missingPath, "1.0.0");

        result.ShouldBeNull();
    }

    [Fact]
    public void TryLoad_ShouldReturnNullForCorruptJson()
    {
        var cachePath = Path.Combine(_tempDir, "corrupt.json");
        File.WriteAllText(cachePath, "{ not valid json }}}");

        var result = BuildCacheReader.TryLoad(cachePath, "1.0.0");

        result.ShouldBeNull();
    }

    [Fact]
    public async Task TryLoad_ShouldReturnNullWhenAtollVersionMismatches()
    {
        var cachePath = Path.Combine(_tempDir, "version-mismatch.json");
        var cache = CreateSampleCache("1.0.0");
        await BuildCacheWriter.WriteAsync(cache, cachePath, _ct);

        var result = BuildCacheReader.TryLoad(cachePath, "2.0.0");

        result.ShouldBeNull();
    }

    [Fact]
    public async Task TryLoad_ShouldReturnNullWhenCacheVersionMismatches()
    {
        var cachePath = Path.Combine(_tempDir, "schema-mismatch.json");
        var cache = CreateSampleCache("1.0.0");
        cache.CacheVersion = "999"; // unknown schema
        await BuildCacheWriter.WriteAsync(cache, cachePath, _ct);

        var result = BuildCacheReader.TryLoad(cachePath, "1.0.0");

        result.ShouldBeNull();
    }

    // ── Round-trip ────────────────────────────────────────────────────────────

    [Fact]
    public async Task RoundTrip_ShouldPreserveAllFields()
    {
        var cachePath = Path.Combine(_tempDir, "roundtrip.json");
        var original = new BuildCache
        {
            AtollVersion = "1.2.3",
            AssemblyHash = "abc123def456",
            ContentHash = "xyz789",
            CssAsset = new BuildCacheAsset
            {
                OutputPath = "_atoll/styles.abc.css",
                FileName = "styles.abc.css",
                Hash = "abc",
            },
            JsAsset = new BuildCacheAsset
            {
                OutputPath = "_atoll/scripts.def.js",
                FileName = "scripts.def.js",
                Hash = "def",
            },
            Pages = new Dictionary<string, BuildCachePage>
            {
                ["/"] = new BuildCachePage { OutputPath = "dist/index.html", IsDynamic = false },
                ["/blog/hello"] = new BuildCachePage { OutputPath = "dist/blog/hello/index.html", IsDynamic = true },
            },
        };

        await BuildCacheWriter.WriteAsync(original, cachePath, _ct);
        var loaded = BuildCacheReader.TryLoad(cachePath, "1.2.3");

        loaded.ShouldNotBeNull();
        loaded.CacheVersion.ShouldBe(BuildCache.CurrentCacheVersion);
        loaded.AtollVersion.ShouldBe("1.2.3");
        loaded.AssemblyHash.ShouldBe("abc123def456");
        loaded.ContentHash.ShouldBe("xyz789");

        loaded.CssAsset.ShouldNotBeNull();
        loaded.CssAsset!.OutputPath.ShouldBe("_atoll/styles.abc.css");
        loaded.CssAsset.FileName.ShouldBe("styles.abc.css");
        loaded.CssAsset.Hash.ShouldBe("abc");

        loaded.JsAsset.ShouldNotBeNull();
        loaded.JsAsset!.OutputPath.ShouldBe("_atoll/scripts.def.js");

        loaded.Pages.Count.ShouldBe(2);
        loaded.Pages["/"].IsDynamic.ShouldBeFalse();
        loaded.Pages["/blog/hello"].OutputPath.ShouldBe("dist/blog/hello/index.html");
        loaded.Pages["/blog/hello"].IsDynamic.ShouldBeTrue();
    }

    [Fact]
    public async Task RoundTrip_NullCssAndJsAssets_ShouldRoundTrip()
    {
        var cachePath = Path.Combine(_tempDir, "no-assets.json");
        var original = CreateSampleCache("1.0.0");
        original.CssAsset = null;
        original.JsAsset = null;

        await BuildCacheWriter.WriteAsync(original, cachePath, _ct);
        var loaded = BuildCacheReader.TryLoad(cachePath, "1.0.0");

        loaded.ShouldNotBeNull();
        loaded!.CssAsset.ShouldBeNull();
        loaded.JsAsset.ShouldBeNull();
    }

    [Fact]
    public async Task Write_ShouldCreateParentDirectoryIfMissing()
    {
        var nestedPath = Path.Combine(_tempDir, "sub", "dir", "cache.json");
        var cache = CreateSampleCache("1.0.0");

        await BuildCacheWriter.WriteAsync(cache, nestedPath, _ct);

        File.Exists(nestedPath).ShouldBeTrue();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static BuildCache CreateSampleCache(string atollVersion) =>
        new()
        {
            AtollVersion = atollVersion,
            AssemblyHash = "testhash",
            ContentHash = "contenthash",
            Pages = [],
        };
}
