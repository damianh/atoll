using System.Text.Json;
using Atoll.Build.Pipeline;
using Atoll.Build.Ssg;
using Atoll.Components;
using Atoll.Css;
using Atoll.Rendering;
using Atoll.Routing;
using Shouldly;
using Xunit;

namespace Atoll.Build.Tests;

/// <summary>
/// End-to-end integration tests for the full Phase 8 SSG build pipeline:
/// render pages → process assets → post-process HTML → write manifest.
/// Validates that the pipeline produces a valid static site on disk.
/// </summary>
public sealed class EndToEndBuildPipelineTests : IDisposable
{
    private readonly string _outputDir;
    private readonly string _publicDir;

    public EndToEndBuildPipelineTests()
    {
        var id = Guid.NewGuid().ToString("N")[..8];
        _outputDir = Path.Combine(Path.GetTempPath(), "atoll-e2e-" + id);
        _publicDir = Path.Combine(Path.GetTempPath(), "atoll-e2e-public-" + id);
    }

    public void Dispose()
    {
        if (Directory.Exists(_outputDir))
        {
            Directory.Delete(_outputDir, recursive: true);
        }

        if (Directory.Exists(_publicDir))
        {
            Directory.Delete(_publicDir, recursive: true);
        }
    }

    [Fact]
    public async Task ShouldProduceCompleteStaticSiteFromSinglePage()
    {
        // Arrange: single home page
        var ssgOptions = new SsgOptions(_outputDir);
        var generator = new StaticSiteGenerator(ssgOptions);
        var routes = new[]
        {
            new RouteEntry("/", typeof(HomePage), "index.cs"),
        };

        // Act: SSG render
        var ssgResult = await generator.GenerateAsync(routes);

        // Assert: successful render
        ssgResult.IsSuccess.ShouldBeTrue();
        ssgResult.TotalCount.ShouldBe(1);

        // Assert: output file exists and contains expected HTML
        var indexPath = Path.Combine(_outputDir, "index.html");
        File.Exists(indexPath).ShouldBeTrue();
        var html = await File.ReadAllTextAsync(indexPath);
        html.ShouldStartWith("<!DOCTYPE html>");
        html.ShouldContain("<h1>Welcome Home</h1>");
    }

    [Fact]
    public async Task ShouldRenderStaticAndDynamicPagesWithAssets()
    {
        // Arrange: mix of static and dynamic routes
        var ssgOptions = new SsgOptions(_outputDir);
        var generator = new StaticSiteGenerator(ssgOptions);
        var routes = new[]
        {
            new RouteEntry("/", typeof(HomePage), "index.cs"),
            new RouteEntry("/about", typeof(AboutPage), "about.cs"),
            new RouteEntry("/blog/[slug]", typeof(BlogPage), "blog/[slug].cs"),
        };

        // Act: SSG render
        var ssgResult = await generator.GenerateAsync(routes);

        // Assert: 4 pages (2 static + 2 dynamic from GetStaticPaths)
        ssgResult.IsSuccess.ShouldBeTrue();
        ssgResult.TotalCount.ShouldBe(4);

        // Assert: all output files exist
        File.Exists(Path.Combine(_outputDir, "index.html")).ShouldBeTrue();
        File.Exists(Path.Combine(_outputDir, "about", "index.html")).ShouldBeTrue();
        File.Exists(Path.Combine(_outputDir, "blog", "hello-world", "index.html")).ShouldBeTrue();
        File.Exists(Path.Combine(_outputDir, "blog", "second-post", "index.html")).ShouldBeTrue();

        // Assert: blog pages contain dynamic content
        var blogHtml = await File.ReadAllTextAsync(Path.Combine(_outputDir, "blog", "hello-world", "index.html"));
        blogHtml.ShouldContain("<h1>Blog: hello-world</h1>");
    }

