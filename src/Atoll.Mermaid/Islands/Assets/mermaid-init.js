/**
 * Atoll Docs — Mermaid Initializer
 *
 * Renders <pre class="mermaid"> diagrams with the Mermaid build bundled in
 * Atoll.Mermaid (served from /scripts/atoll-mermaid/<version>/). Mermaid is only
 * downloaded when the page actually contains a diagram, and nothing is fetched
 * from third-party origins unless the site overrides the module URL.
 *
 * Include as a module script:
 *   <script src="/scripts/atoll-docs-mermaid-init.js" type="module" data-atoll-mermaid></script>
 *
 * To load Mermaid from somewhere else (e.g. a self-hosted newer version), add
 * data-module-src="<url of mermaid.esm.min.mjs>" to that script tag.
 *
 * The active data-theme attribute on <html> selects the dark or default theme,
 * and diagrams are re-rendered when it changes.
 */

// Updated by eng/update-mermaid.ps1 — must match vendor/mermaid/manifest.json.
const BUNDLED_VERSION = '12.0.0';

const diagrams = Array.from(document.querySelectorAll('pre.mermaid'), el => ({
    el,
    // Keep the original source; Mermaid replaces the element content with SVG.
    source: el.textContent
}));

if (diagrams.length > 0) {
    start();
}

function getTheme() {
    return document.documentElement.getAttribute('data-theme') === 'dark' ? 'dark' : 'default';
}

function resolveModuleUrl() {
    const override = document.querySelector('script[data-atoll-mermaid]')?.getAttribute('data-module-src');
    if (override) {
        return new URL(override, document.baseURI).href;
    }
    return new URL(`./atoll-mermaid/${BUNDLED_VERSION}/mermaid.esm.min.mjs`, import.meta.url).href;
}

async function start() {
    const moduleUrl = resolveModuleUrl();
    let mermaid;
    try {
        mermaid = (await import(moduleUrl)).default;
    } catch (err) {
        console.error(`[atoll-mermaid] Failed to load Mermaid from ${moduleUrl}`, err);
        return;
    }

    let renderedTheme = null;
    let queue = Promise.resolve();

    async function render() {
        const theme = getTheme();
        if (theme === renderedTheme) {
            return;
        }
        renderedTheme = theme;

        diagrams.forEach(({ el, source }) => {
            el.removeAttribute('data-processed');
            el.textContent = source;
        });

        mermaid.initialize({ startOnLoad: false, securityLevel: 'strict', theme });
        try {
            await mermaid.run({ nodes: diagrams.map(d => d.el) });
        } catch (err) {
            console.error('[atoll-mermaid] Failed to render one or more diagrams', err);
        }
    }

    // Serialize renders so a theme toggle during a render never overlaps it.
    function scheduleRender() {
        queue = queue.then(render);
    }

    new MutationObserver(scheduleRender).observe(document.documentElement, {
        attributes: true,
        attributeFilter: ['data-theme']
    });

    scheduleRender();
}
