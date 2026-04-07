# Starlight Functional Gap Analysis

Lagoon vs [Astro Starlight](https://starlight.astro.build) — validated against the
production Duende docs site (`docs.duendesoftware.com`, Astro 5.18 + Starlight 0.37).

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
| Mermaid diagrams | `MermaidExtension` — opt-in via `EnableMermaid = true` | Via plugin/integration (e.g. `starlight-client-mermaid`) |
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
| Internationalisation (i18n) | `LocaleConfig`, `UiTranslations`, `BuiltInTranslations`, `LanguagePicker` — 8 built-in languages, locale routing, UI string translation, RTL support | 30+ languages, locale routing, UI string translation, RTL support |
| Content components | `Aside`, `Card`, `CardGrid`, `Steps`, `Tabs`, `FileTree`, `LinkCard`, `LinkButton`, `Icon` — 10 content components for rich documentation authoring | Cards, Tabs, Asides/Callouts, Steps, FileTree, LinkCards, CardGrids, LinkButtons, Icons |
| Versioned documentation | `VersionConfig`, `VersionPicker`, `VersionResolver` — version selector dropdown, per-version sidebars, deprecated version notices, version-scoped search indices | `starlight-versions` plugin |
| Topic-based search metadata | `SearchDocumentInput.Topics` + `SearchEntry.Topics` — multi-topic tagging with auto-seed from `Section`; client-side topic filter chip bar in search dialog; backward compatible with `section`-only indices | Custom remark plugin adds `data-pagefind-meta="topic:..."` per content area |
| Giscus comments | `Atoll.Giscus` plugin — GitHub Discussions comments with mapping, lazy/eager loading, theming | `starlight-giscus` plugin |
| External link handling | Markdown pipeline auto-detects external links | `rehype-external-links` adds `target="_blank"` + rel attributes |
| Markdown extensions | Footnotes, task lists, auto-links, emphasis extras, GFM tables | CommonMark + GFM via remark |
| Component directives in markdown | `:::Component{prop="value"}` + `<Component Prop="value">` syntax | MDX imports + JSX syntax |
| Code copy buttons | Integrated into syntax highlighting | Starlight default |
| Typed content schemas | C# data annotations with compile-time validation on frontmatter | Zod runtime schema validation in `content.config.ts` |
| Islands architecture | 4 hydration modes: `ClientLoad`, `ClientIdle`, `ClientVisible`, `ClientMedia` | Astro `client:load`, `client:idle`, `client:visible`, `client:media` |

## Lagoon Advantages (ahead of Starlight)

Features Lagoon provides that Starlight does not have built-in or that Duende's Astro site
does not use.

| Feature | Lagoon | Starlight |
|---|---|---|
| Multi-version documentation | `VersionPicker` + `VersionResolver` — first-class built-in | Requires `starlight-versions` third-party plugin |
| Draw.IO diagrams | `Atoll.DrawIo` plugin — embedded viewer with pan/zoom | No equivalent |
| Inline annotations | `Atoll.Annotations` plugin — inline text highlighting and comments | No equivalent |
| Blog / articles theme | `Atoll.Reef` — full blog with RSS, series, tags, authors, multiple view modes | Starlight is docs-only; blog requires separate Astro integration |
| Typed frontmatter | C# data annotations with compile-time validation | Runtime-only Zod validation |
| ASP.NET Core middleware pipeline | Full request pipeline with auth, caching, custom middleware | Astro middleware (limited compared to ASP.NET Core) |

## Notable Gaps

Significant features present in Starlight (or the Duende Astro production site) that Lagoon
lacks. Grouped by priority.

### Must-have for production parity

| Feature | Starlight / Duende Implementation | Lagoon Status | Impact |
|---|---|---|---|
| Redirect system | `redirect_from` frontmatter + config-based redirects + build-time `redirects.json` consumed by ASP.NET Core for 301s | No redirect support | URL migration and SEO preservation impossible without manual server config |
| Link validation (build-time) | `starlight-links-validator` checks all internal links during build | No link validation | Broken links ship undetected |
| Dynamic OpenGraph images | `astro-opengraph-images` + Satori/React TSX template generates branded per-page OG images (category + title + description) | No OG image generation | Social sharing has no preview cards |
| Tab sync across page | `<Tabs syncKey="language">` keeps all tab groups with the same key in sync | Tabs are independent | Multi-language docs require clicking each tab group separately |
| Expressive Code blocks | Syntax highlighting with frames, markers, line diffs (`ins`/`del`), annotations, collapsible sections, titles | Standard code blocks only | Rich code block features must be built manually |

### Should-have

| Feature | Starlight / Duende Implementation | Lagoon Status | Impact |
|---|---|---|---|
| Auto-sidebar from file structure | `starlight-auto-sidebar` — zero-config sidebar generated from directory layout | Manual sidebar config only | Higher friction for large doc trees |
| ~~LLM-optimised content export~~ | ~~`starlight-llms-txt` generates `/llms.txt`, `/llms-full.txt`, `/llms-small.txt` for AI agents~~ | ~~`LlmsTxtGenerator` + `ILlmsTxtConfiguration` — generates `/llms.txt` index and `/llms-full.txt` with inlined content~~ | ~~Resolved~~ |
| Trailing slash normalisation | Astro config + ASP.NET middleware + rehype plugin coordinate consistent trailing slash behaviour | No normalisation | Inconsistent URLs harm SEO and caching |
| Global banner system | `Banner.astro` reads from JSON data, conditional display for announcements | No banner component | No built-in way to show site-wide announcements |
| Custom 404 page | `404.md` with styled error page served by ASP.NET middleware | No custom 404 | Users see a generic error |

### Architectural / extensibility gaps

| Feature | Starlight | Lagoon Status | Impact |
|---|---|---|---|
| Component overrides | Replace any built-in UI component by path | No override mechanism | Layout/UI not customisable without forking Lagoon source |
| Plugin system | Plugin API for extending Starlight at build and runtime | No plugin architecture | Extensions must be built as standalone Atoll components |
| Route data API | Typed `StarlightPage` route data accessible in components | No equivalent API | Components cannot access structured page metadata |

### Nice-to-have

| Feature | Starlight / Duende Implementation | Lagoon Status | Impact |
|---|---|---|---|
| Analytics integration | Google Tag Manager snippet injected via component override | No analytics hooks | Requires manual head injection per site |
| Heading badges | `starlight-heading-badges` — inline badges on headings (e.g. "OSS", "Business") | No heading badge support | Product-tier documentation cannot badge individual headings |
| Newsletter / form embed | Custom `Newsletter.astro` with HubSpot JS SDK | No form component | Site-specific; can use generic island |
| Testimonial components | `testimonial.astro` + `testimonial-grid.astro` — blockquote + avatar layout | No testimonial component | Site-specific; easy custom component |
| Robots.txt | Static file in `public/` directory | No robots.txt generation | Trivial to add manually |
| Container / Docker deployment | OCI publish via `dotnet publish /t:PublishContainer`, Alpine base | No containerisation support | Deployment is manual |
| .NET Aspire orchestration | `Docs.AppHost` orchestrates Astro dev + .NET server + dashboard | No Aspire integration | Dev experience enhancement only |

## Summary

**Parity**: 31 features fully covered (including Giscus, external links, markdown extensions,
component directives, typed schemas, islands architecture, and topic-based search metadata).

**Lagoon advantages**: 6 features where Lagoon is ahead (multi-version docs, Draw.IO, annotations,
blog/articles theme, typed frontmatter, ASP.NET middleware).

**Notable gaps**: 15 total (2 resolved).
- **5 must-have** — redirects, link validation, OG images, tab sync, expressive code blocks.
- **4 should-have** — auto-sidebar, trailing slash normalisation, banner, custom 404. *(LLM export and search metadata resolved)*
- **3 architectural** — component overrides, plugin system, route data API.
- **3 nice-to-have** — analytics hooks, heading badges, containerisation.

Lagoon covers the core requirements for a .NET documentation site, including multi-language
support, rich content components, and multi-version documentation. The must-have gaps primarily
affect production readiness (redirects, link validation, OG images) and content authoring
convenience (tab sync, expressive code). The architectural gaps affect long-term extensibility.
