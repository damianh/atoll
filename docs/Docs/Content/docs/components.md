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
        var description = string.IsNullOrEmpty(Description)
            ? ""
            : $"<p>{HtmlEncoder.Encode(Description)}</p>";

        WriteHtml($"""
            <div class="card">
                <h2>{HtmlEncoder.Encode(Title)}</h2>
                {description}
            </div>
            """);
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
        WriteHtml($"<li>{HtmlEncoder.Encode(item.Name)}</li>");
    }
    WriteHtml("</ul>");
}
```

## Razor templates

For markup-heavy components, you can delegate rendering to a Razor `.cshtml` template instead of building HTML strings in C#. The component keeps its `[Parameter]` properties and logic; the template owns the markup.

### Project setup

Your project needs the Razor SDK and the RazorSlices package:

```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">
  <PropertyGroup>
    <AddRazorSupportForMvc>true</AddRazorSupportForMvc>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="RazorSlices" />
  </ItemGroup>
</Project>
```

Add a `_ViewImports.cshtml` at the project root to disable MVC tag helpers (which conflict with plain HTML):

```html
@tagHelperPrefix __disable_tagHelpers__:
@removeTagHelper *, Microsoft.AspNetCore.Mvc.Razor
```

### Pattern

A Razor-templated component has three files:

1. **Component** (`Card.cs`) — declares parameters, builds the model, calls `RenderSliceAsync`
2. **Model** (`CardModel.cs`) — a record carrying data from the component to the template
3. **Template** (`CardTemplate.cshtml`) — the Razor markup

**Model:**

```csharp
public sealed record CardModel(string Title, IconName? IconName);
```

**Component:**

```csharp
public sealed class Card : AtollComponent
{
    [Parameter(Required = true)]
    public string Title { get; set; } = "";

    [Parameter]
    public IconName? IconName { get; set; }

    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var model = new CardModel(Title, IconName);

        // Pass the default slot through to the template.
        var slot = context.Slots.GetSlotFragment(SlotCollection.DefaultSlotName);
        var templateSlots = SlotCollection.FromDefault(slot);

        await ComponentRenderer.RenderSliceAsync<CardTemplate, CardModel>(
            context.Destination,
            model,
            templateSlots);
    }
}
```

**Template (`CardTemplate.cshtml`):**

```html
@inherits Atoll.Components.AtollSlice<MyApp.Components.CardModel>
<div class="card">
    <h3 class="card-title">@Model.Title</h3>
    <div class="card-body">
        @{ await RenderSlotAsync(); }
    </div>
</div>
```

### Template base classes

| Base class | Use for | Key method |
|---|---|---|
| `AtollSlice<TModel>` | Components | `RenderSlotAsync()` — renders the default slot |
| `AtollLayoutSlice<TModel>` | Layouts | `RenderBodyAsync()` — renders the page body |
| `AtollSlice` (no generic) | Slot-only components (no model) | `RenderSlotAsync()` |

### Template helpers

Inside a Razor template, the following helpers are available:

| Helper | Description |
|---|---|
| `@Model.Property` | Access model data (auto HTML-escaped) |
| `await RenderSlotAsync()` | Render the default slot |
| `await RenderSlotAsync("name")` | Render a named slot |
| `HasSlot("name")` | Check if a named slot exists |
| `await RenderComponentAsync<T>(props)` | Render a child C# component inline |
| `WriteLiteral(html)` | Write raw, unescaped HTML |

### When to use Razor templates

Use Razor templates when the component is **markup-heavy** — lots of HTML with conditional attributes, loops over lists, and nested elements. The Razor syntax is more readable than `WriteHtml`/`WriteText` chains.

Keep using `RenderCoreAsync` with `WriteHtml`/`WriteText` when:

- The component has complex control flow or significant C# logic interleaved with rendering
- The component is an island with client directives (`[ClientIdle]`, `[ClientLoad]`) — the component *itself* stays in C#, but parent templates can render islands via `RenderComponentAsync<T>(props)` (island wrapping is automatic)
