using Atoll.Build.Ssg;
using Atoll.Components;
using Atoll.Content.Collections;
using Atoll.Docs;
using Atoll.Docs.Pages;
using Atoll.Rendering;
using Atoll.Routing;
using Shouldly;
using Xunit;

namespace Atoll.Integration.Tests;

/// <summary>
/// End-to-end integration tests for the Atoll documentation site.
/// Verifies content collections, page rendering, layout, sidebar navigation,
/// static path generation, and SSG output.
/// </summary>
public sealed class DocsSampleTests : IDisposable
{
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
    public async Task IndexPageShouldContainFeatureHighlights()
    {
        var query = CreateDefaultQuery();
        var props = new Dictionary<string, object?> { ["Query"] = query };
        var html = await RenderPageAsync<IndexPage>(props);

        html.ShouldContain("Content Collections");
        html.ShouldContain("Islands Architecture");
        html.ShouldContain("Static Site Generation");
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
        var result = await generator.GenerateAsync(routes);

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
        var result = await generator.GenerateAsync(routes);

        foreach (var pageResult in result.PageResults)
        {
            pageResult.Html.ShouldStartWith(
                "<!DOCTYPE html>",
                customMessage: $"Page {pageResult.Route.UrlPath} missing DOCTYPE");
        }
    }

    [Fact]
    public async Task DocsSsgOutputShouldHaveNavigationOnAllDocPages()
    {
        var query = CreateDefaultQuery();
        var serviceProps = new Dictionary<string, object?> { ["Query"] = query };

        var routes = new RouteEntry[]
        {
            new RouteEntry("/docs/[slug]", typeof(DocsPage), "docs/[slug].cs"),
        };

        var ssgOptions = new SsgOptions(_outputDir);
        var generator = new StaticSiteGenerator(ssgOptions, serviceProps);
        var result = await generator.GenerateAsync(routes);

        result.IsSuccess.ShouldBeTrue();

        foreach (var pageResult in result.PageResults)
        {
            // Every doc page should have sidebar navigation links
            pageResult.Html.ShouldContain(
                "href=\"/docs/getting-started\"",
                customMessage: $"Page {pageResult.Route.UrlPath} missing sidebar nav");
        }
    }
}
