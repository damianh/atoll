---
title: Charts Overview
description: Render interactive Chart.js charts from JSON configuration with markdown code blocks and island components.
order: 35
section: Charts Plugin
---

# Charts Overview

The `Atoll.Charts` plugin renders interactive [Chart.js](https://www.chartjs.org/) charts on static pages. Charts are defined using Chart.js's native JSON configuration format (`{ type, data, options }`) and rendered client-side with built-in interactivity — tooltips, hover highlights, and legend toggling.

Two integration modes are provided:

| Mode | Mechanism | Use case |
|---|---|---|
| **Markdown code blocks** | `` ```chart `` fenced blocks | Quick inline charts in markdown content |
| **Island component** | `ChartIsland` (`VanillaJsIsland`) | Programmatic use in Razor pages and components |

| Feature | Description |
|---|---|
| **Zero JS by default** | Chart.js (~200 KB) is only loaded when a chart scrolls into the viewport |
| **Lazy hydration** | Uses `[ClientVisible]` — the vendor script is fetched on first intersection, not page load |
| **Native config format** | No custom schema — pass Chart.js JSON directly |
| **XSS safe** | JSON config is HTML-attribute-encoded at build time |

> **Note:** JavaScript is required to display charts. A `<noscript>` fallback message is shown when JavaScript is disabled.

## Installation

Add the `Atoll.Charts` NuGet package:

```bash
dotnet add package Atoll.Charts
```

Or add directly to your `.csproj`:

```xml
<PackageReference Include="Atoll.Charts" Version="0.1.*" />
```

The Chart.js vendor bundle (`atoll-charts-vendor.min.js`) and init script (`atoll-charts-init.js`) are embedded in the assembly and served automatically via the `IIslandAssetProvider` pipeline — no CDN or additional configuration required.

## Enabling in markdown

Register the `ChartExtension` in your markdown pipeline. When using Lagoon, add it to your `ContentConfig`:

```csharp
using Atoll.Charts;

// In your ContentConfig.Configure() method:
var markdownOptions = DocsMarkdownRenderer.CreateMarkdownOptions(DocsSetup.Config)
    ?? new MarkdownOptions();

// Append ChartExtension to the existing extensions list
var extensions = new List<IMarkdownExtension>(markdownOptions.Extensions ?? []);
extensions.Add(new ChartExtension());
markdownOptions.Extensions = extensions;
```

## Writing charts in markdown

Use a fenced code block with the `chart` language identifier containing a Chart.js configuration object in JSON:

````markdown
```chart
{
  "type": "bar",
  "data": {
    "labels": ["January", "February", "March"],
    "datasets": [{
      "label": "Sales",
      "data": [65, 59, 80]
    }]
  }
}
```
````

The language identifier is case-insensitive — `chart`, `Chart`, and `CHART` all work.

Invalid JSON is handled gracefully — a visible error message is rendered instead of crashing.

## Live examples

The charts below are live — rendered by Chart.js when they scroll into view. Hover over data points to see tooltips. Click legend items to toggle datasets.

### Bar chart

```chart
{
  "type": "bar",
  "data": {
    "labels": ["January", "February", "March", "April", "May", "June"],
    "datasets": [{
      "label": "Page Views",
      "data": [1200, 1900, 3000, 5000, 2300, 3200],
      "backgroundColor": ["#3b82f6", "#8b5cf6", "#ec4899", "#f59e0b", "#10b981", "#6366f1"]
    }]
  },
  "options": {
    "plugins": {
      "legend": { "display": true }
    }
  }
}
```

### Line chart

```chart
{
  "type": "line",
  "data": {
    "labels": ["Week 1", "Week 2", "Week 3", "Week 4", "Week 5", "Week 6"],
    "datasets": [
      {
        "label": "Visitors",
        "data": [500, 800, 750, 1200, 1100, 1400],
        "borderColor": "#3b82f6",
        "tension": 0.3,
        "fill": false
      },
      {
        "label": "Conversions",
        "data": [50, 80, 90, 120, 95, 140],
        "borderColor": "#10b981",
        "tension": 0.3,
        "fill": false
      }
    ]
  },
  "options": {
    "plugins": {
      "legend": { "position": "bottom" }
    }
  }
}
```

### Pie chart

```chart
{
  "type": "pie",
  "data": {
    "labels": ["Desktop", "Mobile", "Tablet"],
    "datasets": [{
      "data": [62, 28, 10],
      "backgroundColor": ["#3b82f6", "#8b5cf6", "#f59e0b"]
    }]
  },
  "options": {
    "plugins": {
      "legend": { "position": "right" }
    }
  }
}
```

### Doughnut chart

```chart
{
  "type": "doughnut",
  "data": {
    "labels": ["HTML", "CSS", "JavaScript", "C#", "Other"],
    "datasets": [{
      "data": [35, 25, 20, 15, 5],
      "backgroundColor": ["#ef4444", "#3b82f6", "#f59e0b", "#8b5cf6", "#6b7280"]
    }]
  }
}
```

### Radar chart

```chart
{
  "type": "radar",
  "data": {
    "labels": ["Performance", "Accessibility", "Best Practices", "SEO", "PWA"],
    "datasets": [
      {
        "label": "Before",
        "data": [65, 72, 80, 55, 40],
        "borderColor": "#ef4444",
        "backgroundColor": "rgba(239, 68, 68, 0.1)"
      },
      {
        "label": "After",
        "data": [95, 98, 95, 92, 85],
        "borderColor": "#10b981",
        "backgroundColor": "rgba(16, 185, 129, 0.1)"
      }
    ]
  }
}
```

### Scatter chart

```chart
{
  "type": "scatter",
  "data": {
    "datasets": [{
      "label": "Build Time vs Page Count",
      "data": [
        {"x": 10, "y": 0.5},
        {"x": 25, "y": 1.2},
        {"x": 50, "y": 2.1},
        {"x": 100, "y": 3.8},
        {"x": 200, "y": 6.5},
        {"x": 500, "y": 12.3}
      ],
      "backgroundColor": "#8b5cf6"
    }]
  },
  "options": {
    "scales": {
      "x": { "title": { "display": true, "text": "Pages" } },
      "y": { "title": { "display": true, "text": "Build time (s)" } }
    }
  }
}
```

### Polar area chart

```chart
{
  "type": "polarArea",
  "data": {
    "labels": ["Components", "Pages", "Layouts", "Islands", "Collections"],
    "datasets": [{
      "data": [42, 18, 6, 12, 8],
      "backgroundColor": [
        "rgba(59, 130, 246, 0.6)",
        "rgba(139, 92, 246, 0.6)",
        "rgba(236, 72, 153, 0.6)",
        "rgba(245, 158, 11, 0.6)",
        "rgba(16, 185, 129, 0.6)"
      ]
    }]
  }
}
```

## Clickable chart elements

Chart elements (bars, points, pie segments, etc.) can be made clickable by adding an `_atoll.links` key to the chart JSON config. Clicking a linked element navigates to the specified URL — no custom JavaScript required.

### Schema

`_atoll.links` is a 2D array indexed by `[datasetIndex][dataPointIndex]`. Each entry is a URL string or `null` (not clickable).

```json
{
  "type": "bar",
  "data": {
    "labels": ["Jan", "Feb", "Mar"],
    "datasets": [
      { "label": "Bugs", "data": [5, 3, 8] },
      { "label": "Features", "data": [12, 7, 15] }
    ]
  },
  "_atoll": {
    "links": [
      ["/bugs?m=jan", "/bugs?m=feb", "/bugs?m=mar"],
      ["/features?m=jan", null, "/features?m=mar"]
    ]
  }
}
```

For single-dataset charts, `links` has one inner array. `null` entries or missing indices are simply not clickable.

### Clickable bar chart

Hover over bars to see the pointer cursor. Click any bar to navigate.

```chart
{
  "type": "bar",
  "data": {
    "labels": ["January", "February", "March"],
    "datasets": [{
      "label": "Page Views",
      "data": [1200, 1900, 3000],
      "backgroundColor": ["#3b82f6", "#8b5cf6", "#ec4899"]
    }]
  },
  "_atoll": {
    "links": [
      ["#january", "#february", "#march"]
    ]
  }
}
```

### Clickable line chart

```chart
{
  "type": "line",
  "data": {
    "labels": ["Week 1", "Week 2", "Week 3", "Week 4"],
    "datasets": [{
      "label": "Visitors",
      "data": [500, 800, 750, 1200],
      "borderColor": "#3b82f6",
      "tension": 0.3,
      "fill": false
    }]
  },
  "_atoll": {
    "links": [
      ["#week-1", "#week-2", "#week-3", "#week-4"]
    ]
  }
}
```

### Clickable pie chart

For pie and doughnut charts, dataset index is always `0`. Data point index maps to each segment.

```chart
{
  "type": "pie",
  "data": {
    "labels": ["Desktop", "Mobile", "Tablet"],
    "datasets": [{
      "data": [62, 28, 10],
      "backgroundColor": ["#3b82f6", "#8b5cf6", "#f59e0b"]
    }]
  },
  "_atoll": {
    "links": [
      ["#desktop", "#mobile", "#tablet"]
    ]
  }
}
```

### Multi-dataset clickable chart

```chart
{
  "type": "bar",
  "data": {
    "labels": ["Q1", "Q2", "Q3"],
    "datasets": [
      {
        "label": "2024",
        "data": [40, 55, 70],
        "backgroundColor": "#3b82f6"
      },
      {
        "label": "2025",
        "data": [50, 65, 80],
        "backgroundColor": "#10b981"
      }
    ]
  },
  "_atoll": {
    "links": [
      ["/report/2024/q1", "/report/2024/q2", "/report/2024/q3"],
      ["/report/2025/q1", "/report/2025/q2", "/report/2025/q3"]
    ]
  }
}
```

### Navigation behaviour

| URL type | Example | Behaviour |
|---|---|---|
| Relative path | `/posts/jan` | Same tab (`window.location.href`) |
| Relative path | `./detail` or `../up` | Same tab |
| External URL | `https://example.com` | New tab (`window.open`, `noopener`) |
| Not provided | `null` or missing index | No navigation; cursor stays default |

### Security

Only the following URL prefixes are permitted. All other schemes (including `javascript:`, `data:`, and `vbscript:`) are silently ignored — no navigation occurs.

- `/` — absolute path
- `./` or `../` — relative path
- `http://` or `https://` — external URL

### Accessibility

Canvas-based charts cannot expose individual elements as focusable DOM nodes. To support all users:

- Always set the `Alt` parameter (or markdown equivalent) to describe the chart and indicate it is interactive (e.g. `"Alt": "Monthly bug counts — click a bar to view details"`).
- Consider providing a supplementary data table beneath the chart with the same links for keyboard and screen reader users.

```csharp
var props = new Dictionary<string, object?>
{
    ["ConfigJson"] = configJson,
    ["Alt"] = "Monthly page views — click a bar to view that month's report",
};
```

## Supported chart types

Any chart type supported by Chart.js works:

| Type | `type` value |
|---|---|
| Bar | `bar` |
| Line | `line` |
| Pie | `pie` |
| Doughnut | `doughnut` |
| Radar | `radar` |
| Scatter | `scatter` |
| Polar area | `polarArea` |
| Bubble | `bubble` |

See the [Chart.js documentation](https://www.chartjs.org/docs/latest/) for the full configuration reference, including axes, animations, plugins, and responsive options.

## Island component

For programmatic use in Razor pages and C# components, use `ChartIsland` directly:

```csharp
using Atoll.Charts.Islands;

var props = new Dictionary<string, object?>
{
    ["ConfigJson"] = """{"type":"bar","data":{"labels":["A","B"],"datasets":[{"data":[10,20]}]}}""",
    ["Alt"] = "Sample bar chart",
    ["Width"] = 600,
    ["Height"] = 400,
};
var fragment = ComponentRenderer.ToFragment<ChartIsland>(props);
await RenderAsync(fragment);
```

### Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `ConfigJson` | `string` | *(required)* | Chart.js configuration JSON (`{ type, data, options }`) |
| `Alt` | `string?` | `null` | Accessible label (`aria-label`) for the chart |
| `Width` | `int?` | `null` | Canvas width in pixels |
| `Height` | `int?` | `null` | Canvas height in pixels |

When `Alt` is set, the wrapper `<div>` receives `role="img"` and `aria-label` for screen readers.

## How it works

The plugin has three parts:

1. **`ChartExtension`** — a Markdig `IMarkdownExtension` that registers `ChartCodeBlockRenderer`. When a fenced code block has language `chart`, the renderer validates the JSON, HTML-attribute-encodes it, and emits `<div class="atoll-chart"><canvas data-chart-config="..."></canvas><noscript>...</noscript></div>`.

2. **`ChartIsland`** — a `VanillaJsIsland` component decorated with `[ClientVisible]`. It renders the same HTML structure as the code block renderer but can be used in Razor pages with typed parameters.

3. **`chart-init.js`** — the client-side init module. When the Atoll island runtime detects a chart island entering the viewport:
   - It lazily loads `atoll-charts-vendor.min.js` (Chart.js UMD build, ~200 KB) via a dynamic `<script>` tag
   - The vendor script is loaded exactly once, even with multiple charts on the page
   - For each `<canvas data-chart-config>` element inside the island, it parses the JSON and creates a `new Chart(canvas, config)`

## Standalone usage

`Atoll.Charts` can be used independently of Lagoon. Add the extension to any Markdig pipeline:

```csharp
using Markdig;
using Atoll.Charts;

var pipeline = new MarkdownPipelineBuilder()
    .Use<ChartExtension>()
    .Build();

var html = Markdown.ToHtml(markdown, pipeline);
```

This converts `` ```chart `` blocks to `<div class="atoll-chart"><canvas data-chart-config="...">` in the HTML output. You are responsible for loading Chart.js on the page and initialising charts from the `data-chart-config` attributes.

## Security

Chart configuration JSON is HTML-attribute-encoded at build time using `HttpUtility.HtmlAttributeEncode`. Characters like `<`, `>`, `&`, and `"` are replaced with their HTML entities. This prevents attribute breakout and HTML injection.

On the client, `chart-init.js` parses the attribute value with `JSON.parse()` inside a `try/catch` block. Invalid configs are logged to the console without affecting other charts on the page.
