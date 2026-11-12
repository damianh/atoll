import init from '/annotations-init.js';

const name = new URL(location.href).searchParams.get('case');
const article = document.querySelector('article');
const wait = () => new Promise(resolve => setTimeout(resolve, 150));

function assert(condition, message) {
    if (!condition) throw new Error(message);
}

function addIsland(kind) {
    const root = document.createElement('div');
    root.innerHTML = kind === 'annotation'
        ? '<div class="atoll-annotations" data-repo="owner/repo" data-target="issue" data-button-text="Comment"></div>'
        : '<div class="atoll-passage-link"></div>';
    document.body.appendChild(root);
    init(root);
}

async function selectText(text) {
    article.textContent = text;
    const range = document.createRange();
    range.selectNodeContents(article);
    const selection = window.getSelection();
    selection.removeAllRanges();
    selection.addRange(range);
    article.dispatchEvent(new MouseEvent('mouseup', { bubbles: true }));
    await wait();
}

function button(label) {
    return document.querySelector(`button[aria-label="${label}"]`);
}

function setClipboard(writeText) {
    Object.defineProperty(navigator, 'clipboard', { configurable: true, value: { writeText } });
}

async function copy() {
    button('Copy link to selected text').click();
    await wait();
}

async function annotationCopiesEncodedLink() {
    let copied;
    setClipboard(async value => { copied = value; });
    history.replaceState(null, '', `${location.pathname}${location.search}#old-heading`);
    addIsland('annotation');
    await selectText("Café & hyphenated-text, isn't 😀");
    assert(button('Add annotation').textContent === 'Comment', 'Missing configured comment action');
    await copy();
    const expected = `${location.origin}/case?case=annotationCopiesEncodedLink#:~:text=Caf%C3%A9%20%26%20hyphenated%2Dtext%2C%20isn%27t%20%F0%9F%98%80`;
    assert(copied === expected, `Unexpected URL: ${copied}`);
    assert(document.querySelector('[role=status]').textContent === 'Link copied', 'Missing copy confirmation');
}

async function unavailableClipboardShowsManualLink() {
    Object.defineProperty(navigator, 'clipboard', { configurable: true, value: undefined });
    addIsland('link');
    await selectText('A passage without clipboard access');
    await copy();
    const input = document.querySelector('input[aria-label="Passage link"]');
    assert(input && input.value.endsWith('#:~:text=A%20passage%20without%20clipboard%20access'), 'Missing manual link');
}

async function dismissedSelectionIgnoresLateClipboardFailure() {
    let rejectCopy;
    setClipboard(() => new Promise((_, reject) => { rejectCopy = reject; }));
    addIsland('link');
    await selectText('Dismiss this selection');
    button('Copy link to selected text').click();
    document.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape', bubbles: true }));
    rejectCopy(new Error('denied'));
    await wait();
    assert(!document.querySelector('[aria-label="Copy passage link"]'), 'Dismissed action reopened');
}

async function linkOnlyProvidesManualFallback() {
    setClipboard(async () => { throw new Error('denied'); });
    addIsland('link');
    await selectText('A selected passage');
    assert(button('Add annotation').style.display === 'none', 'Comment action must be hidden');
    await copy();
    const input = document.querySelector('input[aria-label="Passage link"]');
    assert(input && input.value.endsWith('#:~:text=A%20selected%20passage'), 'Missing fallback URL');
    assert(document.activeElement === input && input.selectionStart === 0 &&
        input.selectionEnd === input.value.length, 'Fallback link was not selected');
    document.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape', bubbles: true }));
    assert(!document.querySelector('[aria-label="Copy passage link"]'), 'Escape did not dismiss fallback');
}

async function bothIslandsPreserveFeedback() {
    let opened;
    window.open = (...args) => { opened = args; };
    addIsland('link');
    await selectText('Selected text for feedback');
    addIsland('annotation');
    assert(document.querySelectorAll('.atoll-ann-toolbar').length === 1, 'Duplicate toolbar');
    assert(button('Add annotation').style.display !== 'none', 'Comment not enabled after hydration');
    button('Add annotation').click();
    await wait();
    document.querySelector('textarea').value = 'Useful feedback';
    [...document.querySelectorAll('button')].find(element => element.textContent === 'Send Feedback').click();
    assert(opened && opened[1] === '_blank' && opened[2] === 'noopener,noreferrer', 'Feedback did not open safely');
    const url = new URL(opened[0]);
    assert(url.pathname === '/owner/repo/issues/new', 'Incorrect feedback target');
    assert(url.searchParams.get('body').includes('Useful feedback'), 'Missing feedback text');
    assert(url.searchParams.get('body').includes('#:~:text=Selected%20text%20for%20feedback'), 'Missing passage link');
}

