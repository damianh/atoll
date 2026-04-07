using Atoll.Build.Content.Collections;
using Atoll.Build.Pipeline;
using Atoll.Build.Ssg;
using Atoll.Components;
using Atoll.Css;
using Atoll.Islands;
using Docs;
using Docs.Pages;
using Atoll.Lagoon.Islands;
using Atoll.Lagoon.Search;
using Atoll.Lagoon.Styles;
using Atoll.Rendering;
using Atoll.Routing;

namespace Atoll.Integration.Tests;

/// <summary>
/// End-to-end integration tests for the Atoll documentation site.
/// Verifies content collections, page rendering, layout, sidebar navigation,
/// static path generation, and SSG output.
/// </summary>
public sealed class DocsSampleTests : IDisposable
{
    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;
    private readonly string _outputDir;

    public DocsSampleTests()
    {
        var id = Guid.NewGuid().ToString("N")[..8];
        _outputDir = Path.Combine(Path.GetTempPath(), "atoll-docs-ssg-" + id);
    }

    public void Dispose()
    {
        if (Directory.Exists(_outputDir))
        {
            Directory.Delete(_outputDir, recursive: true);
        }
    }

    // ── Content setup helpers ──

    private static InMemoryFileProvider CreateDocsContent()
    {
        var provider = new InMemoryFileProvider();

        provider.AddFile("content/docs", "getting-started.md", """
            ---
            title: Getting Started
            description: Create your first Atoll project and render a page.
            order: 1
            section: Basics
            ---

            # Getting Started

            Welcome to **Atoll** — a .NET-native static-site framework.

            ## Installation

            Add the NuGet package:

            ```
            dotnet add package Atoll
            ```
            """);

        provider.AddFile("content/docs", "components.md", """
            ---
            title: Components
            description: Learn how to build reusable Atoll components.
            order: 2
            section: Basics
            ---

            # Components

            All Atoll components extend `AtollComponent` and override `RenderCoreAsync`.
            """);

        provider.AddFile("content/docs", "content-collections.md", """
            ---
            title: Content Collections
            description: Type-safe Markdown content with YAML frontmatter validation.
            order: 5
            section: Features
            ---

            # Content Collections

            Define a schema class and use `CollectionQuery` to load your Markdown files.
            """);

        return provider;
    }

    private static CollectionQuery CreateCollectionQuery(InMemoryFileProvider fileProvider)
    {
        var config = new CollectionConfig("content")
            .AddCollection(ContentCollection.Define<DocSchema>("docs"));
        var loader = new CollectionLoader(config, fileProvider);
        return new CollectionQuery(loader);
    }

    private static CollectionQuery CreateDefaultQuery()
    {
        return CreateCollectionQuery(CreateDocsContent());
    }

    // ── Page rendering helpers ──

    private static async Task<string> RenderPageAsync<TPage>(
        IReadOnlyDictionary<string, object?>? props = null)
        where TPage : IAtollComponent, new()
    {
        var renderer = new PageRenderer();
        var pageProps = props ?? new Dictionary<string, object?>();

        var pageType = typeof(TPage);
        if (LayoutResolver.HasLayout(pageType))
        {
            var pageFragment = RenderFragment.FromAsync(async dest =>
            {
                await ComponentRenderer.RenderComponentAsync<TPage>(dest, pageProps);
            });
            var wrappedFragment = LayoutResolver.WrapWithLayouts(pageType, pageFragment, pageProps);
            var result = await renderer.RenderPageAsync(ctx =>
            {
                return ctx.RenderAsync(wrappedFragment).AsTask();
            });
            return result.Html;
        }
        else
        {
            var result = await renderer.RenderPageAsync<TPage>(pageProps);
            return result.Html;
        }
    }

    // ── Content collection tests ──

    [Fact]
    public void DocsShouldLoadAllDocEntries()
    {
        var query = CreateDefaultQuery();
        var docs = query.GetCollection<DocSchema>("docs");

        docs.Count.ShouldBe(3);
    }

