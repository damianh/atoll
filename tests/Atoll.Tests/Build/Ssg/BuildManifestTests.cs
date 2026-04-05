using System.Text.Json;
using Atoll.Build.Pipeline;
using Atoll.Build.Ssg;
using Atoll.Components;
using Atoll.Routing;
using Shouldly;
using Xunit;

namespace Atoll.Build.Tests.Ssg;

public sealed class BuildManifestTests : IDisposable
{
    private readonly string _testDir;
    private readonly string _outputDir;

    public BuildManifestTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "atoll-manifest-test-" + Guid.NewGuid().ToString("N")[..8]);
        _outputDir = Path.Combine(_testDir, "dist");
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
    public void BuildFromShouldPopulatePagesFromSsgResult()
    {
        var route = new SsgRoute("/about", typeof(TestPage));
        var pageResult = new SsgPageResult(route, Path.Combine(_outputDir, "about", "index.html"), "<h1>About</h1>", TimeSpan.FromMilliseconds(10));
        var ssgResult = new SsgResult([pageResult], TimeSpan.FromMilliseconds(15));
        var options = new SsgOptions(_outputDir);

        var manifest = BuildManifestWriter.BuildFrom(ssgResult, null, options);

        manifest.Pages.Count.ShouldBe(1);
        manifest.Pages[0].UrlPath.ShouldBe("/about");
        manifest.Pages[0].ComponentType.ShouldContain("TestPage");
        manifest.Pages[0].RenderTimeMs.ShouldBe(10);
    }

    [Fact]
    public void BuildFromShouldIncludeRouteParameters()
    {
        var parameters = new Dictionary<string, string> { ["slug"] = "hello" };
        var route = new SsgRoute("/blog/hello", typeof(TestPage), parameters);
        var pageResult = new SsgPageResult(route, Path.Combine(_outputDir, "blog", "hello", "index.html"), "<h1>Hello</h1>", TimeSpan.FromMilliseconds(5));
        var ssgResult = new SsgResult([pageResult], TimeSpan.FromMilliseconds(10));
        var options = new SsgOptions(_outputDir);

        var manifest = BuildManifestWriter.BuildFrom(ssgResult, null, options);

        manifest.Pages[0].Parameters.ShouldNotBeNull();
        manifest.Pages[0].Parameters!["slug"].ShouldBe("hello");
    }

    [Fact]
    public void BuildFromShouldOmitParametersWhenEmpty()
    {
        var route = new SsgRoute("/about", typeof(TestPage));
        var pageResult = new SsgPageResult(route, Path.Combine(_outputDir, "about", "index.html"), "<h1>About</h1>", TimeSpan.Zero);
        var ssgResult = new SsgResult([pageResult], TimeSpan.Zero);
        var options = new SsgOptions(_outputDir);

        var manifest = BuildManifestWriter.BuildFrom(ssgResult, null, options);

        manifest.Pages[0].Parameters.ShouldBeNull();
    }

    [Fact]
    public void BuildFromShouldPopulateAssetsFromPipelineResult()
    {
        var ssgResult = new SsgResult([], TimeSpan.Zero);
        var cssResult = new CssProcessResult("body{margin:0}", "_atoll/styles.abc.css", "styles.abc.css", "abc12345");
        var jsResult = new JsProcessResult("var x=1", "_atoll/scripts.def.js", "scripts.def.js", "def67890");
        var assetResult = new AssetPipelineResult(cssResult, jsResult, null, TimeSpan.Zero);
        var options = new SsgOptions(_outputDir);

        var manifest = BuildManifestWriter.BuildFrom(ssgResult, assetResult, options);

        manifest.Assets.ContainsKey("css").ShouldBeTrue();
        manifest.Assets["css"].OutputPath.ShouldBe("_atoll/styles.abc.css");
        manifest.Assets["css"].Hash.ShouldBe("abc12345");
        manifest.Assets["css"].MimeType.ShouldBe("text/css");
        manifest.Assets["css"].SizeBytes.ShouldBeGreaterThan(0);

        manifest.Assets.ContainsKey("js").ShouldBeTrue();
        manifest.Assets["js"].OutputPath.ShouldBe("_atoll/scripts.def.js");
        manifest.Assets["js"].Hash.ShouldBe("def67890");
        manifest.Assets["js"].MimeType.ShouldBe("application/javascript");
    }

    [Fact]
    public void BuildFromShouldNotIncludeAssetsWhenEmpty()
    {
        var ssgResult = new SsgResult([], TimeSpan.Zero);
        var cssResult = CssProcessResult.Empty;
        var jsResult = JsProcessResult.Empty;
        var assetResult = new AssetPipelineResult(cssResult, jsResult, null, TimeSpan.Zero);
        var options = new SsgOptions(_outputDir);

        var manifest = BuildManifestWriter.BuildFrom(ssgResult, assetResult, options);

        manifest.Assets.Count.ShouldBe(0);
    }

    [Fact]
    public void BuildFromShouldPopulateStats()
    {
        var route1 = new SsgRoute("/", typeof(TestPage));
        var route2 = new SsgRoute("/error", typeof(TestPage));
        var results = new List<SsgPageResult>
        {
            new(route1, Path.Combine(_outputDir, "index.html"), "<h1>Home</h1>", TimeSpan.FromMilliseconds(10)),
            new(route2, new Exception("fail"), TimeSpan.FromMilliseconds(5)),
        };
        var ssgResult = new SsgResult(results, TimeSpan.FromMilliseconds(20));
        var options = new SsgOptions(_outputDir);

        var manifest = BuildManifestWriter.BuildFrom(ssgResult, null, options);

        manifest.Stats.TotalPages.ShouldBe(2);
        manifest.Stats.SuccessPages.ShouldBe(1);
        manifest.Stats.FailedPages.ShouldBe(1);
        manifest.Stats.TotalBuildTimeMs.ShouldBe(20);
    }

    [Fact]
    public void BuildFromShouldIncludeStaticFilesCopiedCount()
    {
        var ssgResult = new SsgResult([], TimeSpan.Zero);
        var cssResult = CssProcessResult.Empty;
        var jsResult = JsProcessResult.Empty;
        var copyResult = new CopyResult([
            new CopiedFile("robots.txt", "/dist/robots.txt", 100),
            new CopiedFile("favicon.ico", "/dist/favicon.ico", 200),
        ]);
        var assetResult = new AssetPipelineResult(cssResult, jsResult, copyResult, TimeSpan.Zero);
        var options = new SsgOptions(_outputDir);

        var manifest = BuildManifestWriter.BuildFrom(ssgResult, assetResult, options);

        manifest.Stats.StaticFilesCopied.ShouldBe(2);
    }

    [Fact]
    public void BuildFromShouldSetBaseUrlAndBasePath()
    {
        var ssgResult = new SsgResult([], TimeSpan.Zero);
        var options = new SsgOptions(_outputDir)
        {
            BaseUrl = "https://example.com",
            BasePath = "/docs",
        };

        var manifest = BuildManifestWriter.BuildFrom(ssgResult, null, options);

        manifest.BaseUrl.ShouldBe("https://example.com");
        manifest.BasePath.ShouldBe("/docs");
    }

    [Fact]
    public void SerializeShouldProduceValidJson()
    {
        var manifest = new BuildManifest
        {
            Version = "1.0.0",
            BaseUrl = "https://example.com",
            Pages =
            [
                new ManifestPage { UrlPath = "/", OutputFile = "index.html", ComponentType = "HomePage" },
            ],
        };

        var json = BuildManifestWriter.Serialize(manifest);

        json.ShouldContain("\"version\"");
        json.ShouldContain("\"baseUrl\"");
        json.ShouldContain("\"pages\"");
        json.ShouldContain("\"urlPath\"");
        json.ShouldContain("index.html");

        // Should be parseable
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("version").GetString().ShouldBe("1.0.0");
    }

    [Fact]
    public void SerializeShouldOmitNullParameters()
    {
        var manifest = new BuildManifest
        {
            Pages =
            [
                new ManifestPage { UrlPath = "/about", Parameters = null },
            ],
        };

        var json = BuildManifestWriter.Serialize(manifest);

        // The "parameters" key should not appear when null
        var doc = JsonDocument.Parse(json);
        var page = doc.RootElement.GetProperty("pages")[0];
        page.TryGetProperty("parameters", out _).ShouldBeFalse();
    }

    [Fact]
    public async Task WriteAsyncShouldCreateManifestFile()
    {
        var manifest = new BuildManifest
        {
            Version = "1.0.0",
            BaseUrl = "https://example.com",
        };

        var writer = new BuildManifestWriter(_outputDir);
        var filePath = await writer.WriteAsync(manifest);

        File.Exists(filePath).ShouldBeTrue();
        filePath.ShouldEndWith(Path.Combine(".atoll", "manifest.json"));

        var content = File.ReadAllText(filePath);
        content.ShouldContain("\"version\"");
        content.ShouldContain("https://example.com");
    }

    [Fact]
    public async Task WriteAsyncShouldCreateAtollDirectory()
    {
        var manifest = new BuildManifest();

        var writer = new BuildManifestWriter(_outputDir);
        await writer.WriteAsync(manifest);

        Directory.Exists(Path.Combine(_outputDir, ".atoll")).ShouldBeTrue();
    }

    [Fact]
    public void GetManifestPathShouldReturnCorrectPath()
    {
        var path = BuildManifestWriter.GetManifestPath(_outputDir);

        path.ShouldBe(Path.Combine(_outputDir, ".atoll", "manifest.json"));
    }

    [Fact]
    public void BuildFromShouldThrowOnNullSsgResult()
    {
        var options = new SsgOptions(_outputDir);
        Should.Throw<ArgumentNullException>(() => BuildManifestWriter.BuildFrom(null!, null, options));
    }

    [Fact]
    public void BuildFromShouldThrowOnNullOptions()
    {
        var ssgResult = new SsgResult([], TimeSpan.Zero);
        Should.Throw<ArgumentNullException>(() => BuildManifestWriter.BuildFrom(ssgResult, null, null!));
    }

    [Fact]
    public void ConstructorShouldThrowOnNullOutputDirectory()
    {
        Should.Throw<ArgumentNullException>(() => new BuildManifestWriter(null!));
    }

    [Fact]
    public void ManifestShouldHaveDefaultValues()
    {
        var manifest = new BuildManifest();

        manifest.Version.ShouldBe("1.0.0");
        manifest.BaseUrl.ShouldBe("");
        manifest.BasePath.ShouldBe("");
        manifest.Pages.ShouldNotBeNull();
        manifest.Pages.Count.ShouldBe(0);
        manifest.Assets.ShouldNotBeNull();
        manifest.Assets.Count.ShouldBe(0);
        manifest.Stats.ShouldNotBeNull();
    }

    [Fact]
    public async Task WriteAsyncShouldOverwriteExistingManifest()
    {
        var writer = new BuildManifestWriter(_outputDir);

        var manifest1 = new BuildManifest { BaseUrl = "https://first.com" };
        await writer.WriteAsync(manifest1);

        var manifest2 = new BuildManifest { BaseUrl = "https://second.com" };
        var filePath = await writer.WriteAsync(manifest2);

        var content = File.ReadAllText(filePath);
        content.ShouldContain("https://second.com");
        content.ShouldNotContain("https://first.com");
    }

    private sealed class TestPage : AtollComponent, IAtollPage
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            context.WriteHtml("<h1>Test</h1>");
            return Task.CompletedTask;
        }
    }
}
