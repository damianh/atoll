/**
 * Atoll Docs — Tabs Island
 *
 * Activates tab switching for a .tabs component rendered by the server.
 * Supports two modes:
 *
 *   Programmatic mode: [role="tablist"] already exists in the HTML (server-rendered).
 *   Slot mode:         No [role="tablist"]. Builds the tablist from [data-tab-label] sections.
 *
 * When data-sync-key is set, synchronizes tab selections across tab groups on the page
 * and persists the selection to localStorage so it survives page reloads.
 */

const SYNC_EVENT = 'atoll-tab-sync';
const STORAGE_PREFIX = 'atoll-tab-';

function readStorage(syncKey) {
    try {
        return localStorage.getItem(STORAGE_PREFIX + syncKey);
    } catch (_) {
        return null;
    }
}

function writeStorage(syncKey, label) {
    try {
        localStorage.setItem(STORAGE_PREFIX + syncKey, label);
    } catch (_) {
        // Ignore unavailable storage (private mode, quota exceeded, etc.)
    }
}

export default function init(element) {
    // syncKey lives on the outer .tabs div (not on the tablist).
    const syncKey = element.dataset.syncKey || null;

    let tabs;
    let panels;

    const existingTabList = element.querySelector('[role="tablist"]');

    if (existingTabList) {
        // ── Programmatic mode ──
        // Server already rendered the tablist; just wire up the existing elements.
        tabs = Array.from(existingTabList.querySelectorAll('[role="tab"]'));
        panels = Array.from(element.querySelectorAll('[role="tabpanel"]'));
    } else {
        // ── Slot mode ──
        // Build the tablist from [data-tab-label] sections rendered by TabItem components.
        const sections = Array.from(element.querySelectorAll('[data-tab-label]'));
        if (sections.length === 0) return;

        const tabListEl = document.createElement('div');
        tabListEl.className = 'tabs-header';
        tabListEl.setAttribute('role', 'tablist');

        tabs = [];
        panels = [];

        sections.forEach((section, i) => {
            const label = section.dataset.tabLabel || String(i);
            const icon = section.dataset.tabIcon || null;
            const tabId = 'tab-' + element.id + '-' + i;
            const panelId = 'panel-' + element.id + '-' + i;

            // Create tab button
            const btn = document.createElement('button');
            btn.setAttribute('role', 'tab');
            btn.setAttribute('aria-selected', 'false');
            btn.setAttribute('aria-controls', panelId);
            btn.id = tabId;
            btn.className = 'tab-button';
            btn.dataset.tabLabel = label;

            if (icon) {
                const iconEl = document.createElement('span');
                iconEl.className = 'tab-icon';
                iconEl.dataset.icon = icon;
                btn.appendChild(iconEl);
            }

            btn.appendChild(document.createTextNode(label));
            tabListEl.appendChild(btn);
            tabs.push(btn);

            // Promote section to tabpanel
            section.setAttribute('role', 'tabpanel');
            section.id = panelId;
            section.setAttribute('aria-labelledby', tabId);
            panels.push(section);
        });

        element.insertBefore(tabListEl, element.firstChild);
    }

    if (tabs.length === 0) return;

    function activate(tab) {
        tabs.forEach(t => {
            t.setAttribute('aria-selected', 'false');
            t.classList.remove('tab-button-active');
        });
        panels.forEach(p => {
            p.hidden = true;
        });

        tab.setAttribute('aria-selected', 'true');
        tab.classList.add('tab-button-active');

        const panelId = tab.getAttribute('aria-controls');
        const panel = panelId ? document.getElementById(panelId) : null;
        if (panel) panel.hidden = false;
    }

    // Determine the initial tab to activate.
    let initialTab = tabs[0];
    if (syncKey) {
        const saved = readStorage(syncKey);
        if (saved) {
            const match = tabs.find(t => t.dataset.tabLabel === saved);
            if (match) initialTab = match;
        }
    }
    activate(initialTab);

    tabs.forEach(tab => {
        tab.addEventListener('click', () => {
            const label = tab.dataset.tabLabel;
            activate(tab);

            if (syncKey && label) {
                writeStorage(syncKey, label);
                document.dispatchEvent(new CustomEvent(SYNC_EVENT, {
                    detail: { syncKey, label }
                }));
            }
        });
    });

    if (syncKey) {
        document.addEventListener(SYNC_EVENT, e => {
            if (e.detail.syncKey !== syncKey) return;
            const target = tabs.find(t => t.dataset.tabLabel === e.detail.label);
            if (target) activate(target);
        });
    }
}