    [Fact]
    public void DocsShouldParseDocFrontmatter()
    {
        var query = CreateDefaultQuery();
        var entry = query.GetEntry<DocSchema>("docs", "getting-started");

        entry.ShouldNotBeNull();
        entry!.Data.Title.ShouldBe("Getting Started");
        entry.Data.Description.ShouldBe("Create your first Atoll project and render a page.");
        entry.Data.Order.ShouldBe(1);
        entry.Data.Section.ShouldBe("Basics");
        entry.Slug.ShouldBe("getting-started");
    }

    [Fact]
    public void DocsShouldRenderMarkdownToHtml()
    {
        var query = CreateDefaultQuery();
        var entry = query.GetEntry<DocSchema>("docs", "getting-started")!;
        var rendered = query.Render(entry);

        rendered.Html.ShouldContain("<h1");
        rendered.Html.ShouldContain("Getting Started");
        rendered.Html.ShouldContain("<strong>Atoll</strong>");
    }

    // ── Index page tests ──

    [Fact]
    public async Task IndexPageShouldRenderLandingContent()
    {
        var query = CreateDefaultQuery();
        var props = new Dictionary<string, object?> { ["Query"] = query };
        var html = await RenderPageAsync<IndexPage>(props);

        html.ShouldContain("Atoll");
        html.ShouldContain("Get Started");
        html.ShouldContain("href=\"/docs/getting-started\"");
    }

    [Fact]
    public async Task IndexPageShouldHaveLayoutStructure()
    {
        var query = CreateDefaultQuery();
        var props = new Dictionary<string, object?> { ["Query"] = query };
        var html = await RenderPageAsync<IndexPage>(props);

        html.ShouldContain("<!DOCTYPE html>");
        html.ShouldContain("<html");
        html.ShouldContain("<header");
        html.ShouldContain("<footer");
        html.ShouldContain("</html>");
    }

    [Fact]
    public async Task IndexPageShouldContainHeroContent()
    {
        var query = CreateDefaultQuery();
        var props = new Dictionary<string, object?> { ["Query"] = query };
        var html = await RenderPageAsync<IndexPage>(props);

        html.ShouldContain("Atoll");
        html.ShouldContain(".NET-native static-site framework");
        html.ShouldContain("View on GitHub");
    }

    // ── Docs page tests ──

    [Fact]
    public async Task DocsPageShouldRenderMarkdownContent()
    {
        var query = CreateDefaultQuery();
        var props = new Dictionary<string, object?>
        {
            ["Slug"] = "getting-started",
            ["Query"] = query,
        };
        var html = await RenderPageAsync<DocsPage>(props);

        html.ShouldContain("Getting Started");
        html.ShouldContain("<h1");
        html.ShouldContain("Welcome to");
        html.ShouldContain("<strong>Atoll</strong>");
    }

    [Fact]
    public async Task DocsPageShouldShowSidebarNavigation()
    {
        var query = CreateDefaultQuery();
        var props = new Dictionary<string, object?>
        {
            ["Slug"] = "getting-started",
            ["Query"] = query,
        };
        var html = await RenderPageAsync<DocsPage>(props);

        html.ShouldContain("href=\"/docs/getting-started\"");
        html.ShouldContain("href=\"/docs/components\"");
        html.ShouldContain("href=\"/docs/content-collections\"");
    }

    [Fact]
    public async Task DocsPageShouldHighlightActiveNavItem()
    {
        var query = CreateDefaultQuery();
        var props = new Dictionary<string, object?>
        {
            ["Slug"] = "getting-started",
            ["Query"] = query,
        };
        var html = await RenderPageAsync<DocsPage>(props);

        // The active link should have aria-current="page"
        html.ShouldContain("aria-current=\"page\"");
        // And it should be on the getting-started link
        var activeIndex = html.IndexOf("aria-current=\"page\"", StringComparison.Ordinal);
        var linkStart = html.LastIndexOf("<a ", activeIndex, StringComparison.Ordinal);
        var linkText = html.Substring(linkStart, activeIndex - linkStart + 50);
        linkText.ShouldContain("getting-started");
    }

