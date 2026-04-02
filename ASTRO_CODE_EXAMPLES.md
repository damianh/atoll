# Astro Source Code Examples: Exact Implementation Details

## 1. TEMPLATE RENDERING (Core Algorithm)

### Source: `runtime/server/render/astro/render-template.ts` (lines 35-101)

```typescript
export class RenderTemplateResult {
    render(destination: RenderDestination): void | Promise<void> {
        const { htmlParts, expressions } = this;

        // FAST PATH: Direct streaming
        for (let i = 0; i < htmlParts.length; i++) {
            const html = htmlParts[i];
            if (html) {
                destination.write(markHTMLString(html));
            }

            if (i >= expressions.length) break;

            const exp = expressions[i];
            // Skip falsy values except 0
            if (!(exp || exp === 0)) continue;

            const result = renderChild(destination, exp);

            // ASYNC ENCOUNTERED: Switch to buffered mode
            if (isPromise(result)) {
                const startIdx = i + 1;
                const remaining = expressions.length - startIdx;
                const flushers = new Array(remaining);
                
                // Create buffered renderers for ALL remaining expressions
                for (let j = 0; j < remaining; j++) {
                    const rExp = expressions[startIdx + j];
                    flushers[j] = createBufferedRenderer(destination, (bufferDestination) => {
                        if (rExp || rExp === 0) {
                            return renderChild(bufferDestination, rExp);
                        }
                    });
                }

                // After first async completes, flush sequentially
                return result.then(() => {
                    let k = 0;
                    const iterate = (): void | Promise<void> => {
                        while (k < flushers.length) {
                            // Write HTML that precedes expression
                            const rHtml = htmlParts[startIdx + k];
                            if (rHtml) {
                                destination.write(markHTMLString(rHtml));
                            }

                            // Flush expression
                            const flushResult = flushers[k++].flush();
                            if (isPromise(flushResult)) {
                                return flushResult.then(iterate);
                            }
                        }
                        
                        // Final HTML part
                        const lastHtml = htmlParts[htmlParts.length - 1];
                        if (lastHtml) {
                            destination.write(markHTMLString(lastHtml));
                        }
                    };
                    return iterate();
                });
            }
        }
    }
}
```

### Key Insights
1. **Template structure invariant:** `htmlParts.length === expressions.length + 1`
2. **Falsy value handling:** `0` and `false` treated differently (0 is rendered, false is not)
3. **Buffering trigger:** ONE async expression causes ALL remaining to be buffered
4. **Sequential flushing:** Guarantees output order even with concurrent async operations

---

## 2. COMPONENT RENDERING DISPATCHER

### Source: `runtime/server/render/component.ts` (lines 74-300+)

