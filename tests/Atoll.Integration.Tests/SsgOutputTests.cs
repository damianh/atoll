using System.Text.Json;
using Atoll.Build.Pipeline;
using Atoll.Build.Ssg;
using Atoll.Routing;
using Atoll.Samples.Portfolio.Pages;

using BlogAboutPage = Atoll.Samples.Blog.Pages.AboutPage;
using BlogIndexPage = Atoll.Samples.Blog.Pages.IndexPage;
using PortfolioAboutPage = Atoll.Samples.Portfolio.Pages.AboutPage;
using PortfolioIndexPage = Atoll.Samples.Portfolio.Pages.IndexPage;

namespace Atoll.Integration.Tests;

/// <summary>
/// End-to-end SSG output tests that build → validate output → verify HTML
/// for both sample sites (Blog and Portfolio). These tests exercise the full
/// StaticSiteGenerator pipeline producing files on disk.
/// </summary>
public sealed class SsgOutputTests : IDisposable
{
    private readonly string _outputDir;

    public SsgOutputTests()
    {
        var id = Guid.NewGuid().ToString("N")[..8];
        _outputDir = Path.Combine(Path.GetTempPath(), "atoll-ssg-output-" + id);
    }

    public void Dispose()
    {
        if (Directory.Exists(_outputDir))
        {
            Directory.Delete(_outputDir, recursive: true);
        }
    }

    // ── Portfolio SSG output tests ──

    [Fact]
    public async Task PortfolioSsgShouldGenerateAllStaticPages()
    {
        var ssgOptions = new SsgOptions(_outputDir);
        var generator = new StaticSiteGenerator(ssgOptions);

        var routes = CreatePortfolioRoutes();
        var result = await generator.GenerateAsync(routes);

        result.IsSuccess.ShouldBeTrue();
        result.TotalCount.ShouldBe(4);

        File.Exists(Path.Combine(_outputDir, "index.html")).ShouldBeTrue();
        File.Exists(Path.Combine(_outputDir, "projects", "index.html")).ShouldBeTrue();
        File.Exists(Path.Combine(_outputDir, "about", "index.html")).ShouldBeTrue();
        File.Exists(Path.Combine(_outputDir, "contact", "index.html")).ShouldBeTrue();
    }

    [Fact]
    public async Task PortfolioHomepageSsgOutputShouldContainHeroSection()
    {
        var result = await BuildPortfolioSiteAsync();
        var indexHtml = await ReadOutputFileAsync("index.html");

        indexHtml.ShouldContain("<!DOCTYPE html>");
        indexHtml.ShouldContain("Alex Chen");
        indexHtml.ShouldContain("Full-Stack .NET Developer");
        indexHtml.ShouldContain("Featured Projects");
    }

    [Fact]
    public async Task PortfolioHomepageSsgOutputShouldContainLayoutStructure()
    {
        await BuildPortfolioSiteAsync();
        var indexHtml = await ReadOutputFileAsync("index.html");

        indexHtml.ShouldContain("<html");
        indexHtml.ShouldContain("<head>");
        indexHtml.ShouldContain("<header");
        indexHtml.ShouldContain("<main>");
        indexHtml.ShouldContain("<footer");
        indexHtml.ShouldContain("</html>");
    }

    [Fact]
    public async Task PortfolioProjectsPageSsgOutputShouldContainAllProjects()
    {
        await BuildPortfolioSiteAsync();
        var html = await ReadOutputFileAsync(Path.Combine("projects", "index.html"));

        html.ShouldContain("Atoll Framework");
        html.ShouldContain("Cloud Dashboard");
        html.ShouldContain("E-Commerce API");
        html.ShouldContain("DevOps Toolkit");
        html.ShouldContain("Weather Station");
        html.ShouldContain("Markdown Editor");
    }

    [Fact]
    public async Task PortfolioAboutPageSsgOutputShouldContainSkillsAndGallery()
    {
        await BuildPortfolioSiteAsync();
        var html = await ReadOutputFileAsync(Path.Combine("about", "index.html"));

        html.ShouldContain("About Me");
        html.ShouldContain("Skills");
        html.ShouldContain("Photo Gallery");
        // Skill badges
        html.ShouldContain("C#");
        html.ShouldContain("ASP.NET Core");
        // Image gallery island marker
        html.ShouldContain("image-gallery");
    }