    [Fact]
    public async Task DocsPageShouldRenderNotFoundForInvalidSlug()
    {
        var query = CreateDefaultQuery();
        var props = new Dictionary<string, object?>
        {
            ["Slug"] = "nonexistent-page",
            ["Query"] = query,
        };
        var html = await RenderPageAsync<DocsPage>(props);

        // Should contain full layout structure (rendered through SiteLayout → DocsLayout)
        html.ShouldContain("<!DOCTYPE html>");
        html.ShouldContain("<html");
        html.ShouldContain("<header");
        html.ShouldContain("<footer");
        html.ShouldContain("</html>");

        // Should contain the styled 404 content (not bare unstyled HTML)
        html.ShouldContain("Page Not Found");
    }

    [Fact]
    public async Task DocsPageShouldProvideStaticPaths()
    {
        var query = CreateDefaultQuery();
        var page = new DocsPage { Query = query, Slug = "" };
        var paths = await page.GetStaticPathsAsync();

        paths.Count.ShouldBe(3);
        paths.Select(p => p.Parameters["slug"]).ShouldContain("getting-started");
        paths.Select(p => p.Parameters["slug"]).ShouldContain("components");
        paths.Select(p => p.Parameters["slug"]).ShouldContain("content-collections");
    }

    [Fact]
    public async Task DocsPageShouldGroupSidebarBySections()
    {
        var query = CreateDefaultQuery();
        var props = new Dictionary<string, object?>
        {
            ["Slug"] = "getting-started",
            ["Query"] = query,
        };
        var html = await RenderPageAsync<DocsPage>(props);

        // Should show section headings
        html.ShouldContain("Basics");
        html.ShouldContain("Features");
    }

    // ── 404 page tests ──

    private static InMemoryFileProvider CreateDocsContentWith404()
    {
        var provider = CreateDocsContent();
        provider.AddFile("content/docs", "404.md", """
            ---
            title: Custom Not Found
            description: This is a custom 404 page.
            order: 0
            section: ""
            ---

            # Custom Not Found

            The page you requested does not exist. Please check the URL or use the sidebar.
            """);
        return provider;
    }

    [Fact]
    public async Task DocsPageShouldExclude404SlugFromStaticPaths()
    {
        var query = CreateCollectionQuery(CreateDocsContentWith404());
        var page = new DocsPage { Query = query, Slug = "" };
        var paths = await page.GetStaticPathsAsync();

        paths.Count.ShouldBe(3);
        paths.Select(p => p.Parameters["slug"]).ShouldNotContain("404");
        paths.Select(p => p.Parameters["slug"]).ShouldContain("getting-started");
        paths.Select(p => p.Parameters["slug"]).ShouldContain("components");
        paths.Select(p => p.Parameters["slug"]).ShouldContain("content-collections");
    }

    [Fact]
    public async Task DocsPageShouldRenderCustom404ContentWhenFileExists()
    {
        var query = CreateCollectionQuery(CreateDocsContentWith404());
        var props = new Dictionary<string, object?>
        {
            ["Slug"] = "nonexistent-page",
            ["Query"] = query,
        };
        var html = await RenderPageAsync<DocsPage>(props);

        // Custom 404.md title and body content should appear
        html.ShouldContain("Custom Not Found");
        html.ShouldContain("The page you requested does not exist");

        // Full layout structure should be present
        html.ShouldContain("<!DOCTYPE html>");
        html.ShouldContain("<html");
        html.ShouldContain("<header");
        html.ShouldContain("<footer");
    }

    [Fact]
    public async Task DocsPageShouldRenderDefaultFallbackWhenNo404FileExists()
    {
        // Default content has no 404.md
        var query = CreateDefaultQuery();
        var props = new Dictionary<string, object?>
        {
            ["Slug"] = "nonexistent-page",
            ["Query"] = query,
        };
        var html = await RenderPageAsync<DocsPage>(props);

        // Default fallback content
        html.ShouldContain("Page Not Found");
        html.ShouldContain("Return to the documentation home");

        // Full layout structure should be present
        html.ShouldContain("<!DOCTYPE html>");
        html.ShouldContain("<html");
        html.ShouldContain("<header");
        html.ShouldContain("<footer");
    }

