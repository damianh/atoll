# Astro Framework Deep-Dive Analysis: Exact Mechanics

## Executive Summary

This document provides precise, code-level details of how the Astro framework works internally. It's based on direct analysis of the Astro source code at `packages/astro/src/`. This is critical information for designing a C#-native equivalent.

---

## 1. .ASTRO FILE COMPILATION OUTPUT

### 1.1 Compilation Flow

**File:** `packages/astro/src/core/compile/compile.ts`

The `.astro` file compilation process:

```typescript
export async function compile({
    astroConfig,
    viteConfig,
    toolbarEnabled,
    filename,
    source,
}: CompileProps): Promise<CompileResult>

// Uses @astrojs/compiler (Rust-based)
transformResult = await transform(source, {
    compact: astroConfig.compressHTML,
    filename,
    normalizedFilename: normalizeFilename(filename, astroConfig.root),
    sourcemap: 'both',
    internalURL: 'astro/compiler-runtime',
    astroGlobalArgs: JSON.stringify(astroConfig.site),
    scopedStyleStrategy: astroConfig.scopedStyleStrategy,
    resultScopedSlot: true,
    transitionsAnimationURL: 'astro/components/viewtransitions.css',
    annotateSourceFile: viteConfig.command === 'serve' && astroConfig.devToolbar?.enabled && toolbarEnabled,
    preprocessStyle: createStylePreprocessor({ /* ... */ }),
    async resolvePath(specifier) { return resolvePath(specifier, filename); }
});
```

**Key Points:**
- The `@astrojs/compiler` is a **Rust-based external compiler** that transforms `.astro` to TypeScript
- Compilation result includes both JavaScript code AND CSS (with scoping info)
- Returns `CompileResult` which extends `TransformResult` with custom CSS handling

### 1.2 Compiled .astro Component Structure

A compiled `.astro` component is a **module that exports a component factory function**.

The factory function signature:
```typescript
// The AstroComponentFactory type
interface AstroComponentFactory {
    (result: SSRResult, props: any, slots: any): 
        RenderTemplateResult | Response | HeadAndContent | ThinHead | Promise<...>
    isAstroComponentFactory?: boolean
    moduleId?: string
    propagation?: PropagationHint
}
```

**Example compilation (conceptual):**

Input (`.astro` file):
```astro
---
const { title } = Astro.props;
const count = 5;
---
<h1>{title}</h1>
<p>Count: {count}</p>
```

Output (compiled TypeScript):
```typescript
// Generated factory function
const $$module = ($$result, $$props, $$slots) => {
    // 1. Extract props
    const { title } = $$props;
    
    // 2. Server-side logic
    const count = 5;
    
    // 3. Render template literal with interpolations
    return renderTemplate`<h1>${title}</h1><p>Count: ${count}</p>`;
};

// Mark as Astro component factory
$$module.isAstroComponentFactory = true;
export default $$module;
```

### 1.3 Template Expression Handling

**File:** `packages/astro/src/runtime/server/render/astro/render-template.ts`

Compiled components return a `RenderTemplateResult`:

```typescript
export class RenderTemplateResult {
    private htmlParts: TemplateStringsArray;    // Static HTML strings
    public expressions: any[];                  // Dynamic expressions
    
    constructor(htmlParts, expressions) {
        this.htmlParts = htmlParts;
        this.expressions = expressions.map((expression) => {
            // Wrap Promises for error handling
            if (isPromise(expression)) {
                return Promise.resolve(expression).catch((err) => {
                    if (!this.error) {
                        this.error = err;
                        throw err;
                    }
                });
            }
            return expression;
        });
    }
    
    render(destination: RenderDestination): void | Promise<void> {
        // Fast path: direct streaming for sync expressions
        // When async expression encountered, buffer remaining for order preservation
        for (let i = 0; i < this.htmlParts.length; i++) {
            const html = this.htmlParts[i];
            if (html) destination.write(markHTMLString(html));
            
            if (i >= this.expressions.length) break;
            const exp = this.expressions[i];
            if (!(exp || exp === 0)) continue;  // Skip falsy (but not 0)
            
            const result = renderChild(destination, exp);
            
            if (isPromise(result)) {
                // Fall back to buffered rendering for remaining async expressions
                // This preserves output order while allowing parallelization
                return result.then(() => { /* flush remaining */ });
            }
        }
    }
}
```

**Template Pattern (ES6 template literal pattern):**
```
Template structure: htmlParts[0] + exp[0] + htmlParts[1] + exp[1] + ... + htmlParts[N]
Where: htmlParts.length === expressions.length + 1
```

### 1.4 Conditional Rendering

Handled through JavaScript ternary/logical operators:

```astro
---
const show = true;
---
{show ? <div>Visible</div> : null}
```

Compiles to:
```typescript
renderTemplate`${show ? renderTemplate`<div>Visible</div>` : null}`
```

The `renderChild` function in `any.ts` handles `null`/`undefined` (renders nothing).

### 1.5 Slot Handling

Slots are passed as a `ComponentSlots` object:

```typescript
export type ComponentSlots = Record<string, ComponentSlotValue>;
export type ComponentSlotValue = (
    result: SSRResult,
) => RenderTemplateResult | Promise<RenderTemplateResult>;
```

Example compiled component with slots:

```astro
---
const { default: defaultSlot } = Astro.slots;
---
<div>
    <slot />
</div>
```

Compiles to:
```typescript
const $$module = ($$result, $$props, $$slots) => {
    return renderTemplate`<div>${renderSlot($$result, $$slots.default)}</div>`;
};
```

