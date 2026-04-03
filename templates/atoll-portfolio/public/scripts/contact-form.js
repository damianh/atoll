// Minimal contact form island script.
// This module is loaded by the ContactForm island component via client:load.
// It enhances the form with client-side validation and async submission,
// displaying status messages without a full page reload.

function showStatus(el, message, isError) {
  el.textContent = message;
  el.style.display = "block";
  el.style.background = isError ? "var(--color-error, #fee2e2)" : "var(--color-success, #d1fae5)";
  el.style.color = isError ? "var(--color-error-text, #991b1b)" : "var(--color-success-text, #065f46)";
}

function init() {
  const form = document.getElementById("contact-form");
  const status = document.getElementById("form-status");
  if (!form || !status) return;

  form.addEventListener("submit", async (e) => {
    e.preventDefault();

    const submitBtn = form.querySelector("[type=submit]");
    if (submitBtn) submitBtn.disabled = true;

    status.style.display = "none";

    try {
      const data = new FormData(form);
      const response = await fetch(form.action, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(Object.fromEntries(data)),
      });

      if (response.ok) {
        showStatus(status, "Message sent! I'll get back to you soon.", false);
        form.reset();
      } else {
        showStatus(status, "Something went wrong. Please try again.", true);
      }
    } catch {
      showStatus(status, "Unable to send message. Please check your connection.", true);
    } finally {
      if (submitBtn) submitBtn.disabled = false;
    }
  });
}

// Run on page load
if (document.readyState === "loading") {
  document.addEventListener("DOMContentLoaded", init);
} else {
  init();
}
