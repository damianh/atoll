/**
 * Atoll Docs — Theme Toggle
 *
 * Reads localStorage preference, toggles `data-theme` on `<html>`,
 * and persists the choice. Falls back to `prefers-color-scheme` when
 * no preference has been stored.
 */

const STORAGE_KEY = 'atoll-theme';
const DARK = 'dark';
const LIGHT = 'light';

function getPreferred() {
    const stored = localStorage.getItem(STORAGE_KEY);
    if (stored === DARK || stored === LIGHT) return stored;
    return window.matchMedia('(prefers-color-scheme: dark)').matches ? DARK : LIGHT;
}

function applyTheme(theme) {
    document.documentElement.setAttribute('data-theme', theme);
    const btn = document.getElementById('theme-toggle');
    if (btn) {
        btn.setAttribute('aria-label', theme === DARK ? 'Switch to light theme' : 'Switch to dark theme');
        btn.textContent = theme === DARK ? '☀' : '☾';
    }
}

function toggle() {
    const current = document.documentElement.getAttribute('data-theme') || getPreferred();
    const next = current === DARK ? LIGHT : DARK;
    localStorage.setItem(STORAGE_KEY, next);
    applyTheme(next);
}

// Apply theme immediately to avoid FOUC (called before DOMContentLoaded).
applyTheme(getPreferred());

export default function init(element) {
    const btn = element.querySelector('#theme-toggle');
    if (btn) {
        btn.addEventListener('click', toggle);
    }
}