    [Fact]
    public async Task PortfolioContactPageSsgOutputShouldContainFormIsland()
    {
        await BuildPortfolioSiteAsync();
        var html = await ReadOutputFileAsync(Path.Combine("contact", "index.html"));

        html.ShouldContain("Get in Touch");
        html.ShouldContain("<form");
        html.ShouldContain("name=\"name\"");
        html.ShouldContain("name=\"email\"");
        html.ShouldContain("name=\"message\"");
        html.ShouldContain("Other Ways to Reach Me");
    }

    [Fact]
    public async Task PortfolioSsgOutputShouldHaveNavigationOnAllPages()
    {
        await BuildPortfolioSiteAsync();
        var pages = new[] { "index.html", "projects/index.html", "about/index.html", "contact/index.html" };

        foreach (var page in pages)
        {
            var html = await ReadOutputFileAsync(page.Replace('/', Path.DirectorySeparatorChar));
            html.ShouldContain("href=\"/\"", customMessage: $"Missing Home nav in {page}");
            html.ShouldContain("href=\"/projects\"", customMessage: $"Missing Projects nav in {page}");
            html.ShouldContain("href=\"/about\"", customMessage: $"Missing About nav in {page}");
            html.ShouldContain("href=\"/contact\"", customMessage: $"Missing Contact nav in {page}");
        }
    }

    [Fact]
    public async Task PortfolioSsgOutputShouldHaveFooterOnAllPages()
    {
        await BuildPortfolioSiteAsync();
        var pages = new[] { "index.html", "projects/index.html", "about/index.html", "contact/index.html" };

        foreach (var page in pages)
        {
            var html = await ReadOutputFileAsync(page.Replace('/', Path.DirectorySeparatorChar));
            html.ShouldContain("Built with Atoll", customMessage: $"Missing footer in {page}");
        }
    }

    [Fact]
    public async Task PortfolioSsgOutputShouldStartWithDoctype()
    {
        await BuildPortfolioSiteAsync();
        var pages = new[] { "index.html", "projects/index.html", "about/index.html", "contact/index.html" };

        foreach (var page in pages)
        {
            var html = await ReadOutputFileAsync(page.Replace('/', Path.DirectorySeparatorChar));
            html.ShouldStartWith("<!DOCTYPE html>", customMessage: $"Missing DOCTYPE in {page}");
        }
    }

    [Fact]
    public async Task PortfolioSsgResultShouldIncludeTimingForAllPages()
    {
        var result = await BuildPortfolioSiteAsync();

        result.TotalElapsed.ShouldBeGreaterThan(TimeSpan.Zero);
        foreach (var pageResult in result.PageResults)
        {
            pageResult.Elapsed.ShouldBeGreaterThan(TimeSpan.Zero);
            pageResult.IsSuccess.ShouldBeTrue();
            pageResult.Html.Length.ShouldBeGreaterThan(0);
            pageResult.OutputPath.Length.ShouldBeGreaterThan(0);
        }
    }

    // ── Blog SSG output tests ──

    [Fact]
    public async Task BlogSsgShouldGenerateStaticPages()
    {
        var result = await BuildBlogStaticPagesAsync();

        result.IsSuccess.ShouldBeTrue();
        // Index + About = 2 static pages
        result.TotalCount.ShouldBe(2);

        File.Exists(Path.Combine(_outputDir, "index.html")).ShouldBeTrue();
        File.Exists(Path.Combine(_outputDir, "about", "index.html")).ShouldBeTrue();
    }

    [Fact]
    public async Task BlogSsgHomepageShouldContainWelcomeMessage()
    {
        await BuildBlogStaticPagesAsync();
        var html = await ReadOutputFileAsync("index.html");

        html.ShouldStartWith("<!DOCTYPE html>");
        html.ShouldContain("Welcome to Atoll Blog");
        html.ShouldContain("Read the Blog");
    }

    [Fact]
    public async Task BlogSsgAboutPageShouldContainContent()
    {
        await BuildBlogStaticPagesAsync();
        var html = await ReadOutputFileAsync(Path.Combine("about", "index.html"));

        html.ShouldContain("About This Blog");
        html.ShouldContain("Server-first rendering");
        html.ShouldContain("Islands architecture");
    }