### 1.6 Component References (Child Components)

When a component references another component:

```astro
---
import Card from './Card.astro';
---
<Card title="Test" />
```

Compiles to:
```typescript
const $$module = ($$result, $$props, $$slots) => {
    return renderTemplate`${renderComponent(
        $$result,
        'Card',
        Card,
        { title: 'Test' },
        $$slots
    )}`;
};
```

The `renderComponent` function (in `component.ts`) handles:
1. **Astro components** → Direct factory invocation
2. **Framework components** (React, Vue, etc.) → Renderer-specific logic
3. **Client-side hydration** → Generation of `<astro-island>` custom element
4. **HTML elements** → Direct string rendering

---

## 2. RENDERING RUNTIME

### 2.1 Core Rendering Types

**File:** `packages/astro/src/runtime/server/render/common.ts`

```typescript
export type RenderDestinationChunk =
    | string
    | HTMLBytes
    | HTMLString
    | SlotString
    | ArrayBufferView
    | RenderInstruction
    | Response;

export interface RenderDestination {
    write(chunk: RenderDestinationChunk): void;
}

export interface RenderInstance {
    render: (destination: RenderDestination) => Promise<void> | void;
}

export type RenderFunction = (destination: RenderDestination) => Promise<void> | void;
```

### 2.2 renderComponent Function

**File:** `packages/astro/src/runtime/server/render/component.ts` (lines 74-593)

This is the **core dispatcher** for rendering all component types:

```typescript
async function renderFrameworkComponent(
    result: SSRResult,
    displayName: string,
    Component: unknown,
    _props: Record<string | number, any>,
    slots: any = {},
): Promise<RenderInstance>

// Steps:
1. Extract client directives (e.g., client:load) via extractDirectives()
2. Render slots via renderSlots()
3. Call renderer.ssr.check() to find matching renderer
4. If hydration needed: generateHydrateScript() → creates <astro-island>
5. Return RenderInstance with render() method
```

### 2.3 renderTemplate Function

**File:** `packages/astro/src/runtime/server/render/astro/render-template.ts`

```typescript
export function renderTemplate(
    htmlParts: TemplateStringsArray,
    ...expressions: any[]
): RenderTemplateResult {
    return new RenderTemplateResult(htmlParts, expressions);
}
```

**Rendering algorithm:**
1. **Fast path:** Stream HTML and sync expressions directly
2. **Async encountered:** Buffer remaining expressions to preserve order
3. **Promise resolution:** Flush buffered expressions sequentially

### 2.4 renderToString (Full Document Rendering)

**File:** `packages/astro/src/runtime/server/render/astro/render.ts`

```typescript
export async function renderToString(
    result: SSRResult,
    componentFactory: AstroComponentFactory,
    props: any,
    children: any,
    isPage = false,
    route?: RouteData,
): Promise<string | Response>

// Process:
1. Call componentFactory(result, props, children) → RenderTemplateResult
2. If Response returned, return immediately (for Astro.response)
3. Create RenderDestination that accumulates to string
4. Call templateResult.render(destination)
5. Auto-inject DOCTYPE if isPage=true and not already present
6. Return complete HTML string
```

### 2.5 renderToReadableStream (Streaming Render)

```typescript
export async function renderToReadableStream(
    result: SSRResult,
    componentFactory: AstroComponentFactory,
    props: any,
    children: any,
    isPage = false,
    route?: RouteData,
): Promise<ReadableStream | Response>

// Process:
1. Create ReadableStream controller
2. RenderDestination writes chunks via controller.enqueue(bytes)
3. Handle Response throws (cannot modify headers after streaming starts)
4. Error handling with try/catch around templateResult.render()
5. Support client disconnects via cancel() callback
```

### 2.6 RenderInstruction Type System

**File:** `packages/astro/src/runtime/server/render/instruction.ts`

```typescript
export type RenderInstruction =
    | RenderDirectiveInstruction          // client:load hydration
    | RenderHeadInstruction               // <head> rendering
    | MaybeRenderHeadInstruction          // Conditional head
    | RendererHydrationScriptInstruction  // Renderer-specific scripts
    | ServerIslandRuntimeInstruction      // Server island runtime
    | RenderScriptInstruction;            // Inline scripts

export interface RenderDirectiveInstruction {
    type: 'directive';
    hydration: HydrationMetadata;        // Contains client directive info
}

export interface RenderHeadInstruction {
    type: 'head';
}

export function createRenderInstruction<T extends RenderInstruction>(instruction: T): T {
    return Object.defineProperty(instruction as T, RenderInstructionSymbol, {
        value: true,
    });
}

export function isRenderInstruction(chunk: any): chunk is RenderInstruction {
    return chunk && typeof chunk === 'object' && chunk[RenderInstructionSymbol];
}
```

**How Instructions Work:**
- Rendered components can emit `RenderInstruction` objects
- These bubble up to page render level (via slot strings)
- Page renderer processes them to inject scripts/head content only once
- Prevents duplicate hydration scripts for multiple islands of same type

### 2.7 Streaming with Order Preservation

From `render-template.ts` (lines 35-101):