    [Fact]
    public async Task DocsPageShouldExclude404SlugFromSidebar()
    {
        var query = CreateCollectionQuery(CreateDocsContentWith404());
        var props = new Dictionary<string, object?>
        {
            ["Slug"] = "getting-started",
            ["Query"] = query,
        };
        var html = await RenderPageAsync<DocsPage>(props);

        // Sidebar should contain normal pages
        html.ShouldContain("href=\"/docs/getting-started\"");
        html.ShouldContain("href=\"/docs/components\"");

        // Sidebar should NOT contain a link to the 404 page
        html.ShouldNotContain("href=\"/docs/404\"");
    }

    [Fact]
    public async Task SearchIndexShouldExclude404Entry()
    {
        var query = CreateCollectionQuery(CreateDocsContentWith404());

        var searchGenerator = new LagoonSearchIndexGenerator(_outputDir);
        var config = new SearchConfig();
        var result = await searchGenerator.GenerateAsync(query, config, _ct);

        // Should only contain the 3 normal docs, not the 404 page
        result.EntryCount.ShouldBe(3);

        var json = await File.ReadAllTextAsync(Path.Combine(_outputDir, "search-index.json"));
        json.ShouldContain("Getting Started");
        json.ShouldContain("Components");
        json.ShouldContain("Content Collections");
        json.ShouldNotContain("Custom Not Found");
        json.ShouldNotContain("/docs/404");
    }

    [Fact]
    public async Task DocsPageShouldSetResponseStatusCodeTo404ForMissingSlug()
    {
        var query = CreateDefaultQuery();
        var page = new DocsPage();
        var props = new Dictionary<string, object?>
        {
            ["Slug"] = "nonexistent-page",
            ["Query"] = query,
        };

        var renderer = new PageRenderer();
        await renderer.RenderPageAsync(page, props);

        page.ResponseStatusCode.ShouldBe(404);
    }

    [Fact]
    public async Task DocsPageShouldSetResponseStatusCodeTo200ForValidSlug()
    {
        var query = CreateDefaultQuery();
        var page = new DocsPage();
        var props = new Dictionary<string, object?>
        {
            ["Slug"] = "getting-started",
            ["Query"] = query,
        };

        var renderer = new PageRenderer();
        await renderer.RenderPageAsync(page, props);

        page.ResponseStatusCode.ShouldBe(200);
    }

    // ── SSG tests ──

    [Fact]
    public async Task DocsSsgShouldGenerateAllPages()
    {
        var query = CreateDefaultQuery();
        var serviceProps = new Dictionary<string, object?> { ["Query"] = query };

        var routes = new RouteEntry[]
        {
            new RouteEntry("/", typeof(IndexPage), "index.cs"),
            new RouteEntry("/docs/[slug]", typeof(DocsPage), "docs/[slug].cs"),
        };

        var ssgOptions = new SsgOptions(_outputDir);
        var generator = new StaticSiteGenerator(ssgOptions, serviceProps);
        var result = await generator.GenerateAsync(routes, _ct);

        result.IsSuccess.ShouldBeTrue();
        // 1 index + 3 doc pages
        result.TotalCount.ShouldBe(4);

        File.Exists(Path.Combine(_outputDir, "index.html")).ShouldBeTrue();
        File.Exists(Path.Combine(_outputDir, "docs", "getting-started", "index.html")).ShouldBeTrue();
        File.Exists(Path.Combine(_outputDir, "docs", "components", "index.html")).ShouldBeTrue();
        File.Exists(Path.Combine(_outputDir, "docs", "content-collections", "index.html")).ShouldBeTrue();
    }

    [Fact]
    public async Task DocsSsgOutputShouldStartWithDoctype()
    {
        var query = CreateDefaultQuery();
        var serviceProps = new Dictionary<string, object?> { ["Query"] = query };

        var routes = new RouteEntry[]
        {
            new RouteEntry("/", typeof(IndexPage), "index.cs"),
            new RouteEntry("/docs/[slug]", typeof(DocsPage), "docs/[slug].cs"),
        };

        var ssgOptions = new SsgOptions(_outputDir);
        var generator = new StaticSiteGenerator(ssgOptions, serviceProps);
        var result = await generator.GenerateAsync(routes, _ct);

        foreach (var pageResult in result.PageResults)
        {
            pageResult.Html.ShouldStartWith(
                "<!DOCTYPE html>",
                customMessage: $"Page {pageResult.Route.UrlPath} missing DOCTYPE");
        }
    }

