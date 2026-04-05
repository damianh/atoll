---
title: Series & Navigation
description: Article series, related content, and navigation helpers.
order: 36
section: Reef Plugin
---

# Series & Navigation

## Multi-part article series

Reef supports multi-part article series with automatic ordering, a series indicator component, and previous/next links between parts.

### Frontmatter fields

Add `series` and `seriesOrder` to your article frontmatter:

```markdown
---
title: "Part 1: Getting Started"
description: Introduction to the series.
pubDate: 2025-01-01
series: Building a Blog with Atoll
seriesOrder: 1
---
```

`ArticleSchema` binds these as `Series` (`string?`) and `SeriesOrder` (`int?`).

### `SeriesResolver.Resolve`

`SeriesResolver` finds all articles in a series, orders them by `SeriesOrder`, and returns the part list plus the current article's 1-based index.

```csharp
var (parts, currentPart) = SeriesResolver.Resolve<ArticleSchema>(
    seriesName: entry.Data.Series!,
    currentSlug: entry.Slug,
    entries: allEntries,   // IReadOnlyList<ContentEntry<ArticleSchema>>
    basePath: "/blog");
```

- `parts` — `IReadOnlyList<SeriesPart>` ordered by `SeriesOrder`, then by title
- `currentPart` — 1-based integer position of the current article within the series

### `ArticleSeries` component

Renders a series indicator showing all parts and highlighting the current one:

```csharp
var seriesFragment = ComponentRenderer.ToFragment<ArticleSeries>(new()
{
    [nameof(ArticleSeries.SeriesName)] = entry.Data.Series,
    [nameof(ArticleSeries.Parts)] = parts,
    [nameof(ArticleSeries.CurrentPart)] = currentPart,
});
await RenderAsync(seriesFragment);
```

Output:

```html
<aside class="article-series" aria-label="Article series">
  <p class="series-header">Part 1 of 3 in "Building a Blog with Atoll"</p>
  <ol class="series-parts">
    <li class="series-part series-part--current" aria-current="true">
      <span>Part 1: Getting Started</span>
    </li>
    <li class="series-part">
      <a href="/blog/series-part-2">Part 2: Components</a>
    </li>
    ...
  </ol>
</aside>
```

Only render `ArticleSeries` when `entry.Data.Series` is not null.

## Related articles

`RelatedArticlesResolver` scores other articles by tag overlap with the current article and returns the top matches, excluding the current article.

### `RelatedArticlesResolver.Resolve`

```csharp
var related = RelatedArticlesResolver.Resolve(
    currentSlug: entry.Slug,
    currentTags: entry.Data.GetTags(),
    allArticles: items,    // IReadOnlyList<ArticleListItem>
    basePath: "/blog",
    maxItems: 3);
```

- Articles with more shared tags rank higher.
- Articles with equal scores are ordered by publication date descending (newest first).
- The current article is always excluded.
- Returns at most `maxItems` results.

### `RelatedArticles` component

```csharp
if (related.Count > 0)
{
    var relatedFragment = ComponentRenderer.ToFragment<RelatedArticles>(new()
    {
        [nameof(RelatedArticles.Articles)] = related,
        [nameof(RelatedArticles.Heading)] = "Related Articles",
        [nameof(RelatedArticles.MaxItems)] = 3,
    });
    await RenderAsync(relatedFragment);
}
```

## Previous/next navigation

`ArticleNav` renders prev/next links between consecutive articles in the collection.

Build `ArticleNavLink` instances from the sorted article list:

```csharp
var sorted = items.OrderByDescending(i => i.PubDate).ToList();
var idx = sorted.FindIndex(i => i.Slug == Slug);
var prev = idx > 0
    ? new ArticleNavLink(sorted[idx - 1].Title, $"/blog/{sorted[idx - 1].Slug}")
    : null;
var next = idx < sorted.Count - 1
    ? new ArticleNavLink(sorted[idx + 1].Title, $"/blog/{sorted[idx + 1].Slug}")
    : null;

var navFragment = ComponentRenderer.ToFragment<ArticleNav>(new()
{
    [nameof(ArticleNav.Previous)] = prev,
    [nameof(ArticleNav.Next)] = next,
});
await RenderAsync(navFragment);
```

