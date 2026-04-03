---
title: API Endpoints
description: Return structured HTTP responses with IAtollEndpoint.
order: 8
section: Features
---

# API Endpoints

Endpoints return structured HTTP responses instead of HTML. They implement `IAtollEndpoint` and can handle different HTTP methods.

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