    [Fact]
    public async Task DocsSsgShouldGenerateNavigationAcrossAllPages()
    {
        var query = CreateDefaultQuery();
        var serviceProps = new Dictionary<string, object?> { ["Query"] = query };

        var routes = new RouteEntry[]
        {
            new RouteEntry("/", typeof(IndexPage), "index.cs"),
            new RouteEntry("/docs/[slug]", typeof(DocsPage), "docs/[slug].cs"),
        };

        var ssgOptions = new SsgOptions(_outputDir);
        var generator = new StaticSiteGenerator(ssgOptions, serviceProps);
        var result = await generator.GenerateAsync(routes, _ct);

        result.IsSuccess.ShouldBeTrue();

        foreach (var pageResult in result.PageResults)
        {
            pageResult.Html.ShouldContain(
                "href=\"/docs/getting-started\"",
                customMessage: $"Page {pageResult.Route.UrlPath} missing navigation link");
        }
    }

    // ── Search index integration tests ──

    [Fact]
    public async Task DocsSsgShouldGenerateSearchIndex()
    {
        var query = CreateDefaultQuery();
        var serviceProps = new Dictionary<string, object?> { ["Query"] = query };

        var routes = new RouteEntry[]
        {
            new RouteEntry("/", typeof(IndexPage), "index.cs"),
            new RouteEntry("/docs/[slug]", typeof(DocsPage), "docs/[slug].cs"),
        };

        var ssgOptions = new SsgOptions(_outputDir);
        var generator = new StaticSiteGenerator(ssgOptions, serviceProps);
        await generator.GenerateAsync(routes, _ct);

        // Run search index generation using the docs SearchConfig
        var searchGenerator = new LagoonSearchIndexGenerator(_outputDir);
        var config = new SearchConfig();
        var result = await searchGenerator.GenerateAsync(query, config, _ct);

        // Verify the file was written
        var searchIndexPath = Path.Combine(_outputDir, "search-index.json");
        File.Exists(searchIndexPath).ShouldBeTrue();

        // Verify result stats
        result.EntryCount.ShouldBe(3);
        result.OutputPath.ShouldBe(searchIndexPath);
        result.Elapsed.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
    }

    [Fact]
    public async Task SearchIndexShouldContainAllDocEntries()
    {
        var query = CreateDefaultQuery();

        var searchGenerator = new LagoonSearchIndexGenerator(_outputDir);
        var config = new SearchConfig();
        await searchGenerator.GenerateAsync(query, config, _ct);

        var json = await File.ReadAllTextAsync(Path.Combine(_outputDir, "search-index.json"));

        // All 3 doc titles should appear in the index
        json.ShouldContain("Getting Started");
        json.ShouldContain("Components");
        json.ShouldContain("Content Collections");
    }

    [Fact]
    public async Task SearchIndexShouldContainBodyWithNoHtmlTags()
    {
        var query = CreateDefaultQuery();

        var searchGenerator = new LagoonSearchIndexGenerator(_outputDir);
        var config = new SearchConfig();
        await searchGenerator.GenerateAsync(query, config, _ct);

        var json = await File.ReadAllTextAsync(Path.Combine(_outputDir, "search-index.json"));

        // The body text should not contain HTML tags
        json.ShouldNotContain("<p>");
        json.ShouldNotContain("<h1>");
        json.ShouldNotContain("<strong>");

        // But should contain actual content words
        json.ShouldContain("Welcome to");
        json.ShouldContain("Atoll");
    }

    [Fact]
    public async Task SearchIndexShouldHaveValidJsonSchema()
    {
        var query = CreateDefaultQuery();

        var searchGenerator = new LagoonSearchIndexGenerator(_outputDir);
        var config = new SearchConfig();
        await searchGenerator.GenerateAsync(query, config, _ct);

        var json = await File.ReadAllTextAsync(Path.Combine(_outputDir, "search-index.json"));

        // Verify top-level schema matches client expectations
        json.ShouldContain("\"entries\":");
        json.ShouldContain("\"generatedAt\":");

        // Verify entry property names are camelCase (checked by exact key:value pattern)
        json.ShouldContain("\"title\":\"");
        json.ShouldContain("\"href\":\"");
    }

