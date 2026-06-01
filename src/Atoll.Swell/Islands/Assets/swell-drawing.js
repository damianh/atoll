(function () {
  'use strict';

  var drawingActive = false;
  var canvas = null;
  var ctx = null;
  var drawing = false;
  var lastX = 0;
  var lastY = 0;

  var strokeColor = '#e94560';
  var strokeWidth = 3;

  function createCanvas() {
    canvas = document.createElement('canvas');
    canvas.id = 'swell-drawing-canvas';
    canvas.style.cssText = [
      'position:fixed',
      'top:0',
      'left:0',
      'width:100%',
      'height:100%',
      'z-index:9999',
      'cursor:crosshair',
      'pointer-events:none',
      'display:none',
    ].join(';');
    document.body.appendChild(canvas);
    ctx = canvas.getContext('2d');
    resizeCanvas();
    window.addEventListener('resize', resizeCanvas);
  }

  function resizeCanvas() {
    if (!canvas) return;
    canvas.width  = window.innerWidth;
    canvas.height = window.innerHeight;
  }

  function clearCanvas() {
    if (ctx) ctx.clearRect(0, 0, canvas.width, canvas.height);
  }

  function toggleDrawing() {
    drawingActive = !drawingActive;
    if (canvas) {
      canvas.style.pointerEvents = drawingActive ? 'auto' : 'none';
      canvas.style.display = drawingActive ? 'block' : 'none';
    }
  }

  // ── Pointer events ────────────────────────────────────────────────────────
  function onPointerDown(e) {
    if (!drawingActive) return;
    drawing = true;
    lastX = e.clientX;
    lastY = e.clientY;
    canvas.setPointerCapture(e.pointerId);
    ctx.beginPath();
    ctx.arc(lastX, lastY, strokeWidth / 2, 0, Math.PI * 2);
    ctx.fillStyle = strokeColor;
    ctx.fill();
    e.preventDefault();
  }

  function onPointerMove(e) {
    if (!drawing || !drawingActive) return;
    ctx.beginPath();
    ctx.moveTo(lastX, lastY);
    ctx.lineTo(e.clientX, e.clientY);
    ctx.strokeStyle = strokeColor;
    ctx.lineWidth = strokeWidth;
    ctx.lineCap = 'round';
    ctx.lineJoin = 'round';
    ctx.stroke();
    lastX = e.clientX;
    lastY = e.clientY;
    e.preventDefault();
  }

  function onPointerUp() {
    drawing = false;
  }

  // ── Keyboard ──────────────────────────────────────────────────────────────
  document.addEventListener('keydown', function (e) {
    if (e.key === 'd' || e.key === 'D') {
      if (!canvas) createCanvas();
      toggleDrawing();
    }
    // Escape: exit drawing mode (do NOT clear — caller's Escape handler handles slide exit)
    if (e.key === 'Escape' && drawingActive) {
      toggleDrawing();
    }
  });

  // ── Slide-change hook: clear canvas ───────────────────────────────────────
  // Listen for BroadcastChannel messages OR a custom DOM event that swell-nav.js can fire.
  if (typeof BroadcastChannel !== 'undefined') {
    var ch = new BroadcastChannel('swell-presenter');
    ch.onmessage = function (evt) {
      if (evt.data && typeof evt.data.slide === 'number') {
        clearCanvas();
      }
    };
  }

  // Also listen to custom event fired by swell-nav.js
  document.addEventListener('swell-slide-change', function () {
    clearCanvas();
  });

  // ── Init ──────────────────────────────────────────────────────────────────
  createCanvas();

  // Attach pointer events lazily (after canvas exists)
  document.addEventListener('pointerdown', onPointerDown);
  document.addEventListener('pointermove', onPointerMove);
  document.addEventListener('pointerup', onPointerUp);

})();
