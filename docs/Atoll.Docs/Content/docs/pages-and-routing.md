---
title: Pages & Routing
description: Route URLs to page components with IAtollPage and PageRoute.
order: 3
section: Basics
---

# Pages & Routing

Pages are components that are directly routable to URLs. They implement `IAtollPage` and use the `[PageRoute]` attribute to declare their URL pattern.

## Creating a page

```csharp
using Atoll.Components;
using Atoll.Routing;

[PageRoute("/about")]
public sealed class AboutPage : AtollComponent, IAtollPage
{
    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<h1>About</h1>");
        WriteHtml("<p>This is the about page.</p>");
        return Task.CompletedTask;
    }
}
```

## Dynamic routes

Use `[param]` syntax in the route pattern for URL parameters:

```csharp
[PageRoute("/blog/[slug]")]
public sealed class BlogPostPage : AtollComponent, IAtollPage
{
    [Parameter(Required = true)]
    public string Slug { get; set; } = "";

    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<h1>Post: ");
        WriteText(Slug);
        WriteHtml("</h1>");
        return Task.CompletedTask;
    }
}
```

## Static paths for SSG

For static site generation, dynamic pages implement `IStaticPathsProvider` to enumerate all possible paths:

```csharp
[PageRoute("/blog/[slug]")]
public sealed class BlogPostPage : AtollComponent, IAtollPage, IStaticPathsProvider
{
    [Parameter(Required = true)]
    public string Slug { get; set; } = "";

    public Task<IReadOnlyList<StaticPath>> GetStaticPathsAsync()
    {
        var paths = new List<StaticPath>
        {
            new(new Dictionary<string, string> { ["slug"] = "hello-world" }),
            new(new Dictionary<string, string> { ["slug"] = "second-post" }),
        };
        return Task.FromResult<IReadOnlyList<StaticPath>>(paths);
    }
}
```

## Route matching

Routes are matched in specificity order. More specific patterns take precedence:

| Pattern | Matches |
|---|---|
| `/` | Exact root |
| `/about` | Exact path |
| `/blog/[slug]` | Dynamic segment |
| `/docs/[section]/[slug]` | Multiple dynamic segments |

## Registering routes

Routes are registered as `RouteEntry` objects when building or hosting the site:

```csharp
var routes = new[]
{
    new RouteEntry("/", typeof(IndexPage), ""),
    new RouteEntry("/about", typeof(AboutPage), ""),
    new RouteEntry("/blog/[slug]", typeof(BlogPostPage), ""),
};
```
