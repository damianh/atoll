---
title: Getting Started with Atoll
description: Learn how to build your first static site with the Atoll framework for .NET.
pubDate: 2026-01-15
author: Jane Developer
tags: atoll, tutorial, getting-started
draft: false
---

# Getting Started with Atoll

Atoll is a .NET-native framework inspired by [Astro](https://astro.build). It brings
server-first rendering, islands architecture, and content collections to the .NET ecosystem.

## Installation

Create a new Atoll project with the CLI:

```
atoll new my-blog
cd my-blog
atoll dev
```

## Creating Your First Page

Pages are C# classes that implement `IAtollPage`:

```csharp
[Layout(typeof(BaseLayout))]
public sealed class HomePage : AtollComponent, IAtollPage
{
    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<h1>Hello, Atoll!</h1>");
        return Task.CompletedTask;
    }
}
```

## What's Next?

- Add **layouts** to wrap your pages with consistent structure
- Create **components** for reusable UI elements
- Use **content collections** for Markdown-based content
- Add **islands** for interactive client-side features
