---
title: DrawIO Overview
description: Render draw.io diagrams as static or interactive SVG with pan, zoom, and layer controls.
order: 40
section: DrawIO Plugin
---

# DrawIO Overview

The `Atoll.DrawIo` plugin renders `.drawio` (draw.io / diagrams.net) files as inline SVG — no external processes, no JavaScript, and no third-party services. Diagrams are parsed and rendered entirely in .NET at build time.

Two components are provided:

| Component | Type | Use case |
|---|---|---|
| `DrawioDiagram` | `AtollComponent` | Static SVG — zero JavaScript |
| `InteractiveDrawioDiagram` | `VanillaJsIsland` | Client-side pan, zoom, and layer toggling |

## Live examples

### Static diagram

The following diagram is rendered at build time as inline SVG with no client-side JavaScript:

<DrawioDiagram FilePath="docs/Docs/Content/diagrams/sample.drawio" Alt="Atoll build pipeline — Markdown to static HTML" />

### Interactive diagram

This diagram includes pan, zoom, and layer toggle controls. Try dragging to pan, scrolling to zoom, and clicking the layer buttons below the diagram:

<InteractiveDrawioDiagram FilePath="docs/Docs/Content/diagrams/sample.drawio" PageName="Architecture" Alt="Atoll plugin architecture" />

## Installation

Add a project reference to `Atoll.DrawIo`:

```xml
<ProjectReference Include="..\..\src\Atoll.DrawIo\Atoll.DrawIo.csproj" />
```

Register the components in your `ContentConfig`:

```csharp
using Atoll.DrawIo.Components;
using Atoll.DrawIo.Islands;

markdownOptions.Components = new ComponentMap()
    // ... other components ...
    .Add<DrawioDiagram>("drawio-diagram")
    .Add<InteractiveDrawioDiagram>("interactive-drawio-diagram");
```

No additional NuGet packages are required. The island JavaScript asset is embedded in the assembly and served automatically via the `IIslandAssetProvider` pipeline (auto-discovered from referenced assemblies).

## Static diagram

`DrawioDiagram` renders a `.drawio` file as inline SVG with no client-side JavaScript.

### Markdown usage

Use the PascalCase tag syntax in your `.md` files:

```html
<DrawioDiagram FilePath="Content/diagrams/architecture.drawio" Alt="System architecture overview" />
```

Or the directive syntax:

```markdown
:::drawio-diagram{FilePath="Content/diagrams/architecture.drawio" Alt="System architecture overview"}
:::
```

### C# usage

```csharp
using Atoll.DrawIo.Components;

var props = new Dictionary<string, object?>
{
    ["FilePath"] = "Content/diagrams/architecture.drawio",
    ["Alt"] = "System architecture overview",
};
var fragment = ComponentRenderer.ToFragment<DrawioDiagram>(props);
await RenderAsync(fragment);
```

### Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `FilePath` | `string` | *(required)* | Path to the `.drawio` file |
| `Page` | `int?` | `0` | Zero-based page index to render |
| `PageName` | `string?` | `null` | Page name to render (alternative to `Page`) |
| `Width` | `string?` | `null` | SVG width attribute (e.g. `"800px"`, `"100%"`) |
| `Height` | `string?` | `null` | SVG height attribute (e.g. `"600px"`, `"auto"`) |
| `VisibleLayers` | `IReadOnlyList<string>?` | `null` | Explicit layer names/IDs to show; `null` = all |
| `HiddenLayers` | `IReadOnlyList<string>?` | `null` | Layer names/IDs to hide |
| `Background` | `string?` | `null` | SVG background color (e.g. `"#ffffff"`) |
| `Alt` | `string?` | `null` | Accessible label (`aria-label`) for the diagram |

When `Alt` is set, the wrapper `<div>` receives `role="img"` and `aria-label` for screen readers.

## Interactive diagram

`InteractiveDrawioDiagram` is an island that renders the diagram as static SVG on the server, then hydrates with JavaScript for pan, zoom, and layer toggling.

### Markdown usage

```html
<InteractiveDrawioDiagram FilePath="Content/diagrams/architecture.drawio" Alt="Interactive architecture diagram" />
```

### C# usage

