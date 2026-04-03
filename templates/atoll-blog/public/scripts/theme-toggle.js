// Minimal theme toggle island script.
// This module is loaded by the ThemeToggle island component via client:load.
// It toggles a `data-theme` attribute on <html> between "light" and "dark",
// persisting the user's preference in localStorage.

const STORAGE_KEY = "atoll-theme";

function getPreferredTheme() {
  const stored = localStorage.getItem(STORAGE_KEY);
  if (stored === "light" || stored === "dark") return stored;
  return window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
}

function applyTheme(theme) {
  document.documentElement.setAttribute("data-theme", theme);
  localStorage.setItem(STORAGE_KEY, theme);
}

function init() {
  applyTheme(getPreferredTheme());

  const btn = document.getElementById("theme-toggle");
  if (!btn) return;

  btn.addEventListener("click", () => {
    const current = document.documentElement.getAttribute("data-theme") ?? "light";
    applyTheme(current === "dark" ? "light" : "dark");
  });
}

// Run on page load
if (document.readyState === "loading") {
  document.addEventListener("DOMContentLoaded", init);
} else {
  init();
}
