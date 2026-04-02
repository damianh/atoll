# Atoll: .NET-Native Astro Framework — Implementation Plan

## TL;DR
> **Summary**: Build a .NET 10.0 reimplementation of Astro's core architecture — server-first rendering, islands architecture, file-based routing, content collections, and streaming SSR — using C#-native components instead of .astro template files. The framework supports both SSG and SSR, ships zero JS by default, and integrates as ASP.NET Core middleware or standalone CLI.
> **Estimated Effort**: XL

## Context

### Original Request
Build "Atoll" — a .NET-native framework inspired by Astro's architecture. Server-first rendering, islands for partial hydration, content collections, file-based routing. C# components (not .astro syntax). Both SSG and SSR. No Node.js dependency.

### Key Findings
- The repo currently contains only `.gitignore`, `LICENSE`, and four Astro analysis documents (~20k words of architecture reference)
- Astro's core is a streaming-first HTML renderer with async buffering for order preservation
- The component factory pattern `(SSRResult, props, slots) → RenderTemplateResult` maps cleanly to C# delegates/interfaces
- Astro's island system uses a `<astro-island>` Web Component with type-tagged prop serialization — directly portable
- Head deduplication uses a stable-props-key algorithm — trivial to reimplement
- Middleware is chain-of-responsibility with context mutation and rewrite support — maps to ASP.NET Core middleware
- Content collections are loaders + schema validation + render functions — maps to C# types + DataAnnotations/FluentValidation + Markdig

### Coding Standards (from skills)
- **All concrete classes must be `sealed` unless explicitly exempted**
- **Use PascalCase for test method names** (e.g., `ShouldRenderTemplateWithSlots`)
- **Use method overloads instead of optional/default parameters**
- **Types in `*.Internal.*` namespaces must be `internal`**
- **Fix all compilation warnings at source; no suppression without explicit approval**
- **Favor integration tests over unit tests**
- **Build in Release mode (`dotnet build -c Release`) for verification**

---

## Architecture

### Solution Structure

```
Atoll.sln
├── src/
│   ├── Atoll.Core/                    # Core rendering engine, component model, no HTTP dependency
│   │   ├── Components/                # Base types, IAtollComponent, AtollComponent, RenderContext
│   │   ├── Rendering/                 # RenderTemplate, RenderDestination, streaming, async buffering
│   │   ├── Islands/                   # Island generation, prop serialization, hydration scripts
│   │   ├── Head/                      # Head management, deduplication, injection
│   │   ├── Slots/                     # Slot system (lazy, function-based)
│   │   ├── Instructions/              # RenderInstruction types and processing
│   │   ├── Css/                       # Style scoping, hash generation
│   │   └── Internal/                  # Internal helpers (must be `internal`)
│   │
│   ├── Atoll.Routing/                 # File-based routing, route matching, params extraction
│   │   ├── FileSystem/                # Convention-based route discovery from src/pages/
│   │   ├── Matching/                  # Pattern matching, dynamic segments, rest params
│   │   └── Internal/
│   │
│   ├── Atoll.Content/                 # Content collections: Markdown, frontmatter, schema
│   │   ├── Collections/               # Collection definition, loader, query API
│   │   ├── Markdown/                  # Markdig integration, rendering pipeline
│   │   ├── Frontmatter/              # YAML parsing, schema validation
│   │   └── Internal/
│   │
│   ├── Atoll.Middleware/              # Atoll-level middleware (not ASP.NET Core middleware)
│   │   ├── Pipeline/                  # Chain of responsibility, sequence(), rewrite support
│   │   └── Internal/
│   │
│   ├── Atoll.Server/                  # ASP.NET Core integration (SSR runtime)
│   │   ├── Hosting/                   # Middleware registration, endpoint mapping
│   │   ├── DevServer/                 # Development server with HMR, file watching
│   │   ├── StaticFiles/               # Static asset serving
│   │   └── Internal/
│   │
│   ├── Atoll.Build/                   # Build pipeline for SSG
│   │   ├── Pipeline/                  # Asset pipeline: CSS, JS bundling, minification
│   │   ├── Ssg/                       # Static site generation orchestrator
│   │   ├── Tools/                     # Tailwind CLI, esbuild binary management
│   │   └── Internal/
│   │
│   ├── Atoll.Blazor/                  # Blazor WASM island support
│   │   ├── Islands/                   # Blazor component → island bridge
│   │   └── Internal/
│   │
│   └── Atoll.Cli/                     # CLI tool (`atoll dev`, `atoll build`, `atoll preview`)
│       └── Commands/                  # Dev, Build, Preview, New commands
│
├── tests/
│   ├── Atoll.Core.Tests/
│   ├── Atoll.Routing.Tests/
│   ├── Atoll.Content.Tests/
│   ├── Atoll.Middleware.Tests/
│   ├── Atoll.Server.Tests/
│   ├── Atoll.Build.Tests/
│   ├── Atoll.Blazor.Tests/
│   └── Atoll.Integration.Tests/       # End-to-end tests with real HTTP
│
├── samples/
│   ├── Atoll.Samples.Blog/            # Blog with content collections
│   ├── Atoll.Samples.Portfolio/       # Portfolio with islands
│   └── Atoll.Samples.Docs/           # Documentation site
│
├── tools/                             # Build tool binaries (esbuild, tailwindcss)
│   └── .gitkeep
│
├── Directory.Build.props              # Shared MSBuild properties
├── Directory.Build.targets            # Shared MSBuild targets
├── Directory.Packages.props           # Central package management
├── global.json                        # .NET SDK version pinning
├── .editorconfig                      # Code style enforcement
└── nuget.config                       # NuGet feed configuration
```

### Key Abstractions

```
┌─────────────────────────────────────────────────────────────────┐
│                        Atoll.Cli                                │
│   atoll dev | atoll build | atoll preview | atoll new           │
└──────────┬──────────────────────────┬───────────────────────────┘
           │                          │
           ▼                          ▼
┌─────────────────────┐    ┌─────────────────────┐
│   Atoll.Server      │    │   Atoll.Build       │
│   (ASP.NET Core     │    │   (SSG pipeline,    │
│    integration,     │    │    asset processing, │
│    dev server)      │    │    output writer)    │
└──────────┬──────────┘    └──────────┬───────────┘
           │                          │
           ▼                          ▼
┌─────────────────────────────────────────────────────────────────┐
│                     Atoll.Middleware                             │
│   Request pipeline, sequence(), rewrite support                 │
└──────────┬──────────────────────────────────────────────────────┘
           │
           ▼
┌──────────────────────┐  ┌──────────────────────┐
│   Atoll.Routing      │  │   Atoll.Content      │
│   File-based routes, │  │   Collections,       │
│   pattern matching   │  │   Markdown, schemas  │
└──────────┬───────────┘  └──────────┬───────────┘
           │                         │
           ▼                         ▼
┌─────────────────────────────────────────────────────────────────┐
│                       Atoll.Core                                │
│   Components, Rendering, Islands, Head, Slots, CSS, Streaming  │
└──────────┬──────────────────────────────────────────────────────┘
           │
           ▼
┌─────────────────────┐
│   Atoll.Blazor      │
│   (optional)        │
│   Blazor WASM       │
│   island adapter    │
└─────────────────────┘
```

---

## Component Model Recommendation

### Decision: **Hybrid — Class-based primary + Functional secondary**

**Primary: Class-based with `IAtollComponent` interface**
```csharp
public interface IAtollComponent
{
    Task RenderAsync(RenderContext context);
}

public abstract class AtollComponent : IAtollComponent
{
    public abstract Task RenderAsync(RenderContext context);

    // Helpers available to all components
    protected RenderFragment Html(string html);
    protected RenderFragment Html(FormattableString html);      // Auto-escaping
    protected RenderFragment HtmlRaw(string rawHtml);           // No escaping
    protected Task<RenderFragment> RenderSlotAsync(string name = "default");
    protected bool HasSlot(string name);
}
```