    [Fact]
    public async Task SearchIndexHrefsShouldMatchExpectedPattern()
    {
        var query = CreateDefaultQuery();

        var searchGenerator = new LagoonSearchIndexGenerator(_outputDir);
        var config = new SearchConfig();
        await searchGenerator.GenerateAsync(query, config, _ct);

        var json = await File.ReadAllTextAsync(Path.Combine(_outputDir, "search-index.json"));

        // Hrefs should follow /docs/{slug} pattern
        json.ShouldContain("/docs/getting-started");
        json.ShouldContain("/docs/components");
        json.ShouldContain("/docs/content-collections");
    }

    // ── Global CSS discovery integration tests ──

    [Fact]
    public async Task AssetPipelineShouldIncludeDocsThemeCss()
    {
        var pipelineOptions = new AssetPipelineOptions(_outputDir)
        {
            Minify = false,
            Fingerprint = false,
        };
        var outputWriter = new OutputWriter(_outputDir);
        var pipeline = new AssetPipeline(pipelineOptions, outputWriter);

        var result = await pipeline.RunAsync(new[] { typeof(DocsTheme) }, Array.Empty<string>(), _ct);

        result.Css.HasContent.ShouldBeTrue();
        result.Css.Css.ShouldContain("--docs-bg");
        result.Css.Css.ShouldContain("--docs-text");
        result.Css.Css.ShouldContain(".docs-sidebar");
        result.Css.Css.ShouldContain(".prose");
    }

    [Fact]
    public void GlobalStyleDiscoveryShouldFindDocsTheme()
    {
        var result = GlobalStyleDiscovery.DiscoverGlobalStyles(typeof(DocsTheme).Assembly);

        result.ShouldContain(typeof(DocsTheme));
    }

    // ── Island asset integration tests ──

    [Fact]
    public async Task IslandAssetsShouldBeWrittenToOutputDirectory()
    {
        var provider = new LagoonIslandAssetProvider();
        var assets = provider.GetAssets().ToList();

        var writer = new IslandAssetWriter(_outputDir);
        var result = await writer.WriteAsync(assets, _ct);

        result.FileCount.ShouldBe(6);

        File.Exists(Path.Combine(_outputDir, "scripts", "atoll-docs-search-dialog.js")).ShouldBeTrue();
        File.Exists(Path.Combine(_outputDir, "scripts", "atoll-theme-toggle.js")).ShouldBeTrue();
        File.Exists(Path.Combine(_outputDir, "scripts", "atoll-docs-mobile-nav.js")).ShouldBeTrue();
        File.Exists(Path.Combine(_outputDir, "scripts", "atoll-sidebar-state.js")).ShouldBeTrue();
        File.Exists(Path.Combine(_outputDir, "scripts", "atoll-sidebar-resize.js")).ShouldBeTrue();
        File.Exists(Path.Combine(_outputDir, "scripts", "atoll-docs-tabs.js")).ShouldBeTrue();

        // Verify one file has expected content
        var searchDialogContent = await File.ReadAllTextAsync(
            Path.Combine(_outputDir, "scripts", "atoll-docs-search-dialog.js"));
        searchDialogContent.ShouldContain("Search Dialog");
    }

    // ── Per-page head injection tests ──

    private static CollectionQuery CreateQueryWithHeadInjectionContent()
    {
        var provider = CreateDocsContent();

        provider.AddFile("content/docs", "with-head-injection.md", """
            ---
            title: Head Injection Test
            description: Tests per-page head injection.
            order: 99
            section: Features
            head: |
              <meta property="og:title" content="Head Injection Test">
              <script src="/analytics.js"></script>
            ---

            # Head Injection Test

            This page has custom head content.
            """);

        return CreateCollectionQuery(provider);
    }

