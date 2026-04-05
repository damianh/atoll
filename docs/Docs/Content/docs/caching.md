---
title: HTTP Caching
description: Cache-control headers, ETags, and _headers file generation for SSG and live server deployments.
order: 10
section: Features
---

# HTTP Caching in Atoll

Atoll ships with a thoughtful, opinionated caching strategy designed to be optimal out of the box for both SSG (static-site generation) and live server deployments.

## Overview

Atoll's caching philosophy is:

- **Fingerprinted assets are immutable.** Files in `_atoll/` with a content hash in the filename (e.g., `styles.a1b2c3d4.css`) can be cached for one year. They never change — a new deploy produces a new filename.
- **HTML always revalidates.** HTML pages are stale as soon as a new deploy happens, so they are served with `Cache-Control: public, max-age=0, must-revalidate` (or `no-cache` on the live server). Browsers revalidate on every navigation but can serve from cache if the content hasn't changed (304 Not Modified).
- **Dev server is never cached.** The dev server always sends `Cache-Control: no-cache` everywhere so you always see your latest changes.

:::aside{type="tip" title="No hard-refreshing in dev mode"}
The dev server always sends `Cache-Control: no-cache` on every response, so you never need to hard-refresh during development. This behaviour is intentional and cannot be disabled.
:::

---

## SSG Deployments

When you run `atoll build`, Atoll generates a `_headers` file in the output directory alongside your static HTML, assets, and the `_atoll/` folder.

### `_headers` file format

