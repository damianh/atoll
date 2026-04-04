/**
 * Atoll Docs — Search Dialog
 *
 * Opens a <dialog> on click or Ctrl+K / ⌘K.
 * Fetches the search index lazily on first open and performs client-side
 * full-text search with prefix + word-boundary matching and keyword highlighting.
 *
 * The search index URL is read from the wrapper element's `data-index-url` attribute,
 * defaulting to `/search-index.json` if not set. Set `data-index-url` to support
 * sites hosted at a base path (e.g. `/docs/search-index.json`).
 */

let index = null;

async function loadIndex(element) {
    if (index) return index;
    const indexUrl = element.dataset.indexUrl || '/search-index.json';
    const resp = await fetch(indexUrl);
    index = await resp.json();
    return index;
}

function escapeHtml(text) {
    return text
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}

/** Highlights query matches in already-HTML-encoded text by inserting <mark> tags. */
function highlight(encodedText, query) {
    const escapedQuery = query.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    const re = new RegExp(`(${escapeHtml(escapedQuery)})`, 'gi');
    return encodedText.replace(re, '<mark>$1</mark>');
}

function search(entries, query) {
    const q = query.trim().toLowerCase();
    if (!q) return [];
    return entries
        .filter(e =>
            e.title.toLowerCase().includes(q) ||
            (e.description && e.description.toLowerCase().includes(q)) ||
            (e.body && e.body.toLowerCase().includes(q))
        )
        .slice(0, 10);
}

function renderResults(container, results, query, noResultsText) {
    container.innerHTML = '';
    if (!results.length) {
        container.innerHTML = '<p class="search-no-results">' + escapeHtml(noResultsText) + '</p>';
        return;
    }
    const list = document.createElement('ul');
    list.className = 'search-results-list';
    list.setAttribute('role', 'listbox');
    results.forEach((r, i) => {
        const li = document.createElement('li');
        li.setAttribute('role', 'option');
        li.setAttribute('tabindex', '-1');
        li.dataset.href = r.href;
        const encodedHref = escapeHtml(r.href);
        const encodedTitle = highlight(escapeHtml(r.title), query);
        const encodedDesc = r.description ? highlight(escapeHtml(r.description), query) : '';
        li.innerHTML = `
            <a href="${encodedHref}" class="search-result-link">
                <span class="search-result-title">${encodedTitle}</span>
                ${encodedDesc ? `<span class="search-result-desc">${encodedDesc}</span>` : ''}
            </a>`;
        list.appendChild(li);
    });
    container.appendChild(list);
}

export default function init(element) {
    const wrapper = element.querySelector('.search-wrapper') || element;
    const noResultsText = wrapper.dataset.noResults || 'No results found.';
    const trigger = element.querySelector('#search-trigger');
    const dialog = element.querySelector('#search-dialog');
    const input = element.querySelector('#search-input');
    const results = element.querySelector('#search-results');
    const closeBtn = element.querySelector('#search-close');

    if (!trigger || !dialog) return;

    function openDialog() {
        dialog.showModal();
        input && input.focus();
    }

    function closeDialog() {
        dialog.close();
    }

    trigger.addEventListener('click', openDialog);
    closeBtn && closeBtn.addEventListener('click', closeDialog);

    dialog.addEventListener('click', function (e) {
        if (e.target === dialog) closeDialog();
    });

    document.addEventListener('keydown', function (e) {
        if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
            e.preventDefault();
            openDialog();
        }
    });

    dialog.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') closeDialog();

        if (!results) return;
        const items = results.querySelectorAll('[role="option"]');
        const current = document.activeElement;
        const idx = Array.from(items).indexOf(current);

        if (e.key === 'ArrowDown') {
            e.preventDefault();
            const next = items[idx + 1] || items[0];
            next && next.focus();
        } else if (e.key === 'ArrowUp') {
            e.preventDefault();
            const prev = items[idx - 1] || items[items.length - 1];
            prev && prev.focus();
        } else if (e.key === 'Enter' && idx >= 0) {
            const href = current.dataset.href;
            if (href) window.location.href = href;
        }
    });

    input && input.addEventListener('input', async function () {
        const q = input.value;
        if (!q.trim()) {
            results && (results.innerHTML = '');
            return;
        }
        const data = await loadIndex(element);
        const hits = search(data.entries || data, q);
        results && renderResults(results, hits, q, noResultsText);
    });
}
