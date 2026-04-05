using System.Net;
using Atoll.Build.Content.Collections;
using Atoll.Components;
using Atoll.Reef.Components;
using Atoll.Reef.Configuration;
using Atoll.Reef.Islands;
using Atoll.Routing;
using Atoll.Slots;
using Docs.Layouts;

namespace Docs.Pages;

/// <summary>
/// A live demo page that renders all Reef article listing components (List, Grid, Table, Timeline)
/// with hardcoded sample data, plus interactive ViewToggle and ArticleFilter islands.
/// Route: /docs/reef/live-demo
/// </summary>
[Layout(typeof(SiteLayout))]
[PageRoute("/docs/reef/live-demo")]
public sealed class ReefLiveDemoPage : AtollComponent, IAtollPage
{
    private static readonly IReadOnlyList<ArticleListItem> SampleArticles =
    [
        new("Getting Started with Atoll",
            "getting-started-with-atoll",
            "Learn how to build your first static site with Atoll, the .NET-native framework inspired by Astro.",
            new DateTime(2026, 3, 15), "Alice Smith", ["dotnet", "tutorial"], 8),

        new("Islands Architecture in Depth",
            "islands-architecture-in-depth",
            "A deep dive into selective hydration and how islands keep your site fast while enabling interactivity.",
            new DateTime(2026, 2, 28), "Bob Jones", ["architecture", "performance"], 12),

        new("Content Collections Best Practices",
            "content-collections-best-practices",
            "Patterns and tips for organising your content collections, schemas, and frontmatter validation.",
            new DateTime(2026, 1, 10), "Carol Chen", ["dotnet", "content"], 6),

        new("Building a Blog with Reef",
            "building-a-blog-with-reef",
            "Step-by-step guide to creating a fully-featured blog using the Reef articles addon.",
            new DateTime(2025, 11, 22), "Alice Smith", ["tutorial", "reef"], 10),

        new("Performance Tuning Static Sites",
            "performance-tuning-static-sites",
            "Techniques for optimising build times, output size, and runtime performance in Atoll sites.",
            new DateTime(2025, 9, 5), "Bob Jones", ["performance", "dotnet"], 15),

        new("CSS Scoping Without JavaScript",
            "css-scoping-without-javascript",
            "How Atoll's compile-time CSS scoping eliminates runtime overhead while keeping styles isolated.",
            new DateTime(2025, 7, 18), "Carol Chen", ["architecture", "css"], 7),
    ];

    /// <summary>Gets or sets the collection query (required by SiteLayout).</summary>
    [Parameter(Required = true)]
    public CollectionQuery Query { get; set; } = null!;

    /// <summary>Gets or sets the slug — fixed to <c>"reef/live-demo"</c> for sidebar highlighting.</summary>
    [Parameter]
    public string Slug { get; set; } = "reef/live-demo";

    /// <summary>Gets or sets the page title shown in the layout.</summary>
    [Parameter]
    public string PageTitle { get; set; } = "Reef Live Demo";

    /// <summary>Gets or sets the page description for the meta tag.</summary>
    [Parameter]
    public string? PageDescription { get; set; } =
        "Live demo of the Reef plugin's article listing components: List, Grid, Table, and Timeline views with interactive ViewToggle and ArticleFilter islands.";

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var tags = SampleArticles
            .SelectMany(a => a.Tags)
            .Distinct()
            .OrderBy(t => t)
            .ToList();

        var authors = SampleArticles
            .Where(a => a.Author is not null)
            .Select(a => a.Author!)
            .Distinct()
            .OrderBy(a => a)
            .ToList();

        // Use "#" as base path so article and tag links are inert (no 404 navigation)
        const string basePath = "#";

        // Inject scoped CSS: view-panel visibility, filter tag styling, demo link
        // neutralisation, and reef variable bridge
        WriteHtml("""
            <style>
            [data-view-container] [data-view-panel] { display: none; }
            [data-view-container][data-view="list"] [data-view-panel="list"],
            [data-view-container][data-view="grid"] [data-view-panel="grid"],
            [data-view-container][data-view="table"] [data-view-panel="table"] {
                display: block;
            }
            .reef-listing-controls {
                display: flex;
                align-items: center;
                gap: 1rem;
                flex-wrap: wrap;
                margin-bottom: 1.5rem;
            }

            /* Filter tag pills — match the ReefTheme filter-pill design */
            .reef-demo .article-filter .tag-pill {
                cursor: pointer;
                padding: 0.3rem 0.75rem;
                border: 1px solid var(--reef-border);
                border-radius: 9999px;
                font-size: 0.8rem;
                font-weight: 500;
                background: var(--reef-bg-raised);
                color: var(--reef-text);
                transition: background 0.15s, color 0.15s, border-color 0.15s;
            }
            .reef-demo .article-filter .tag-pill:hover,
            .reef-demo .article-filter .tag-pill--active {
                background: var(--reef-primary);
                color: #fff;
                border-color: var(--reef-primary);
            }

            /* Author dropdown styling */
            .reef-demo .article-filter__author-label {
                font-size: 0.875rem;
                color: var(--reef-text-muted);
                margin-right: 0.25rem;
            }
            .reef-demo .article-filter__author-select {
                padding: 0.3rem 0.5rem;
                border: 1px solid var(--reef-border);
                border-radius: 0.375rem;
                background: var(--reef-bg-raised);
                color: var(--reef-text);
                font-size: 0.8rem;
            }

            /* Neutralise demo links — articles and tags are sample-only */
            .reef-demo a[href^="#/"] {
                pointer-events: none;
                cursor: default;
            }

            .reef-demo {
                --reef-bg: var(--docs-bg, #ffffff);
                --reef-bg-raised: var(--docs-bg-raised, #f9fafb);
                --reef-bg-subtle: var(--docs-bg-subtle, #f3f4f6);
                --reef-text: var(--docs-text, #111827);
                --reef-text-muted: var(--docs-text-muted, #6b7280);
                --reef-primary: var(--docs-link, #0f3460);
                --reef-link: var(--docs-link, #0f3460);
                --reef-link-hover: var(--docs-link-hover, #e94560);
                --reef-border: var(--docs-border, #e5e7eb);
                --reef-tag-bg: #eff6ff;
                --reef-tag-text: var(--docs-link, #0f3460);
            }
            </style>
            """);

