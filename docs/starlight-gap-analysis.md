# Starlight Functional Gap Analysis

Lagoon vs [Astro Starlight](https://starlight.astro.build) — extracted from the docs site comparison page.

## Feature Parity (Lagoon supports today)

| Feature | Lagoon Implementation | Starlight Equivalent |
|---|---|---|
| Configuration | `DocsConfig` | `starlight()` config |
| Sidebar navigation | `SidebarBuilder` — manual items, groups, auto-generate, badges, collapse | `sidebar` config with autogenerate, badges, groups |
| Site search | `SearchDialog` + `LagoonSearchIndexGenerator` — build-time JSON index, client-side dialog | Pagefind integration |
| Dark mode | `ThemeToggle` island — `localStorage` persistence, `prefers-color-scheme` fallback | Built-in theme toggle |
| Mobile navigation | `MobileNav` island — responsive hamburger with focus trapping | Built-in responsive nav |
| Breadcrumbs | `BreadcrumbBuilder` + `Breadcrumbs` component — auto-generated from sidebar tree | Built-in breadcrumbs |
| Pagination | `PaginationResolver` + `Pagination` component — prev/next from sidebar order | Built-in prev/next |
| Table of contents | `TableOfContents` component — configurable heading-level range | Built-in TOC |
| Hero component | `Hero` — title, tagline, image, primary/secondary CTA buttons | `hero` frontmatter |
| Mermaid diagrams | `MermaidExtension` — opt-in via `EnableMermaid = true` | Via plugin/integration |
| Custom CSS | `DocsConfig.CustomCss` — additional stylesheets on every page | `customCss` config |
| Social links | `SocialLink` + `SocialIcon` — 8 platform icons in header | `social` config |

## Minor Gaps

Limited impact on most documentation sites.

| Feature | Starlight | Lagoon Status | Workaround |
|---|---|---|---|
| Edit page links | "Edit this page on GitHub" link per page | Not implemented | Could be added as a `DocsConfig` option |
| Last updated date | Shows last modified timestamp from git | Not implemented | None |
| Draft mode | `draft: true` frontmatter hides pages from nav/build | Not implemented | Content filtering in `ISearchIndexConfiguration` / sidebar config |
| Sidebar badge variants | Colour variants (success, caution, tip, danger) | Text-only badges (`SidebarItem.Badge` is `string?`) | None — no colour support |
| Custom footer content | Configurable footer text and links | Hardcoded footer text in `DocsLayout` | Fork/override `DocsLayout` |
| Favicon configuration | `favicon` option in site config | Not in `DocsConfig` | Add a custom `<link>` via head override or `CustomCss` workaround |

## Notable Gaps

Significant features that may block adoption depending on project requirements.

| Feature | Starlight | Lagoon Status | Impact |
|---|---|---|---|
| Internationalisation (i18n) | 30+ languages, locale routing, UI string translation, RTL support | No i18n support | Single-language sites only; blocks any multilingual project |
| Component overrides | Replace any built-in UI component by path | No override mechanism | Layout/UI not customisable without forking Lagoon source |
| Plugin system | Plugin API for extending Starlight at build and runtime | No plugin architecture | Extensions must be built as standalone Atoll components |
| Rich content components | Cards, Tabs, Asides/Callouts, Steps, FileTree, LinkCards, CardGrids, LinkButtons, Icons, expressive Code blocks (title, highlights, line numbers) | `Hero` only — structural components are layout, not content authoring | Inline components must be written from scratch as Atoll components |
| Splash / landing page template | `template: splash` for wide, sidebar-free pages | No template variants — `Hero` exists but no separate page template | Landing pages require manual layout work |
| Versioned documentation | Version selector for multiple doc versions simultaneously | Not implemented | No way to serve multiple versions side-by-side |
| Per-page head / script injection | `head:` frontmatter field for custom `<head>` tags per page | `DocsBaseHead` is a fixed structure | Cannot inject analytics, social meta, or custom scripts per page |
| Route data API | Typed `StarlightPage` route data accessible in components | No equivalent API | Components cannot access structured page metadata |

## Summary

**Parity**: 12 features fully covered.
**Minor gaps**: 6 — small convenience features, most have workarounds.
**Notable gaps**: 8 — significant features that affect extensibility, internationalisation, and content authoring richness.

Lagoon covers the core requirements for a single-language .NET documentation site. The notable gaps primarily affect projects needing i18n, deep UI customisation, or rich inline content components.
