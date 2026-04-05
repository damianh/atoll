---
title: Configuration
description: Configure Reef with ReefConfig and ArticleSchema.
order: 31
section: Reef Theme
---

# Configuration

## `ReefConfig`

`ReefConfig` is the root configuration object for the Reef theme. Pass it to `ArticleLayout` and `ArticleListLayout` via the `Config` parameter.

| Property | Type | Default | Description |
|---|---|---|---|
| `Title` | `string` | `""` | Site title shown in the header |
| `Description` | `string` | `""` | Site description used in meta tags and the RSS channel |
| `LogoSrc` | `string?` | `null` | URL of the header logo image |
| `LogoAlt` | `string` | `""` | Alt text for the header logo |
| `FaviconHref` | `string?` | `null` | URL of the favicon |
| `ArticlesPerPage` | `int` | `10` | Number of articles per listing page |
| `DefaultView` | `DefaultView` | `List` | Default article listing view mode |
| `TagPageEnabled` | `bool` | `true` | Whether tag pages are generated |
| `AuthorPageEnabled` | `bool` | `true` | Whether author pages are generated |
| `RssEnabled` | `bool` | `true` | Whether the RSS feed is generated |
| `BasePath` | `string` | `""` | URL base path prefix (e.g. `"/blog"`) |
| `SiteUrl` | `string` | `""` | Absolute site URL used in RSS and OG tags (e.g. `"https://myblog.com"`) |
| `CollectionName` | `string` | `"articles"` | Name of the content collection to query |
| `Social` | `IReadOnlyList<SocialLink>` | `[]` | Social links rendered in the header/footer |
| `CustomCss` | `IReadOnlyList<string>` | `[]` | Additional CSS URLs appended to `<head>` |
| `Authors` | `IReadOnlyDictionary<string, AuthorInfo>` | `{}` | Author profiles keyed by author identifier |

### Example

```csharp
public static class ArticlesConfig
{
    public static ReefConfig Config { get; } = new ReefConfig
    {
        Title = "My Blog",
        Description = "Thoughts on .NET, web, and open source.",
        SiteUrl = "https://myblog.com",
        BasePath = "/blog",
        ArticlesPerPage = 12,
        DefaultView = DefaultView.Grid,
        RssEnabled = true,
        Authors = new Dictionary<string, AuthorInfo>
        {
            ["alice"] = new AuthorInfo
            {
                Name = "Alice Smith",
                AvatarUrl = "/images/alice.jpg",
                Bio = "Senior .NET engineer.",
                Url = "https://alice.dev",
            },
        },
    };
}
```

## `DefaultView` enum

Controls the default rendering style for article listings.

| Value | Description |
|---|---|
| `List` | Compact vertical list (title + date + description) |
| `Grid` | Card grid with optional images |
| `Table` | Sortable tabular view |

## `AuthorInfo`

Defines an author profile referenced by the `author` frontmatter field.

| Property | Type | Description |
|---|---|---|
| `Name` | `string` | Author's display name |
| `AvatarUrl` | `string?` | URL of the author's avatar image |
| `Bio` | `string?` | Short biography text |
| `Url` | `string?` | Link to the author's profile or website |

## `SocialLink`

Defines a social link shown in the header.

| Property | Type | Description |
|---|---|---|
| Constructor arg 1 | `string label` | Accessible label for the link |
| Constructor arg 2 | `string href` | URL of the social profile |
| Constructor arg 3 | `SocialIcon icon` | Icon to render (see `SocialIcon` enum) |

### `SocialIcon` enum

`GitHub`, `Twitter`, `LinkedIn`, `YouTube`, `Mastodon`, `Bluesky`, `RSS`

## `ArticleSchema`

`ArticleSchema` is the typed frontmatter model for article content entries. Use it when defining your content collection.

| Property | Type | Required | Description |
|---|---|---|---|
| `Title` | `string` | ✅ | Article title |
| `Description` | `string` | ✅ | Short description / excerpt |
| `PubDate` | `DateTime` | ✅ | Publication date |
| `Author` | `string` | | Author identifier key in `ReefConfig.Authors` |
| `Tags` | `string` | | Comma-separated tag list (e.g. `"dotnet, csharp"`) |
| `Series` | `string?` | | Series name for multi-part articles |
| `SeriesOrder` | `int?` | | Position within the series (1-based) |
| `Image` | `string?` | | URL of the article hero image |
| `ImageAlt` | `string?` | | Alt text for the hero image |
| `Draft` | `bool` | | When `true`, exclude from production builds |
| `ReadingTimeMinutes` | `int?` | | Override for the auto-calculated reading time |

### `GetTags()` helper

`ArticleSchema.GetTags()` splits the `Tags` string by comma and trims whitespace, returning a `string[]`. Use this to build tag pages and tag clouds.

```csharp
var tags = entry.Data.GetTags(); // ["dotnet", "csharp"]
```

## Content collection setup

Register your articles collection in a class implementing `IContentConfiguration`:

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

Place your markdown files in `Content/articles/` and Atoll will bind the YAML frontmatter to `ArticleSchema` automatically.