The `_headers` file uses the [Netlify headers format](https://docs.netlify.com/routing/headers/), which is also supported by Cloudflare Pages. Example output:

```
/_atoll/*
  Cache-Control: public, max-age=31536000, immutable

/*.html
  Cache-Control: public, max-age=0, must-revalidate

/
  Cache-Control: public, max-age=0, must-revalidate

/search-index.json
  Cache-Control: public, max-age=0, must-revalidate
```

### Supported platforms

| Platform | Support |
|---|---|
| **Netlify** | `_headers` is auto-detected in the publish directory |
| **Cloudflare Pages** | `_headers` is auto-detected in the build output |
| **Vercel** | Use `vercel.json` (see below) |
| **Azure Static Web Apps** | Use `staticwebapp.config.json` (see below) |
| **nginx** | Use a `location` block (see below) |
| **IIS** | Use `web.config` (see below) |

### Disabling `_headers` generation

```json
{
  "build": {
    "cache": {
      "generateHeadersFile": false
    }
  }
}
```

### Custom rules

Append extra rules after the Atoll defaults via `atoll.json`:

```json
{
  "build": {
    "cache": {
      "customRules": [
        {
          "path": "/api/*",
          "headers": { "Cache-Control": "no-store" }
        },
        {
          "path": "/fonts/*",
          "headers": { "Cache-Control": "public, max-age=31536000, immutable" }
        }
      ]
    }
  }
}
```

---

## Live Server Deployments

When you run Atoll as a live ASP.NET Core server (using `services.AddAtoll()` + `app.UseAtoll()`), the server automatically adds ETag-based conditional request support to every page response.

### ETag / 304 Not Modified

For every page response, Atoll:

:::steps
1. Renders the page to HTML.
2. Computes a [weak ETag](https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/ETag) from the full SHA-256 hash of the rendered bytes: `W/"e3b0c44..."`.
3. Compares the ETag with the request's `If-None-Match` header (if present).
4. If they match, returns `304 Not Modified` — no body, just the ETag header. The browser serves the page from its local cache.
5. If they don't match (or no `If-None-Match` was sent), returns `200 OK` with the full HTML, the `ETag` header, and `Cache-Control: no-cache`.
:::

This means the first visit to a page always fetches the full HTML; subsequent visits send `If-None-Match` and typically get a lightweight `304` response back.

### Disabling ETag on page responses

:::aside{type="caution" title="Disabling ETags removes caching benefits"}
When ETags are disabled, the live server renders and streams pages with no `ETag` or `Cache-Control` header. Every request will transfer the full response body, increasing bandwidth and response times.
:::

```csharp
services.AddAtoll(options =>
{
    options.EnableCacheControl = false;
});
```

When disabled, the live server renders and streams pages with no ETag or Cache-Control header, matching the original behaviour.

### ETag on endpoint responses

For custom Atoll endpoints (types implementing `IAtollEndpoint`), ETags are not added automatically. To add ETag support to an endpoint pipeline, compose `CacheControlMiddleware` into your middleware sequence:

```csharp
// Create a middleware pipeline with ETag support
var pipeline = MiddlewareSequencer.Sequence(
    CacheControlMiddleware.Create()
);
```

With custom options:

```csharp
var pipeline = MiddlewareSequencer.Sequence(
    CacheControlMiddleware.Create(new CacheControlMiddlewareOptions
    {
        DefaultCacheControl = "public, max-age=60",
        IncludeCacheControlHeader = true
    })
);
```

`CacheControlMiddleware` will not overwrite an `ETag` header that is already present on the response — if your endpoint sets its own ETag, the middleware passes through unchanged.

:::aside{type="note" title="Existing ETags are preserved"}
If your endpoint already sets an `ETag` header, `CacheControlMiddleware` will not overwrite it. This lets you implement custom ETag logic when needed.
:::

---

## Preview Server

The `atoll preview` command serves the `dist/` directory with the following cache headers:

| Path pattern | Cache-Control |
|---|---|
| `/_atoll/*` (fingerprinted assets) | `public, max-age=31536000, immutable` |
| `*.html` and directory indexes | `public, max-age=0, must-revalidate` |
| All other files | `public, max-age=3600` |

This matches what a CDN like Netlify or Cloudflare Pages would apply based on the `_headers` file, giving you a faithful local preview of production caching behaviour.

---

## Configuration reference

The `build.cache` section in `atoll.json`:

```json
{
  "build": {
    "cache": {
      "generateHeadersFile": true,
      "customRules": [
        {
          "path": "/api/*",
          "headers": {
            "Cache-Control": "no-store"
          }
        }
      ]
    }
  }
}
```

| Property | Type | Default | Description |
|---|---|---|---|
| `generateHeadersFile` | `bool` | `true` | Whether to write a `_headers` file to the output directory during `atoll build`. |
| `customRules` | `array` | `[]` | Extra path → headers rules appended after the built-in defaults. |
| `customRules[].path` | `string` | `""` | URL path pattern (e.g., `/api/*`). Rules with an empty path are skipped. |
| `customRules[].headers` | `object` | `{}` | Key/value map of header name → value. Rules with no headers are skipped. |

---

## Hosting-specific guidance

:::card-grid{stagger=true}
:::card{title="Vercel"}
Configure headers in `vercel.json` — Vercel does not use `_headers`.
:::
:::card{title="Azure Static Web Apps"}
Configure headers in `staticwebapp.config.json`.
:::
:::card{title="nginx"}
Use `location` blocks to set `Cache-Control` headers per path pattern.
:::
:::card{title="IIS"}
Use `web.config` with `<location>` elements to control static content caching.
:::
:::

### Vercel

Vercel does not use `_headers`. Configure headers in `vercel.json`:

```json
{
  "headers": [
    {
      "source": "/_atoll/(.*)",
      "headers": [{ "key": "Cache-Control", "value": "public, max-age=31536000, immutable" }]
    },
    {
      "source": "/(.*)\\.html",
      "headers": [{ "key": "Cache-Control", "value": "public, max-age=0, must-revalidate" }]
    },
    {
      "source": "/",
      "headers": [{ "key": "Cache-Control", "value": "public, max-age=0, must-revalidate" }]
    }
  ]
}
```

### Azure Static Web Apps

Configure headers in `staticwebapp.config.json`:

```json
{
  "routes": [
    {
      "route": "/_atoll/*",
      "headers": { "Cache-Control": "public, max-age=31536000, immutable" }
    },
    {
      "route": "/*.html",
      "headers": { "Cache-Control": "public, max-age=0, must-revalidate" }
    }
  ]
}
```

### nginx

```nginx
location /_atoll/ {
    add_header Cache-Control "public, max-age=31536000, immutable";
}

location ~* \.html$ {
    add_header Cache-Control "public, max-age=0, must-revalidate";
}
```

### IIS

```xml
<configuration>
  <location path="_atoll">
    <system.webServer>
      <staticContent>
        <clientCache cacheControlMode="UseMaxAge" cacheControlMaxAge="365.00:00:00" />
      </staticContent>
      <httpProtocol>
        <customHeaders>
          <add name="Cache-Control" value="public, max-age=31536000, immutable" />
        </customHeaders>
      </httpProtocol>
    </system.webServer>
  </location>
</configuration>
```

---

## Development mode

The Atoll dev server (`atoll dev`) always sends `Cache-Control: no-cache` on every response, including assets and API endpoints. This ensures you always see the latest changes during development without needing to hard-refresh.

This behaviour is intentional and cannot be disabled for the dev server.
