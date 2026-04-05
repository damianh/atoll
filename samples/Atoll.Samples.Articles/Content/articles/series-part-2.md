---
title: "Building a Plugin for Atoll, Part 2: Layouts and Themes"
description: Add a full-page layout and a CSS theme to your Atoll plugin.
pubDate: 2025-04-08
author: alice
tags: atoll, plugins, css, dotnet
series: Building a Plugin for Atoll
seriesOrder: 2
---

# Building a Plugin for Atoll, Part 2: Layouts and Themes

In Part 1 we scaffolded the plugin and created a first component. Now we'll add a layout and a CSS theme.

## Creating a layout

A layout is just an `AtollComponent` that renders a full HTML document:

```csharp
namespace MyPlugin.Layouts;

public sealed class MyLayout : AtollComponent
{
    [Parameter(Required = true)] public MyConfig Config { get; set; } = null!;

    protected override async Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<!DOCTYPE html><html lang=\"en\">");
        // render <head> ...
        WriteHtml("<body>");
        await RenderSlotAsync(); // page content goes here
        WriteHtml("</body></html>");
    }
}
```

## Adding a CSS theme

Use `[GlobalStyle]` and `[Styles(...)]` to embed CSS:

```csharp
[GlobalStyle]
[Styles(Reset + Tokens + Layout)]
public sealed class MyTheme : AtollComponent
{
    private const string Reset = "*, *::before, *::after { box-sizing: border-box; }";
    private const string Tokens = ":root { --color-bg: #fff; }";
    private const string Layout = "body { margin: 0; font-family: system-ui; }";

    protected override Task RenderCoreAsync(RenderContext context) => Task.CompletedTask;
}
```

That's all for this series. Check the Reef source code for a complete real-world example.
