/**
 * Atoll Island Custom Element
 *
 * Client-side Web Component that handles island hydration.
 * This is the Atoll equivalent of Astro's astro-island custom element.
 *
 * The element reads metadata attributes set during SSR to:
 * 1. Load the component's client-side JavaScript module
 * 2. Deserialize type-tagged props
 * 3. Collect slot content from the DOM
 * 4. Hydrate the component when the directive conditions are met
 * 5. Coordinate top-down hydration for nested islands
 */

// Prop type constants matching server-side PropType enum
const PROP_TYPE = {
  Value: 0,
  JSON: 1,
  RegExp: 2,
  Date: 3,
  Map: 4,
  Set: 5,
  BigInt: 6,
  URL: 7,
  Uint8Array: 8,
  Uint16Array: 9,
  Uint32Array: 10,
  Infinity: 11,
};

// Prop type revival functions
const propTypes = {
  [PROP_TYPE.Value]: (value) => reviveObject(value),
  [PROP_TYPE.JSON]: (value) => reviveArray(value),
  [PROP_TYPE.RegExp]: (value) => new RegExp(value),
  [PROP_TYPE.Date]: (value) => new Date(value),
  [PROP_TYPE.Map]: (value) => new Map(reviveArray(value)),
  [PROP_TYPE.Set]: (value) => new Set(reviveArray(value)),
  [PROP_TYPE.BigInt]: (value) => BigInt(value),
  [PROP_TYPE.URL]: (value) => new URL(value),
  [PROP_TYPE.Uint8Array]: (value) => new Uint8Array(value),
  [PROP_TYPE.Uint16Array]: (value) => new Uint16Array(value),
  [PROP_TYPE.Uint32Array]: (value) => new Uint32Array(value),
  [PROP_TYPE.Infinity]: (value) => Number.POSITIVE_INFINITY * value,
};

/**
 * Revive a single [type, value] tuple into its original value.
 */
function reviveTuple(raw) {
  const [type, value] = raw;
  return type in propTypes ? propTypes[type](value) : undefined;
}

/**
 * Revive an array of [type, value] tuples.
 */
function reviveArray(raw) {
  return Array.isArray(raw) ? raw.map(reviveTuple) : raw;
}

/**
 * Revive an object whose values are [type, value] tuples.
 */
function reviveObject(raw) {
  if (typeof raw !== "object" || raw === null) return raw;
  return Object.fromEntries(
    Object.entries(raw).map(([key, value]) => [key, reviveTuple(value)])
  );
}

// Global directive registry — populated by directive scripts
window.Atoll = window.Atoll || {};

class AtollIsland extends HTMLElement {
  Component = null;
  hydrator = null;

  static observedAttributes = ["props"];

  disconnectedCallback() {
    document.removeEventListener("atoll:after-swap", this.unmount);
    document.addEventListener("atoll:after-swap", this.unmount, { once: true });
  }

  connectedCallback() {
    if (
      !this.hasAttribute("await-children") ||
      document.readyState === "interactive" ||
      document.readyState === "complete"
    ) {
      this.childrenConnectedCallback();
    } else {
      // For HTML streaming: wait for children to render
      const onConnected = () => {
        document.removeEventListener("DOMContentLoaded", onConnected);
        mo.disconnect();
        this.childrenConnectedCallback();
      };
      const mo = new MutationObserver(() => {
        if (
          this.lastChild?.nodeType === Node.COMMENT_NODE &&
          this.lastChild.nodeValue === "atoll:end"
        ) {
          this.lastChild.remove();
          onConnected();
        }
      });
      mo.observe(this, { childList: true });
      document.addEventListener("DOMContentLoaded", onConnected);
    }
  }

  async childrenConnectedCallback() {
    const beforeHydrationUrl = this.getAttribute("before-hydration-url");
    if (beforeHydrationUrl) {
      await import(beforeHydrationUrl);
    }
    this.start();
  }