**Secondary: Functional delegate for lightweight components**
```csharp
public delegate Task<RenderFragment> ComponentDelegate(RenderContext context);

// Usage:
ComponentDelegate card = async (ctx) =>
{
    var title = ctx.Props.Get<string>("title");
    return ctx.Html($"""
        <div class="card">
            <h2>{title}</h2>
            {await ctx.RenderSlotAsync()}
        </div>
    """);
};
```

### Justification

1. **Class-based is idiomatic C#** — .NET developers expect classes with properties, not loose delegates. Razor Components, MVC Controllers, Minimal API handlers all use class/interface patterns.

2. **`[Parameter]` attributes enable tooling** — Source generators can create parameter binding, validation, and documentation from attributed properties.

3. **Functional delegates for simple cases** — Small wrapper components, layout fragments, and test stubs benefit from concise delegate syntax.

4. **Both compile to the same `RenderFragment`** — The rendering pipeline doesn't care how the fragment was produced. A `RenderFragment` is `Func<RenderDestination, ValueTask>` internally.

5. **No .atoll template files in v1** — We avoid the complexity of a custom parser/compiler. Raw string literals in C# 11+ (`$"""..."""`) give us interpolated multi-line HTML with compile-time type checking. A template format can be added later as a source generator.

### `RenderContext` — the Atoll equivalent of Astro's `Astro` global

```csharp
public sealed class RenderContext
{
    public IReadOnlyDictionary<string, object?> Props { get; }
    public SlotCollection Slots { get; }
    public HttpContext? HttpContext { get; }           // null during SSG
    public Uri Url { get; }
    public IReadOnlyDictionary<string, string> Params { get; }
    public IDictionary<string, object?> Locals { get; }   // Middleware-set data
    public ResponseInfo Response { get; }              // Status, headers (mutable)
    public ISiteConfig Site { get; }

    // Rendering helpers
    public RenderFragment Html(string html);
    public RenderFragment Html(FormattableString html);
    public RenderFragment HtmlRaw(string rawHtml);
    public Task<RenderFragment> RenderSlotAsync(string name = "default");
    public bool HasSlot(string name);
    public Task<RenderFragment> RenderComponentAsync<T>(object? props = null, SlotCollection? slots = null) where T : IAtollComponent;
    public RedirectResult Redirect(string path, int statusCode = 302);
    public RewriteResult Rewrite(string path);
}
```

### `RenderFragment` — the core rendering primitive

```csharp
// The fundamental unit of renderable content
// Equivalent to Astro's RenderTemplateResult
public readonly struct RenderFragment
{
    // A function that writes content to a destination, possibly asynchronously
    private readonly Func<IRenderDestination, ValueTask> _renderer;

    // Static HTML (fast path — no async, no expressions)
    public static RenderFragment FromHtml(string html);

    // Interpolated HTML (expressions that may be async)
    public static RenderFragment FromInterpolated(string[] htmlParts, RenderFragment[] expressions);

    // Render to destination (streaming)
    public ValueTask RenderAsync(IRenderDestination destination);

    // Render to string (buffered)
    public ValueTask<string> RenderToStringAsync();

    // Composition
    public static RenderFragment Concat(params RenderFragment[] fragments);
    public static readonly RenderFragment Empty;
}
```

---

## Technology Decisions

| Role | Library | Version | Justification |
|------|---------|---------|--------------|
| **Markdown** | Markdig | 0.37+ | CommonMark compliant, extensible, widely used |
| **HTML parsing** | AngleSharp | 1.2+ | W3C-spec DOM, CSS selector support |
| **YAML frontmatter** | YamlDotNet | 16+ | Mature, handles all YAML edge cases |
| **JS/CSS minification** | NUglify | 1.21+ | .NET-native, no Node dependency |
| **CSS processing** | Tailwind CSS standalone | 4.x | Binary download, no Node |
| **JS bundling** | esbuild standalone | 0.24+ | Binary download, no Node |
| **Schema validation** | DataAnnotations + custom | built-in | No extra dependency; FluentValidation optional |
| **Test framework** | xUnit | 2.9+ | Standard .NET test framework |
| **Test assertions** | Shouldly or FluentAssertions | latest | Readable assertion messages |
| **HTTP testing** | Microsoft.AspNetCore.Mvc.Testing | 9.0 | In-process test server |
| **File watching** | FileSystemWatcher + Polling | built-in | Cross-platform, no dependency |
| **WebSocket (HMR)** | ASP.NET Core WebSocket | 9.0 | Built-in, no SignalR needed for simple reload |
| **CLI framework** | System.CommandLine | 2.0+ | Microsoft-supported, async, testable |
| **Logging** | Microsoft.Extensions.Logging | 9.0 | Standard .NET logging abstractions |
| **Hashing (CSS scope)** | XxHash64 | System.IO.Hashing | Fast, deterministic, built-in |
| **JSON (props)** | System.Text.Json | 9.0 | Built-in, source-gen compatible |

---

## What We're NOT Building

### Explicit Scope Exclusions
- **NO React/Vue/Svelte/SolidJS island support** — Only vanilla JS, Web Components, and Blazor WASM
- **NO .astro template file format** — C# classes and raw string literals only (v1)
- **NO MDX equivalent** — Markdown with frontmatter; for interactive content, use islands
- **NO view transitions API** — Complex browser API; defer to v2
- **NO server islands** — Astro's newer feature; defer to v2
- **NO i18n routing** — Can be built as middleware by users
- **NO image optimization** — Complex; use external tools or defer to v2
- **NO database/ORM integration** — Users bring their own data layer
- **NO deployment adapters** — SSG output is static files; SSR runs as standard ASP.NET Core app
- **NO Actions API** — Astro 4.x feature; defer to v2
- **NO Sessions API** — Astro 5.x feature; defer to v2
- **NO dev toolbar** — Nice-to-have; defer to v2
- **NO source generators for components** — v1 uses runtime reflection; source generators in v2

---

## Objectives

### Core Objective
Deliver a usable, testable, documented .NET framework that implements Astro's core value proposition: server-first HTML rendering with islands architecture, zero JS by default, and streaming SSR.

### Deliverables
- [ ] Core rendering engine with streaming support
- [ ] C#-native component model (class-based + functional)
- [ ] Islands architecture (vanilla JS, Web Components, Blazor WASM)
- [ ] File-based routing with dynamic segments
- [ ] Content collections with Markdown + frontmatter + schema validation
- [ ] ASP.NET Core integration (SSR dev server)
- [ ] Static site generation pipeline
- [ ] CSS scoping + Tailwind CSS integration
- [ ] CLI tool (`atoll dev`, `atoll build`, `atoll preview`)
- [ ] Library mode (embeddable ASP.NET Core middleware)
- [ ] At least one complete sample site (blog)
- [ ] Integration test suite covering all core features

### Definition of Done
- [ ] `dotnet build -c Release` succeeds with zero warnings
- [ ] `dotnet test` passes all tests
- [ ] `atoll build` generates a complete static blog site from the sample
- [ ] `atoll dev` serves the sample site with live reload on file changes
- [ ] `atoll preview` serves the built static site
- [ ] The blog sample demonstrates: layouts, components, slots, islands, content collections, Tailwind CSS

### Guardrails (Must NOT)
- Must NOT require Node.js, npm, or any JavaScript runtime on the build machine
- Must NOT use `#pragma warning disable` without explicit approval
- Must NOT use optional/default parameters (use overloads)
- Must NOT declare non-abstract classes as non-sealed
- Must NOT commit secrets, credentials, or environment files

---

## TODOs

### Phase 0: Solution Scaffolding & CI
**Dependencies**: None
**Complexity**: 2-3 days
**Risk**: Low

