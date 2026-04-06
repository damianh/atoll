---
title: Reef Overview
description: An articles and blog theme addon for Atoll.
order: 30
section: Reef Plugin
---

# Reef Overview

Reef is the official articles and blog theme addon for Atoll. Add it to your project to get a full-featured article site — with multiple view modes, tag and author pages, RSS feeds, OpenGraph meta, series navigation, and related articles — without building any of it yourself.

## What's included

| Feature | Description |
|---|---|
| **Multiple view modes** | List, grid, table, and timeline views for article listings |
| **ArticleLayout** | Full-page HTML shell for individual article pages |
| **ArticleListLayout** | Full-page layout for article listing and tag/author pages |
| **Pagination** | Static page number navigation with configurable page size |
| **Tag pages** | Automatic per-tag listing pages |
| **Author pages** | Author bio cards and per-author article listings |
| **RSS feed** | Generated RSS 2.0 feed with Atom namespace |
| **OpenGraph meta** | `og:*` and `twitter:card` meta tags for social sharing |
| **Series navigation** | Multi-part article series with previous/next links |
| **Related articles** | Tag-based related article suggestions |
| **ArticleFilter island** | Client-side tag/author filtering (hydrates on idle) |
| **ViewToggle island** | Client-side list/grid/table view switcher |
| **TagCloud** | Tag pill list with counts and links |
| **AuthorCard** | Author avatar, bio, and profile link |
| **Reading time** | Automatic reading time estimation from body word count |

## Quick start

### 1. Add the NuGet package

```bash
dotnet add package Atoll.Reef
```

Or add directly to your `.csproj`:

```xml
<PackageReference Include="Atoll.Reef" Version="0.1.*" />
```

### 2. Define a content collection

Create a `ContentConfig.cs` file that registers your articles collection using `ArticleSchema` as the frontmatter type:

```csharp
using Atoll.Build.Content.Collections;
using Atoll.Reef.Configuration;

public sealed class ContentConfig : IContentConfiguration
{
    public void Configure(ContentCollectionBuilder builder)
    {
        builder.Add(ContentCollection.Define<ArticleSchema>("articles"));
    }
}
```

### 3. Configure `ReefConfig`

Create a static singleton config for your site:

```csharp
using Atoll.Reef.Configuration;

public static class ArticlesConfig
{
    public static ReefConfig Config { get; } = new ReefConfig
    {
        Title = "My Blog",
        Description = "Articles about .NET and web development.",
        SiteUrl = "https://myblog.com",
        BasePath = "/blog",
        ArticlesPerPage = 10,
        RssEnabled = true,
    };
}
```

### 4. Create a listing page

```csharp
using Atoll.Build.Content.Collections;
using Atoll.Components;
using Atoll.Reef.Components;
using Atoll.Reef.Configuration;
using Atoll.Reef.Layouts;
using Atoll.Reef.Navigation;
using Atoll.Routing;

[Layout(typeof(ArticleListLayout))]
[PageRoute("/blog")]
public sealed class ArticleIndexPage : AtollComponent, IAtollPage
{
    [Parameter(Required = true)] public CollectionQuery Query { get; set; } = null!;

    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var entries = Query.GetCollection<ArticleSchema>("articles");
        var items = ArticleListItemBuilder.Build(entries);

        var listFragment = ComponentRenderer.ToFragment<ArticleList>(new()
        {
            [nameof(ArticleList.Items)] = items,
            [nameof(ArticleList.BasePath)] = ArticlesConfig.Config.BasePath,
        });

        await RenderAsync(listFragment);
    }
}
```

### 5. Create an article page

```csharp
[Layout(typeof(ArticleLayout))]
[PageRoute("/blog/[slug]")]
public sealed class ArticlePostPage : AtollComponent, IAtollPage, IStaticPathsProvider
{
    [Parameter(Required = true)] public string Slug { get; set; } = "";
    [Parameter(Required = true)] public CollectionQuery Query { get; set; } = null!;

    public Task<IReadOnlyList<StaticPath>> GetStaticPathsAsync()
    {
        var entries = Query.GetCollection<ArticleSchema>("articles");
        return Task.FromResult<IReadOnlyList<StaticPath>>(
            entries.Select(e => new StaticPath(
                new Dictionary<string, string> { ["slug"] = e.Slug })).ToList());
    }

    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var entry = Query.GetEntry<ArticleSchema>("articles", Slug);
        if (entry is null) return;

        var bodyFragment = Query.Render(entry);
        await RenderAsync(bodyFragment);
    }
}
```

## ArticleSchema frontmatter

Article markdown files use YAML frontmatter bound to `ArticleSchema`. See [Configuration](/docs/reef/configuration) for the full property reference.

```markdown
---
title: Getting Started with Atoll
description: A beginner's guide to building static sites with Atoll.
pubDate: 2025-03-15
author: alice
tags: dotnet, atoll, getting-started
---

Your article body here...
```
