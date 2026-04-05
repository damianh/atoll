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
| Pages | files in `src/pages/` | classes implementing `IAtollPage` |
| API routes | `.ts` files exporting handlers | classes implementing `IAtollEndpoint` |
| Islands | `client:load`, `client:idle`, etc. | `[ClientLoad]`, `[ClientIdle]`, etc. |
| Content collections | `getCollection()` + YAML schemas | `CollectionQuery` + C# schema classes |
| Middleware | `defineMiddleware()` | `MiddlewareHandler` delegates |
| CSS scoping | automatic `astro-HASH` classes | `[Styles]` attribute with `:where(.atoll-HASH)` |
| Build output | `dist/` via `astro build` | `dist/` via `StaticSiteGenerator` |

## Quickstart

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) or later

### 1. Create a project

```bash
dotnet new classlib -n MySite
cd MySite
```

Add the Atoll project references (or NuGet packages when published):

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="path/to/Atoll/Atoll.csproj" />
    <ProjectReference Include="path/to/Atoll.Middleware/Atoll.Middleware.csproj" />
  </ItemGroup>
</Project>
```

### 2. Create a layout

Layouts wrap pages with shared structure (HTML shell, nav, footer). A layout renders its page content via `RenderSlotAsync()`:

```csharp
using Atoll.Components;

public sealed class MainLayout : AtollComponent
{
    [Parameter]
    public string Title { get; set; } = "My Site";

    protected override async Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("""
            <html lang="en">
            <head>
                <meta charset="utf-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1" />
                <title>
            """);
        WriteText(Title);
        WriteHtml("""
                </title>
            </head>
            <body>
                <nav><a href="/">Home</a> | <a href="/about">About</a></nav>
                <main>
            """);
        await RenderSlotAsync();
        WriteHtml("""
                </main>
                <footer>Built with Atoll</footer>
            </body>
            </html>
            """);
    }
}
```

### 3. Create a page

Pages implement `IAtollPage` and are routed to URLs. Use the `[Layout]` attribute to wrap the page in a layout:

```csharp
using Atoll.Components;
using Atoll.Routing;

[Layout(typeof(MainLayout))]
public sealed class IndexPage : AtollComponent, IAtollPage
{
    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<h1>Hello, Atoll!</h1>");
        WriteHtml("<p>This is a .NET-native framework inspired by Astro.</p>");
        return Task.CompletedTask;
    }
}
```

### 4. Host with ASP.NET Core

```csharp
using Atoll.Middleware.Server.Hosting;
using Atoll.Routing;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAtoll(options =>
{
    options.RouteEntries.Add(new RouteEntry("/", typeof(IndexPage)));
    options.RouteEntries.Add(new RouteEntry("/about", typeof(AboutPage)));
});

var app = builder.Build();
app.UseAtoll();
app.Run();
```

### 5. Or generate a static site

```csharp
using Atoll.Build.Ssg;

var options = new SsgOptions("dist")
{
    BaseUrl = "https://example.com",
};

var generator = new StaticSiteGenerator(options);
var result = await generator.GenerateAsync(routes, assemblies);
// result.Pages contains the generated HTML files
```

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
- **`GetProp<T>(name)`** — read a typed prop value

Components are rendered top-down, synchronously or asynchronously, producing a stream of HTML chunks.

## Components

Components are reusable UI building blocks. They accept props via `[Parameter]` properties:

```csharp
using Atoll.Components;

public sealed class Card : AtollComponent
{
    [Parameter(Required = true)]
    public string Title { get; set; } = "";

    [Parameter]
    public string Description { get; set; } = "";

    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<div class=\"card\">");
        WriteHtml("<h2>");
        WriteText(Title);
        WriteHtml("</h2>");
        if (!string.IsNullOrEmpty(Description))
        {
            WriteHtml("<p>");
            WriteText(Description);
            WriteHtml("</p>");
        }
        WriteHtml("</div>");
        return Task.CompletedTask;
    }
}
```

Render a child component from a parent using `ComponentRenderer.ToFragment<T>`:

```csharp
var props = new Dictionary<string, object?>
{
    ["Title"] = "My Card",
    ["Description"] = "A reusable component",
};
var fragment = ComponentRenderer.ToFragment<Card>(props);
await RenderAsync(fragment);
```

## Pages

Pages are components that are directly routable. They implement `IAtollPage`:

```csharp
[Layout(typeof(MainLayout))]
public sealed class AboutPage : AtollComponent, IAtollPage
{
    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<h1>About</h1><p>This is the about page.</p>");
        return Task.CompletedTask;
    }
}
```

### Dynamic routes

Pages with URL parameters use `[Parameter]` properties. For static site generation, implement `IStaticPathsProvider` to enumerate all possible paths:

```csharp
[Layout(typeof(MainLayout))]
public sealed class BlogPostPage : AtollComponent, IAtollPage, IStaticPathsProvider
{
    [Parameter(Required = true)]
    public string Slug { get; set; } = "";

