/**
 * Atoll Island Directive Handlers
 *
 * This module registers the hydration directive handlers that determine
 * WHEN an island component is hydrated on the client side.
 *
 * Directives:
 * - load: Hydrate immediately on page load
 * - idle: Hydrate when the browser is idle (requestIdleCallback)
 * - visible: Hydrate when the element enters the viewport (IntersectionObserver)
 * - media: Hydrate when a CSS media query matches (matchMedia)
 */

window.Atoll = window.Atoll || {};

/**
 * client:load — Hydrate immediately after page load.
 * Uses requestAnimationFrame to ensure the first paint has occurred.
 */
Atoll.load = (cb, _opts, _el) => {
  requestAnimationFrame(async () => {
    const hydrate = await cb();
    await hydrate();
  });
};

/**
 * client:idle — Hydrate when the browser is idle.
 * Uses requestIdleCallback where available, with a setTimeout fallback.
 */
Atoll.idle = (cb, _opts, _el) => {
  const onIdle = async () => {
    const hydrate = await cb();
    await hydrate();
  };

  if ("requestIdleCallback" in window) {
    requestIdleCallback(onIdle);
  } else {
    setTimeout(onIdle, 200);
  }
};

/**
 * client:visible — Hydrate when the element enters the viewport.
 * Uses IntersectionObserver. Supports rootMargin via opts.value.
 */
Atoll.visible = (cb, opts, el) => {
  const rootMargin = opts.value || undefined;

  const observer = new IntersectionObserver(
    async (entries) => {
      for (const entry of entries) {
        if (!entry.isIntersecting) continue;
        observer.disconnect();
        const hydrate = await cb();
        await hydrate();
        break;
      }
    },
    rootMargin ? { rootMargin } : undefined
  );

  // Observe all direct children (the island element's content)
  // If the element has no children yet (streaming), observe the element itself
  const targets = el.children.length > 0 ? el.children : [el];
  for (const target of targets) {
    observer.observe(target);
  }

  // Cleanup if the element is removed from the DOM
  el.addEventListener(
    "atoll:unmount",
    () => {
      observer.disconnect();
    },
    { once: true }
  );
};

/**
 * client:media — Hydrate when a CSS media query matches.
 * Uses matchMedia to listen for query changes.
 */
Atoll.media = (cb, opts, _el) => {
  const query = opts.value;
  if (!query) {
    console.warn("[atoll:media] No media query provided, hydrating immediately");
    requestAnimationFrame(async () => {
      const hydrate = await cb();
      await hydrate();
    });
    return;
  }

  const mql = window.matchMedia(query);

  const handleChange = async () => {
    if (mql.matches) {
      mql.removeEventListener("change", handleChange);
      const hydrate = await cb();
      await hydrate();
    }
  };

  if (mql.matches) {
    // Already matches — hydrate immediately
    requestAnimationFrame(async () => {
      const hydrate = await cb();
      await hydrate();
    });
  } else {
    mql.addEventListener("change", handleChange);
  }
};

// Dispatch events to notify islands that directives are ready
["load", "idle", "visible", "media"].forEach((directive) => {
  window.dispatchEvent(new Event(`atoll:${directive}`));
});
