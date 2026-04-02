# Astro Framework Quick Reference for C# Implementation

## 1. COMPONENT COMPILATION MODEL

### Input (.astro file)
```astro
---
const { title } = Astro.props;
---
<h1>{title}</h1>
```

### Output (Compiled TypeScript)
```typescript
const $$module = ($$result, $$props, $$slots) => {
    const { title } = $$props;
    return renderTemplate`<h1>${title}</h1>`;
};
$$module.isAstroComponentFactory = true;
export default $$module;
```

### Key Types
- **AstroComponentFactory**: `(result: SSRResult, props: any, slots: any) => RenderTemplateResult | Response | Promise<...>`
- **RenderTemplateResult**: Contains `htmlParts` (static strings) and `expressions` (dynamic values)

---

## 2. RENDERING FLOW DIAGRAM

```
┌─────────────────────────────────────────────┐
│ renderToString() / renderToReadableStream() │
└────────────────┬────────────────────────────┘
                 │
                 ▼
        ┌─────────────────────┐
        │ Call Component      │
        │ Factory Function    │
        └────────┬────────────┘
                 │
                 ▼
        ┌─────────────────────┐
        │ RenderTemplateResult│
        │ (htmlParts +        │
        │  expressions)       │
        └────────┬────────────┘
                 │
                 ▼
        ┌─────────────────────┐
        │ .render(destination)│
        │ (streaming loop)    │
        └────────┬────────────┘
                 │
        ┌────────▼─────────────┐
        │  Async expression?   │
        └────┬──────────┬──────┘
         no  │          │ yes
             │          ▼
             │   ┌──────────────┐
             │   │ Buffer       │
             │   │ remaining    │
             │   │ sequentially │
             │   └──────┬───────┘
             │          │
             └──────┬───┘
                    ▼
        ┌─────────────────────┐
        │ Destination.write() │
        │ (HTML chunks)       │
        └─────────────────────┘
```

---

## 3. CORE ABSTRACTIONS

### RenderDestination
```csharp
interface IRenderDestination {
    void Write(object chunk);  // string | ArrayBuffer | HTMLString | RenderInstruction | Response
}
```

### RenderInstance
```csharp
interface IRenderInstance {
    Task Render(IRenderDestination destination);
}
```

### RenderInstruction
```csharp
class RenderInstruction {
    string Type;  // "directive" | "head" | "maybe-head" | "renderer-hydration-script" | "script" | "server-island-runtime"
}

// Subtypes:
class DirectiveInstruction : RenderInstruction {
    HydrationMetadata Hydration;  // client:load info
}
```

---

## 4. COMPONENT TYPES & ROUTING

### Astro Component (.astro)
- Server-only rendering
- Returns `RenderTemplateResult`
- No client JavaScript unless has `client:` directive

### Framework Component (React, Vue, etc.)
- SSR via framework renderer
- Props serialized to JSON with type tags
- Creates `<astro-island>` for hydration

### API Endpoint (.ts)
- Returns `Response`
- Receives `APIContext` (subset of Astro)
- No rendering, direct HTTP response

---

## 5. PROP SERIALIZATION FORMAT

### Type Tags
```
0: Plain values (number, string, bool)
1: Array
2: RegExp
3: Date
4: Map
5: Set
6: BigInt
7: URL
8: Uint8Array
9: Uint16Array
10: Uint32Array
11: Infinity (±)
```

### Example
```javascript
// Input
{ name: "test", date: new Date(), count: 42n }

// Serialized format
[[0, { name: [0, "test"], date: [3, "2024-01-01T..."], count: [6, "42"] }]]

// HTML attribute
props="[[0, { name: [0, \"test\"], date: [3, \"2024-01-01T...\"], count: [6, \"42\"] }]]"
```

---

## 6. CLIENT-SIDE HYDRATION SEQUENCE

1. **connectedCallback()** → Island attached to DOM
2. **Check children ready** → For HTML streaming support
3. **Load modules** → Component + framework renderer
4. **Deserialize props** → Type-tagged JSON → JS objects
5. **Collect slots** → From `<astro-slot>` or `<template data-astro-template>`
6. **Call hydrator** → Framework renderer hydrates component
7. **Top-down hydration** → Parent islands hydrate before children
8. **Dispatch event** → `astro:hydrate` for parent coordination

---

## 7. SLOT RENDERING PATTERN

### Server-Side (Component receives slots)
```typescript
type ComponentSlots = Record<string, (result: SSRResult) => RenderTemplateResult>;

// Usage in component:
const slot = slots.default;
const rendered = slot(result);  // Returns RenderTemplateResult
```

### Client-Side (Island receives slot HTML)
```html
<astro-island client="load">
    <!-- SSR-rendered content -->
    <astro-slot name="default">
        <p>Slot content</p>
    </astro-slot>
    
    <!-- Or as template (for unused slots) -->
    <template data-astro-template="sidebar">
        <aside>Sidebar</aside>
    </template>
</astro-island>
```

---

## 8. HEAD MANAGEMENT DEDUPLICATION

### Stable Props Key Algorithm
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

