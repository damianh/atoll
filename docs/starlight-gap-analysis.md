# Starlight Functional Gap Analysis

Lagoon vs [Astro Starlight](https://starlight.astro.build) — extracted from the docs site comparison page.

## Feature Parity (Lagoon supports today)

| Feature | Lagoon Implementation | Starlight Equivalent |
|---|---|---|
| Configuration | `DocsConfig` | `starlight()` config |
| Sidebar navigation | `SidebarBuilder` — manual items, groups, auto-generate, badges (with colour variants), collapse, draft filtering | `sidebar` config with autogenerate, badges, groups |
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
| Splash / landing page template | `SplashLayout` — full-width, sidebar-free layout for landing pages; Hero component works naturally inside | `template: splash` |
| Per-page head injection | `DocsBaseHead.PageHeadContent` — raw HTML from frontmatter `head:` field | `head:` frontmatter field |
| Edit page links | `DocsConfig.EditUrl` + `DocsLayout.PageSlug` — renders an "Edit page" link below the article | "Edit this page on GitHub" link per page |
| Last updated date | `DocsLayout.LastUpdated` (`DateTimeOffset?`) — renders a "Last updated" date below the article | Shows last modified timestamp from git |
| Draft mode | `SidebarEntry.Draft` — auto-generated sidebar groups filter out draft entries; `SearchDocumentInput.Draft` guides caller-side search index filtering | `draft: true` frontmatter hides pages from nav/build |
| Sidebar badge colour variants | `BadgeVariant` enum (Default, Note, Tip, Success, Caution, Danger) on `SidebarBadge` — renders variant-specific CSS classes reusing aside colour tokens | Colour variants (success, caution, tip, danger) |
| Custom footer content | `DocsConfig.Footer` (`FooterConfig`) — configurable text and link list replacing the default "Built with" footer | Configurable footer text and links |
| Favicon configuration | `DocsConfig.FaviconHref` — sets the `<link rel="icon">` in `DocsBaseHead`; falls back to default Atoll logo | `favicon` option in site config |

## Notable Gaps

Significant features that may block adoption depending on project requirements.

| Feature | Starlight | Lagoon Status | Impact |
|---|---|---|---|
| Internationalisation (i18n) | 30+ languages, locale routing, UI string translation, RTL support | No i18n support | Single-language sites only; blocks any multilingual project |
| Component overrides | Replace any built-in UI component by path | No override mechanism | Layout/UI not customisable without forking Lagoon source |
| Plugin system | Plugin API for extending Starlight at build and runtime | No plugin architecture | Extensions must be built as standalone Atoll components |
| Rich content components | Cards, Tabs, Asides/Callouts, Steps, FileTree, LinkCards, CardGrids, LinkButtons, Icons, expressive Code blocks (title, highlights, line numbers) | `Hero` only — structural components are layout, not content authoring | Inline components must be written from scratch as Atoll components |
| Versioned documentation | Version selector for multiple doc versions simultaneously | Not implemented | No way to serve multiple versions side-by-side |
| Route data API | Typed `StarlightPage` route data accessible in components | No equivalent API | Components cannot access structured page metadata |

## Summary

**Parity**: 20 features fully covered (including all 6 previously identified minor gaps).
**Minor gaps**: 0 — all closed.
**Notable gaps**: 6 — significant features that affect extensibility, internationalisation, and content authoring richness.

Lagoon covers the core requirements for a single-language .NET documentation site. The notable gaps primarily affect projects needing i18n, deep UI customisation, or rich inline content components.
