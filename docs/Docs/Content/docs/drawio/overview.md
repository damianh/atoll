---
title: DrawIO Overview
description: Render draw.io diagrams in the browser using the official viewer-static.min.js with pan, zoom, toolbar, and layer controls.
order: 40
section: DrawIO Plugin
---

# DrawIO Overview

The `Atoll.DrawIo` plugin embeds draw.io diagrams into static pages using the official `viewer-static.min.js` from diagrams.net. Diagrams are rendered client-side with full pan, zoom, layer toggling, and lightbox support.

A single component is provided:

| Component | Type | Use case |
|---|---|---|
| `DrawioDiagram` | `VanillaJsIsland` | Client-side diagram rendering with pan, zoom, toolbar, and layers |

> **Note:** JavaScript is required to display diagrams. A `<noscript>` fallback message is shown when JavaScript is disabled.

## Live example

<DrawioDiagram FilePath="docs/Docs/Content/diagrams/sample.drawio" Alt="Atoll build pipeline overview" />

## Installation

Add a project reference to `Atoll.DrawIo`:

```xml
<ProjectReference Include="..\..\src\Atoll.DrawIo\Atoll.DrawIo.csproj" />
```

Register the component in your `ContentConfig`:

```csharp
using Atoll.DrawIo.Islands;

markdownOptions.Components = new ComponentMap()
    // ... other components ...
    .Add<DrawioDiagram>("drawio-diagram");
```

The viewer JavaScript assets (`atoll-drawio-viewer.min.js` and `atoll-drawio-viewer-init.js`) are embedded in the assembly and served automatically via the `IIslandAssetProvider` pipeline — no additional configuration required.

## Usage

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
using Atoll.DrawIo.Islands;

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
| `Alt` | `string?` | `null` | Accessible label (`aria-label`) for the diagram |
| `Toolbar` | `bool` | `true` | Show the viewer toolbar (zoom, layers, lightbox) |
| `Lightbox` | `bool` | `false` | Enable the full-screen lightbox view |

When `Alt` is set, the wrapper `<div>` receives `role="img"` and `aria-label` for screen readers.

## Multi-page diagrams

draw.io files can contain multiple pages (tabs). By default, the first page (index `0`) is rendered. Select a different page by index or name:

```html
<!-- By name -->
<DrawioDiagram FilePath="Content/diagrams/multi-page.drawio" PageName="Deployment" />

<!-- By index -->
<DrawioDiagram FilePath="Content/diagrams/multi-page.drawio" Page="2" />
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

Save this as a `.drawio` file and point `FilePath` at it to embed it in your page.
