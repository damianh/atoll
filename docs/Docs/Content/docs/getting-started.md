---
title: Getting Started
description: Create your first Atoll project and render a page.
order: 1
section: Basics
---

# Getting Started

Atoll is a .NET-native static site framework inspired by [Astro](https://astro.build). It brings server-first HTML rendering, islands architecture, content collections, and static site generation to the .NET ecosystem.

:::aside{type="tip" title="Prerequisites"}
You need the [.NET 10 SDK](https://dotnet.microsoft.com/download) or later installed before continuing.
:::

## Setup

:::steps
1. **Create a project** — Scaffold a new class library and navigate into it:

   ```bash
   dotnet new classlib -n MySite
   cd MySite
   ```

   Add the Atoll NuGet package:

   ```bash
   dotnet add package Atoll.Middleware
   ```

   Your `.csproj` should look like:

   ```xml
   <Project Sdk="Microsoft.NET.Sdk">
     <PropertyGroup>
       <TargetFramework>net10.0</TargetFramework>
     </PropertyGroup>
     <ItemGroup>
       <PackageReference Include="Atoll.Middleware" Version="0.1.*" />
     </ItemGroup>
   </Project>
   ```

2. **Create your first component** — Add a simple `AtollComponent` that renders HTML:

   ```csharp
   using Atoll.Components;

   public sealed class HelloWorld : AtollComponent
   {
       protected override Task RenderCoreAsync(RenderContext context)
       {
           WriteHtml("""
               <h1>Hello, Atoll!</h1>
               <p>A .NET-native framework inspired by Astro.</p>
               """);
           return Task.CompletedTask;
       }
   }
   ```

3. **Generate a static site** — Wire up the generator and produce output:

   ```csharp
   using Atoll.Build.Ssg;
   using Atoll.Routing;

   var options = new SsgOptions("dist") { BaseUrl = "https://example.com" };
   var routes = new[] { new RouteEntry("/", typeof(IndexPage), "") };
   var generator = new StaticSiteGenerator(options);
   var result = await generator.GenerateAsync(routes, [typeof(IndexPage).Assembly]);
   ```

   The output will be written to the `dist/` directory.
:::

## What's next?

:::card-grid{stagger=true}
:::link-card{title="Components" href="./components" description="Build reusable UI with Atoll components."}
:::
:::link-card{title="Pages & Routing" href="./pages-and-routing" description="Route URLs to pages and handle navigation."}
:::
:::link-card{title="Layouts" href="./layouts" description="Wrap pages with shared structure and chrome."}
:::
:::