```typescript
async function renderFrameworkComponent(
    result: SSRResult,
    displayName: string,
    Component: unknown,
    _props: Record<string | number, any>,
    slots: any = {},
): Promise<RenderInstance> {
    // STEP 1: Extract client directives
    const { hydration, isPage, props, propsWithoutTransitionAttributes } = extractDirectives(
        _props,
        result.clientDirectives,
    );

    // STEP 2: Metadata for hydration
    let metadata: AstroComponentMetadata = {
        astroStaticSlot: true,
        displayName,
    };

    if (hydration) {
        metadata.hydrate = hydration.directive as AstroComponentMetadata['hydrate'];
        metadata.hydrateArgs = hydration.value;
        metadata.componentExport = hydration.componentExport;
        metadata.componentUrl = hydration.componentUrl;
    }

    // STEP 3: Render slots
    const { children, slotInstructions } = await renderSlots(result, slots);

    // STEP 4: Find renderer
    let renderer: SSRLoadedRenderer | undefined;
    
    if (metadata.hydrate !== 'only') {
        // Check if component has renderer tag
        let isTagged = false;
        try {
            isTagged = Component && (Component as any)[Renderer];
        } catch {
            // Ignore Proxy access errors
        }

        if (isTagged) {
            const rendererName = (Component as any)[Renderer];
            renderer = renderers.find(({ name }) => name === rendererName);
        }

        // Try each renderer's check() hook
        if (!renderer) {
            let error;
            for (const r of renderers) {
                try {
                    if (await r.ssr.check.call({ result }, Component, props, children, metadata)) {
                        renderer = r;
                        break;
                    }
                } catch (e) {
                    error ??= e;
                }
            }
            if (!renderer && error) throw error;
        }

        // Check if HTML element
        if (!renderer && typeof HTMLElement === 'function' && componentIsHTMLElement(Component)) {
            const output = await renderHTMLElement(result, Component, _props, slots);
            return {
                render(destination) {
                    destination.write(output);
                },
            };
        }
    }

    // STEP 5: Render component
    let html = '';
    let attrs: Record<string, string> | undefined = undefined;

    if (!renderer) {
        // Handle client:only and errors
        if (metadata.hydrate === 'only') {
            html = await renderSlotToString(result, slots?.fallback);
        } else {
            throw new AstroError({
                ...AstroErrorData.NoMatchingRenderer,
                message: AstroErrorData.NoMatchingRenderer.message(displayName, ...),
            });
        }
    } else {
        if (metadata.hydrate === 'only') {
            html = await renderSlotToString(result, slots?.fallback);
        } else {
            ({ html, attrs } = await renderer.ssr.renderToStaticMarkup.call(
                { result },
                Component,
                propsWithoutTransitionAttributes,
                children,
                metadata,
            ));
        }
    }

    // STEP 6: Generate hydration island if needed
    let hydrationScript = '';
    if (renderer && metadata.hydrate && hydration) {
        const astroId = shorthash(displayName + metadata.componentUrl);
        const island = await generateHydrateScript(
            { renderer, result, astroId, props, attrs },
            metadata as any,
        );
        
        // Wrap SSR output in island
        hydrationScript = renderElement('astro-island', island);
    }

    // STEP 7: Return render instance
    return {
        async render(destination) {
            if (slotInstructions) {
                slotInstructions.forEach((instruction) => destination.write(instruction));
            }
            destination.write(markHTMLString(hydrationScript || html));
        },
    };
}
```

### Key Points
1. **Renderer selection:** Uses `.ssr.check()` hook to find capable renderer
2. **Component tagging:** Avoids repeated checks via `Renderer` symbol
3. **Error propagation:** Captures first error from renderer checks
4. **Slot rendering:** Parallelized via `Promise.all()` in `renderSlots()`
5. **Island wrapping:** SSR HTML wrapped in `<astro-island>` custom element
6. **Instruction bubbling:** `slotInstructions` propagate hydration metadata upward

---

## 3. PROPS SERIALIZATION WITH TYPE TAGS

### Source: `runtime/server/serialize.ts` (lines 4-114)

```typescript
const PROP_TYPE = {
    Value: 0,
    JSON: 1,           // Arrays
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

function serializeArray(
    value: any[],
    metadata: AstroComponentMetadata | Record<string, any> = {},
    parents = new WeakSet<any>(),
): any[] {
    if (parents.has(value)) {
        throw new Error(`Cyclic reference detected while serializing props for <${metadata.displayName} client:${metadata.hydrate}>!
Cyclic references cannot be safely serialized for client-side usage. Please remove the cyclic reference.`);
    }
    parents.add(value);
    const serialized = value.map((v) => {
        return convertToSerializedForm(v, metadata, parents);
    });
    parents.delete(value);
    return serialized;
}

function serializeObject(
    value: Record<any, any>,
    metadata: AstroComponentMetadata | Record<string, any> = {},
    parents = new WeakSet<any>(),
): Record<any, any> {
    if (parents.has(value)) {
        throw new Error(`Cyclic reference detected while serializing props for <${metadata.displayName} client:${metadata.hydrate}>!