```typescript
render(destination: RenderDestination): void | Promise<void> {
    // Fast path
    for (let i = 0; i < this.htmlParts.length; i++) {
        const html = this.htmlParts[i];
        if (html) destination.write(markHTMLString(html));
        if (i >= this.expressions.length) break;
        
        const exp = this.expressions[i];
        if (!(exp || exp === 0)) continue;
        
        const result = renderChild(destination, exp);
        
        if (isPromise(result)) {
            // Async encountered: buffer remaining expressions
            const startIdx = i + 1;
            const remaining = this.expressions.length - startIdx;
            const flushers = new Array(remaining);
            
            for (let j = 0; j < remaining; j++) {
                flushers[j] = createBufferedRenderer(destination, (bufferDestination) => {
                    const rExp = this.expressions[startIdx + j];
                    if (rExp || rExp === 0) {
                        return renderChild(bufferDestination, rExp);
                    }
                });
            }
            
            return result.then(() => {
                // Sequentially flush remaining expressions
                let k = 0;
                const iterate = (): void | Promise<void> => {
                    while (k < flushers.length) {
                        const rHtml = this.htmlParts[startIdx + k];
                        if (rHtml) destination.write(markHTMLString(rHtml));
                        
                        const flushResult = flushers[k++].flush();
                        if (isPromise(flushResult)) {
                            return flushResult.then(iterate);
                        }
                    }
                    const lastHtml = this.htmlParts[this.htmlParts.length - 1];
                    if (lastHtml) destination.write(markHTMLString(lastHtml));
                };
                return iterate();
            });
        }
    }
}
```

**Key Insight:** Once an async expression is encountered, all remaining expressions are rendered in buffered mode (sequentially) to guarantee output order is preserved.

---

## 3. ASTRO-ISLAND CUSTOM ELEMENT

### 3.1 Complete Implementation

**File:** `packages/astro/src/runtime/server/astro-island.ts`

The custom element is a **Web Component** that handles client-side hydration:

```typescript
class AstroIsland extends HTMLElement {
    public Component: any;
    public hydrator: any;
    static observedAttributes = ['props'];

    disconnectedCallback() {
        document.removeEventListener('astro:after-swap', this.unmount);
        document.addEventListener('astro:after-swap', this.unmount, { once: true });
    }

    connectedCallback() {
        if (
            !this.hasAttribute('await-children') ||
            document.readyState === 'interactive' ||
            document.readyState === 'complete'
        ) {
            this.childrenConnectedCallback();
        } else {
            // For HTML streaming: wait for children to render
            const onConnected = () => {
                document.removeEventListener('DOMContentLoaded', onConnected);
                mo.disconnect();
                this.childrenConnectedCallback();
            };
            const mo = new MutationObserver(() => {
                if (
                    this.lastChild?.nodeType === Node.COMMENT_NODE &&
                    this.lastChild.nodeValue === 'astro:end'
                ) {
                    this.lastChild.remove();
                    onConnected();
                }
            });
            mo.observe(this, { childList: true });
            document.addEventListener('DOMContentLoaded', onConnected);
        }
    }

    async childrenConnectedCallback() {
        let beforeHydrationUrl = this.getAttribute('before-hydration-url');
        if (beforeHydrationUrl) {
            await import(beforeHydrationUrl);
        }
        this.start();
    }

    async start() {
        const opts = JSON.parse(this.getAttribute('opts')!);
        const directive = this.getAttribute('client') as directiveAstroKeys;
        
        if (Astro[directive] === undefined) {
            // Directive handler not loaded yet, wait for it
            window.addEventListener(`astro:${directive}`, () => this.start(), { once: true });
            return;
        }
        
        try {
            await Astro[directive]!(
                async () => {
                    // Load component and renderer
                    const rendererUrl = this.getAttribute('renderer-url');
                    const [componentModule, { default: hydrator }] = await Promise.all([
                        import(this.getAttribute('component-url')!),
                        rendererUrl ? import(rendererUrl) : () => () => {},
                    ]);
                    
                    // Handle nested exports
                    const componentExport = this.getAttribute('component-export') || 'default';
                    if (!componentExport.includes('.')) {
                        this.Component = componentModule[componentExport];
                    } else {
                        this.Component = componentModule;
                        for (const part of componentExport.split('.')) {
                            this.Component = this.Component[part];
                        }
                    }
                    
                    this.hydrator = hydrator;
                    return this.hydrate;  // Return hydration callback
                },
                opts,
                this,
            );
        } catch (e) {
            console.error(`[astro-island] Error hydrating ${this.getAttribute('component-url')}`, e);
        }
    }

    hydrate = async () => {
        if (!this.hydrator) return;
        if (!this.isConnected) return;

        // Wait for parent island to hydrate first (top-down)
        const parentSsrIsland = this.parentElement?.closest('astro-island[ssr]');
        if (parentSsrIsland) {
            parentSsrIsland.addEventListener('astro:hydrate', this.hydrate, { once: true });
            return;
        }

        // Collect slots
        const slotted = this.querySelectorAll('astro-slot');
        const slots: Record<string, string> = {};
        const templates = this.querySelectorAll('template[data-astro-template]');
        
        for (const template of templates) {
            const closest = template.closest(this.tagName);
            if (!closest?.isSameNode(this)) continue;
            slots[template.getAttribute('data-astro-template') || 'default'] = template.innerHTML;
            template.remove();
        }
        
        for (const slot of slotted) {
            const closest = slot.closest(this.tagName);
            if (!closest?.isSameNode(this)) continue;
            slots[slot.getAttribute('name') || 'default'] = slot.innerHTML;
        }

        // Deserialize props
        let props: Record<string, unknown>;
        try {
            props = this.hasAttribute('props')
                ? reviveObject(JSON.parse(this.getAttribute('props')!))
                : {};
        } catch (e) {
            console.error(`[hydrate] Error parsing props`, this.getAttribute('props'), e);
            throw e;
        }

        // Execute hydration
        let hydrationTimeStart;
        const hydrator = this.hydrator(this);
        if (process.env.NODE_ENV === 'development') hydrationTimeStart = performance.now();
        
        await hydrator(this.Component, props, slots, {
            client: this.getAttribute('client'),
        });
        
        if (process.env.NODE_ENV === 'development' && hydrationTimeStart) {
            this.setAttribute(
                'client-render-time',
                (performance.now() - hydrationTimeStart).toString(),
            );
        }
        
        this.removeAttribute('ssr');
        this.dispatchEvent(new CustomEvent('astro:hydrate'));
    };

    attributeChangedCallback() {
        this.hydrate();
    }

    unmount = () => {
        if (!this.isConnected) this.dispatchEvent(new CustomEvent('astro:unmount'));
    };
}

if (!customElements.get('astro-island')) {
    customElements.define('astro-island', AstroIsland);
}
```

