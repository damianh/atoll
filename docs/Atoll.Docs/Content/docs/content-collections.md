---
title: Content Collections
description: Manage Markdown files with typed YAML frontmatter using CollectionQuery.
order: 5
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
using Atoll.Content.Collections;

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

## File provider

In production, use `PhysicalFileProvider`. In tests, use `InMemoryFileProvider`:

```csharp
// Production
var fileProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory());

// Tests
var fileProvider = new InMemoryFileProvider();
fileProvider.AddFile("Content/blog", "my-post.md", "---\ntitle: Test\n---\n# Test");
```
