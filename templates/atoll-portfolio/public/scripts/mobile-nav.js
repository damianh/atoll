// Minimal mobile navigation island script.
// This module is loaded by the MobileNav island component via client:media("(max-width: 768px)").
// It is only hydrated on mobile viewports, keeping the hamburger menu interactive
// without loading JS on desktop where it is not needed.

function init() {
  const toggle = document.getElementById("mobile-nav-toggle");
  const menu = document.getElementById("mobile-nav-menu");
  const close = document.getElementById("mobile-nav-close");
  if (!toggle || !menu) return;

  // Show the toggle button now that JS is active
  toggle.style.display = "inline-block";

  function openMenu() {
    menu.style.display = "block";
    document.body.style.overflow = "hidden";
    close?.focus();
  }

  function closeMenu() {
    menu.style.display = "none";
    document.body.style.overflow = "";
    toggle.focus();
  }

  toggle.addEventListener("click", openMenu);
  close?.addEventListener("click", closeMenu);

  // Close on Escape key
  document.addEventListener("keydown", (e) => {
    if (e.key === "Escape" && menu.style.display === "block") {
      closeMenu();
    }
  });

  // Close when a nav link is clicked
  menu.querySelectorAll("a").forEach((link) => {
    link.addEventListener("click", closeMenu);
  });
}

// Run on page load
if (document.readyState === "loading") {
  document.addEventListener("DOMContentLoaded", init);
} else {
  init();
}