### 3.2 Props Serialization

**File:** `packages/astro/src/runtime/server/serialize.ts`

Props are serialized to a **type-tagged format** to handle complex types:

```typescript
const PROP_TYPE = {
    Value: 0,           // Plain JS values (number, string, bool)
    JSON: 1,            // Arrays
    RegExp: 2,          // /regex/
    Date: 3,            // new Date()
    Map: 4,             // Map objects
    Set: 5,             // Set objects
    BigInt: 6,          // BigInt(123n)
    URL: 7,             // new URL()
    Uint8Array: 8,      // Binary data
    Uint16Array: 9,
    Uint32Array: 10,
    Infinity: 11,       // ±Infinity
};

export function serializeProps(props: any, metadata: AstroComponentMetadata): string {
    const serialized = JSON.stringify(serializeObject(props, metadata));
    return serialized;
}
```

**Serialized Format Example:**
```javascript
// Input: { name: "test", date: new Date("2024-01-01"), count: 42n, urls: [new URL("...")] }

// Serialized: [[0,{"name":[0,"test"],"date":[3,"2024-01-01T00:00:00.000Z"],"count":[6,"42"],"urls":[1,[[0,"https://..."]]]}]]
```

**Island HTML:**
```html
<astro-island
    component-url="/MyComponent.js"
    component-export="default"
    renderer-url="/@astrojs/react/hydrator.js"
    client="load"
    props="[[0,{...}]]"
    ssr
>
    <!-- SSR content -->
</astro-island>
```

### 3.3 Props Deserialization (Client-Side)

From `astro-island.ts` (lines 15-46):

```typescript
const propTypes: PropTypeSelector = {
    0: (value) => reviveObject(value),
    1: (value) => reviveArray(value),
    2: (value) => new RegExp(value),
    3: (value) => new Date(value),
    4: (value) => new Map(reviveArray(value)),
    5: (value) => new Set(reviveArray(value)),
    6: (value) => BigInt(value),
    7: (value) => new URL(value),
    8: (value) => new Uint8Array(value),
    9: (value) => new Uint16Array(value),
    10: (value) => new Uint32Array(value),
    11: (value) => Number.POSITIVE_INFINITY * value,
};

const reviveTuple = (raw: any): any => {
    const [type, value] = raw;
    return type in propTypes ? propTypes[type](value) : undefined;
};

const reviveArray = (raw: any): any => (raw as Array<any>).map(reviveTuple);

const reviveObject = (raw: any): any => {
    if (typeof raw !== 'object' || raw === null) return raw;
    return Object.fromEntries(Object.entries(raw).map(([key, value]) => [key, reviveTuple(value)]));
};
```

### 3.4 Hydration Handshake

**Server side (hydration.ts):**
```typescript
export async function generateHydrateScript(
    scriptOptions: HydrateScriptOptions,
    metadata: Required<AstroComponentMetadata>,
): Promise<SSRElement>

// Creates island element with attributes:
const island: SSRElement = {
    children: '',
    props: {
        'component-url': await result.resolve(decodeURI(componentUrl)),
        'component-export': componentExport.value,
        'renderer-url': await result.resolve(decodeURI(renderer.clientEntrypoint)),
        'props': escapeHTML(serializeProps(props, metadata)),
        'ssr': '',                          // Marks as needing hydration
        'client': hydrate,                  // Directive: 'load', 'idle', 'visible', etc.
        'before-hydration-url': beforeHydrationUrl,
        'opts': escapeHTML(JSON.stringify({
            name: metadata.displayName,
            value: metadata.hydrateArgs || '',
        })),
    }
};
```

**Client-side flow:**
1. `connectedCallback()` → Check if children ready
2. `childrenConnectedCallback()` → Load `before-hydration-url` if present
3. `start()` → Wait for client directive handler (e.g., `Astro.load()`)
4. Client directive callback → Load component module and hydrator
5. `hydrate()` → Call framework hydrator with Component, props, slots
6. Remove `ssr` attribute
7. Dispatch `astro:hydrate` event (for parent islands to detect completion)

### 3.5 Slots in Islands

Slots are either:
1. **Rendered templates** → `<template data-astro-template="name">...</template>`
2. **Slot elements** → `<astro-slot name="name">...</astro-slot>`

The hydration function (from framework renderer) gets slots as HTML strings and lets framework render them.

---

## 4. STYLE SCOPING

### 4.1 Style Processing Pipeline

**File:** `packages/astro/src/core/compile/style.ts`

