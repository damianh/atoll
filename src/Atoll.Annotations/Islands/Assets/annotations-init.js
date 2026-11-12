/**
 * Atoll Annotations — Island Init
 *
 * Enables text-selection links and optional contextual GitHub feedback.
 */

// ── Constants ────────────────────────────────────────────────────────────────

const MAX_QUOTE_BODY_CHARS = 2000;
const MAX_FRAGMENT_CHARS = 200;
const MAX_TOTAL_URL_CHARS = 8000;
const QUOTE_DISPLAY_CHARS = 200;
const registrations = new Map();
let selectionControls;

// ── Styles ───────────────────────────────────────────────────────────────────

function injectStyles() {
    if (document.getElementById('atoll-annotations-styles')) return;

    const style = document.createElement('style');
    style.id = 'atoll-annotations-styles';
    style.textContent = `
        .atoll-ann-toolbar {
            position: absolute;
            z-index: 9999;
            display: flex;
            align-items: center;
            gap: 4px;
            border: 1px solid var(--atoll-ann-border, #d0d7de);
            border-radius: 6px;
            background: var(--atoll-ann-bg, #ffffff);
            color: var(--atoll-ann-text, #24292f);
            font-size: 13px;
            font-family: inherit;
            box-shadow: 0 2px 8px rgba(0,0,0,0.12);
            white-space: nowrap;
            transition: box-shadow 0.15s ease;
            user-select: none;
        }
        .atoll-ann-btn {
            padding: 4px 10px;
            border: 0;
            border-radius: 6px;
            background: transparent;
            color: inherit;
            font: inherit;
            cursor: pointer;
        }
        .atoll-ann-btn:hover {
            background: var(--atoll-ann-input-bg, #f6f8fa);
        }
        .atoll-ann-btn:focus-visible {
            outline: 2px solid var(--atoll-ann-accent, #0969da);
            outline-offset: -2px;
        }
        .atoll-ann-status {
            padding-right: 8px;
            font-size: 12px;
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
        .atoll-ann-popover textarea, .atoll-ann-popover input {
            display: block;
            width: 100%;
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
        .atoll-ann-popover textarea {
            min-height: 72px;
        }
        .atoll-ann-popover textarea:focus, .atoll-ann-popover input:focus {
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

function encodeFragmentText(text) {
    return encodeURIComponent(text).replace(/[!'()*-]/g, c =>
        `%${c.charCodeAt(0).toString(16).toUpperCase()}`);
}

function fragmentEdge(text, fromEnd, limit) {
    const characters = Array.from(text);
    if (characters.length <= limit) return text;
    if (fromEnd) {
        const suffix = characters.slice(-limit).join('');
        const boundary = suffix.search(/\s/);
        if (/\s/.test(characters[characters.length - limit - 1])) return suffix;
        if (boundary >= 0) return suffix.slice(boundary).trim();
        return text.slice(text.lastIndexOf(' ') + 1);
    }
    const prefix = characters.slice(0, limit).join('');
    if (/\s/.test(characters[limit])) return prefix;
    const boundary = prefix.lastIndexOf(' ');
    if (boundary >= 0) return prefix.slice(0, boundary);
    const next = text.indexOf(' ');
    return next < 0 ? text : text.slice(0, next);
}

function buildPassageUrl(selectedText) {
    const text = selectedText.trim().replace(/\s+/g, ' ');
    const characters = Array.from(text);
    const lines = selectedText.trim().split(/[\r\n]+/).map(line => line.trim().replace(/\s+/g, ' ')).filter(Boolean);
    let fragment;
    if ((characters.length <= MAX_FRAGMENT_CHARS && lines.length <= 1) ||
        (lines.length <= 1 && !text.includes(' '))) {
        fragment = encodeFragmentText(text);
    } else {
        const half = Math.floor(MAX_FRAGMENT_CHARS / 2);
        const start = fragmentEdge(lines.length > 1 ? lines[0] : text, false, half);
        const end = fragmentEdge(lines.length > 1 ? lines[lines.length - 1] : text, true, half);
        fragment = `${encodeFragmentText(start)},${encodeFragmentText(end)}`;
    }
    const url = new URL(location.href);
    url.hash = '';
    return `${url.href}#:~:text=${fragment}`;
}

