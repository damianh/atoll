---
title: Working with Content Collections
description: Learn how to use Atoll's content collections for type-safe Markdown content with frontmatter validation.
pubDate: 2026-03-05
author: John Writer
tags: atoll, content, markdown
draft: false
---

# Working with Content Collections

Content collections bring type-safety to your Markdown content. Define a schema,
write your content in Markdown with YAML frontmatter, and Atoll validates and
types everything for you.

## Defining a Schema

Create a C# class with DataAnnotation attributes:

```csharp
public sealed class BlogPostSchema
{
    [Required]
    public string Title { get; set; } = "";

    [Required]
    public string Description { get; set; } = "";

    [Required]
    public DateTime PubDate { get; set; }

    public string Author { get; set; } = "";
    public string Tags { get; set; } = "";
}
```

## Querying Content

Use `CollectionQuery` to load and render entries:

```csharp
var posts = query.GetCollection<BlogPostSchema>("blog");
var entry = query.GetEntry<BlogPostSchema>("blog", "my-post");
var rendered = query.Render(entry);
```

## Rendering Content

The rendered Markdown becomes a `ContentComponent` that you can compose
with other Atoll components:

```csharp
var contentComponent = ContentComponent.FromRenderedContent(rendered);
await RenderAsync(contentComponent.ToRenderFragment());
```

Content collections make it easy to build blogs, documentation sites,
and any content-heavy application with full type safety.