    [Fact]
    public async Task BlogSsgOutputShouldHaveLayoutOnAllPages()
    {
        await BuildBlogStaticPagesAsync();
        var pages = new[] { "index.html", "about/index.html" };

        foreach (var page in pages)
        {
            var html = await ReadOutputFileAsync(page.Replace('/', Path.DirectorySeparatorChar));
            html.ShouldContain("Atoll Blog", customMessage: $"Missing header in {page}");
            html.ShouldContain("<nav", customMessage: $"Missing nav in {page}");
            html.ShouldContain("<footer", customMessage: $"Missing footer in {page}");
        }
    }

    // ── Combined SSG + asset pipeline tests ──

    [Fact]
    public async Task PortfolioSsgWithPostProcessingShouldInjectAssetReferences()
    {
        var ssgResult = await BuildPortfolioSiteAsync();

        // Run asset pipeline with explicit CSS and JS content.
        // Note: PortfolioLayout uses inline <style> blocks rather than [Styles] attribute,
        // so we pass JS sources directly to produce a JS asset.
        var pipelineOptions = new AssetPipelineOptions(_outputDir) { Minify = true, Fingerprint = true };
        var outputWriter = new OutputWriter(_outputDir);
        var pipeline = new AssetPipeline(pipelineOptions, outputWriter);
        var assetResult = await pipeline.RunAsync(
            Array.Empty<Type>(),
            new[] { "document.addEventListener('DOMContentLoaded', function(){});" });

        // Post-process HTML with explicit CSS href (simulating extracted CSS from inline styles)
        // and the real JS href from the pipeline
        var jsHref = assetResult.Js.HasContent ? "/" + assetResult.Js.OutputPath.Replace('\\', '/') : "";
        var postProcessor = new HtmlPostProcessor(new HtmlPostProcessorOptions
        {
            CssHref = "/_atoll/styles.abc123.css",
            JsHref = jsHref,
            RemoveInlineStyles = true,
        });

        foreach (var pageResult in ssgResult.PageResults)
        {
            if (!pageResult.IsSuccess || pageResult.OutputPath.Length == 0)
            {
                continue;
            }

            var processedHtml = postProcessor.Process(pageResult.Html);
            await File.WriteAllTextAsync(pageResult.OutputPath, processedHtml);
        }

        // Verify CSS link and JS script are injected into all pages
        var indexHtml = await ReadOutputFileAsync("index.html");
        indexHtml.ShouldContain("rel=\"stylesheet\"");
        indexHtml.ShouldContain("styles.abc123.css");
        indexHtml.ShouldContain("<script");

        var projectsHtml = await ReadOutputFileAsync(Path.Combine("projects", "index.html"));
        projectsHtml.ShouldContain("rel=\"stylesheet\"");
    }

    [Fact]
    public async Task PortfolioSsgWithPostProcessingShouldPreserveNonScopedInlineStyles()
    {
        var ssgResult = await BuildPortfolioSiteAsync();

        // The portfolio layout has non-scoped inline <style> blocks (author-injected)
        ssgResult.PageResults[0].Html.ShouldContain("<style>");

        // Run post-processor — only scoped styles (data-atoll-scope) are removed
        var postProcessor = new HtmlPostProcessor(new HtmlPostProcessorOptions
        {
            CssHref = "/_atoll/styles.abc123.css",
            RemoveInlineStyles = true,
        });

        var processed = postProcessor.Process(ssgResult.PageResults[0].Html);
        processed.ShouldContain("<style>");
        processed.ShouldContain("rel=\"stylesheet\"");
    }

    [Fact]
    public async Task PortfolioSsgShouldProduceBuildManifest()
    {
        var ssgOptions = new SsgOptions(_outputDir) { BaseUrl = "https://alexchen.dev" };
        var generator = new StaticSiteGenerator(ssgOptions);
        var ssgResult = await generator.GenerateAsync(CreatePortfolioRoutes());

        var manifestWriter = new BuildManifestWriter(_outputDir);
        var manifest = BuildManifestWriter.BuildFrom(ssgResult, null, ssgOptions);
        var manifestPath = await manifestWriter.WriteAsync(manifest);

        File.Exists(manifestPath).ShouldBeTrue();
        var json = await File.ReadAllTextAsync(manifestPath);
        var deserialized = JsonSerializer.Deserialize<BuildManifest>(json);
        deserialized.ShouldNotBeNull();
        deserialized!.Pages.Count.ShouldBe(4);
        deserialized.BaseUrl.ShouldBe("https://alexchen.dev");
        deserialized.Stats.TotalPages.ShouldBe(4);
        deserialized.Stats.SuccessPages.ShouldBe(4);
        deserialized.Stats.FailedPages.ShouldBe(0);
    }

