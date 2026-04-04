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

const theme = document.documentElement.getAttribute('data-theme') === 'dark' ? 'dark' : 'default';

mermaid.initialize({ startOnLoad: true, theme });

// Re-initialize when the theme toggles so diagrams re-render with updated colours.
const observer = new MutationObserver(() => {
    const newTheme = document.documentElement.getAttribute('data-theme') === 'dark' ? 'dark' : 'default';
    mermaid.initialize({ startOnLoad: false, theme: newTheme });
    mermaid.run();
});

observer.observe(document.documentElement, {
    attributes: true,
    attributeFilter: ['data-theme']
});
