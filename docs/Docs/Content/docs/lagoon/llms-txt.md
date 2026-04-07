---
title: LLM Content Export
description: Generate llms.txt and llms-full.txt for AI agent consumption.
order: 27
section: Lagoon Plugin
---

# LLM Content Export

Lagoon can generate `llms.txt` and `llms-full.txt` files at build time, following the [llms.txt specification](https://llmstxt.org/). These files allow AI agents and LLM-powered tools to discover and consume your documentation content efficiently.

- **`llms.txt`** — a structured index of links to your documentation pages, grouped by section
- **`llms-full.txt`** — the same index with each page's full markdown body inlined (only generated when at least one document provides a markdown body)

## How it works

1. **Implement `ILlmsTxtConfiguration`** in your project to describe your site and documents.
2. **`atoll build`** discovers your implementation at build time by scanning the compiled assembly — no registration required.
3. **`LlmsTxtGenerator`** writes `llms.txt` (and optionally `llms-full.txt`) to the output directory.

## Implementing `ILlmsTxtConfiguration`

Create a class that implements `ILlmsTxtConfiguration`:

```csharp
// LlmsTxtConfig.cs
using Atoll.Build.Content.Collections;
using Atoll.Lagoon.LlmsTxt;

public sealed class LlmsTxtConfig : ILlmsTxtConfiguration
{
    public LlmsTxtSiteInfo GetSiteInfo() => new(
        DocsSetup.Config.Title,
        DocsSetup.Config.Description);

    public IEnumerable<LlmsTxtDocumentInput> GetDocuments(CollectionQuery query)
    {
        var docs = query.GetCollection<DocSchema>("docs");
        foreach (var entry in docs)
        {
            yield return new LlmsTxtDocumentInput(entry.Data.Title, $"/docs/{entry.Slug}")
            {
                Description  = entry.Data.Description,
                Section      = entry.Data.Section.Length > 0 ? entry.Data.Section : null,
                MarkdownBody = entry.Body,
            };
        }
    }
}
```

`ILlmsTxtConfiguration` has two members:

| Member | Description |
|---|---|
| `GetSiteInfo()` | Returns a `LlmsTxtSiteInfo` with the site title and optional summary. Used as the H1 and blockquote header of `llms.txt`. |
| `GetDocuments(CollectionQuery)` | Returns `IEnumerable<LlmsTxtDocumentInput>` — one descriptor per documentation page. |

## `LlmsTxtDocumentInput` properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Title` | `string` | *(required)* | Document title used as the link text in `llms.txt` |
| `Href` | `string` | *(required)* | URL path for the document (e.g. `/docs/getting-started/`) |
| `Description` | `string?` | `null` | Short description appended after the link |
| `Section` | `string?` | `null` | Group label for H2 grouping in the output. Defaults to `"Documentation"` when null |
| `MarkdownBody` | `string?` | `null` | Full markdown body for `llms-full.txt`. When `null`, the document appears in the index only |

## `LlmsTxtSiteInfo`

```csharp
new LlmsTxtSiteInfo(string Title, string? Description)
```

| Property | Description |
|---|---|
| `Title` | Site/project name — rendered as the H1 heading in `llms.txt` |
| `Description` | Optional summary — rendered as a blockquote below the title. Omitted when `null` |

## Output format

### `llms.txt` (index)

```
# My Docs

> Developer documentation for My Project.

## Getting Started

- [Getting Started](/docs/getting-started): Install and create your first project.

## Basics

- [Components](/docs/components): Build reusable UI with AtollComponent.
- [Layouts](/docs/layouts): Wrap pages with shared HTML structure.
```

### `llms-full.txt` (expanded)

`llms-full.txt` is generated when at least one document provides a `MarkdownBody`. It begins with the same header as `llms.txt`, then inlines each document's full markdown body under its H2 group heading.

## `LlmsTxtGenerationResult`

The `LlmsTxtGenerator.GenerateAsync()` return value provides generation metadata:

| Property | Type | Description |
|---|---|---|
| `DocumentCount` | `int` | Number of documents included |
| `LlmsTxtPath` | `string` | Full output path of the written `llms.txt` file |
| `LlmsFullTxtPath` | `string?` | Full output path of `llms-full.txt`, or `null` if not generated |
| `Elapsed` | `TimeSpan` | Time taken to generate and write the files |
