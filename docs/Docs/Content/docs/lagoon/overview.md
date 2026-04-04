---
title: Lagoon Overview
description: A documentation theme addon for Atoll, inspired by Astro Starlight.
order: 20
section: Lagoon Theme
---

# Lagoon Overview

Lagoon is the official documentation theme addon for Atoll. Add it to your Atoll project to get a full documentation site — with sidebar navigation, search, dark mode, breadcrumbs, pagination, and a responsive layout — without building any of it yourself. It is the Atoll equivalent of [Astro Starlight](https://starlight.astro.build).

## What's included

| Feature | Description |
|---|---|
| **Layout** | Full-page HTML shell with header, sidebar, main content, TOC, and footer |
| **Splash layout** | Full-width, sidebar-free layout for landing pages (`SplashLayout`) |
| **Sidebar navigation** | Manual links, collapsible groups, auto-generated sections, badges |
| **Site search** | Build-time JSON index + client-side `Ctrl+K` search dialog |
| **Dark mode** | System-preference detection with manual toggle, persisted to localStorage |
| **Mobile nav** | Responsive hamburger menu that activates at narrow viewports |
| **Breadcrumbs** | Auto-generated trail from the sidebar tree |
| **Pagination** | Previous / Next links following sidebar order |
| **Table of contents** | "On this page" heading list, configurable level range |
| **Hero component** | Landing-page hero with title, tagline, image, and CTA buttons |
| **Mermaid diagrams** | Opt-in rendering of ` ```mermaid ` code blocks |
| **Custom CSS** | Inject additional stylesheets alongside the built-in theme |
| **Per-page head injection** | Inject analytics, social meta, or custom scripts per page via frontmatter `head:` field |
| **Social links** | GitHub, Discord, Twitter, and more in the header |

## Quick start

### 1. Add the package reference

```xml
<PackageReference Include="Atoll.Lagoon" Version="*" />
```

### 2. Declare your content collection

```csharp
// ContentConfig.cs
using Atoll.Build.Content.Collections;

public sealed class ContentConfig : IContentConfiguration
{
    public CollectionConfig Configure() =>
        new CollectionConfig("Content")
            .AddCollection(ContentCollection.Define<DocSchema>("docs"));
}
```

Your frontmatter schema (`DocSchema`) needs at minimum a `Title` and `Description` property.

### 3. Configure the theme

```csharp
// DocsSetup.cs
using Atoll.Lagoon.Configuration;

public static class DocsSetup
{
    public static DocsConfig Config { get; } = new DocsConfig
    {
        Title = "My Project",
        Description = "Documentation for My Project.",
        Social =
        [
            new SocialLink("GitHub", "https://github.com/me/my-project", SocialIcon.GitHub),
        ],
        Sidebar =
        [
            new SidebarItem { Label = "Getting Started", Link = "/docs/getting-started" },
            new SidebarItem
            {
                Label = "Guides",
                Items =
                [
                    new SidebarItem { Label = "Installation", Link = "/docs/installation" },
                    new SidebarItem { Label = "Configuration", Link = "/docs/configuration" },
                ],
            },
        ],
    };
}
```

### 4. Create a wrapper layout

```csharp
// Layouts/SiteLayout.cs
using Atoll.Build.Content.Collections;
using Atoll.Components;
using Atoll.Lagoon.Navigation;
using Atoll.Slots;
using AddonLayout = Atoll.Lagoon.Layouts.DocsLayout;

public sealed class SiteLayout : AtollComponent
{
    [Parameter(Required = true)]
    public CollectionQuery Query { get; set; } = null!;

    [Parameter]
    public string Slug { get; set; } = "";

    [Parameter]
    public string PageTitle { get; set; } = "";

    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var config = DocsSetup.Config;
        var currentHref = $"/docs/{Slug}";

        var entries = Query.GetCollection<DocSchema>("docs")
            .Select(e => new SidebarEntry(e.Data.Title, $"/docs/{e.Slug}", e.Slug, e.Data.Order, null))
            .ToList();

        var sidebarItems = new SidebarBuilder(config.Sidebar, entries).Build(currentHref);
        var pagination = new PaginationResolver(sidebarItems, flatten: true).Resolve(currentHref);
        var breadcrumbs = new BreadcrumbBuilder(sidebarItems).Build(currentHref);

        var pageSlot = context.Slots.GetSlotFragment(SlotCollection.DefaultSlotName);
        var addonSlots = SlotCollection.FromDefault(pageSlot);

        var addonProps = new Dictionary<string, object?>
        {
            ["Config"]          = config,
            ["PageTitle"]       = PageTitle,
            ["SidebarItems"]    = sidebarItems,
            ["Previous"]        = pagination.Previous,
            ["Next"]            = pagination.Next,
            ["BreadcrumbItems"] = breadcrumbs,
        };

        await RenderAsync(ComponentRenderer.ToFragment<AddonLayout>(addonProps, addonSlots));
    }
}
```

### 5. Configure search

```csharp
// SearchConfig.cs
using Atoll.Lagoon.Search;

public sealed class SearchConfig : ISearchIndexConfiguration
{
    public IEnumerable<SearchDocumentInput> GetDocuments(CollectionQuery query)
    {
        foreach (var entry in query.GetCollection<DocSchema>("docs"))
        {
            var rendered = query.Render(entry);
            yield return new SearchDocumentInput(entry.Data.Title, $"/docs/{entry.Slug}")
            {
                Description = entry.Data.Description,
                HtmlBody    = rendered.Html,
            };
        }
    }
}
```

## What's next?

- [Configuration](./configuration) — full `DocsConfig` reference
- [Sidebar Navigation](./sidebar) — manual links, groups, auto-generate
- [Theme & Styling](./theming) — design tokens, dark mode, custom CSS
- [Components & Layout](./components) — `DocsLayout`, `Hero`, navigation helpers
- [Site Search](./search) — build-time indexing and the search dialog
- [Islands & Mermaid](./islands-and-mermaid) — built-in islands and diagram support
- [Starlight Comparison](./starlight-comparison) — feature parity and known gaps
