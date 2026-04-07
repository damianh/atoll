using System.Net;
using Atoll.Build.Content.Collections;
using Atoll.Components;
using Atoll.Rendering;
using Atoll.Middleware.Server.Hosting;
using Atoll.Samples.Blog;
using Atoll.Samples.Blog.Components;
using Atoll.Samples.Blog.Islands;
using Atoll.Samples.Blog.Pages;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Atoll.Integration.Tests;

/// <summary>
/// End-to-end integration tests for the blog sample site.
/// Verifies that all pages, components, layouts, content collections,
/// islands, and tag filtering work correctly through the full rendering pipeline.
/// </summary>
public sealed class BlogSampleTests
{
    // ── Content setup helpers ──

    private static InMemoryFileProvider CreateBlogContent()
    {
        var provider = new InMemoryFileProvider();

        provider.AddFile("content/blog", "getting-started.md", """
            ---
            title: Getting Started with Atoll
            description: Learn how to build your first site.
            pubDate: 2026-01-15
            author: Jane Developer
            tags: atoll, tutorial, getting-started
            draft: false
            ---

            # Getting Started

            Welcome to **Atoll**!
            """);

        provider.AddFile("content/blog", "islands-architecture.md", """
            ---
            title: Understanding Islands Architecture
            description: Explore islands architecture with minimal JavaScript.
            pubDate: 2026-02-10
            author: Jane Developer
            tags: atoll, islands, architecture
            draft: false
            ---

            # Islands Architecture

            Islands are interactive components.
            """);

        provider.AddFile("content/blog", "content-collections.md", """
            ---
            title: Working with Content Collections
            description: Type-safe Markdown content with frontmatter validation.
            pubDate: 2026-03-05
            author: John Writer
            tags: atoll, content, markdown
            draft: false
            ---

            # Content Collections

            Content collections bring type-safety.
            """);

        provider.AddFile("content/blog", "draft-upcoming.md", """
            ---
            title: "Draft: Upcoming Features"
            description: A sneak peek at what is coming.
            pubDate: 2026-04-01
            author: Jane Developer
            tags: atoll, roadmap
            draft: true
            ---

            # Upcoming Features

            This is a draft.
            """);

        return provider;
    }

    private static CollectionQuery CreateCollectionQuery(InMemoryFileProvider fileProvider)
    {
        var config = new CollectionConfig("content")
            .AddCollection(ContentCollection.Define<BlogPostSchema>("blog"));
        var loader = new CollectionLoader(config, fileProvider);
        return new CollectionQuery(loader);
    }

    private static CollectionQuery CreateDefaultQuery()
    {
        return CreateCollectionQuery(CreateBlogContent());
    }

    // ── Helper to render a page to HTML string ──