```csharp
using Atoll.DrawIo.Islands;

var props = new Dictionary<string, object?>
{
    ["FilePath"] = "Content/diagrams/architecture.drawio",
    ["EnablePanZoom"] = true,
    ["ShowLayerControls"] = true,
};
var fragment = ComponentRenderer.ToFragment<InteractiveDrawioDiagram>(props);
await RenderAsync(fragment);
```

| Detail | Value |
|---|---|
| Directive | `client:visible` (default for `VanillaJsIsland`) |
| Script | `atoll-drawio-interactive.js` (embedded) |

The JavaScript module is only loaded when the element scrolls into view, keeping page weight minimal for diagrams below the fold.

### Parameters

All parameters from `DrawioDiagram` are supported, plus:

| Parameter | Type | Default | Description |
|---|---|---|---|
| `ShowLayerControls` | `bool` | `true` | Render layer toggle buttons below the diagram |
| `EnablePanZoom` | `bool` | `true` | Enable mouse/touch pan and zoom |

**Behaviour:**

- Pan by click-dragging the diagram
- Zoom with the scroll wheel or pinch gesture
- Layer toggle buttons show/hide individual layers; each button displays the layer name

## Multi-page diagrams

draw.io files can contain multiple pages (tabs). By default, the first page (index `0`) is rendered. Select a different page by index or name:

```html
<!-- By name -->
<DrawioDiagram FilePath="Content/diagrams/multi-page.drawio" PageName="Deployment" />

<!-- By index -->
<DrawioDiagram FilePath="Content/diagrams/multi-page.drawio" Page="2" />
```

## Layer visibility

Control which layers are rendered using `VisibleLayers` (allowlist) or `HiddenLayers` (blocklist). `VisibleLayers` takes precedence when both are set.

```csharp
// Show only specific layers
var props = new Dictionary<string, object?>
{
    ["FilePath"] = "Content/diagrams/layers.drawio",
    ["VisibleLayers"] = new List<string> { "Infrastructure", "Services" },
};

// Hide specific layers
var props = new Dictionary<string, object?>
{
    ["FilePath"] = "Content/diagrams/layers.drawio",
    ["HiddenLayers"] = new List<string> { "Debug" },
};
```

## Content collections

`DrawioCollectionLoader` loads `.drawio` and `.dio` files from a directory as content entries with auto-populated metadata. This integrates draw.io files into Atoll's content pipeline.

```csharp
using Atoll.DrawIo.Content;

var loader = new DrawioCollectionLoader();
var entries = loader.LoadCollection("Content/diagrams", "diagrams");

foreach (var entry in entries)
{
    Console.WriteLine($"{entry.Data.Title}: {entry.Data.PageCount} pages");
    foreach (var page in entry.Data.Pages)
    {
        Console.WriteLine($"  - {page.Name} ({page.Layers.Count} layers)");
    }
}
```

Each entry provides:

| Property | Description |
|---|---|
| `entry.Id` | Relative file path within the directory |
| `entry.Slug` | File name without extension |
| `entry.Body` | Raw XML content |
| `entry.Data.Title` | Diagram title (derived from file name) |
| `entry.Data.PageCount` | Number of pages in the file |
| `entry.Data.Pages` | List of `DrawioPageInfo` with name, ID, and layer info |

## Sample `.drawio` file

A minimal draw.io file with two shapes and one connector:

```xml
<mxfile version="21.0.0">
  <diagram name="Page-1" id="page1">
    <mxGraphModel>
      <root>
        <mxCell id="0" />
        <mxCell id="1" parent="0" />
        <mxCell id="2" value="Start"
                style="rounded=1;whiteSpace=wrap;html=1;"
                vertex="1" parent="1">
          <mxGeometry x="100" y="100" width="120" height="60"
                      as="geometry" />
        </mxCell>
        <mxCell id="3" value="End"
                style="ellipse;whiteSpace=wrap;html=1;"
                vertex="1" parent="1">
          <mxGeometry x="320" y="100" width="120" height="60"
                      as="geometry" />
        </mxCell>
        <mxCell id="4" value="Connect"
                style="edgeStyle=orthogonalEdgeStyle;"
                edge="1" source="2" target="3" parent="1">
          <mxGeometry relative="1" as="geometry" />
        </mxCell>
      </root>
    </mxGraphModel>
  </diagram>
</mxfile>
```

Save this as a `.drawio` file and point `FilePath` at it to render it as inline SVG.
