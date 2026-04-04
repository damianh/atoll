/**
 * Atoll Docs — Tabs Island
 *
 * Activates tab switching for a .tabs component rendered by the server.
 * On load, hides all panels except the first (or synced) one.
 * If data-sync-key is set, synchronizes tab selections across tab groups.
 */

const SYNC_EVENT = 'atoll-tab-sync';

export default function init(element) {
    const tabList = element.querySelector('[role="tablist"]');
    if (!tabList) return;

    const tabs = Array.from(tabList.querySelectorAll('[role="tab"]'));
    const panels = Array.from(element.querySelectorAll('[role="tabpanel"]'));
    const syncKey = tabList.dataset.syncKey || null;

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
        const panel = element.querySelector('#' + panelId);
        if (panel) panel.hidden = false;
    }

    // Initialise: hide all panels except first
    if (tabs.length > 0) {
        activate(tabs[0]);
    }

    tabs.forEach(tab => {
        tab.addEventListener('click', () => {
            const label = tab.dataset.tabLabel;
            activate(tab);

            if (syncKey && label) {
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