    [Fact]
    public async Task ShouldRunAssetPipelineAndWriteProcessedFiles()
    {
        // Arrange: render pages that have CSS
        var ssgOptions = new SsgOptions(_outputDir);
        var generator = new StaticSiteGenerator(ssgOptions);
        var routes = new[]
        {
            new RouteEntry("/", typeof(StyledHomePage), "index.cs"),
        };

        var ssgResult = await generator.GenerateAsync(routes);
        ssgResult.IsSuccess.ShouldBeTrue();

        // Act: run asset pipeline
        var pipelineOptions = new AssetPipelineOptions(_outputDir)
        {
            Minify = true,
            Fingerprint = true,
        };
        var outputWriter = new OutputWriter(_outputDir);
        var pipeline = new AssetPipeline(pipelineOptions, outputWriter);

        var componentTypes = new[] { typeof(StyledHomePage) };
        var jsSources = new[] { "console.log('hello');" };
        var assetResult = await pipeline.RunAsync(componentTypes, jsSources);

        // Assert: CSS was processed
        assetResult.Css.HasContent.ShouldBeTrue();
        assetResult.Css.Hash.ShouldNotBeNull();
        assetResult.Css.OutputPath.ShouldContain("_astro");
        assetResult.Css.OutputPath.ShouldEndWith(".css");

        // Assert: JS was processed
        assetResult.Js.HasContent.ShouldBeTrue();
        assetResult.Js.Hash.ShouldNotBeNull();
        assetResult.Js.OutputPath.ShouldContain("_astro");
        assetResult.Js.OutputPath.ShouldEndWith(".js");

        // Assert: processed CSS file exists on disk
        var cssFullPath = Path.Combine(_outputDir, assetResult.Css.OutputPath);
        File.Exists(cssFullPath).ShouldBeTrue();
        var cssContent = await File.ReadAllTextAsync(cssFullPath);
        cssContent.Length.ShouldBeGreaterThan(0);

        // Assert: processed JS file exists on disk
        var jsFullPath = Path.Combine(_outputDir, assetResult.Js.OutputPath);
        File.Exists(jsFullPath).ShouldBeTrue();
        var jsContent = await File.ReadAllTextAsync(jsFullPath);
        jsContent.Length.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task ShouldPostProcessHtmlWithAssetInjection()
    {
        // Arrange: render a page
        var ssgOptions = new SsgOptions(_outputDir);
        var generator = new StaticSiteGenerator(ssgOptions);
        var routes = new[]
        {
            new RouteEntry("/", typeof(StyledHomePage), "index.cs"),
        };

        var ssgResult = await generator.GenerateAsync(routes);
        ssgResult.IsSuccess.ShouldBeTrue();

        // Run asset pipeline to get fingerprinted paths
        var pipelineOptions = new AssetPipelineOptions(_outputDir) { Minify = true, Fingerprint = true };
        var outputWriter = new OutputWriter(_outputDir);
        var pipeline = new AssetPipeline(pipelineOptions, outputWriter);
        var assetResult = await pipeline.RunAsync(new[] { typeof(StyledHomePage) }, new[] { "console.log('app');" });

        // Act: post-process HTML
        var postProcessorOptions = new HtmlPostProcessorOptions
        {
            CssHref = "/" + assetResult.Css.OutputPath.Replace('\\', '/'),
            JsHref = "/" + assetResult.Js.OutputPath.Replace('\\', '/'),
            RemoveInlineStyles = true,
        };
        var postProcessor = new HtmlPostProcessor(postProcessorOptions);

        var originalHtml = ssgResult.PageResults[0].Html;
        var processedHtml = postProcessor.Process(originalHtml);

        // Assert: CSS link injected
        processedHtml.ShouldContain("rel=\"stylesheet\"");
        processedHtml.ShouldContain(assetResult.Css.OutputPath.Replace('\\', '/'));

        // Assert: JS script injected
        processedHtml.ShouldContain("<script");
        processedHtml.ShouldContain(assetResult.Js.OutputPath.Replace('\\', '/'));

        // Assert: inline styles removed
        processedHtml.ShouldNotContain("<style>");
    }

    [Fact]
    public async Task ShouldWriteBuildManifestWithAllPagesAndAssets()
    {
        // Arrange: full pipeline
        var ssgOptions = new SsgOptions(_outputDir)
        {
            BaseUrl = "https://example.com",
            BasePath = "/docs",
        };
        var generator = new StaticSiteGenerator(ssgOptions);
        var routes = new[]
        {
            new RouteEntry("/", typeof(HomePage), "index.cs"),
            new RouteEntry("/about", typeof(AboutPage), "about.cs"),
        };

        var ssgResult = await generator.GenerateAsync(routes);

        // Run asset pipeline
        var pipelineOptions = new AssetPipelineOptions(_outputDir) { Minify = true, Fingerprint = true };
        var outputWriter = new OutputWriter(_outputDir);
        var pipeline = new AssetPipeline(pipelineOptions, outputWriter);
        var assetResult = await pipeline.RunAsync(
            new[] { typeof(StyledHomePage) },
            new[] { "console.log('init');" });

        // Act: write manifest
        var manifestWriter = new BuildManifestWriter(_outputDir);
        var manifest = BuildManifestWriter.BuildFrom(ssgResult, assetResult, ssgOptions);
        var manifestPath = await manifestWriter.WriteAsync(manifest);

        // Assert: manifest file exists
        File.Exists(manifestPath).ShouldBeTrue();
        manifestPath.ShouldEndWith(Path.Combine(".atoll", "manifest.json"));

        // Assert: manifest is valid JSON with expected structure
        var json = await File.ReadAllTextAsync(manifestPath);
        var deserialized = JsonSerializer.Deserialize<BuildManifest>(json);
        deserialized.ShouldNotBeNull();
        deserialized!.Pages.Count.ShouldBe(2);
        deserialized.BaseUrl.ShouldBe("https://example.com");
        deserialized.BasePath.ShouldBe("/docs");
        deserialized.Stats.TotalPages.ShouldBe(2);
        deserialized.Stats.SuccessPages.ShouldBe(2);
        deserialized.Stats.FailedPages.ShouldBe(0);
        deserialized.Stats.TotalAssets.ShouldBe(2); // CSS + JS
    }

    [Fact]
    public async Task ShouldRunFullPipelineWithStaticAssets()
    {
        // Arrange: create public/ directory with static assets
        Directory.CreateDirectory(_publicDir);
        await File.WriteAllTextAsync(Path.Combine(_publicDir, "robots.txt"), "User-agent: *\nAllow: /");
        Directory.CreateDirectory(Path.Combine(_publicDir, "images"));
        await File.WriteAllBytesAsync(Path.Combine(_publicDir, "images", "logo.png"), new byte[] { 0x89, 0x50, 0x4E, 0x47 });

        // SSG render
        var ssgOptions = new SsgOptions(_outputDir);
        var generator = new StaticSiteGenerator(ssgOptions);
        var routes = new[]
        {
            new RouteEntry("/", typeof(HomePage), "index.cs"),
        };
        var ssgResult = await generator.GenerateAsync(routes);
        ssgResult.IsSuccess.ShouldBeTrue();

        // Act: run asset pipeline with static asset copying
        var pipelineOptions = new AssetPipelineOptions(_outputDir)
        {
            PublicDirectory = _publicDir,
            Minify = false,
            Fingerprint = false,
        };
        var outputWriter = new OutputWriter(_outputDir);
        var pipeline = new AssetPipeline(pipelineOptions, outputWriter);
        var assetResult = await pipeline.RunAsync(Array.Empty<Type>(), Array.Empty<string>());

        // Assert: static assets were copied
        assetResult.StaticAssets.ShouldNotBeNull();
        assetResult.StaticAssets!.Count.ShouldBe(2);

        File.Exists(Path.Combine(_outputDir, "robots.txt")).ShouldBeTrue();
        File.Exists(Path.Combine(_outputDir, "images", "logo.png")).ShouldBeTrue();

        var robotsTxt = await File.ReadAllTextAsync(Path.Combine(_outputDir, "robots.txt"));
        robotsTxt.ShouldContain("User-agent: *");
    }

    [Fact]
    public async Task ShouldProduceValidStaticSiteWithFullPipeline()
    {
        // This is the primary end-to-end test: SSG → assets → post-process → manifest
        // Verifies the complete output is a valid static site.

        // Step 1: Create public/ directory with static files
        Directory.CreateDirectory(_publicDir);
        await File.WriteAllTextAsync(Path.Combine(_publicDir, "favicon.ico"), "fake-icon");

        // Step 2: SSG render all pages
        var ssgOptions = new SsgOptions(_outputDir)
        {
            BaseUrl = "https://mysite.com",
        };
        var generator = new StaticSiteGenerator(ssgOptions);
        var routes = new[]
        {
            new RouteEntry("/", typeof(StyledHomePage), "index.cs"),
            new RouteEntry("/about", typeof(AboutPage), "about.cs"),
            new RouteEntry("/blog/[slug]", typeof(BlogPage), "blog/[slug].cs"),
        };

        var ssgResult = await generator.GenerateAsync(routes);
        ssgResult.IsSuccess.ShouldBeTrue();
        ssgResult.TotalCount.ShouldBe(4); // 2 static + 2 dynamic from blog

        // Step 3: Run asset pipeline (CSS + JS + static files)
        var pipelineOptions = new AssetPipelineOptions(_outputDir)
        {
            PublicDirectory = _publicDir,
            Minify = true,
            Fingerprint = true,
        };
        var outputWriter = new OutputWriter(_outputDir);
        var pipeline = new AssetPipeline(pipelineOptions, outputWriter);
        var assetResult = await pipeline.RunAsync(
            new[] { typeof(StyledHomePage) },
            new[] { "document.addEventListener('DOMContentLoaded', function() { console.log('ready'); });" });

        // Step 4: Post-process all HTML files
        var cssHref = assetResult.Css.HasContent ? "/" + assetResult.Css.OutputPath.Replace('\\', '/') : "";
        var jsHref = assetResult.Js.HasContent ? "/" + assetResult.Js.OutputPath.Replace('\\', '/') : "";

        var postProcessorOptions = new HtmlPostProcessorOptions
        {
            CssHref = cssHref,
            JsHref = jsHref,
            RemoveInlineStyles = true,
        };
        var postProcessor = new HtmlPostProcessor(postProcessorOptions);

        // Post-process and overwrite each rendered HTML file
        foreach (var pageResult in ssgResult.PageResults)
        {
            if (!pageResult.IsSuccess || pageResult.OutputPath.Length == 0)
            {
                continue;
            }

            var processedHtml = postProcessor.Process(pageResult.Html);
            await File.WriteAllTextAsync(pageResult.OutputPath, processedHtml);
        }

        // Step 5: Write build manifest
        var manifestWriter = new BuildManifestWriter(_outputDir);
        var manifest = BuildManifestWriter.BuildFrom(ssgResult, assetResult, ssgOptions);
        await manifestWriter.WriteAsync(manifest);

        // ── Verify the complete output ──

        // Page files exist
        File.Exists(Path.Combine(_outputDir, "index.html")).ShouldBeTrue();
        File.Exists(Path.Combine(_outputDir, "about", "index.html")).ShouldBeTrue();
        File.Exists(Path.Combine(_outputDir, "blog", "hello-world", "index.html")).ShouldBeTrue();
        File.Exists(Path.Combine(_outputDir, "blog", "second-post", "index.html")).ShouldBeTrue();

        // Asset files exist
        File.Exists(Path.Combine(_outputDir, assetResult.Css.OutputPath)).ShouldBeTrue();
        File.Exists(Path.Combine(_outputDir, assetResult.Js.OutputPath)).ShouldBeTrue();

        // Static files were copied
        File.Exists(Path.Combine(_outputDir, "favicon.ico")).ShouldBeTrue();

        // Manifest exists
        File.Exists(BuildManifestWriter.GetManifestPath(_outputDir)).ShouldBeTrue();

        // Post-processed HTML has asset references injected
        var indexHtml = await File.ReadAllTextAsync(Path.Combine(_outputDir, "index.html"));
        indexHtml.ShouldContain("rel=\"stylesheet\"");
        indexHtml.ShouldContain("<script");

        // About page (no inline styles) also has asset references
        var aboutHtml = await File.ReadAllTextAsync(Path.Combine(_outputDir, "about", "index.html"));
        aboutHtml.ShouldContain("rel=\"stylesheet\"");
        aboutHtml.ShouldContain("<script");

        // Manifest has correct page count
        var manifestJson = await File.ReadAllTextAsync(BuildManifestWriter.GetManifestPath(_outputDir));
        var deserializedManifest = JsonSerializer.Deserialize<BuildManifest>(manifestJson);
        deserializedManifest.ShouldNotBeNull();
        deserializedManifest!.Pages.Count.ShouldBe(4);
        deserializedManifest.Stats.SuccessPages.ShouldBe(4);
    }

    [Fact]
    public async Task ShouldHandleBasePathAcrossEntirePipeline()
    {
        // Tests that basePath is correctly applied through CSS URL rewriting
        // and HTML post-processing.

        var ssgOptions = new SsgOptions(_outputDir)
        {
            BasePath = "/docs",
        };
        var generator = new StaticSiteGenerator(ssgOptions);
        var routes = new[]
        {
            new RouteEntry("/", typeof(PageWithLinks), "index.cs"),
        };

        var ssgResult = await generator.GenerateAsync(routes);
        ssgResult.IsSuccess.ShouldBeTrue();

        // Run asset pipeline with base path
        var pipelineOptions = new AssetPipelineOptions(_outputDir)
        {
            BasePath = "/docs",
            Minify = false,
            Fingerprint = true,
        };
        var outputWriter = new OutputWriter(_outputDir);
        var pipeline = new AssetPipeline(pipelineOptions, outputWriter);
        var assetResult = await pipeline.RunAsync(Array.Empty<Type>(), new[] { "var x = 1;" });

        // Post-process with base path
        var jsHref = assetResult.Js.HasContent ? "/" + assetResult.Js.OutputPath.Replace('\\', '/') : "";
        var postProcessorOptions = new HtmlPostProcessorOptions
        {
            JsHref = jsHref,
            BasePath = "/docs",
        };
        var postProcessor = new HtmlPostProcessor(postProcessorOptions);

        var html = ssgResult.PageResults[0].Html;
        var processedHtml = postProcessor.Process(html);

        // Assert: links are adjusted with base path
        processedHtml.ShouldContain("href=\"/docs/about\"");
        processedHtml.ShouldContain("src=\"/docs/images/hero.png\"");
    }

    [Fact]
    public async Task ShouldHandleFailedPagesGracefullyInPipeline()
    {
        // One page succeeds, one fails — pipeline should still produce output for the good page

        var ssgOptions = new SsgOptions(_outputDir);
        var generator = new StaticSiteGenerator(ssgOptions);
        var routes = new[]
        {
            new RouteEntry("/", typeof(HomePage), "index.cs"),
            new RouteEntry("/error", typeof(ErrorPage), "error.cs"),
        };

        var ssgResult = await generator.GenerateAsync(routes);

        ssgResult.IsSuccess.ShouldBeFalse();
        ssgResult.SuccessCount.ShouldBe(1);
        ssgResult.FailureCount.ShouldBe(1);

        // The successful page should still have its output file
        File.Exists(Path.Combine(_outputDir, "index.html")).ShouldBeTrue();

        // Write manifest — should record both success and failure
        var manifestWriter = new BuildManifestWriter(_outputDir);
        var manifest = BuildManifestWriter.BuildFrom(ssgResult, null, ssgOptions);
        var manifestPath = await manifestWriter.WriteAsync(manifest);

        var json = await File.ReadAllTextAsync(manifestPath);
        var deserialized = JsonSerializer.Deserialize<BuildManifest>(json);
        deserialized.ShouldNotBeNull();
        deserialized!.Stats.TotalPages.ShouldBe(2);
        deserialized.Stats.SuccessPages.ShouldBe(1);
        deserialized.Stats.FailedPages.ShouldBe(1);
    }

    [Fact]
    public async Task ShouldProduceConsistentFingerprintsAcrossRuns()
    {
        // Same content should produce the same fingerprinted filenames
        var pipelineOptions = new AssetPipelineOptions(_outputDir)
        {
            Minify = true,
            Fingerprint = true,
        };
        var outputWriter = new OutputWriter(_outputDir);

        var pipeline1 = new AssetPipeline(pipelineOptions, outputWriter);
        var result1 = await pipeline1.RunAsync(
            new[] { typeof(StyledHomePage) },
            new[] { "console.log('test');" });

        // Clean and run again
        if (Directory.Exists(_outputDir))
        {
            Directory.Delete(_outputDir, recursive: true);
        }

        var pipeline2 = new AssetPipeline(pipelineOptions, outputWriter);
        var result2 = await pipeline2.RunAsync(
            new[] { typeof(StyledHomePage) },
            new[] { "console.log('test');" });

        // Fingerprints should be identical
        result1.Css.Hash.ShouldBe(result2.Css.Hash);
        result1.Css.OutputPath.ShouldBe(result2.Css.OutputPath);
        result1.Js.Hash.ShouldBe(result2.Js.Hash);
        result1.Js.OutputPath.ShouldBe(result2.Js.OutputPath);
    }

    [Fact]
    public async Task ShouldCleanOutputDirectoryBeforeBuild()
    {
        // Pre-populate output with stale files
        Directory.CreateDirectory(_outputDir);
        await File.WriteAllTextAsync(Path.Combine(_outputDir, "stale.html"), "old content");
        Directory.CreateDirectory(Path.Combine(_outputDir, "old-dir"));
        await File.WriteAllTextAsync(Path.Combine(_outputDir, "old-dir", "stale.txt"), "old");

        var ssgOptions = new SsgOptions(_outputDir) { CleanOutputDirectory = true };
        var generator = new StaticSiteGenerator(ssgOptions);
        var routes = new[]
        {
            new RouteEntry("/", typeof(HomePage), "index.cs"),
        };

        var ssgResult = await generator.GenerateAsync(routes);
        ssgResult.IsSuccess.ShouldBeTrue();

        // Stale files should be gone
        File.Exists(Path.Combine(_outputDir, "stale.html")).ShouldBeFalse();
        Directory.Exists(Path.Combine(_outputDir, "old-dir")).ShouldBeFalse();

        // New file should exist
        File.Exists(Path.Combine(_outputDir, "index.html")).ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldRenderPagesWithLayoutsAndProduceValidHtml()
    {
        var ssgOptions = new SsgOptions(_outputDir);
        var generator = new StaticSiteGenerator(ssgOptions);
        var routes = new[]
        {
            new RouteEntry("/", typeof(LayoutPage), "index.cs"),
        };

        var ssgResult = await generator.GenerateAsync(routes);
        ssgResult.IsSuccess.ShouldBeTrue();

        var html = ssgResult.PageResults[0].Html;
        html.ShouldStartWith("<!DOCTYPE html>");
        html.ShouldContain("<html>");
        html.ShouldContain("<head>");
        html.ShouldContain("<body>");
        html.ShouldContain("<main>");
        html.ShouldContain("<h1>Laid Out Page</h1>");
        html.ShouldContain("</main>");
        html.ShouldContain("</body>");
        html.ShouldContain("</html>");
    }

    [Fact]
    public async Task ShouldPreserveDynamicRouteParametersInManifest()
    {
        var ssgOptions = new SsgOptions(_outputDir);
        var generator = new StaticSiteGenerator(ssgOptions);
        var routes = new[]
        {
            new RouteEntry("/blog/[slug]", typeof(BlogPage), "blog/[slug].cs"),
        };

        var ssgResult = await generator.GenerateAsync(routes);
        ssgResult.IsSuccess.ShouldBeTrue();

        var manifest = BuildManifestWriter.BuildFrom(ssgResult, null, ssgOptions);

        manifest.Pages.Count.ShouldBe(2);

        var helloPage = manifest.Pages.First(p => p.UrlPath == "/blog/hello-world");
        helloPage.Parameters.ShouldNotBeNull();
        helloPage.Parameters!.ShouldContainKey("slug");
        helloPage.Parameters["slug"].ShouldBe("hello-world");

        var secondPage = manifest.Pages.First(p => p.UrlPath == "/blog/second-post");
        secondPage.Parameters.ShouldNotBeNull();
        secondPage.Parameters!.ShouldContainKey("slug");
        secondPage.Parameters["slug"].ShouldBe("second-post");
    }

    [Fact]
    public async Task ShouldHandleEmptyBuild()
    {
        // No routes at all
        var ssgOptions = new SsgOptions(_outputDir);
        var generator = new StaticSiteGenerator(ssgOptions);

        var ssgResult = await generator.GenerateAsync([]);
        ssgResult.IsSuccess.ShouldBeTrue();
        ssgResult.TotalCount.ShouldBe(0);

        // Asset pipeline with no content
        var pipelineOptions = new AssetPipelineOptions(_outputDir);
        var outputWriter = new OutputWriter(_outputDir);
        var pipeline = new AssetPipeline(pipelineOptions, outputWriter);
        var assetResult = await pipeline.RunAsync(Array.Empty<Type>(), Array.Empty<string>());

        assetResult.Css.HasContent.ShouldBeFalse();
        assetResult.Js.HasContent.ShouldBeFalse();

        // Manifest with no pages or assets
        var manifest = BuildManifestWriter.BuildFrom(ssgResult, assetResult, ssgOptions);
        manifest.Pages.Count.ShouldBe(0);
        manifest.Stats.TotalPages.ShouldBe(0);
        manifest.Stats.TotalAssets.ShouldBe(0);
    }

    [Fact]
    public async Task ShouldRoundTripManifestThroughJsonSerialization()
    {
        var ssgOptions = new SsgOptions(_outputDir) { BaseUrl = "https://example.com", BasePath = "/app" };
        var generator = new StaticSiteGenerator(ssgOptions);
        var routes = new[]
        {
            new RouteEntry("/", typeof(StyledHomePage), "index.cs"),
            new RouteEntry("/blog/[slug]", typeof(BlogPage), "blog/[slug].cs"),
        };

        var ssgResult = await generator.GenerateAsync(routes);

        var pipelineOptions = new AssetPipelineOptions(_outputDir) { Minify = true, Fingerprint = true };
        var outputWriter = new OutputWriter(_outputDir);
        var pipeline = new AssetPipeline(pipelineOptions, outputWriter);
        var assetResult = await pipeline.RunAsync(
            new[] { typeof(StyledHomePage) },
            new[] { "console.log('x');" });

        var manifest = BuildManifestWriter.BuildFrom(ssgResult, assetResult, ssgOptions);
        var json = BuildManifestWriter.Serialize(manifest);
        var deserialized = JsonSerializer.Deserialize<BuildManifest>(json);

        deserialized.ShouldNotBeNull();
        deserialized!.BaseUrl.ShouldBe("https://example.com");
        deserialized.BasePath.ShouldBe("/app");
        deserialized.Pages.Count.ShouldBe(3); // 1 styled + 2 blog dynamic
        deserialized.Stats.TotalPages.ShouldBe(3);
        deserialized.Stats.SuccessPages.ShouldBe(3);
        deserialized.Assets.ShouldContainKey("css");
        deserialized.Assets.ShouldContainKey("js");
        deserialized.Assets["css"].MimeType.ShouldBe("text/css");
        deserialized.Assets["js"].MimeType.ShouldBe("application/javascript");
    }

    [Fact]
    public async Task ShouldGenerateMinifiedCssFromComponentStyles()
    {
        // Verify that CSS from [Styles] attribute is collected, minified, and written
        var pipelineOptions = new AssetPipelineOptions(_outputDir)
        {
            Minify = true,
            Fingerprint = false,
        };
        var outputWriter = new OutputWriter(_outputDir);
        var pipeline = new AssetPipeline(pipelineOptions, outputWriter);

        var assetResult = await pipeline.RunAsync(
            new[] { typeof(StyledHomePage) },
            Array.Empty<string>());

        assetResult.Css.HasContent.ShouldBeTrue();
        // Minified CSS should not have excessive whitespace
        assetResult.Css.Css.ShouldNotContain("  "); // no double spaces after minification
        assetResult.Css.Css.ShouldContain("padding"); // style content preserved

        // Verify on disk
        var cssPath = Path.Combine(_outputDir, assetResult.Css.OutputPath);
        File.Exists(cssPath).ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldSkipEndpointRoutesInSsg()
    {
        var ssgOptions = new SsgOptions(_outputDir);
        var generator = new StaticSiteGenerator(ssgOptions);
        var routes = new[]
        {
            new RouteEntry("/", typeof(HomePage), "index.cs"),
            new RouteEntry("/api/data", typeof(ApiEndpoint), "api/data.cs"),
        };

        var ssgResult = await generator.GenerateAsync(routes);

        // Only the page should be rendered; the endpoint is skipped
        ssgResult.TotalCount.ShouldBe(1);
        ssgResult.PageResults[0].Route.UrlPath.ShouldBe("/");
    }

    [Fact]
    public async Task ShouldWriteOutputWithCorrectDirectoryStructure()
    {
        var ssgOptions = new SsgOptions(_outputDir);
        var generator = new StaticSiteGenerator(ssgOptions);
        var routes = new[]
        {
            new RouteEntry("/", typeof(HomePage), "index.cs"),
            new RouteEntry("/about", typeof(AboutPage), "about.cs"),
            new RouteEntry("/blog/[slug]", typeof(BlogPage), "blog/[slug].cs"),
        };

        var ssgResult = await generator.GenerateAsync(routes);

        // Run pipeline to also create _astro/ subdirectory
        var pipelineOptions = new AssetPipelineOptions(_outputDir) { Minify = true, Fingerprint = true };
        var outputWriter = new OutputWriter(_outputDir);
        var pipeline = new AssetPipeline(pipelineOptions, outputWriter);
        await pipeline.RunAsync(new[] { typeof(StyledHomePage) }, new[] { "var a=1;" });

        // Verify directory structure
        Directory.Exists(_outputDir).ShouldBeTrue();
        Directory.Exists(Path.Combine(_outputDir, "about")).ShouldBeTrue();
        Directory.Exists(Path.Combine(_outputDir, "blog")).ShouldBeTrue();
        Directory.Exists(Path.Combine(_outputDir, "blog", "hello-world")).ShouldBeTrue();
        Directory.Exists(Path.Combine(_outputDir, "blog", "second-post")).ShouldBeTrue();
        Directory.Exists(Path.Combine(_outputDir, "_astro")).ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldProduceTimingInformationAcrossPipeline()
    {
        var ssgOptions = new SsgOptions(_outputDir);
        var generator = new StaticSiteGenerator(ssgOptions);
        var routes = new[]
        {
            new RouteEntry("/", typeof(HomePage), "index.cs"),
            new RouteEntry("/about", typeof(AboutPage), "about.cs"),
        };

        var ssgResult = await generator.GenerateAsync(routes);

        // SSG timing
        ssgResult.TotalElapsed.ShouldBeGreaterThan(TimeSpan.Zero);
        foreach (var pageResult in ssgResult.PageResults)
        {
            pageResult.Elapsed.ShouldBeGreaterThan(TimeSpan.Zero);
        }

        // Pipeline timing
        var pipelineOptions = new AssetPipelineOptions(_outputDir);
        var outputWriter = new OutputWriter(_outputDir);
        var pipeline = new AssetPipeline(pipelineOptions, outputWriter);
        var assetResult = await pipeline.RunAsync(Array.Empty<Type>(), Array.Empty<string>());
        assetResult.Elapsed.ShouldBeGreaterThan(TimeSpan.Zero);

        // Manifest stats capture timing
        var manifest = BuildManifestWriter.BuildFrom(ssgResult, assetResult, ssgOptions);
        manifest.Stats.TotalBuildTimeMs.ShouldBeGreaterThan(0);
        manifest.Pages.ShouldAllBe(p => p.RenderTimeMs >= 0);
    }

    // ── Test page components ─────────────────────────────────────

    private sealed class HomePage : AtollComponent, IAtollPage
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            context.WriteHtml("<h1>Welcome Home</h1><p>This is the home page.</p>");
            return Task.CompletedTask;
        }
    }

    private sealed class AboutPage : AtollComponent, IAtollPage
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            context.WriteHtml("<h1>About Us</h1><p>We build great software.</p>");
            return Task.CompletedTask;
        }
    }

