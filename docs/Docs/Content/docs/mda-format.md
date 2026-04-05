---
title: MDA Format
description: Atoll's .mda file format — standard Markdown with YAML frontmatter and component directives.
order: 5
section: Features
---

# MDA Format

`.mda` (Markdown Atoll) is Atoll's content file format. It extends standard Markdown with YAML frontmatter and `:::` component directives, while remaining plain-text and editor-friendly.

`.md` files continue to work identically — `.mda` is an opt-in extension for projects that want to distinguish Atoll content from plain Markdown.

## Why `.mda`?

Atoll content files use the same Markdown + frontmatter + directive syntax regardless of extension. The `.mda` extension communicates intent:

| Extension | Meaning |
|---|---|
| `.md` | Standard Markdown (any tool) |
| `.mdx` | Markdown + JSX (Astro, Next.js) |
| `.mda` | Markdown + Atoll directives |

Using `.mda` disambiguates your content files and signals that they may contain Atoll-specific `:::` component directives.

## File structure

An `.mda` file has three parts:

```markdown
---
title: My Page
description: A short summary.
pubDate: 2026-01-15
---

# My Page

Standard Markdown content here.

:::CalloutBox{type=info}
This is rendered by the `CalloutBox` component.
:::
```

1. **YAML frontmatter** — delimited by `---`, maps to your schema class
2. **Markdown body** — standard CommonMark
3. **Component directives** — `:::ComponentName{prop=value}` blocks (optional)

## Component directives

Directives embed registered components inside Markdown content. The syntax follows the [Markdown directives proposal](https://talk.commonmark.org/t/generic-directives-plugins-syntax/444):

```markdown
:::ComponentName{prop="value" flag}
Optional child content rendered into the default slot.
:::
```

- **Name** — matches a registered component by name (case-sensitive)
- **Attributes** — quoted strings or bare values; boolean flags without a value are `true`
- **Child content** — rendered as Markdown and passed to `RenderSlotAsync()`

### Example

```csharp
public sealed class CalloutBox : AtollComponent
{
    [Parameter(Required = true)]
    public string Type { get; set; } = "info";

    protected override async Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml($"<div class=\"callout callout-{Type}\">");
        await RenderSlotAsync();   // renders child Markdown
        WriteHtml("</div>");
    }
}
```

```markdown
:::CalloutBox{type=warning}
Be careful — this action cannot be undone.
:::
```

## Register components

Register components in your `IContentConfiguration` implementation using `ComponentMap`:

```csharp
using Atoll.Build.Content.Collections;
using Atoll.Build.Content.Markdown;

public sealed class ContentConfig : IContentConfiguration
{
    public CollectionConfig Configure()
    {
        return new CollectionConfig("Content")
            .AddCollection(ContentCollection.Define<BlogPostSchema>("blog"))
            .WithComponentMap(map => map
                .Add<CalloutBox>()
                .Add<CodeSandbox>());
    }
}
```

Components must be registered before any Markdown is rendered. Unrecognised directive names produce an empty output without throwing.

## Rendering

Render `.mda` content the same way as `.md` — via `CollectionQuery`:

```csharp
// Get a single entry
var entry = query.GetEntry<BlogPostSchema>("blog", "my-post");

// Render Markdown + directives to HTML
var rendered = query.Render(entry);

// Use in a component
var contentComponent = ContentComponent.FromRenderedContent(rendered);
await RenderAsync(contentComponent.ToRenderFragment());
```

`rendered.Html` contains the full rendered output, including any components resolved from directives.

## Island directives

Components decorated with a client directive attribute (`[ClientLoad]`, `[ClientVisible]`, etc.) hydrate on the client when rendered inside `.mda` content:

```csharp
[ClientLoad]
public sealed class LiveCounter : VanillaJsIsland
{
    public override string ClientModuleUrl => "/scripts/counter.js";

    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<button id=\"counter\">Count: 0</button>");
        return Task.CompletedTask;
    }
}
```

```markdown
:::LiveCounter{}
:::
```

The island is server-rendered to static HTML and hydrated on the client exactly as if it were used directly in a component. See [Islands](./islands) for the full client directive reference.

## Link resolution

Links to other `.mda` files have their extension stripped automatically, producing clean URLs:

```markdown
[Introduction](./guides/intro.mda)   <!-- rendered as href="/docs/guides/intro/" -->
[Reference](./api-reference.md)      <!-- rendered as href="/docs/api-reference/" -->
```

Both `.md` and `.mda` extensions are stripped. No configuration is required — this is the default behaviour of `LinkResolutionOptions`.

## File provider

Both `PhysicalFileProvider` and `InMemoryFileProvider` discover `*.mda` files alongside `*.md`. When both `slug.md` and `slug.mda` exist in the same directory, `.md` takes priority.

```csharp
// Production — discovers both *.md and *.mda
var fileProvider = new PhysicalFileProvider();

// Tests
var fileProvider = new InMemoryFileProvider()
    .AddFile("Content/blog", "my-post.mda", "---\ntitle: Test\n---\n# Test");
```

See [Content Collections](./content-collections) for the full loading and querying API.