    [Fact]
    public async Task DocsPageShouldRenderPerPageHeadContent()
    {
        var query = CreateQueryWithHeadInjectionContent();
        var props = new Dictionary<string, object?>
        {
            ["Slug"] = "with-head-injection",
            ["Query"] = query,
        };
        var html = await RenderPageAsync<DocsPage>(props);

        // Head content should appear inside <head>
        var headStart = html.IndexOf("<head>", StringComparison.Ordinal);
        var headEnd = html.IndexOf("</head>", StringComparison.Ordinal);
        headStart.ShouldBeGreaterThanOrEqualTo(0);
        headEnd.ShouldBeGreaterThan(headStart);
        var headSection = html.Substring(headStart, headEnd - headStart);

        headSection.ShouldContain("<meta property=\"og:title\" content=\"Head Injection Test\">");
        headSection.ShouldContain("<script src=\"/analytics.js\"></script>");
    }

    [Fact]
    public async Task DocsPageWithoutHeadFrontmatterShouldRenderNormally()
    {
        var query = CreateQueryWithHeadInjectionContent();
        var props = new Dictionary<string, object?>
        {
            ["Slug"] = "getting-started",
            ["Query"] = query,
        };
        var html = await RenderPageAsync<DocsPage>(props);

        html.ShouldContain("Getting Started");
        html.ShouldContain("<h1");
        // Ensure no og:title meta leaked from the other page
        html.ShouldNotContain("og:title");
    }

    // ── End-to-end build test ──

    [Fact]
    public async Task FullBuildShouldIncludeCssAndIslandAssets()
    {
        var query = CreateDefaultQuery();
        var serviceProps = new Dictionary<string, object?> { ["Query"] = query };

        // 1. SSG
        var routes = new RouteEntry[]
        {
            new RouteEntry("/", typeof(IndexPage), "index.cs"),
            new RouteEntry("/docs/[slug]", typeof(DocsPage), "docs/[slug].cs"),
        };
        var ssgOptions = new SsgOptions(_outputDir);
        var generator = new StaticSiteGenerator(ssgOptions, serviceProps);
        var ssgResult = await generator.GenerateAsync(routes, _ct);
        ssgResult.IsSuccess.ShouldBeTrue();

        // 2. Asset pipeline with DocsTheme CSS
        var globalStyleTypes = GlobalStyleDiscovery.DiscoverGlobalStyles(typeof(DocsTheme).Assembly);
        var pipelineOptions = new AssetPipelineOptions(_outputDir)
        {
            Minify = false,
            Fingerprint = false,
        };
        var outputWriter = new OutputWriter(_outputDir);
        var pipeline = new AssetPipeline(pipelineOptions, outputWriter);
        var assetResult = await pipeline.RunAsync(globalStyleTypes, Array.Empty<string>(), _ct);

        // 3. Island assets
        var islandProvider = new LagoonIslandAssetProvider();
        var islandWriter = new IslandAssetWriter(_outputDir);
        var islandResult = await islandWriter.WriteAsync(islandProvider.GetAssets(), _ct);

        // Assert CSS
        assetResult.Css.HasContent.ShouldBeTrue();
        assetResult.Css.Css.ShouldContain("--docs-bg");

        // Assert island JS
        islandResult.FileCount.ShouldBe(6);
        File.Exists(Path.Combine(_outputDir, "scripts", "atoll-docs-search-dialog.js")).ShouldBeTrue();
        File.Exists(Path.Combine(_outputDir, "scripts", "atoll-theme-toggle.js")).ShouldBeTrue();
        File.Exists(Path.Combine(_outputDir, "scripts", "atoll-docs-mobile-nav.js")).ShouldBeTrue();
        File.Exists(Path.Combine(_outputDir, "scripts", "atoll-sidebar-state.js")).ShouldBeTrue();
        File.Exists(Path.Combine(_outputDir, "scripts", "atoll-sidebar-resize.js")).ShouldBeTrue();
        File.Exists(Path.Combine(_outputDir, "scripts", "atoll-docs-tabs.js")).ShouldBeTrue();

        // Assert HTML pages (no regression)
        File.Exists(Path.Combine(_outputDir, "index.html")).ShouldBeTrue();
        File.Exists(Path.Combine(_outputDir, "docs", "getting-started", "index.html")).ShouldBeTrue();
    }
}

