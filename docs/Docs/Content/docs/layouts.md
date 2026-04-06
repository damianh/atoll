---
title: Layouts
description: Wrap pages with shared HTML structure using the Layout attribute.
order: 4
section: Basics
---

# Layouts

Layouts wrap pages with shared structure — the HTML shell, navigation, header, footer. A layout renders its page content via `RenderBodyAsync()` (Razor) or `RenderSlotAsync()` (C#).

## Creating a layout

Layouts are best authored as Razor `.cshtml` templates — the markup-heavy nature of HTML document shells is a natural fit. The C# component keeps its `[Parameter]` properties and logic; the template owns the markup.

A Razor layout has three files:

1. **Layout** (`MainLayout.cs`) — declares parameters, builds the model, forwards the page slot
2. **Model** (`MainLayoutModel.cs`) — a record carrying data to the template
3. **Template** (`MainLayoutTemplate.cshtml`) — the Razor markup

**Model:**

```csharp
public sealed record MainLayoutModel(string Title, string Description);
```

**Layout:**

```csharp
using Atoll.Components;
using Atoll.Slots;

public sealed class MainLayout : AtollComponent
{
    [Parameter]
    public string Title { get; set; } = "My Site";

    [Parameter]
    public string Description { get; set; } = "";

    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var model = new MainLayoutModel(Title, Description);

        // Forward the page content slot to the template.
        var pageSlot = context.Slots.GetSlotFragment(SlotCollection.DefaultSlotName);
        var templateSlots = SlotCollection.FromDefault(pageSlot);

        await ComponentRenderer.RenderSliceAsync<MainLayoutTemplate, MainLayoutModel>(
            context.Destination,
            model,
            templateSlots);
    }
}
```

**Template (`MainLayoutTemplate.cshtml`):**

```html
@inherits Atoll.Components.AtollLayoutSlice<MyApp.Layouts.MainLayoutModel>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>@Model.Title</title>
    @if (!string.IsNullOrEmpty(Model.Description))
    {
        <meta name="description" content="@Model.Description" />
    }
</head>
<body>
    <nav><a href="/">Home</a> | <a href="/about">About</a></nav>
    <main>
        @{ await RenderBodyAsync(); }
    </main>
    <footer>Built with Atoll</footer>
</body>
</html>
```

Key points:

- Use `@inherits AtollLayoutSlice<TModel>` as the base class
- Call `RenderBodyAsync()` where the page content should appear
- `@Model.Property` expressions are automatically HTML-escaped
- See [Components — Razor templates](/docs/components/#razor-templates) for project setup and the full list of template helpers

## Applying a layout to a page

Use `[Layout(typeof(...))]` on any page component:

```csharp
[Layout(typeof(MainLayout))]
[PageRoute("/about")]
public sealed class AboutPage : AtollComponent, IAtollPage
{
    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("""
            <h1>About</h1>
            <p>This is the about page.</p>
            """);
        return Task.CompletedTask;
    }
}
```

The layout renders the page's output wherever `RenderBodyAsync()` (or `RenderSlotAsync()`) is called.

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

## C# inline rendering

For layouts with significant control flow or embedded scripts, you can write the HTML directly in C# instead of using a Razor template:

```csharp
using Atoll.Components;

public sealed class MainLayout : AtollComponent
{
    [Parameter]
    public string Title { get; set; } = "My Site";

    protected override async Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml($"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
                <meta charset="utf-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1" />
                <title>{HtmlEncoder.Encode(Title)}</title>
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

| Method | Description |
|---|---|
| `WriteHtml(string)` | Write trusted HTML directly to output |
| `WriteText(string)` | Write text with automatic HTML-escaping |
| `RenderSlotAsync()` | Render the default slot (page body) |
| `RenderSlotAsync(name)` | Render a named slot |
