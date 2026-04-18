# Atoll.Hosting.Aspire

[![NuGet](https://img.shields.io/nuget/v/Atoll.Hosting.Aspire.svg)](https://www.nuget.org/packages/Atoll.Hosting.Aspire)

[.NET Aspire](https://learn.microsoft.com/en-us/dotnet/aspire/) hosting integration for [Atoll](https://github.com/damianh/atoll) — the .NET-native Astro-inspired static site framework.

Adds `AddAtollSite()` to your AppHost, launching the `atoll dev` server as a managed Aspire resource with HTTP endpoint and readiness health check support.

## Installation

```bash
dotnet add package Atoll.Hosting.Aspire
```

## Usage

In your Aspire AppHost `Program.cs`:

```csharp
var site = builder.AddAtollSite("my-site", "../MySite");
```

With options:

```csharp
var site = builder.AddAtollSite("my-site", "../MySite")
    .WithWriteDist(); // write rendered output to dist/ after each rebuild cycle
```

### `WithWriteDist()`

When enabled, the dev server writes all rendered pages and static assets to the `dist/` directory after each rebuild. Useful when another resource (e.g. a static file server or an ASP.NET Core app) needs to serve the site from disk during development.

## Aspire Dashboard Readiness

The package registers an HTTP health check against the `/__health` endpoint on the Atoll dev server. The Aspire dashboard shows the resource as **Starting** until the dev server is listening and healthy, then transitions to **Running**.

The `/__health` endpoint is a lightweight `GET` route built into `atoll dev` — it returns `200 OK` with no body overhead.

## API

| Method | Description |
|---|---|
| `AddAtollSite(name, siteDirectory)` | Adds the Atoll dev server as an Aspire resource. Runs `atoll dev` on port 4321 by default. |
| `WithWriteDist()` | Appends `--write-dist` to the dev server command. |

## Requirements

- .NET Aspire 9.x or later
- `atoll` CLI tool installed globally: `dotnet tool install -g Atoll.Cli`

## More Information

- [Atoll repository](https://github.com/damianh/atoll)
- [Atoll documentation](https://github.com/damianh/atoll/tree/main/docs)
- [NuGet: Atoll.Hosting.Aspire](https://www.nuget.org/packages/Atoll.Hosting.Aspire)
