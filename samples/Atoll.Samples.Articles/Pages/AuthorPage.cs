using Atoll.Build.Content.Collections;
using Atoll.Components;
using Atoll.Reef.Components;
using Atoll.Reef.Configuration;
using Atoll.Routing;

namespace Atoll.Samples.Articles.Pages;

/// <summary>
/// Displays articles filtered by a specific author.
/// Route: /articles/author/[author]
/// </summary>
[Layout(typeof(ArticlesLayout))]
[PageRoute("/articles/author/[author]")]
public sealed class AuthorPage : AtollComponent, IAtollPage, IStaticPathsProvider
{
    /// <summary>Gets or sets the author name from the URL parameter.</summary>
    [Parameter(Required = true)]
    public string Author { get; set; } = "";

    /// <summary>Gets or sets the collection query for loading articles.</summary>
    [Parameter(Required = true)]
    public CollectionQuery Query { get; set; } = null!;

    /// <inheritdoc />
    public Task<IReadOnlyList<StaticPath>> GetStaticPathsAsync()
    {
        var entries = Query.GetCollection<ArticleSchema>("articles",
            entry => !entry.Data.Draft && !string.IsNullOrEmpty(entry.Data.Author));

        var authors = entries
            .Select(e => e.Data.Author)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var paths = authors
            .Select(author => new StaticPath(
                new Dictionary<string, string> { ["author"] = author.ToLowerInvariant() }))
            .ToList();

        return Task.FromResult<IReadOnlyList<StaticPath>>(paths);
    }

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var basePath = ArticlesConfig.Current.BasePath;

        WriteHtml("<p><a href=\"");
        WriteHtml(System.Net.WebUtility.HtmlEncode(basePath + "/articles"));
        WriteHtml("\">&larr; All articles</a></p>");

        var entries = Query.GetCollection<ArticleSchema>("articles",
            entry => !entry.Data.Draft &&
                     string.Equals(entry.Data.Author, Author, StringComparison.OrdinalIgnoreCase));

        var items = entries
            .OrderByDescending(e => e.Data.PubDate)
            .Select(e => new ArticleListItem(
                title: e.Data.Title,
                slug: e.Slug,
                description: e.Data.Description,
                pubDate: e.Data.PubDate,
                author: string.IsNullOrEmpty(e.Data.Author) ? null : e.Data.Author,
                tags: e.Data.GetTags(),
                readingTimeMinutes: e.Data.ReadingTimeMinutes))
            .ToList();

        if (items.Count == 0)
        {
            WriteHtml("<p>No articles found for this author.</p>");
            return;
        }

        var listFragment = ComponentRenderer.ToFragment<ArticleList>(new Dictionary<string, object?>
        {
            [nameof(ArticleList.Items)] = (IReadOnlyList<ArticleListItem>)items,
            [nameof(ArticleList.BasePath)] = basePath,
        });
        await RenderAsync(listFragment);
    }
}