    [Styles(".hero { padding: 2rem; background: #f0f0f0; } .hero h1 { color: navy; }")]
    private sealed class StyledHomePage : AtollComponent, IAtollPage
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            context.WriteHtml("<div class=\"hero\"><h1>Styled Home</h1></div>");
            return Task.CompletedTask;
        }
    }

    private sealed class BlogPage : AtollComponent, IAtollPage, IStaticPathsProvider
    {
        [Parameter]
        public string Slug { get; set; } = "";

        public Task<IReadOnlyList<StaticPath>> GetStaticPathsAsync()
        {
            IReadOnlyList<StaticPath> paths = new[]
            {
                new StaticPath(new Dictionary<string, string> { ["slug"] = "hello-world" }),
                new StaticPath(new Dictionary<string, string> { ["slug"] = "second-post" }),
            };
            return Task.FromResult(paths);
        }

        protected override Task RenderCoreAsync(RenderContext context)
        {
            context.WriteHtml($"<h1>Blog: {Slug}</h1><p>Blog post content.</p>");
            return Task.CompletedTask;
        }
    }

    private sealed class ErrorPage : AtollComponent, IAtollPage
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            throw new InvalidOperationException("Intentional page render error");
        }
    }

    private sealed class BaseLayout : AtollComponent
    {
        protected override async Task RenderCoreAsync(RenderContext context)
        {
            context.WriteHtml("<html><head></head><body><main>");
            await RenderSlotAsync();
            context.WriteHtml("</main></body></html>");
        }
    }

    [Layout(typeof(BaseLayout))]
    private sealed class LayoutPage : AtollComponent, IAtollPage
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            context.WriteHtml("<h1>Laid Out Page</h1><p>Content inside layout.</p>");
            return Task.CompletedTask;
        }
    }

    private sealed class PageWithLinks : AtollComponent, IAtollPage
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            context.WriteHtml(
                "<a href=\"/about\">About</a>" +
                "<img src=\"/images/hero.png\" alt=\"hero\" />" +
                "<a href=\"https://external.com\">External</a>");
            return Task.CompletedTask;
        }
    }

    private sealed class ApiEndpoint : IAtollEndpoint
    {
        public Task<AtollResponse> GetAsync(EndpointContext context)
        {
            return Task.FromResult(AtollResponse.Json(new { status = "ok" }));
        }
    }
}
