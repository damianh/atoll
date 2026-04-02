using Atoll.Build.Ssg;
using Shouldly;
using Xunit;

namespace Atoll.Build.Tests.Ssg;

public sealed class OutputWriterTests : IDisposable
{
    private readonly string _testOutputDir;
    private readonly OutputWriter _writer;

    public OutputWriterTests()
    {
        _testOutputDir = Path.Combine(Path.GetTempPath(), "atoll-test-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(_testOutputDir);
        _writer = new OutputWriter(_testOutputDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testOutputDir))
        {
            Directory.Delete(_testOutputDir, recursive: true);
        }
    }

    [Fact]
    public async Task ShouldWriteRootPageAsIndexHtml()
    {
        var path = await _writer.WritePageAsync("/", "<h1>Home</h1>");

        File.Exists(path).ShouldBeTrue();
        var content = await File.ReadAllTextAsync(path);
        content.ShouldBe("<h1>Home</h1>");
        path.ShouldEndWith("index.html");
    }

    [Fact]
    public async Task ShouldWritePageWithCleanUrl()
    {
        var path = await _writer.WritePageAsync("/about", "<h1>About</h1>");

        File.Exists(path).ShouldBeTrue();
        path.ShouldContain("about");
        path.ShouldEndWith("index.html");
        var content = await File.ReadAllTextAsync(path);
        content.ShouldBe("<h1>About</h1>");
    }

    [Fact]
    public async Task ShouldWriteNestedPage()
    {
        var path = await _writer.WritePageAsync("/blog/my-post", "<h1>Blog Post</h1>");

        File.Exists(path).ShouldBeTrue();
        path.ShouldContain("blog");
        path.ShouldContain("my-post");
        path.ShouldEndWith("index.html");
    }

    [Fact]
    public async Task ShouldWriteDeeplyNestedPage()
    {
        var path = await _writer.WritePageAsync("/docs/guides/advanced/setup", "<h1>Setup</h1>");

        File.Exists(path).ShouldBeTrue();
        path.ShouldContain("docs");
        path.ShouldContain("guides");
        path.ShouldContain("advanced");
        path.ShouldContain("setup");
    }

    [Fact]
    public async Task ShouldWriteFileWithRelativePath()
    {
        var path = await _writer.WriteFileAsync("assets/style.css", "body { margin: 0; }");

        File.Exists(path).ShouldBeTrue();
        var content = await File.ReadAllTextAsync(path);
        content.ShouldBe("body { margin: 0; }");
    }

    [Fact]
    public async Task ShouldWriteBinaryFile()
    {
        var content = new byte[] { 0x89, 0x50, 0x4E, 0x47 }; // PNG header
        var path = await _writer.WriteBinaryFileAsync("images/logo.png", content);

        File.Exists(path).ShouldBeTrue();
        var written = await File.ReadAllBytesAsync(path);
        written.ShouldBe(content);
    }

    [Fact]
    public void ShouldCleanOutputDirectory()
    {
        // Create some files first
        File.WriteAllText(Path.Combine(_testOutputDir, "old-file.html"), "old");
        Directory.CreateDirectory(Path.Combine(_testOutputDir, "old-dir"));
        File.WriteAllText(Path.Combine(_testOutputDir, "old-dir", "old-file2.html"), "old2");

        _writer.Clean();

        Directory.Exists(_testOutputDir).ShouldBeTrue();
        Directory.GetFiles(_testOutputDir).Length.ShouldBe(0);
        Directory.GetDirectories(_testOutputDir).Length.ShouldBe(0);
    }

    [Fact]
    public void ShouldCreateOutputDirectoryOnClean()
    {
        var newDir = Path.Combine(Path.GetTempPath(), "atoll-test-new-" + Guid.NewGuid().ToString("N")[..8]);
        var writer = new OutputWriter(newDir);

        try
        {
            writer.Clean();
            Directory.Exists(newDir).ShouldBeTrue();
        }
        finally
        {
            if (Directory.Exists(newDir))
            {
                Directory.Delete(newDir, recursive: true);
            }
        }
    }

    [Theory]
    [InlineData("/", "index.html")]
    [InlineData("/about", "about")]
    [InlineData("/blog/my-post", "blog")]
    public async Task ShouldConvertUrlPathToCorrectFilePath(string urlPath, string expectedContains)
    {
        var path = await _writer.WritePageAsync(urlPath, "<html></html>");

        path.ShouldEndWith("index.html");
        path.ShouldContain(expectedContains);
        File.Exists(path).ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldWriteRootUrlToIndexHtml()
    {
        var path = await _writer.WritePageAsync("/", "<html></html>");

        // The path should be directly in the output dir as index.html
        Path.GetFileName(path).ShouldBe("index.html");
        Path.GetDirectoryName(path).ShouldBe(_testOutputDir);
    }

    [Fact]
    public void ShouldThrowOnNullUrlPath()
    {
        Should.ThrowAsync<ArgumentNullException>(() => _writer.WritePageAsync(null!, "html"));
    }

    [Fact]
    public void ShouldThrowOnNullHtml()
    {
        Should.ThrowAsync<ArgumentNullException>(() => _writer.WritePageAsync("/", null!));
    }

    [Fact]
    public void ShouldThrowOnNullOutputDirectory()
    {
        Should.Throw<ArgumentNullException>(() => new OutputWriter(null!));
    }
}