  async start() {
    const opts = JSON.parse(this.getAttribute("opts") || "{}");
    const directive = this.getAttribute("client");

    // Check if directive handler is registered
    if (!directive || !Atoll[directive]) {
      // Directive handler not loaded yet, wait for it
      window.addEventListener(`atoll:${directive}`, () => this.start(), {
        once: true,
      });
      return;
    }

    try {
      await Atoll[directive](
        async () => {
          // Load component module
          const componentUrl = this.getAttribute("component-url");
          const componentExport =
            this.getAttribute("component-export") || "default";

          const componentModule = await import(componentUrl);

          // Handle nested exports (e.g., "Namespace.Component")
          if (!componentExport.includes(".")) {
            this.Component = componentModule[componentExport];
          } else {
            this.Component = componentModule;
            for (const part of componentExport.split(".")) {
              this.Component = this.Component[part];
            }
          }

          // For vanilla JS / Web Component islands, the hydrator is the
          // component's init function itself
          this.hydrator =
            this.Component && typeof this.Component.hydrate === "function"
              ? (el) => this.Component.hydrate
              : (el) => (Component, props, slots, metadata) => {
                  if (typeof Component === "function") {
                    Component(el, props, slots, metadata);
                  }
                };

          return this.hydrate;
        },
        opts,
        this
      );
    } catch (e) {
      console.error(
        `[atoll-island] Error hydrating ${this.getAttribute("component-url")}`,
        e
      );
    }
  }

  hydrate = async () => {
    // Guard 1: Hydrator loaded
    if (!this.hydrator) return;

    // Guard 2: Island still in DOM
    if (!this.isConnected) return;

    // Guard 3: Wait for parent island to hydrate first (top-down coordination)
    const parentSsrIsland = this.parentElement?.closest(
      "atoll-island[ssr]"
    );
    if (parentSsrIsland) {
      parentSsrIsland.addEventListener("atoll:hydrate", this.hydrate, {
        once: true,
      });
      return;
    }

    // Collect slots from atoll-slot elements
    const slotted = this.querySelectorAll("atoll-slot");
    const slots = {};

    // Check for template-based slots first (for unused/hidden slots)
    const templates = this.querySelectorAll(
      "template[data-atoll-template]"
    );
    for (const template of templates) {
      const closest = template.closest(this.tagName);
      if (!closest?.isSameNode(this)) continue;
      slots[template.getAttribute("data-atoll-template") || "default"] =
        template.innerHTML;
      template.remove();
    }

    // Collect slots from atoll-slot elements
    for (const slot of slotted) {
      const closest = slot.closest(this.tagName);
      if (!closest?.isSameNode(this)) continue;
      slots[slot.getAttribute("name") || "default"] = slot.innerHTML;
    }

    // Deserialize props
    let props;
    try {
      props = this.hasAttribute("props")
        ? reviveObject(JSON.parse(this.getAttribute("props")))
        : {};
    } catch (e) {
      let componentName =
        this.getAttribute("component-url") || "<unknown>";
      const componentExport = this.getAttribute("component-export");
      if (componentExport) {
        componentName += ` (export ${componentExport})`;
      }
      console.error(
        `[atoll-island] Error parsing props for ${componentName}`,
        this.getAttribute("props"),
        e
      );
      throw e;
    }

    // Execute hydration
    const hydrator = this.hydrator(this);
    await hydrator(this.Component, props, slots, {
      client: this.getAttribute("client"),
    });

    // Complete hydration
    this.removeAttribute("ssr");
    this.dispatchEvent(new CustomEvent("atoll:hydrate"));
  };

  attributeChangedCallback() {
    this.hydrate();
  }

  unmount = () => {
    if (!this.isConnected) {
      this.dispatchEvent(new CustomEvent("atoll:unmount"));
    }
  };
}

// Register the custom element (idempotent)
if (!customElements.get("atoll-island")) {
  customElements.define("atoll-island", AtollIsland);
}
