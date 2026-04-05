---
title: Giscus Overview
description: Add GitHub Discussions-powered comments to any Atoll page with lazy-loaded, theme-synced islands.
order: 50
section: Giscus Plugin
---

# Giscus Overview

The `Atoll.Giscus` plugin embeds [giscus](https://giscus.app/) comments powered by GitHub Discussions. Comments load lazily when the user scrolls near them, keeping page weight at zero JavaScript until needed. The widget automatically syncs its theme with Atoll's theme toggle.

| Feature | Description |
|---|---|
| **Zero JS by default** | Server-renders a placeholder `<div>` — no JavaScript shipped until hydration |
| **Lazy hydration** | `client:visible` with 300 px preload margin triggers loading before the widget is in view |
| **Theme sync** | Watches `<html data-theme>` and forwards changes to the giscus iframe via `postMessage` |
| **Full giscus API** | All giscus configuration options exposed as strongly-typed C# parameters |

## Installation

Add a project reference to `Atoll.Giscus`:

```xml
<ProjectReference Include="..\..\src\Atoll.Giscus\Atoll.Giscus.csproj" />
```

Register the island asset provider so the embedded JavaScript is copied to the output directory during build:

```csharp
using Atoll.Giscus;

builder.Services.AddIslandAssetProvider<GiscusIslandAssetProvider>();
```

No additional NuGet packages are required. The island JavaScript asset is embedded in the assembly and served automatically via the `IIslandAssetProvider` pipeline.

## Quick start

Visit [giscus.app](https://giscus.app/) to obtain your **Repository ID** and **Category ID**, then render the `GiscusComments` island in any page or layout:

```csharp
using Atoll.Components;
using Atoll.Giscus;

var props = new Dictionary<string, object?>
{
    [nameof(GiscusComments.Repo)] = "myuser/myrepo",
    [nameof(GiscusComments.RepoId)] = "R_kgDOABCDEF",
    [nameof(GiscusComments.CategoryId)] = "DIC_kwDOABCDEF",
};
var fragment = ComponentRenderer.ToFragment<GiscusComments>(props);
await RenderAsync(fragment);
```

The component renders a `<div class="giscus">` placeholder with all configuration encoded as `data-*` attributes. When the user scrolls within 300 px, the island hydrates, injects the giscus `<script>` tag, and the comment widget appears.

## Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Repo` | `string` | *(required)* | GitHub repository in `owner/repo` format. Must have the giscus app installed. |
| `RepoId` | `string` | *(required)* | Base64-encoded repository ID from [giscus.app](https://giscus.app/). |
| `CategoryId` | `string` | *(required)* | Base64-encoded discussions category ID from [giscus.app](https://giscus.app/). |
| `Category` | `string?` | `null` | GitHub Discussions category name. Optional but recommended. |
| `Mapping` | `GiscusMapping` | `Pathname` | How pages map to discussions (see below). |
| `Term` | `string?` | `null` | Custom search term when `Mapping` is `Specific` or `Number`. |
| `Strict` | `bool` | `false` | Use SHA-1 title hashing to avoid false discussion matches. |
| `ReactionsEnabled` | `bool` | `true` | Show emoji reactions on the top-level discussion post. |
| `EmitMetadata` | `bool` | `false` | Emit discussion metadata to the parent page via `postMessage`. |
| `InputPosition` | `GiscusInputPosition` | `Bottom` | Comment input box placement: `Top` or `Bottom`. |
| `Theme` | `string` | `"preferred_color_scheme"` | Built-in theme name or URL to a custom CSS file. |
| `Lang` | `string` | `"en"` | BCP 47 language tag for the giscus UI. |
| `Loading` | `GiscusLoading` | `Lazy` | Iframe loading strategy: `Lazy` or `Eager`. |

## Discussion mapping

The `Mapping` parameter controls how giscus finds or creates a GitHub Discussion for each page.

| Value | Wire format | Behaviour |
|---|---|---|
| `Pathname` | `pathname` | Discussion title contains the page pathname (e.g. `/blog/my-post`). **Recommended default.** |
| `Url` | `url` | Discussion title contains the full page URL. |
| `Title` | `title` | Discussion title contains the page `<title>` text. |
| `OgTitle` | `og:title` | Discussion title contains the `og:title` meta value. |
| `Specific` | `specific` | Uses a custom search term set via `Term`. |
| `Number` | `number` | References an existing discussion by number via `Term`. Discussions are not created automatically. |

## Theme synchronisation

When the Atoll theme toggle is present (setting `data-theme` on `<html>`), the island observes attribute changes with a `MutationObserver` and forwards the new theme to the giscus iframe via the `postMessage` API. No additional wiring is needed.

To use a custom theme, set `Theme` to the URL of a CSS file:

```csharp
[nameof(GiscusComments.Theme)] = "https://cdn.example.com/giscus-theme.css"
```

## Full example

A blog article page with comments at the bottom:

```csharp
using Atoll.Components;
using Atoll.Giscus;

public sealed class ArticlePage : AtollComponent
{
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<article>");
        WriteHtml("<h1>My Article</h1>");
        WriteHtml("<p>Article content goes here.</p>");
        WriteHtml("</article>");

        var props = new Dictionary<string, object?>
        {
            [nameof(GiscusComments.Repo)] = "myuser/myblog",
            [nameof(GiscusComments.RepoId)] = "R_kgDOABCDEF",
            [nameof(GiscusComments.Category)] = "Blog Comments",
            [nameof(GiscusComments.CategoryId)] = "DIC_kwDOABCDEF",
            [nameof(GiscusComments.Mapping)] = GiscusMapping.Pathname,
            [nameof(GiscusComments.InputPosition)] = GiscusInputPosition.Top,
            [nameof(GiscusComments.Theme)] = "preferred_color_scheme",
            [nameof(GiscusComments.Lang)] = "en",
        };
        var fragment = ComponentRenderer.ToFragment<GiscusComments>(props);
        await RenderAsync(fragment);
    }
}
```
