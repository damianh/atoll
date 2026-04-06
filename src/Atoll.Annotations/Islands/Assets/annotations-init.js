/**
 * Atoll Annotations — Island Init
 *
 * Enables contextual text-selection feedback on any Atoll-powered page.
 * When a user selects text within the configured content area, a floating
 * button appears. Clicking it opens a small comment form. On submit, a new
 * browser tab opens with a pre-populated GitHub Issue or Discussion URL
 * containing the quoted text, user comment, and a text-fragment link.
 */

// ── Constants ────────────────────────────────────────────────────────────────

const MAX_QUOTE_BODY_CHARS = 2000;
const MAX_FRAGMENT_CHARS = 200;
const MAX_TOTAL_URL_CHARS = 8000;
const QUOTE_DISPLAY_CHARS = 200;

// ── Styles ───────────────────────────────────────────────────────────────────

function injectStyles() {
    if (document.getElementById('atoll-annotations-styles')) return;

    const style = document.createElement('style');
    style.id = 'atoll-annotations-styles';
    style.textContent = `
        .atoll-ann-btn {
            position: absolute;
            z-index: 9999;
            display: flex;
            align-items: center;
            gap: 4px;
            padding: 4px 10px;
            border: 1px solid var(--atoll-ann-border, #d0d7de);
            border-radius: 6px;
            background: var(--atoll-ann-bg, #ffffff);
            color: var(--atoll-ann-text, #24292f);
            font-size: 13px;
            font-family: inherit;
            cursor: pointer;
            box-shadow: 0 2px 8px rgba(0,0,0,0.12);
            white-space: nowrap;
            transition: box-shadow 0.15s ease;
            user-select: none;
        }
        .atoll-ann-btn:hover {
            box-shadow: 0 4px 12px rgba(0,0,0,0.18);
        }
        .atoll-ann-popover {
            position: absolute;
            z-index: 9999;
            width: 340px;
            max-width: calc(100vw - 32px);
            padding: 12px;
            border: 1px solid var(--atoll-ann-border, #d0d7de);
            border-radius: 8px;
            background: var(--atoll-ann-bg, #ffffff);
            color: var(--atoll-ann-text, #24292f);
            font-size: 14px;
            font-family: inherit;
            box-shadow: 0 4px 16px rgba(0,0,0,0.16);
        }
        .atoll-ann-popover blockquote {
            margin: 0 0 10px 0;
            padding: 6px 10px;
            border-left: 3px solid var(--atoll-ann-border, #d0d7de);
            font-size: 12px;
            color: var(--atoll-ann-muted, #57606a);
            overflow: hidden;
            display: -webkit-box;
            -webkit-line-clamp: 3;
            -webkit-box-orient: vertical;
        }
        .atoll-ann-popover textarea {
            display: block;
            width: 100%;
            min-height: 72px;
            padding: 6px 8px;
            border: 1px solid var(--atoll-ann-border, #d0d7de);
            border-radius: 6px;
            background: var(--atoll-ann-input-bg, #f6f8fa);
            color: var(--atoll-ann-text, #24292f);
            font-size: 13px;
            font-family: inherit;
            resize: vertical;
            box-sizing: border-box;
            outline: none;
        }
        .atoll-ann-popover textarea:focus {
            border-color: var(--atoll-ann-accent, #0969da);
            box-shadow: 0 0 0 3px rgba(9,105,218,0.12);
        }
        .atoll-ann-actions {
            display: flex;
            justify-content: flex-end;
            gap: 8px;
            margin-top: 10px;
        }
        .atoll-ann-submit {
            padding: 5px 14px;
            border: none;
            border-radius: 6px;
            background: var(--atoll-ann-accent, #0969da);
            color: #ffffff;
            font-size: 13px;
            font-family: inherit;
            font-weight: 500;
            cursor: pointer;
        }
        .atoll-ann-submit:hover {
            opacity: 0.88;
        }
        .atoll-ann-cancel {
            padding: 5px 10px;
            border: 1px solid var(--atoll-ann-border, #d0d7de);
            border-radius: 6px;
            background: transparent;
            color: var(--atoll-ann-muted, #57606a);
            font-size: 13px;
            font-family: inherit;
            cursor: pointer;
        }
        .atoll-ann-cancel:hover {
            background: var(--atoll-ann-input-bg, #f6f8fa);
        }
        .atoll-ann-close {
            position: absolute;
            top: 8px;
            right: 8px;
            padding: 2px 6px;
            border: none;
            background: transparent;
            color: var(--atoll-ann-muted, #57606a);
            font-size: 16px;
            cursor: pointer;
            line-height: 1;
        }
        .atoll-ann-close:hover {
            color: var(--atoll-ann-text, #24292f);
        }
    `;
    document.head.appendChild(style);
}

