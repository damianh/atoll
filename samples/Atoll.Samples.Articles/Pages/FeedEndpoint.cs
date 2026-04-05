using Atoll.Build.Content.Collections;
using Atoll.Components;
using Atoll.Reef.Components;
using Atoll.Reef.Configuration;
using Atoll.Reef.Feed;
using Atoll.Routing;
using System.Collections.ObjectModel;
using System.Text;

namespace Atoll.Samples.Articles.Pages;

/// <summary>
/// RSS 2.0 feed endpoint. Generates the site feed at <c>/articles/feed.xml</c>.
/// </summary>
[PageRoute("/articles/feed.xml")]
public sealed class FeedEndpoint : IAtollEndpoint, IStaticPathsProvider
{
    /// <summary>Gets or sets the collection query for loading articles.</summary>
    [Parameter(Required = true)]
    public CollectionQuery Query { get; set; } = null!;

    /// <inheritdoc />
    public Task<AtollResponse>? GetAsync(EndpointContext context)
    {
        var config = ArticlesConfig.Current;
        var entries = Query.GetCollection<ArticleSchema>(config.CollectionName,
            entry => !entry.Data.Draft);

        var articles = entries.Select(e => new ArticleListItem(
            e.Data.Title,
            e.Slug,
            e.Data.Description,
            e.Data.PubDate,
            string.IsNullOrEmpty(e.Data.Author) ? null : e.Data.Author,
            e.Data.GetTags(),
            e.Data.ReadingTimeMinutes))
            .OrderByDescending(a => a.PubDate)
            .ToList();

        var xml = RssFeedGenerator.Generate(config, articles, config.BasePath);
        var body = Encoding.UTF8.GetBytes(xml);
        var headers = new Dictionary<string, string>
        {
            ["Content-Type"] = "application/rss+xml; charset=utf-8",
        };
        return Task.FromResult(new AtollResponse(200,
            new ReadOnlyDictionary<string, string>(headers), body));
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<StaticPath>> GetStaticPathsAsync() =>
        Task.FromResult<IReadOnlyList<StaticPath>>(
            [new StaticPath(new Dictionary<string, string>())]);
}
