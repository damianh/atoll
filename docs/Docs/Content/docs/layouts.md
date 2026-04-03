---
title: Layouts
description: Wrap pages with shared HTML structure using the Layout attribute.
order: 4
section: Basics
---

# Layouts

Layouts wrap pages with shared structure — the HTML shell, navigation, header, footer. A layout renders its page content via `RenderSlotAsync()`.

## Creating a layout

```csharp
using Atoll.Components;

public sealed class MainLayout : AtollComponent
{
    [Parameter]
    public string Title { get; set; } = "My Site";

    protected override async Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("""
            <!DOCTYPE html>
            <html lang="en">
            <head>
                <meta charset="utf-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1" />
                <title>
            """);
        WriteText(Title);
        WriteHtml("""
                </title>
            </head>
            <body>
                <nav><a href="/">Home</a> | <a href="/about">About</a></nav>
                <main>
            """);
        await RenderSlotAsync();
        WriteHtml("""
                </main>
                <footer>Built with Atoll</footer>
            </body>
            </html>
            """);
    }
}
```

## Applying a layout to a page

Use `[Layout(typeof(...))]` on any page component:

```csharp
[Layout(typeof(MainLayout))]
[PageRoute("/about")]
public sealed class AboutPage : AtollComponent, IAtollPage
{
    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<h1>About</h1><p>This is the about page.</p>");
        return Task.CompletedTask;
    }
}
```

The layout renders the page's output wherever `RenderSlotAsync()` is called.

## Named slots

Layouts can define named slots for injecting content into specific regions:

```csharp
// In the layout:
await RenderSlotAsync("head-extra");   // page can inject extra <head> content
await RenderSlotAsync();               // default slot — page body

// In the page:
// Named slots are rendered via RenderContext.DefineSlot (advanced usage)
```

## Layout parameters

Layout parameters are passed automatically when the page has matching parameter names. For example, a page with a `Title` property will pass it to the layout's `Title` parameter.

## Nested layouts

Layouts can be nested by applying `[Layout]` to a layout component itself, enabling multi-level wrapping (e.g., a docs layout inside a site-wide shell).
