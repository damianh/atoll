---
title: Islands, Mermaid & Charts
description: Built-in interactive islands, Mermaid diagram support, and Chart.js integration.
order: 26
section: Lagoon Plugin
---

# Islands, Mermaid & Charts

Lagoon ships three built-in islands that handle interactive UI, plus optional Mermaid diagram and Chart.js chart rendering. All islands use Atoll's zero-JS-by-default model — JavaScript only loads when the chosen client directive condition is met.

## Built-in islands

### `ThemeToggle`

Renders a button that switches between light and dark themes.

| Detail | Value |
|---|---|
| Directive | `client:load` |
| Script | `atoll-theme-toggle.js` |

`client:load` is chosen so the correct theme icon is shown as early as possible — before user interaction.

**Behaviour:**
- On hydration, reads `localStorage` for a persisted preference
- Falls back to the system `prefers-color-scheme` media query
- Sets `data-theme="dark"` or `data-theme="light"` on `<html>`
- On click, toggles the attribute and saves the new choice to `localStorage`

`ThemeToggle` is rendered automatically by `DocsLayout` in the header. No manual wiring is needed.

### `SearchDialog`

Renders a search trigger button and a `<dialog>` overlay for full-text search.

| Detail | Value |
|---|---|
| Directive | `client:idle` |
| Script | `atoll-docs-search-dialog.js` |

`client:idle` is chosen because search is not on the critical path — it does not need to be ready before the user starts reading.

**Behaviour:**
- Opens on button click, `Ctrl+K` (Windows / Linux), or `⌘K` (macOS)
- Fetches `search-index.json` lazily on first open
- Filters results client-side as the user types
- Arrow keys navigate results; `Enter` follows the selected link
- `Escape` or clicking outside the dialog closes it

**Parameters:**

| Parameter | Default | Description |
|---|---|---|
| `Placeholder` | `"Search docs..."` | Input placeholder and trigger button text |
| `IndexUrl` | `"/search-index.json"` | URL of the pre-built search index |

`SearchDialog` is rendered automatically by `DocsLayout`. Provide a custom `IndexUrl` when your site is hosted at a sub-path. See [Site Search](./search) for details.

### `MobileNav`

Renders a hamburger menu button for narrow viewports.

| Detail | Value |
|---|---|
| Directive | `client:media("(max-width: 768px)")` |
| Script | `atoll-docs-mobile-nav.js` |

`client:media` is chosen so the JavaScript is never downloaded on desktop — the cost is zero for viewport widths above 768 px.

**Behaviour:**
- Shows/hides the sidebar overlay (`id="mobile-nav-menu"`)
- Traps keyboard focus within the open menu
- Closes on `Escape` or clicking the overlay backdrop

`MobileNav` is rendered automatically by `DocsLayout`. No manual wiring is needed.

## Mermaid diagrams

Lagoon integrates the `Atoll.Mermaid` plugin to render [Mermaid](https://mermaid.js.org/) diagrams from fenced code blocks. Set `EnableMermaid = true` in `DocsConfig` and write `` ```mermaid `` blocks in your markdown — diagrams render as SVGs at page load with automatic dark/light theme support.

See the [Mermaid Plugin](../mermaid/overview) documentation for installation, usage, supported diagram types, and technical details.

## Charts

Lagoon integrates the `Atoll.Charts` plugin to render interactive [Chart.js](https://www.chartjs.org/) charts from fenced code blocks. Register the `ChartExtension` in your markdown pipeline and write `` ```chart `` blocks containing Chart.js JSON configuration. Charts are lazy-loaded when they scroll into view.

Chart elements (bars, points, pie segments) can be made clickable by adding an `_atoll.links` key to the chart JSON config — no custom JavaScript required per-site.

See the [Charts Plugin](../charts/overview) documentation for installation, usage, supported chart types, clickable elements, and technical details.
