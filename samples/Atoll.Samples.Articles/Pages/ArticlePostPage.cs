using Atoll.Build.Content.Collections;
using Atoll.Components;
using Atoll.Reef.Components;
using Atoll.Reef.Configuration;
using Atoll.Reef.Navigation;
using Atoll.Routing;

namespace Atoll.Samples.Articles.Pages;

/// <summary>
/// The individual article page. Displays the rendered Markdown content
/// with article metadata (title, date, author, tags, reading time),
/// an optional series indicator, related articles, and previous/next navigation.
/// Route: /articles/[slug]
/// </summary>
[Layout(typeof(ArticlesLayout))]
[PageRoute("/articles/[slug]")]
public sealed class ArticlePostPage : AtollComponent, IAtollPage, IStaticPathsProvider
{
    /// <summary>Gets or sets the article slug from the URL parameter.</summary>
    [Parameter(Required = true)]
    public string Slug { get; set; } = "";

    /// <summary>Gets or sets the collection query for loading articles.</summary>
    [Parameter(Required = true)]
    public CollectionQuery Query { get; set; } = null!;

    /// <inheritdoc />
    public Task<IReadOnlyList<StaticPath>> GetStaticPathsAsync()
    {
        var entries = Query.GetCollection<ArticleSchema>("articles",
            entry => !entry.Data.Draft);

        var paths = entries.Select(e =>
            new StaticPath(
                new Dictionary<string, string> { ["slug"] = e.Slug }))
            .ToList();

        return Task.FromResult<IReadOnlyList<StaticPath>>(paths);
    }

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var allEntries = Query.GetCollection<ArticleSchema>("articles",
            entry => !entry.Data.Draft);
        var entry = Query.GetEntry<ArticleSchema>("articles", Slug);

        if (entry is null)
        {
            WriteHtml("<h1>Article Not Found</h1>");
            WriteHtml("<p>The requested article could not be found.</p>");
            return;
        }

        var allItems = ArticleListItemBuilder.Build(allEntries);
        var rendered = Query.Render(entry);
        var basePath = ArticlesConfig.Current.BasePath;

        WriteHtml("<article class=\"reef-article prose\">");

        // Article header with meta
        WriteHtml("<header class=\"reef-article-header\">");
        WriteHtml("<h1 class=\"reef-article-title\">");
        WriteText(entry.Data.Title);
        WriteHtml("</h1>");

        var metaFragment = ComponentRenderer.ToFragment<ArticleMeta>(new Dictionary<string, object?>
        {
            [nameof(ArticleMeta.PubDate)] = entry.Data.PubDate,
            [nameof(ArticleMeta.Author)] = string.IsNullOrEmpty(entry.Data.Author)
                ? null : entry.Data.Author,
            [nameof(ArticleMeta.ReadingTimeMinutes)] = entry.Data.ReadingTimeMinutes,
            [nameof(ArticleMeta.Tags)] = entry.Data.GetTags(),
            [nameof(ArticleMeta.BasePath)] = basePath,
        });
        await RenderAsync(metaFragment);

        WriteHtml("</header>");

        // Series indicator (when article belongs to a series)
        if (!string.IsNullOrEmpty(entry.Data.Series))
        {
            var (parts, currentPart) = SeriesResolver.Resolve(
                entry.Data.Series, entry.Slug, allEntries, basePath);

            var seriesFragment = ComponentRenderer.ToFragment<ArticleSeries>(new Dictionary<string, object?>
            {
                [nameof(ArticleSeries.SeriesName)] = entry.Data.Series,
                [nameof(ArticleSeries.Parts)] = parts,
                [nameof(ArticleSeries.CurrentPart)] = currentPart,
            });
            await RenderAsync(seriesFragment);
        }

        // Rendered Markdown content
        WriteHtml("<div class=\"reef-prose\">");
        var contentComponent = ContentComponent.FromRenderedContent(rendered);
        await RenderAsync(contentComponent.ToRenderFragment());
        WriteHtml("</div>");

        // Related articles
        var related = RelatedArticlesResolver.Resolve(
            entry.Slug,
            entry.Data.GetTags(),
            allItems,
            basePath,
            maxItems: 3);

        if (related.Count > 0)
        {
            var relatedFragment = ComponentRenderer.ToFragment<RelatedArticles>(new Dictionary<string, object?>
            {
                [nameof(RelatedArticles.Articles)] = related,
                [nameof(RelatedArticles.Heading)] = "Related Articles",
            });
            await RenderAsync(relatedFragment);
        }

        // Previous/next navigation
        var sorted = allItems.OrderByDescending(i => i.PubDate).ToList();
        var idx = sorted.FindIndex(i => i.Slug == Slug);
        var prev = idx > 0
            ? new ArticleNavLink(sorted[idx - 1].Title, $"{basePath}/{sorted[idx - 1].Slug}")
            : null;
        var next = idx < sorted.Count - 1
            ? new ArticleNavLink(sorted[idx + 1].Title, $"{basePath}/{sorted[idx + 1].Slug}")
            : null;

        if (prev is not null || next is not null)
        {
            var navFragment = ComponentRenderer.ToFragment<ArticleNav>(new Dictionary<string, object?>
            {
                [nameof(ArticleNav.Previous)] = prev,
                [nameof(ArticleNav.Next)] = next,
            });
            await RenderAsync(navFragment);
        }

        WriteHtml("</article>");
    }
}
