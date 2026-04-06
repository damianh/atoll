---
title: CSS Scoping
description: Scope CSS to components automatically with the Styles attribute.
order: 8
section: Features
---

# CSS Scoping

Atoll automatically scopes CSS to the component that declares it, using `:where(.atoll-HASH)` selector wrapping. This prevents styles from leaking between components without requiring naming conventions like BEM.

## Applying styles

Use the `[Styles]` attribute on any component:

```csharp
using Atoll.Css;
using Atoll.Components;

[Styles(".card { padding: 1rem; border: 1px solid #ddd; } .card h2 { color: navy; }")]
public sealed class Card : AtollComponent
{
    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("""<div class="card"><h2>Scoped Title</h2></div>""");
        return Task.CompletedTask;
    }
}
```

Atoll will:
1. Generate a hash from the CSS content
2. Add a scope class (e.g., `atoll-a1b2c3`) to the root element
3. Wrap all CSS rules with `:where(.atoll-a1b2c3)` for scoping

## Global styles

Use `[GlobalStyle]` to opt out of scoping. Useful for base styles or third-party overrides:

```csharp
[GlobalStyle("body { font-family: system-ui; } a { color: navy; }")]
public sealed class BaseStyles : AtollComponent
{
    protected override Task RenderCoreAsync(RenderContext context) =>
        Task.CompletedTask; // renders nothing — just injects global CSS
}
```

## CSS aggregation

The asset pipeline aggregates all component CSS into a single stylesheet. During SSG, this outputs a `.css` file that can be linked in the layout's `<head>`.

## How scoping works

Given this input CSS:

```css
.card { padding: 1rem; }
.card h2 { color: navy; }
```

The scoped output becomes:

```css
:where(.atoll-a1b2c3) .card { padding: 1rem; }
:where(.atoll-a1b2c3) .card h2 { color: navy; }
```

The `:where()` pseudo-class has zero specificity, so user agent styles and global overrides still work correctly.

## URL rewriting

Relative URLs in CSS (e.g., `background-image: url(../images/bg.png)`) are automatically rewritten to absolute paths when the CSS is aggregated into a single bundle.
