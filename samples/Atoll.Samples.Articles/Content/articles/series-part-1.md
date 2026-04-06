---
title: "Building a Plugin for Atoll, Part 1: Project Setup"
description: How to scaffold a new Atoll plugin project and wire up your first component.
pubDate: 2025-04-01
author: alice
tags: atoll, plugins, dotnet
series: Building a Plugin for Atoll
seriesOrder: 1
---

# Building a Plugin for Atoll, Part 1: Project Setup

In this series we walk through building a real Atoll plugin from scratch. Part 1 covers project scaffolding, adding the `Atoll` NuGet package, and creating your first `AtollComponent`.

## Creating the project

Start by adding a new class library targeting `net10.0`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <PackageReference Include="Atoll" Version="0.1.*" />
  </ItemGroup>
</Project>
```

## Your first component

```csharp
namespace MyPlugin.Components;

public sealed class HelloBanner : AtollComponent
{
    [Parameter(Required = true)] public string Message { get; set; } = "";

    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<div class=\"hello-banner\">");
        WriteText(Message);
        WriteHtml("</div>");
        return Task.CompletedTask;
    }
}
```

In Part 2 we'll add a full layout and a CSS theme.