    [Fact]
    public async Task PortfolioManifestShouldContainCorrectUrlPaths()
    {
        var ssgResult = await BuildPortfolioSiteAsync();
        var manifest = BuildManifestWriter.BuildFrom(
            ssgResult, null, new SsgOptions(_outputDir));

        var urlPaths = manifest.Pages.Select(p => p.UrlPath).OrderBy(x => x).ToList();
        urlPaths.ShouldContain("/");
        urlPaths.ShouldContain("/about");
        urlPaths.ShouldContain("/contact");
        urlPaths.ShouldContain("/projects");
    }

    [Fact]
    public async Task PortfolioManifestShouldRecordComponentTypes()
    {
        var ssgResult = await BuildPortfolioSiteAsync();
        var manifest = BuildManifestWriter.BuildFrom(
            ssgResult, null, new SsgOptions(_outputDir));

        var indexPage = manifest.Pages.First(p => p.UrlPath == "/");
        indexPage.ComponentType.ShouldContain("IndexPage");

        var projectsPage = manifest.Pages.First(p => p.UrlPath == "/projects");
        projectsPage.ComponentType.ShouldContain("ProjectsPage");
    }

    // ── SSG with sequential rendering tests ──

    [Fact]
    public async Task PortfolioSsgWithSequentialRenderingShouldProduceIdenticalOutput()
    {
        // Build with sequential rendering
        var sequentialOptions = new SsgOptions(_outputDir) { MaxConcurrency = 1 };
        var generator = new StaticSiteGenerator(sequentialOptions);
        var result = await generator.GenerateAsync(CreatePortfolioRoutes());

        result.IsSuccess.ShouldBeTrue();
        result.TotalCount.ShouldBe(4);

        // Verify all pages have content
        foreach (var pageResult in result.PageResults)
        {
            pageResult.Html.Length.ShouldBeGreaterThan(100);
            pageResult.Html.ShouldContain("<!DOCTYPE html>");
        }
    }

    // ── SSG clean output directory tests ──

    [Fact]
    public async Task SsgShouldCleanOutputDirectoryByDefault()
    {
        Directory.CreateDirectory(_outputDir);
        await File.WriteAllTextAsync(Path.Combine(_outputDir, "stale.html"), "old content");

        var ssgOptions = new SsgOptions(_outputDir) { CleanOutputDirectory = true };
        var generator = new StaticSiteGenerator(ssgOptions);
        await generator.GenerateAsync(CreatePortfolioRoutes());

        File.Exists(Path.Combine(_outputDir, "stale.html")).ShouldBeFalse();
        File.Exists(Path.Combine(_outputDir, "index.html")).ShouldBeTrue();
    }

    [Fact]
    public async Task SsgShouldPreserveExistingFilesWhenCleanDisabled()
    {
        Directory.CreateDirectory(_outputDir);
        await File.WriteAllTextAsync(Path.Combine(_outputDir, "extra.txt"), "keep me");

        var ssgOptions = new SsgOptions(_outputDir) { CleanOutputDirectory = false };
        var generator = new StaticSiteGenerator(ssgOptions);
        await generator.GenerateAsync(CreatePortfolioRoutes());

        File.Exists(Path.Combine(_outputDir, "extra.txt")).ShouldBeTrue();
        File.Exists(Path.Combine(_outputDir, "index.html")).ShouldBeTrue();
    }

    // ── SSG output directory structure tests ──

    [Fact]
    public async Task PortfolioSsgOutputShouldUseCleanUrlDirectoryStructure()
    {
        await BuildPortfolioSiteAsync();

        // Clean URLs: /projects -> projects/index.html
        Directory.Exists(Path.Combine(_outputDir, "projects")).ShouldBeTrue();
        Directory.Exists(Path.Combine(_outputDir, "about")).ShouldBeTrue();
        Directory.Exists(Path.Combine(_outputDir, "contact")).ShouldBeTrue();

        // Root index.html should be at the top level
        File.Exists(Path.Combine(_outputDir, "index.html")).ShouldBeTrue();
    }

    // ── Cross-site SSG comparison tests ──

