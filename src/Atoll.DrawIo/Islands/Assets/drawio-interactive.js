/**
 * draw.io interactive diagram — Atoll island module
 * Exports a default init(element, props) function.
 *
 * Features:
 *   - Pan: click-and-drag to pan the SVG viewBox
 *   - Zoom: mouse-wheel to zoom in/out
 *   - Layer toggle: clicking layer buttons shows/hides SVG <g> layers
 *   - Fit/Reset: double-click to reset to the original viewBox
 *   - Basic touch support: single-finger pan, two-finger pinch-zoom
 */
export default function init(element) {
  const svg = element.querySelector('svg');
  if (!svg) return;

  // ── ViewBox helpers ──────────────────────────────────────────────────────

  function getViewBox() {
    const vb = svg.getAttribute('viewBox') || '0 0 800 600';
    return vb.split(/\s+|,/).map(Number);
  }

  function setViewBox(x, y, w, h) {
    svg.setAttribute('viewBox', `${x} ${y} ${w} ${h}`);
  }

  const originalViewBox = getViewBox();

  // ── Pan ──────────────────────────────────────────────────────────────────

  let isPanning = false;
  let panStart = null;
  let viewBoxAtPanStart = null;

  svg.addEventListener('mousedown', (e) => {
    if (e.button !== 0) return;
    isPanning = true;
    panStart = { x: e.clientX, y: e.clientY };
    viewBoxAtPanStart = getViewBox();
    svg.style.cursor = 'grabbing';
    e.preventDefault();
  });

  window.addEventListener('mousemove', (e) => {
    if (!isPanning) return;
    const [vx, vy, vw, vh] = viewBoxAtPanStart;
    const rect = svg.getBoundingClientRect();
    const scaleX = vw / rect.width;
    const scaleY = vh / rect.height;
    const dx = (e.clientX - panStart.x) * scaleX;
    const dy = (e.clientY - panStart.y) * scaleY;
    setViewBox(vx - dx, vy - dy, vw, vh);
  });

  window.addEventListener('mouseup', () => {
    if (isPanning) {
      isPanning = false;
      svg.style.cursor = 'grab';
    }
  });

  svg.style.cursor = 'grab';

  // ── Zoom ─────────────────────────────────────────────────────────────────

  svg.addEventListener('wheel', (e) => {
    e.preventDefault();
    const [vx, vy, vw, vh] = getViewBox();
    const factor = e.deltaY > 0 ? 1.1 : 0.9;

    // Zoom toward the mouse cursor position within the SVG
    const rect = svg.getBoundingClientRect();
    const mx = ((e.clientX - rect.left) / rect.width) * vw + vx;
    const my = ((e.clientY - rect.top) / rect.height) * vh + vy;

    const newW = vw * factor;
    const newH = vh * factor;
    const newX = mx - (mx - vx) * factor;
    const newY = my - (my - vy) * factor;
    setViewBox(newX, newY, newW, newH);
  }, { passive: false });

  // ── Reset (double-click) ─────────────────────────────────────────────────

  svg.addEventListener('dblclick', () => {
    setViewBox(...originalViewBox);
  });

  // ── Touch: pan ───────────────────────────────────────────────────────────

  let lastTouches = null;
  let touchViewBox = null;

  svg.addEventListener('touchstart', (e) => {
    lastTouches = Array.from(e.touches);
    touchViewBox = getViewBox();
    e.preventDefault();
  }, { passive: false });

  svg.addEventListener('touchmove', (e) => {
    const touches = Array.from(e.touches);
    if (!touchViewBox) return;
    const [vx, vy, vw, vh] = touchViewBox;
    const rect = svg.getBoundingClientRect();

    if (touches.length === 1 && lastTouches.length === 1) {
      // Single-finger pan
      const scaleX = vw / rect.width;
      const scaleY = vh / rect.height;
      const dx = (touches[0].clientX - lastTouches[0].clientX) * scaleX;
      const dy = (touches[0].clientY - lastTouches[0].clientY) * scaleY;
      setViewBox(vx - dx, vy - dy, vw, vh);
    } else if (touches.length === 2 && lastTouches.length >= 2) {
      // Two-finger pinch-zoom
      const d0 = Math.hypot(
        lastTouches[0].clientX - lastTouches[1].clientX,
        lastTouches[0].clientY - lastTouches[1].clientY
      );
      const d1 = Math.hypot(
        touches[0].clientX - touches[1].clientX,
        touches[0].clientY - touches[1].clientY
      );
      if (d0 === 0) return;
      const factor = d0 / d1;
      const cx = (touches[0].clientX + touches[1].clientX) / 2;
      const cy = (touches[0].clientY + touches[1].clientY) / 2;
      const mx = ((cx - rect.left) / rect.width) * vw + vx;
      const my = ((cy - rect.top) / rect.height) * vh + vy;
      setViewBox(mx - (mx - vx) * factor, my - (my - vy) * factor, vw * factor, vh * factor);
    }

    lastTouches = touches;
    touchViewBox = getViewBox();
    e.preventDefault();
  }, { passive: false });

  svg.addEventListener('touchend', () => {
    lastTouches = null;
    touchViewBox = null;
  });

  // ── Layer toggle buttons ─────────────────────────────────────────────────

  const buttons = element.querySelectorAll('.drawio-layer-btn');
  const activeClass = 'drawio-layer-btn--active';

  buttons.forEach((btn) => {
    // Initialise button state
    const layerId = btn.dataset.layerId;
    const layerEl = layerId ? svg.querySelector(`#${CSS.escape(layerId)}`) : null;
    if (layerEl) {
      const visible = layerEl.getAttribute('display') !== 'none';
      if (visible) btn.classList.add(activeClass);

      btn.addEventListener('click', () => {
        const isVisible = layerEl.getAttribute('display') !== 'none';
        layerEl.setAttribute('display', isVisible ? 'none' : 'inline');
        btn.classList.toggle(activeClass, !isVisible);
      });
    }
  });
}
