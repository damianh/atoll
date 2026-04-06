---
title: Sidebar Navigation
description: Configure sidebar items — manual links, groups, auto-generated sections, badges, and collapse.
order: 22
section: Lagoon Plugin
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

Add a badge to any sidebar item to surface a label (e.g. `"New"`, `"Beta"`). A badge can be a plain string or a `SidebarBadge` with a colour variant.

### Plain text badge

```csharp
new SidebarItem { Label = "Search", Link = "/docs/search", Badge = "New" }
```

Plain strings are implicitly converted to a `SidebarBadge` with `BadgeVariant.Default`, so existing code continues to work without changes.

### Coloured badge

```csharp
new SidebarItem { Label = "v1 API", Link = "/docs/v1-api", Badge = new SidebarBadge("Deprecated", BadgeVariant.Danger) }
```

### `BadgeVariant` values

| Value | Colour | Use for |
|---|---|---|
| `Default` | Accent | General-purpose labels |
| `Note` | Blue | Informational notes |
| `Tip` | Green | Tips and recommendations |
| `Success` | Green | Positive / recommended |
| `Caution` | Amber | Proceed with care |
| `Danger` | Red | Deprecation, breaking changes |

See [Configuration](./configuration#sidebarbadge) for the full `SidebarBadge` API reference.

## Draft mode

Mark a content entry as a draft to exclude it from auto-generated sidebar groups and (optionally) the search index. Draft pages are still accessible by direct URL — they are hidden from navigation, not from the site.

### Creating draft entries

Use the 6-parameter `SidebarEntry` constructor with `draft: true`:

```csharp
var entries = Query.GetCollection<DocSchema>("docs")
    .Select(e => new SidebarEntry(
        label: e.Data.Title,
        href:  $"/docs/{e.Slug}",
        slug:  e.Slug,
        order: e.Data.Order,
        badge: null,
        draft: e.Data.Draft))   // ← from your frontmatter schema
    .ToList();
```

`SidebarBuilder` automatically filters out entries where `Draft == true` when populating auto-generated groups. No additional filtering is needed on the sidebar side.

### Search index filtering

Draft entries should also be excluded from the search index. Filter them in your `ISearchIndexConfiguration`:

```csharp
public IEnumerable<SearchDocumentInput> GetDocuments(CollectionQuery query)
{
    foreach (var entry in query.GetCollection<DocSchema>("docs"))
    {
        var rendered = query.Render(entry);
        yield return new SearchDocumentInput(entry.Data.Title, $"/docs/{entry.Slug}")
        {
            Description = entry.Data.Description,
            HtmlBody    = rendered.Html,
            Draft       = entry.Data.Draft,  // ← caller-side marker
        };
    }
}
```

The `SearchDocumentInput.Draft` property is a caller-side marker — callers are responsible for filtering draft documents before passing them to the search index generator.

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
        badge: null,
        draft: false))
    .ToList();

var sidebarItems = new SidebarBuilder(config.Sidebar, entries).Build(currentHref);
```

Pass the resolved `sidebarItems` to `DocsLayout` via the `SidebarItems` parameter.

## Per-version sidebars

When versioned documentation is enabled, you can define a different sidebar for each version by setting `VersionConfig.Sidebar`. When `null`, the version falls back to `DocsConfig.Sidebar`.

```csharp
Versions = new Dictionary<string, VersionConfig>
{
    ["current"] = new VersionConfig { Label = "Latest" },
    ["v1.0"] = new VersionConfig
    {
        Label    = "v1.0",
        Sidebar  =
        [
            new SidebarItem { Label = "Introduction", Link = "/docs/v1.0/intro" },
            new SidebarItem { Label = "Getting Started", Link = "/docs/v1.0/getting-started" },
        ],
    },
}
```

At render time, `SidebarBuilder` accepts `versionPrefix` and `versionKey` parameters so it can filter auto-generated entries to the current version and prefix all hrefs correctly. Use the 6-parameter `Build()` overload when both locale and version resolution are active:

```csharp
var sidebarItems = new SidebarBuilder(config.Sidebar, entries)
    .Build(currentHref, localePrefix, basePath, localeKey, versionPrefix, versionKey);
```

See [Versioned Documentation](./versioning) for full configuration details.

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
