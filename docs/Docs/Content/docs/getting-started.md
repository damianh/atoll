---
title: Getting Started
description: Create your first Atoll project and render a page.
order: 1
section: Basics
---

# Getting Started

Atoll is a .NET-native static site framework inspired by [Astro](https://astro.build). It brings server-first HTML rendering, islands architecture, content collections, and static site generation to the .NET ecosystem.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) or later

## Create a project

```bash
dotnet new classlib -n MySite
cd MySite
```

Add the Atoll project reference:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="path/to/src/Atoll.Middleware/Atoll.Middleware.csproj" />
  </ItemGroup>
</Project>
```

## Create your first component

```csharp
using Atoll.Components;

public sealed class HelloWorld : AtollComponent
{
    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<h1>Hello, Atoll!</h1>");
        WriteHtml("<p>A .NET-native framework inspired by Astro.</p>");
        return Task.CompletedTask;
    }
}
```

## Generate a static site

```csharp
using Atoll.Build.Ssg;
using Atoll.Routing;

var options = new SsgOptions("dist") { BaseUrl = "https://example.com" };
var routes = new[] { new RouteEntry("/", typeof(IndexPage), "") };
var generator = new StaticSiteGenerator(options);
var result = await generator.GenerateAsync(routes, [typeof(IndexPage).Assembly]);
```

The output will be written to the `dist/` directory.

## What's next?

- [Components](./components) — build reusable UI
- [Pages & Routing](./pages-and-routing) — route URLs to pages
- [Layouts](./layouts) — wrap pages with shared structure
