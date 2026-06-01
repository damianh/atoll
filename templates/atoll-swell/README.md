# My Slides

A slide deck project powered by [Atoll.Swell](https://github.com/damianh/atoll).

## Getting Started

```bash
dotnet run
```

Then open [http://localhost:4321](http://localhost:4321) in your browser.

## Editing Slides

Edit `Content/slides.md`. The slide deck is separated by `\n\n---\n\n` (blank lines required).

### Layouts

Set per-slide layout in the frontmatter block:

```markdown
---
layout: cover
---
```

Available layouts: `default`, `cover`, `center`, `two-cols`, `image-right`, `image-left`, `section`, `end`.

### Keyboard Shortcuts

| Key | Action |
|-----|--------|
| `→` / `Space` | Next slide |
| `←` | Previous slide |
| `o` | Overview grid |
| `f` | Fullscreen |
| `p` | Presenter mode |
| `Escape` | Exit overview / fullscreen |

## Building

```bash
atoll build
```

Output is written to `dist/`.