// ── Theme ────────────────────────────────────────────────────────────────────

function applyTheme(isDark) {
    const root = document.documentElement;
    if (isDark) {
        root.style.setProperty('--atoll-ann-bg', '#161b22');
        root.style.setProperty('--atoll-ann-text', '#e6edf3');
        root.style.setProperty('--atoll-ann-border', '#30363d');
        root.style.setProperty('--atoll-ann-muted', '#8b949e');
        root.style.setProperty('--atoll-ann-input-bg', '#0d1117');
        root.style.setProperty('--atoll-ann-accent', '#388bfd');
    } else {
        root.style.setProperty('--atoll-ann-bg', '#ffffff');
        root.style.setProperty('--atoll-ann-text', '#24292f');
        root.style.setProperty('--atoll-ann-border', '#d0d7de');
        root.style.setProperty('--atoll-ann-muted', '#57606a');
        root.style.setProperty('--atoll-ann-input-bg', '#f6f8fa');
        root.style.setProperty('--atoll-ann-accent', '#0969da');
    }
}

function setupTheme() {
    const isDark = () =>
        document.documentElement.getAttribute('data-theme') === 'dark';

    applyTheme(isDark());

    new MutationObserver(() => applyTheme(isDark())).observe(
        document.documentElement,
        { attributes: true, attributeFilter: ['data-theme'] }
    );
}

// ── URL Construction ─────────────────────────────────────────────────────────

function truncate(text, maxLen) {
    if (text.length <= maxLen) return text;
    return text.slice(0, maxLen) + '\u2026';
}

function buildGitHubUrl(config, selectedText, userComment) {
    const pageTitle = document.title;
    const pageUrl = location.href.split('#')[0];
    const fragment = encodeURIComponent(truncate(selectedText.trim(), MAX_FRAGMENT_CHARS));
    const sourceLink = `[${pageTitle}](${pageUrl}#:~:text=${fragment})`;

    const quotedText = truncate(selectedText.trim(), MAX_QUOTE_BODY_CHARS);
    const commentPart = userComment.trim();

    let body = `> ${quotedText.replace(/\n/g, '\n> ')}\n\n`;
    if (commentPart) {
        body += `${commentPart}\n\n`;
    }
    body += `---\n\uD83D\uDCCD ${sourceLink}`;

    const title = `${config.titlePrefix} ${pageTitle}`.trim();

    let url;
    if (config.target === 'discussion') {
        const params = new URLSearchParams({ title, body });
        if (config.category) {
            params.set('category', config.category);
        }
        url = `https://github.com/${config.repo}/discussions/new?${params.toString()}`;
    } else {
        const params = new URLSearchParams({ title, body });
        if (config.labels) {
            params.set('labels', config.labels);
        }
        url = `https://github.com/${config.repo}/issues/new?${params.toString()}`;
    }

    // Safety: truncate body further if URL is too long
    if (url.length > MAX_TOTAL_URL_CHARS) {
        const excess = url.length - MAX_TOTAL_URL_CHARS;
        const shorterBody = body.slice(0, body.length - excess - 1) + '\u2026';
        if (config.target === 'discussion') {
            const params = new URLSearchParams({ title, body: shorterBody });
            if (config.category) params.set('category', config.category);
            url = `https://github.com/${config.repo}/discussions/new?${params.toString()}`;
        } else {
            const params = new URLSearchParams({ title, body: shorterBody });
            if (config.labels) params.set('labels', config.labels);
            url = `https://github.com/${config.repo}/issues/new?${params.toString()}`;
        }
    }

    return url;
}

