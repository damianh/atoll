---
title: Understanding Islands Architecture
description: Explore how Atoll's islands architecture delivers interactive components with minimal JavaScript.
pubDate: 2026-02-10
author: Jane Developer
tags: atoll, islands, architecture
draft: false
---

# Understanding Islands Architecture

Islands architecture is a pattern where most of your page is static HTML,
with small "islands" of interactivity sprinkled in where needed.

## Zero JavaScript by Default

Atoll renders all pages as static HTML on the server. No JavaScript is
shipped to the client unless you explicitly opt in with a **client directive**.

## Client Directives

Atoll supports four client directives that control **when** an island hydrates:

| Directive | When it Hydrates |
|-----------|-----------------|
| `[ClientLoad]` | Immediately on page load |
| `[ClientIdle]` | When the browser is idle |
| `[ClientVisible]` | When the element scrolls into view |
| `[ClientMedia("...")]` | When a media query matches |

## Creating an Island

Here's a simple counter island:

```csharp
[ClientLoad]
public sealed class Counter : VanillaJsIsland
{
    public override string ClientModuleUrl => "/scripts/counter.js";

    [Parameter]
    public int InitialCount { get; set; }

    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml($"<div class='counter'>{InitialCount}</div>");
        return Task.CompletedTask;
    }
}
```

The island renders its HTML on the server, then hydrates with JavaScript
only when the directive condition is met.
