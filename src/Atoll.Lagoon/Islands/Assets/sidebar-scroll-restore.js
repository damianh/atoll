// Loaded synchronously right after the sidebar list so the saved scroll position
// is restored before the sidebar paints.
(function () {
  try {
    if (window.matchMedia('(max-width:768px)').matches) return;
    var state = window.__atollSidebarState;
    if (!state || typeof state.scroll !== 'number') return;
    var aside = document.querySelector('.docs-sidebar');
    if (aside) aside.scrollTop = state.scroll;
  } catch (e) {}
})();