// ── Popover / Button UI ──────────────────────────────────────────────────────

function positionElement(el, rect) {
    const scrollX = window.scrollX;
    const scrollY = window.scrollY;
    const margin = 8;

    let top = rect.top + scrollY - el.offsetHeight - margin;
    let left = rect.left + scrollX + (rect.width / 2) - (el.offsetWidth / 2);

    // Flip below if not enough space above
    if (top < scrollY + margin) {
        top = rect.bottom + scrollY + margin;
    }

    // Clamp to viewport width
    const maxLeft = scrollX + window.innerWidth - el.offsetWidth - margin;
    left = Math.max(scrollX + margin, Math.min(left, maxLeft));

    el.style.top = `${top}px`;
    el.style.left = `${left}px`;
}

function createButton(config, getSelectionRect, onActivate) {
    const btn = document.createElement('button');
    btn.type = 'button';
    btn.className = 'atoll-ann-btn';
    btn.textContent = config.buttonText;
    btn.setAttribute('aria-label', 'Add annotation');
    btn.style.display = 'none';
    document.body.appendChild(btn);

    btn.addEventListener('click', (e) => {
        e.preventDefault();
        e.stopPropagation();
        onActivate();
    });

    return btn;
}

function createPopover(config, selectedText, onSubmit, onClose) {
    const popover = document.createElement('div');
    popover.className = 'atoll-ann-popover';
    popover.setAttribute('role', 'dialog');
    popover.setAttribute('aria-label', 'Add comment');
    popover.style.display = 'none';
    popover.style.position = 'absolute';

    const closeBtn = document.createElement('button');
    closeBtn.type = 'button';
    closeBtn.className = 'atoll-ann-close';
    closeBtn.textContent = '\u00D7';
    closeBtn.setAttribute('aria-label', 'Close');
    closeBtn.addEventListener('click', onClose);

    const quote = document.createElement('blockquote');
    quote.textContent = truncate(selectedText.trim(), QUOTE_DISPLAY_CHARS);

    const textarea = document.createElement('textarea');
    textarea.placeholder = 'Add your comment\u2026';
    textarea.setAttribute('aria-label', 'Your comment');

    const actions = document.createElement('div');
    actions.className = 'atoll-ann-actions';

    const cancelBtn = document.createElement('button');
    cancelBtn.type = 'button';
    cancelBtn.className = 'atoll-ann-cancel';
    cancelBtn.textContent = 'Cancel';
    cancelBtn.addEventListener('click', onClose);

    const submitBtn = document.createElement('button');
    submitBtn.type = 'button';
    submitBtn.className = 'atoll-ann-submit';
    submitBtn.textContent = 'Send Feedback';
    submitBtn.addEventListener('click', () => {
        onSubmit(textarea.value);
    });

    actions.appendChild(cancelBtn);
    actions.appendChild(submitBtn);

    popover.appendChild(closeBtn);
    popover.appendChild(quote);
    popover.appendChild(textarea);
    popover.appendChild(actions);

    document.body.appendChild(popover);

    return { popover, textarea };
}

// ── Main Init ────────────────────────────────────────────────────────────────

