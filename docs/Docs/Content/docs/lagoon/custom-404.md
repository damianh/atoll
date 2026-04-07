---
title: Custom 404 Pages
description: Convention-based 404 page support with custom content and full layout rendering.
order: 28
section: Lagoon Plugin
---

# Custom 404 Pages

Lagoon's `DocsPage` component supports convention-based custom 404 pages. When a requested slug is not found, the full layout pipeline (sidebar, TOC, header, footer) still renders — but with your custom 404 content and a proper HTTP 404 status code.

## How it works

When `DocsPage` cannot find the requested slug:

1. Sets `ResponseStatusCode = 404` (propagated to the HTTP response by the Atoll request handlers)
2. Looks for a content file at `Content/docs/404.md`
3. If found, renders it through the full `SiteLayout` → `DocsLayout` pipeline — the same layout used for every other docs page
4. If not found, renders a styled default fallback within the layout

The `404` slug is automatically excluded from:
- Static path generation (`GetStaticPathsAsync`) — so it is not emitted as a standalone page during SSG
- Sidebar navigation entries — the `SiteLayout` filters it out
- The search index — `SearchConfig` should skip the `NotFoundSlug` entry (see example below)

## Creating a custom 404 page

Create `Content/docs/404.md` with standard Lagoon frontmatter:

```markdown
---
title: Page Not Found
description: The requested documentation page could not be found.
order: 0
section: ""
---

# Page Not Found

Sorry, we couldn't find the page you're looking for. It may have been moved or removed.

Try using the sidebar navigation or [return to the homepage](/docs/getting-started).
```

:::aside{type="tip"}
Set `section: ""` to avoid the page being grouped under a section label in the search index or any other section-aware features.
:::

The file is treated as a regular content entry by the markdown pipeline, so you can use the full MDA format — component directives, tabs, asides, and so on.

## Excluding from the search index

The `404` slug should also be excluded from your `ISearchIndexConfiguration` to prevent it from appearing in search results:

```csharp
public IEnumerable<SearchDocumentInput> GetDocuments(CollectionQuery query)
{
    var docs = query.GetCollection<DocSchema>("docs");
    foreach (var entry in docs)
    {
        if (entry.Slug == DocsPage.NotFoundSlug)
        {
            continue;
        }

        // ... yield search documents as normal
    }
}
```

## Framework support: `IPageStatusCodeProvider`

`IPageStatusCodeProvider` is an optional interface in `Atoll.Components` that any page component can implement to signal a non-200 HTTP status code:

```csharp
namespace Atoll.Components;

public interface IPageStatusCodeProvider
{
    int ResponseStatusCode { get; }
}
```

After a page renders, `AtollRequestHandler` and `DevAtollRequestHandler` check whether the component implements `IPageStatusCodeProvider`. If it does, `ResponseStatusCode` is read and written to the HTTP response instead of the default 200.

This is how `DocsPage` returns a real 404 to the browser:

```csharp
public sealed class DocsPage : AtollComponent, IAtollPage, IPageStatusCodeProvider
{
    public int ResponseStatusCode { get; private set; } = 200;

    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var entry = Query.GetEntry<DocSchema>("docs", Slug);

        if (entry is null)
        {
            ResponseStatusCode = 404;
            // ... render custom 404 content or fallback
            return;
        }

        // ... render the found page
    }
}
```

`PageRenderResult` also carries a `StatusCode` property (default 200) that `PageRenderer` populates from the component after rendering.
