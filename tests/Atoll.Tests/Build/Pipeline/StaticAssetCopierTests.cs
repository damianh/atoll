using Atoll.Build.Pipeline;

namespace Atoll.Build.Tests.Pipeline;

public sealed class StaticAssetCopierTests : IDisposable
{
    private readonly string _testDir;
    private readonly string _sourceDir;
    private readonly string _outputDir;

    public StaticAssetCopierTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "atoll-copier-test-" + Guid.NewGuid().ToString("N")[..8]);
        _sourceDir = Path.Combine(_testDir, "public");
        _outputDir = Path.Combine(_testDir, "dist");
        Directory.CreateDirectory(_sourceDir);
        Directory.CreateDirectory(_outputDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
        {
            Directory.Delete(_testDir, recursive: true);
        }
    }

    [Fact]
    public void CopyShouldCopyFilesToOutputDirectory()
    {
        File.WriteAllText(Path.Combine(_sourceDir, "robots.txt"), "User-agent: *");

        var copier = new StaticAssetCopier(_outputDir);
        var result = copier.Copy(_sourceDir);

        result.Count.ShouldBe(1);
        File.Exists(Path.Combine(_outputDir, "robots.txt")).ShouldBeTrue();
    }

    [Fact]
    public void CopyShouldPreserveDirectoryStructure()
    {
        var imagesDir = Path.Combine(_sourceDir, "images");
        Directory.CreateDirectory(imagesDir);
        File.WriteAllBytes(Path.Combine(imagesDir, "logo.png"), [0x89, 0x50, 0x4E, 0x47]);

        var copier = new StaticAssetCopier(_outputDir);
        var result = copier.Copy(_sourceDir);

        result.Count.ShouldBe(1);
        File.Exists(Path.Combine(_outputDir, "images", "logo.png")).ShouldBeTrue();
    }

    [Fact]
    public void CopyShouldCopyMultipleFilesAndDirectories()
    {
        File.WriteAllText(Path.Combine(_sourceDir, "favicon.ico"), "icon");
        File.WriteAllText(Path.Combine(_sourceDir, "robots.txt"), "User-agent: *");
        var cssDir = Path.Combine(_sourceDir, "css");
        Directory.CreateDirectory(cssDir);
        File.WriteAllText(Path.Combine(cssDir, "reset.css"), "body { margin: 0; }");

        var copier = new StaticAssetCopier(_outputDir);
        var result = copier.Copy(_sourceDir);

        result.Count.ShouldBe(3);
        File.Exists(Path.Combine(_outputDir, "favicon.ico")).ShouldBeTrue();
        File.Exists(Path.Combine(_outputDir, "robots.txt")).ShouldBeTrue();
        File.Exists(Path.Combine(_outputDir, "css", "reset.css")).ShouldBeTrue();
    }

    [Fact]
    public void CopyShouldReturnCorrectRelativePaths()
    {
        var imagesDir = Path.Combine(_sourceDir, "images");
        Directory.CreateDirectory(imagesDir);
        File.WriteAllText(Path.Combine(imagesDir, "logo.png"), "png");
        File.WriteAllText(Path.Combine(_sourceDir, "robots.txt"), "bot");

        var copier = new StaticAssetCopier(_outputDir);
        var result = copier.Copy(_sourceDir);

        var relativePaths = result.Files.Select(f => f.RelativePath).OrderBy(p => p).ToList();
        relativePaths.ShouldContain(Path.Combine("images", "logo.png"));
        relativePaths.ShouldContain("robots.txt");
    }

    [Fact]
    public void CopyShouldReturnFileSizes()
    {
        var content = "User-agent: *\nDisallow: /private";
        File.WriteAllText(Path.Combine(_sourceDir, "robots.txt"), content);

        var copier = new StaticAssetCopier(_outputDir);
        var result = copier.Copy(_sourceDir);

        result.Count.ShouldBe(1);
        result.Files[0].Size.ShouldBeGreaterThan(0);
        result.TotalSize.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void CopyShouldReturnEmptyResultForNonExistentSource()
    {
        var copier = new StaticAssetCopier(_outputDir);
        var result = copier.Copy(Path.Combine(_testDir, "nonexistent"));

        result.Count.ShouldBe(0);
    }

    [Fact]
    public void CopyShouldReturnEmptyResultForEmptySourceDirectory()
    {
        var copier = new StaticAssetCopier(_outputDir);
        var result = copier.Copy(_sourceDir);

        result.Count.ShouldBe(0);
    }

    [Fact]
    public void CopyShouldOverwriteExistingFiles()
    {
        File.WriteAllText(Path.Combine(_outputDir, "robots.txt"), "old content");
        File.WriteAllText(Path.Combine(_sourceDir, "robots.txt"), "new content");

        var copier = new StaticAssetCopier(_outputDir);
        copier.Copy(_sourceDir);

        File.ReadAllText(Path.Combine(_outputDir, "robots.txt")).ShouldBe("new content");
    }

    [Fact]
    public async Task CopyAsyncShouldCopyFilesToOutputDirectory()
    {
        File.WriteAllText(Path.Combine(_sourceDir, "robots.txt"), "User-agent: *");

        var copier = new StaticAssetCopier(_outputDir);
        var result = await copier.CopyAsync(_sourceDir);

        result.Count.ShouldBe(1);
        File.Exists(Path.Combine(_outputDir, "robots.txt")).ShouldBeTrue();
    }

    [Fact]
    public async Task CopyAsyncShouldPreserveDirectoryStructure()
    {
        var fontsDir = Path.Combine(_sourceDir, "fonts");
        Directory.CreateDirectory(fontsDir);
        File.WriteAllText(Path.Combine(fontsDir, "inter.woff2"), "font data");

        var copier = new StaticAssetCopier(_outputDir);
        var result = await copier.CopyAsync(_sourceDir);

        result.Count.ShouldBe(1);
        File.Exists(Path.Combine(_outputDir, "fonts", "inter.woff2")).ShouldBeTrue();
    }

    [Fact]
    public async Task CopyAsyncShouldReturnEmptyForNonExistentSource()
    {
        var copier = new StaticAssetCopier(_outputDir);
        var result = await copier.CopyAsync(Path.Combine(_testDir, "missing"));

        result.Count.ShouldBe(0);
    }

    [Fact]
    public void CopyShouldThrowOnNullSourceDirectory()
    {
        var copier = new StaticAssetCopier(_outputDir);
        Should.Throw<ArgumentNullException>(() => copier.Copy(null!));
    }

    [Fact]
    public void ConstructorShouldThrowOnNullOutputDirectory()
    {
        Should.Throw<ArgumentNullException>(() => new StaticAssetCopier(null!));
    }

    [Fact]
    public void CopyResultShouldComputeTotalSize()
    {
        var files = new List<CopiedFile>
        {
            new("a.txt", "/out/a.txt", 100),
            new("b.txt", "/out/b.txt", 200),
        };
        var result = new CopyResult(files);

        result.TotalSize.ShouldBe(300);
        result.Count.ShouldBe(2);
    }
}
