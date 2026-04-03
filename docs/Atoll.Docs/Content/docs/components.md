---
title: Components
description: Build reusable UI building blocks with AtollComponent.
order: 2
section: Basics
---

# Components

Components are the fundamental building blocks of an Atoll site. Every component extends `AtollComponent` and overrides `RenderCoreAsync`.

## Basic component

```csharp
using Atoll.Components;

public sealed class Card : AtollComponent
{
    [Parameter(Required = true)]
    public string Title { get; set; } = "";

    [Parameter]
    public string Description { get; set; } = "";

    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<div class=\"card\">");
        WriteHtml("<h2>");
        WriteText(Title);   // HTML-escaped
        WriteHtml("</h2>");
        if (!string.IsNullOrEmpty(Description))
        {
            WriteHtml("<p>");
            WriteText(Description);
            WriteHtml("</p>");
        }
        WriteHtml("</div>");
        return Task.CompletedTask;
    }
}
```

## Rendering API

| Method | Description |
|---|---|
| `WriteHtml(string)` | Write trusted HTML directly to output |
| `WriteText(string)` | Write text with automatic HTML-escaping |
| `RenderSlotAsync()` | Render the default slot (child content) |
| `RenderSlotAsync(name)` | Render a named slot |
| `RenderAsync(fragment)` | Render a `RenderFragment` |

## Parameters

Use `[Parameter]` to declare props. Set `Required = true` to enforce that callers provide a value.

```csharp
[Parameter(Required = true)]
public string Title { get; set; } = "";

[Parameter]
public bool Featured { get; set; }
```

## Rendering child components

Use `ComponentRenderer.ToFragment<T>` to compose child components:

```csharp
var props = new Dictionary<string, object?>
{
    ["Title"] = "My Card",
    ["Description"] = "A reusable component",
};
var fragment = ComponentRenderer.ToFragment<Card>(props);
await RenderAsync(fragment);
```

## Async rendering

Components fully support async rendering for data loading:

```csharp
protected override async Task RenderCoreAsync(RenderContext context)
{
    var data = await LoadDataAsync();
    WriteHtml("<ul>");
    foreach (var item in data)
    {
        WriteHtml("<li>");
        WriteText(item.Name);
        WriteHtml("</li>");
    }
    WriteHtml("</ul>");
}
```
