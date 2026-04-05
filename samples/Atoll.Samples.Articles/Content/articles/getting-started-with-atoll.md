---
title: Getting Started with Atoll
description: Learn how to build your first static site with the Atoll framework for .NET.
pubDate: 2026-01-15
author: Jane Developer
tags: atoll, tutorial, getting-started
draft: false
---

# Getting Started with Atoll

Atoll is a .NET-native static site framework inspired by Astro. It brings server-first rendering,
content collections, and the islands architecture to the .NET ecosystem.

## Installation

Create a new Atoll project:

```
atoll new my-site
cd my-site
atoll dev
```

## Creating a Page

Pages are C# classes that implement `IAtollPage`:

```csharp
[Layout(typeof(MyLayout))]
[PageRoute("/hello")]
public sealed class HelloPage : AtollComponent, IAtollPage
{
    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<h1>Hello, Atoll!</h1>");
        return Task.CompletedTask;
    }
}
```

## Next Steps

- Define a **content collection** to organise your Markdown files
- Create **components** for reusable HTML fragments
- Add **islands** for client-side interactivity
