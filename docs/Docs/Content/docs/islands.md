---
title: Islands (Partial Hydration)
description: Add client-side interactivity with zero-JS-by-default islands architecture.
order: 6
section: Features
---

# Islands (Partial Hydration)

Islands are components that render static HTML on the server, then optionally load JavaScript on the client for interactivity. This is Atoll's implementation of Astro's islands architecture — **ship zero JavaScript by default**, then selectively hydrate.

## Client directives

Each directive controls *when* the island's JavaScript loads:

| Directive | Attribute | Behavior |
|---|---|---|
| `client:load` | `[ClientLoad]` | Hydrate immediately on page load |
| `client:idle` | `[ClientIdle]` | Hydrate when browser is idle (`requestIdleCallback`) |
| `client:visible` | `[ClientVisible]` | Hydrate when element scrolls into view |
| `client:media` | `[ClientMedia("query")]` | Hydrate when CSS media query matches |

## Creating an island

Extend `VanillaJsIsland` and apply a client directive:

```csharp
using Atoll.Islands;
using Atoll.Components;

[ClientLoad]
public sealed class ThemeToggle : VanillaJsIsland
{
    public override string ClientModuleUrl => "/scripts/theme-toggle.js";

    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("""
            <button id="theme-toggle" type="button" aria-label="Toggle theme">
                Toggle Theme
            </button>
            """);
        return Task.CompletedTask;
    }
}
```

## Lazy-loaded islands

Use `[ClientVisible]` for content below the fold:

```csharp
[ClientVisible(RootMargin = "200px")]
public sealed class ImageGallery : VanillaJsIsland
{
    public override string ClientModuleUrl => "/scripts/gallery.js";

    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<div class=\"gallery\"><!-- server-rendered gallery grid --></div>");
        return Task.CompletedTask;
    }
}
```

## Responsive islands

Use `[ClientMedia]` for islands that only hydrate at certain viewport sizes:

```csharp
[ClientMedia("(max-width: 768px)")]
public sealed class MobileNav : VanillaJsIsland
{
    public override string ClientModuleUrl => "/scripts/mobile-nav.js";

    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<nav class=\"mobile-nav\"><!-- hamburger menu --></nav>");
        return Task.CompletedTask;
    }
}
```

## Client-side JavaScript

The island renders as an `<atoll-island>` custom element. Your JavaScript module receives `(element, props, slots, metadata)`:

```javascript
// /scripts/theme-toggle.js
export default function init(element, props, slots, metadata) {
    const button = element.querySelector('#theme-toggle');
    button.addEventListener('click', () => {
        document.documentElement.classList.toggle('dark');
    });
}
```

## How it works

1. Server renders the island as an `<atoll-island>` element containing the static HTML
2. Serialized props are embedded as JSON in a `<script type="application/json">` element
3. The Atoll runtime script (`atoll-island.js`) bootstraps hydration based on the directive
4. Your client module is dynamically imported and called with the element and props
