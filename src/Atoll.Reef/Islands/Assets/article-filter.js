/**
 * Atoll.Reef — ArticleFilter island
 *
 * Reads click events on [data-filter-tag] buttons and change events on
 * [data-filter-author] selects, then shows/hides article cards that carry
 * matching data-tags / data-author attributes.
 */
(function () {
  'use strict';

  function getRoot(el) {
    return el.closest('[data-filter-root]');
  }

  function applyFilter(root) {
    var activeTag = root.querySelector('[data-filter-tag][aria-pressed="true"]');
    var authorSelect = root.querySelector('[data-filter-author]');
    var tag = activeTag ? activeTag.getAttribute('data-filter-tag') : '';
    var author = authorSelect ? authorSelect.value : '';

    // Walk sibling containers to find article cards
    var container = root.parentElement;
    if (!container) return;
    var cards = container.querySelectorAll('[data-tags], [data-author]');
    cards.forEach(function (card) {
      var cardTags = (card.getAttribute('data-tags') || '').split(',').map(function (t) { return t.trim(); });
      var cardAuthor = card.getAttribute('data-author') || '';
      var tagMatch = tag === '' || cardTags.indexOf(tag) !== -1;
      var authorMatch = author === '' || cardAuthor === author;
      card.style.display = tagMatch && authorMatch ? '' : 'none';
    });
  }

  function init(root) {
    root.addEventListener('click', function (e) {
      var btn = e.target.closest('[data-filter-tag]');
      if (!btn) return;
      root.querySelectorAll('[data-filter-tag]').forEach(function (b) {
        b.setAttribute('aria-pressed', 'false');
        b.classList.remove('tag-pill--active');
      });
      btn.setAttribute('aria-pressed', 'true');
      btn.classList.add('tag-pill--active');
      applyFilter(root);
    });

    var sel = root.querySelector('[data-filter-author]');
    if (sel) {
      sel.addEventListener('change', function () { applyFilter(root); });
    }
  }

  document.querySelectorAll('[data-filter-root]').forEach(init);
})();
