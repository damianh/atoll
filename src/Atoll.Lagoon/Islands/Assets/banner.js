// Loaded synchronously right after the banner so a dismissed banner is hidden before it paints.
(function () {
  try {
    var b = document.getElementById('docs-banner');
    if (!b) return;
    var k = b.getAttribute('data-dismiss-key');
    if (k && localStorage.getItem(k) === '1') { b.hidden = true; return; }
    var btn = b.querySelector('.docs-banner-dismiss');
    if (btn) {
      btn.addEventListener('click', function () {
        b.hidden = true;
        try { if (k) localStorage.setItem(k, '1'); } catch (e) {}
      });
    }
  } catch (e) {}
})();
