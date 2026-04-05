/**
 * Atoll.Reef — ViewToggle island
 *
 * Reads click events on [data-view-btn] buttons, persists the selection in
 * localStorage, and sets a data-view attribute on the nearest ancestor
 * [data-view-container] so CSS can show the correct view variant.
 */
(function () {
  'use strict';

  var STORAGE_KEY = 'atoll-reef-view';

  function setView(container, view) {
    container.setAttribute('data-view', view);
    localStorage.setItem(STORAGE_KEY, view);
  }

  function init(toggle) {
    var container = toggle.closest('[data-view-container]') || toggle.parentElement;
    if (!container) return;

    // Restore persisted view
    var saved = localStorage.getItem(STORAGE_KEY);
    if (saved) {
      container.setAttribute('data-view', saved);
      toggle.querySelectorAll('[data-view-btn]').forEach(function (btn) {
        var active = btn.getAttribute('data-view-btn') === saved;
        btn.setAttribute('aria-pressed', active ? 'true' : 'false');
        btn.classList.toggle('view-toggle__btn--active', active);
      });
    }

    toggle.addEventListener('click', function (e) {
      var btn = e.target.closest('[data-view-btn]');
      if (!btn) return;
      var view = btn.getAttribute('data-view-btn');
      setView(container, view);
      toggle.querySelectorAll('[data-view-btn]').forEach(function (b) {
        var active = b === btn;
        b.setAttribute('aria-pressed', active ? 'true' : 'false');
        b.classList.toggle('view-toggle__btn--active', active);
      });
    });
  }

  document.querySelectorAll('[data-view-toggle]').forEach(init);
})();