    private static async Task<string> RenderPageAsync<TPage>(
        IReadOnlyDictionary<string, object?>? props = null)
        where TPage : IAtollComponent, new()
    {
        var renderer = new PageRenderer();
        var pageProps = props ?? new Dictionary<string, object?>();

        // Check if layout wrapping is needed
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

    private static async Task<string> RenderComponentAsync<TComponent>(
        IReadOnlyDictionary<string, object?>? props = null)
        where TComponent : IAtollComponent, new()
    {
        var dest = new StringRenderDestination();
        var componentProps = props ?? new Dictionary<string, object?>();
        await ComponentRenderer.RenderComponentAsync<TComponent>(dest, componentProps);
        return dest.GetOutput();
    }

    // ── Helper to render via RenderContext ──

    private static RenderContext CreateContextWithDestination(
        StringRenderDestination dest,
        IReadOnlyDictionary<string, object?>? props = null)
    {
        return new RenderContext(dest, props ?? new Dictionary<string, object?>());
    }

    // ── Blog schema tests ──

    [Fact]
    public void BlogPostSchemaShouldParseTagsFromCommaSeparatedString()
    {
        var schema = new BlogPostSchema { Tags = "atoll, tutorial, getting-started" };
        var tags = schema.GetTags();
        tags.Length.ShouldBe(3);
        tags[0].ShouldBe("atoll");
        tags[1].ShouldBe("tutorial");
        tags[2].ShouldBe("getting-started");
    }

    [Fact]
    public void BlogPostSchemaShouldReturnEmptyArrayForEmptyTags()
    {
        var schema = new BlogPostSchema { Tags = "" };
        schema.GetTags().ShouldBeEmpty();
    }

    [Fact]
    public void BlogPostSchemaShouldReturnEmptyArrayForWhitespaceTags()
    {
        var schema = new BlogPostSchema { Tags = "   " };
        schema.GetTags().ShouldBeEmpty();
    }

    [Fact]
    public void BlogPostSchemaShouldTrimTagWhitespace()
    {
        var schema = new BlogPostSchema { Tags = " atoll , tutorial " };
        var tags = schema.GetTags();
        tags.Length.ShouldBe(2);
        tags[0].ShouldBe("atoll");
        tags[1].ShouldBe("tutorial");
    }

    // ── Layout rendering tests ──

    [Fact]
    public async Task BlogLayoutShouldRenderHtmlStructure()
    {
        var html = await RenderPageAsync<IndexPage>();

        html.ShouldContain("<!DOCTYPE html>");
        html.ShouldContain("<html lang=\"en\">");
        html.ShouldContain("<meta charset=\"utf-8\"");
        html.ShouldContain("<header");
        html.ShouldContain("Atoll Blog");
        html.ShouldContain("<nav");
        html.ShouldContain("<main");
        html.ShouldContain("<footer");
        html.ShouldContain("</html>");
    }

    [Fact]
    public async Task BlogLayoutShouldContainNavigationLinks()
    {
        var html = await RenderPageAsync<IndexPage>();

        html.ShouldContain("href=\"/\"");
        html.ShouldContain("href=\"/blog\"");
        html.ShouldContain("href=\"/tags\"");
        html.ShouldContain("href=\"/about\"");
    }

    [Fact]
    public async Task BlogLayoutShouldRenderFooter()
    {
        var html = await RenderPageAsync<IndexPage>();
        html.ShouldContain("Built with Atoll");
    }

    // ── Index page tests ──

    [Fact]
    public async Task IndexPageShouldRenderWelcomeMessage()
    {
        var html = await RenderPageAsync<IndexPage>();

        html.ShouldContain("Welcome to Atoll Blog");
        html.ShouldContain("Read the Blog");
        html.ShouldContain("href=\"/blog\"");
    }

    // ── About page tests ──

    [Fact]
    public async Task AboutPageShouldRenderContent()
    {
        var html = await RenderPageAsync<AboutPage>();

        html.ShouldContain("About This Blog");
        html.ShouldContain("Server-first rendering");
        html.ShouldContain("Islands architecture");
        html.ShouldContain("Content collections");
    }

    // ── Blog index page tests ──

    [Fact]
    public async Task BlogIndexPageShouldRenderPublishedPosts()
    {
        var query = CreateDefaultQuery();
        var props = new Dictionary<string, object?> { ["Query"] = query };
        var html = await RenderPageAsync<BlogIndexPage>(props);

        // Should show 3 published posts
        html.ShouldContain("Getting Started with Atoll");
        html.ShouldContain("Understanding Islands Architecture");
        html.ShouldContain("Working with Content Collections");
    }

    [Fact]
    public async Task BlogIndexPageShouldExcludeDraftPosts()
    {
        var query = CreateDefaultQuery();
        var props = new Dictionary<string, object?> { ["Query"] = query };
        var html = await RenderPageAsync<BlogIndexPage>(props);

        html.ShouldNotContain("Upcoming Features");
        html.ShouldNotContain("draft-upcoming");
    }

    [Fact]
    public async Task BlogIndexPageShouldRenderPostCards()
    {
        var query = CreateDefaultQuery();
        var props = new Dictionary<string, object?> { ["Query"] = query };
        var html = await RenderPageAsync<BlogIndexPage>(props);

        // Should have post card article elements
        html.ShouldContain("<article");
        // Should have links to individual posts
        html.ShouldContain("href=\"/blog/getting-started\"");
        html.ShouldContain("href=\"/blog/islands-architecture\"");
        html.ShouldContain("href=\"/blog/content-collections\"");
    }

    [Fact]
    public async Task BlogIndexPageShouldShowPostDescriptions()
    {
        var query = CreateDefaultQuery();
        var props = new Dictionary<string, object?> { ["Query"] = query };
        var html = await RenderPageAsync<BlogIndexPage>(props);

        html.ShouldContain("Learn how to build your first site.");
        html.ShouldContain("Explore islands architecture with minimal JavaScript.");
        html.ShouldContain("Type-safe Markdown content with frontmatter validation.");
    }

    [Fact]
    public async Task BlogIndexPageShouldSortByDateDescending()
    {
        var query = CreateDefaultQuery();
        var props = new Dictionary<string, object?> { ["Query"] = query };
        var html = await RenderPageAsync<BlogIndexPage>(props);

        // Content Collections (March 5) should appear before Islands (Feb 10) should appear before Getting Started (Jan 15)
        var contentPos = html.IndexOf("Content Collections", StringComparison.Ordinal);
        var islandsPos = html.IndexOf("Islands Architecture", StringComparison.Ordinal);
        var gettingStartedPos = html.IndexOf("Getting Started", StringComparison.Ordinal);

        contentPos.ShouldBeLessThan(islandsPos);
        islandsPos.ShouldBeLessThan(gettingStartedPos);
    }

    [Fact]
    public async Task BlogIndexPageShouldShowTagLinks()
    {
        var query = CreateDefaultQuery();
        var props = new Dictionary<string, object?> { ["Query"] = query };
        var html = await RenderPageAsync<BlogIndexPage>(props);

        html.ShouldContain("href=\"/tags/atoll\"");
        html.ShouldContain("href=\"/tags/tutorial\"");
    }

    [Fact]
    public async Task BlogIndexPageShouldHandleEmptyCollection()
    {
        var emptyProvider = new InMemoryFileProvider();
        var query = CreateCollectionQuery(emptyProvider);
        var props = new Dictionary<string, object?> { ["Query"] = query };
        var html = await RenderPageAsync<BlogIndexPage>(props);

        html.ShouldContain("No posts yet");
    }

    // ── Blog post page tests ──

    [Fact]
    public async Task BlogPostPageShouldRenderMarkdownContent()
    {
        var query = CreateDefaultQuery();
        var props = new Dictionary<string, object?>
        {
            ["Slug"] = "getting-started",
            ["Query"] = query,
        };
        var html = await RenderPageAsync<BlogPostPage>(props);

        html.ShouldContain("Getting Started with Atoll");
        html.ShouldContain("<h1");
        html.ShouldContain("Welcome to");
        html.ShouldContain("<strong>Atoll</strong>");
    }

    [Fact]
    public async Task BlogPostPageShouldShowPostMetadata()
    {
        var query = CreateDefaultQuery();
        var props = new Dictionary<string, object?>
        {
            ["Slug"] = "getting-started",
            ["Query"] = query,
        };
        var html = await RenderPageAsync<BlogPostPage>(props);

        html.ShouldContain("January 15, 2026");
        html.ShouldContain("Jane Developer");
    }

    [Fact]
    public async Task BlogPostPageShouldShowTagBadges()
    {
        var query = CreateDefaultQuery();
        var props = new Dictionary<string, object?>
        {
            ["Slug"] = "getting-started",
            ["Query"] = query,
        };
        var html = await RenderPageAsync<BlogPostPage>(props);

        html.ShouldContain("href=\"/tags/atoll\"");
        html.ShouldContain("href=\"/tags/tutorial\"");
        html.ShouldContain("href=\"/tags/getting-started\"");
    }

    [Fact]
    public async Task BlogPostPageShouldRenderNotFoundForInvalidSlug()
    {
        var query = CreateDefaultQuery();
        var props = new Dictionary<string, object?>
        {
            ["Slug"] = "nonexistent-post",
            ["Query"] = query,
        };
        var html = await RenderPageAsync<BlogPostPage>(props);

        html.ShouldContain("Post Not Found");
    }

    [Fact]
    public async Task BlogPostPageShouldWrapContentInLayout()
    {
        var query = CreateDefaultQuery();
        var props = new Dictionary<string, object?>
        {
            ["Slug"] = "getting-started",
            ["Query"] = query,
        };
        var html = await RenderPageAsync<BlogPostPage>(props);

        // Should have layout structure
        html.ShouldContain("<header");
        html.ShouldContain("<footer");
        html.ShouldContain("<nav");
    }

    [Fact]
    public async Task BlogPostPageShouldProvideStaticPaths()
    {
        var query = CreateDefaultQuery();
        var page = new BlogPostPage { Query = query, Slug = "" };
        var paths = await page.GetStaticPathsAsync();

        // Should have 3 paths (excludes draft)
        paths.Count.ShouldBe(3);
        paths.Select(p => p.Parameters["slug"]).ShouldContain("getting-started");
        paths.Select(p => p.Parameters["slug"]).ShouldContain("islands-architecture");
        paths.Select(p => p.Parameters["slug"]).ShouldContain("content-collections");
    }

    [Fact]
    public async Task BlogPostPageStaticPathsShouldExcludeDrafts()
    {
        var query = CreateDefaultQuery();
        var page = new BlogPostPage { Query = query, Slug = "" };
        var paths = await page.GetStaticPathsAsync();

        paths.Select(p => p.Parameters["slug"]).ShouldNotContain("draft-upcoming");
    }

    // ── Tags index page tests ──

    [Fact]
    public async Task TagsIndexPageShouldListAllUniqueTags()
    {
        var query = CreateDefaultQuery();
        var props = new Dictionary<string, object?> { ["Query"] = query };
        var html = await RenderPageAsync<TagsIndexPage>(props);

        html.ShouldContain("All Tags");
        html.ShouldContain("atoll");
        html.ShouldContain("tutorial");
        html.ShouldContain("getting-started");
        html.ShouldContain("islands");
        html.ShouldContain("architecture");
        html.ShouldContain("content");
        html.ShouldContain("markdown");
    }

    [Fact]
    public async Task TagsIndexPageShouldExcludeTagsFromDraftPosts()
    {
        var query = CreateDefaultQuery();
        var props = new Dictionary<string, object?> { ["Query"] = query };
        var html = await RenderPageAsync<TagsIndexPage>(props);

        // "roadmap" tag is only on the draft post
        html.ShouldNotContain("roadmap");
    }

    [Fact]
    public async Task TagsIndexPageShouldShowTagCounts()
    {
        var query = CreateDefaultQuery();
        var props = new Dictionary<string, object?> { ["Query"] = query };
        var html = await RenderPageAsync<TagsIndexPage>(props);

        // "atoll" tag appears in all 3 published posts
        html.ShouldContain("(3)");
    }

    [Fact]
    public async Task TagsIndexPageShouldHandleNoTags()
    {
        var emptyProvider = new InMemoryFileProvider();
        var query = CreateCollectionQuery(emptyProvider);
        var props = new Dictionary<string, object?> { ["Query"] = query };
        var html = await RenderPageAsync<TagsIndexPage>(props);

        html.ShouldContain("No tags found");
    }

    // ── Tag page (filtered listing) tests ──

    [Fact]
    public async Task TagPageShouldFilterPostsByTag()
    {
        var query = CreateDefaultQuery();
        var props = new Dictionary<string, object?>
        {
            ["Tag"] = "tutorial",
            ["Query"] = query,
        };
        var html = await RenderPageAsync<TagPage>(props);

        html.ShouldContain("Posts tagged: tutorial");
        html.ShouldContain("Getting Started with Atoll");
        // Islands and Content Collections don't have "tutorial" tag
        html.ShouldNotContain("Understanding Islands Architecture");
        html.ShouldNotContain("Working with Content Collections");
    }

    [Fact]
    public async Task TagPageShouldShowAllPostsForSharedTag()
    {
        var query = CreateDefaultQuery();
        var props = new Dictionary<string, object?>
        {
            ["Tag"] = "atoll",
            ["Query"] = query,
        };
        var html = await RenderPageAsync<TagPage>(props);

        // All 3 published posts have the "atoll" tag
        html.ShouldContain("Getting Started with Atoll");
        html.ShouldContain("Understanding Islands Architecture");
        html.ShouldContain("Working with Content Collections");
    }

    [Fact]
    public async Task TagPageShouldHandleNoMatchingPosts()
    {
        var query = CreateDefaultQuery();
        var props = new Dictionary<string, object?>
        {
            ["Tag"] = "nonexistent-tag",
            ["Query"] = query,
        };
        var html = await RenderPageAsync<TagPage>(props);

        html.ShouldContain("No posts found with this tag");
    }

    [Fact]
    public async Task TagPageShouldProvideStaticPaths()
    {
        var query = CreateDefaultQuery();
        var page = new TagPage { Query = query, Tag = "" };
        var paths = await page.GetStaticPathsAsync();

        // Should have unique tags from published posts
        paths.Count.ShouldBeGreaterThan(0);
        var tagSlugs = paths.Select(p => p.Parameters["tag"]).ToList();
        tagSlugs.ShouldContain("atoll");
        tagSlugs.ShouldContain("tutorial");
        tagSlugs.ShouldContain("islands");
    }

    [Fact]
    public async Task TagPageStaticPathsShouldExcludeTagsFromDrafts()
    {
        var query = CreateDefaultQuery();
        var page = new TagPage { Query = query, Tag = "" };
        var paths = await page.GetStaticPathsAsync();

        var tagSlugs = paths.Select(p => p.Parameters["tag"]).ToList();
        tagSlugs.ShouldNotContain("roadmap");
    }

    [Fact]
    public async Task TagPageShouldShowBackLink()
    {
        var query = CreateDefaultQuery();
        var props = new Dictionary<string, object?>
        {
            ["Tag"] = "atoll",
            ["Query"] = query,
        };
        var html = await RenderPageAsync<TagPage>(props);

        html.ShouldContain("href=\"/tags\"");
        html.ShouldContain("All tags");
    }

    // ── PostCard component tests ──

    [Fact]
    public async Task PostCardShouldRenderArticleElement()
    {
        var props = new Dictionary<string, object?>
        {
            ["Title"] = "Test Post",
            ["Slug"] = "test-post",
            ["Description"] = "A test description.",
            ["Date"] = "January 1, 2026",
            ["Tags"] = "test, sample",
        };
        var html = await RenderComponentAsync<PostCard>(props);

        html.ShouldContain("<article");
        html.ShouldContain("Test Post");
        html.ShouldContain("href=\"/blog/test-post\"");
        html.ShouldContain("A test description.");
        html.ShouldContain("January 1, 2026");
        html.ShouldContain("href=\"/tags/test\"");
        html.ShouldContain("href=\"/tags/sample\"");
    }

    [Fact]
    public async Task PostCardShouldSkipEmptyOptionalFields()
    {
        var props = new Dictionary<string, object?>
        {
            ["Title"] = "Minimal Post",
            ["Slug"] = "minimal",
            ["Description"] = "",
            ["Date"] = "",
            ["Tags"] = "",
        };
        var html = await RenderComponentAsync<PostCard>(props);

        html.ShouldContain("Minimal Post");
        html.ShouldNotContain("<time");
        html.ShouldNotContain("href=\"/tags/");
    }

    // ── TagBadge component tests ──

    [Fact]
    public async Task TagBadgeShouldRenderAsLink()
    {
        var props = new Dictionary<string, object?> { ["Tag"] = "Atoll" };
        var html = await RenderComponentAsync<TagBadge>(props);

        html.ShouldContain("href=\"/tags/atoll\""); // lowercase
        html.ShouldContain("Atoll"); // preserves display casing
    }

    // ── Island component tests ──

    [Fact]
    public async Task ThemeToggleShouldRenderButton()
    {
        var html = await RenderComponentAsync<ThemeToggle>();

        html.ShouldContain("<button");
        html.ShouldContain("id=\"theme-toggle\"");
        html.ShouldContain("Toggle theme");
    }

    [Fact]
    public void ThemeToggleShouldHaveClientLoadDirective()
    {
        var directive = Atoll.Islands.DirectiveExtractor.GetDirective(typeof(ThemeToggle));
        directive.ShouldNotBeNull();
        directive.DirectiveType.ShouldBe(Atoll.Instructions.ClientDirectiveType.Load);
    }

    [Fact]
    public void ThemeToggleShouldProvideClientModuleUrl()
    {
        var toggle = new ThemeToggle();
        toggle.ClientModuleUrl.ShouldBe("/scripts/theme-toggle.js");
    }

    [Fact]
    public async Task SearchBoxShouldRenderInput()
    {
        var html = await RenderComponentAsync<SearchBox>();

        html.ShouldContain("<input");
        html.ShouldContain("type=\"search\"");
        html.ShouldContain("id=\"search-input\"");
        html.ShouldContain("Search posts...");
    }

    [Fact]
    public async Task SearchBoxShouldRenderCustomPlaceholder()
    {
        var props = new Dictionary<string, object?> { ["Placeholder"] = "Find articles..." };
        var html = await RenderComponentAsync<SearchBox>(props);

        html.ShouldContain("Find articles...");
    }

    [Fact]
    public void SearchBoxShouldHaveClientLoadDirective()
    {
        var directive = Atoll.Islands.DirectiveExtractor.GetDirective(typeof(SearchBox));
        directive.ShouldNotBeNull();
        directive.DirectiveType.ShouldBe(Atoll.Instructions.ClientDirectiveType.Load);
    }

    [Fact]
    public void SearchBoxShouldProvideClientModuleUrl()
    {
        var search = new SearchBox();
        search.ClientModuleUrl.ShouldBe("/scripts/search.js");
    }

    // ── Content collection integration tests ──

    [Fact]
    public void ContentCollectionShouldLoadPublishedPosts()
    {
        var query = CreateDefaultQuery();
        var posts = query.GetCollection<BlogPostSchema>("blog",
            entry => !entry.Data.Draft);

        posts.Count.ShouldBe(3);
    }

    [Fact]
    public void ContentCollectionShouldLoadDraftPosts()
    {
        var query = CreateDefaultQuery();
        var drafts = query.GetCollection<BlogPostSchema>("blog",
            entry => entry.Data.Draft);

        drafts.Count.ShouldBe(1);
        drafts[0].Data.Title.ShouldBe("Draft: Upcoming Features");
    }

    [Fact]
    public void ContentCollectionShouldGetEntryBySlug()
    {
        var query = CreateDefaultQuery();
        var entry = query.GetEntry<BlogPostSchema>("blog", "getting-started");

        entry.ShouldNotBeNull();
        entry.Data.Title.ShouldBe("Getting Started with Atoll");
        entry.Data.Author.ShouldBe("Jane Developer");
        entry.Slug.ShouldBe("getting-started");
    }

    [Fact]
    public void ContentCollectionShouldReturnNullForMissingEntry()
    {
        var query = CreateDefaultQuery();
        var entry = query.GetEntry<BlogPostSchema>("blog", "nonexistent");
        entry.ShouldBeNull();
    }

    [Fact]
    public void ContentCollectionShouldRenderMarkdownToHtml()
    {
        var query = CreateDefaultQuery();
        var entry = query.GetEntry<BlogPostSchema>("blog", "getting-started")!;
        var rendered = query.Render(entry);

        rendered.Html.ShouldContain("<h1");
        rendered.Html.ShouldContain("Getting Started");
        rendered.Html.ShouldContain("<strong>Atoll</strong>");
    }

    // ── ASP.NET Core middleware integration tests ──

    private static HttpClient CreateBlogTestClient()
    {
        var fileProvider = CreateBlogContent();
        var query = CreateCollectionQuery(fileProvider);

        var builder = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost.UseTestServer();
                webHost.ConfigureServices(services =>
                {
                    services.AddAtoll(options =>
                    {
                        options.RouteEntries.Add(("index.cs", typeof(IndexPage)));
                        options.RouteEntries.Add(("about.cs", typeof(AboutPage)));
                        options.RouteEntries.Add(("blog/index.cs", typeof(BlogIndexPage)));
                        options.RouteEntries.Add(("blog/[slug].cs", typeof(BlogPostPage)));
                        options.RouteEntries.Add(("tags/index.cs", typeof(TagsIndexPage)));
                        options.RouteEntries.Add(("tags/[tag].cs", typeof(TagPage)));
                    });
                    services.AddSingleton(query);
                    services.AddLogging();
                });
                webHost.Configure(app =>
                {
                    app.UseAtoll();
                });
            });

