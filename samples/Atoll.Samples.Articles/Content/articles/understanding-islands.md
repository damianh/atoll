---
title: Understanding Islands Architecture
description: How Atoll uses the islands pattern to deliver fast, partially-interactive pages.
pubDate: 2026-02-10
author: Jane Developer
tags: islands, architecture, performance
draft: false
---

# Understanding Islands Architecture

The islands architecture isolates interactive components ("islands") within an otherwise
static, server-rendered page. The result: fast initial loads with targeted hydration.

## How Islands Work in Atoll

In Atoll, an island is a C# class that extends `VanillaJsIsland`. The server renders
the initial HTML. The client then hydrates only the island component — not the whole page.

```csharp
[ClientIdle]
public sealed class ThemeToggle : VanillaJsIsland
{
    public override string ClientModuleUrl => "/scripts/theme-toggle.js";

    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<button class=\"theme-toggle\" aria-label=\"Toggle theme\">🌙</button>");
        return Task.CompletedTask;
    }
}
```

## Hydration Strategies

| Directive | When |
|-----------|------|
| `[ClientLoad]` | Immediately on page load |
| `[ClientIdle]` | When the browser is idle |
| `[ClientVisible]` | When the element enters the viewport |
| `[ClientMedia("...")]` | When a CSS media query matches |

## Benefits

- **Performance** — only the interactive parts are hydrated
- **Simplicity** — islands are isolated; no shared global state
- **Progressive enhancement** — pages work without JavaScript
