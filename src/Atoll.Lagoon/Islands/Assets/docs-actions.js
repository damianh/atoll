// Delegated handlers that replace inline on* attributes so pages work under a
// strict Content-Security-Policy (script-src 'self').
(function () {
  if (window.__atollDocsActions) return;
  window.__atollDocsActions = true;

  document.addEventListener('click', function (e) {
    var btn = e.target instanceof Element ? e.target.closest('[data-atoll-copy]') : null;
    if (!btn) return;
    var w = btn.closest('.ec-frame') || btn.closest('.code-block-wrapper');
    var c = w && w.querySelector('code');
    if (!c || !navigator.clipboard) return;
    navigator.clipboard.writeText(c.innerText).then(function () {
      btn.classList.add('copied');
      setTimeout(function () { btn.classList.remove('copied'); }, 2000);
    });
  });

  document.addEventListener('change', function (e) {
    var sel = e.target instanceof Element ? e.target.closest('select[data-atoll-navigate]') : null;
    if (sel && sel.value) window.location.href = sel.value;
  });
})();
