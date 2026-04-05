/**
 * Atoll Docs — Mermaid Initializer
 *
 * Loads the Mermaid JS library from CDN and initialises it.
 * This module is only included when EnableMermaid is true in DocsConfig.
 * It respects the active data-theme attribute for dark/light mode.
 *
 * Import this from the DocsLayout as a module script:
 *   <script type="module" src="/scripts/atoll-docs-mermaid-init.js"></script>
 */

import mermaid from 'https://cdn.jsdelivr.net/npm/mermaid/dist/mermaid.esm.min.mjs';

function getTheme() {
    return document.documentElement.getAttribute('data-theme') === 'dark' ? 'dark' : 'default';
}

// Store the original diagram source before Mermaid replaces it with SVG.
const diagrams = [];
document.querySelectorAll('pre.mermaid').forEach(el => {
    diagrams.push({ el, source: el.textContent });
});

mermaid.initialize({ startOnLoad: true, theme: getTheme() });

// Re-render when the theme toggles: restore original source, reset processed
// state, re-initialize with the new theme, and re-run.
const observer = new MutationObserver(async () => {
    const newTheme = getTheme();
    diagrams.forEach(({ el, source }) => {
        el.removeAttribute('data-processed');
        el.textContent = source;
    });
    mermaid.initialize({ startOnLoad: false, theme: newTheme });
    await mermaid.run();
});

observer.observe(document.documentElement, {
    attributes: true,
    attributeFilter: ['data-theme']
});
