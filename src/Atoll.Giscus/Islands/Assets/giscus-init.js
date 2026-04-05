/**
 * Atoll Giscus — Island Init
 *
 * Reads giscus configuration from data-* attributes on the placeholder div,
 * creates the giscus <script> tag, and watches for Atoll theme changes to
 * keep the giscus widget in sync via the postMessage API.
 */

export default function init(element) {
    const placeholder = element.querySelector('.giscus');
    if (!placeholder) return;

    // Read all configuration from data-* attributes on the placeholder div.
    const repo = placeholder.dataset.repo || '';
    const repoId = placeholder.dataset.repoId || '';
    const category = placeholder.dataset.category || '';
    const categoryId = placeholder.dataset.categoryId || '';
    const mapping = placeholder.dataset.mapping || 'pathname';
    const term = placeholder.dataset.term || '';
    const strict = placeholder.dataset.strict || '0';
    const reactionsEnabled = placeholder.dataset.reactionsEnabled || '1';
    const emitMetadata = placeholder.dataset.emitMetadata || '0';
    const inputPosition = placeholder.dataset.inputPosition || 'bottom';
    const theme = placeholder.dataset.theme || 'preferred_color_scheme';
    const lang = placeholder.dataset.lang || 'en';
    const loading = placeholder.dataset.loading || 'lazy';

    // Create and configure the giscus script element.
    const script = document.createElement('script');
    script.src = 'https://giscus.app/client.js';
    script.dataset.repo = repo;
    script.dataset.repoId = repoId;
    if (category) script.dataset.category = category;
    script.dataset.categoryId = categoryId;
    script.dataset.mapping = mapping;
    if (term) script.dataset.term = term;
    script.dataset.strict = strict;
    script.dataset.reactionsEnabled = reactionsEnabled;
    script.dataset.emitMetadata = emitMetadata;
    script.dataset.inputPosition = inputPosition;
    script.dataset.theme = theme;
    script.dataset.lang = lang;
    script.dataset.loading = loading;
    script.crossOrigin = 'anonymous';
    script.async = true;

    // Insert the script before the placeholder div.
    // Giscus will find the div with class "giscus" and replace its content.
    placeholder.parentNode.insertBefore(script, placeholder);

    // Watch for Atoll theme changes and sync giscus via postMessage.
    const observer = new MutationObserver(() => {
        const newTheme = document.documentElement.getAttribute('data-theme');
        if (!newTheme) return;

        const iframe = element.querySelector('iframe.giscus-frame');
        if (iframe && iframe.contentWindow) {
            iframe.contentWindow.postMessage(
                { giscus: { setConfig: { theme: newTheme } } },
                'https://giscus.app'
            );
        }
    });

    observer.observe(document.documentElement, {
        attributes: true,
        attributeFilter: ['data-theme'],
    });
}
