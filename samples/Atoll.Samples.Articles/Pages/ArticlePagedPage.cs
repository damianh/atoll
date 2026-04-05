using Atoll.Build.Content.Collections;
using Atoll.Components;
using Atoll.Reef.Components;
using Atoll.Reef.Configuration;
using Atoll.Reef.Navigation;
using Atoll.Routing;

namespace Atoll.Samples.Articles.Pages;

/// <summary>
/// Paginated article listing page. Handles routes like <c>/articles/page/2</c>,
/// <c>/articles/page/3</c>, etc. Page 1 is always served by <see cref="ArticleIndexPage"/>.
/// </summary>
[Layout(typeof(ArticlesLayout))]
[PageRoute("/articles/page/[page]")]
public sealed class ArticlePagedPage : AtollComponent, IAtollPage, IStaticPathsProvider
{
    /// <summary>Gets or sets the page number from the URL parameter.</summary>
    [Parameter(Required = true)]
    public string Page { get; set; } = "";

    /// <summary>Gets or sets the collection query for loading articles.</summary>
    [Parameter(Required = true)]
    public CollectionQuery Query { get; set; } = null!;

    /// <inheritdoc />
    public Task<IReadOnlyList<StaticPath>> GetStaticPathsAsync()
    {
        var config = ArticlesConfig.Current;
        var entries = Query.GetCollection<ArticleSchema>("articles",
            entry => !entry.Data.Draft);

        var totalPages = (int)Math.Ceiling((double)entries.Count / config.ArticlesPerPage);

        // Page 1 is handled by ArticleIndexPage; generate paths for pages 2..N
        var paths = Enumerable.Range(2, Math.Max(0, totalPages - 1))
            .Select(p => new StaticPath(
                new Dictionary<string, string> { ["page"] = p.ToString() }))
            .ToList();

        return Task.FromResult<IReadOnlyList<StaticPath>>(paths);
    }

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var config = ArticlesConfig.Current;

        if (!int.TryParse(Page, out var pageNumber) || pageNumber < 2)
        {
            WriteHtml("<p>Invalid page number.</p>");
            return;
        }

        var entries = Query.GetCollection<ArticleSchema>("articles",
            entry => !entry.Data.Draft);

        var sorted = entries.OrderByDescending(e => e.Data.PubDate).ToList();
        var totalPages = (int)Math.Ceiling((double)sorted.Count / config.ArticlesPerPage);

        var paginationInfo = new PaginationInfo(
            currentPage: pageNumber,
            totalPages: Math.Max(1, totalPages),
            baseUrl: config.BasePath);

        var skip = (pageNumber - 1) * config.ArticlesPerPage;
        var pageItems = sorted
            .Skip(skip)
            .Take(config.ArticlesPerPage)
            .Select(e => new ArticleListItem(
                title: e.Data.Title,
                slug: e.Slug,
                description: e.Data.Description,
                pubDate: e.Data.PubDate,
                author: string.IsNullOrEmpty(e.Data.Author) ? null : e.Data.Author,
                tags: e.Data.GetTags(),
                readingTimeMinutes: e.Data.ReadingTimeMinutes))
            .ToList();

        if (pageItems.Count == 0)
        {
            WriteHtml("<p>No articles on this page.</p>");
            return;
        }

        var listFragment = ComponentRenderer.ToFragment<ArticleList>(new Dictionary<string, object?>
        {
            [nameof(ArticleList.Items)] = (IReadOnlyList<ArticleListItem>)pageItems,
            [nameof(ArticleList.BasePath)] = config.BasePath,
        });
        await RenderAsync(listFragment);

        var paginationFragment = ComponentRenderer.ToFragment<Pagination>(new Dictionary<string, object?>
        {
            [nameof(Pagination.Info)] = paginationInfo,
        });
        await RenderAsync(paginationFragment);
    }
}
