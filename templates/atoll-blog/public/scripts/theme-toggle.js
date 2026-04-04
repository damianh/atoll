// Minimal theme toggle island script.
// This module is loaded by the ThemeToggle island component via client:load.
// It toggles a `data-theme` attribute on <html> between "light" and "dark",
// persisting the user's preference in localStorage.

const STORAGE_KEY = "atoll-theme";
const DARK = "dark";
const LIGHT = "light";

function getPreferred() {
  const stored = localStorage.getItem(STORAGE_KEY);
  if (stored === DARK || stored === LIGHT) return stored;
  return window.matchMedia("(prefers-color-scheme: dark)").matches ? DARK : LIGHT;
}

function applyTheme(theme) {
  document.documentElement.setAttribute("data-theme", theme);
  const btn = document.getElementById("theme-toggle");
  if (btn) {
    btn.setAttribute("aria-label", theme === DARK ? "Switch to light theme" : "Switch to dark theme");
    btn.textContent = theme === DARK ? "\u2600" : "\u263E";
  }
}

function toggle() {
  const current = document.documentElement.getAttribute("data-theme") || getPreferred();
  const next = current === DARK ? LIGHT : DARK;
  localStorage.setItem(STORAGE_KEY, next);
  applyTheme(next);
}

// Apply theme immediately to avoid FOUC.
applyTheme(getPreferred());

export default function init(element) {
  const btn = element.querySelector("#theme-toggle");
  if (btn) {
    btn.addEventListener("click", toggle);
  }
}
