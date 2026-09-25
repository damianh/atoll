---
title: Content Security Policy
description: The Content-Security-Policy an Atoll site needs, and how Atoll avoids inline scripts so script-src 'self' works.
order: 11
section: Features
---

# Content Security Policy

Atoll doesn't write inline `<script>` blocks or inline event handlers (`onclick="…"`, `onchange="…"`) into the pages it generates. All client-side behaviour is in external files under `/scripts/` and `/_atoll/`, served from your own origin. That means an Atoll site can enforce a strict [Content-Security-Policy](https://developer.mozilla.org/docs/Web/HTTP/CSP) without `'unsafe-inline'` or `'unsafe-hashes'` in `script-src`.

Atoll doesn't use nonces. A nonce must be different on every response, so it only works when a server rewrites each HTML response. Atoll builds static sites, and external files work on any host.

## Recommended policy

```text
Content-Security-Policy:
  default-src 'self';
  script-src 'self';
  style-src 'self' 'unsafe-inline';
  img-src 'self' data:;
  font-src 'self';
  connect-src 'self';
  frame-ancestors 'self';
  base-uri 'self';
  object-src 'none'
```

| Directive | Why |
|---|---|
| `script-src 'self'` | Theme scripts, islands and integrations all load from your origin. This is enough for Lagoon, Reef and Swell with their default features. |
| `style-src 'self' 'unsafe-inline'` | Needed when Mermaid is enabled, because Mermaid, D3 and KaTeX write inline styles. Swell decks also embed their theme in a `<style>` element and set `style` attributes. Inline styles are much less risky than inline scripts. |
| `img-src 'self' data:` | Diagrams (Mermaid, draw.io) and some icons use `data:` URIs. |
| `connect-src 'self'` | The Lagoon search dialog fetches its `search-index.json` from your origin. |

## Integrations that add origins

| Integration | What to add |
|---|---|
| Giscus comments (`Atoll.Giscus`) | `script-src https://giscus.app` and `frame-src https://giscus.app` |
| Mermaid with a custom `MermaidModuleUrl` | The origin of that URL in `script-src` (for example `https://cdn.jsdelivr.net`). The default bundled Mermaid build needs nothing extra. |
| draw.io (`Atoll.DrawIo`) | Usually nothing. If a diagram contains math, the viewer loads MathJax from `https://viewer.diagrams.net`. Add that origin to `script-src`, or accept that math labels won't typeset. |
| Charts (`Atoll.Charts`), Annotations | Nothing. Their scripts are bundled and served from `/scripts/`. |

The bundled draw.io viewer contains `eval` code paths, but they sit behind flags Atoll doesn't enable, so you shouldn't need `'unsafe-eval'`.

## How the built-in scripts load

Scripts that have to run before the page paints are synchronous, external `<script src>` tags:

| Script | Where | Purpose |
|---|---|---|
| `atoll-theme-init.js` (Lagoon) / `atoll-reef-theme-init.js` (Reef) | `<head>`, no `defer`/`async` | Sets `data-theme` before first paint so the page never flashes the wrong theme |
| `atoll-sidebar-restore.js`, `atoll-sidebar-scroll-restore.js` | inside the sidebar | Restores the sidebar's open groups and scroll position before it paints |
| `atoll-docs-banner.js` | right after the banner | Hides a dismissed banner before it paints |

Everything else uses `defer` or `type="module"`. Lagoon's inline handlers are replaced by `data-*` attributes and one delegated listener in `atoll-docs-actions.js`:

- `data-atoll-copy` on code block copy buttons
- `data-atoll-navigate` on the language and version pickers

`DocsBaseHead` includes `atoll-docs-actions.js`. If a custom layout renders Lagoon markdown without `DocsBaseHead`, add `<script src="/scripts/atoll-docs-actions.js" defer></script>` so that copy buttons work.

Data the browser only reads, such as Swell's presenter notes, goes in `<script type="application/json">` blocks. The browser doesn't execute JSON data blocks, so CSP doesn't apply to them.

## What can reintroduce inline scripts

- **`ScriptInstruction.Inline(...)`** writes an inline `<script>`. Use `ScriptInstruction.External(...)` or `ScriptInstruction.Module(...)` with a static asset instead.
- **Per-page `head:` frontmatter** and custom head content are written as-is. Reference external scripts there, not inline code.
- **Raw HTML in Markdown** (including Swell slides and notes) is your own content. Atoll writes it unchanged.
- **The dev server's live-reload script** is inline, but it's only injected by `atoll dev`, never into built output.

## Try it out

Start with `Content-Security-Policy-Report-Only` and watch the browser console, or a `report-to` endpoint, for violations. When it's clean, switch to the enforcing header. On static hosts that support a `_headers` file (see [HTTP Caching](/caching)), you can set the header there:

```text
/*
  Content-Security-Policy: default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data:
```
