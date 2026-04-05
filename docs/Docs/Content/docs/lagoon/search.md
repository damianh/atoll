---
title: Site Search
description: Build-time search index generation and client-side search dialog.
order: 25
section: Lagoon Plugin
---

# Site Search

Lagoon provides fully static search: the `atoll build` CLI generates a `search-index.json` file at build time, and the `SearchDialog` island loads it on the client for fast, zero-backend search.

## How it works

1. **Implement `ISearchIndexConfiguration`** in your project to declare which content entries to index.
2. **`atoll build`** discovers your implementation, renders each entry, and writes `search-index.json` to the output directory.
3. **`SearchDialog`** fetches the JSON lazily on first open and performs client-side full-text search.

## Implementing `ISearchIndexConfiguration`

Create a class that implements `ISearchIndexConfiguration`. The `atoll build` CLI scans your assembly for it automatically — no registration required.

```csharp
// SearchConfig.cs
using Atoll.Build.Content.Collections;
using Atoll.Lagoon.Search;

public sealed class SearchConfig : ISearchIndexConfiguration
{
    public IEnumerable<SearchDocumentInput> GetDocuments(CollectionQuery query)
    {
        var docs = query.GetCollection<DocSchema>("docs");
        foreach (var entry in docs)
        {
            var rendered = query.Render(entry);
            yield return new SearchDocumentInput(entry.Data.Title, $"/docs/{entry.Slug}")
            {
                Description = entry.Data.Description,
                Section     = entry.Data.Section.Length > 0 ? entry.Data.Section : null,
                HtmlBody    = rendered.Html,
            };
        }
    }
}
```

## `SearchDocumentInput` properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Title` | `string` | *(required)* | Document title shown in results |
| `Href` | `string` | *(required)* | URL path for the result link |
| `Description` | `string?` | `null` | Short description shown under the title in results |
| `Section` | `string?` | `null` | Section label (e.g. sidebar group name) shown in results |
| `HtmlBody` | `string` | `""` | Raw HTML body; HTML tags are stripped to produce the searchable plain-text excerpt |
| `Headings` | `IReadOnlyList<string>` | `[]` | Pre-extracted headings; if empty, parsed from `HtmlBody` |
| `PlainBody` | `string?` | `null` | Plain-text override; when set, `HtmlBody` is ignored for body text |
| `MaxBodyLength` | `int` | `500` | Maximum plain-text body length in characters (truncated if exceeded) |

## Search index processing

When building the index, `SearchIndexBuilder`:

1. Strips all HTML tags from `HtmlBody` (or uses `PlainBody` if provided)
2. Collapses whitespace to single spaces
3. Truncates the result to `MaxBodyLength` characters
4. Extracts headings from `<h1>`–`<h6>` elements if `Headings` is empty
5. Writes the complete index as `search-index.json` via `SearchIndexWriter`

The resulting JSON is an array of entries, each with `title`, `href`, `description`, `section`, `headings`, and `body` fields.

## `SearchDialog` island

`SearchDialog` provides the search UI — a trigger button and a `<dialog>` overlay. It is already included in `DocsLayout`; you do not need to add it yourself.

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Placeholder` | `string` | `"Search docs..."` | Placeholder text in the search input and trigger button |
| `IndexUrl` | `string` | `"/search-index.json"` | URL to fetch the search index from |

The dialog opens on button click, `Ctrl+K` (Windows/Linux), or `⌘K` (macOS). Results support arrow-key navigation; `Enter` navigates to the selected result. `Escape` closes the dialog.

## Base path support

When your site is hosted at a sub-path (e.g. `https://example.com/docs/`), the search index is written to `dist/search-index.json` but served at `/docs/search-index.json`.

`DocsLayout` renders `SearchDialog` with an empty parameter dictionary and does **not** automatically prepend `BasePath` to `IndexUrl`. The workaround is to render `SearchDialog` yourself in a custom layout instead of delegating to `DocsLayout`, passing `IndexUrl` explicitly:

```csharp
await RenderAsync(ComponentRenderer.ToFragment<SearchDialog>(new Dictionary<string, object?>
{
    ["IndexUrl"] = $"{config.BasePath}/search-index.json",
}));
```

> **Note:** Automatic `BasePath`-prefixing for the search index is a known gap. See [Starlight Comparison](./starlight-comparison) for the full list of functional differences.
