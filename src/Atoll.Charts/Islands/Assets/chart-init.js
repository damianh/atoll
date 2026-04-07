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
 */

// Derive the vendor script URL from this module's own URL so it resolves correctly
// when the site is deployed under a sub-path (e.g. /atoll/).
const VENDOR_SCRIPT_URL = new URL('./atoll-charts-vendor.min.js', import.meta.url).href;
const VENDOR_SCRIPT_ID  = 'atoll-charts-vendor-script';

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

        // Ensure responsive resizing works in both directions (shrink and grow).
        // Chart.js uses a ResizeObserver on the canvas parent, which must have
        // position:relative (provided by the .atoll-chart CSS).
        if (!config.options) {
          config.options = {};
        }
        config.options.responsive = true;
        config.options.maintainAspectRatio = true;

        new Chart(canvas, config);
      } catch (err) {
        console.error('[atoll-charts] Failed to render chart:', err);
      }
    }
  }).catch((err) => {
    console.error('[atoll-charts] Failed to load Chart.js:', err);
  });
}