    public Task<IReadOnlyList<StaticPath>> GetStaticPathsAsync()
    {
        var paths = new List<StaticPath>
        {
            new(new Dictionary<string, string> { ["slug"] = "hello-world" }),
            new(new Dictionary<string, string> { ["slug"] = "second-post" }),
        };
        return Task.FromResult<IReadOnlyList<StaticPath>>(paths);
    }

    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml($"<h1>Post: ");
        WriteText(Slug);
        WriteHtml("</h1>");
        return Task.CompletedTask;
    }
}
```

Register with a dynamic route pattern:

```csharp
options.RouteEntries.Add(new RouteEntry("/blog/[slug]", typeof(BlogPostPage)));
```

## Islands (Partial Hydration)

Islands are components that render static HTML on the server, then load JavaScript on the client for interactivity. This is Atoll's implementation of Astro's islands architecture.

### Client directives

Each directive controls *when* the island's JavaScript loads:

| Directive | Attribute | Behavior |
|---|---|---|
| `client:load` | `[ClientLoad]` | Hydrate immediately on page load |
| `client:idle` | `[ClientIdle]` | Hydrate when browser is idle (`requestIdleCallback`) |
| `client:visible` | `[ClientVisible]` | Hydrate when the element scrolls into view (`IntersectionObserver`) |
| `client:media` | `[ClientMedia("(max-width: 768px)")]` | Hydrate when a CSS media query matches |

### Creating an island

Extend `VanillaJsIsland` and apply a client directive attribute:

```csharp
using Atoll.Components;
using Atoll.Islands;

[ClientLoad]
public sealed class ThemeToggle : VanillaJsIsland
{
    public override string ClientModuleUrl => "/scripts/theme-toggle.js";

    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("""
            <button id="theme-toggle" type="button" aria-label="Toggle theme">
                Toggle Theme
            </button>
            """);
        return Task.CompletedTask;
    }
}
```

The island renders as an `<atoll-island>` custom element with hydration metadata. The client-side JavaScript module should export a default function (or named `init`) that receives `(element, props, slots, metadata)`.

### Lazy-loaded islands

Use `[ClientVisible]` for content below the fold:

```csharp
[ClientVisible(RootMargin = "200px")]
public sealed class ImageGallery : VanillaJsIsland
{
    public override string ClientModuleUrl => "/scripts/gallery.js";

    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<div class=\"gallery\"><!-- server-rendered gallery grid --></div>");
        return Task.CompletedTask;
    }
}
```

Or `[ClientMedia]` for responsive islands that only hydrate at certain viewport sizes:

```csharp
[ClientMedia("(max-width: 768px)")]
public sealed class MobileNav : VanillaJsIsland
{
    public override string ClientModuleUrl => "/scripts/mobile-nav.js";

    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<nav class=\"mobile-nav\"><!-- hamburger menu --></nav>");
        return Task.CompletedTask;
    }
}
```

## Content Collections

Content collections let you manage Markdown files with typed YAML frontmatter. Define a schema class, put your `.md` files in a directory, and query them with `CollectionQuery`.

### 1. Define a frontmatter schema

```csharp
using System.ComponentModel.DataAnnotations;

public sealed class BlogPostSchema
{
    [Required]
    public string Title { get; set; } = "";

    [Required]
    public string Description { get; set; } = "";

    [Required]
    public DateTime PubDate { get; set; }

    public string Author { get; set; } = "";
    public string Tags { get; set; } = "";
    public bool Draft { get; set; }
}
```

### 2. Create Markdown content files

Place `.md` files in a `Content/blog/` directory:

```markdown
---
title: Getting Started with Atoll
description: Learn how to build your first site.
pubDate: 2026-01-15
author: Jane Developer
tags: atoll, tutorial
draft: false
---

# Getting Started with Atoll

Your Markdown content goes here. Supports standard Markdown plus:
- Footnotes
- Task lists
- Auto-links
- Emphasis extras
```

### 3. Query and render content

```csharp
// Load all non-draft posts, sorted by date
var posts = query.GetCollection<BlogPostSchema>("blog",
    entry => !entry.Data.Draft);

