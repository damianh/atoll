---
title: Swell Overview
description: Create presentation slide decks from Markdown with keyboard navigation, presenter mode, drawing annotations, and export to PDF, PPTX, and ODP.
order: 36
section: Swell Plugin
---

# Swell Overview

Swell is a presentation slide deck plugin for Atoll. Write your slides in Markdown, get a full-featured presentation with keyboard navigation, presenter mode, live drawing, click-reveal animations, and export to PDF, PPTX, and ODP --- all without Node.js or JavaScript frameworks.

## Live example

The deck below is rendered from Markdown using Swell. Use arrow keys or click to navigate. Press **o** for slide overview, **p** for presenter mode, or **d** to draw.

<SwellDeck src="/swell/example-slides" title="Swell Example Deck" AspectRatio="16/9" />

| Feature | Description |
|---|---|
| **Markdown authoring** | Write slides in standard Markdown with YAML frontmatter |
| **8 built-in layouts** | Cover, center, section, end, two-cols, image-left, image-right, default |
| **Keyboard navigation** | Arrow keys, Space, PageUp/Down, plus overview and fullscreen modes |
| **Presenter mode** | Separate window with current slide, next preview, notes, and timers |
| **Click reveal** | Progressive content reveal with the `:::Click` directive |
| **Drawing overlay** | Freehand annotations during presentation (press `d`) |
| **Transitions** | Fade, slide-left, slide-right, slide-up, or none |
| **Export** | PDF, PPTX, and ODP via Playwright screenshots and OpenXml |
| **Theming** | CSS custom properties with pluggable `ISwellTheme` interface |
| **Syntax highlighting** | Fenced code blocks with language-aware highlighting |
| **Accessibility** | ARIA roles, live regions, skip links, semantic HTML |

## Quick start

### 1. Create a new project from the template

```bash
dotnet new atoll-swell -n MyPresentation
cd MyPresentation
dotnet run
```

Open `http://localhost:4321` to see your slide deck.

### 2. Or add to an existing project

Add the `Atoll.Swell` NuGet package:

```bash
dotnet add package Atoll.Swell
```

Or add directly to your `.csproj`:

```xml
<PackageReference Include="Atoll.Swell" Version="0.1.*" />
```

Create a page that renders your Markdown as a slide deck:

```csharp
using Atoll.Components;
using Atoll.Rendering;
using Atoll.Routing;
using Atoll.Swell.Components;

[PageRoute("/slides")]
public sealed class SlidesPage : AtollComponent, IAtollPage
{
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var slidePath = Path.Combine(
            Directory.GetCurrentDirectory(), "Content", "slides.md");
        var content = await File.ReadAllTextAsync(slidePath);

        var props = new Dictionary<string, object?>
        {
            [nameof(SwellPage.MarkdownContent)] = content,
        };

        await ComponentRenderer.RenderComponentAsync<SwellPage>(
            context.Destination, props);
    }
}
```

## Writing slides

Slide decks are plain Markdown files with three parts:

1. **Deck headmatter** --- YAML at the top configuring the entire deck
2. **Slide separators** --- a line of `---` surrounded by blank lines
3. **Per-slide frontmatter** --- optional YAML at the start of each slide chunk

```markdown
---
title: My Presentation
aspectRatio: 16/9
transition: fade
slideNumbers: true
---

---
layout: cover
---

# Welcome

Subtitle text here.

<!-- Presenter notes go in HTML comments at the end of a slide. -->

---

## Slide Two

Regular content with **Markdown** formatting.

:::Click
- This appears on the first click
:::

:::Click
- This appears on the second click
:::

---
layout: two-cols
---

## Left Column

Content on the left.

::right::

## Right Column

Content on the right.

---
layout: end
---

# Thank You!
```

> **Note:** The slide separator must have blank lines on both sides (`\n\n---\n\n`). A bare `---` without surrounding blank lines is treated as a Markdown horizontal rule.

## Deck configuration

YAML frontmatter at the top of the file configures deck-level settings.

| Property | Type | Default | Description |
|---|---|---|---|
| `title` | `string` | `""` | Browser tab title and cover slide heading |
| `aspectRatio` | `string` | `16/9` | Slide aspect ratio: `16/9`, `4/3`, or `3/2` |
| `transition` | `string` | `none` | Default transition: `fade`, `slide-left`, `slide-right`, `slide-up`, or `none` |
| `slideNumbers` | `bool` | `true` | Show slide number in bottom-right corner |
| `theme` | `string` | `"default"` | Theme name |
| `export` | `string[]` | `[]` | Build-time export formats: `pdf`, `pptx`, `odp` |

## Slide configuration

Each slide can override deck defaults with its own YAML frontmatter.

| Property | Type | Default | Description |
|---|---|---|---|
| `layout` | `string` | `"default"` | Layout name (see below) |
| `background` | `string?` | `null` | CSS background value (colour or image URL) |
| `class` | `string?` | `null` | Additional CSS classes on the slide element |
| `transition` | `string?` | deck default | Transition override for this slide |
| `slideNumber` | `bool?` | deck default | Show/hide slide number for this slide |

