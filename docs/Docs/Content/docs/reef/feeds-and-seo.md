---
title: Feeds & SEO
description: RSS feeds, OpenGraph meta, and SEO for article sites.
order: 35
section: Reef Theme
---

# Feeds & SEO

## RSS Feed

Reef includes `RssFeedGenerator`, a utility that generates a complete RSS 2.0 feed with Atom namespace from your article collection.

### `RssFeedGenerator.Generate`

```csharp
string xml = RssFeedGenerator.Generate(
    config: ArticlesConfig.Config,
    articles: items,         // IReadOnlyList<ArticleListItem>
    basePath: "/blog");      // URL path prefix
```

The generated XML follows the RSS 2.0 spec and includes:
- `<rss version="2.0" xmlns:atom="http://www.w3.org/2005/Atom">`
- `<channel>` with `<title>`, `<link>`, `<description>`, `<language>`, `<generator>`
- `<atom:link rel="self">` pointing to `{SiteUrl}{BasePath}/feed.xml`
- One `<item>` per article with `<title>`, `<link>`, `<guid>`, `<description>`, `<pubDate>`, optional `<author>`, and one `<category>` per tag

The `<atom:link rel="self">` is only emitted when `config.RssEnabled` is `true`.

### Feed endpoint

Create an `IAtollEndpoint` to serve the feed at a static path:

```csharp
using System.Collections.ObjectModel;
using System.Text;
using Atoll.Build.Content.Collections;
using Atoll.Reef.Feed;
using Atoll.Reef.Navigation;
using Atoll.Routing;

[PageRoute("/blog/feed.xml")]
public sealed class FeedEndpoint : IAtollEndpoint, IStaticPathsProvider
{
    [Parameter(Required = true)] public CollectionQuery Query { get; set; } = null!;

    public Task<AtollResponse>? GetAsync(EndpointContext context)
    {
        var entries = Query.GetCollection<ArticleSchema>("articles");
        var items = ArticleListItemBuilder.Build(entries);
        var xml = RssFeedGenerator.Generate(ArticlesConfig.Config, items, "/blog");
        var body = Encoding.UTF8.GetBytes(xml);
        var headers = new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>
            {
                ["Content-Type"] = "application/rss+xml; charset=utf-8",
            });
        return Task.FromResult(new AtollResponse(200, headers, body));
    }

    public Task<IReadOnlyList<StaticPath>> GetStaticPathsAsync()
        => Task.FromResult<IReadOnlyList<StaticPath>>(
            [new StaticPath(new Dictionary<string, string>())]);
}
```

### RSS autodiscovery link

Add `<link rel="alternate">` to your `ArticleBaseHead` `PageHeadContent` to let browsers and feed readers discover the RSS feed:

```csharp
string PageHeadContent =
    """<link rel="alternate" type="application/rss+xml" title="My Blog" href="/blog/feed.xml" />""";
```

Pass this as the `PageHeadContent` parameter on your listing layout.

## OpenGraph meta tags

`OpenGraphMeta` renders a set of `<meta>` tags for the `<head>` section that enable rich previews on social platforms.

### Parameters

| Parameter | Type | Description |
|---|---|---|
| `Title` | `string` | `og:title`, `twitter:title` |
| `Description` | `string?` | `og:description`, `twitter:description` |
| `ImageUrl` | `string?` | `og:image`, `twitter:image` |
| `Url` | `string?` | `og:url` canonical page URL |
| `Author` | `string?` | `article:author` |
| `PubDate` | `DateTime?` | `article:published_time` (ISO 8601) |
| `SiteName` | `string` | `og:site_name` |

### Generated tags

```html
<meta property="og:type" content="article" />
<meta property="og:title" content="..." />
<meta property="og:description" content="..." />
<meta property="og:image" content="..." />
<meta property="og:url" content="..." />
<meta property="og:site_name" content="..." />
<meta property="article:published_time" content="2025-03-15T00:00:00Z" />
<meta property="article:author" content="..." />
<meta name="twitter:card" content="summary_large_image" />
<meta name="twitter:title" content="..." />
<meta name="twitter:description" content="..." />
<meta name="twitter:image" content="..." />
```

When `ImageUrl` is null, `twitter:card` is set to `"summary"` instead of `"summary_large_image"`.

### Injecting OG tags into the layout

Render `OpenGraphMeta` to a string destination and pass the result as `PageHeadContent` to the layout:

```csharp
// In ArticlePostPage.RenderCoreAsync (before rendering body):
var ogDest = new StringRenderDestination();
await ComponentRenderer.RenderComponentAsync<OpenGraphMeta>(ogDest, new()
{
    [nameof(OpenGraphMeta.Title)] = entry.Data.Title,
    [nameof(OpenGraphMeta.Description)] = entry.Data.Description,
    [nameof(OpenGraphMeta.ImageUrl)] = entry.Data.Image,
    [nameof(OpenGraphMeta.Url)] = $"{ArticlesConfig.Config.SiteUrl}/blog/{entry.Slug}",
    [nameof(OpenGraphMeta.Author)] = entry.Data.Author,
    [nameof(OpenGraphMeta.PubDate)] = entry.Data.PubDate,
    [nameof(OpenGraphMeta.SiteName)] = ArticlesConfig.Config.Title,
});
// Pass ogDest.GetOutput() as PageHeadContent to the layout
```

## Reading time

`ReadingTimeCalculator.Calculate(markdownBody)` estimates reading time based on a 200-words-per-minute rate. The minimum returned value is 1 minute.

```csharp
int minutes = ReadingTimeCalculator.Calculate(entry.Body); // e.g. 5
```

When `ArticleSchema.ReadingTimeMinutes` is set in the frontmatter, `ArticleListItemBuilder` uses that value directly and skips the calculation.

## SEO best practices

- Set `SiteUrl` in `ReefConfig` to enable absolute URLs in the RSS feed and OG tags.
- Set `FaviconHref` in `ReefConfig` for a `<link rel="icon">` in every page.
- Use `PageDescription` on listing pages and individual article pages for `<meta name="description">`.
- Add the RSS autodiscovery link to all listing pages.
- Use meaningful `ImageAlt` values in `ArticleSchema` for article hero images.
- Set `Draft: true` in frontmatter and filter draft entries during development builds.
