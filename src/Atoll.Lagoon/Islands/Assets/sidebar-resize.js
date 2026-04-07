(function () {
  try {
    if (window.matchMedia('(max-width:768px)').matches) return;

    var KEY = 'atoll:sidebar-width';
    var MIN_WIDTH = 180;
    var MAX_WIDTH = 600;
    var handle = document.getElementById('sidebar-resize');
    var wrapper = document.getElementById('sidebar-wrapper');
    var body = document.querySelector('.docs-body');

    if (!handle || !wrapper || !body) return;

    // Restore saved width from localStorage
    var saved = localStorage.getItem(KEY);
    if (saved) {
      var w = parseInt(saved, 10);
      if (w >= MIN_WIDTH && w <= MAX_WIDTH) {
        body.style.setProperty('--docs-sidebar-width', w + 'px');
      }
    }

    var dragging = false;
    var startX = 0;
    var startWidth = 0;

    function onPointerDown(e) {
      if (window.matchMedia('(max-width:768px)').matches) return;
      e.preventDefault();
      dragging = true;
      startX = e.clientX;
      startWidth = wrapper.getBoundingClientRect().width;
      handle.classList.add('active');
      document.body.style.cursor = 'col-resize';
      document.body.style.userSelect = 'none';
      document.addEventListener('pointermove', onPointerMove);
      document.addEventListener('pointerup', onPointerUp);
    }

    function onPointerMove(e) {
      if (!dragging) return;
      var isRtl = document.documentElement.dir === 'rtl';
      var delta = isRtl ? (startX - e.clientX) : (e.clientX - startX);
      var newWidth = Math.min(MAX_WIDTH, Math.max(MIN_WIDTH, startWidth + delta));
      body.style.setProperty('--docs-sidebar-width', newWidth + 'px');
    }

    function onPointerUp() {
      if (!dragging) return;
      dragging = false;
      handle.classList.remove('active');
      document.body.style.cursor = '';
      document.body.style.userSelect = '';
      document.removeEventListener('pointermove', onPointerMove);
      document.removeEventListener('pointerup', onPointerUp);

      // Persist the final width
      var width = wrapper.getBoundingClientRect().width;
      try {
        localStorage.setItem(KEY, Math.round(width).toString());
      } catch (e) {}
    }

    handle.addEventListener('pointerdown', onPointerDown);

    // Keyboard support: arrow keys to resize
    handle.addEventListener('keydown', function (e) {
      if (window.matchMedia('(max-width:768px)').matches) return;
      var step = e.shiftKey ? 50 : 10;
      var currentWidth = wrapper.getBoundingClientRect().width;
      var newWidth;

      if (e.key === 'ArrowRight' || e.key === 'ArrowLeft') {
        var isRtl = document.documentElement.dir === 'rtl';
        var grow = isRtl ? (e.key === 'ArrowLeft') : (e.key === 'ArrowRight');
        newWidth = grow
          ? Math.min(MAX_WIDTH, currentWidth + step)
          : Math.max(MIN_WIDTH, currentWidth - step);
        body.style.setProperty('--docs-sidebar-width', newWidth + 'px');
        try {
          localStorage.setItem(KEY, Math.round(newWidth).toString());
        } catch (ex) {}
        e.preventDefault();
      }
    });

    // Double-click to reset to default width
    handle.addEventListener('dblclick', function () {
      body.style.removeProperty('--docs-sidebar-width');
      try {
        localStorage.removeItem(KEY);
      } catch (e) {}
    });
  } catch (e) {}
})();
