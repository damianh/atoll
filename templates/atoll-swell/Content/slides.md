---
title: My Presentation
aspectRatio: 16/9
transition: fade
slideNumbers: true
---

---
layout: cover
---

# Welcome to Atoll.Swell

**Markdown-driven slides for .NET developers**

No Node.js. No JavaScript frameworks. Just C#.

<!-- This is a presenter note for the cover slide. Use <!-- comment --> blocks at the end of each slide. -->

---

## What is Atoll.Swell?

- ✅ Write slides in **Markdown**
- ✅ Multiple built-in **layouts**
- ✅ Keyboard and touch **navigation**
- ✅ **Presenter mode** with notes and timer
- ✅ Export to **PDF, PPTX, ODP**

:::Click
- ✅ Progressive **click reveal**
:::

---
layout: two-cols
---

## Two Column Layout

Left column content goes here.

You can use **Markdown** formatting freely.

::right::

## Right Column

Right column content goes here.

```csharp
var swell = new SwellPage
{
    MarkdownContent = content
};
```

---
layout: center
---

## Centered Content

Use `layout: center` to vertically and horizontally centre your slide content.

Perfect for quotes, key statements, or transition points.

---
layout: section
---

## Part 2: Code Examples

---

## Syntax Highlighting

```csharp
// Swell uses Atoll's built-in TextMateSharp highlighting
public record SlideData(
    int Index,
    SlideConfig Config,
    string Body,
    string Notes);
```

Use fenced code blocks with language identifiers for syntax highlighting.

---
layout: end
---

# Thank You!

Questions?

*Built with [Atoll.Swell](https://github.com/damianh/atoll)*
