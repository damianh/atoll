using Atoll.Components;
using Atoll.Routing;
using Atoll.Swell.Components;

namespace Docs.Pages;

/// <summary>
/// Renders an example Swell slide deck at /swell/example-slides.
/// This page is embedded as an iframe via SwellDeck on the Swell overview docs page.
/// </summary>
[PageRoute("/swell/example-slides")]
public sealed class SwellExamplePage : AtollComponent, IAtollPage
{
    private const string ExampleMarkdown =
"""
---
title: Building with Swell
aspectRatio: 16/9
transition: fade
slideNumbers: true
---

---
layout: cover
---

# Building with Swell

Presentation slides from Markdown — no JavaScript frameworks required.

<!-- Welcome! This example deck demonstrates every major Swell feature: layouts, click reveal, transitions, rich Markdown, code blocks, tables, and presenter notes. -->

---

## Why Swell?

Traditional slide tools force you into a **GUI** or a **JavaScript toolchain**.
Swell takes a different approach:

- Write slides in **plain Markdown** with YAML frontmatter
- Renders to semantic HTML with **zero client-side frameworks**
- Full **keyboard navigation**, presenter mode, and drawing overlay
- Export to **PDF**, **PPTX**, and **ODP** at build time

> *"The best presentation tool is the one that stays out of your way."*

<!-- Presenter notes: emphasise that Swell is server-rendered. No React, no Vue, no Svelte. Just Markdown in, HTML out. -->

---

---
layout: section
---

## Layouts & Styling

Swell ships with 8 built-in layouts.

---

---
layout: center
---

### The `center` Layout

Content is centred **both vertically and horizontally**.

Use it for impactful single statements,
key takeaways, or dramatic pauses.

---

---
layout: two-cols
---

## Two-Column Layout

Split content with the `::right::` delimiter.

**Left column** is great for explanations,
context, or bullet points.

- Explain the *what*
- Describe the *why*
- Show the *how* →

::right::

## Code on the Right

```csharp
[PageRoute("/slides")]
public sealed class SlidesPage
    : AtollComponent, IAtollPage
{
    protected override async Task
        RenderCoreAsync(RenderContext ctx)
    {
        var md = await File
            .ReadAllTextAsync("slides.md");

        await ComponentRenderer
            .RenderComponentAsync<SwellPage>(
                ctx.Destination,
                new Dictionary<string, object?>
                {
                    ["MarkdownContent"] = md,
                });
    }
}
```

<!-- The two-cols layout is ideal for code walkthroughs. Put the narrative on the left and the code on the right. -->

---

---
layout: section
---

## Rich Markdown

Tables, task lists, code blocks, and more.

---

## Tables & Formatted Text

Swell supports the **full Atoll Markdown pipeline**:

| Feature | Syntax | Rendered |
|---|---|---|
| **Bold** | `**bold**` | **bold** |
| *Italic* | `*italic*` | *italic* |
| ~~Strikethrough~~ | `~~strike~~` | ~~strike~~ |
| `Inline code` | `` `code` `` | `code` |
| [Links](https://github.com/damianh/atoll) | `[text](url)` | clickable |

### Task Lists

- [x] Markdown authoring
- [x] Keyboard navigation
- [x] Presenter mode
- [ ] World domination

<!-- Tables render with full styling. Task lists use standard GitHub-flavoured Markdown checkbox syntax. -->

---

## Syntax Highlighting

Fenced code blocks with language-aware highlighting:

```yaml
# Deck configuration (YAML frontmatter)
title: My Presentation
aspectRatio: 16/9
transition: slide-left
slideNumbers: true
export: [pdf, pptx]
```

```html
<!-- Embed a deck in any Atoll page -->
<SwellDeck
  src="/slides/"
  title="My Talk"
  AspectRatio="16/9" />
```

---

---
layout: section
---

## Interactive Features

Click reveal, transitions, drawing, and presenter mode.

---

## Click Reveal

Build up your argument one step at a time:

:::Click
**Step 1** — Define your slide content in Markdown
:::

:::Click
**Step 2** — Add `:::Click` directives around each block
:::

:::Click
**Step 3** — Each block appears on successive key presses
:::

:::Click
**Step 4** — Navigate back to hide blocks in reverse order
:::

<!-- Click reveal is great for keeping the audience focused on one point at a time. State resets when you navigate away from the slide. -->

---

---
transition: slide-left
---

## Per-Slide Transitions

This slide uses `transition: slide-left` — overriding the deck default of `fade`.

Available transitions:

| Transition | Effect |
|---|---|
| `fade` | Cross-fade between slides |
| `slide-left` | Slide in from the right |
| `slide-right` | Slide in from the left |
| `slide-up` | Slide in from below |
| `none` | Instant switch |

---

---
transition: slide-up
---

## And This One Slides Up

Set `transition` in per-slide frontmatter to override the deck default for any individual slide.

```markdown
---
transition: slide-up
---

## My Slide Content
```

---

---
layout: two-cols
---

## Keyboard Shortcuts

| Key | Action |
|---|---|
| `→` `Space` `↓` | Next slide / reveal |
| `←` `↑` | Previous slide |
| `o` | Overview grid |
| `f` | Fullscreen |
| `p` | Presenter mode |
| `d` | Drawing overlay |
| `Esc` | Exit mode |

::right::

## Export Formats

Add to your deck frontmatter:

```yaml
export: [pdf, pptx, odp]
```

| Format | Method |
|---|---|
| **PDF** | Print-to-PDF |
| **PPTX** | Screenshots + OpenXml |
| **ODP** | Screenshots + ODP ZIP |

Speaker notes are preserved in PPTX and ODP exports.

---

---
layout: center
background: "linear-gradient(135deg, #0f3460 0%, #1a1a2e 100%)"
class: dark-slide
---

### Custom Backgrounds

Set `background` in slide frontmatter to any CSS value — colours, gradients, or images.

```yaml
background: "linear-gradient(135deg, #0f3460, #1a1a2e)"
```

---

---
layout: end
---

# Get Started

```bash
dotnet new atoll-swell -n MyPresentation
cd MyPresentation
dotnet run
```

Press **o** for overview · **p** for presenter mode · **d** to draw

<!-- Thanks for exploring the example deck! Visit the Swell docs for the full reference. -->
""";

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var props = new Dictionary<string, object?>
        {
            [nameof(SwellPage.MarkdownContent)] = ExampleMarkdown,
        };

        await ComponentRenderer.RenderComponentAsync<SwellPage>(
            context.Destination, props);
    }
}