- [x] 1. **Create solution structure**
  **What**: Create `Atoll.sln` with all projects, directory structure, shared build props
  **Files**:
    - `Atoll.sln`
    - `global.json` (pin .NET 9.0 SDK)
    - `Directory.Build.props` (shared settings: TFM, nullable, implicit usings, treat warnings as errors in Release)
    - `Directory.Build.targets`
    - `Directory.Packages.props` (central package management)
    - `.editorconfig` (code style: tabs/spaces, naming rules, severity levels)
    - `nuget.config`
    - `src/Atoll.Core/Atoll.Core.csproj`
    - `src/Atoll.Routing/Atoll.Routing.csproj`
    - `src/Atoll.Content/Atoll.Content.csproj`
    - `src/Atoll.Middleware/Atoll.Middleware.csproj`
    - `src/Atoll.Server/Atoll.Server.csproj`
    - `src/Atoll.Build/Atoll.Build.csproj`
    - `src/Atoll.Blazor/Atoll.Blazor.csproj`
    - `src/Atoll.Cli/Atoll.Cli.csproj`
    - `tests/Atoll.Core.Tests/Atoll.Core.Tests.csproj`
    - `tests/Atoll.Routing.Tests/Atoll.Routing.Tests.csproj`
    - `tests/Atoll.Content.Tests/Atoll.Content.Tests.csproj`
    - `tests/Atoll.Middleware.Tests/Atoll.Middleware.Tests.csproj`
    - `tests/Atoll.Server.Tests/Atoll.Server.Tests.csproj`
    - `tests/Atoll.Build.Tests/Atoll.Build.Tests.csproj`
    - `tests/Atoll.Integration.Tests/Atoll.Integration.Tests.csproj`
  **Acceptance**: `dotnet build -c Release` succeeds; `dotnet test` discovers 0 tests (no failures)

- [ ] 2. **Configure CI with GitHub Actions**
  **What**: Build + test on push/PR. Matrix: windows + ubuntu. Release mode build.
  **Files**:
    - `.github/workflows/ci.yml`
  **Acceptance**: Push triggers CI; build + test passes on both OS

- [ ] 3. **Add .editorconfig and analyzer configuration**
  **What**: Enforce coding standards via analyzers. Enable `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` in Release.
  **Files**:
    - `.editorconfig`
    - `Directory.Build.props` (analyzer packages, severity config)
  **Acceptance**: Code style violations produce build warnings (errors in Release)

---

### Phase 1: Core Rendering Engine
**Dependencies**: Phase 0
**Complexity**: 8-10 days
**Risk**: Medium — async buffering algorithm is the hardest part

- [x] 4. **Define core rendering primitives**
  **What**: `IRenderDestination`, `RenderFragment`, `HtmlString` (trusted HTML marker), `RenderChunk` discriminated union
  **Files**:
    - `src/Atoll.Core/Rendering/IRenderDestination.cs`
    - `src/Atoll.Core/Rendering/RenderFragment.cs`
    - `src/Atoll.Core/Rendering/HtmlString.cs`
    - `src/Atoll.Core/Rendering/RenderChunk.cs`
  **Acceptance**: Can create `RenderFragment` from static HTML and render to a `StringRenderDestination`

- [x] 5. **Implement interpolated template rendering with async buffering**
  **What**: Port Astro's `RenderTemplateResult` algorithm — interleaved static HTML + dynamic expressions, fast sync path, buffered async path for order preservation. Use `ValueTask` for hot-path efficiency.
  **Files**:
    - `src/Atoll.Core/Rendering/InterpolatedTemplate.cs` (htmlParts[] + expressions[] → streaming render)
    - `src/Atoll.Core/Rendering/BufferedRenderer.cs` (captures async output, flushes in order)
    - `src/Atoll.Core/Rendering/StringRenderDestination.cs` (for testing / renderToString)
    - `src/Atoll.Core/Rendering/StreamRenderDestination.cs` (for streaming to `Stream` / `PipeWriter`)
  **Acceptance**: Integration test proves correct output order with mix of sync and async expressions

- [x] 6. **Implement component model**
  **What**: `IAtollComponent` interface, `AtollComponent` abstract base, `ComponentDelegate` functional type, `RenderContext` with props/slots/helpers
  **Files**:
    - `src/Atoll.Core/Components/IAtollComponent.cs`
    - `src/Atoll.Core/Components/AtollComponent.cs`
    - `src/Atoll.Core/Components/ComponentDelegate.cs`
    - `src/Atoll.Core/Components/RenderContext.cs`
    - `src/Atoll.Core/Components/ParameterAttribute.cs`
    - `src/Atoll.Core/Components/ComponentRenderer.cs` (resolves + renders a component by type)
  **Acceptance**: Can render a `Card` component with title prop and default slot

- [x] 7. **Implement slot system**
  **What**: Lazy function-based slots — `SlotCollection` holds named slots as `Func<IRenderDestination, ValueTask>`. Default slot. Named slots. `HasSlot()` check. Fallback content.
  **Files**:
    - `src/Atoll.Core/Slots/SlotCollection.cs`
    - `src/Atoll.Core/Slots/SlotBuilder.cs` (fluent API for defining slots when composing components)
  **Acceptance**: Component with named slots renders correctly; unused slots don't execute; HasSlot returns false for missing slots

- [x] 8. **Implement render instruction system**
  **What**: `RenderInstruction` base type with subtypes: `HeadInstruction`, `MaybeHeadInstruction`, `DirectiveInstruction`, `ScriptInstruction`. Instructions bubble up through component tree and are deduplicated at page level.
  **Files**:
    - `src/Atoll.Core/Instructions/RenderInstruction.cs`
    - `src/Atoll.Core/Instructions/HeadInstruction.cs`
    - `src/Atoll.Core/Instructions/DirectiveInstruction.cs`
    - `src/Atoll.Core/Instructions/ScriptInstruction.cs`
    - `src/Atoll.Core/Instructions/InstructionProcessor.cs` (collects + deduplicates)
  **Acceptance**: Nested components emit instructions; page-level processor deduplicates and injects once

- [x] 9. **Implement head management**
  **What**: Head content collection (styles, scripts, links), stable-props-key deduplication algorithm, single injection point. `RenderAllHeadContent()` equivalent.
  **Files**:
    - `src/Atoll.Core/Head/HeadManager.cs` (collects styles/scripts/links during rendering)
    - `src/Atoll.Core/Head/HeadElement.cs` (tag + props + children)
    - `src/Atoll.Core/Head/HeadDeduplicator.cs` (stable key generation, Set-based dedup)
  **Acceptance**: Two components referencing same stylesheet → only one `<link>` in output; props order doesn't matter

- [x] 10. **Implement page rendering orchestrator**
  **What**: `PageRenderer` that takes a page component, renders it (with head injection, DOCTYPE auto-insertion), outputs to string or stream. Supports `renderToString` and `renderToStream`.
  **Files**:
    - `src/Atoll.Core/Rendering/PageRenderer.cs`
    - `src/Atoll.Core/Rendering/PageRenderResult.cs`
  **Acceptance**: Full page render produces `<!DOCTYPE html>...` with head content injected at correct location

- [x] 11. **Write tests for Phase 1**
  **What**: Integration tests for template rendering (sync, async, mixed), component rendering, slots, head dedup, page orchestration
  **Files**:
    - `tests/Atoll.Core.Tests/Rendering/InterpolatedTemplateTests.cs`
    - `tests/Atoll.Core.Tests/Rendering/AsyncBufferingTests.cs`
    - `tests/Atoll.Core.Tests/Components/ComponentRenderingTests.cs`
    - `tests/Atoll.Core.Tests/Slots/SlotRenderingTests.cs`
    - `tests/Atoll.Core.Tests/Head/HeadDeduplicationTests.cs`
    - `tests/Atoll.Core.Tests/Rendering/PageRendererTests.cs`
  **Acceptance**: All tests pass; `dotnet build -c Release` produces zero warnings