function buildGitHubUrl(config, selectedText, userComment) {
    const pageTitle = document.title;
    const sourceLink = `[${pageTitle}](${buildPassageUrl(selectedText)})`;

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

/**
 * Position the feedback button in the right gutter — just outside the
 * content area's right edge, vertically centred on the selection rect.
 * This avoids collision with the browser's native context menu / selection
 * toolbar that appears directly above or below the selected text.
 *
 * Falls back to the standard centred-above/below positioning when the
 * viewport is too narrow for a visible gutter (e.g. mobile portrait).
 */
function positionButtonInGutter(el, selRect, contentArea) {
    const scrollX = window.scrollX;
    const scrollY = window.scrollY;
    const margin = 8;

    const contentRect = contentArea.getBoundingClientRect();
    const gutterLeft = contentRect.right + scrollX + margin;
    const spaceRight = window.innerWidth - contentRect.right;

    // If not enough gutter space, fall back to centred positioning
    if (spaceRight < el.offsetWidth + margin * 2) {
        positionElement(el, selRect);
        return;
    }

    // Vertically centre on the selection
    let top = selRect.top + scrollY + (selRect.height / 2) - (el.offsetHeight / 2);

    // Clamp vertically to viewport
    const minTop = scrollY + margin;
    const maxTop = scrollY + window.innerHeight - el.offsetHeight - margin;
    top = Math.max(minTop, Math.min(top, maxTop));

    el.style.top = `${top}px`;
    el.style.left = `${gutterLeft}px`;
}

function createButton(text, label, onActivate) {
    const btn = document.createElement('button');
    btn.type = 'button';
    btn.className = 'atoll-ann-btn';
    btn.textContent = text;
    btn.setAttribute('aria-label', label);
    btn.title = label;

    btn.addEventListener('click', (e) => {
        e.preventDefault();
        e.stopPropagation();
        onActivate();
    });

    return btn;
}

function createCopyFallback(url, onClose) {
    const popover = document.createElement('div');
    popover.className = 'atoll-ann-popover';
    popover.setAttribute('role', 'dialog');
    popover.setAttribute('aria-label', 'Copy passage link');

    const label = document.createElement('label');
    label.textContent = 'Clipboard access was unavailable. Copy this link manually:';
    const input = document.createElement('input');
    input.type = 'text';
    input.readOnly = true;
    input.value = url;
    input.setAttribute('aria-label', 'Passage link');
    const actions = document.createElement('div');
    actions.className = 'atoll-ann-actions';
    const close = document.createElement('button');
    close.type = 'button';
    close.className = 'atoll-ann-cancel';
    close.textContent = 'Close';
    close.addEventListener('click', onClose);
    label.appendChild(input);
    actions.appendChild(close);
    popover.appendChild(label);
    popover.appendChild(actions);
    document.body.appendChild(popover);
    return { popover, input };
}

function createPopover(selectedText, onSubmit, onClose) {
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
    const placeholder = element.querySelector('.atoll-annotations, .atoll-passage-link');
    if (!placeholder) return;

    const isAnnotation = placeholder.classList.contains('atoll-annotations');
    const config = isAnnotation ? {
        repo: placeholder.dataset.repo || '',
        target: placeholder.dataset.target || 'issue',
        category: placeholder.dataset.category || '',
        labels: placeholder.dataset.labels || '',
        titlePrefix: placeholder.dataset.titlePrefix || 'Feedback:',
        buttonText: placeholder.dataset.buttonText || '\uD83D\uDCAC',
    } : null;
    if (isAnnotation && !config.repo) return;
    const contentSelector = placeholder.dataset.contentSelector || 'article';
    const contentArea = document.querySelector(contentSelector);
    if (!contentArea) return;

    // Both islands may target the same content, in either hydration order.
    const registration = registrations.get(contentArea) || { contentArea, config: null };
    if (config && config.repo) registration.config = config;
    registrations.set(contentArea, registration);

    if (!selectionControls) {
        selectionControls = createSelectionControls();
    } else {
        selectionControls.handleSelectionChange();
    }
}

function createSelectionControls() {
    injectStyles();
    setupTheme();

    let selectedText = '';
    let selectionRect = null;
    let activeRegistration = null;
    let popoverEl = null;
    let isPopoverOpen = false;
    let copyRequest = 0;

    const toolbar = document.createElement('div');
    toolbar.className = 'atoll-ann-toolbar';
    toolbar.setAttribute('role', 'group');
    toolbar.setAttribute('aria-label', 'Selected text actions');
    toolbar.style.display = 'none';
    const commentBtn = createButton('', 'Add annotation', openPopover);
    const linkBtn = createButton('\uD83D\uDD17', 'Copy link to selected text', copyLink);
    const status = document.createElement('span');
    status.className = 'atoll-ann-status';
    status.setAttribute('role', 'status');
    status.setAttribute('aria-live', 'polite');
    toolbar.appendChild(commentBtn);
    toolbar.appendChild(linkBtn);
    toolbar.appendChild(status);
    document.body.appendChild(toolbar);

    toolbar.addEventListener('pointerdown', e => e.preventDefault());
    toolbar.addEventListener('mousedown', e => e.preventDefault());

    function showButton() {
        if (isPopoverOpen) return;
        const config = activeRegistration.config;
        commentBtn.style.display = config ? '' : 'none';
        if (config) commentBtn.textContent = config.buttonText;
        toolbar.style.display = '';
        requestAnimationFrame(() => {
            if (selectionRect && activeRegistration && toolbar.style.display !== 'none') {
                positionButtonInGutter(toolbar, selectionRect, activeRegistration.contentArea);
            }
        });
    }

    function hideButton() {
        toolbar.style.display = 'none';
        status.textContent = '';
    }

    function openPopover() {
        if (!activeRegistration || !activeRegistration.config || !selectedText) return;
        hideButton();
        isPopoverOpen = true;

        const { popover, textarea } = createPopover(
            selectedText,
            submitFeedback,
            closePopover
        );
        popoverEl = popover;
        popover.style.display = '';

        requestAnimationFrame(() => {
            if (selectionRect) {
                positionElement(popover, selectionRect);
            }
            textarea.focus();
        });
    }

    function closePopover() {
        copyRequest++;
        if (popoverEl) {
            popoverEl.remove();
            popoverEl = null;
        }
        isPopoverOpen = false;
        selectedText = '';
        selectionRect = null;
        activeRegistration = null;
    }

    function submitFeedback(userComment) {
        const url = buildGitHubUrl(activeRegistration.config, selectedText, userComment);
        window.open(url, '_blank', 'noopener,noreferrer');
        closePopover();
    }

    async function copyLink() {
        if (!selectedText) return;
        const url = buildPassageUrl(selectedText);
        const request = ++copyRequest;
        try {
            if (!navigator.clipboard || !navigator.clipboard.writeText) {
                throw new Error('Clipboard API unavailable');
            }
            await navigator.clipboard.writeText(url);
            if (request === copyRequest && toolbar.style.display !== 'none') {
                status.textContent = 'Link copied';
                requestAnimationFrame(() => {
                    if (selectionRect && activeRegistration) {
                        positionButtonInGutter(toolbar, selectionRect, activeRegistration.contentArea);
                    }
                });
            }
        } catch {
            if (request !== copyRequest) return;
            hideButton();
            isPopoverOpen = true;
            const { popover, input } = createCopyFallback(url, closePopover);
            popoverEl = popover;
            requestAnimationFrame(() => {
                if (selectionRect) positionElement(popover, selectionRect);
                input.focus();
                input.select();
            });
        }
    }

    // ── Selection Handling ───────────────────────────────────────────────────

    function handleSelectionChange() {
        if (isPopoverOpen) return;
        const selection = window.getSelection();
        const text = selection ? selection.toString().trim() : '';

        if (!text) {
            if (selectedText) copyRequest++;
            hideButton();
            selectedText = '';
            selectionRect = null;
            activeRegistration = null;
            return;
        }

        if (!selection || selection.rangeCount === 0) return;
        const range = selection.getRangeAt(0);
        let registration = null;
        for (const candidate of registrations.values()) {
            if (candidate.contentArea.contains(range.commonAncestorContainer) &&
                (!registration || registration.contentArea.contains(candidate.contentArea))) {
                registration = candidate;
            }
        }
        if (!registration) {
            if (selectedText) copyRequest++;
            hideButton();
            selectedText = '';
            selectionRect = null;
            activeRegistration = null;
            return;
        }

        if (selectedText !== text || activeRegistration !== registration) {
            copyRequest++;
            status.textContent = '';
        }
        selectedText = text;
        activeRegistration = registration;
        selectionRect = range.getBoundingClientRect();
        showButton();
    }

    document.addEventListener('mouseup', handleSelectionChange);
    document.addEventListener('touchend', handleSelectionChange);
    let selectionDebounce;
    document.addEventListener('selectionchange', () => {
        // Debounce slightly — selectionchange fires repeatedly during drag
        clearTimeout(selectionDebounce);
        selectionDebounce = setTimeout(handleSelectionChange, 100);
    });

    // ── Dismissal ────────────────────────────────────────────────────────────

    document.addEventListener('mousedown', (e) => {
        if (!isPopoverOpen && toolbar.style.display === 'none') return;

        const clickedBtn = toolbar.contains(e.target);
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
                closePopover();
                hideButton();
            }
        }
    });

    return { handleSelectionChange };
}
