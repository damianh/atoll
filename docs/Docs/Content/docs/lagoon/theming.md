---
title: Theme & Styling
description: Design tokens, dark mode, custom CSS, and the DocsTheme component.
order: 23
section: Lagoon Theme
---

# Theme & Styling

Lagoon ships a complete CSS theme in `DocsTheme` — a single component that injects all design tokens, layout rules, typography, and component styles into the page. It is applied globally (no scope wrapper) via `[GlobalStyle]` so styles affect the entire document.

## CSS sections

`DocsTheme` is composed of named sections applied in this order:

| Section | Description |
|---|---|
| `Reset` | Box-sizing reset, body font/line-height, global `a` link styles |
| `LightTokens` | CSS custom properties for light mode (`:root`) |
| `DarkTokens` | Overrides for dark mode (`[data-theme="dark"]`) |
| `Layout` | Header, body grid, sidebar column, TOC column, main content area |
| `Typography` | Heading sizes, paragraph spacing |
| `Prose` | Markdown content styles (lists, blockquotes, tables, `<hr>`) |
| `CodeBlocks` | Fenced code block styling (dark background, scrollable) |
| `SidebarNav` | Sidebar link, group header, badge, and active-item styles |
| `TocNav` | "On this page" heading list styles |
| `PaginationStyles` | Previous / Next footer nav styles |
| `BreadcrumbStyles` | Breadcrumb trail and separator styles |
| `HeroStyles` | Landing-page hero section with actions and image |
| `SearchStyles` | Search trigger button and `<dialog>` overlay styles |

## Design tokens

All visual properties are CSS custom properties so you can override any of them.

### Light mode tokens (`:root`)

| Token | Default | Description |
|---|---|---|
| `--docs-bg` | `#ffffff` | Page background |
| `--docs-bg-raised` | `#f9fafb` | Slightly elevated surface (sidebar, cards) |
| `--docs-bg-subtle` | `#f3f4f6` | Even subtler surface (inline code) |
| `--docs-text` | `#111827` | Primary text colour |
| `--docs-text-muted` | `#6b7280` | Secondary / muted text |
| `--docs-text-faint` | `#9ca3af` | Tertiary / placeholder text |
| `--docs-primary` | `#0f3460` | Primary accent (headings, active links) |
| `--docs-primary-hover` | `#1e5fa8` | Primary hover state |
| `--docs-accent` | `#e94560` | Secondary accent (hover links) |
| `--docs-link` | `#0f3460` | Link colour |
| `--docs-link-hover` | `#e94560` | Link hover colour |
| `--docs-border` | `#e5e7eb` | Border colour |
| `--docs-sidebar-bg` | `#f9fafb` | Sidebar background |
| `--docs-sidebar-link-active-bg` | `#eff6ff` | Active sidebar link background |
| `--docs-sidebar-link-active-text` | `#0f3460` | Active sidebar link text |
| `--docs-code-bg` | `#1e293b` | Fenced code block background |
| `--docs-code-text` | `#e2e8f0` | Fenced code block text |
| `--docs-code-inline-bg` | `#f3f4f6` | Inline code background |
| `--docs-code-inline-text` | `#be123c` | Inline code text |
| `--docs-sidebar-width` | `16rem` | Sidebar column width |
| `--docs-toc-width` | `14rem` | TOC column width |
| `--docs-header-height` | `3.5rem` | Sticky header height |

### Dark mode tokens (`[data-theme="dark"]`)

| Token | Dark value |
|---|---|
| `--docs-bg` | `#0f172a` |
| `--docs-bg-raised` | `#1e293b` |
| `--docs-bg-subtle` | `#1e293b` |
| `--docs-text` | `#f1f5f9` |
| `--docs-text-muted` | `#94a3b8` |
| `--docs-text-faint` | `#64748b` |
| `--docs-primary` | `#7dd3fc` |
| `--docs-primary-hover` | `#bae6fd` |
| `--docs-accent` | `#f472b6` |
| `--docs-link` | `#7dd3fc` |
| `--docs-link-hover` | `#f472b6` |
| `--docs-border` | `#334155` |
| `--docs-sidebar-bg` | `#1e293b` |
| `--docs-sidebar-link-active-bg` | `#0f172a` |
| `--docs-sidebar-link-active-text` | `#7dd3fc` |
| `--docs-code-bg` | `#020617` |
| `--docs-code-text` | `#e2e8f0` |
| `--docs-code-inline-bg` | `#1e293b` |
| `--docs-code-inline-text` | `#f9a8d4` |

## Dark mode

Dark mode is controlled by the `data-theme="dark"` attribute on the `<html>` element. The `ThemeToggle` island sets this attribute and persists the choice in `localStorage`. On page load it applies the stored preference (or falls back to the system `prefers-color-scheme` setting).

You can also set the attribute server-side, or control it from your own script by toggling `document.documentElement.dataset.theme` between `"dark"` and `"light"`.

## Overriding tokens

Override any token in a custom CSS file referenced via `DocsConfig.CustomCss`:

```css
/* public/styles/custom.css */
:root {
    --docs-primary: #7c3aed;
    --docs-accent: #db2777;
    --docs-link: #7c3aed;
    --docs-link-hover: #db2777;
}

[data-theme="dark"] {
    --docs-primary: #a78bfa;
    --docs-accent: #f9a8d4;
}
```

Then register it in `DocsConfig`:

```csharp
CustomCss = ["/styles/custom.css"],
```

## Adding custom CSS

`CustomCss` accepts any number of paths or absolute URLs:

```csharp
CustomCss =
[
    "/styles/custom.css",
    "/styles/syntax-overrides.css",
    "https://fonts.googleapis.com/css2?family=Inter:wght@400;600&display=swap",
],
```

Each path is emitted as a `<link rel="stylesheet">` in the page `<head>` after the built-in theme, so your rules take precedence.
