---
title: Content Collections Deep Dive
description: A thorough look at Atoll's content collections system for Markdown-based content.
pubDate: 2026-03-05
author: Alex Writer
tags: content-collections, markdown, frontmatter
draft: false
---

# Content Collections Deep Dive

Content collections let you organise Markdown files and query them with a type-safe API.

## Defining a Schema

Create a C# class to represent your frontmatter:

```csharp
public sealed class ArticleSchema
{
    [Required] public string Title { get; set; } = "";
    [Required] public string Description { get; set; } = "";
    [Required] public DateTime PubDate { get; set; }
    public string Author { get; set; } = "";
    public string Tags { get; set; } = "";
    public bool Draft { get; set; }
}
```

## Configuring the Collection

Implement `IContentConfiguration`:

```csharp
public sealed class ContentConfig : IContentConfiguration
{
    public CollectionConfig Configure() =>
        new CollectionConfig("Content")
            .AddCollection(ContentCollection.Define<ArticleSchema>("articles"));
}
```

## Querying Content

In a page or component, inject `CollectionQuery`:

```csharp
var entries = Query.GetCollection<ArticleSchema>("articles",
    e => !e.Data.Draft);

foreach (var entry in entries.OrderByDescending(e => e.Data.PubDate))
{
    var rendered = Query.Render(entry);
    // entry.Data.Title, entry.Slug, rendered content...
}
```

## Frontmatter Conventions

- **camelCase** YAML keys map to **PascalCase** C# properties automatically
- Dates use `YYYY-MM-DD` ISO format
- Tags are stored as a comma-separated string and split via a helper method