Cyclic references cannot be safely serialized for client-side usage. Please remove the cyclic reference.`);
    }
    parents.add(value);
    const serialized = Object.fromEntries(
        Object.entries(value).map(([k, v]) => {
            return [k, convertToSerializedForm(v, metadata, parents)];
        }),
    );
    parents.delete(value);
    return serialized;
}

function convertToSerializedForm(
    value: any,
    metadata: AstroComponentMetadata | Record<string, any> = {},
    parents = new WeakSet<any>(),
): [ValueOf<typeof PROP_TYPE>, any] | [ValueOf<typeof PROP_TYPE>] {
    const tag = Object.prototype.toString.call(value);
    switch (tag) {
        case '[object Date]': {
            return [PROP_TYPE.Date, (value as Date).toISOString()];
        }
        case '[object RegExp]': {
            return [PROP_TYPE.RegExp, (value as RegExp).source];
        }
        case '[object Map]': {
            return [PROP_TYPE.Map, serializeArray(Array.from(value as Map<any, any>), metadata, parents)];
        }
        case '[object Set]': {
            return [PROP_TYPE.Set, serializeArray(Array.from(value as Set<any>), metadata, parents)];
        }
        case '[object BigInt]': {
            return [PROP_TYPE.BigInt, (value as bigint).toString()];
        }
        case '[object URL]': {
            return [PROP_TYPE.URL, (value as URL).toString()];
        }
        case '[object Array]': {
            return [PROP_TYPE.JSON, serializeArray(value, metadata, parents)];
        }
        case '[object Uint8Array]': {
            return [PROP_TYPE.Uint8Array, Array.from(value as Uint8Array)];
        }
        case '[object Uint16Array]': {
            return [PROP_TYPE.Uint16Array, Array.from(value as Uint16Array)];
        }
        case '[object Uint32Array]': {
            return [PROP_TYPE.Uint32Array, Array.from(value as Uint32Array)];
        }
        default: {
            if (value !== null && typeof value === 'object') {
                return [PROP_TYPE.Value, serializeObject(value, metadata, parents)];
            }
            if (value === Number.POSITIVE_INFINITY) {
                return [PROP_TYPE.Infinity, 1];
            }
            if (value === Number.NEGATIVE_INFINITY) {
                return [PROP_TYPE.Infinity, -1];
            }
            if (value === undefined) {
                return [PROP_TYPE.Value];
            }
            return [PROP_TYPE.Value, value];
        }
    }
}

export function serializeProps(props: any, metadata: AstroComponentMetadata) {
    const serialized = JSON.stringify(serializeObject(props, metadata));
    return serialized;
}
```

### Key Insights
1. **WeakSet for cycle detection:** Prevents infinite loops
2. **Object.prototype.toString.call():** Accurate type detection
3. **Array flattening:** Arrays serialized via `PROP_TYPE.JSON`
4. **Infinity handling:** ±Infinity encoded as `[11, ±1]`
5. **Undefined handling:** Serialized as `[0]` (type only, no value)

---

## 4. CLIENT-SIDE ISLAND HYDRATION

### Source: `runtime/server/astro-island.ts` (lines 133-200)

```typescript
hydrate = async () => {
    // Guard 1: Hydrator loaded
    if (!this.hydrator) return;

    // Guard 2: Island still in DOM
    if (!this.isConnected) return;

    // Guard 3: Wait for parent island
    const parentSsrIsland = this.parentElement?.closest('astro-island[ssr]');
    if (parentSsrIsland) {
        parentSsrIsland.addEventListener('astro:hydrate', this.hydrate, { once: true });
        return;
    }

    // Collect slots from astro-slot elements
    const slotted = this.querySelectorAll('astro-slot');
    const slots: Record<string, string> = {};
    
    // Also check for template-based slots (for unused slots)
    const templates = this.querySelectorAll('template[data-astro-template]');
    for (const template of templates) {
        const closest = template.closest(this.tagName);
        if (!closest?.isSameNode(this)) continue;
        slots[template.getAttribute('data-astro-template') || 'default'] = template.innerHTML;
        template.remove();
    }

    // Collect slots from astro-slot elements
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
        let componentName: string = this.getAttribute('component-url') || '<unknown>';
        const componentExport = this.getAttribute('component-export');
        if (componentExport) {
            componentName += ` (export ${componentExport})`;
        }
        console.error(
            `[hydrate] Error parsing props for component ${componentName}`,
            this.getAttribute('props'),
            e,
        );
        throw e;
    }

    // Execute framework hydration
    let hydrationTimeStart;
    const hydrator = this.hydrator(this);
    if (process.env.NODE_ENV === 'development') hydrationTimeStart = performance.now();
    
    await hydrator(this.Component, props, slots, {
        client: this.getAttribute('client'),
    });

    // Performance tracking
    if (process.env.NODE_ENV === 'development' && hydrationTimeStart)
        this.setAttribute(
            'client-render-time',
            (performance.now() - hydrationTimeStart).toString(),
        );

    // Complete hydration
    this.removeAttribute('ssr');
    this.dispatchEvent(new CustomEvent('astro:hydrate'));
};
```