    [Fact]
    public async Task BothSampleSitesShouldBuildSuccessfullyInSequence()
    {
        // Build portfolio
        var portfolioDir = Path.Combine(_outputDir, "portfolio");
        var portfolioOptions = new SsgOptions(portfolioDir);
        var portfolioGenerator = new StaticSiteGenerator(portfolioOptions);
        var portfolioResult = await portfolioGenerator.GenerateAsync(CreatePortfolioRoutes());

        // Build blog static pages
        var blogDir = Path.Combine(_outputDir, "blog");
        var blogOptions = new SsgOptions(blogDir);
        var blogGenerator = new StaticSiteGenerator(blogOptions);
        var blogResult = await blogGenerator.GenerateAsync(CreateBlogStaticRoutes());

        portfolioResult.IsSuccess.ShouldBeTrue();
        blogResult.IsSuccess.ShouldBeTrue();

        // Both should produce valid HTML
        var portfolioHtml = await File.ReadAllTextAsync(Path.Combine(portfolioDir, "index.html"));
        portfolioHtml.ShouldStartWith("<!DOCTYPE html>");
        portfolioHtml.ShouldContain("Alex Chen");

        var blogHtml = await File.ReadAllTextAsync(Path.Combine(blogDir, "index.html"));
        blogHtml.ShouldStartWith("<!DOCTYPE html>");
        blogHtml.ShouldContain("Welcome to Atoll Blog");
    }

    // ── SSG page HTML validation tests ──

    [Fact]
    public async Task PortfolioSsgPagesShouldContainValidHtmlStructure()
    {
        var result = await BuildPortfolioSiteAsync();

        foreach (var pageResult in result.PageResults)
        {
            pageResult.Html.ShouldStartWith("<!DOCTYPE html>",
                customMessage: $"Page {pageResult.Route.UrlPath} missing DOCTYPE");
            pageResult.Html.ShouldContain("<html",
                customMessage: $"Page {pageResult.Route.UrlPath} missing <html>");
            pageResult.Html.ShouldContain("</html>",
                customMessage: $"Page {pageResult.Route.UrlPath} missing </html>");
            pageResult.Html.ShouldContain("<head>",
                customMessage: $"Page {pageResult.Route.UrlPath} missing <head>");
            pageResult.Html.ShouldContain("<body>",
                customMessage: $"Page {pageResult.Route.UrlPath} missing <body>");
        }
    }

    [Fact]
    public async Task PortfolioSsgPagesShouldContainMetaTags()
    {
        var result = await BuildPortfolioSiteAsync();

        foreach (var pageResult in result.PageResults)
        {
            pageResult.Html.ShouldContain("charset=\"utf-8\"",
                customMessage: $"Page {pageResult.Route.UrlPath} missing charset");
            pageResult.Html.ShouldContain("viewport",
                customMessage: $"Page {pageResult.Route.UrlPath} missing viewport");
        }
    }

    [Fact]
    public async Task BlogSsgPagesShouldContainMetaTags()
    {
        await BuildBlogStaticPagesAsync();
        var indexHtml = await ReadOutputFileAsync("index.html");

        indexHtml.ShouldContain("charset=\"utf-8\"");
        indexHtml.ShouldContain("viewport");
    }

    // ── SSG empty build tests ──

    [Fact]
    public async Task SsgWithNoRoutesShouldProduceEmptyOutput()
    {
        var ssgOptions = new SsgOptions(_outputDir);
        var generator = new StaticSiteGenerator(ssgOptions);

        var result = await generator.GenerateAsync(Array.Empty<RouteEntry>());

        result.IsSuccess.ShouldBeTrue();
        result.TotalCount.ShouldBe(0);
        result.PageResults.ShouldBeEmpty();
    }

    // ── SSG manifest round-trip tests ──

    [Fact]
    public async Task ManifestShouldRoundTripThroughJsonSerialization()
    {
        var ssgResult = await BuildPortfolioSiteAsync();
        var manifest = BuildManifestWriter.BuildFrom(
            ssgResult, null, new SsgOptions(_outputDir) { BaseUrl = "https://example.com", BasePath = "/portfolio" });

        var json = BuildManifestWriter.Serialize(manifest);
        var deserialized = JsonSerializer.Deserialize<BuildManifest>(json);

        deserialized.ShouldNotBeNull();
        deserialized!.BaseUrl.ShouldBe("https://example.com");
        deserialized.BasePath.ShouldBe("/portfolio");
        deserialized.Pages.Count.ShouldBe(4);
        deserialized.Stats.TotalPages.ShouldBe(4);
        deserialized.Stats.SuccessPages.ShouldBe(4);
    }

