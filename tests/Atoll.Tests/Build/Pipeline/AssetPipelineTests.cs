using Atoll.Build.Pipeline;
using Atoll.Build.Ssg;
using Atoll.Components;
using Atoll.Css;

namespace Atoll.Build.Tests.Pipeline;

public sealed class AssetPipelineTests : IDisposable
{
    private readonly string _testDir;
    private readonly string _outputDir;
    private readonly string _publicDir;

    public AssetPipelineTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "atoll-pipeline-test-" + Guid.NewGuid().ToString("N")[..8]);
        _outputDir = Path.Combine(_testDir, "dist");
        _publicDir = Path.Combine(_testDir, "public");
        Directory.CreateDirectory(_outputDir);
        Directory.CreateDirectory(_publicDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
        {
            Directory.Delete(_testDir, recursive: true);
        }
    }

    [Fact]
    public async Task RunAsyncShouldProcessCssFromComponentTypes()
    {
        var options = new AssetPipelineOptions(_outputDir)
        {
            Minify = false,
            Fingerprint = false,
        };
        var outputWriter = new OutputWriter(_outputDir);
        var pipeline = new AssetPipeline(options, outputWriter);

        var result = await pipeline.RunAsync(
            new[] { typeof(StyledCard) },
            Array.Empty<string>());

        result.Css.HasContent.ShouldBeTrue();
        var cssFilePath = Path.Combine(_outputDir, result.Css.OutputPath);
        File.Exists(cssFilePath).ShouldBeTrue();
        var writtenCss = File.ReadAllText(cssFilePath);
        writtenCss.ShouldContain("padding");
    }

    [Fact]
    public async Task RunAsyncShouldProcessJsSources()
    {
        var options = new AssetPipelineOptions(_outputDir)
        {
            Minify = false,
            Fingerprint = false,
        };
        var outputWriter = new OutputWriter(_outputDir);
        var pipeline = new AssetPipeline(options, outputWriter);

        var result = await pipeline.RunAsync(
            Array.Empty<Type>(),
            new[] { "console.log('hello');" });

        result.Js.HasContent.ShouldBeTrue();
        var jsFilePath = Path.Combine(_outputDir, result.Js.OutputPath);
        File.Exists(jsFilePath).ShouldBeTrue();
        var writtenJs = File.ReadAllText(jsFilePath);
        writtenJs.ShouldContain("hello");
    }

    [Fact]
    public async Task RunAsyncShouldCopyStaticAssets()
    {
        File.WriteAllText(Path.Combine(_publicDir, "robots.txt"), "User-agent: *");

        var options = new AssetPipelineOptions(_outputDir)
        {
            PublicDirectory = _publicDir,
        };
        var outputWriter = new OutputWriter(_outputDir);
        var pipeline = new AssetPipeline(options, outputWriter);

        var result = await pipeline.RunAsync(
            Array.Empty<Type>(),
            Array.Empty<string>());

        result.StaticAssets.ShouldNotBeNull();
        result.StaticAssets!.Count.ShouldBe(1);
        File.Exists(Path.Combine(_outputDir, "robots.txt")).ShouldBeTrue();
    }

    [Fact]
    public async Task RunAsyncShouldNotCopyStaticAssetsWhenNoPublicDirectory()
    {
        var options = new AssetPipelineOptions(_outputDir);
        var outputWriter = new OutputWriter(_outputDir);
        var pipeline = new AssetPipeline(options, outputWriter);

        var result = await pipeline.RunAsync(
            Array.Empty<Type>(),
            Array.Empty<string>());

        result.StaticAssets.ShouldBeNull();
    }

    [Fact]
    public async Task RunAsyncShouldFingerprintAssets()
    {
        var options = new AssetPipelineOptions(_outputDir)
        {
            Fingerprint = true,
        };
        var outputWriter = new OutputWriter(_outputDir);
        var pipeline = new AssetPipeline(options, outputWriter);

        var result = await pipeline.RunAsync(
            new[] { typeof(StyledCard) },
            new[] { "var x = 1;" });

        result.Css.Hash.ShouldNotBeNull();
        result.Css.FileName.ShouldContain(result.Css.Hash!);
        result.Js.Hash.ShouldNotBeNull();
        result.Js.FileName.ShouldContain(result.Js.Hash!);
    }

    [Fact]
    public async Task RunAsyncShouldHandleCssAndJsTogether()
    {
        File.WriteAllText(Path.Combine(_publicDir, "favicon.ico"), "icon");

        var options = new AssetPipelineOptions(_outputDir)
        {
            Minify = false,
            Fingerprint = false,
            PublicDirectory = _publicDir,
        };
        var outputWriter = new OutputWriter(_outputDir);
        var pipeline = new AssetPipeline(options, outputWriter);

        var result = await pipeline.RunAsync(
            new[] { typeof(StyledCard) },
            new[] { "console.log('init');" });

        result.Css.HasContent.ShouldBeTrue();
        result.Js.HasContent.ShouldBeTrue();
        result.StaticAssets.ShouldNotBeNull();
        result.StaticAssets!.Count.ShouldBe(1);
        result.Elapsed.ShouldBeGreaterThan(TimeSpan.Zero);
    }

    [Fact]
    public async Task RunAsyncShouldWriteToCorrectSubdirectory()
    {
        var options = new AssetPipelineOptions(_outputDir)
        {
            Minify = false,
            Fingerprint = false,
            AssetSubdirectory = "_assets",
        };
        var outputWriter = new OutputWriter(_outputDir);
        var pipeline = new AssetPipeline(options, outputWriter);

        var result = await pipeline.RunAsync(
            new[] { typeof(StyledCard) },
            new[] { "var x = 1;" });

        result.Css.OutputPath.ShouldStartWith("_assets");
        result.Js.OutputPath.ShouldStartWith("_assets");
        File.Exists(Path.Combine(_outputDir, result.Css.OutputPath)).ShouldBeTrue();
        File.Exists(Path.Combine(_outputDir, result.Js.OutputPath)).ShouldBeTrue();
    }

    [Fact]
    public async Task RunAsyncWithAggregatorShouldProcessCss()
    {
        var aggregator = new CssAggregator();
        aggregator.Add("global", "body { margin: 0; }", true);

        var options = new AssetPipelineOptions(_outputDir)
        {
            Minify = false,
            Fingerprint = false,
        };
        var outputWriter = new OutputWriter(_outputDir);
        var pipeline = new AssetPipeline(options, outputWriter);

        var result = await pipeline.RunAsync(
            aggregator,
            Array.Empty<string>());

        result.Css.HasContent.ShouldBeTrue();
        var cssFilePath = Path.Combine(_outputDir, result.Css.OutputPath);
        File.Exists(cssFilePath).ShouldBeTrue();
        var writtenCss = File.ReadAllText(cssFilePath);
        writtenCss.ShouldContain("margin");
    }

    [Fact]
    public async Task RunAsyncShouldNotWriteEmptyCssFile()
    {
        var options = new AssetPipelineOptions(_outputDir)
        {
            Fingerprint = false,
        };
        var outputWriter = new OutputWriter(_outputDir);
        var pipeline = new AssetPipeline(options, outputWriter);

        var result = await pipeline.RunAsync(
            Array.Empty<Type>(),
            Array.Empty<string>());

        result.Css.HasContent.ShouldBeFalse();
        // No CSS file should be written
        Directory.GetFiles(_outputDir, "*.css", SearchOption.AllDirectories).Length.ShouldBe(0);
    }

    [Fact]
    public async Task RunAsyncShouldNotWriteEmptyJsFile()
    {
        var options = new AssetPipelineOptions(_outputDir)
        {
            Fingerprint = false,
        };
        var outputWriter = new OutputWriter(_outputDir);
        var pipeline = new AssetPipeline(options, outputWriter);

        var result = await pipeline.RunAsync(
            Array.Empty<Type>(),
            Array.Empty<string>());

        result.Js.HasContent.ShouldBeFalse();
        Directory.GetFiles(_outputDir, "*.js", SearchOption.AllDirectories).Length.ShouldBe(0);
    }

    [Fact]
    public void AssetPipelineOptionsShouldHaveSensibleDefaults()
    {
        var options = new AssetPipelineOptions("/dist");

        options.OutputDirectory.ShouldBe("/dist");
        options.BasePath.ShouldBe("");
        options.PublicDirectory.ShouldBe("");
        options.Minify.ShouldBeTrue();
        options.Fingerprint.ShouldBeTrue();
        options.AssetSubdirectory.ShouldBe("_atoll");
        options.CssOutputFileName.ShouldBe("styles.css");
        options.JsOutputFileName.ShouldBe("scripts.js");
    }

    [Fact]
    public void ConstructorShouldThrowOnNullOptions()
    {
        var outputWriter = new OutputWriter(_outputDir);
        Should.Throw<ArgumentNullException>(() => new AssetPipeline(null!, outputWriter));
    }

    [Fact]
    public void ConstructorShouldThrowOnNullOutputWriter()
    {
        var options = new AssetPipelineOptions(_outputDir);
        Should.Throw<ArgumentNullException>(() => new AssetPipeline(options, null!));
    }

    [Fact]
    public async Task RunAsyncShouldApplyUrlRewriting()
    {
        var options = new AssetPipelineOptions(_outputDir)
        {
            Minify = false,
            Fingerprint = false,
            BasePath = "/docs",
        };
        var outputWriter = new OutputWriter(_outputDir);
        var pipeline = new AssetPipeline(options, outputWriter);

        var result = await pipeline.RunAsync(
            new[] { typeof(UrlCard) },
            Array.Empty<string>());

        result.Css.HasContent.ShouldBeTrue();
        var cssContent = File.ReadAllText(Path.Combine(_outputDir, result.Css.OutputPath));
        cssContent.ShouldContain("/docs/images/bg.png");
    }

    // Test components
    [Styles(".card { padding: 1rem; border: 1px solid gray; }")]
    private sealed class StyledCard : AtollComponent
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            context.WriteHtml("<div class='card'>Content</div>");
            return Task.CompletedTask;
        }
    }

    [Styles(".card { background: url('/images/bg.png'); }")]
    private sealed class UrlCard : AtollComponent
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            context.WriteHtml("<div class='card'>Content</div>");
            return Task.CompletedTask;
        }
    }
}
