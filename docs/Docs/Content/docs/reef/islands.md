---
title: Islands
description: Interactive islands for filtering and view switching.
order: 33
section: Reef Theme
---

# Islands

Reef ships two client-side islands using the Atoll Islands Architecture. Islands hydrate progressively — server-rendered HTML is delivered immediately, and JavaScript enhances it after the page loads.

## ArticleFilter

A tag and author filter bar that shows and hides articles on the listing page without a full page reload.

**Hydration strategy**: `[ClientIdle]` — activates after the browser is idle, keeping the initial render fast.

**Module URL**: `/scripts/atoll-reef-article-filter.js`

### Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Tags` | `IReadOnlyList<string>` | `[]` | All available tags to display as filter pills |
| `Authors` | `IReadOnlyList<string>` | `[]` | All available author identifiers for the author dropdown |

### SSR output

The island renders a filter bar with clickable tag pills and an author dropdown. Each article card should carry `data-tags` and `data-author` attributes for the JavaScript to read. When the user clicks a tag pill, the script toggles visibility of cards that don't match.

### JavaScript behaviour

`article-filter.js` listens for `click` events on tag pills and `change` on the author dropdown. It reads `data-tags` (space-separated) and `data-author` on each `.article-card` element and toggles the `hidden` attribute accordingly. Multiple selected tags use AND logic.

### Usage

```csharp
var filterFragment = await IslandRenderer.RenderIslandAsync<ArticleFilter>(
    destination,
    ArticleFilter.Metadata,
    new()
    {
        [nameof(ArticleFilter.Tags)] = allTags,
        [nameof(ArticleFilter.Authors)] = allAuthors,
    },
    SlotCollection.Empty);
```

## ViewToggle

A button group that switches the listing page between list, grid, and table views without a page reload.

**Hydration strategy**: `[ClientLoad]` — activates immediately on page load.

**Module URL**: `/scripts/atoll-reef-view-toggle.js`

### Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `CurrentView` | `DefaultView` | `List` | The initially active view mode |

### SSR output

Renders a `<div class="view-toggle">` with three toggle buttons (list, grid, table). The active button receives `aria-pressed="true"`.

### JavaScript behaviour

`view-toggle.js` listens for `click` events on the toggle buttons. When a button is clicked, it:
1. Updates `aria-pressed` on all buttons.
2. Sets a `data-view` attribute on the nearest `.article-listing` ancestor.
3. Persists the choice to `localStorage` as `reef-view`.
4. On load, restores the last saved view preference.

CSS in `ReefTheme` hides the non-active view containers using `[data-view=list] .article-grid { display: none; }` patterns.

### Usage

```csharp
var toggleFragment = await IslandRenderer.RenderIslandAsync<ViewToggle>(
    destination,
    ViewToggle.Metadata,
    new()
    {
        [nameof(ViewToggle.CurrentView)] = ArticlesConfig.Config.DefaultView,
    },
    SlotCollection.Empty);
```

## ReefIslandAssetProvider

`ReefIslandAssetProvider` implements `IIslandAssetProvider` and registers the two embedded JS assets with the Atoll asset pipeline.

```csharp
public sealed class ReefIslandAssetProvider : IIslandAssetProvider
```

### Registration

Register `ReefIslandAssetProvider` when configuring your Atoll middleware:

```csharp
builder.Services.AddSingleton<IIslandAssetProvider, ReefIslandAssetProvider>();
```

Atoll will then serve `article-filter.js` at `/scripts/atoll-reef-article-filter.js` and `view-toggle.js` at `/scripts/atoll-reef-view-toggle.js`.

### Asset descriptors

| Script URL | Embedded resource |
|---|---|
| `/scripts/atoll-reef-article-filter.js` | `Atoll.Reef.Islands.Assets.article-filter.js` |
| `/scripts/atoll-reef-view-toggle.js` | `Atoll.Reef.Islands.Assets.view-toggle.js` |