---

### Phase 2: File-Based Routing + ASP.NET Core Integration
**Dependencies**: Phase 1
**Complexity**: 6-8 days
**Risk**: Medium — route pattern matching edge cases

- [x] 12. **Implement route discovery from file system**
  **What**: Scan `src/pages/` directory, map files to URL patterns. Support: `index.cs` → `/`, `about.cs` → `/about`, `blog/[slug].cs` → `/blog/:slug`, `[...rest].cs` → catch-all. Convention: classes implementing `IAtollPage` in the pages directory.
  **Files**:
    - `src/Atoll.Routing/FileSystem/RouteDiscovery.cs`
    - `src/Atoll.Routing/FileSystem/RouteConventions.cs` (naming rules, segment types)
    - `src/Atoll.Routing/RouteEntry.cs` (pattern, component type, prerender flag)
  **Acceptance**: Given a directory tree, produces correct route table with patterns

- [x] 13. **Implement route pattern matching**
  **What**: Match incoming URL path to route pattern. Extract params from dynamic segments. Support: static segments, `[param]` dynamic, `[...rest]` catch-all, route priority ordering (most specific first).
  **Files**:
    - `src/Atoll.Routing/Matching/RoutePattern.cs` (parsed pattern representation)
    - `src/Atoll.Routing/Matching/RouteMatcher.cs` (URL → RouteEntry + params)
    - `src/Atoll.Routing/Matching/RouteComparer.cs` (priority ordering)
  **Acceptance**: `/blog/hello-world` matches `blog/[slug]` with `slug=hello-world`; `/docs/a/b/c` matches `docs/[...rest]` with `rest=a/b/c`

- [x] 14. **Implement `IAtollPage` interface and page types**
  **What**: Define page contract. Pages are components with additional routing metadata. Support `GetStaticPaths()` for SSG dynamic routes.
  **Files**:
    - `src/Atoll.Routing/IAtollPage.cs` (extends IAtollComponent with route metadata)
    - `src/Atoll.Routing/IStaticPathsProvider.cs` (GetStaticPaths for SSG)
    - `src/Atoll.Routing/StaticPath.cs` (params + props for a single static path)
  **Acceptance**: Page with `[slug]` implements `GetStaticPaths` returning list of slugs

- [x] 15. **Implement API endpoint support**
  **What**: Classes implementing `IAtollEndpoint` in `src/pages/` with `GET`, `POST`, etc. methods. Return `AtollResponse` (wraps HttpResponse semantics without tight coupling to ASP.NET Core).
  **Files**:
    - `src/Atoll.Routing/IAtollEndpoint.cs`
    - `src/Atoll.Routing/AtollResponse.cs`
    - `src/Atoll.Routing/EndpointContext.cs` (API context — subset of RenderContext)
  **Acceptance**: `api/posts.cs` with GET method returns JSON response

- [x] 16. **Implement ASP.NET Core hosting middleware**
  **What**: `UseAtoll()` extension method. Registers route table, handles incoming requests, dispatches to page/endpoint components, streams SSR responses.
  **Files**:
    - `src/Atoll.Server/Hosting/AtollMiddleware.cs`
    - `src/Atoll.Server/Hosting/AtollMiddlewareExtensions.cs` (`UseAtoll()`, `AddAtoll()`)
    - `src/Atoll.Server/Hosting/AtollOptions.cs` (configuration: pages directory, site URL, etc.)
    - `src/Atoll.Server/Hosting/AtollRequestHandler.cs` (routes request → page → response)
  **Acceptance**: ASP.NET Core app with `UseAtoll()` serves pages from `src/pages/`

- [x] 17. **Implement layout system**
  **What**: Components that wrap page content. Convention: `Layout` property on page, or `[Layout(typeof(BaseLayout))]` attribute. Layouts receive page content as default slot.
  **Files**:
    - `src/Atoll.Core/Components/LayoutAttribute.cs`
    - `src/Atoll.Core/Components/LayoutResolver.cs`
  **Acceptance**: Page with `[Layout(typeof(BaseLayout))]` renders inside layout; layout has `<slot />` for page content

- [ ] 18. **Write tests for Phase 2**
  **What**: Route discovery tests (file tree → route table), pattern matching tests, ASP.NET Core integration tests (HTTP request → HTML response)
  **Files**:
    - `tests/Atoll.Routing.Tests/RouteDiscoveryTests.cs`
    - `tests/Atoll.Routing.Tests/RouteMatchingTests.cs`
    - `tests/Atoll.Routing.Tests/RouteOrderingTests.cs`
    - `tests/Atoll.Server.Tests/MiddlewareIntegrationTests.cs`
  **Acceptance**: All tests pass; full HTTP round-trip renders correct page

---

### Phase 3: Islands Architecture (Vanilla JS + Web Components)
**Dependencies**: Phase 1
**Complexity**: 8-10 days
**Risk**: High — the hydration handshake is the most complex cross-boundary feature

- [ ] 19. **Implement client directive system**
  **What**: `[ClientLoad]`, `[ClientIdle]`, `[ClientVisible]`, `[ClientMedia("(max-width: 768px)")]` attributes. These mark a component for client-side hydration. The directive determines WHEN hydration occurs.
  **Files**:
    - `src/Atoll.Core/Islands/ClientDirective.cs` (enum: Load, Idle, Visible, Media)
    - `src/Atoll.Core/Islands/ClientLoadAttribute.cs`
    - `src/Atoll.Core/Islands/ClientIdleAttribute.cs`
    - `src/Atoll.Core/Islands/ClientVisibleAttribute.cs`
    - `src/Atoll.Core/Islands/ClientMediaAttribute.cs`
    - `src/Atoll.Core/Islands/DirectiveExtractor.cs` (reads attributes from component type)
  **Acceptance**: Component with `[ClientLoad]` is detected as requiring hydration

