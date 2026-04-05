---
title: Components
description: Reusable server-rendered components for article views.
order: 32
section: Reef Plugin
---

# Components

Reef provides 13 server-rendered components for building article pages. All components are subclasses of `AtollComponent` and live in the `Atoll.Reef.Components` namespace.

## ArticleCard

A card displaying a single article with an optional hero image, title, date, description, reading time, author, and tags.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `Title` | `string` | ✅ | Article title |
| `Slug` | `string` | ✅ | URL slug (used to build the article link) |
| `Description` | `string` | | Excerpt or description |
| `PubDate` | `DateTime` | | Publication date |
| `Author` | `string?` | | Author display name |
| `Tags` | `string[]` | | Tag list |
| `ImageSrc` | `string?` | | Hero image URL |
| `ImageAlt` | `string` | | Hero image alt text |
| `ReadingTimeMinutes` | `int?` | | Estimated reading time |
| `BasePath` | `string` | | URL base path prefix |

Renders an `<article class="article-card">` element. Internally composes `ArticleMeta`.

## ArticleList

A vertical list view — compact rows of title, date, description, and tags.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `Items` | `IReadOnlyList<ArticleListItem>` | ✅ | List of articles to render |
| `BasePath` | `string` | | URL base path prefix |

Renders a `<div class="article-list">` containing `<article class="article-list-item">` elements.

### `ArticleListItem`

Data record passed to `ArticleList`, `ArticleTimeline`, `ArticleTable`, `RelatedArticlesResolver`, and `TagCountBuilder`.

```csharp
new ArticleListItem(
    title: "Getting Started",
    slug: "getting-started",
    description: "A beginner's guide.",
    pubDate: new DateTime(2025, 3, 15),
    author: "alice",
    tags: ["dotnet", "getting-started"],
    readingTimeMinutes: 5)
```

Use `ArticleListItemBuilder.Build(entries)` to convert `ContentEntry<ArticleSchema>` entries to `ArticleListItem` instances automatically.

## ArticleGrid

A responsive CSS grid container wrapping `ArticleCard` components.

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Columns` | `int` | `3` | Number of grid columns |

Renders a `<div class="article-grid">` with a CSS custom property `--grid-cols`. Add `ArticleCard` fragments as the slot content.

```csharp
var gridFragment = ComponentRenderer.ToFragment<ArticleGrid>(new()
{
    [nameof(ArticleGrid.Columns)] = 3,
});
```

## ArticleTimeline

Chronological timeline view with articles grouped by year (and optionally month).

| Parameter | Type | Required | Description |
|---|---|---|---|
| `Items` | `IReadOnlyList<ArticleListItem>` | ✅ | List of articles |
| `GroupByMonth` | `bool` | | Group within year sections by month |
| `BasePath` | `string` | | URL base path prefix |

Renders `<div class="article-timeline">` with `<section class="timeline-year">` subsections.

## ArticleTable

Tabular view with configurable columns.

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Items` | `IReadOnlyList<ArticleListItem>` | ✅ | List of articles |
| `ShowAuthor` | `bool` | `true` | Render the Author column |
| `ShowTags` | `bool` | `true` | Render the Tags column |
| `ShowReadingTime` | `bool` | `true` | Render the Reading Time column |
| `BasePath` | `string` | | URL base path prefix |

Renders a `<table class="article-table">` with a `<thead>` and `<tbody>`.

## ArticleMeta

Date, author, reading time, and tags strip for an article.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `PubDate` | `DateTime` | ✅ | Publication date |
| `Author` | `string?` | | Author display name |
| `ReadingTimeMinutes` | `int?` | | Reading time in minutes |
| `Tags` | `string[]` | | Tags with links to tag pages |
| `BasePath` | `string` | | URL base path prefix |

Renders `<div class="article-meta">`.

## ArticleNav

Previous/next navigation between individual articles.

| Parameter | Type | Description |
|---|---|---|
| `Previous` | `ArticleNavLink?` | Previous article link |
| `Next` | `ArticleNavLink?` | Next article link |

Renders `<nav class="article-nav" aria-label="Article navigation">`. Either or both links may be null.

### `ArticleNavLink`

```csharp
new ArticleNavLink(title: "Previous Post", href: "/blog/previous-post")
```

## ArticleSeries

Multi-part series indicator showing all parts with links, highlighting the current part.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `SeriesName` | `string` | ✅ | Name of the series |
| `Parts` | `IReadOnlyList<SeriesPart>` | ✅ | All series parts |
| `CurrentPart` | `int` | ✅ | 1-based index of the current part |

Renders `<aside class="article-series">`. Use `SeriesResolver.Resolve()` to build the parts list.

### `SeriesPart`

```csharp
new SeriesPart(title: "Part 1: Introduction", href: "/blog/series-part-1")
```

## RelatedArticles

"You might also like" section based on shared tags.

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Articles` | `IReadOnlyList<ArticleNavLink>` | ✅ | Related article links |
| `Heading` | `string` | `"Related Articles"` | Section heading |
| `MaxItems` | `int` | `3` | Maximum items to show |

Renders `<aside class="related-articles">`. Use `RelatedArticlesResolver.Resolve()` to build the list.

## AuthorCard

Author bio card with avatar, name, bio text, and optional profile link.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `Name` | `string` | ✅ | Author's display name |
| `AvatarUrl` | `string?` | | Avatar image URL |
| `Bio` | `string?` | | Short bio text |
| `Url` | `string?` | | Profile page URL (renders name as `<a>` when set) |

Renders `<aside class="author-card">`.

## TagCloud

Tag pill list with counts and links to tag pages.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `Tags` | `IReadOnlyList<TagCount>` | ✅ | Tags with counts |
| `BasePath` | `string` | | URL base path prefix |

Renders `<nav class="tag-cloud" aria-label="Tags">`. Use `TagCountBuilder.Build(items)` to build the tag list.

### `TagCount`

```csharp
new TagCount(name: "dotnet", count: 12)
// Slug property: "dotnet" (lowercased)
```

## Pagination

Page number navigation for listing pages.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `Info` | `PaginationInfo` | ✅ | Pagination state |

Renders `<nav class="pagination" aria-label="Page navigation">` with previous, numbered, and next links. Uses ellipsis for large page counts.

### `PaginationInfo`

```csharp
new PaginationInfo(currentPage: 2, totalPages: 5, baseUrl: "/blog")
// GetPageUrl(1) → "/blog"
// GetPageUrl(2) → "/blog/page/2"
```

## OpenGraphMeta

Renders OpenGraph and Twitter Card meta tags in the `<head>`.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `Title` | `string` | ✅ | `og:title` and `twitter:title` |
| `Description` | `string?` | | `og:description` and `twitter:description` |
| `ImageUrl` | `string?` | | `og:image` and `twitter:image` |
| `Url` | `string?` | | `og:url` canonical URL |
| `Author` | `string?` | | `article:author` |
| `PubDate` | `DateTime?` | | `article:published_time` |
| `SiteName` | `string` | | `og:site_name` |

Renders a series of `<meta>` tags. Include in the `PageHeadContent` parameter of the layout.
