/**
 * draw.io viewer island bootstrapper — Atoll island module
 *
 * Exports a default init(element) function per the VanillaJsIsland contract.
 *
 * On first call, dynamically injects viewer-static.min.js into the page.
 * Subsequent calls on the same page reuse the same loading promise so the
 * viewer script is only fetched once, even when multiple diagrams are present.
 *
 * After the viewer loads, GraphViewer.createViewerForElement() is called for
 * each data-mxgraph element inside the island container.
 *
 * NOTE: GraphViewer.processElements() expects a CSS *class name*, not a DOM
 * element. We call createViewerForElement() directly to target the exact
 * element(s) within this island.
 */

const VIEWER_SCRIPT_URL = '/scripts/atoll-drawio-viewer.min.js';
const VIEWER_SCRIPT_ID  = 'atoll-drawio-viewer-script';

/** @type {Promise<void> | null} */
let viewerLoadPromise = null;

/**
 * Ensures viewer-static.min.js is loaded exactly once.
 * Suppresses the viewer's built-in auto-init (which scans document.body for
 * class="mxgraph") by setting window.onDrawioViewerLoad to a no-op before
 * the script is injected. This prevents a redundant body scan and avoids
 * double-processing if elements happen to have the mxgraph class.
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

  // Suppress the viewer's auto-init. The viewer script's tail calls:
  //   if (window.onDrawioViewerLoad) window.onDrawioViewerLoad();
  //   else GraphViewer.processElements();
  // By setting a no-op callback, we skip the default processElements() call
  // and instead render each diagram ourselves via createViewerForElement().
  window.onDrawioViewerLoad = function () { /* handled per-island */ };

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
    if (typeof GraphViewer === 'undefined' || typeof GraphViewer.createViewerForElement !== 'function') {
      return;
    }

    // Find all data-mxgraph elements inside this island and render each one.
    const targets = element.querySelectorAll('[data-mxgraph]');
    for (const target of targets) {
      GraphViewer.createViewerForElement(target);
    }
  }).catch((err) => {
    console.error('[atoll-drawio] Failed to load diagram viewer:', err);
  });
}
