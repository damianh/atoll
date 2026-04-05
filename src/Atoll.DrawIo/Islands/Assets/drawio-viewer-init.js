/**
 * draw.io viewer island bootstrapper — Atoll island module
 *
 * Exports a default init(element) function per the VanillaJsIsland contract.
 *
 * On first call, dynamically injects viewer-static.min.js into the page.
 * Subsequent calls on the same page reuse the same loading promise so the
 * viewer script is only fetched once, even when multiple diagrams are present.
 *
 * After the viewer loads, GraphViewer.processElements() is called to render
 * the data-mxgraph element(s) inside the island container.
 */

const VIEWER_SCRIPT_URL = '/scripts/atoll-drawio-viewer.min.js';
const VIEWER_SCRIPT_ID  = 'atoll-drawio-viewer-script';

/** @type {Promise<void> | null} */
let viewerLoadPromise = null;

/**
 * Ensures viewer-static.min.js is loaded exactly once.
 * Returns a Promise that resolves when the script is ready.
 *
 * @returns {Promise<void>}
 */
function loadViewer() {
  if (viewerLoadPromise !== null) {
    return viewerLoadPromise;
  }

  // If the script tag already exists (e.g. added by another mechanism), resolve immediately.
  if (document.getElementById(VIEWER_SCRIPT_ID)) {
    viewerLoadPromise = Promise.resolve();
    return viewerLoadPromise;
  }

  viewerLoadPromise = new Promise((resolve, reject) => {
    const script = document.createElement('script');
    script.id  = VIEWER_SCRIPT_ID;
    script.src = VIEWER_SCRIPT_URL;
    script.onload  = () => resolve();
    script.onerror = () => reject(new Error(`Failed to load draw.io viewer from ${VIEWER_SCRIPT_URL}`));
    document.head.appendChild(script);
  });

  return viewerLoadPromise;
}

/**
 * Island init function — called by the Atoll island runtime when the element
 * enters the viewport (ClientVisible directive).
 *
 * @param {HTMLElement} element - The <atoll-island> container element.
 */
export default function init(element) {
  loadViewer().then(() => {
    // GraphViewer.processElements() scans for data-mxgraph elements and renders them.
    // Pass the container so only diagrams inside this island are processed.
    if (typeof GraphViewer !== 'undefined' && typeof GraphViewer.processElements === 'function') {
      GraphViewer.processElements(element);
    }
  }).catch((err) => {
    console.error('[atoll-drawio] Failed to load diagram viewer:', err);
  });
}