- [ ] 20. **Implement prop serialization for islands**
  **What**: Type-tagged serialization format compatible with Astro's `[type, value]` tuples. Support: primitives, arrays, objects, DateTime, Uri, nested objects. Cycle detection.
  **Files**:
    - `src/Atoll.Core/Islands/PropSerializer.cs` (C# object → type-tagged JSON)
    - `src/Atoll.Core/Islands/PropType.cs` (enum matching Astro's PROP_TYPE)
    - `src/Atoll.Core/Islands/CycleDetector.cs` (reference tracking during serialization)
  **Acceptance**: Round-trip test: serialize C# object → JSON → deserialize in test JS → matches original values

- [ ] 21. **Implement `<atoll-island>` HTML generation (server-side)**
  **What**: During SSR, island components render their static HTML wrapped in `<atoll-island>` custom element with metadata attributes: `component-url`, `client`, `props`, `ssr`, `opts`.
  **Files**:
    - `src/Atoll.Core/Islands/IslandRenderer.cs` (wraps component SSR output in island element)
    - `src/Atoll.Core/Islands/IslandMetadata.cs` (component URL, export, directive, props)
    - `src/Atoll.Core/Islands/HydrationScriptGenerator.cs` (generates the one-time hydration bootstrap script)
  **Acceptance**: Island component renders `<atoll-island client="load" props="..." ssr>...SSR HTML...</atoll-island>`

- [ ] 22. **Implement `atoll-island` client-side Web Component (JavaScript)**
  **What**: Port Astro's `astro-island.ts` → `atoll-island.js`. Handles: `connectedCallback`, prop deserialization, slot collection, module loading, hydration handshake, top-down parent coordination, `atoll:hydrate` event.
  **Files**:
    - `src/Atoll.Core/Islands/Assets/atoll-island.js` (embedded resource)
    - `src/Atoll.Core/Islands/Assets/atoll-directives.js` (client:load, client:idle, client:visible, client:media implementations)
  **Acceptance**: Browser test: island with `client:load` hydrates correctly; island with `client:visible` only hydrates when scrolled into view

- [ ] 23. **Implement vanilla JS island support**
  **What**: Allow islands that are plain HTML + vanilla JavaScript. Component provides SSR HTML + a JS module URL. The JS module exports an `init(element, props)` function called during hydration.
  **Files**:
    - `src/Atoll.Core/Islands/VanillaJsIsland.cs` (base class for vanilla JS islands)
    - `src/Atoll.Core/Islands/IClientComponent.cs` (interface: SSR render + client module URL)
  **Acceptance**: Counter component with `client:load` renders server HTML, then JS makes it interactive

- [ ] 24. **Implement Web Component island support**
  **What**: Allow `<custom-element>` Web Components as islands. SSR renders the light DOM / shadow DOM content. Client-side, the custom element definition is loaded and upgraded.
  **Files**:
    - `src/Atoll.Core/Islands/WebComponentIsland.cs`
    - `src/Atoll.Core/Islands/WebComponentAdapter.cs` (bridges Atoll island protocol ↔ custom element lifecycle)
  **Acceptance**: Web Component island hydrates correctly; custom element upgrade occurs after `client:load`

- [ ] 25. **Implement hydration script deduplication**
  **What**: Multiple islands of the same type should not duplicate the bootstrap script. Use render instructions to track which directive handlers have been emitted.
  **Files**:
    - `src/Atoll.Core/Islands/HydrationTracker.cs` (tracks emitted scripts per-page)
  **Acceptance**: Page with 5 `client:load` islands → only one copy of the load directive script

- [ ] 26. **Write tests for Phase 3**
  **What**: Prop serialization round-trip tests, island HTML generation tests, hydration script deduplication tests. Browser-based tests deferred to Phase 9.
  **Files**:
    - `tests/Atoll.Core.Tests/Islands/PropSerializerTests.cs`
    - `tests/Atoll.Core.Tests/Islands/IslandRendererTests.cs`
    - `tests/Atoll.Core.Tests/Islands/HydrationDeduplicationTests.cs`
  **Acceptance**: All tests pass; generated HTML contains correct `<atoll-island>` markup

---

### Phase 4: Content Collections
**Dependencies**: Phase 1
**Complexity**: 5-7 days
**Risk**: Low-Medium — Markdig is well-documented; YAML parsing is straightforward

- [ ] 27. **Implement content collection definition**
  **What**: Collections are defined by directory convention (`src/content/{collection}/`) and a C# schema class. Schema class has properties with DataAnnotations for validation.
  **Files**:
    - `src/Atoll.Content/Collections/ContentCollection.cs` (collection definition: name, schema type, directory)
    - `src/Atoll.Content/Collections/CollectionConfig.cs` (configuration: base directory, collections list)
    - `src/Atoll.Content/Collections/ContentEntry.cs` (single entry: id, slug, body, data, collection)
  **Acceptance**: Can define a "blog" collection with a `BlogPost` schema class

- [ ] 28. **Implement frontmatter parsing**
  **What**: Extract YAML frontmatter from Markdown files (between `---` delimiters). Parse YAML to dictionary, then bind to schema class. Validate using DataAnnotations.
  **Files**:
    - `src/Atoll.Content/Frontmatter/FrontmatterParser.cs` (extracts YAML + body from markdown)
    - `src/Atoll.Content/Frontmatter/FrontmatterBinder.cs` (YAML dict → C# schema object)
    - `src/Atoll.Content/Frontmatter/FrontmatterValidator.cs` (DataAnnotations validation)
  **Acceptance**: Markdown with frontmatter → parsed schema object with validated properties

- [ ] 29. **Implement Markdown rendering**
  **What**: Markdig pipeline: CommonMark + extensions (tables, autolinks, task lists, syntax highlighting). Render Markdown body to HTML. Support custom Markdig extensions.
  **Files**:
    - `src/Atoll.Content/Markdown/MarkdownRenderer.cs` (configures Markdig pipeline, renders to HTML)
    - `src/Atoll.Content/Markdown/MarkdownOptions.cs` (extension toggles, syntax highlight theme)
  **Acceptance**: Markdown with tables, code blocks, and task lists renders to correct HTML

- [ ] 30. **Implement collection query API**
  **What**: `GetCollection<T>("blog")` → all entries. `GetEntry<T>("blog", "my-post")` → single entry. `Render(entry)` → HTML + headings. Type-safe access to frontmatter data.
  **Files**:
    - `src/Atoll.Content/Collections/CollectionLoader.cs` (scans directory, loads entries)
    - `src/Atoll.Content/Collections/CollectionQuery.cs` (GetCollection, GetEntry, query/filter)
    - `src/Atoll.Content/Collections/RenderedContent.cs` (HTML + headings + metadata)
  **Acceptance**: `GetCollection<BlogPost>("blog")` returns typed entries; `Render()` returns HTML

- [ ] 31. **Implement content as Atoll component**
  **What**: Rendered Markdown content can be used as a `RenderFragment` inside Atoll components. The content HTML is injected into a layout/page.
  **Files**:
    - `src/Atoll.Content/Collections/ContentComponent.cs` (wraps rendered content as IAtollComponent)
  **Acceptance**: Blog post page renders Markdown content inside a layout with header/footer

- [ ] 32. **Write tests for Phase 4**
  **What**: Frontmatter parsing, schema validation (valid + invalid), Markdown rendering, collection loading, query API
  **Files**:
    - `tests/Atoll.Content.Tests/Frontmatter/FrontmatterParserTests.cs`
    - `tests/Atoll.Content.Tests/Frontmatter/FrontmatterValidationTests.cs`
    - `tests/Atoll.Content.Tests/Markdown/MarkdownRenderingTests.cs`
    - `tests/Atoll.Content.Tests/Collections/CollectionQueryTests.cs`
    - Test fixture: `tests/Atoll.Content.Tests/TestContent/blog/` (sample .md files)
  **Acceptance**: All tests pass; round-trip: .md file → frontmatter + HTML

---

### Phase 5: Blazor WASM Islands
**Dependencies**: Phase 3 (island infrastructure)
**Complexity**: 5-7 days
**Risk**: High — Blazor WASM loading/initialization is complex; interop with non-Blazor page is tricky

- [ ] 33. **Research Blazor WASM standalone component loading**
  **What**: Investigate how to load a Blazor WASM component into a non-Blazor page. Key questions: Can we initialize Blazor runtime per-island? Can we use `Blazor.rootComponents.add()`? What's the minimal WASM payload? Document findings.
  **Files**:
    - `.weave/research/blazor-wasm-islands.md` (findings document)
  **Acceptance**: Clear understanding of Blazor WASM initialization API and constraints

- [ ] 34. **Implement Blazor island adapter**
  **What**: Bridge between Atoll island protocol and Blazor WASM. Server-side: render Blazor component to static HTML (prerendering). Client-side: boot Blazor runtime, attach to island element, hydrate component.
  **Files**:
    - `src/Atoll.Blazor/Islands/BlazorIslandAdapter.cs`
    - `src/Atoll.Blazor/Islands/BlazorPrerenderService.cs` (server-side Blazor prerendering)
    - `src/Atoll.Blazor/Islands/BlazorHydrationScript.cs` (client-side Blazor bootstrap)
  **Acceptance**: Blazor Counter component renders as island; button click works after hydration

- [ ] 35. **Implement Blazor WASM runtime management**
  **What**: Manage Blazor WASM runtime lifecycle. Only load once per page even with multiple islands. Handle Blazor-specific assets (framework files, DLLs).
  **Files**:
    - `src/Atoll.Blazor/Runtime/BlazorRuntimeLoader.cs`
    - `src/Atoll.Blazor/Runtime/BlazorAssetManager.cs`
  **Acceptance**: Page with 3 Blazor islands loads Blazor runtime once; all islands hydrate correctly

- [ ] 36. **Write tests for Phase 5**
  **What**: Blazor island rendering tests (server-side prerender), adapter integration tests
  **Files**:
    - `tests/Atoll.Blazor.Tests/BlazorIslandAdapterTests.cs`
    - `tests/Atoll.Blazor.Tests/BlazorPrerenderTests.cs`
  **Acceptance**: All tests pass

---

### Phase 6: CSS Processing
**Dependencies**: Phase 1 (rendering), Phase 2 (ASP.NET Core integration)
**Complexity**: 5-7 days
**Risk**: Medium — CSS scoping hash generation must be deterministic; Tailwind CLI integration is OS-dependent

- [ ] 37. **Implement style scoping**
  **What**: Hash-based CSS class scoping using `:where(.atoll-HASH)` strategy. Hash derived from component type full name (deterministic). Components declare styles via `[Styles]` attribute or `GetStyles()` method. AngleSharp for CSS parsing/transformation.
  **Files**:
    - `src/Atoll.Core/Css/StyleScoper.cs` (applies `:where(.atoll-HASH)` wrapping)
    - `src/Atoll.Core/Css/ScopeHashGenerator.cs` (XxHash64 from component type name)
    - `src/Atoll.Core/Css/StylesAttribute.cs` (inline CSS declaration on component)
    - `src/Atoll.Core/Css/GlobalStyleAttribute.cs` (marks CSS as unscoped)
  **Acceptance**: Two components with same `.container` class → different scoped selectors; `[GlobalStyle]` CSS is unscoped

- [ ] 38. **Implement CSS aggregation and minification**
  **What**: Collect all component CSS during rendering, deduplicate, minify with NUglify, inject into head. Support external CSS file references.
  **Files**:
    - `src/Atoll.Core/Css/CssAggregator.cs` (collects CSS from rendered component tree)
    - `src/Atoll.Core/Css/CssMinifier.cs` (NUglify wrapper)
    - `src/Atoll.Core/Css/CssInjector.cs` (outputs `<style>` or `<link>` in head)
  **Acceptance**: Page with 10 components → single minified `<style>` block in head (or external CSS file)

- [ ] 39. **Implement Tailwind CSS integration**
  **What**: Download Tailwind CSS standalone binary for current OS/arch. Run Tailwind CLI during build to scan component files for classes and generate utility CSS. Embed output in build artifacts.
  **Files**:
    - `src/Atoll.Build/Tools/TailwindRunner.cs` (manages Tailwind CLI binary)
    - `src/Atoll.Build/Tools/ToolDownloader.cs` (downloads platform-specific binaries)
    - `src/Atoll.Build/Tools/TailwindConfig.cs` (generates tailwind.config.js equivalent)
  **Acceptance**: Component using `class="flex items-center"` → Tailwind CSS output contains only used utilities

- [ ] 40. **Implement CSS URL rewriting**
  **What**: Rewrite relative URLs in CSS (`url(...)`) to account for base path configuration. Port Astro's `rewriteCssUrls` logic.
  **Files**:
    - `src/Atoll.Core/Css/CssUrlRewriter.cs`
  **Acceptance**: `url('/images/bg.png')` with base `/docs` → `url('/docs/images/bg.png')`

- [ ] 41. **Write tests for Phase 6**
  **What**: Style scoping tests, hash determinism, CSS minification, URL rewriting, Tailwind integration
  **Files**:
    - `tests/Atoll.Core.Tests/Css/StyleScopingTests.cs`
    - `tests/Atoll.Core.Tests/Css/CssUrlRewriterTests.cs`
    - `tests/Atoll.Build.Tests/TailwindRunnerTests.cs`
  **Acceptance**: All tests pass; scoped CSS is deterministic across runs

---

### Phase 7: Atoll Middleware Pipeline
**Dependencies**: Phase 2 (routing)
**Complexity**: 3-4 days
**Risk**: Low — well-understood pattern; Astro's middleware is simpler than ASP.NET Core's

- [ ] 42. **Implement Atoll middleware types**
  **What**: `AtollMiddleware` delegate type, `AtollMiddlewareContext` (extends RenderContext with `next()` support), `DefineMiddleware()` helper. This is Atoll's own middleware, not ASP.NET Core's — it runs within the Atoll pipeline after ASP.NET Core routing.
  **Files**:
    - `src/Atoll.Middleware/AtollMiddleware.cs` (delegate: (context, next) → Task)
    - `src/Atoll.Middleware/AtollMiddlewareContext.cs`
    - `src/Atoll.Middleware/MiddlewareNext.cs` (delegate: returns Task<AtollResponse>)
  **Acceptance**: Middleware can short-circuit, pass-through, or modify response

- [ ] 43. **Implement middleware sequencing**
  **What**: `Sequence()` function that chains multiple middleware handlers. Support rewrite within chain (updates context URL/params). Port Astro's `sequence.ts` logic.
  **Files**:
    - `src/Atoll.Middleware/Pipeline/MiddlewareSequencer.cs`
    - `src/Atoll.Middleware/Pipeline/MiddlewareRunner.cs` (executes sequence against a request)
  **Acceptance**: `Sequence(logging, auth, cors)` executes in order; auth can short-circuit

- [ ] 44. **Implement rewrite support**
  **What**: Middleware can call `next(rewriteTo: "/new-path")`. The pipeline re-resolves the route and updates context (URL, params, route data). Validate SSR→prerendered rewrites.
  **Files**:
    - `src/Atoll.Middleware/Pipeline/RewriteHandler.cs`
  **Acceptance**: Middleware rewrite from `/old` to `/new` renders the `/new` page

- [ ] 45. **Write tests for Phase 7**
  **What**: Middleware execution order, short-circuit, pass-through, rewrite, sequence composition
  **Files**:
    - `tests/Atoll.Middleware.Tests/MiddlewareSequencerTests.cs`
    - `tests/Atoll.Middleware.Tests/MiddlewareRewriteTests.cs`
  **Acceptance**: All tests pass

---

### Phase 8: Static Site Generation (Build Pipeline)
**Dependencies**: Phase 1-4, Phase 6
**Complexity**: 7-9 days
**Risk**: Medium — must handle dynamic routes via GetStaticPaths, asset fingerprinting, output directory management

- [ ] 46. **Implement SSG orchestrator**
  **What**: Discovers all routes (static + dynamic via `GetStaticPaths()`), renders each to HTML file, writes to output directory (`dist/`). Parallel rendering with configurable concurrency.
  **Files**:
    - `src/Atoll.Build/Ssg/StaticSiteGenerator.cs` (main orchestrator)
    - `src/Atoll.Build/Ssg/SsgOptions.cs` (output dir, base URL, concurrency)
    - `src/Atoll.Build/Ssg/RouteEnumerator.cs` (expands dynamic routes via GetStaticPaths)
    - `src/Atoll.Build/Ssg/OutputWriter.cs` (writes HTML files to correct directory structure)
  **Acceptance**: `atoll build` generates `dist/index.html`, `dist/about/index.html`, `dist/blog/my-post/index.html`

- [ ] 47. **Implement asset pipeline**
  **What**: Process CSS (scoping + Tailwind + minification), process JS (bundling via esbuild + minification), copy static assets. Content-hash fingerprinting for cache busting.
  **Files**:
    - `src/Atoll.Build/Pipeline/AssetPipeline.cs` (orchestrates CSS + JS + static processing)
    - `src/Atoll.Build/Pipeline/CssProcessor.cs` (aggregation → Tailwind → scoping → minification)
    - `src/Atoll.Build/Pipeline/JsProcessor.cs` (esbuild bundling → NUglify minification)
    - `src/Atoll.Build/Pipeline/StaticAssetCopier.cs` (copies `public/` to `dist/`)
    - `src/Atoll.Build/Pipeline/AssetFingerprinter.cs` (content hash → filename)
  **Acceptance**: Built output has fingerprinted CSS/JS files; HTML references use hashed URLs

- [ ] 48. **Implement esbuild binary management**
  **What**: Download esbuild standalone binary for current OS/arch. Run esbuild for JS bundling (island client scripts, component JS).
  **Files**:
    - `src/Atoll.Build/Tools/EsbuildRunner.cs`
  **Acceptance**: Island JS modules are bundled into single files with tree-shaking

- [ ] 49. **Implement HTML post-processing**
  **What**: After rendering, use AngleSharp to post-process HTML: inject asset references with fingerprinted URLs, ensure all relative URLs respect base path, compress HTML if configured.
  **Files**:
    - `src/Atoll.Build/Pipeline/HtmlPostProcessor.cs`
  **Acceptance**: All `<link>`, `<script>`, `<img>` references in output HTML point to correct fingerprinted assets

- [ ] 50. **Implement build manifest**
  **What**: Generate build manifest (JSON) listing all pages, assets, routes. Useful for sitemaps, preloading, and debugging.
  **Files**:
    - `src/Atoll.Build/Ssg/BuildManifest.cs`
    - `src/Atoll.Build/Ssg/BuildManifestWriter.cs`
  **Acceptance**: `dist/.atoll/manifest.json` contains page list and asset map

- [ ] 51. **Write tests for Phase 8**
  **What**: SSG end-to-end test (sample pages → output files), asset pipeline tests, fingerprinting tests
  **Files**:
    - `tests/Atoll.Build.Tests/Ssg/StaticSiteGeneratorTests.cs`
    - `tests/Atoll.Build.Tests/Pipeline/AssetPipelineTests.cs`
    - `tests/Atoll.Build.Tests/Pipeline/AssetFingerprintTests.cs`
  **Acceptance**: All tests pass; generated site is a valid static site

---

### Phase 9: CLI Tool + Library Mode
**Dependencies**: Phase 2 (server), Phase 8 (build)
**Complexity**: 4-5 days
**Risk**: Low — System.CommandLine is well-documented

- [ ] 52. **Implement `atoll dev` command**
  **What**: Starts ASP.NET Core dev server with file watching. On file change: re-discover routes, re-render affected pages, notify browser via WebSocket. Hot reload for CSS changes (no full page reload).
  **Files**:
    - `src/Atoll.Cli/Commands/DevCommand.cs`
    - `src/Atoll.Server/DevServer/FileWatcher.cs` (watches src/ for changes)
    - `src/Atoll.Server/DevServer/DevServerOptions.cs`
  **Acceptance**: `atoll dev` starts server; editing a component triggers browser refresh

- [ ] 53. **Implement `atoll build` command**
  **What**: Runs SSG pipeline. Discovers routes, renders pages, processes assets, writes output.
  **Files**:
    - `src/Atoll.Cli/Commands/BuildCommand.cs`
  **Acceptance**: `atoll build` produces `dist/` with complete static site

- [ ] 54. **Implement `atoll preview` command**
  **What**: Serves the built `dist/` directory as a static file server. Uses ASP.NET Core static file middleware.
  **Files**:
    - `src/Atoll.Cli/Commands/PreviewCommand.cs`
  **Acceptance**: `atoll preview` serves files from `dist/`; navigation works correctly

- [ ] 55. **Implement `atoll new` command**
  **What**: Scaffolds a new Atoll project from a template. Creates directory structure, sample page, layout, and config.
  **Files**:
    - `src/Atoll.Cli/Commands/NewCommand.cs`
    - `src/Atoll.Cli/Templates/` (embedded template files)
  **Acceptance**: `atoll new my-site` creates runnable project; `cd my-site && atoll dev` works

- [ ] 56. **Implement library mode (embeddable middleware)**
  **What**: `AddAtoll()`/`UseAtoll()` extension methods allow embedding Atoll in an existing ASP.NET Core app. Coexists with existing controllers/endpoints. Configurable base path.
  **Files**:
    - `src/Atoll.Server/Hosting/AtollServiceCollectionExtensions.cs`
    - `src/Atoll.Server/Hosting/AtollApplicationBuilderExtensions.cs`
  **Acceptance**: Existing ASP.NET Core app adds `UseAtoll()` → Atoll pages work alongside existing API endpoints

- [ ] 57. **Implement CLI entry point and shared configuration**
  **What**: Main CLI entry point using System.CommandLine. Shared `atoll.json` project configuration file.
  **Files**:
    - `src/Atoll.Cli/Program.cs`
    - `src/Atoll.Core/Configuration/AtollConfig.cs` (project configuration model)
    - `src/Atoll.Core/Configuration/AtollConfigLoader.cs` (reads atoll.json)
  **Acceptance**: `atoll --help` shows available commands; `atoll.json` configures site URL, output dir, etc.

- [ ] 58. **Write tests for Phase 9**
  **What**: CLI command parsing tests, library mode integration tests
  **Files**:
    - `tests/Atoll.Integration.Tests/CliCommandTests.cs`
    - `tests/Atoll.Integration.Tests/LibraryModeTests.cs`
  **Acceptance**: All tests pass

---

### Phase 10: Developer Experience
**Dependencies**: Phase 9 (CLI + dev server)
**Complexity**: 5-7 days
**Risk**: Medium — WebSocket-based HMR requires careful coordination

- [ ] 59. **Implement live reload via WebSocket**
  **What**: Dev server injects a small JS snippet into pages that opens a WebSocket. On file change, server sends `reload` message. Browser refreshes. CSS-only changes trigger style-only reload (no full page refresh).
  **Files**:
    - `src/Atoll.Server/DevServer/LiveReloadMiddleware.cs`
    - `src/Atoll.Server/DevServer/LiveReloadWebSocketHandler.cs`
    - `src/Atoll.Server/DevServer/Assets/live-reload.js` (embedded resource)
  **Acceptance**: Edit component → browser auto-refreshes within 500ms

- [ ] 60. **Implement error overlay**
  **What**: When a component throws during dev rendering, show a styled error page with: exception type, message, stack trace, source file path + line number. Replaces the white error screen.
  **Files**:
    - `src/Atoll.Server/DevServer/ErrorOverlay.cs` (renders error HTML)
    - `src/Atoll.Server/DevServer/Assets/error-overlay.css` (embedded resource)
  **Acceptance**: Throw in component → browser shows styled error with file path and line number

- [ ] 61. **Implement build diagnostics**
  **What**: During `atoll build`, report: pages rendered, time per page, total assets, total output size, warnings (unused content, broken links). Colorized terminal output.
  **Files**:
    - `src/Atoll.Build/Diagnostics/BuildReporter.cs`
    - `src/Atoll.Build/Diagnostics/BuildDiagnostic.cs`
  **Acceptance**: `atoll build` output shows page count, asset sizes, timing, and any warnings

- [ ] 62. **Implement `atoll.json` IntelliSense schema**
  **What**: JSON Schema for `atoll.json` configuration file. Enables IntelliSense in VS Code and Visual Studio.
  **Files**:
    - `schemas/atoll.schema.json`
  **Acceptance**: Opening `atoll.json` in VS Code shows autocomplete for configuration properties

- [ ] 63. **Write tests for Phase 10**
  **What**: Error overlay rendering tests, build diagnostics tests
  **Files**:
    - `tests/Atoll.Server.Tests/DevServer/ErrorOverlayTests.cs`
    - `tests/Atoll.Build.Tests/Diagnostics/BuildReporterTests.cs`
  **Acceptance**: All tests pass

---

### Phase 11: Polish, Samples, Documentation
**Dependencies**: All previous phases
**Complexity**: 5-7 days
**Risk**: Low

- [ ] 64. **Build blog sample site**
  **What**: Complete blog with: layout, header/footer components, blog post list, individual post pages (from content collections), tag filtering, islands (search, theme toggle), Tailwind CSS styling.
  **Files**:
    - `samples/Atoll.Samples.Blog/` (complete project)
  **Acceptance**: `cd samples/Atoll.Samples.Blog && atoll dev` → working blog; `atoll build` → static site

- [ ] 65. **Build portfolio sample site**
  **What**: Portfolio with: hero section, project cards, contact form (island), image gallery (client:visible island), responsive layout with Tailwind.
  **Files**:
    - `samples/Atoll.Samples.Portfolio/` (complete project)
  **Acceptance**: Demonstrates islands with different client directives

- [ ] 66. **Write end-to-end integration tests**
  **What**: Full round-trip tests using the blog sample: build → validate output → serve → HTTP requests → verify HTML. Content collection → rendered page tests.
  **Files**:
    - `tests/Atoll.Integration.Tests/BlogSampleTests.cs`
    - `tests/Atoll.Integration.Tests/SsgOutputTests.cs`
  **Acceptance**: All tests pass; blog sample builds and serves correctly

- [ ] 67. **Final code quality pass**
  **What**: Review all code for: naming consistency, XML doc comments on public API, sealed classes, no optional params, no warnings in Release build. Run analyzers.
  **Files**:
    - `Directory.Build.props` (analyzer configuration review)
    - `src/**/*.cs` (code quality sweep across all source projects)
  **Acceptance**: `dotnet build -c Release` → zero warnings; all public types have XML docs

- [ ] 68. **Update README with getting-started guide**
  **What**: Project README with: what Atoll is, quickstart, architecture overview, comparison with Astro, how to create components/pages/islands/content.
  **Files**:
    - `README.md`
  **Acceptance**: New developer can follow README to create and run a basic Atoll site

---

## Open Questions

### Architecture
1. **Component discovery mechanism**: Runtime reflection (Assembly scanning) vs. explicit registration vs. source generator? v1 recommendation: explicit registration in `Program.cs` via `builder.AddAtollComponents(typeof(Program).Assembly)`. Source generator in v2.

2. **Hot reload granularity**: Full page reload on any change, or can we do partial component re-rendering? v1 recommendation: full page reload via WebSocket. Partial re-rendering is complex and can be deferred.

3. **Streaming SSR backpressure**: Should we use `PipeWriter` for backpressure-aware streaming? v1 recommendation: yes, use `System.IO.Pipelines` for production SSR. Fall back to `Stream` for simplicity in SSG.

4. **Thread safety of RenderContext**: Is `RenderContext` per-request (thread-safe by isolation) or shared? Recommendation: per-request, allocated from a pool.

### Component Model
5. **Props binding**: How are props passed between components? Dictionary-based (flexible, like Astro) or strongly-typed (C#-native, like Blazor)? Recommendation: strongly-typed via `[Parameter]` properties for class-based components; dictionary for functional delegates.

6. **Conditional rendering syntax**: Astro uses `{condition && <div>...</div>}`. In C#, we'd use `if` blocks inside `Html()`. Is this ergonomic enough? Recommendation: yes, raw string literals with ternary operators are natural in C#:
   ```csharp
   Html($"""
       {(showSidebar ? "<aside>Sidebar</aside>" : "")}
   """)
   ```

7. **Fragment/children syntax**: How does a parent pass children to a component? Recommendation: via `SlotBuilder`:
   ```csharp
   await ctx.RenderComponentAsync<Card>(
       props: new { Title = "My Card" },
       slots: new SlotBuilder()
           .Default(Html("<p>Card content</p>"))
           .Named("footer", Html("<small>Footer</small>"))
           .Build()
   );
   ```

### Islands
8. **Blazor WASM payload size**: A single Blazor island downloads the entire .NET runtime (~2MB gzipped). Is this acceptable? For a "zero JS by default" framework, this is a tension. Recommendation: document the tradeoff clearly; Blazor islands are opt-in for apps that already use Blazor.

9. **Island JS module format**: ES modules (import/export) or IIFE? Recommendation: ES modules. They're natively supported in all modern browsers and enable tree-shaking.

10. **Cross-island communication**: How do islands communicate? Recommendation: v1 uses DOM events (`CustomEvent`). v2 could add a lightweight pub/sub.

### Build
11. **Incremental builds**: Should `atoll build` support incremental rebuilds (only changed pages)? Recommendation: defer to v2. v1 does full rebuild. Track file hashes for v2 incremental support.

12. **Platform-specific binary management**: How to handle esbuild/Tailwind binaries across OS/arch? Recommendation: download on first use, cache in `~/.atoll/tools/`. Verify checksums.

### Content
13. **Content collection loaders**: Astro supports custom loaders (API, CMS, etc.). Should Atoll? Recommendation: v1 supports file-system only. Define `IContentLoader` interface for v2 extensibility.

14. **Syntax highlighting**: Which library for code block highlighting? Recommendation: Use Markdig's syntax highlighting extension with a bundled highlight.js CSS theme, or integrate with Shiki (would require esbuild/Node — conflicts with "no Node" goal). Alternative: server-side highlighting via a .NET library like ColorCode.

---

## Risk Register

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| Blazor WASM island integration is harder than expected | High | Medium | Phase 5 starts with research spike; can defer to v2 if blocked |
| Tailwind/esbuild binary download fails in CI or restricted networks | Medium | Medium | Support vendoring binaries in `tools/` directory; fallback to NUglify-only |
| Async buffering algorithm has subtle ordering bugs | Medium | High | Port Astro's exact algorithm; extensive property-based testing |
| CSS scoping with AngleSharp is too slow for large sites | Low | Medium | Profile early; fallback to regex-based scoping if needed |
| Component ergonomics with raw string literals feel clunky | Medium | Medium | User testing with sample sites; iterate on API before v1 release |
| File watching is unreliable on some OS/filesystem combos | Medium | Low | Use polling fallback; configurable in `atoll.json` |

---

## Verification

- [ ] `dotnet build -c Release` succeeds with zero warnings across entire solution
- [ ] `dotnet test` passes all tests across all test projects
- [ ] Blog sample site builds successfully with `atoll build`
- [ ] Blog sample site serves correctly with `atoll dev`
- [ ] Blog sample site serves correctly with `atoll preview`
- [ ] Islands hydrate correctly in browser (manual verification with blog sample)
- [ ] No Node.js or npm dependency in any build step
- [ ] All public API types have XML documentation comments
- [ ] All concrete classes are `sealed` (unless explicitly exempted)
- [ ] No optional/default parameters in public API

---

## Effort Summary

| Phase | Description | Est. Days | Cumulative |
|-------|-------------|-----------|------------|
| 0 | Solution scaffolding + CI | 2-3 | 2-3 |
| 1 | Core rendering engine | 8-10 | 10-13 |
| 2 | File-based routing + ASP.NET Core | 6-8 | 16-21 |
| 3 | Islands (vanilla JS + Web Components) | 8-10 | 24-31 |
| 4 | Content collections | 5-7 | 29-38 |
| 5 | Blazor WASM islands | 5-7 | 34-45 |
| 6 | CSS processing | 5-7 | 39-52 |
| 7 | Atoll middleware | 3-4 | 42-56 |
| 8 | Build pipeline (SSG) | 7-9 | 49-65 |
| 9 | CLI + library mode | 4-5 | 53-70 |
| 10 | Developer experience | 5-7 | 58-77 |
| 11 | Polish, samples, docs | 5-7 | 63-84 |
| **Total** | | **63-84 days** | **~13-17 weeks** |

Note: Phases 3, 4, 6, 7 can be parallelized (independent dependencies on Phase 1/2). With two developers, total calendar time could be reduced to ~10-12 weeks.
