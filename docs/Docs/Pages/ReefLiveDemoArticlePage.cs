using Atoll.Build.Content.Collections;
using Atoll.Components;
using Atoll.Reef.Components;
using Atoll.Routing;
using Docs.Layouts;

namespace Docs.Pages;

/// <summary>
/// Stub article page for the Reef live demo. Renders a placeholder article detail
/// view for each sample article slug, so that article links from the demo listing
/// page navigate to a real page instead of a 404.
/// Route: /reef/live-demo/[slug]
/// </summary>
[Layout(typeof(SiteLayout))]
[PageRoute("/reef/live-demo/[slug]")]
public sealed class ReefLiveDemoArticlePage : AtollComponent, IAtollPage, IStaticPathsProvider
{
    private const string BasePath = "/reef/live-demo";

    /// <summary>Gets or sets the article slug from the route parameter.</summary>
    [Parameter(Required = true)]
    public string Slug { get; set; } = "";

    /// <summary>Gets or sets the collection query (required by SiteLayout).</summary>
    [Parameter(Required = true)]
    public CollectionQuery Query { get; set; } = null!;

    /// <summary>Gets or sets the page title shown in the layout.</summary>
    [Parameter]
    public string PageTitle { get; set; } = "";

    /// <summary>Gets or sets the page description for the meta tag.</summary>
    [Parameter]
    public string? PageDescription { get; set; }

    /// <inheritdoc />
    public Task<IReadOnlyList<StaticPath>> GetStaticPathsAsync()
    {
        var paths = ReefLiveDemoPage.SampleArticles
            .Select(a => new StaticPath(new Dictionary<string, string> { ["slug"] = a.Slug }))
            .ToList();

        return Task.FromResult<IReadOnlyList<StaticPath>>(paths);
    }

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var article = ReefLiveDemoPage.SampleArticles.FirstOrDefault(a => a.Slug == Slug);

        if (article is null)
        {
            PageTitle = "Article Not Found";
            WriteHtml("<h1>Article Not Found</h1>");
            WriteHtml("<p>The requested article does not exist.</p>");
            WriteHtml($"<p><a href=\"{BasePath}\">&larr; Back to Reef Live Demo</a></p>");
            return;
        }

        PageTitle = article.Title;
        PageDescription = article.Description;

        WriteHtml("<article class=\"reef-demo\">");

        // Header
        WriteHtml("<header style=\"margin-bottom: 2rem;\">");
        WriteHtml("<h1>");
        WriteText(article.Title);
        WriteHtml("</h1>");

        // Render metadata via ArticleMeta component
        var metaFragment = ComponentRenderer.ToFragment<ArticleMeta>(new Dictionary<string, object?>
        {
            [nameof(ArticleMeta.PubDate)] = article.PubDate,
            [nameof(ArticleMeta.Author)] = article.Author,
            [nameof(ArticleMeta.ReadingTimeMinutes)] = article.ReadingTimeMinutes,
            [nameof(ArticleMeta.Tags)] = article.Tags.ToArray(),
            [nameof(ArticleMeta.BasePath)] = BasePath,
        });
        await RenderAsync(metaFragment);

        WriteHtml("</header>");

        // Placeholder body
        WriteHtml("<div class=\"prose\">");
        WriteHtml("<p>");
        WriteText(article.Description);
        WriteHtml("</p>");
        WriteHtml("""
            <p style="color: var(--docs-text-muted); font-style: italic;">
            This is a placeholder article page for the Reef live demo.
            In a real site, the full article content would be rendered here from a content collection entry.
            </p>
            """);
        WriteHtml("</div>");

        // Back link
        WriteHtml("<hr style=\"margin: 2rem 0;\">");
        WriteHtml($"<p><a href=\"{BasePath}\">&larr; Back to Reef Live Demo</a></p>");

        WriteHtml("</article>");
    }
}
