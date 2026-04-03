---
title: Getting Started with Atoll
description: Learn how to build your first static site with the Atoll framework for .NET.
pubDate: 2026-01-15
author: Blog Author
tags: atoll, tutorial, getting-started
draft: false
---

# Getting Started with Atoll

Atoll is a .NET-native framework inspired by [Astro](https://astro.build). It brings
server-first rendering, islands architecture, and content collections to the .NET ecosystem.

## Creating Pages

Pages are C# classes that implement `IAtollPage`:

```csharp
[Layout(typeof(BlogLayout))]
[PageRoute("/my-page")]
public sealed class MyPage : AtollComponent, IAtollPage
{
    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<h1>Hello, Atoll!</h1>");
        return Task.CompletedTask;
    }
}
```

## Content Collections

Content collections let you write blog posts in Markdown with validated frontmatter:

```csharp
var posts = Query.GetCollection<BlogPostSchema>("blog");
```

## Islands Architecture

Add interactivity with island components — only the interactive parts load JavaScript:

```csharp
[ClientLoad]
public sealed class MyIsland : VanillaJsIsland
{
    public override string ClientModuleUrl => "/scripts/my-island.js";
    // ...
}
```

## What's Next?

- Customize the `BlogLayout` in `Layouts/BlogLayout.cs`
- Add more pages in the `Pages/` directory
- Create new blog posts in `Content/blog/`
- Explore the [Atoll documentation](https://github.com/damianh/atoll) for advanced features
