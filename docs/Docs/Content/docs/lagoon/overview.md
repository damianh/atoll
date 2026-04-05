---
title: Lagoon Overview
description: A documentation theme addon for Atoll, inspired by Astro Starlight.
order: 20
section: Lagoon Plugin
---

# Lagoon Overview

:::aside{type="tip" title="Starlight for .NET"}
Lagoon is the official documentation theme addon for Atoll. Add it to your Atoll project to get a full documentation site — with sidebar navigation, search, dark mode, breadcrumbs, pagination, and a responsive layout — without building any of it yourself. It is the Atoll equivalent of [Astro Starlight](https://starlight.astro.build).
:::

> **Note**: The `:::aside{...}` syntax above can also be written as PascalCase HTML-like tags:
>
> ```md
> <Aside Type="tip" Title="Starlight for .NET">
> Lagoon is the official documentation theme addon for Atoll...
> </Aside>
> ```
>
> Both syntaxes coexist — use whichever you prefer.

## What's included

:::card-grid{stagger=true}
:::card{title="Layout"}
Full-page HTML shell with header, sidebar, main content, TOC, and footer.
:::
:::card{title="Splash Layout"}
Full-width, sidebar-free layout for landing pages (`SplashLayout`).
:::
:::card{title="Sidebar Navigation"}
Manual links, collapsible groups, auto-generated sections, badges.
:::
:::card{title="Site Search"}
Build-time JSON index + client-side `Ctrl+K` search dialog.
:::
:::card{title="Dark Mode"}
System-preference detection with manual toggle, persisted to localStorage.
:::
:::card{title="Mobile Nav"}
Responsive hamburger menu that activates at narrow viewports.
:::
:::card{title="Breadcrumbs"}
Auto-generated trail from the sidebar tree.
:::
:::card{title="Pagination"}
Previous / Next links following sidebar order.
:::
:::card{title="Table of Contents"}
"On this page" heading list, configurable level range.
:::
:::card{title="Hero Component"}
Landing-page hero with title, tagline, image, and CTA buttons.
:::
:::card{title="Mermaid Diagrams"}
Opt-in rendering of ` ```mermaid ` code blocks.
:::
:::card{title="Custom CSS"}
Inject additional stylesheets alongside the built-in theme.
:::
:::card{title="Per-page Head Injection"}
Inject analytics, social meta, or custom scripts per page via frontmatter `head:` field.
:::
:::card{title="Social Links"}
GitHub, Discord, Twitter, and more in the header.
:::
:::card{title="Edit Page Links"}
"Edit this page" link with configurable repository URL.
:::
:::card{title="Last Updated Date"}
Per-page last-modified timestamp displayed below content.
:::
:::card{title="Draft Mode"}
Hide draft pages from sidebar navigation and search index.
:::
:::card{title="Badge Variants"}
Colour-coded sidebar badges — note, tip, success, caution, danger.
:::
:::card{title="Custom Footer"}
Replace the default footer with custom text and navigation links.
:::
:::

## Quick start

:::steps
1. **Add the package reference**

   ```xml
   <PackageReference Include="Atoll.Lagoon" Version="*" />
   ```

2. **Declare your content collection**

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

3. **Configure the theme**

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

4. **Create a wrapper layout**

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
                .Select(e => new SidebarEntry(e.Data.Title, $"/docs/{e.Slug}", e.Slug, e.Data.Order, null, false))
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

5. **Configure search**

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
:::

## What's next?

:::card-grid
:::link-card{title="Configuration" href="./configuration" description="Full DocsConfig reference."}
:::
<LinkCard Title="Sidebar Navigation" Href="./sidebar" Description="Manual links, groups, auto-generate." />
:::link-card{title="Theme & Styling" href="./theming" description="Design tokens, dark mode, custom CSS."}
:::
:::link-card{title="Components & Layout" href="./components" description="DocsLayout, Hero, navigation helpers."}
:::
:::link-card{title="Site Search" href="./search" description="Build-time indexing and the search dialog."}
:::
:::link-card{title="Islands & Mermaid" href="./islands-and-mermaid" description="Built-in islands and diagram support."}
:::
:::link-card{title="Starlight Comparison" href="./starlight-comparison" description="Feature parity and known gaps."}
:::
:::