```typescript
export function createStylePreprocessor({
    filename,
    viteConfig,
    astroConfig,
    cssPartialCompileResults,
    cssTransformErrors,
}): PreprocessStyleFn {
    return async (content, attrs) => {
        const index = processedStylesCount++;
        const lang = `.${attrs?.lang || 'css'}`.toLowerCase();
        const id = `${filename}?astro&type=style&index=${index}&lang${lang}`;
        
        try {
            // Use Vite's CSS preprocessor
            const result = await preprocessCSS(content, id, viteConfig);
            
            // Rewrite CSS URLs to include base path
            const rewrittenCode = rewriteCssUrls(result.code, astroConfig.base);
            
            cssPartialCompileResults[index] = {
                isGlobal: 'is:global' in attrs,
                dependencies: result.deps ? [...result.deps].map((dep) => normalizePath(dep)) : [],
            };
            
            return { code: rewrittenCode, map: result.map };
        } catch (err: any) {
            err = enhanceCSSError(err, filename, content);
            cssTransformErrors.push(err);
            return { error: err + '' };
        }
    };
}
```

### 4.2 Style Scoping Strategy

**Configuration:** `astroConfig.scopedStyleStrategy`

Astro supports multiple scoping strategies:
1. **`where`** → Uses `:where()` pseudo-class (0 specificity)
2. **`class`** → Adds `.astro-XXXX` class selector
3. **`attribute`** → Adds `[data-astro-XXXX]` attribute selector

The **Rust compiler** applies the scoping based on this config. The exact hash is generated from the file path.

### 4.3 Global Styles

Styles with `is:global` directive are NOT scoped:

```astro
<style is:global>
    /* This applies globally */
    body { margin: 0; }
</style>

<style>
    /* This is scoped to this component */
    .container { padding: 10px; }
</style>
```

The `isGlobal` flag is captured in `CompileCssResult`:

```typescript
export interface CompileCssResult {
    code: string;
    isGlobal: boolean;           // Whether this is <style is:global>
    dependencies: string[];      // CSS dependencies (for hot reload)
}
```

### 4.4 CSS Variable Scoping

CSS variables are **automatically scoped** with a prefix like `--astro-HASH`:

Input:
```css
:root {
    --color: blue;
}
```

Output (with `where` strategy):
```css
:where(.astro-XXXX) :root {
    --color: blue;
}
```

This ensures styles don't conflict across components.

### 4.5 URL Rewriting in CSS

From `style.ts` (lines 41-92):

```typescript
function rewriteCssUrls(css: string, base: string): string {
    if (!base || base === '/') return css;
    
    const normalizedBase = base.endsWith('/') ? base.slice(0, -1) : base;
    if (!normalizedBase.startsWith('/')) return css;
    
    // Regex matches url(...) in CSS
    const cssUrlRE = /(?<!@import\s+)(?<=^|[^\w\-\u0080-\uffff])url\((\s*('[^']+'|"[^"]+")\s*|(?:\\.|[^'")\\])+)\)/g;
    
    return css.replace(cssUrlRE, (match, rawUrl: string) => {
        let url = rawUrl.trim();
        let quote = '';
        
        if ((url.startsWith("'") && url.endsWith("'")) || (url.startsWith('"') && url.endsWith('"'))) {
            quote = url[0];
            url = url.slice(1, -1);
        }
        
        url = url.trim();
        
        const isRootRelative = url.startsWith('/') && !url.startsWith('//');
        const isExternal = url.startsWith('data:') || url.startsWith('http:') || url.startsWith('https:');
        const alreadyHasBase = url.startsWith(normalizedBase + '/');
        
        if (isRootRelative && !isExternal && !alreadyHasBase) {
            return `url(${quote}${normalizedBase}${url}${quote})`;
        }
        
        return match;
    });
}
```

---

## 5. ASTRO GLOBAL OBJECT

### 5.1 Available Properties and Methods

**File:** `packages/astro/src/types/public/context.ts`

The `Astro` global in `.astro` files extends `APIContext` and adds:

```typescript
export interface AstroGlobal {
    // From APIContext:
    site: URL | undefined;
    generator: string;
    clientAddress: string;
    cookies: AstroCookies;
    session: AstroSession | undefined;
    cache: CacheLike;
    request: Request;
    url: URL;
    originPathname: string;
    params: Record<string, string | undefined>;
    props: Record<string, any>;
    locals: Record<string, any>;
    
    // Astro-specific:
    response: ResponseInit & { readonly headers: Headers };
    self: AstroComponentFactory;  // For recursive component calls
    slots: Record<string, true | undefined> & {
        has(slotName: string): boolean;
        render(slotName: string, args?: any[]): Promise<string>;
    };
    
    // Utilities:
    redirect(path: string, status?: number): Response;
    rewrite(rewrite: RewritePayload): void;
    url: URL;
    
    // Methods:
    getActionResult<TAction>(action: TAction): ActionReturnType<TAction> | undefined;
    callAction<TAction>(action: TAction, input: any): Promise<any>;
}
```

### 5.2 Astro.props

Contains component props passed from parent:

```astro
---
const { title, description } = Astro.props;
---
<h1>{title}</h1>
<p>{description}</p>
```

### 5.3 Astro.slots

Object with slot names as keys:

```astro
---
if (Astro.slots.has('sidebar')) {
    // Render sidebar if provided
}
---
{Astro.slots.has('default') && await Astro.slots.render('default')}
```

### 5.4 Astro.response

Mutable response object for setting status/headers:

```astro
---
Astro.response.status = 404;
Astro.response.headers.set('Cache-Control', 'no-cache');
---
<h1>Not Found</h1>
```

### 5.5 Astro.redirect and Astro.rewrite

