---
title: Annotations Overview
description: Enable contextual text-selection feedback that opens pre-populated GitHub Issues or Discussions — no backend required.
order: 60
section: Annotations Plugin
---

# Annotations Overview

The `Atoll.Annotations` plugin lets readers select text on any page, write a comment, and submit it as a GitHub Issue or Discussion in a new browser tab. The quoted text, user comment, and a text-fragment link back to the exact selection are all pre-populated automatically.

| Feature | Description |
|---|---|
| **Zero JS by default** | Server-renders a placeholder `<div>` — no JavaScript shipped until hydration |
| **Idle hydration** | `client:idle` defers loading until the browser is idle, keeping the critical path fast |
| **No backend** | All feedback goes directly to GitHub — no database, no auth, no server-side API |
| **Theme sync** | Watches `<html data-theme>` and adapts the popover UI for light and dark modes |
| **Text fragment link** | The submitted issue body includes a `#:~:text=` link that scrolls readers to the exact selection |

## Installation

Add the `Atoll.Annotations` NuGet package:

```bash
dotnet add package Atoll.Annotations
```

Or add directly to your `.csproj`:

```xml
<PackageReference Include="Atoll.Annotations" Version="0.1.*" />
```

Register the island asset provider so the embedded JavaScript is copied to the output directory during build:

```csharp
using Atoll.Annotations;

builder.Services.AddIslandAssetProvider<AnnotationsIslandAssetProvider>();
```

No additional NuGet packages are required. The island JavaScript asset is embedded in the assembly and served automatically via the `IIslandAssetProvider` pipeline.

## Quick start

Render the `TextAnnotation` island in any page or layout — typically near the closing `</body>` or inside a shared layout so it applies site-wide:

```csharp
using Atoll.Components;
using Atoll.Annotations;

var props = new Dictionary<string, object?>
{
    [nameof(TextAnnotation.Repo)] = "myuser/myrepo",
    [nameof(TextAnnotation.Target)] = AnnotationTarget.Issue,
};
var fragment = ComponentRenderer.ToFragment<TextAnnotation>(props);
await RenderAsync(fragment);
```

The component renders a `<div class="atoll-annotations">` placeholder with all configuration encoded as `data-*` attributes. When the browser is idle, the island hydrates and begins listening for text selections within the content area.

## How it works

1. The user selects text inside the content area (matched by `ContentSelector`, default `article`).
2. A floating button appears near the selection.
3. Clicking the button opens a small popover with the quoted text and a comment textarea.
4. On submit, a new browser tab opens with a pre-populated GitHub Issue or Discussion URL containing:
   - The selected text in a blockquote
   - The user's comment
   - A text-fragment link (`#:~:text=...`) back to the exact selection on the page

## Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Repo` | `string` | *(required)* | GitHub repository in `owner/repo` format. |
| `Target` | `AnnotationTarget` | `Issue` | Where feedback is submitted: `Issue` or `Discussion`. |
| `Category` | `string?` | `null` | GitHub Discussions category name. Required when `Target` is `Discussion`. |
| `Labels` | `string?` | `null` | Comma-separated label names to pre-populate on new issues. Only applies when `Target` is `Issue`. |
| `TitlePrefix` | `string` | `"Feedback:"` | Prefix prepended to the page title in the issue/discussion title. |
| `ContentSelector` | `string` | `"article"` | CSS selector identifying the content area where text selection is enabled. |
| `ButtonText` | `string` | `"💬"` | Text or icon displayed in the floating trigger button. |

## Annotation target

The `Target` parameter controls where the feedback is submitted.

| Value | Behaviour |
|---|---|
| `Issue` | Opens a new GitHub Issue with the quoted text and comment. Labels can be pre-populated via `Labels`. |
| `Discussion` | Opens a new GitHub Discussion. The `Category` parameter must match an existing category in the repository. |

## Theme synchronisation

When the Atoll theme toggle is present (setting `data-theme` on `<html>`), the island observes attribute changes with a `MutationObserver` and updates the popover colours automatically. Both the floating button and the comment form adapt to light and dark themes.

## URL length safety

GitHub URLs have a practical limit of around 8,000 characters. The island automatically truncates the quoted text at 2,000 characters and enforces a total URL cap of 8,000 characters. Selections longer than 200 characters are truncated when constructing the text-fragment link, since very long fragments are unreliable across browsers.

## Full example — Issues

A documentation site that collects text-selection feedback as GitHub Issues with `feedback` and `docs` labels:

```csharp
using Atoll.Components;
using Atoll.Annotations;

public sealed class DocsLayout : AtollComponent
{
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<article>");
        WriteHtml("<h1>My Documentation Page</h1>");
        WriteHtml("<p>Content goes here.</p>");
        WriteHtml("</article>");

        var props = new Dictionary<string, object?>
        {
            [nameof(TextAnnotation.Repo)] = "myuser/mydocs",
            [nameof(TextAnnotation.Target)] = AnnotationTarget.Issue,
            [nameof(TextAnnotation.Labels)] = "feedback,docs",
            [nameof(TextAnnotation.TitlePrefix)] = "Docs Feedback:",
            [nameof(TextAnnotation.ContentSelector)] = "article",
        };
        var fragment = ComponentRenderer.ToFragment<TextAnnotation>(props);
        await RenderAsync(fragment);
    }
}
```

## Full example — Discussions

A blog that routes text-selection feedback into a GitHub Discussions category:

```csharp
using Atoll.Components;
using Atoll.Annotations;

public sealed class BlogLayout : AtollComponent
{
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<article>");
        WriteHtml("<h1>My Blog Post</h1>");
        WriteHtml("<p>Post content goes here.</p>");
        WriteHtml("</article>");

        var props = new Dictionary<string, object?>
        {
            [nameof(TextAnnotation.Repo)] = "myuser/myblog",
            [nameof(TextAnnotation.Target)] = AnnotationTarget.Discussion,
            [nameof(TextAnnotation.Category)] = "Feedback",
            [nameof(TextAnnotation.TitlePrefix)] = "Reader Feedback:",
        };
        var fragment = ComponentRenderer.ToFragment<TextAnnotation>(props);
        await RenderAsync(fragment);
    }
}
```
