<p align="center">
  <img src="assets/logo.png" alt="Atoll" width="120" />
</p>

# Atoll

A .NET-native framework inspired by [Astro](https://astro.build). Atoll brings server-first HTML rendering, islands architecture, content collections, and static site generation to the .NET ecosystem.

## Why Atoll?

Atoll applies Astro's core philosophy to .NET: **ship zero JavaScript by default**, then selectively hydrate interactive components ("islands") on the client. Pages and components render server-side as plain HTML. Only the parts that need interactivity load JavaScript — and only when they need to.

### Comparison with Astro

| Concept | Astro | Atoll |
|---|---|---|
| Component format | `.astro` files (HTML + JS frontmatter) | C# classes extending `AtollComponent` |
| Template language | JSX-like expressions | `WriteHtml` / `WriteText` methods |
| Layouts | `<slot />` in `.astro` files | `[Layout]` attribute + `RenderSlotAsync()` |
| Pages | files in `src/pages/` | classes implementing `IAtollPage` with `[PageRoute]` |
| API routes | `.ts` files exporting handlers | classes implementing `IAtollEndpoint` |
| Islands | `client:load`, `client:idle`, etc. | `[ClientLoad]`, `[ClientIdle]`, etc. |
| Content collections | `getCollection()` + YAML schemas | `CollectionQuery` + C# schema classes |
| Middleware | `defineMiddleware()` | `MiddlewareHandler` delegates |
| CSS scoping | automatic `astro-HASH` classes | `[Styles]` attribute with `:where(.atoll-HASH)` |
| Build output | `dist/` via `astro build` | `dist/` via `StaticSiteGenerator` |

## Architecture

Atoll is organized into focused libraries:

```
Atoll                Components, rendering, routing, islands, CSS scoping, slots,
                     head management, content collections, Markdown, SSG, asset pipeline
Atoll.Middleware     ASP.NET Core hosting integration, request middleware, dev server, live reload
Atoll.Cli            CLI commands (build, dev, new, preview)
```

### Request flow

```
HTTP Request
  → ASP.NET Core pipeline
    → AtollMiddleware
      → RouteMatcher (pattern matching)
        → IAtollPage → LayoutResolver → PageRenderer → HTML response
        → IAtollEndpoint → EndpointDispatcher → JSON/text/redirect response
```

### Rendering model

Every component extends `AtollComponent` and implements `RenderCoreAsync`. The rendering API is intentionally simple:

- **`WriteHtml(string)`** — write trusted HTML directly to the output
- **`WriteText(string)`** — write text that is automatically HTML-escaped
- **`RenderSlotAsync()`** — render child content (the default slot)
- **`RenderSlotAsync(name)`** — render a named slot
- **`RenderAsync(fragment)`** — render a `RenderFragment` (for composing child components)

Components are rendered top-down, synchronously or asynchronously, producing a stream of HTML chunks.

## Documentation

Full guides and API details live in [`docs/`](docs/Docs/Content/docs/):

| Topic | Description |
|---|---|
| [Getting Started](docs/Docs/Content/docs/getting-started.md) | Create your first project and render a page |
| [Components](docs/Docs/Content/docs/components.md) | Reusable UI building blocks with `AtollComponent` |
| [Pages & Routing](docs/Docs/Content/docs/pages-and-routing.md) | Route URLs to pages with `IAtollPage` and `[PageRoute]` |
| [Layouts](docs/Docs/Content/docs/layouts.md) | Wrap pages with shared HTML structure |
| [MDA Format](docs/Docs/Content/docs/mda-format.md) | Markdown + YAML frontmatter + component directives |
| [Content Collections](docs/Docs/Content/docs/content-collections.md) | Typed Markdown content with `CollectionQuery` |
| [Islands](docs/Docs/Content/docs/islands.md) | Partial hydration with `[ClientLoad]`, `[ClientVisible]`, etc. |
| [CSS Scoping](docs/Docs/Content/docs/css-scoping.md) | Automatic per-component CSS isolation |
| [API Endpoints](docs/Docs/Content/docs/api-endpoints.md) | Structured HTTP responses with `IAtollEndpoint` |
| [Static Site Generation](docs/Docs/Content/docs/static-site-generation.md) | Generate a complete static site with `StaticSiteGenerator` |
| [Configuration](docs/Docs/Content/docs/configuration.md) | Project settings via `atoll.yaml` |
| [HTTP Caching](docs/Docs/Content/docs/caching.md) | Cache-control, ETags, and `_headers` file generation |

## Sample Sites

The repository includes three complete sample sites in [`samples/`](samples/):

- **[Blog](samples/Atoll.Samples.Blog/)** — Layouts, content collections with frontmatter, tag filtering, Markdown rendering, interactive islands (theme toggle, search)
- **[Portfolio](samples/Atoll.Samples.Portfolio/)** — Hero sections, project cards, `[ClientLoad]` / `[ClientVisible]` / `[ClientMedia]` islands, responsive layout
- **[Articles](samples/Atoll.Samples.Articles/)** — Article series with navigation, documentation-style theme

## Project Structure

A typical Atoll project:

```
MySite/
├── Components/          # Reusable UI components
├── Content/             # Markdown content collections
│   └── blog/
├── Islands/             # Interactive client-side islands
├── Layouts/             # Page layout wrappers
├── Pages/               # Routable page components
├── BlogPostSchema.cs    # Content collection schema
└── MySite.csproj
```

## Building

```bash
# Build in release mode
dotnet build -c Release

# Run tests
dotnet test

# Run the dev server (if using Atoll.Cli)
atoll dev

# Build static site
atoll build
```

## License

See [LICENSE](LICENSE) for details.