```astro
---
// Redirect
if (!user) {
    return Astro.redirect('/login');  // Returns Response
}

// Rewrite (internal)
if (Astro.url.pathname === '/old-path') {
    return Astro.rewrite('/new-path');
}
---
```

### 5.6 Astro.self

For recursive component rendering:

```astro
---
// Tree.astro
export interface Props {
    items: (TreeNode | string)[];
}
type TreeNode = { name: string; children?: TreeNode[] };

const { items } = Astro.props;
---
<ul>
    {items.map(item => (
        <li>
            {typeof item === 'string' ? item : (
                <>
                    {item.name}
                    {item.children && (
                        <Astro.self items={item.children} />
                    )}
                </>
            )}
        </li>
    ))}
</ul>
```

### 5.7 Astro in getStaticPaths()

In `getStaticPaths()` context, `Astro` is limited (created by `createAstro()`):

```typescript
export function createAstro(site: string | undefined): AstroGlobal {
    return {
        get site() {
            console.warn('Astro.site inside getStaticPaths is deprecated');
            return site ? new URL(site) : undefined;
        },
        get generator() { return ASTRO_GENERATOR; },
        // All other properties throw:
        get props() { throw createError('props'); },
        get request() { throw createError('request'); },
        // ... etc
    };
}
```

---

## 6. HEAD MANAGEMENT

### 6.1 Head Injection System

**File:** `packages/astro/src/runtime/server/render/head.ts`

The head content is collected during rendering and injected at one point:

```typescript
export function renderAllHeadContent(result: SSRResult): string {
    result._metadata.hasRenderedHead = true;
    let content = '';
    
    // 1. CSP meta tags (if configured)
    if (result.shouldInjectCspMetaTags && result.cspDestination === 'meta') {
        content += renderElement('meta', {
            props: {
                'http-equiv': 'content-security-policy',
                content: renderCspContent(result),
            },
            children: '',
        }, false);
    }
    
    // 2. Styles (deduplicated)
    const styles = deduplicateElements(Array.from(result.styles)).map((style) =>
        style.props.rel === 'stylesheet' ? renderElement('link', style) : renderElement('style', style),
    );
    result.styles.clear();  // Clear for any new styles added later
    
    // 3. Links
    const links = deduplicateElements(Array.from(result.links)).map((link) =>
        renderElement('link', link, false),
    );
    
    // 4. Scripts
    const scripts = deduplicateElements(Array.from(result.scripts)).map((script) => {
        if (result.userAssetsBase) {
            script.props.src = (result.base === '/' ? '' : result.base) + result.userAssetsBase + script.props.src;
        }
        return renderElement('script', script, false);
    });
    
    // Order: styles -> links -> scripts
    content += styles.join('\n') + links.join('\n') + scripts.join('\n');
    
    // 5. Extra head content (from integrations)
    if (result._metadata.extraHead.length > 0) {
        for (const part of result._metadata.extraHead) {
            content += part;
        }
    }
    
    return markHTMLString(content);
}
```

### 6.2 Head Deduplication

```typescript
function stablePropsKey(props: Record<string, unknown>): string {
    const keys = Object.keys(props).sort();
    let result = '{';
    for (let i = 0; i < keys.length; i++) {
        if (i > 0) result += ',';
        result += JSON.stringify(keys[i]) + ':' + JSON.stringify(props[keys[i]]);
    }
    result += '}';
    return result;
}

function deduplicateElements(elements: any[]): any[] {
    if (elements.length <= 1) return elements;
    const seen = new Set<string>();
    return elements.filter((item) => {
        const key = stablePropsKey(item.props) + item.children;
        if (seen.has(key)) return false;
        seen.add(key);
        return true;
    });
}
```

**Key-order independent deduplication:** Elements are identified by sorted prop keys, ensuring `<meta a="1" b="2">` and `<meta b="2" a="1">` are treated as duplicates.

### 6.3 Head Propagation (Nested Components)

**File:** `packages/astro/src/core/head-propagation/`

Head content from child components is **propagated up** to the page-level head:

1. **Rendering order:** Top-down component tree
2. **Head collection:** Each component can add to `result.styles`, `result.scripts`, `result.links`
3. **RenderInstruction mechanism:** Head instructions bubble up through slot strings
4. **Single injection:** `renderAllHeadContent()` called once at page level

### 6.4 renderHead() and maybeRenderHead()

```typescript
export function renderHead(): RenderHeadInstruction {
    return createRenderInstruction({ type: 'head' });
}

export function maybeRenderHead(): MaybeRenderHeadInstruction {
    return createRenderInstruction({ type: 'maybe-head' });
}
```

- `renderHead()` → Used in explicit `<head>` components
- `maybeRenderHead()` → Inserted before first non-head element; deduplicated at page level

---

## 7. MIDDLEWARE

### 7.1 Middleware Definition

**File:** `packages/astro/src/core/middleware/defineMiddleware.ts`

```typescript
export function defineMiddleware(fn: MiddlewareHandler): MiddlewareHandler {
    return fn;
}

export type MiddlewareHandler = (
    context: APIContext,
    next: MiddlewareNext,
) => Promise<Response> | Response | Promise<void> | void;

export type MiddlewareNext = (rewritePayload?: RewritePayload) => Promise<Response>;
```

Simple identity function—the real work is in `callMiddleware()`.

### 7.2 Middleware Execution

**File:** `packages/astro/src/core/middleware/callMiddleware.ts`

