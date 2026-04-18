using Atoll.Build.Ssg;
using Atoll.Components;
using Atoll.Routing;
using Shouldly;

namespace Atoll.Build.Tests.Ssg;

/// <summary>
/// Tests for <see cref="InputHasher"/>.
/// </summary>
public sealed class InputHasherTests : IDisposable
{
    private readonly string _tempDir;

    public InputHasherTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "atoll-hasher-test-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }

    // ── HashAssembly ──────────────────────────────────────────────────────────

    [Fact]
    public void HashAssembly_ShouldReturnEmptyStringForMissingFile()
    {
        var result = InputHasher.HashAssembly(Path.Combine(_tempDir, "nonexistent.dll"));

        result.ShouldBe("");
    }

    [Fact]
    public void HashAssembly_ShouldReturnNonEmptyHashForExistingFile()
    {
        var dllPath = Path.Combine(_tempDir, "test.dll");
        File.WriteAllBytes(dllPath, [0x4d, 0x5a, 0x01, 0x02, 0x03]);

        var result = InputHasher.HashAssembly(dllPath);

        result.ShouldNotBeEmpty();
    }

    [Fact]
    public void HashAssembly_ShouldReturnSameHashForSameContent()
    {
        var dllPath = Path.Combine(_tempDir, "stable.dll");
        File.WriteAllBytes(dllPath, [0x10, 0x20, 0x30, 0x40]);

        var hash1 = InputHasher.HashAssembly(dllPath);
        var hash2 = InputHasher.HashAssembly(dllPath);

        hash1.ShouldBe(hash2);
    }

    [Fact]
    public void HashAssembly_ShouldReturnDifferentHashForDifferentContent()
    {
        var path1 = Path.Combine(_tempDir, "a.dll");
        var path2 = Path.Combine(_tempDir, "b.dll");
        File.WriteAllBytes(path1, [0x01, 0x02, 0x03]);
        File.WriteAllBytes(path2, [0x04, 0x05, 0x06]);

        var hash1 = InputHasher.HashAssembly(path1);
        var hash2 = InputHasher.HashAssembly(path2);

        hash1.ShouldNotBe(hash2);
    }

    // ── HashDirectory ─────────────────────────────────────────────────────────

    [Fact]
    public void HashDirectory_ShouldReturnEmptyStringForMissingDirectory()
    {
        var result = InputHasher.HashDirectory(Path.Combine(_tempDir, "nonexistent"));

        result.ShouldBe("");
    }

    [Fact]
    public void HashDirectory_ShouldReturnEmptyStringForEmptyDirectory()
    {
        var emptyDir = Path.Combine(_tempDir, "empty");
        Directory.CreateDirectory(emptyDir);

        var result = InputHasher.HashDirectory(emptyDir);

        result.ShouldBe("");
    }

    [Fact]
    public void HashDirectory_ShouldReturnNonEmptyHashWhenFilesExist()
    {
        var contentDir = Path.Combine(_tempDir, "content");
        Directory.CreateDirectory(contentDir);
        File.WriteAllText(Path.Combine(contentDir, "post.md"), "# Hello");

        var result = InputHasher.HashDirectory(contentDir);

        result.ShouldNotBeEmpty();
    }

    [Fact]
    public void HashDirectory_ShouldReturnSameHashForSameFiles()
    {
        var contentDir = Path.Combine(_tempDir, "stable-content");
        Directory.CreateDirectory(contentDir);
        File.WriteAllText(Path.Combine(contentDir, "post.md"), "# Hello");

        var hash1 = InputHasher.HashDirectory(contentDir);
        var hash2 = InputHasher.HashDirectory(contentDir);

        hash1.ShouldBe(hash2);
    }

    // ── HashLayoutChain ───────────────────────────────────────────────────────

    [Fact]
    public void HashLayoutChain_ShouldReturnNonEmptyHash()
    {
        var result = InputHasher.HashLayoutChain(typeof(SamplePage));

        result.ShouldNotBeEmpty();
    }

    [Fact]
    public void HashLayoutChain_ShouldReturnSameHashForSameType()
    {
        var hash1 = InputHasher.HashLayoutChain(typeof(SamplePage));
        var hash2 = InputHasher.HashLayoutChain(typeof(SamplePage));

        hash1.ShouldBe(hash2);
    }

    [Fact]
    public void HashLayoutChain_ShouldReturnDifferentHashForDifferentTypes()
    {
        var hash1 = InputHasher.HashLayoutChain(typeof(SamplePage));
        var hash2 = InputHasher.HashLayoutChain(typeof(AnotherPage));

        hash1.ShouldNotBe(hash2);
    }

    // ── IsDynamicRoute ────────────────────────────────────────────────────────

    [Fact]
    public void IsDynamicRoute_ShouldReturnFalseForStaticPage()
    {
        InputHasher.IsDynamicRoute(typeof(SamplePage)).ShouldBeFalse();
    }

    [Fact]
    public void IsDynamicRoute_ShouldReturnTrueForDynamicPage()
    {
        InputHasher.IsDynamicRoute(typeof(DynamicPage)).ShouldBeTrue();
    }

    // ── Test component stubs ──────────────────────────────────────────────────

    private sealed class SamplePage : IAtollComponent
    {
        public Task RenderAsync(RenderContext context) => Task.CompletedTask;
    }

    private sealed class AnotherPage : IAtollComponent
    {
        public Task RenderAsync(RenderContext context) => Task.CompletedTask;
    }

    private sealed class DynamicPage : IAtollComponent, IStaticPathsProvider
    {
        public Task RenderAsync(RenderContext context) => Task.CompletedTask;

        public Task<IReadOnlyList<StaticPath>> GetStaticPathsAsync() =>
            Task.FromResult<IReadOnlyList<StaticPath>>(Array.Empty<StaticPath>());
    }
}