        WriteHtml("<h1>Reef Live Demo</h1>");
        WriteHtml("<p>This page renders live Reef components with sample article data. Use the toggle to switch between List, Grid, and Table views. Use the tag pills and author dropdown to filter articles. The Timeline section is always visible below.</p>");

        // View-toggle container
        WriteHtml("<div class=\"reef-demo\" data-view-container data-view=\"list\">");

        // Controls bar: ViewToggle island + ArticleFilter island
        WriteHtml("<div class=\"reef-listing-controls\">");

        var viewToggle = new ViewToggle();
        await viewToggle.RenderIslandAsync(context.Destination, new Dictionary<string, object?>
        {
            [nameof(ViewToggle.CurrentView)] = DefaultView.List,
        });

        var articleFilter = new ArticleFilter();
        await articleFilter.RenderIslandAsync(context.Destination, new Dictionary<string, object?>
        {
            [nameof(ArticleFilter.Tags)] = (IReadOnlyList<string>)tags,
            [nameof(ArticleFilter.Authors)] = (IReadOnlyList<string>)authors,
        });

        WriteHtml("</div>"); // .reef-listing-controls

        // List view panel — each article wrapped with data-tags/data-author for filtering
        WriteHtml("<div data-view-panel=\"list\">");
        await WriteFilterableArticles(context, "list");
        WriteHtml("</div>");

        // Grid view panel
        WriteHtml("<div data-view-panel=\"grid\">");
        await WriteFilterableArticles(context, "grid");
        WriteHtml("</div>");

        // Table view panel — table can't easily wrap individual rows, render without
        // per-row filtering wrappers (filter still works on the sibling card containers)
        WriteHtml("<div data-view-panel=\"table\">");
        var tableFragment = ComponentRenderer.ToFragment<ArticleTable>(new Dictionary<string, object?>
        {
            [nameof(ArticleTable.Items)] = SampleArticles,
            [nameof(ArticleTable.BasePath)] = basePath,
        });
        await RenderAsync(tableFragment);
        WriteHtml("</div>");

        WriteHtml("</div>"); // .reef-demo data-view-container

        // Timeline section (always visible, outside toggle)
        WriteHtml("<hr style=\"margin: 2rem 0;\">");
        WriteHtml("<h2>Timeline View</h2>");
        WriteHtml("<p>The timeline groups articles by year and shows a chronological feed.</p>");
        WriteHtml("<div class=\"reef-demo\">");
        var timelineFragment = ComponentRenderer.ToFragment<ArticleTimeline>(new Dictionary<string, object?>
        {
            [nameof(ArticleTimeline.Items)] = SampleArticles,
            [nameof(ArticleTimeline.BasePath)] = basePath,
        });
        await RenderAsync(timelineFragment);
        WriteHtml("</div>");
    }

    /// <summary>
    /// Renders each article individually inside a wrapper <c>&lt;div&gt;</c> carrying
    /// <c>data-tags</c> and <c>data-author</c> attributes so the ArticleFilter island
    /// JS can show/hide them.
    /// </summary>
    private async Task WriteFilterableArticles(RenderContext context, string viewKind)
    {
        const string basePath = "#";

        if (viewKind == "grid")
        {
            WriteHtml("<div class=\"article-grid\" style=\"--grid-cols:2\">");
        }

        foreach (var item in SampleArticles)
        {
            var tagsCsv = string.Join(",", item.Tags);
            var encodedTags = WebUtility.HtmlEncode(tagsCsv);
            var encodedAuthor = WebUtility.HtmlEncode(item.Author ?? "");

            WriteHtml($"<div data-tags=\"{encodedTags}\" data-author=\"{encodedAuthor}\">");

            if (viewKind == "list")
            {
                // Render a single-item ArticleList
                var fragment = ComponentRenderer.ToFragment<ArticleList>(new Dictionary<string, object?>
                {
                    [nameof(ArticleList.Items)] = new[] { item },
                    [nameof(ArticleList.BasePath)] = basePath,
                });
                await RenderAsync(fragment);
            }
            else if (viewKind == "grid")
            {
                // Render a single ArticleCard
                var fragment = ComponentRenderer.ToFragment<ArticleCard>(new Dictionary<string, object?>
                {
                    [nameof(ArticleCard.Title)] = item.Title,
                    [nameof(ArticleCard.Slug)] = item.Slug,
                    [nameof(ArticleCard.Description)] = item.Description,
                    [nameof(ArticleCard.PubDate)] = item.PubDate,
                    [nameof(ArticleCard.Author)] = item.Author,
                    [nameof(ArticleCard.Tags)] = item.Tags,
                    [nameof(ArticleCard.ReadingTimeMinutes)] = item.ReadingTimeMinutes,
                    [nameof(ArticleCard.BasePath)] = basePath,
                });
                await RenderAsync(fragment);
            }

            WriteHtml("</div>");
        }

        if (viewKind == "grid")
        {
            WriteHtml("</div>");
        }
    }
}