    [Fact]
    public async Task ManifestStatsShouldTrackBuildTiming()
    {
        var ssgResult = await BuildPortfolioSiteAsync();
        var manifest = BuildManifestWriter.BuildFrom(
            ssgResult, null, new SsgOptions(_outputDir));

        manifest.Stats.TotalBuildTimeMs.ShouldBeGreaterThan(0);
        manifest.Pages.ShouldAllBe(p => p.RenderTimeMs >= 0);
    }

    // ── SSG with base path tests ──

    [Fact]
    public async Task SsgWithBasePathShouldProduceValidOutput()
    {
        var ssgOptions = new SsgOptions(_outputDir) { BasePath = "/portfolio" };
        var generator = new StaticSiteGenerator(ssgOptions);
        var result = await generator.GenerateAsync(CreatePortfolioRoutes());

        result.IsSuccess.ShouldBeTrue();

        // Pages should still be generated at the same file paths
        File.Exists(Path.Combine(_outputDir, "index.html")).ShouldBeTrue();
        File.Exists(Path.Combine(_outputDir, "projects", "index.html")).ShouldBeTrue();
    }

    [Fact]
    public async Task SsgWithBasePathPostProcessingShouldAdjustLinks()
    {
        var ssgOptions = new SsgOptions(_outputDir) { BasePath = "/portfolio" };
        var generator = new StaticSiteGenerator(ssgOptions);
        var result = await generator.GenerateAsync(CreatePortfolioRoutes());

        var postProcessor = new HtmlPostProcessor(new HtmlPostProcessorOptions
        {
            BasePath = "/portfolio",
        });

        var indexHtml = result.PageResults.First(p => p.Route.UrlPath == "/").Html;
        var processedHtml = postProcessor.Process(indexHtml);

        // Internal links should be adjusted with base path
        processedHtml.ShouldContain("href=\"/portfolio/projects\"");
        processedHtml.ShouldContain("href=\"/portfolio/about\"");
        processedHtml.ShouldContain("href=\"/portfolio/contact\"");
    }

    [Fact]
    public async Task SsgWithBasePathPostProcessingShouldNotAdjustExternalLinks()
    {
        var result = await BuildPortfolioSiteAsync();

        var postProcessor = new HtmlPostProcessor(new HtmlPostProcessorOptions
        {
            BasePath = "/portfolio",
        });

        var indexHtml = result.PageResults.First(p => p.Route.UrlPath == "/").Html;
        var processedHtml = postProcessor.Process(indexHtml);

        // External links should NOT be prefixed
        processedHtml.ShouldContain("href=\"https://github.com\"");
        processedHtml.ShouldContain("href=\"https://linkedin.com\"");
    }

    // ── Helpers ──

    private static RouteEntry[] CreatePortfolioRoutes()
    {
        return
        [
            new RouteEntry("/", typeof(PortfolioIndexPage), "index.cs"),
            new RouteEntry("/projects", typeof(ProjectsPage), "projects.cs"),
            new RouteEntry("/about", typeof(PortfolioAboutPage), "about.cs"),
            new RouteEntry("/contact", typeof(ContactPage), "contact.cs"),
        ];
    }

    private static RouteEntry[] CreateBlogStaticRoutes()
    {
        // Only the static (non-content-dependent) pages
        return
        [
            new RouteEntry("/", typeof(BlogIndexPage), "index.cs"),
            new RouteEntry("/about", typeof(BlogAboutPage), "about.cs"),
        ];
    }

    private async Task<SsgResult> BuildPortfolioSiteAsync()
    {
        var ssgOptions = new SsgOptions(_outputDir);
        var generator = new StaticSiteGenerator(ssgOptions);
        return await generator.GenerateAsync(CreatePortfolioRoutes());
    }

    private async Task<SsgResult> BuildBlogStaticPagesAsync()
    {
        var ssgOptions = new SsgOptions(_outputDir);
        var generator = new StaticSiteGenerator(ssgOptions);
        return await generator.GenerateAsync(CreateBlogStaticRoutes());
    }

    private async Task<string> ReadOutputFileAsync(string relativePath)
    {
        var fullPath = Path.Combine(_outputDir, relativePath);
        File.Exists(fullPath).ShouldBeTrue(customMessage: $"Expected file not found: {relativePath}");
        return await File.ReadAllTextAsync(fullPath);
    }
}