### Hydration Sequence
1. Check prerequisites (hydrator, connectivity, parent)
2. Collect slot HTML from DOM (astro-slot + templates)
3. Deserialize props with type revival
4. Call framework hydrator with Component, props, slots
5. Remove `ssr` marker attribute
6. Dispatch `astro:hydrate` event

---

## 5. HEAD DEDUPLICATION ALGORITHM

### Source: `runtime/server/render/head.ts` (lines 8-31)

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

export function renderAllHeadContent(result: SSRResult) {
    result._metadata.hasRenderedHead = true;
    let content = '';
    
    // CSP meta tags
    if (result.shouldInjectCspMetaTags && result.cspDestination === 'meta') {
        content += renderElement('meta', {
            props: {
                'http-equiv': 'content-security-policy',
                content: renderCspContent(result),
            },
            children: '',
        }, false);
    }
    
    // Deduplicate and render styles
    const styles = deduplicateElements(Array.from(result.styles)).map((style) =>
        style.props.rel === 'stylesheet' ? renderElement('link', style) : renderElement('style', style),
    );
    result.styles.clear();
    
    // Deduplicate and render scripts
    const scripts = deduplicateElements(Array.from(result.scripts)).map((script) => {
        if (result.userAssetsBase) {
            script.props.src =
                (result.base === '/' ? '' : result.base) + result.userAssetsBase + script.props.src;
        }
        return renderElement('script', script, false);
    });
    
    // Deduplicate and render links
    const links = deduplicateElements(Array.from(result.links)).map((link) =>
        renderElement('link', link, false),
    );

    // Order: styles -> links -> scripts
    content += styles.join('\n') + links.join('\n') + scripts.join('\n');

    // Extra head content from integrations
    if (result._metadata.extraHead.length > 0) {
        for (const part of result._metadata.extraHead) {
            content += part;
        }
    }

    return markHTMLString(content);
}
```

### Deduplication Mechanism
1. **Stable props key:** Sorted JSON of props (order-independent)
2. **Children included:** Key = propsKey + HTML content
3. **Set-based detection:** O(n) instead of O(n²)
4. **Clear after render:** Prevents duplicate injection

---

## 6. MIDDLEWARE EXECUTION WITH REWRITES

### Source: `core/middleware/sequence.ts` (lines 15-96)

```typescript
export function sequence(...handlers: MiddlewareHandler[]): MiddlewareHandler {
    const filtered = handlers.filter((h) => !!h);
    const length = filtered.length;
    
    if (!length) {
        return defineMiddleware((_context, next) => {
            return next();
        });
    }
    
    return defineMiddleware((context, next) => {
        let carriedPayload: RewritePayload | undefined = undefined;
        return applyHandle(0, context);

        function applyHandle(i: number, handleContext: APIContext) {
            const handle = filtered[i];
            
            const result = handle(handleContext, async (payload?: RewritePayload) => {
                if (i < length - 1) {
                    // Not the last middleware, continue chain
                    if (payload) {
                        // Create new request for rewrite
                        let newRequest;
                        if (payload instanceof Request) {
                            newRequest = payload;
                        } else if (payload instanceof URL) {
                            // Clone to avoid consuming stream twice
                            newRequest = new Request(payload, handleContext.request.clone());
                        } else {
                            // String path
                            newRequest = new Request(
                                new URL(payload, handleContext.url.origin),
                                handleContext.request.clone(),
                            );
                        }
                        
                        const oldPathname = handleContext.url.pathname;
                        const pipeline: Pipeline = Reflect.get(handleContext, pipelineSymbol);
                        
                        // Resolve route for rewritten path
                        const { routeData, pathname } = await pipeline.tryRewrite(
                            payload,
                            handleContext.request,
                        );

                        // Validation: can't rewrite SSR → prerendered
                        if (
                            pipeline.manifest.serverLike === true &&
                            handleContext.isPrerendered === false &&
                            routeData.prerender === true
                        ) {
                            throw new AstroError({
                                ...ForbiddenRewrite,
                                message: ForbiddenRewrite.message(
                                    handleContext.url.pathname,
                                    pathname,
                                    routeData.component,
                                ),
                            });
                        }

                        // Update context for next middleware
                        carriedPayload = payload;
                        handleContext.request = newRequest;
                        handleContext.url = new URL(newRequest.url);
                        handleContext.params = getParams(routeData, pathname);
                        handleContext.routePattern = routeData.route;
                        setOriginPathname(
                            handleContext.request,
                            oldPathname,
                            pipeline.manifest.trailingSlash,
                            pipeline.manifest.buildFormat,
                        );
                    }
                    
                    // Call next middleware
                    return applyHandle(i + 1, handleContext);
                } else {
                    // Last middleware, call actual route
                    return next(payload ?? carriedPayload);
                }
            });
            
            return result;
        }
    });
}
```

### Rewrite Semantics
1. **Payload types:** String path, URL object, or Request object
2. **Request cloning:** Prevents stream consumption errors
3. **Context updates:** URL, params, pathname all updated
4. **Pipeline validation:** Checks for forbidden SSR→prerendered rewrites
5. **Origin tracking:** Preserves original pathname across rewrites

---

## 7. RENDER INSTRUCTION SYSTEM

### Source: `runtime/server/render/instruction.ts`

```typescript
const RenderInstructionSymbol = Symbol.for('astro:render');