```typescript
export async function callMiddleware(
    onRequest: MiddlewareHandler,
    apiContext: APIContext,
    responseFunction: (
        apiContext: APIContext,
        rewritePayload?: RewritePayload,
    ) => Promise<Response> | Response,
): Promise<Response> {
    let nextCalled = false;
    let responseFunctionPromise: Promise<Response> | Response | undefined = undefined;
    
    const next: MiddlewareNext = async (payload) => {
        nextCalled = true;
        responseFunctionPromise = responseFunction(apiContext, payload);
        return responseFunctionPromise;
    };
    
    const middlewarePromise = onRequest(apiContext, next);
    
    return await Promise.resolve(middlewarePromise).then(async (value) => {
        if (nextCalled) {
            // next() was called
            if (typeof value !== 'undefined') {
                // Middleware returned after calling next()
                if (!(value instanceof Response)) {
                    throw new AstroError(AstroErrorData.MiddlewareNotAResponse);
                }
                return value;
            } else {
                // Middleware called next() and returned nothing (void)
                if (responseFunctionPromise) {
                    return responseFunctionPromise;
                } else {
                    throw new AstroError(AstroErrorData.MiddlewareNotAResponse);
                }
            }
        } else if (typeof value === 'undefined') {
            // next() NOT called and no return
            throw new AstroError(AstroErrorData.MiddlewareNoDataOrNextCalled);
        } else if (!(value instanceof Response)) {
            // next() NOT called but non-Response returned
            throw new AstroError(AstroErrorData.MiddlewareNotAResponse);
        } else {
            // next() NOT called and Response returned
            return value;
        }
    });
}
```

**Execution Modes:**
1. **Pass-through:** `next()` called, returns undefined → Use response from route
2. **Modify & forward:** `next()` called, returns Response → Use modified response
3. **Short-circuit:** No `next()` call, returns Response → Skip route, use middleware response
4. **Invalid:** No `next()` call, no Response → Error

### 7.3 Middleware Sequencing

**File:** `packages/astro/src/core/middleware/sequence.ts`

```typescript
export function sequence(...handlers: MiddlewareHandler[]): MiddlewareHandler {
    const filtered = handlers.filter((h) => !!h);
    const length = filtered.length;
    
    if (!length) {
        return defineMiddleware((_context, next) => next());
    }
    
    return defineMiddleware((context, next) => {
        let carriedPayload: RewritePayload | undefined = undefined;
        return applyHandle(0, context);
        
        function applyHandle(i: number, handleContext: APIContext) {
            const handle = filtered[i];
            const result = handle(handleContext, async (payload?: RewritePayload) => {
                if (i < length - 1) {
                    if (payload) {
                        // Create new request for rewrite
                        let newRequest;
                        if (payload instanceof Request) {
                            newRequest = payload;
                        } else if (payload instanceof URL) {
                            newRequest = new Request(payload, handleContext.request.clone());
                        } else {
                            newRequest = new Request(
                                new URL(payload, handleContext.url.origin),
                                handleContext.request.clone(),
                            );
                        }
                        
                        // Update context for next middleware
                        const oldPathname = handleContext.url.pathname;
                        const pipeline: Pipeline = Reflect.get(handleContext, pipelineSymbol);
                        const { routeData, pathname } = await pipeline.tryRewrite(payload, handleContext.request);
                        
                        // Validation: can't rewrite SSR→prerendered
                        if (pipeline.manifest.serverLike === true &&
                            handleContext.isPrerendered === false &&
                            routeData.prerender === true) {
                            throw new AstroError(ForbiddenRewrite);
                        }
                        
                        carriedPayload = payload;
                        handleContext.request = newRequest;
                        handleContext.url = new URL(newRequest.url);
                        handleContext.params = getParams(routeData, pathname);
                        handleContext.routePattern = routeData.route;
                        setOriginPathname(handleContext.request, oldPathname, ...);
                    }
                    return applyHandle(i + 1, handleContext);
                } else {
                    return next(payload ?? carriedPayload);
                }
            });
            return result;
        }
    });
}
```

**Key Features:**
- Supports rewrites within middleware chain
- Updates context (URL, params, routeData) for each middleware
- Tracks origin pathname before rewrites
- Validates SSR→prerendered rewrites (forbidden)

### 7.4 Middleware Composition Example

```typescript
// src/middleware.ts
import { defineMiddleware, sequence } from 'astro:middleware';

const authMiddleware = defineMiddleware(async (context, next) => {
    if (!context.request.headers.get('authorization')) {
        return new Response('Unauthorized', { status: 401 });
    }
    return next();
});

const loggingMiddleware = defineMiddleware(async (context, next) => {
    const response = await next();
    console.log(`${context.request.method} ${context.url.pathname} → ${response.status}`);
    return response;
});

export const onRequest = sequence(loggingMiddleware, authMiddleware);
```

---

## 8. CONTENT COLLECTION RENDERING

### 8.1 Render Function

**File:** `packages/astro/src/types/public/content.ts`

```typescript
export type ContentEntryRenderFunction = (entry: DataEntry) => Promise<RenderedContent>;

export interface RenderedContent {
    html: string;
}
```

The `render()` function is defined by the content entry type handler (e.g., markdown plugin).

### 8.2 Content Entry Types

```typescript
export interface ContentEntryType {
    extensions: string[];  // e.g., ['.md', '.mdx']
    
    getEntryInfo(params: {
        fileUrl: URL;
        contents: string;
    }): GetContentEntryInfoReturnType | Promise<GetContentEntryInfoReturnType>;
    
    getRenderModule?(
        this: rollup.PluginContext,
        params: {
            contents: string;
            fileUrl: URL;
            viteId: string;
        },
    ): rollup.LoadResult | Promise<rollup.LoadResult>;
    
    contentModuleTypes?: string;
    
    getRenderFunction?(config: AstroConfig): Promise<ContentEntryRenderFunction>;
    
    handlePropagation?: boolean;  // For propagating styles/scripts from rendered content
}
```

