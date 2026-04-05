---
title: Starlight Comparison
description: Feature comparison and gap analysis between Lagoon and Astro Starlight.
order: 27
section: Lagoon Plugin
---

# Starlight Comparison

This page maps Lagoon's features against [Astro Starlight](https://starlight.astro.build), the reference documentation theme in the Astro ecosystem. It is a roadmap reference — not a criticism. Lagoon is early-stage and intentionally focused on a solid, production-ready core before expanding. Many of the gaps listed here are planned for future releases.

## What Lagoon supports today

These features exist in Lagoon and are fully documented in this section.

| Feature | Implementation | Notes |
|---|---|---|
| Configuration | `DocsConfig` | Comparable to Starlight's `starlight()` config |
| Sidebar navigation | `SidebarBuilder` | Manual items, groups, auto-generate, badges, collapse |
| Site search | `SearchDialog` + `LagoonSearchIndexGenerator` | Build-time JSON index, client-side search dialog |
| Dark mode | `ThemeToggle` island | `localStorage` persistence, `prefers-color-scheme` fallback |
| Mobile navigation | `MobileNav` island | Responsive hamburger menu with focus trapping |
| Breadcrumbs | `BreadcrumbBuilder` + `Breadcrumbs` component | Auto-generated from the sidebar tree |
| Pagination | `PaginationResolver` + `Pagination` component | Prev/Next links following sidebar order |
| Table of contents | `TableOfContents` component | Configurable heading-level range |
| Hero component | `Hero` | Title, tagline, image, primary and secondary CTA buttons |
| Mermaid diagrams | `MermaidExtension` | Opt-in via `EnableMermaid = true` |
| Custom CSS | `DocsConfig.CustomCss` | Additional stylesheets loaded on every page |
| Social links | `SocialLink` + `SocialIcon` | 8 platform icons in the header |
| Splash / landing page template | `SplashLayout` | Full-width, sidebar-free layout for landing pages |
| Per-page head injection | `DocsBaseHead.PageHeadContent` | Raw HTML injection from frontmatter `head:` field |
| Internationalisation (i18n) | `LocaleConfig`, `UiTranslations`, `BuiltInTranslations`, `LanguagePicker` | 8 built-in languages, locale routing, UI string translation, RTL support |
| Content components | `Aside`, `Card`, `CardGrid`, `Steps`, `Tabs`, `FileTree`, `LinkCard`, `LinkButton`, `Icon` | 10 content components for rich documentation authoring |
| Edit page links | `DocsConfig.EditUrl` + `DocsLayout.PageSlug` | "Edit this page" link rendered below content |
| Last updated date | `DocsLayout.LastUpdated` | Per-page last-modified timestamp in the content footer |
| Draft mode | `SidebarEntry.Draft` + `SidebarBuilder` auto-filter | Draft pages excluded from sidebar and (optionally) search |
| Sidebar badge variants | `SidebarBadge` + `BadgeVariant` enum | 6 colour variants: Default, Note, Tip, Success, Caution, Danger |
| Custom footer | `FooterConfig` + `FooterLink` | Configurable footer text and navigation links |
| Favicon configuration | `DocsConfig.FaviconHref` | Custom favicon path or URL |

## Minor gaps

No minor gaps remain. All six features previously listed here have been implemented.

## Notable gaps

Significant features that Starlight has but Lagoon does not currently support. These may matter depending on your project's requirements.

| Feature | Starlight | Lagoon | Notes |
|---|---|---|---|
| Component overrides | Replace any built-in UI component by path | No override mechanism | Layout and UI are not customisable without forking |
| Plugin system | Plugin API for extending Starlight at build and runtime | No plugin architecture | — |
| Expressive Code blocks | Syntax highlighting with frames, markers, diffs, and collapsible sections | Standard code blocks only | — |
| Versioned documentation | Version selector for multiple doc versions simultaneously | Not implemented | — |
| Route data API | Typed `StarlightPage` route data accessible in components | No equivalent API | — |

## Summary

Lagoon covers the core of what you need to build a .NET documentation site: layout, navigation, search, dark mode, i18n, content components, and a responsive design. If your project requires component customisation or a plugin architecture, you will hit the notable gaps above.

Use the [GitHub issues](https://github.com/damianh/atoll/issues) page to request features or track progress.
