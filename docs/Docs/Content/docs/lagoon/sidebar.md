---
title: Sidebar Navigation
description: Configure sidebar items — manual links, groups, auto-generated sections, badges, and collapse.
order: 22
section: Lagoon Theme
---

# Sidebar Navigation

The sidebar is configured via `DocsConfig.Sidebar`, a list of `SidebarItem` objects. Items can be simple leaf links, manually-defined groups, or groups whose children are auto-generated from your content collection.

At render time, `SidebarBuilder` resolves the configuration into a `ResolvedSidebarItem` tree, marks the active item for the current page, and passes it to `DocsLayout`.

## Item types

### Leaf link

A `SidebarItem` with a `Link` renders as a direct navigation link:

```csharp
new SidebarItem { Label = "Getting Started", Link = "/docs/getting-started" }
```

### Manual group

Omit `Link` and provide `Items` to create a collapsible section with manually-ordered children:

```csharp
new SidebarItem
{
    Label = "Guides",
    Items =
    [
        new SidebarItem { Label = "Installation",  Link = "/docs/installation" },
        new SidebarItem { Label = "Configuration", Link = "/docs/configuration" },
        new SidebarItem { Label = "Deployment",    Link = "/docs/deployment" },
    ],
}
```

Groups are expanded by default. Set `Collapsed = true` to start them closed:

```csharp
new SidebarItem
{
    Label     = "Advanced",
    Collapsed = true,
    Items     = [ /* ... */ ],
}
```

### Auto-generated group

Set `AutoGenerate` to a directory slug to populate the group's children from your content entries. The children are sorted by the `Order` value on each entry and their labels come from the entry title:

```csharp
new SidebarItem
{
    Label        = "API Reference",
    AutoGenerate = "reference",   // matches entries whose slug starts with "reference/"
}
```

Auto-generated and manually-defined items cannot be mixed in the same group. If `AutoGenerate` is set, `Items` is ignored.

## Badges

Add a short badge text to any item to surface a label (e.g. `"New"`, `"Beta"`):

```csharp
new SidebarItem { Label = "Search", Link = "/docs/search", Badge = "New" }
```

Badges are plain text; color variants are not yet supported (see [Starlight Comparison](./starlight-comparison)).

## Active state

`SidebarBuilder` marks an item as active when its `Link` matches the current page path. Matching is case-insensitive and tolerates a trailing slash on either side, so `/docs/configuration`, `/docs/configuration/`, and `/DOCS/CONFIGURATION` all match the same item.

The active item and its ancestor groups are automatically expanded regardless of the `Collapsed` setting.

## Integration: wiring SidebarEntry

`SidebarBuilder` takes two arguments: the sidebar configuration from `DocsConfig` and a list of `SidebarEntry` objects derived from your content collection. A `SidebarEntry` provides the data that auto-generated groups use to populate their children:

```csharp
var entries = Query.GetCollection<DocSchema>("docs")
    .Select(e => new SidebarEntry(
        label: e.Data.Title,
        href:  $"/docs/{e.Slug}",
        slug:  e.Slug,
        order: e.Data.Order,
        badge: null))
    .ToList();

var sidebarItems = new SidebarBuilder(config.Sidebar, entries).Build(currentHref);
```

Pass the resolved `sidebarItems` to `DocsLayout` via the `SidebarItems` parameter.

## Nested groups

Groups can be nested to any depth:

```csharp
new SidebarItem
{
    Label = "Framework",
    Items =
    [
        new SidebarItem
        {
            Label = "Core",
            Items =
            [
                new SidebarItem { Label = "Components", Link = "/docs/components" },
                new SidebarItem { Label = "Layouts",    Link = "/docs/layouts" },
            ],
        },
        new SidebarItem
        {
            Label = "Extensions",
            Items =
            [
                new SidebarItem { Label = "Islands", Link = "/docs/islands" },
            ],
        },
    ],
}
```
