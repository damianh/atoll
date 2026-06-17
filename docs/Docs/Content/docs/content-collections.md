---
title: Content Collections
description: Manage Markdown files with typed YAML frontmatter using CollectionQuery.
order: 6
section: Features
---

# Content Collections

Content collections let you manage Markdown files with typed YAML frontmatter. Define a schema class, put your `.md` files in a directory, and query them with `CollectionQuery`.

## Define a frontmatter schema

```csharp
using System.ComponentModel.DataAnnotations;

public sealed class BlogPostSchema
{
    [Required]
    public string Title { get; set; } = "";

    [Required]
    public string Description { get; set; } = "";

    [Required]
    public DateTime PubDate { get; set; }

    public string Author { get; set; } = "";
    public bool Draft { get; set; }
}
```

## Declare a collection

Implement `IContentConfiguration` to register your collections:

```csharp
using Atoll.Build.Content.Collections;

public sealed class ContentConfig : IContentConfiguration
{
    public CollectionConfig Configure()
    {
        return new CollectionConfig("Content")
            .AddCollection(ContentCollection.Define<BlogPostSchema>("blog"));
    }
}
```

The base directory (`"Content"`) is relative to the project root. The collection name (`"blog"`) maps to a subdirectory: `Content/blog/`.

## Query entries

```csharp
// All non-draft posts
var posts = query.GetCollection<BlogPostSchema>("blog",
    entry => !entry.Data.Draft);

// Sort by date
var sorted = posts
    .OrderByDescending(p => p.Data.PubDate)
    .ToList();

// Get a single entry by slug
var entry = query.GetEntry<BlogPostSchema>("blog", "my-post-slug");
```

## Render Markdown to HTML

```csharp
var rendered = query.Render(entry);  // Returns RenderedContent
// rendered.Html — the full HTML string
// rendered.Headings — extracted headings for a TOC

// Render inside a component:
var contentComponent = ContentComponent.FromRenderedContent(rendered);
await RenderAsync(contentComponent.ToRenderFragment());
```

## Frontmatter format

Markdown files use YAML frontmatter delimited by `---`:

```markdown
---
title: My Post
description: A short description.
pubDate: 2026-01-15
author: Jane Developer
draft: false
---

# My Post

Content goes here...
```

Any property in your schema class can appear in frontmatter. For example, the Lagoon theme's `DocSchema` supports a `head:` field for per-page `<head>` injection — use a YAML literal block (`|`) for multi-line values:

```markdown
---
title: My Page
description: A page with custom head content.
head: |
  <meta property="og:title" content="My Page">
  <script src="/analytics.js"></script>
---
```

See [Lagoon Configuration](./lagoon/configuration) for details on the `head:` field.

## File provider

In production, use `PhysicalFileProvider`. In tests, use `InMemoryFileProvider`:

```csharp
// Production
var fileProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory());

// Tests
var fileProvider = new InMemoryFileProvider();
fileProvider.AddFile("Content/blog", "my-post.md", "---\ntitle: Test\n---\n# Test");
```

## Custom collection directories

By default, each collection maps to a subdirectory under the base directory — `ContentCollection.Define<T>("blog")` reads from `Content/blog/`. You can override this with `.FromDirectory()` to point a collection at any path relative to the project root:

```csharp
public sealed class ContentConfig : IContentConfiguration
{
    public CollectionConfig Configure()
    {
        return new CollectionConfig("Content")
            // Standard collection — reads from Content/blog/
            .AddCollection(ContentCollection.Define<BlogPostSchema>("blog"))
            // Custom directory — reads from a sibling package
            .AddCollection(ContentCollection.Define<DocSchema>("weather-api-docs")
                .FromDirectory("../../libs/weather-api/docs/content"));
    }
}
```

This is useful in monorepo scenarios where documentation lives alongside the component it describes, rather than in a centralised docs directory:

```
repo/
├── docs/
│   └── site/                    ← Atoll project root
│       ├── Content/
│       │   └── blog/            ← standard collection
│       └── ContentConfig.cs
└── libs/
    └── weather-api/
        └── docs/
            └── content/         ← custom collection directory
                ├── overview.md
                └── configuration.md
```

The collection name (`"weather-api-docs"`) is the logical identifier used in `query.GetCollection<T>()` and `query.GetEntry<T>()` calls — it does not need to match the directory name.

## URL prefix

By default, a collection's URL prefix is `/{collectionName}` — entries in a collection named `"blog"` are expected at `/blog/{slug}`. You can override this with `.WithPrefix()`:

```csharp
return new CollectionConfig("Content")
    // Entries accessible at /guides/{slug}
    .AddCollection(ContentCollection.Define<DocSchema>("guides")
        .WithPrefix("/guides"))
    // Entries accessible at the root: /{slug}
    .AddCollection(ContentCollection.Define<DocSchema>("docs")
        .WithPrefix("/"));
```

Use `query.GetCollectionPrefix("collectionName")` in layouts and pages to build hrefs without hardcoding paths:

```csharp
var prefix = Query.GetCollectionPrefix("docs").TrimEnd('/');
var entries = Query.GetCollection<DocSchema>("docs")
    .Select(e => new SidebarEntry(e.Data.Title, $"{prefix}/{e.Slug}", ...))
    .ToList();
```

This is particularly useful when a site has multiple collections that each need their own URL namespace, or when using `.FromDirectory()` to pull content from external paths that should be served under a specific URL prefix.

### Dev server

When using `atoll dev`, custom collection directories are automatically watched for `.md` changes. Edits to content files in external directories trigger the same content-only hot-reload as changes in the standard content directory.