## Tag pages

Tag pages list all articles sharing a given tag. Use `ArticleSchema.GetTags()` to filter:

```csharp
[Layout(typeof(ArticleListLayout))]
[PageRoute("/blog/tag/[tag]")]
public sealed class TagPage : AtollComponent, IAtollPage, IStaticPathsProvider
{
    [Parameter(Required = true)] public string Tag { get; set; } = "";
    [Parameter(Required = true)] public CollectionQuery Query { get; set; } = null!;

    public Task<IReadOnlyList<StaticPath>> GetStaticPathsAsync()
    {
        var entries = Query.GetCollection<ArticleSchema>("articles");
        var allTags = entries.SelectMany(e => e.Data.GetTags()).Distinct().ToList();
        return Task.FromResult<IReadOnlyList<StaticPath>>(allTags
            .Select(t => new StaticPath(new Dictionary<string, string> { ["tag"] = t }))
            .ToList());
    }

    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var entries = Query.GetCollection<ArticleSchema>("articles");
        var items = ArticleListItemBuilder.Build(entries)
            .Where(i => i.Tags.Contains(Tag, StringComparer.OrdinalIgnoreCase))
            .ToList();

        var listFragment = ComponentRenderer.ToFragment<ArticleList>(new()
        {
            [nameof(ArticleList.Items)] = items,
            [nameof(ArticleList.BasePath)] = "/blog",
        });
        await RenderAsync(listFragment);
    }
}
```

## Author pages

Author pages list all articles by a given author. The `author` frontmatter value is matched against the `Author` property of `ArticleListItem`:

```csharp
[Layout(typeof(ArticleListLayout))]
[PageRoute("/blog/author/[author]")]
public sealed class AuthorPage : AtollComponent, IAtollPage, IStaticPathsProvider
{
    [Parameter(Required = true)] public string Author { get; set; } = "";
    [Parameter(Required = true)] public CollectionQuery Query { get; set; } = null!;

    public Task<IReadOnlyList<StaticPath>> GetStaticPathsAsync()
    {
        var entries = Query.GetCollection<ArticleSchema>("articles");
        var authors = entries
            .Select(e => e.Data.Author)
            .Where(a => !string.IsNullOrEmpty(a))
            .Distinct()
            .ToList();
        return Task.FromResult<IReadOnlyList<StaticPath>>(authors
            .Select(a => new StaticPath(new Dictionary<string, string> { ["author"] = a }))
            .ToList());
    }

    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var entries = Query.GetCollection<ArticleSchema>("articles");
        var items = ArticleListItemBuilder.Build(entries)
            .Where(i => string.Equals(i.Author, Author, StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Optionally render an AuthorCard from ReefConfig.Authors
        if (ArticlesConfig.Config.Authors.TryGetValue(Author, out var info))
        {
            var cardFragment = ComponentRenderer.ToFragment<AuthorCard>(new()
            {
                [nameof(AuthorCard.Name)] = info.Name,
                [nameof(AuthorCard.AvatarUrl)] = info.AvatarUrl,
                [nameof(AuthorCard.Bio)] = info.Bio,
                [nameof(AuthorCard.Url)] = info.Url,
            });
            await RenderAsync(cardFragment);
        }

        var listFragment = ComponentRenderer.ToFragment<ArticleList>(new()
        {
            [nameof(ArticleList.Items)] = items,
            [nameof(ArticleList.BasePath)] = "/blog",
        });
        await RenderAsync(listFragment);
    }
}
```

## `ArticleListItemBuilder`

`ArticleListItemBuilder.Build` converts `IReadOnlyList<ContentEntry<ArticleSchema>>` to `IReadOnlyList<ArticleListItem>`:

- Maps all frontmatter fields.
- Uses `ReadingTimeMinutes` from frontmatter if set; otherwise calls `ReadingTimeCalculator.Calculate(entry.Body)`.
- Calls `entry.Data.GetTags()` to split the comma-separated tag string.

```csharp
var items = ArticleListItemBuilder.Build(entries);
```

Use this in every page that needs to work with the article list.