var sorted = posts
    .OrderByDescending(p => p.Data.PubDate)
    .ToList();

// Render a single post's Markdown to HTML
var entry = query.GetEntry<BlogPostSchema>("blog", "getting-started");
var rendered = query.Render(entry);  // RenderedContent with .Html and .Headings

// Use in a component
var contentComponent = ContentComponent.FromRenderedContent(rendered);
await RenderAsync(contentComponent.ToRenderFragment());
```

## API Endpoints

Endpoints return structured HTTP responses instead of HTML. They implement `IAtollEndpoint` and share the same routing system as pages — same `RouteMatcher`, same file-based discovery conventions, same dynamic segment syntax. This unified routing is why Atoll has its own endpoint abstraction rather than delegating to ASP.NET Core's minimal APIs. It also means endpoint code is decoupled from `HttpContext`, so the same endpoints can be pre-rendered as JSON during static site generation.

```csharp
using Atoll.Routing;

public sealed class PostsEndpoint : IAtollEndpoint
{
    public Task<AtollResponse> GetAsync(EndpointContext context)
    {
        var posts = new[] { new { Id = 1, Title = "Hello World" } };
        return Task.FromResult(AtollResponse.Json(posts));
    }

    public Task<AtollResponse> PostAsync(EndpointContext context)
    {
        return Task.FromResult(AtollResponse.Json(
            new { Id = 2, Title = "Created" }, 201));
    }
}
```

Register on an API route:

```csharp
options.RouteEntries.Add(new RouteEntry("/api/posts", typeof(PostsEndpoint)));
```

Unimplemented HTTP methods automatically return `405 Method Not Allowed` with the correct `Allow` header.

## CSS Scoping

The `[Styles]` attribute attaches CSS to a component and automatically scopes it using `:where(.atoll-HASH)`:

```csharp
[Styles(".card { padding: 1rem; border: 1px solid #ddd; } .card h2 { color: navy; }")]
public sealed class Card : AtollComponent
{
    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<div class=\"card\"><h2>Scoped Title</h2></div>");
        return Task.CompletedTask;
    }
}
```

The CSS is extracted, scoped, and can be bundled into a single stylesheet by the asset pipeline. Use `[GlobalStyle]` on a component to opt out of scoping.

## Middleware

Atoll has its own middleware pipeline (independent of ASP.NET Core middleware) for request-level logic:

```csharp
using Atoll.Middleware;

// Define a middleware handler
MiddlewareHandler authMiddleware = async (context, next) =>
{
    // Set data for downstream handlers
    context.Locals["user"] = "authenticated-user";

    // Continue to next middleware or final handler
    await next();
};

// Sequence multiple middleware
var pipeline = MiddlewareSequencer.Sequence(authMiddleware, loggingMiddleware);
```

Middleware can also rewrite requests:

```csharp
MiddlewareHandler redirectMiddleware = async (context, next) =>
{
    if (context.Request.Url.AbsolutePath == "/old-page")
    {
        context.Rewrite("/new-page");
    }
    await next();
};
```

## Sample Sites

The repository includes two complete sample sites:

### Blog Sample (`samples/Atoll.Samples.Blog/`)

A full blog with:
- Layouts, navigation, and footer
- Blog index with post cards
- Individual post pages with Markdown rendering
- Tag-based filtering
- Content collections with frontmatter validation
- Interactive islands (theme toggle, search box)

### Portfolio Sample (`samples/Atoll.Samples.Portfolio/`)

A portfolio site demonstrating:
- Hero sections, project cards, skill badges
- `[ClientLoad]` island — contact form with validation
- `[ClientVisible]` island — image gallery with lightbox
- `[ClientMedia]` island — mobile hamburger navigation
- Four pages with responsive layout

## Project Structure

A typical Atoll project is organized as:

```
MySite/
├── Components/          # Reusable UI components
│   ├── Card.cs
│   └── Header.cs
├── Content/             # Markdown content collections
│   └── blog/
│       ├── first-post.md
│       └── second-post.md
├── Islands/             # Interactive client-side islands
│   └── ThemeToggle.cs
├── Layouts/             # Page layout wrappers
│   └── MainLayout.cs
├── Pages/               # Routable page components
│   ├── IndexPage.cs
│   ├── AboutPage.cs
│   └── BlogPostPage.cs
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
