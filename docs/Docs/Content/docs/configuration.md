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
```