async function annotationFirstDoesNotDuplicateControls() {
    addIsland('annotation');
    addIsland('link');
    await selectText('A passage');
    assert(document.querySelectorAll('.atoll-ann-toolbar').length === 1, 'Duplicate toolbar');
    assert(button('Add annotation').style.display !== 'none', 'Comment action disappeared');
    assert(button('Copy link to selected text').style.display !== 'none', 'Link action disappeared');
}

async function longAndMultilineSelectionsUseRanges() {
    let copied;
    setClipboard(async value => { copied = value; });
    addIsland('link');
    const words = Array.from({ length: 60 }, (_, index) => `word${index}`).join(' ');
    await selectText(words);
    await copy();
    const fragment = copied.split('#:~:text=')[1];
    const parts = fragment.split(',');
    assert(parts.length === 2, 'Long selection should use start and end text');
    assert(words.startsWith(decodeURIComponent(parts[0])), 'Incorrect range start');
    assert(words.endsWith(decodeURIComponent(parts[1])), 'Incorrect range end');
    assert(!decodeURIComponent(fragment).includes('…'), 'Fragment contains an ellipsis');
    article.innerHTML = '<p>First paragraph</p><p>Second paragraph</p>';
    const range = document.createRange();
    range.selectNodeContents(article);
    const selection = window.getSelection();
    selection.removeAllRanges();
    selection.addRange(range);
    article.dispatchEvent(new MouseEvent('mouseup', { bubbles: true }));
    await wait();
    await copy();
    assert(copied.endsWith('#:~:text=First%20paragraph,Second%20paragraph'), `Incorrect multiline fragment: ${copied}`);
}

async function outsideSelectionHidesActions() {
    addIsland('link');
    await selectText('Inside');
    assert(document.querySelector('.atoll-ann-toolbar').style.display !== 'none', 'Toolbar not shown');
    const outside = document.querySelector('#outside');
    const range = document.createRange();
    range.selectNodeContents(outside);
    const selection = window.getSelection();
    selection.removeAllRanges();
    selection.addRange(range);
    document.dispatchEvent(new MouseEvent('mouseup', { bubbles: true }));
    await wait();
    assert(document.querySelector('.atoll-ann-toolbar').style.display === 'none', 'Toolbar shown outside content');
}

async function copiedLinkForNavigation() {
    let copied;
    setClipboard(async value => { copied = value; });
    addIsland('link');
    await selectText('A distinctive destination passage');
    await copy();
    const url = new URL(copied);
    url.pathname = '/destination';
    return url.href;
}

async function copiedMultilineLinkForNavigation() {
    let copied;
    setClipboard(async value => { copied = value; });
    addIsland('link');
    article.innerHTML = '<p>A distinctive opening passage</p><p>A distinctive ending passage</p>';
    const range = document.createRange();
    range.selectNodeContents(article);
    const selection = window.getSelection();
    selection.removeAllRanges();
    selection.addRange(range);
    article.dispatchEvent(new MouseEvent('mouseup', { bubbles: true }));
    await wait();
    await copy();
    const url = new URL(copied);
    url.pathname = '/destination-multiline';
    return url.href;
}

const cases = {
    annotationCopiesEncodedLink,
    linkOnlyProvidesManualFallback,
    unavailableClipboardShowsManualLink,
    dismissedSelectionIgnoresLateClipboardFailure,
    bothIslandsPreserveFeedback,
    annotationFirstDoesNotDuplicateControls,
    longAndMultilineSelectionsUseRanges,
    outsideSelectionHidesActions,
    copiedLinkForNavigation,
    copiedMultilineLinkForNavigation,
};

try {
    const url = await cases[name]();
    parent.postMessage({ name, ok: true, url }, location.origin);
} catch (error) {
    parent.postMessage({ name, ok: false, error: error.stack || String(error) }, location.origin);
}