export type RenderDirectiveInstruction = {
    type: 'directive';
    hydration: HydrationMetadata;
};

export type RenderHeadInstruction = {
    type: 'head';
};

export type RendererHydrationScriptInstruction = {
    type: 'renderer-hydration-script';
    rendererName: string;
    render: () => string;
};

export type MaybeRenderHeadInstruction = {
    type: 'maybe-head';
};

export type ServerIslandRuntimeInstruction = {
    type: 'server-island-runtime';
};

export type RenderScriptInstruction = {
    type: 'script';
    id: string;
    content: string;
};

export type RenderInstruction =
    | RenderDirectiveInstruction
    | RenderHeadInstruction
    | MaybeRenderHeadInstruction
    | RendererHydrationScriptInstruction
    | ServerIslandRuntimeInstruction
    | RenderScriptInstruction;

export function createRenderInstruction<T extends RenderInstruction>(instruction: T): T {
    return Object.defineProperty(instruction as T, RenderInstructionSymbol, {
        value: true,
    });
}

export function isRenderInstruction(chunk: any): chunk is RenderInstruction {
    return chunk && typeof chunk === 'object' && chunk[RenderInstructionSymbol];
}
```

### Processing (from `common.ts`)

```typescript
function stringifyChunk(
    result: SSRResult,
    chunk: string | HTMLString | SlotString | RenderInstruction,
): string {
    if (isRenderInstruction(chunk)) {
        const instruction = chunk;
        switch (instruction.type) {
            case 'directive': {
                // Emit hydration script for client directive
                const { hydration } = instruction;
                let needsHydrationScript = hydration && determineIfNeedsHydrationScript(result);
                let needsDirectiveScript = hydration && determinesIfNeedsDirectiveScript(result, hydration.directive);

                if (needsHydrationScript) {
                    let prescripts = getPrescripts(result, 'both', hydration.directive);
                    return markHTMLString(prescripts);
                } else if (needsDirectiveScript) {
                    let prescripts = getPrescripts(result, 'directive', hydration.directive);
                    return markHTMLString(prescripts);
                } else {
                    return '';
                }
            }
            case 'head': {
                if (!shouldRenderInstruction('head', getInstructionRenderState(result))) {
                    return '';
                }
                return renderAllHeadContent(result);
            }
            case 'maybe-head': {
                if (!shouldRenderInstruction('maybe-head', getInstructionRenderState(result))) {
                    return '';
                }
                return renderAllHeadContent(result);
            }
            case 'renderer-hydration-script': {
                const { rendererSpecificHydrationScripts } = result._metadata;
                const { rendererName } = instruction;

                if (!rendererSpecificHydrationScripts.has(rendererName)) {
                    rendererSpecificHydrationScripts.add(rendererName);
                    return instruction.render();
                }
                return '';
            }
            // ... other cases
        }
    }
    // ... regular chunks
}
```

---

## 8. STREAMING WITH READABLE STREAM

### Source: `runtime/server/render/astro/render.ts` (lines 84-167)

```typescript
export async function renderToReadableStream(
    result: SSRResult,
    componentFactory: AstroComponentFactory,
    props: any,
    children: any,
    isPage = false,
    route?: RouteData,
): Promise<ReadableStream | Response> {
    const templateResult = await callComponentAsTemplateResultOrResponse(
        result,
        componentFactory,
        props,
        children,
        route,
    );

    if (templateResult instanceof Response) return templateResult;

    let renderedFirstPageChunk = false;

    if (isPage) {
        await bufferHeadContent(result);
    }

    return new ReadableStream({
        start(controller) {
            const destination: RenderDestination = {
                write(chunk) {
                    // Auto-inject doctype for first chunk
                    if (isPage && !renderedFirstPageChunk) {
                        renderedFirstPageChunk = true;
                        if (!result.partial && !DOCTYPE_EXP.test(String(chunk))) {
                            const doctype = result.compressHTML ? '<!DOCTYPE html>' : '<!DOCTYPE html>\n';
                            controller.enqueue(encoder.encode(doctype));
                        }
                    }

                    // Responses mid-stream are errors
                    if (chunk instanceof Response) {
                        throw new AstroError({
                            ...AstroErrorData.ResponseSentError,
                        });
                    }

                    // Encode and enqueue bytes
                    const bytes = chunkToByteArray(result, chunk);
                    controller.enqueue(bytes);
                },
            };

            (async () => {
                try {
                    await templateResult.render(destination);
                    controller.close();
                } catch (e) {
                    // Enhance error with location info
                    if (AstroError.is(e) && !e.loc) {
                        e.setLocation({
                            file: route?.component,
                        });
                    }

                    // Queue error asynchronously to flush sync chunks
                    setTimeout(() => controller.error(e), 0);
                }
            })();
        },
        cancel() {
            // Client disconnected, stop rendering
            result.cancelled = true;
        },
    });
}
```

### Key Features
1. **DOCTYPE auto-injection:** Only if not present and `isPage=true`
2. **TextEncoder:** Converts strings to UTF-8 bytes
3. **Error handling:** Queued after synchronous chunks flush
4. **Cancellation support:** Sets `result.cancelled` flag
5. **Response errors:** Throws if Response encountered mid-stream

---

## COMPILATION OUTPUT PATTERN

A compiled `.astro` component with slots and children:

```astro
---
import Button from './Button.astro';
const { title } = Astro.props;
---
<div class="card">
    <h2>{title}</h2>
    <slot />
    <Button>Click me</Button>