// Same props in different order = same key
stablePropsKey({ a: 1, b: 2 }) === stablePropsKey({ b: 2, a: 1 });  // true
```

---

## 9. MIDDLEWARE EXECUTION MODEL

### States
1. **Pass-through** → `next()` called, returns undefined
2. **Modify & forward** → `next()` called, returns Response
3. **Short-circuit** → No `next()`, returns Response
4. **Invalid** → No `next()`, no Response → Error

### Sequencing
```typescript
sequence(middleware1, middleware2, middleware3)

// Execution:
middleware1.next() → 
    middleware2.next() → 
        middleware3.next() → 
            route()
```

### Context Mutation
```csharp
// Middleware can rewrite URL
context.Url = new URL("/new-path");
context.Request = new Request("/new-path", oldRequest);
context.Params = { /* new params */ };
```

---

## 10. STYLE SCOPING MECHANISMS

### Three Strategies
1. **`:where()`** → `:where(.astro-HASH) { selector }`
2. **`.class`** → `.astro-HASH selector`
3. **`[attr]`** → `[data-astro-HASH] selector`

### Global Styles
```astro
<style is:global>
    /* No scoping applied */
</style>

<style>
    /* Scoped to component */
</style>
```

### URL Rewriting
```css
/* Input */
background: url('/images/bg.png');

/* Output (with base="/docs") */
background: url('/docs/images/bg.png');
```

---

## 11. STREAMING WITH ORDER PRESERVATION

### Algorithm
```
for each (html, expression) pair:
    write html
    result = renderChild(expression)
    
    if isPromise(result):
        // Buffer remaining expressions
        // Process them sequentially after promise resolves
        // Write HTML parts and flushed expressions in order
    else:
        continue
```

**Key Insight:** Once ANY async expression is encountered, ALL remaining expressions are buffered and processed sequentially to guarantee output order.

---

## 12. ASTRO GLOBAL PROPERTIES

### Available in .astro components
- `Astro.props` → Component props
- `Astro.url` → Current URL
- `Astro.request` → HTTP Request
- `Astro.response` → HTTP Response (mutable)
- `Astro.cookies` → Cookie utilities
- `Astro.locals` → Middleware-set data
- `Astro.params` → URL parameters
- `Astro.slots` → Slot utilities
- `Astro.site` → Site config
- `Astro.generator` → Astro version

### Available in endpoints/middleware
- Subset of above (no `slots`, `self`, `response`)
- `APIContext` interface

---

## 13. CONTENT ENTRY MODULE STRUCTURE

```typescript
export const id = 'blog/post-1.md';
export const collection = 'blog';
export const slug = 'post-1';
export const body = '# Title\nContent...';  // Raw markdown
export const data = { title: 'My Post', date: new Date() };  // Parsed frontmatter

export async function render() {
    return {
        html: '<h1>My Post</h1>...',
    };
}

export const Content = async (props) => {
    // AstroComponentFactory
    return renderTemplate`<h1>My Post</h1>...`;
};
```

---

## 14. RENDER INSTRUCTION TYPES

```typescript
// Directive (hydration script)
{ type: 'directive', hydration: { directive: 'load', componentUrl, ... } }

// Head injection
{ type: 'head' }

// Maybe head (conditional)
{ type: 'maybe-head' }

// Renderer-specific setup (React, Vue hydration)
{ type: 'renderer-hydration-script', rendererName: '@astrojs/react', render: () => '...' }

// Server island runtime
{ type: 'server-island-runtime' }

// Inline script
{ type: 'script', id: 'unique-id', content: '...' }
```

These bubble up through slot strings and are deduplicated at page render level.

---

## 15. CRITICAL FILES FOR C# PORT

| File | Purpose | Critical? |
|------|---------|-----------|
| `compile.ts` | Calls Rust compiler | NO (use separate Rust compiler) |
| `render-template.ts` | Template rendering logic | **YES** |
| `component.ts` | Component rendering dispatcher | **YES** |
| `hydration.ts` | Island generation | **YES** |
| `serialize.ts` | Prop type-tagging | **YES** |
| `astro-island.ts` | Client-side custom element | NO (use .js, or port to .ts) |
| `render.ts` | Page rendering orchestration | **YES** |
| `head.ts` | Head deduplication | **YES** |
| `slot.ts` | Slot rendering | **YES** |
| `instruction.ts` | Render instruction system | **YES** |
| `common.ts` | RenderDestination, RenderInstance | **YES** |
| `callMiddleware.ts` | Middleware execution | **YES** |
| `sequence.ts` | Middleware sequencing | **YES** |

---

## MINIMUM VIABLE C# IMPLEMENTATION

To get a basic C# Astro equivalent working:

1. **Compilation** → Call external Rust compiler via subprocess
2. **Template Rendering** → Mimic `RenderTemplateResult` class
3. **Component Factory** → Delegate pattern for component functions
4. **Streaming** → Use `IAsyncEnumerable<string>` or similar
5. **Islands** → Generate custom element HTML with serialized props
6. **Middleware** → Chain of responsibility pattern
7. **Head Management** → Set-based deduplication with stable keys

**Estimated complexity:** 2-3 weeks for senior .NET developer.