### 8.3 Content Entry Module

Content files are loaded as modules:

```typescript
export type ContentEntryModule = {
    id: string;                 // Unique identifier
    collection: string;         // Collection name
    slug: string;              // URL slug
    body: string;              // Raw content
    data: Record<string, unknown>;  // Parsed frontmatter
    _internal: {
        rawData: string;       // Unparsed frontmatter
        filePath: string;      // File system path
    };
};
```

### 8.4 Markdown Rendering Example

When using `@astrojs/markdown`:

```typescript
// Markdown entry module
export const id = 'blog/post-1.md';
export const collection = 'blog';
export const slug = 'post-1';
export const body = '# Title\n\nContent here...';
export const data = { title: 'My Post', date: new Date() };

export const Content = async (props) => {
    // Returns AstroComponentFactory that renders compiled markdown
    return renderTemplate`<h1>My Post</h1><p>Content here...</p>`;
};

// Or with render():
export async function render() {
    return {
        html: '<h1>My Post</h1><p>Content here...</p>',
    };
}
```

### 8.5 MDX Rendering

For MDX files, content is compiled to JavaScript:

```typescript
export const Content: MDXContent = async (props) => {
    // MDX component with custom component override support
    return renderTemplate`
        ${renderComponent(
            result,
            'MDXComponent',
            MdxComponent,
            { components: props.components },
            {}
        )}
    `;
};
```

The `MDXContent` type supports passing custom components:

```astro
---
import { getEntry, render } from 'astro:content';
import CustomHeading from '../components/CustomHeading.astro';

const entry = await getEntry('blog', 'post-1');
const { Content } = await render(entry);
---
<Content components={{ h1: CustomHeading }} />
```

### 8.6 Content Collection Query API

Content collections are queried via:

```typescript
import { getCollection, getEntry, render } from 'astro:content';

// Get all entries in a collection
const posts = await getCollection('blog');

// Get single entry
const post = await getEntry('blog', 'post-1');

// Render entry
const { Content, headings } = await render(post);

// Type-safe access
const { title, date } = post.data;
```

---

## SUMMARY TABLE: Component Rendering Path

| Stage | File | Process |
|-------|------|---------|
| **1. Compilation** | `core/compile/compile.ts` | `.astro` → TypeScript via Rust compiler |
| **2. Component Factory** | `runtime/server/render/astro/factory.ts` | Returns `AstroComponentFactory` (function) |
| **3. Template Result** | `runtime/server/render/astro/render-template.ts` | `renderTemplate()` creates `RenderTemplateResult` |
| **4. Component Instance** | `runtime/server/render/astro/instance.ts` | `AstroComponentInstance` wraps factory + slots |
| **5. Framework Detection** | `runtime/server/render/component.ts` | Detects React/Vue/Svelte via renderer.ssr.check() |
| **6. Hydration Script** | `runtime/server/hydration.ts` | Generates `<astro-island>` custom element |
| **7. Island Element** | `runtime/server/astro-island.ts` | Client-side hydration orchestrator |
| **8. Page Rendering** | `runtime/server/render/astro/render.ts` | `renderToString()` or `renderToReadableStream()` |
| **9. Head Injection** | `runtime/server/render/head.ts` | `renderAllHeadContent()` dedupes & injects |
| **10. Streaming Output** | `runtime/server/render/common.ts` | `RenderDestination.write()` handles chunk types |

---

## KEY ARCHITECTURAL INSIGHTS

### 1. **Two-Phase Rendering**
- **Phase 1 (Server):** Component tree → HTML strings
- **Phase 2 (Client):** Islands hydrate with JavaScript

### 2. **Async-First Architecture**
- All component rendering returns `Promise<RenderInstance>`
- Streaming respects order: once async expression hit, buffer remaining
- Enables true progressive HTML streaming

### 3. **Prop Serialization**
- Type-tagged format for cross-boundary serialization
- Supports complex types: Date, Map, Set, BigInt, URL, typed arrays
- Cyclic reference detection

### 4. **Head Propagation**
- RenderInstructions bubble up through nested components
- Deduplication prevents duplicate scripts/styles
- Single injection point at page level

### 5. **Middleware Chain**
- Supports rewrites with context updates
- Validation layer for SSR→prerendered rewrites
- Composable via `sequence()`

### 6. **Client Directive System**
- `Astro[directive]()` callbacks for load/idle/visible/media/only
- Top-down hydration (parent before children)
- Event-based parent-child coordination

---

## FOR C# IMPLEMENTATION

Key things to replicate:

1. **Component Factory Pattern** → Interface returning renderable templates
2. **RenderTemplate Class** → Template literal handling with expression streaming
3. **Async Rendering Pipeline** → Preserve output order during streaming
4. **Island Custom Element** → Web Component equivalent, prop serialization/deserialization
5. **Render Instructions** → Tagged metadata objects that bubble up
6. **Head Deduplication** → Stable props key generation
7. **Middleware Chain Execution** → Sequential with context mutation
8. **Style Scoping** → Hash-based prefixes from file paths
9. **Slot System** → Function-based lazy rendering with context
10. **Streaming with Backpressure** → ReadableStream integration

The Astro architecture is **highly optimized for streaming HTML** with **progressive enhancement** as the core philosophy.