</div>
```

Compiles to (conceptually):

```typescript
const $$module = ($$result, $$props, $$slots) => {
    const { title } = $$props;
    
    return renderTemplate`<div class="card"><h2>${title}</h2>${renderSlot($$result, $$slots.default)}<astro-island ...>${renderComponent($$result, "Button", Button, {}, $$slots)}</astro-island></div>`;
};

$$module.isAstroComponentFactory = true;
$$module.propagation = getPropagationHint(...);
export default $$module;
```

---

## CRITICAL IMPLEMENTATION PATTERNS

### Pattern 1: WeakSet for Cycle Detection
```typescript
const parents = new WeakSet<any>();
if (parents.has(value)) throw new Error('Cyclic reference');
parents.add(value);
// process
parents.delete(value);
```

### Pattern 2: Object.prototype.toString for Type Detection
```typescript
const tag = Object.prototype.toString.call(value);
// '[object Date]', '[object RegExp]', etc.
```

### Pattern 3: Stable Props Key (Order-Independent)
```typescript
const keys = Object.keys(props).sort();
const key = '{' + keys.map(k => JSON.stringify(k) + ':' + JSON.stringify(props[k])).join(',') + '}';
```

### Pattern 4: Symbol Tagging for Type Discrimination
```typescript
const symbol = Symbol.for('astro:something');
Object.defineProperty(obj, symbol, { value: true });
function isOfType(obj) { return !!obj?.[symbol]; }
```

### Pattern 5: Async Expression Buffering
```typescript
if (isPromise(result)) {
    // Buffer all remaining expressions
    const flushers = remaining.map(exp => createBufferedRenderer(...));
    return result.then(() => flushSequentially(flushers));
}
```

All these patterns are essential to replicate in a C# implementation.
