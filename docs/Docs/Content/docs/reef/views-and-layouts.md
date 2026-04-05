---
title: Views & Layouts
description: Article listing views and page layouts.
order: 34
section: Reef Theme
---

# Views & Layouts

Reef separates *views* (how a list of articles is rendered) from *layouts* (the full HTML document shell). Mix and match them to build the listing experience you want.

## Article listing views

Four view modes are available for rendering a list of `ArticleListItem` entries.

### List view — `ArticleList`

The default and most compact view. Each entry renders as a horizontal row with title, date, description snippet, and tags. Best for content-heavy sites where readers scan quickly.

```
Title                                          Mar 2025
Short description text here.         dotnet  csharp
─────────────────────────────────────────────────────
Next Article Title                             Feb 2025
```

```csharp
var listFragment = ComponentRenderer.ToFragment<ArticleList>(new()
{
    [nameof(ArticleList.Items)] = items,
    [nameof(ArticleList.BasePath)] = "/blog",
});
```

### Grid view — `ArticleGrid` + `ArticleCard`

A responsive CSS grid of cards, each showing an optional hero image, title, date, excerpt, reading time, and tags. Suitable for visually rich sites.

```csharp
// Compose cards into a grid slot
var cards = items.Select(item =>
    ComponentRenderer.ToFragment<ArticleCard>(new()
    {
        [nameof(ArticleCard.Title)] = item.Title,
        [nameof(ArticleCard.Slug)] = item.Slug,
        [nameof(ArticleCard.PubDate)] = item.PubDate,
        [nameof(ArticleCard.BasePath)] = "/blog",
    })).ToList();

// Render grid with cards as slot content
```

The `ArticleGrid.Columns` parameter (default `3`) controls column count via `--grid-cols` CSS variable. The `ReefTheme` CSS collapses to fewer columns at smaller breakpoints.

### Table view — `ArticleTable`

A sortable `<table>` with columns for title, date, author, tags, and reading time. Best for technical blogs or reference-style archives where readers want to compare articles.

```csharp
var tableFragment = ComponentRenderer.ToFragment<ArticleTable>(new()
{
    [nameof(ArticleTable.Items)] = items,
    [nameof(ArticleTable.ShowAuthor)] = true,
    [nameof(ArticleTable.ShowTags)] = true,
    [nameof(ArticleTable.ShowReadingTime)] = true,
    [nameof(ArticleTable.BasePath)] = "/blog",
});
```

### Timeline view — `ArticleTimeline`

Groups articles by year (and optionally by month) in a `<dl>`-style chronological list. Ideal for personal sites, changelogs, or archives.

```csharp
var timelineFragment = ComponentRenderer.ToFragment<ArticleTimeline>(new()
{
    [nameof(ArticleTimeline.Items)] = items,
    [nameof(ArticleTimeline.GroupByMonth)] = false,
    [nameof(ArticleTimeline.BasePath)] = "/blog",
});
```

## Pagination strategy

Reef uses *static pagination* — each page is pre-generated at build time. This keeps navigation fast and requires no JavaScript.

1. Define a `[PageRoute("/blog/page/[page]")]` page that accepts a `page` parameter.
2. Implement `IStaticPathsProvider` to yield paths for pages 2, 3, … N.
3. Use `PaginationInfo` to build the pagination component:

```csharp
var info = new PaginationInfo(
    currentPage: CurrentPage,
    totalPages: (int)Math.Ceiling(entries.Count / (double)ArticlesConfig.Config.ArticlesPerPage),
    baseUrl: ArticlesConfig.Config.BasePath);

var paginationFragment = ComponentRenderer.ToFragment<Pagination>(new()
{
    [nameof(Pagination.Info)] = info,
});
```

The `ViewToggle` island adds client-side view switching on top of the statically rendered views — users can flip between list/grid/table without a page reload while each view mode renders the same articles.

## Layouts

Layouts are full HTML document shells. They are assigned to pages via the `[Layout(typeof(...))]` attribute.

### `ArticleListLayout`

Use for article listing pages: index, tag pages, author pages, paginated pages.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `Config` | `ReefConfig` | ✅ | Site configuration |
| `PageTitle` | `string` | | Page title shown in `<title>` and the page heading |
| `PageDescription` | `string?` | | Meta description for the page |
| `PageHeadContent` | `string?` | | Raw HTML injected into `<head>` (for OG tags etc.) |

Renders:
- `<!DOCTYPE html><html>` shell
- `<head>` via `ArticleBaseHead` (title, meta, favicon, theme FOUC script, custom CSS)
- `<header>` with site logo and navigation
- `<main class="article-listing">` where your page's slot content renders
- `<footer>`

### `ArticleLayout`

Use for individual article pages.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `Config` | `ReefConfig` | ✅ | Site configuration |
| `PageTitle` | `string` | | Article title (also used in `<title>`) |
| `PageDescription` | `string?` | | Article description |
| `PageHeadContent` | `string?` | | Raw HTML injected into `<head>` (for OG meta) |

Renders:
- Full HTML shell
- `<header>` with site navigation
- `<main><article>` wrapping the page slot content
- `<footer>`

### `ArticleBaseHead`

Internal sub-component used by both layouts to render `<head>`. You generally do not use this directly. It renders:
- `<title>` (page title + site title)
- `<meta name="description">` (page or site description)
- `<link rel="icon">` (when `FaviconHref` is set)
- Inline FOUC-prevention script for dark/light mode
- `ReefTheme` inline CSS
- Any `CustomCss` URLs
- Any raw `PageHeadContent` (use for OpenGraph meta, RSS link etc.)

## Setting the `[Layout]` attribute on pages

```csharp
[Layout(typeof(ArticleListLayout))]
[PageRoute("/blog")]
public sealed class ArticleIndexPage : AtollComponent, IAtollPage
{
    // The layout reads Config and PageTitle from the IAtollPage contract.
    // Provide them via Parameters on the page.
    [Parameter(Required = true)] public ReefConfig Config { get; set; } = ArticlesConfig.Config;
    [Parameter] public string PageTitle { get; set; } = "Blog";
    ...
}

[Layout(typeof(ArticleLayout))]
[PageRoute("/blog/[slug]")]
public sealed class ArticlePostPage : AtollComponent, IAtollPage, IStaticPathsProvider
{
    [Parameter(Required = true)] public ReefConfig Config { get; set; } = ArticlesConfig.Config;
    [Parameter] public string PageTitle { get; set; } = "";
    ...
}
```
