---
title: Lagoon Configuration
description: Complete DocsConfig reference — title, sidebar, social links, theming, and more.
order: 21
section: Lagoon Theme
---

# Lagoon Configuration

All Lagoon options live in a single `DocsConfig` instance. Create one in a static class (e.g. `DocsSetup.cs`) and pass it to `DocsLayout` via the `Config` parameter.

## `DocsConfig` properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Title` | `string` | `""` | Site title shown in the header and browser `<title>` tag |
| `Description` | `string` | `""` | Default meta description for SEO |
| `LogoSrc` | `string?` | `null` | URL or path to the logo image; omit for text-only header |
| `LogoAlt` | `string` | `""` | Alt text for the logo image |
| `Sidebar` | `IReadOnlyList<SidebarItem>` | `[]` | Top-level sidebar navigation items |
| `TableOfContents` | `TableOfContentsConfig` | see below | Controls which headings appear in the TOC |
| `Social` | `IReadOnlyList<SocialLink>` | `[]` | Social/external links shown in the header |
| `CustomCss` | `IReadOnlyList<string>` | `[]` | Paths or URLs of additional CSS files to load on every page |
| `EnableMermaid` | `bool` | `false` | Load the Mermaid JS library and render ` ```mermaid ` blocks |
| `BasePath` | `string` | `""` | URL prefix when hosting at a sub-path (e.g. `"/docs"`) |

## `SidebarItem` properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Label` | `string` | `""` | Display text for this item |
| `Link` | `string?` | `null` | URL for a leaf link; omit for group headers |
| `Badge` | `string?` | `null` | Short badge text shown next to the label (e.g. `"New"`) |
| `Collapsed` | `bool` | `false` | Start the group collapsed; only applies to items with `Items` |
| `AutoGenerate` | `string?` | `null` | Directory slug to auto-populate children from content entries |
| `Items` | `IReadOnlyList<SidebarItem>` | `[]` | Child items for manual groups; ignored when `AutoGenerate` is set |

See [Sidebar Navigation](./sidebar) for detailed usage examples.

## `SocialLink`

```csharp
new SocialLink(string label, string url, SocialIcon icon)
```

| Property | Description |
|---|---|
| `Label` | Accessible label (e.g. `"GitHub"`) |
| `Url` | Destination URL |
| `Icon` | One of the `SocialIcon` enum values below |

### `SocialIcon` values

| Value | Platform |
|---|---|
| `GitHub` | GitHub |
| `Discord` | Discord |
| `Twitter` | Twitter / X |
| `Mastodon` | Mastodon |
| `LinkedIn` | LinkedIn |
| `YouTube` | YouTube |
| `Rss` | RSS feed |
| `ExternalLink` | Generic external link |

## `TableOfContentsConfig` properties

| Property | Type | Default | Description |
|---|---|---|---|
| `MinHeadingLevel` | `int` | `2` | Lowest heading level to include (inclusive) |
| `MaxHeadingLevel` | `int` | `3` | Highest heading level to include (inclusive) |

To hide the TOC entirely, set `MinHeadingLevel` above `MaxHeadingLevel` (e.g. `MinHeadingLevel = 7, MaxHeadingLevel = 2`).

## Complete example

```csharp
using Atoll.Lagoon.Configuration;

public static class DocsSetup
{
    public static DocsConfig Config { get; } = new DocsConfig
    {
        Title        = "Atoll",
        Description  = "A .NET-native static-site framework inspired by Astro.",
        BasePath     = "",
        EnableMermaid = false,
        TableOfContents = new TableOfContentsConfig
        {
            MinHeadingLevel = 2,
            MaxHeadingLevel = 3,
        },
        Social =
        [
            new SocialLink("GitHub", "https://github.com/me/my-project", SocialIcon.GitHub),
            new SocialLink("Discord", "https://discord.gg/example", SocialIcon.Discord),
        ],
        CustomCss = ["/styles/custom.css"],
        Sidebar =
        [
            new SidebarItem { Label = "Getting Started", Link = "/docs/getting-started" },
            new SidebarItem
            {
                Label = "Guides",
                Items =
                [
                    new SidebarItem { Label = "Installation",  Link = "/docs/installation" },
                    new SidebarItem { Label = "Configuration", Link = "/docs/configuration", Badge = "New" },
                ],
            },
            new SidebarItem
            {
                Label        = "API Reference",
                AutoGenerate = "reference",
                Collapsed    = true,
            },
        ],
    };
}
```

## BasePath

Use `BasePath` when your docs site is hosted at a URL sub-path rather than the root:

```csharp
BasePath = "/docs"
```

You must also set the corresponding `IndexUrl` on the `SearchDialog` island so it fetches the search index from the correct path. See [Site Search](./search) for details.

## Frontmatter fields

In addition to the `DocsConfig` site-level options, Lagoon supports per-page configuration through YAML frontmatter in your Markdown files. The fields available depend on your `DocSchema` class.

### `head`

Inject raw HTML into the page's `<head>` section. Use a YAML literal block (`head: |`) for multi-line content. This is useful for analytics scripts, OpenGraph / social meta tags, or page-specific stylesheets.

```markdown
---
title: My Page
description: Page description for SEO.
head: |
  <meta property="og:title" content="My Page">
  <meta property="og:image" content="/images/my-page-card.png">
  <script src="/analytics.js"></script>
---
```

The content is injected after custom CSS links and before `</head>`. Pages without a `head:` field render identically to before — no extra output is added.

To use this field, add a `Head` property to your frontmatter schema:

```csharp
public sealed class DocSchema
{
    [Required]
    public string Title { get; set; } = "";

    public string Description { get; set; } = "";

    /// <summary>
    /// Optional raw HTML to inject into the page's &lt;head&gt; section.
    /// </summary>
    public string? Head { get; set; }
}
```

Then wire it in your wrapper layout — see [Components & Layout](./components) for the full integration pattern.
