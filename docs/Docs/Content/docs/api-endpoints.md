---
title: API Endpoints
description: Return structured HTTP responses with IAtollEndpoint.
order: 8
section: Features
---

# API Endpoints

Endpoints return structured HTTP responses instead of HTML. They implement `IAtollEndpoint` and can handle different HTTP methods.

## Why not use ASP.NET Core minimal APIs?

ASP.NET Core ships with a mature endpoint routing system (minimal APIs, `MapGet`/`MapPost`, etc.). Atoll provides its own endpoint abstraction for several reasons:

- **Unified routing** — Pages (`IAtollPage`) and endpoints (`IAtollEndpoint`) share the same route table and `RouteMatcher`. A single routing system handles both HTML pages and API endpoints with identical pattern matching, dynamic segments, and priority rules. With minimal APIs, page routing and API routing would be two separate systems.

- **File-based route discovery** — Atoll discovers routes from file-system conventions (e.g. `src/pages/api/posts.cs` → `/api/posts`, `src/pages/api/posts/[id].cs` → `/api/posts/[id]`). There is no manual `app.MapGet(...)` registration. This follows the same Astro-inspired convention used for pages.

- **SSG compatibility** — The same endpoint code runs at build time (pre-rendered as JSON files) and at runtime under ASP.NET Core. This is possible because `EndpointContext`, `EndpointRequest`, and `AtollResponse` are plain objects with no dependency on `HttpContext`. Minimal API handlers are coupled to the ASP.NET Core runtime and cannot run during static site generation.

- **Class-per-route organization** — One class handles all HTTP methods for a route. `GetAsync`, `PostAsync`, `PutAsync`, etc. live together, and unimplemented methods automatically return `405 Method Not Allowed`. With minimal APIs, each method is a separate `MapGet`/`MapPost` call, typically scattered across registration code.

- **Testability without infrastructure** — Endpoints can be unit tested by constructing an `EndpointContext` directly — no `HttpContext`, `TestServer`, or `WebApplicationFactory` required.

If you don't need these properties — for example, you're building a traditional API without pages, SSG, or file-based routing — ASP.NET Core's minimal APIs are a great choice. Atoll's endpoint abstraction exists to integrate API routes into the same conventions and build pipeline that power pages, layouts, and static site generation.

## Creating an endpoint

```csharp
using Atoll.Routing;

[PageRoute("/api/posts")]
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

## Response types

| Factory method | Description |
|---|---|
| `AtollResponse.Json(data)` | JSON response (200 OK) |
| `AtollResponse.Json(data, statusCode)` | JSON with custom status |
| `AtollResponse.Text(content)` | Plain text response |
| `AtollResponse.Html(content)` | HTML response |
| `AtollResponse.Redirect(url)` | 302 redirect |
| `AtollResponse.NotFound()` | 404 Not Found |

## HTTP method handling

Implement only the methods you need. Unimplemented methods automatically return `405 Method Not Allowed` with the correct `Allow` header.

```csharp
public sealed class ReadOnlyEndpoint : IAtollEndpoint
{
    // Only GET is supported
    public Task<AtollResponse> GetAsync(EndpointContext context)
    {
        return Task.FromResult(AtollResponse.Json(new { ok = true }));
    }

    // POST, PUT, DELETE etc. → 405 automatically
}
```

## Endpoint context

`EndpointContext` provides access to the request:

```csharp
public Task<AtollResponse> GetAsync(EndpointContext context)
{
    var request = context.Request;
    var query = request.Query["filter"];
    var headers = request.Headers;
    return Task.FromResult(AtollResponse.Json(new { query }));
}
```

## Registering endpoints

Endpoints are registered alongside pages as `RouteEntry` objects:

```csharp
var routes = new[]
{
    new RouteEntry("/", typeof(IndexPage), ""),
    new RouteEntry("/api/posts", typeof(PostsEndpoint), ""),
};
```
