/**
 * Atoll Docs — Search Dialog
 *
 * Opens a <dialog> on click or Ctrl+K / ⌘K.
 * Fetches the search index lazily on first open and performs client-side
 * full-text search with prefix + word-boundary matching and keyword highlighting.
 *
 * Results are grouped by page in a tree view: a parent node showing the page
 * title (with a document icon) and child nodes for each matching heading
 * within that page (connected by tree-diagram lines).
 *
 * The search index URL is read from the wrapper element's `data-index-url` attribute,
 * defaulting to `/search-index.json` if not set. Set `data-index-url` to support
 * sites hosted at a base path (e.g. `/docs/search-index.json`).
 *
 * Topic filtering: entries may carry a `topics` array (e.g. `["IdentityServer","Security"]`).
 * A chip bar is rendered above the results when the index contains any topics.
 * Selecting one or more chips narrows results to entries that share at least one
 * selected topic. Old indices that only have a `section` field are supported via
 * automatic fallback.
 */

let index = null;

async function loadIndex(element) {
    if (index) return index;
    const indexUrl = element.dataset.indexUrl || '/search-index.json';
    const basePath = element.dataset.basePath || '';
    const resp = await fetch(indexUrl);
    const data = await resp.json();
    // Prefix entry hrefs with the base path so links resolve correctly
    // when the site is hosted under a sub-path (e.g. /atoll/).
    if (basePath) {
        const entries = data.entries || data;
        for (const entry of entries) {
            if (entry.href && entry.href.startsWith('/') && !entry.href.startsWith(basePath)) {
                entry.href = basePath + entry.href;
            }
        }
    }
    index = data;
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

/** Generates a URL-safe slug from heading text (matches Markdig AutoIdentifiers output). */
function slugify(text) {
    return text
        .toLowerCase()
        .replace(/[^\w\s-]/g, '')
        .replace(/\s+/g, '-')
        .replace(/-+/g, '-')
        .replace(/^-|-$/g, '');
}

/**
 * Returns the effective topic list for an entry.
 * Prefers `entry.topics` (new multi-topic field); falls back to `[entry.section]`
 * for backward compatibility with older indices that only carry `section`.
 */
function getEntryTopics(entry) {
    if (entry.topics && entry.topics.length > 0) return entry.topics;
    if (entry.section) return [entry.section];
    return [];
}

/**
 * Extracts all unique topic values from the index entries, sorted alphabetically.
 */
function buildTopicList(entries) {
    const seen = new Set();
    for (const entry of entries) {
        for (const topic of getEntryTopics(entry)) {
            seen.add(topic);
        }
    }
    return Array.from(seen).sort();
}

/**
 * Searches the index and returns results grouped by page.
 * Each result includes the page entry and which headings matched the query.
 * When `selectedTopics` is a non-empty Set, only entries whose effective topics
 * include at least one selected topic are returned.
 */
function search(entries, query, selectedTopics) {
    const q = query.trim().toLowerCase();
    if (!q) return [];
    return entries
        .filter(e => {
            const textMatch =
                e.title.toLowerCase().includes(q) ||
                (e.description && e.description.toLowerCase().includes(q)) ||
                (e.body && e.body.toLowerCase().includes(q)) ||
                (e.headings && e.headings.some(h => h.toLowerCase().includes(q)));
            if (!textMatch) return false;
            if (selectedTopics && selectedTopics.size > 0) {
                const entryTopics = getEntryTopics(e);
                return entryTopics.some(t => selectedTopics.has(t));
            }
            return true;
        })
        .slice(0, 8)
        .map(e => {
            const matchedHeadings = (e.headings || []).filter(h =>
                h.toLowerCase().includes(q)
            );
            // If the query matches the body/description but no headings,
            // still show the page (just without sub-results).
            return { ...e, matchedHeadings };
        });
}

/** Finds a body snippet containing the query, returning a ~120 char window with the match highlighted. */
function getSnippet(body, query) {
    if (!body) return '';
    const q = query.trim().toLowerCase();
    const idx = body.toLowerCase().indexOf(q);
    if (idx < 0) return '';
    const start = Math.max(0, idx - 40);
    const end = Math.min(body.length, idx + q.length + 80);
    let snippet = body.slice(start, end);
    if (start > 0) snippet = '…' + snippet;
    if (end < body.length) snippet += '…';
    return highlight(escapeHtml(snippet), query);
}

/**
 * Renders the topic filter chip bar into `container`.
 * `topics` is a sorted array of unique topic strings.
 * `selectedTopics` is the current Set of active topic filters.
 * `onFilterChange` is called with the updated Set after a chip is toggled.
 * `filterLabel` is the accessible aria-label for the chip bar (defaults to "Filter by topic").
 */
function renderTopicFilter(container, topics, selectedTopics, onFilterChange, filterLabel) {
    container.innerHTML = '';
    if (!topics.length) return;

    const label = filterLabel || 'Filter by topic';
    container.setAttribute('aria-label', label);

    // "All" chip — clears selection
    const allChip = document.createElement('button');
    allChip.type = 'button';
    allChip.className = 'search-topic-chip' + (selectedTopics.size === 0 ? ' search-topic-chip-active' : '');
    allChip.textContent = 'All';
    allChip.addEventListener('click', () => {
        onFilterChange(new Set());
    });
    container.appendChild(allChip);

    for (const topic of topics) {
        const chip = document.createElement('button');
        chip.type = 'button';
        chip.className = 'search-topic-chip' + (selectedTopics.has(topic) ? ' search-topic-chip-active' : '');
        chip.textContent = topic;
        chip.addEventListener('click', () => {
            const next = new Set(selectedTopics);
            if (next.has(topic)) {
                next.delete(topic);
            } else {
                next.add(topic);
            }
            onFilterChange(next);
        });
        container.appendChild(chip);
    }
}

function renderResults(container, results, query, noResultsText) {
    container.innerHTML = '';
    if (!results.length) {
        container.innerHTML = '<p class="search-no-results">' + escapeHtml(noResultsText) + '</p>';
        return;
    }

    const countEl = document.createElement('p');
    countEl.className = 'search-result-count';
    countEl.textContent = results.length + ' result' + (results.length !== 1 ? 's' : '') + ' for ' + query;
    container.appendChild(countEl);

    results.forEach((r) => {
        const group = document.createElement('div');
        group.className = 'search-result-group';

        // -- Page-level parent result --
        const pageEl = document.createElement('div');
        pageEl.className = 'search-result-page';
        pageEl.setAttribute('role', 'option');
        pageEl.setAttribute('tabindex', '-1');
        pageEl.dataset.href = r.href;
        const encodedHref = escapeHtml(r.href);
        const encodedTitle = highlight(escapeHtml(r.title), query);
        const snippet = getSnippet(r.body || r.description || '', query);
        pageEl.innerHTML = `<a href="${encodedHref}" class="search-result-link">
                <span class="search-result-title">${encodedTitle}</span>
                ${snippet ? `<span class="search-result-desc">${snippet}</span>` : ''}
            </a>`;
        group.appendChild(pageEl);

        // -- Matched heading sub-results (tree children) --
        r.matchedHeadings.forEach((heading, i) => {
            const headingEl = document.createElement('div');
            headingEl.className = 'search-result-nested';
            if (i === r.matchedHeadings.length - 1) {
                headingEl.classList.add('search-result-nested-last');
            }
            headingEl.setAttribute('role', 'option');
            headingEl.setAttribute('tabindex', '-1');
            const slug = slugify(heading);
            const headingHref = escapeHtml(r.href + '#' + slug);
            headingEl.dataset.href = r.href + '#' + slug;
            const encodedHeading = highlight(escapeHtml(heading), query);
            headingEl.innerHTML = `<a href="${headingHref}" class="search-result-link">
                    <span class="search-result-title">${encodedHeading}</span>
                </a>`;
            group.appendChild(headingEl);
        });

        // -- Topic badges: render one per topic (topics array preferred, section as fallback) --
        const entryTopics = getEntryTopics(r);
        if (entryTopics.length > 0) {
            const badgeRow = document.createElement('div');
            badgeRow.className = 'search-result-badge-row';
            for (const topic of entryTopics) {
                const badge = document.createElement('span');
                badge.className = 'search-result-badge';
                badge.textContent = topic;
                badgeRow.appendChild(badge);
            }
            group.appendChild(badgeRow);
        }

        container.appendChild(group);
    });
}

export default function init(element) {
    const wrapper = element.querySelector('.search-wrapper') || element;
    const noResultsText = wrapper.dataset.noResults || 'No results found.';
    const topicFilterLabel = wrapper.dataset.topicFilterLabel || 'Filter by topic';
    const trigger = element.querySelector('#search-trigger');
    const dialog = element.querySelector('#search-dialog');
    const input = element.querySelector('#search-input');
    const results = element.querySelector('#search-results');
    const topicsContainer = element.querySelector('#search-topics');
    const closeBtn = element.querySelector('#search-close');

    if (!trigger || !dialog) return;

    let selectedTopics = new Set();
    let allTopics = [];

    function runSearch() {
        if (!input || !results) return;
        const q = input.value;
        if (!q.trim()) {
            results.innerHTML = '';
            return;
        }
        const data = index;
        if (!data) return;
        const hits = search(data.entries || data, q, selectedTopics);
        renderResults(results, hits, q, noResultsText);
    }

    function onTopicFilterChange(newSelection) {
        selectedTopics = newSelection;
        if (topicsContainer) {
            renderTopicFilter(topicsContainer, allTopics, selectedTopics, onTopicFilterChange, topicFilterLabel);
        }
        runSearch();
    }

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
        const data = await loadIndex(wrapper);
        // Build and render the topic filter on first load (or whenever index changes)
        if (topicsContainer) {
            const newTopics = buildTopicList(data.entries || data);
            if (newTopics.join(',') !== allTopics.join(',')) {
                allTopics = newTopics;
                // Reset selection when index changes
                selectedTopics = new Set();
                renderTopicFilter(topicsContainer, allTopics, selectedTopics, onTopicFilterChange, topicFilterLabel);
            }
        }
        const hits = search(data.entries || data, q, selectedTopics);
        results && renderResults(results, hits, q, noResultsText);
    });
}