## Layouts

Eight built-in layouts control how slide content is arranged.

| Layout | Description |
|---|---|
| `default` | Standard vertical flow with title and body |
| `cover` | Hero slide with centred content on a gradient background |
| `center` | Content centred both vertically and horizontally |
| `section` | Section divider with large centred title and accent border |
| `end` | Closing slide with centred content |
| `two-cols` | Two equal columns split by `::right::` delimiter |
| `image-right` | Content on the left, image on the right |
| `image-left` | Image on the left, content on the right |

Set the layout in per-slide frontmatter:

```markdown
---
layout: cover
---

# Opening Slide
```

### Two-column layout

Use the `::right::` delimiter to split content between columns:

```markdown
---
layout: two-cols
---

## Left Side

Left column content.

::right::

## Right Side

Right column content.
```

## Keyboard shortcuts

| Key | Action |
|---|---|
| `->` `Space` `Down` `PageDown` | Next slide (or reveal next click block) |
| `<-` `Up` `PageUp` | Previous slide (or hide last click block) |
| `o` | Toggle overview grid |
| `f` | Toggle fullscreen |
| `p` | Open presenter mode |
| `d` | Toggle drawing overlay |
| `Escape` | Exit overview, fullscreen, or drawing |

## Click reveal

Wrap content in `:::Click` directives for progressive reveal. Each block appears on successive key presses:

```markdown
:::Click
- First point (appears on first click)
:::

:::Click
- Second point (appears on second click)
:::
```

Click state is tracked per slide and resets when navigating away.

## Presenter mode

Press `p` to open a presenter window with:

- **Current slide** (large preview)
- **Next slide** (small preview)
- **Speaker notes** (from `<!-- comment -->` blocks)
- **Elapsed timer** (time since presenter mode opened)
- **Wall clock**

Navigation in the presenter window stays synchronised with the main window via `BroadcastChannel`.

## Drawing overlay

Press `d` to activate a freehand drawing canvas over the current slide. Annotations are drawn in red and cleared automatically when navigating to a new slide.

## Embedding a deck

Use the `SwellDeck` component to embed a slide deck as an iframe in any Atoll page:

```csharp
var props = new Dictionary<string, object?>
{
    ["Src"] = "/slides/",
    ["Title"] = "My Presentation",
    ["AspectRatio"] = "16/9",
};
await ComponentRenderer.RenderComponentAsync<SwellDeck>(
    context.Destination, props);
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Src` | `string` | *(required)* | URL of the slide deck page |
| `Title` | `string` | `"Slide deck"` | Accessible iframe title |
| `AspectRatio` | `string` | `"16/9"` | CSS aspect-ratio for the container |

## Export

Swell can export decks to PDF, PPTX, and ODP at build time. Add the desired formats to the deck headmatter:

```yaml
export: [pdf, pptx, odp]
```

Add the `Atoll.Swell.Export` NuGet package:

```bash
dotnet add package Atoll.Swell.Export
```

Export uses Playwright to capture slide screenshots at the configured aspect ratio, then assembles them into the target format:

| Format | Method | Notes |
|---|---|---|
| **PDF** | Print-to-PDF via `@media print` styles | A4 landscape, one slide per page |
| **PPTX** | Screenshot images embedded via OpenXml | Speaker notes included |
| **ODP** | Screenshot images in OpenDocument ZIP | Speaker notes included |

> **Note:** Playwright requires Chromium browsers to be installed. Run `pwsh playwright.ps1 install chromium` before exporting.

## Theming

Swell uses CSS custom properties for styling. The default theme provides these tokens:

| Token | Default | Description |
|---|---|---|
| `--swell-bg-outer` | `#1a1a2e` | Outer background (letterbox area) |
| `--swell-bg-slide` | `#ffffff` | Slide background |
| `--swell-bg-cover` | Gradient blue | Cover layout background |
| `--swell-bg-section` | `#f0f4ff` | Section layout background |
| `--swell-text` | `#1a1a2e` | Primary text colour |
| `--swell-accent` | `#e94560` | Accent colour (links, borders, drawing) |
| `--swell-transition-duration` | `400ms` | Transition animation duration |

### Custom themes

Implement the `ISwellTheme` interface to create a custom theme:

```csharp
using Atoll.Swell.Configuration;

public class DarkTheme : ISwellTheme
{
    public string Name => "dark";

    public string? AdditionalCss => """
        :root {
            --swell-bg-slide: #1e1e2e;
            --swell-text: #cdd6f4;
            --swell-accent: #89b4fa;
        }
        """;

    public Type? ResolveLayoutOverride(string layoutName) => null;
}
```

Reference the theme in your deck headmatter:

```yaml
theme: dark
```

## Markdown features

Swell supports the full Atoll Markdown pipeline:

- Tables, task lists, strikethrough, subscript/superscript
- Fenced code blocks with syntax highlighting (TextMateSharp)
- Auto-generated heading IDs
- Auto-links
- Mermaid diagrams (when `Atoll.Mermaid` is installed)
- Content components via `:::directive` syntax
