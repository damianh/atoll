---
title: Configuration
description: Configure Atoll with AtollConfig and AtollConfigLoader.
order: 11
section: Advanced
---

# Configuration

Atoll uses a simple YAML configuration file (`atoll.yaml` or `atoll.yml`) for project-level settings. Configuration is loaded via `AtollConfigLoader`.

## Configuration file

Create `atoll.yaml` in your project root:

```yaml
baseUrl: https://example.com
outputDir: dist
title: My Atoll Site
description: A site built with Atoll
```

## Loading configuration

```csharp
using Atoll.Configuration;

var config = AtollConfigLoader.Load("atoll.yaml");
// config.BaseUrl, config.OutputDir, config.Title, config.Description
```

## AtollConfig properties

| Property | Type | Default | Description |
|---|---|---|---|
| `BaseUrl` | `string` | `""` | The canonical base URL of the site |
| `OutputDir` | `string` | `"dist"` | Output directory for SSG |
| `Title` | `string` | `""` | Site title |
| `Description` | `string` | `""` | Site description for metadata |

## Using config in the SSG pipeline

```csharp
var config = AtollConfigLoader.Load("atoll.yaml");

var options = new SsgOptions(config.OutputDir)
{
    BaseUrl = config.BaseUrl,
};

var generator = new StaticSiteGenerator(options);
await generator.GenerateAsync(routes, assemblies);
```

## Using config in layouts

Pass configuration as a layout parameter or via service props to make it available in components:

```csharp
var serviceProps = new Dictionary<string, object?>
{
    ["Query"] = query,
    ["SiteTitle"] = config.Title,
};
```

## CLI configuration

When using `atoll build` or `atoll dev` from the CLI, the tool automatically discovers and loads `atoll.yaml` from the current directory. CLI flags can override individual config values:

```bash
atoll build --base-url https://staging.example.com
atoll dev --port 5001
atoll dev --write-dist
```

### `--write-dist` flag

The `--write-dist` flag on `atoll dev` writes all rendered pages and assets to the output directory (default `dist/`) after each rebuild cycle. This keeps the output directory synchronized with the dev server state in real-time.

This is useful when an external process needs to serve the site from static files during development — for example, an ASP.NET AppHost that serves `dist/` via `UseStaticFiles()`.

```bash
atoll dev --write-dist --port 4321
```

Without `--write-dist`, the dev server renders pages in-memory per-request and never writes to disk. The `dist/` directory is only populated by `atoll build`.

**What gets written:**

- HTML pages (all routes, including dynamic routes expanded via `GetStaticPaths`)
- Island JavaScript assets
- Search index JSON (if configured)
- Core Atoll scripts (`_atoll/island.js`, `_atoll/directives.js`)

**Stale file cleanup:** When pages are added or removed, the output directory is automatically updated — stale files from previous rebuilds are deleted.
