---
title: Components & Layout
description: DocsLayout, Hero, breadcrumbs, pagination, and table of contents components.
order: 24
section: Lagoon Theme
---

# Components & Layout

Lagoon provides `DocsLayout` as the full-page shell, a `Hero` component for landing pages, and structural navigation components (breadcrumbs, pagination, table of contents). All are `AtollComponent` subclasses.

## `DocsLayout`

`DocsLayout` assembles the entire HTML document, including:

1. `<!DOCTYPE html>` + `<html lang="en">`
2. **`<head>`** via `DocsBaseHead` (charset, viewport, `<title>`, meta description, CSS links)
3. **Header** — logo/title, `SearchDialog` island, social links, `ThemeToggle` island
4. **Mobile nav** — `MobileNav` island (only activates on narrow viewports)
5. **Sidebar** — resolved navigation tree
6. **Main content** — breadcrumbs, page heading, default slot (your page content), pagination
7. **Table of contents** — "On this page" heading list

### Parameters

| Parameter | Type | Required | Description |
|---|---|---|---|
| `Config` | `DocsConfig` | ✅ | Site configuration |
| `PageTitle` | `string` | | Page-specific title appended to the site title |
| `PageDescription` | `string?` | | Meta description for this page |
| `Headings` | `IReadOnlyList<MarkdownHeading>` | | Headings for the TOC (from rendered markdown) |
| `SidebarItems` | `IReadOnlyList<ResolvedSidebarItem>` | | Resolved sidebar tree (from `SidebarBuilder`) |
| `Previous` | `PaginationLink?` | | Previous page link (from `PaginationResolver`) |
| `Next` | `PaginationLink?` | | Next page link (from `PaginationResolver`) |
| `BreadcrumbItems` | `IReadOnlyList<BreadcrumbItem>` | | Breadcrumb trail (from `BreadcrumbBuilder`) |

The **default slot** receives your page content.

## `DocsBaseHead`

`DocsBaseHead` renders the `<head>` section. It is used internally by `DocsLayout` and accepts the same `Config`, `PageTitle`, and `PageDescription` parameters. You rarely need to use it directly.

It emits:
- `<meta charset="UTF-8">` and viewport meta
- `<title>PageTitle | SiteTitle</title>` (or just `SiteTitle` when `PageTitle` is empty)
- `<meta name="description">` from `PageDescription ?? Config.Description`
- `<link rel="stylesheet">` for each entry in `Config.CustomCss`

## `Hero`

Use `Hero` on landing pages for a visually prominent introduction:

```csharp
await RenderAsync(ComponentRenderer.ToFragment<Hero>(new Dictionary<string, object?>
{
    ["Title"]    = "My Project",
    ["Tagline"]  = "Fast, lightweight, .NET-native.",
    ["ImageSrc"] = "/images/hero.png",
    ["ImageAlt"] = "Diagram showing the architecture",
    ["Actions"]  = new[]
    {
        new HeroAction("Get Started", "/docs/getting-started"),
        new HeroAction("GitHub", "https://github.com/me/my-project", HeroActionVariant.Secondary),
    },
}));
```

### `Hero` parameters

| Parameter | Type | Required | Description |
|---|---|---|---|
| `Title` | `string` | ✅ | Large heading text |
| `Tagline` | `string?` | | Subtitle paragraph |
| `ImageSrc` | `string?` | | URL of the hero image |
| `ImageAlt` | `string` | | Alt text for the hero image |
| `Actions` | `IReadOnlyList<HeroAction>` | | Call-to-action buttons |

### `HeroAction`

| Property | Type | Description |
|---|---|---|
| `Label` | `string` | Button text |
| `Href` | `string` | Destination URL |
| `Variant` | `HeroActionVariant` | `Primary` (filled) or `Secondary` (outlined) |

Constructors:
```csharp
new HeroAction("Get Started", "/docs/getting-started")                       // Primary
new HeroAction("View Source", "https://github.com/...", HeroActionVariant.Secondary)
```

## Navigation helpers

These classes are used in your wrapper layout to compute the values passed to `DocsLayout`.

### `SidebarBuilder`

Resolves `DocsConfig.Sidebar` into a `ResolvedSidebarItem` tree, marking the active item and populating auto-generated groups from content entries:

```csharp
var builder = new SidebarBuilder(config.Sidebar, entries);
var sidebarItems = builder.Build(currentHref);
```

### `PaginationResolver`

Flattens the sidebar tree into a sequential list and finds the previous and next items relative to the current page:

```csharp
var resolver = new PaginationResolver(sidebarItems, flatten: true);
var pagination = resolver.Resolve(currentHref);
// pagination.Previous — PaginationLink? (Label, Href)
// pagination.Next     — PaginationLink? (Label, Href)
```

### `BreadcrumbBuilder`

Builds the breadcrumb trail by walking the sidebar tree from the root to the active item:

```csharp
var builder = new BreadcrumbBuilder(sidebarItems);
var crumbs = builder.Build(currentHref);
// IReadOnlyList<BreadcrumbItem> — each has Label and Href
```

## Integration pattern

The typical integration uses a thin wrapper layout (`SiteLayout`) that wires the navigation helpers and passes the results to `DocsLayout`:

```csharp
// Layouts/SiteLayout.cs
public sealed class SiteLayout : AtollComponent
{
    [Parameter(Required = true)]
    public CollectionQuery Query { get; set; } = null!;

    [Parameter]
    public string Slug { get; set; } = "";

    [Parameter]
    public string PageTitle { get; set; } = "";

    [Parameter]
    public string? PageDescription { get; set; }

    [Parameter]
    public IReadOnlyList<MarkdownHeading> Headings { get; set; } = [];

    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var config = DocsSetup.Config;
        var currentHref = $"/docs/{Slug}";

        var entries = Query.GetCollection<DocSchema>("docs")
            .Select(e => new SidebarEntry(e.Data.Title, $"/docs/{e.Slug}", e.Slug, e.Data.Order, null))
            .ToList();

        var sidebarItems  = new SidebarBuilder(config.Sidebar, entries).Build(currentHref);
        var pagination    = new PaginationResolver(sidebarItems, flatten: true).Resolve(currentHref);
        var breadcrumbs   = new BreadcrumbBuilder(sidebarItems).Build(currentHref);
        var pageSlot      = context.Slots.GetSlotFragment(SlotCollection.DefaultSlotName);

        var addonProps = new Dictionary<string, object?>
        {
            ["Config"]          = config,
            ["PageTitle"]       = PageTitle,
            ["PageDescription"] = PageDescription,
            ["Headings"]        = Headings,
            ["SidebarItems"]    = sidebarItems,
            ["Previous"]        = pagination.Previous,
            ["Next"]            = pagination.Next,
            ["BreadcrumbItems"] = breadcrumbs,
        };

        await RenderAsync(ComponentRenderer.ToFragment<DocsLayout>(addonProps,
            SlotCollection.FromDefault(pageSlot)));
    }
}
```

Your docs page then uses `SiteLayout` as its layout:

```csharp
[Layout(typeof(SiteLayout))]
public sealed class GuidePage : AtollComponent, IAtollPage { /* ... */ }
```