        var host = builder.Start();
        return host.GetTestClient();
    }

    [Fact]
    public async Task MiddlewareShouldServeHomePage()
    {
        using var client = CreateBlogTestClient();
        var response = await client.GetAsync("/");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var html = await response.Content.ReadAsStringAsync();
        html.ShouldContain("Welcome to Atoll Blog");
    }

    [Fact]
    public async Task MiddlewareShouldServeAboutPage()
    {
        using var client = CreateBlogTestClient();
        var response = await client.GetAsync("/about");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var html = await response.Content.ReadAsStringAsync();
        html.ShouldContain("About This Blog");
    }

    // Note: Middleware tests for pages that require CollectionQuery (BlogIndexPage,
    // BlogPostPage, TagsIndexPage, TagPage) are not included here because the
    // AtollRequestHandler currently only passes route parameters as props — it does
    // not inject DI-registered services. Those pages are thoroughly tested via the
    // direct rendering helpers above. DI prop injection is tracked as a future enhancement.

    [Fact]
    public async Task MiddlewareShouldReturn404ForUnknownRoute()
    {
        using var client = CreateBlogTestClient();
        var response = await client.GetAsync("/nonexistent");
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task MiddlewareShouldReturnHtmlContentType()
    {
        using var client = CreateBlogTestClient();
        var response = await client.GetAsync("/");
        response.Content.Headers.ContentType!.MediaType.ShouldBe("text/html");
    }

    [Fact]
    public async Task MiddlewareShouldReturnDoctypeInResponse()
    {
        using var client = CreateBlogTestClient();
        var response = await client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();
        html.ShouldStartWith("<!DOCTYPE html>");
    }

    [Fact]
    public async Task MiddlewareShouldReturn200ForAboutPage()
    {
        using var client = CreateBlogTestClient();
        var response = await client.GetAsync("/about");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.ShouldBe("text/html");
    }

    // ── Content collection → rendered page round-trip tests ──

    [Fact]
    public async Task ContentCollectionRoundTripShouldRenderPostWithMarkdownHeadings()
    {
        var query = CreateDefaultQuery();
        var entry = query.GetEntry<BlogPostSchema>("blog", "getting-started")!;
        var rendered = query.Render(entry);

        // Verify the rendered content has proper heading HTML
        rendered.Html.ShouldContain("<h1");
        rendered.Html.ShouldContain("Getting Started");

        // Now render the full page with this content
        var props = new Dictionary<string, object?>
        {
            ["Slug"] = "getting-started",
            ["Query"] = query,
        };
        var html = await RenderPageAsync<BlogPostPage>(props);

        // Full round-trip: content file → frontmatter parse → markdown render → page component → layout
        html.ShouldContain("<!DOCTYPE html>");
        html.ShouldContain("<article");
        html.ShouldContain("<h1");
        html.ShouldContain("Getting Started with Atoll");
        html.ShouldContain("<strong>Atoll</strong>");
        html.ShouldContain("Jane Developer");
        html.ShouldContain("January 15, 2026");
        html.ShouldContain("<header"); // layout header
        html.ShouldContain("<footer"); // layout footer
    }

    [Fact]
    public async Task ContentCollectionRoundTripShouldRenderDifferentPosts()
    {
        var query = CreateDefaultQuery();
        var slugs = new[] { "getting-started", "islands-architecture", "content-collections" };

        foreach (var slug in slugs)
        {
            var props = new Dictionary<string, object?>
            {
                ["Slug"] = slug,
                ["Query"] = query,
            };
            var html = await RenderPageAsync<BlogPostPage>(props);
            html.ShouldContain("<!DOCTYPE html>", customMessage: $"Post {slug} missing DOCTYPE");
            html.ShouldContain("<article", customMessage: $"Post {slug} missing article");
        }
    }

    [Fact]
    public async Task ContentCollectionRoundTripShouldFilterAndSortPosts()
    {
        var query = CreateDefaultQuery();
        var props = new Dictionary<string, object?> { ["Query"] = query };
        var html = await RenderPageAsync<BlogIndexPage>(props);

        // Should have 3 published posts (excludes draft)
        html.ShouldContain("Getting Started with Atoll");
        html.ShouldContain("Understanding Islands Architecture");
        html.ShouldContain("Working with Content Collections");
        html.ShouldNotContain("Upcoming Features");

        // Sorted newest first: Content Collections (Mar) > Islands (Feb) > Getting Started (Jan)
        var ccPos = html.IndexOf("Content Collections", StringComparison.Ordinal);
        var islandsPos = html.IndexOf("Islands Architecture", StringComparison.Ordinal);
        var gsPos = html.IndexOf("Getting Started", StringComparison.Ordinal);
        ccPos.ShouldBeLessThan(islandsPos);
        islandsPos.ShouldBeLessThan(gsPos);
    }

    [Fact]
    public async Task ContentCollectionRoundTripShouldGenerateTagPagesFromContent()
    {
        var query = CreateDefaultQuery();

        // Verify static paths are generated from content
        var tagPage = new TagPage { Query = query, Tag = "" };
        var paths = await tagPage.GetStaticPathsAsync();
        paths.Count.ShouldBeGreaterThan(0);

        // Render a specific tag page
        var props = new Dictionary<string, object?>
        {
            ["Tag"] = "atoll",
            ["Query"] = query,
        };
        var html = await RenderPageAsync<TagPage>(props);

        // All 3 published posts have the "atoll" tag
        html.ShouldContain("Getting Started with Atoll");
        html.ShouldContain("Understanding Islands Architecture");
        html.ShouldContain("Working with Content Collections");
    }

    [Fact]
    public void ContentCollectionShouldPreserveFrontmatterMetadata()
    {
        var query = CreateDefaultQuery();
        var entries = query.GetCollection<BlogPostSchema>("blog",
            entry => !entry.Data.Draft);

        foreach (var entry in entries)
        {
            entry.Data.Title.ShouldNotBeNullOrEmpty(customMessage: $"Slug {entry.Slug} missing title");
            entry.Data.Description.ShouldNotBeNullOrEmpty(customMessage: $"Slug {entry.Slug} missing description");
            entry.Data.Author.ShouldNotBeNullOrEmpty(customMessage: $"Slug {entry.Slug} missing author");
            entry.Data.PubDate.ShouldNotBe(default, customMessage: $"Slug {entry.Slug} missing pubDate");
            entry.Data.GetTags().Length.ShouldBeGreaterThan(0, customMessage: $"Slug {entry.Slug} missing tags");
        }
    }

    [Fact]
    public void ContentCollectionShouldIdentifyDraftPosts()
    {
        var query = CreateDefaultQuery();
        var allEntries = query.GetCollection<BlogPostSchema>("blog");

        var drafts = allEntries.Where(e => e.Data.Draft).ToList();
        var published = allEntries.Where(e => !e.Data.Draft).ToList();

        drafts.Count.ShouldBe(1);
        published.Count.ShouldBe(3);
        drafts[0].Data.Title.ShouldContain("Draft");
    }

    [Fact]
    public void ContentCollectionEntriesShouldHaveCorrectSlugs()
    {
        var query = CreateDefaultQuery();
        var entries = query.GetCollection<BlogPostSchema>("blog");

        var slugs = entries.Select(e => e.Slug).ToHashSet();
        slugs.ShouldContain("getting-started");
        slugs.ShouldContain("islands-architecture");
        slugs.ShouldContain("content-collections");
        slugs.ShouldContain("draft-upcoming");
    }

    [Fact]
    public void ContentCollectionShouldRenderAllPostsToHtml()
    {
        var query = CreateDefaultQuery();
        var entries = query.GetCollection<BlogPostSchema>("blog");

        foreach (var entry in entries)
        {
            var rendered = query.Render(entry);
            rendered.Html.ShouldNotBeNullOrEmpty(
                customMessage: $"Post {entry.Slug} rendered to empty HTML");
            rendered.Html.ShouldContain("<h1",
                customMessage: $"Post {entry.Slug} missing heading in rendered HTML");
        }
    }
}