export default function init(element) {
    const placeholder = element.querySelector('.atoll-annotations');
    if (!placeholder) return;

    const config = {
        repo: placeholder.dataset.repo || '',
        target: placeholder.dataset.target || 'issue',
        category: placeholder.dataset.category || '',
        labels: placeholder.dataset.labels || '',
        titlePrefix: placeholder.dataset.titlePrefix || 'Feedback:',
        contentSelector: placeholder.dataset.contentSelector || 'article',
        buttonText: placeholder.dataset.buttonText || '\uD83D\uDCAC',
    };

    if (!config.repo) return;

    const contentArea = document.querySelector(config.contentSelector);
    if (!contentArea) return;

    injectStyles();
    setupTheme();

    // State
    let selectedText = '';
    let selectionRect = null;
    let popoverEl = null;
    let textareaEl = null;
    let isPopoverOpen = false;

    const floatingBtn = createButton(config, () => selectionRect, openPopover);

    function showButton() {
        if (isPopoverOpen) return;
        floatingBtn.style.display = '';
        // Position after display so offsetHeight is valid
        requestAnimationFrame(() => {
            if (selectionRect) {
                positionElement(floatingBtn, selectionRect);
            }
        });
    }

    function hideButton() {
        floatingBtn.style.display = 'none';
    }

    function openPopover() {
        hideButton();
        isPopoverOpen = true;

        const { popover, textarea } = createPopover(
            config,
            selectedText,
            submitFeedback,
            closePopover
        );
        popoverEl = popover;
        textareaEl = textarea;
        popover.style.display = '';

        requestAnimationFrame(() => {
            if (selectionRect) {
                positionElement(popover, selectionRect);
            }
            textarea.focus();
        });
    }

    function closePopover() {
        if (popoverEl) {
            popoverEl.remove();
            popoverEl = null;
            textareaEl = null;
        }
        isPopoverOpen = false;
        selectedText = '';
        selectionRect = null;
    }

    function submitFeedback(userComment) {
        const url = buildGitHubUrl(config, selectedText, userComment);
        window.open(url, '_blank', 'noopener,noreferrer');
        closePopover();
    }

    // ── Selection Handling ───────────────────────────────────────────────────

    function handleSelectionChange() {
        const selection = window.getSelection();
        const text = selection ? selection.toString().trim() : '';

        if (!text) {
            if (!isPopoverOpen) {
                hideButton();
                selectedText = '';
                selectionRect = null;
            }
            return;
        }

        // Check selection is within the content area
        if (!selection || selection.rangeCount === 0) return;
        const range = selection.getRangeAt(0);
        if (!contentArea.contains(range.commonAncestorContainer)) {
            if (!isPopoverOpen) {
                hideButton();
                selectedText = '';
                selectionRect = null;
            }
            return;
        }

        // Ignore selection of the annotations UI itself
        if (
            (floatingBtn && floatingBtn.contains(range.commonAncestorContainer)) ||
            (popoverEl && popoverEl.contains(range.commonAncestorContainer))
        ) {
            return;
        }

        selectedText = text;
        selectionRect = range.getBoundingClientRect();
        showButton();
    }

    contentArea.addEventListener('mouseup', handleSelectionChange);
    contentArea.addEventListener('touchend', handleSelectionChange);
    let selectionDebounce;
    document.addEventListener('selectionchange', () => {
        // Debounce slightly — selectionchange fires repeatedly during drag
        clearTimeout(selectionDebounce);
        selectionDebounce = setTimeout(handleSelectionChange, 100);
    });

    // ── Dismissal ────────────────────────────────────────────────────────────

    document.addEventListener('mousedown', (e) => {
        if (!isPopoverOpen && floatingBtn.style.display === 'none') return;

        const clickedBtn = floatingBtn.contains(e.target);
        const clickedPopover = popoverEl && popoverEl.contains(e.target);

        if (!clickedBtn && !clickedPopover) {
            closePopover();
            hideButton();
        }
    });

    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape') {
            if (isPopoverOpen) {
                closePopover();
            } else {
                hideButton();
            }
        }
    });
}
