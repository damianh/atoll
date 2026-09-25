// Loaded synchronously from inside the sidebar <nav>, before the groups are parsed,
// so saved open/closed state is applied before the sidebar paints.
(function () {
  try {
    if (window.matchMedia('(max-width:768px)').matches) return;
    var key = 'atoll:sidebar-state';
    var raw = sessionStorage.getItem(key);
    if (!raw) return;
    var state = JSON.parse(raw);
    var nav = document.querySelector('.docs-sidebar nav[data-hash]');
    if (!nav || nav.getAttribute('data-hash') !== state.hash) { sessionStorage.removeItem(key); return; }
    window.__atollSidebarState = state;
  } catch (e) {}
})();
if (!customElements.get('sl-sidebar-restore')) {
  customElements.define('sl-sidebar-restore', class extends HTMLElement {
    connectedCallback() {
      try {
        var state = window.__atollSidebarState;
        if (!state || !state.open) return;
        var i = parseInt(this.dataset.index);
        if (isNaN(i) || i >= state.open.length) return;
        var val = state.open[i];
        if (val === null || val === undefined) return;
        var details = this.closest('details');
        if (!details) return;
        if (details.hasAttribute('data-active')) return;
        if (val) details.setAttribute('open', '');
        else details.removeAttribute('open');
      } catch (e) {}
    }
  });
}
