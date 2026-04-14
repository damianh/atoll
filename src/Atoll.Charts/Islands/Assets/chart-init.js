/**
 * Chart.js island bootstrapper — Atoll island module
 *
 * Exports a default init(element) function per the VanillaJsIsland contract.
 *
 * On first call, dynamically injects atoll-charts-vendor.min.js (Chart.js UMD build)
 * into the page. Subsequent calls on the same page reuse the same loading promise so
 * the vendor script is only fetched once, even when multiple charts are present.
 *
 * After Chart.js loads, a new Chart instance is created for each
 * canvas[data-chart-config] element inside the island container.
 *
 * Chart.js built-in responsive mode has a known bug (#12177) where charts shrink
 * but don't grow back. We work around this by disabling Chart.js responsive mode
 * and managing resize ourselves: a ResizeObserver on the .atoll-chart container
 * calls chart.resize(width, height) with explicit dimensions derived from the
 * container. This avoids feedback loops because we never modify the container.
 */

// Derive the vendor script URL from this module's own URL so it resolves correctly
// when the site is deployed under a sub-path (e.g. /atoll/).
const VENDOR_SCRIPT_URL = new URL('./atoll-charts-vendor.min.js', import.meta.url).href;
const VENDOR_SCRIPT_ID  = 'atoll-charts-vendor-script';

// --- Atoll link helpers ---

const ALLOWED_URL_PREFIXES = ['/', './', '../', 'http://', 'https://'];

/**
 * Returns true if the URL is allowed for navigation (blocks javascript:, data:, etc.).
 *
 * @param {string} url
 * @returns {boolean}
 */
function isAllowedUrl(url) {
  if (url.startsWith('//')) return false; // block protocol-relative URLs (e.g. //evil.com)
  return ALLOWED_URL_PREFIXES.some(prefix => url.startsWith(prefix));
}

/**
 * Navigates to the given URL. External URLs open in a new tab; relative URLs navigate in place.
 *
 * @param {string} url
 */
function navigateToUrl(url) {
  if (url.startsWith('http://') || url.startsWith('https://')) {
    window.open(url, '_blank', 'noopener');
  } else {
    window.location.href = url;
  }
}

/** @type {Promise<void> | null} */
let vendorLoadPromise = null;

/**
 * Ensures atoll-charts-vendor.min.js is loaded exactly once.
 *
 * @returns {Promise<void>}
 */
function loadChartJs() {
  if (vendorLoadPromise !== null) {
    return vendorLoadPromise;
  }

  // If the script tag already exists (e.g. added by another mechanism), resolve immediately.
  if (document.getElementById(VENDOR_SCRIPT_ID)) {
    vendorLoadPromise = Promise.resolve();
    return vendorLoadPromise;
  }

  vendorLoadPromise = new Promise((resolve, reject) => {
    const script = document.createElement('script');
    script.id  = VENDOR_SCRIPT_ID;
    script.src = VENDOR_SCRIPT_URL;
    script.onload  = () => resolve();
    script.onerror = () => reject(new Error(`Failed to load Chart.js from ${VENDOR_SCRIPT_URL}`));
    document.head.appendChild(script);
  });

  return vendorLoadPromise;
}

/**
 * Island init function — called by the Atoll island runtime when the element
 * enters the viewport (ClientVisible directive).
 *
 * @param {HTMLElement} element - The <atoll-island> container element.
 */
export default function init(element) {
  return loadChartJs().then(() => {
    if (typeof Chart === 'undefined') {
      return;
    }

    // Find all canvas elements with chart config inside this island and render each one.
    const canvases = element.querySelectorAll('canvas[data-chart-config]');
    for (const canvas of canvases) {
      try {
        const config = JSON.parse(canvas.getAttribute('data-chart-config'));
        if (!config.options) {
          config.options = {};
        }

        // If the user explicitly set responsive: false, honour it and skip
        // our resize management entirely.
        const userDisabledResponsive = config.options.responsive === false;

        // Disable Chart.js built-in responsive mode — we manage sizing ourselves
        // to work around the grow-back bug (#12177).
        if (!userDisabledResponsive) {
          config.options.responsive = false;
        }

        // Read the user's desired aspect ratio (default 2 for cartesian, 1 for radial).
        const aspectRatio = config.options.aspectRatio
          || (['pie', 'doughnut', 'radar', 'polarArea'].includes(config.type) ? 1 : 2);

        const container = canvas.closest('.atoll-chart');

        // Set initial size from container before creating the chart.
        if (!userDisabledResponsive && container) {
          const w = container.clientWidth;
          const h = config.options.maintainAspectRatio === false
            ? container.clientHeight
            : Math.round(w / aspectRatio);
          canvas.width = w;
          canvas.height = h;
          canvas.style.width = w + 'px';
          canvas.style.height = h + 'px';
        }

        // --- Atoll links (clickable chart elements) ---
        const links = config._atoll?.links;
        if (links && Array.isArray(links)) {
          // Wire onClick for navigation
          if (!config.options.onClick) {
            config.options.onClick = (_event, activeElements) => {
              if (!activeElements || activeElements.length === 0) return;
              const el = activeElements[0];
              const url = links[el.datasetIndex]?.[el.index];
              if (typeof url === 'string' && isAllowedUrl(url)) {
                navigateToUrl(url);
              }
            };
          }

          // Wire onHover for cursor change
          const existingOnHover = config.options.onHover;
          config.options.onHover = (event, activeElements) => {
            if (existingOnHover) existingOnHover(event, activeElements);
            let hasLink = false;
            if (activeElements && activeElements.length > 0) {
              const el = activeElements[0];
              const url = links[el.datasetIndex]?.[el.index];
              hasLink = typeof url === 'string' && url.length > 0;
            }
            canvas.style.cursor = hasLink ? 'pointer' : 'default';
          };
        }

        const chart = new Chart(canvas, config);

        // Observe the container and resize the chart when it changes.
        // We only touch the canvas — never the container — so no feedback loop.
        if (!userDisabledResponsive && container) {
          const observer = new ResizeObserver((entries) => {
            const entry = entries[0];
            if (!entry) return;
            const w = Math.round(entry.contentRect.width);
            if (w === 0) return; // hidden element
            const h = config.options.maintainAspectRatio === false
              ? Math.round(entry.contentRect.height)
              : Math.round(w / aspectRatio);
            chart.resize(w, h);
          });
          observer.observe(container);
        }
      } catch (err) {
        console.error('[atoll-charts] Failed to render chart:', err);
      }
    }
  }).catch((err) => {
    console.error('[atoll-charts] Failed to load Chart.js:', err);
  });
}
