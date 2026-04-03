---
title: Static Site Generation
description: Generate a complete static HTML site with StaticSiteGenerator.
order: 9
section: Advanced
---

# Static Site Generation

`StaticSiteGenerator` crawls all registered routes and generates static HTML files, a CSS bundle, and copies static assets to an output directory.

## Basic usage

```csharp
using Atoll.Build.Ssg;
using Atoll.Routing;

var options = new SsgOptions("dist")
{
    BaseUrl = "https://example.com",
};

var routes = new[]
{
    new RouteEntry("/", typeof(IndexPage), ""),
    new RouteEntry("/about", typeof(AboutPage), ""),
    new RouteEntry("/blog/[slug]", typeof(BlogPostPage), ""),
};

var generator = new StaticSiteGenerator(options);
var result = await generator.GenerateAsync(routes, [typeof(IndexPage).Assembly]);
```

## Output structure

```
dist/
├── index.html
├── about/
│   └── index.html
├── blog/
│   ├── hello-world/
│   │   └── index.html
│   └── second-post/
│       └── index.html
└── styles.css
```

## Service props injection

For pages that need `CollectionQuery` or other services, pass them as service props:

```csharp
var fileProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory());
var config = new CollectionConfig("Content")
    .AddCollection(ContentCollection.Define<BlogPostSchema>("blog"));
var loader = new CollectionLoader(config, fileProvider);
var query = new CollectionQuery(loader);

var serviceProps = new Dictionary<string, object?> { ["Query"] = query };
var generator = new StaticSiteGenerator(options, serviceProps);
```

## SsgOptions

| Property | Type | Description |
|---|---|---|
| `OutputDirectory` | `string` | Where to write output files |
| `BaseUrl` | `string` | Base URL for the site (used in sitemaps, canonical links) |
| `CleanOutput` | `bool` | Delete output dir before generating (default: `true`) |

## Build manifest

After generation, `StaticSiteGenerator` returns an `SsgResult` containing a `BuildManifest` with metadata about every generated page, including file paths and render timings.

## Static assets

Files in a `public/` directory (relative to the project root) are copied verbatim to the output directory. This is the right place for favicons, fonts, images, and other static files.
