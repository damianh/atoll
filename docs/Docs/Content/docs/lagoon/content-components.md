---
title: Content Components
description: Card, Aside, Steps, Tabs, FileTree, LinkCard, LinkButton, Icon, and CardGrid components for rich documentation content.
order: 25
section: Lagoon Plugin
---

# Content Components

Lagoon includes a library of content components for authoring rich documentation pages. These mirror the components available in [Astro Starlight](https://starlight.astro.build/components/using-components/) and are all `AtollComponent` subclasses that render semantic, accessible HTML.

:::aside{type="tip" title="Two syntax options"}
All live examples on this page use `:::` directive syntax. Every component also supports `<PascalCaseName>` HTML-like tag syntax — for example, `:::aside{type="tip"}...:::` can be written as `<Aside Type="tip">...</Aside>`. See the [MDA Format](../mda-format) page for details on both syntaxes.
:::

## Aside

Callout boxes for tips, notes, cautions, and warnings. Use these to draw attention to important information.

### Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Type` | `AsideType` | `Note` | Visual variant: `Note`, `Tip`, `Caution`, or `Danger` |
| `Title` | `string?` | Variant name | Custom title; defaults to the variant name (e.g. "Note") |

The **default slot** receives the callout body content.

### Variants

| Type | Icon | Colour | Use for |
|---|---|---|---|
| `Note` | ℹ️ Information | Blue | General supplementary information |
| `Tip` | 💡 Tip | Green | Helpful suggestions and best practices |
| `Caution` | ⚠️ Warning | Yellow | Things to watch out for |
| `Danger` | 🔴 Danger | Red | Breaking changes, destructive actions, security risks |

### Usage

```csharp
await RenderAsync<Aside>(new { Type = AsideType.Tip, Title = "Performance tip" }, slot =>
{
    slot.WriteHtml("""<p>Use <code>client:idle</code> for non-critical islands to avoid blocking the main thread.</p>""");
});

await RenderAsync<Aside>(new { Type = AsideType.Danger }, slot =>
{
    slot.WriteHtml("""<p>This operation is destructive and cannot be undone.</p>""");
});
```

### Output

```html
<aside class="aside aside-tip" role="note" aria-label="Performance tip">
  <p class="aside-title"><svg ...>...</svg> Performance tip</p>
  <div class="aside-content">
    <p>Use <code>client:idle</code> for non-critical islands...</p>
  </div>
</aside>
```

### Live examples

:::aside
Aside defaults to the **Note** variant when no `type` is specified. Use notes for general supplementary information.
:::

:::aside{type="tip" title="Performance tip"}
Use `client:idle` for non-critical islands to avoid blocking the main thread.
:::

:::aside{type="caution" title="Compatibility notice"}
This API is stable but may change in the next major version. Pin your dependency version to avoid surprises.
:::

:::aside{type="danger"}
This operation is destructive and cannot be undone. Make sure you have a backup before proceeding.
:::

---

## Card

A bordered content card with a title, optional icon, and slotted body content.

### Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Title` | `string` | *(required)* | Card heading text |
| `IconName` | `IconName?` | `null` | Optional icon displayed next to the title |

### Usage

```csharp
await RenderAsync<Card>(new { Title = "Getting Started", IconName = IconName.Rocket }, slot =>
{
    slot.WriteHtml("""<p>Set up your first Atoll project in under 5 minutes.</p>""");
});
```

### Output

```html
<div class="card">
  <div class="card-header">
    <svg class="icon" ...>...</svg>
    <h3 class="card-title">Getting Started</h3>
  </div>
  <div class="card-body">
    <p>Set up your first Atoll project in under 5 minutes.</p>
  </div>
</div>
```

### Live example

:::card{title="Getting Started" iconName="Rocket"}
Set up your first Atoll project in under 5 minutes. Atoll's CLI scaffolds everything you need — just add content.
:::

---

## CardGrid

A responsive grid container for `Card` components.

### Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Stagger` | `bool` | `false` | When `true`, alternate cards are offset vertically for a staggered visual effect |

### Usage

```csharp
await RenderAsync<CardGrid>(new { Stagger = false }, async slot =>
{
    await slot.RenderAsync<Card>(new { Title = "Fast" }, s =>
    {
        s.WriteHtml("""<p>Sub-millisecond component rendering.</p>""");
    });
    await slot.RenderAsync<Card>(new { Title = "Type-safe" }, s =>
    {
        s.WriteHtml("""<p>Full C# type safety for all parameters.</p>""");
    });
});
```

### Output

```html
<div class="card-grid">
  <div class="card">...</div>
  <div class="card">...</div>
</div>
```

### Live example

:::card-grid
:::card{title="Fast"}
Sub-millisecond component rendering powered by server-side C#.
:::
:::card{title="Type-safe"}
Full C# type safety for all parameters — catch errors at compile time.
:::
:::card{title="Accessible"}
Semantic HTML with ARIA attributes and keyboard navigation built in.
:::

:::

### Live example — staggered layout

:::card-grid{stagger}
:::card{title="Write content" iconName="Pencil"}
Author documentation in Markdown with full component support.
:::
:::card{title="Ship fast" iconName="Rocket"}
Build and deploy static sites with zero JavaScript by default.
:::

:::

---

## Steps

A wrapper that applies numbered step styling to an ordered list. Uses CSS counters so the numbering is automatic.

### Parameters

None. The default slot should contain an `<ol>` element.

### Usage

```csharp
await RenderAsync<Steps>(slot =>
{
    slot.WriteHtml("""
        <ol>
            <li><strong>Install the CLI</strong><p>Run <code>dotnet tool install -g atoll</code></p></li>
            <li><strong>Create a project</strong><p>Run <code>atoll new docs</code></p></li>
            <li><strong>Start developing</strong><p>Run <code>atoll dev</code></p></li>
        </ol>
    """);
});
```

### Output

```html
<div class="steps">
  <ol>
    <li><strong>Install the CLI</strong><p>...</p></li>
    ...
  </ol>
</div>
```

### Live example

:::steps
1. **Install the CLI** — Run `dotnet tool install -g atoll` to install the Atoll CLI globally.
2. **Create a project** — Run `atoll new docs` to scaffold a new documentation site.
3. **Start developing** — Run `atoll dev` to launch the local dev server with hot reload.
:::

---

## Tabs

A tabbed interface island that renders all panels server-side and switches between them with client-side JavaScript. Uses `[ClientLoad]` for immediate interactivity.

### Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `TabItems` | `IReadOnlyList<TabItemData>` | *(required)* | The tab definitions |
| `SyncKey` | `string?` | `null` | When set, tabs with the same sync key switch together across the page |

### TabItemData

| Property | Type | Description |
|---|---|---|
| `Label` | `string` | Tab button label |
| `Content` | `RenderFragment` | Panel content |
| `IconName` | `IconName?` | Optional icon before the label |

### Usage

```csharp
var tabs = new List<TabItemData>
{
    new("npm", RenderFragment.From("<pre>npm install atoll</pre>")),
    new("pnpm", RenderFragment.From("<pre>pnpm add atoll</pre>")),
    new("yarn", RenderFragment.From("<pre>yarn add atoll</pre>")),
};

await RenderAsync<Tabs>(new { TabItems = tabs, SyncKey = "pkg-manager" });
```

### Cross-group synchronisation

When multiple `Tabs` instances share the same `SyncKey`, selecting a tab in one group automatically selects the tab with the same label in all other groups. This is useful when you want the user's package manager preference to persist across code samples on a page.

### Output

```html
<div class="tabs" data-sync-key="pkg-manager">
  <div class="tabs-header" role="tablist">
    <button role="tab" aria-selected="true" class="tab-button tab-button-active">npm</button>
    <button role="tab" aria-selected="false" class="tab-button">pnpm</button>
    <button role="tab" aria-selected="false" class="tab-button">yarn</button>
  </div>
  <div role="tabpanel" class="tab-panel"><pre>npm install atoll</pre></div>
  <div role="tabpanel" class="tab-panel" hidden><pre>pnpm add atoll</pre></div>
  <div role="tabpanel" class="tab-panel" hidden><pre>yarn add atoll</pre></div>
</div>
```

### Live examples

#### Basic tabs

:::tabs
:::tab-item{label="npm"}
```sh
npm install atoll
```
:::
:::tab-item{label="pnpm"}
```sh
pnpm add atoll
```
:::
:::tab-item{label="yarn"}
```sh
yarn add atoll
```
:::
:::

#### Synchronised tabs

These two tab groups share `syncKey="pkg"` — selecting a tab in one group switches the same label in the other.

:::tabs{syncKey="pkg"}
:::tab-item{label="npm"}
```sh
npm install atoll
```
:::
:::tab-item{label="pnpm"}
```sh
pnpm add atoll
```
:::
:::tab-item{label="yarn"}
```sh
yarn add atoll
```
:::
:::

:::tabs{syncKey="pkg"}
:::tab-item{label="npm"}
```sh
npm run build
```
:::
:::tab-item{label="pnpm"}
```sh
pnpm build
```
:::
:::tab-item{label="yarn"}
```sh
yarn build
```
:::
:::

#### Tabs with icons

:::tabs
:::tab-item{label="Stars" iconName="Star"}
Sirius, Vega, Betelgeuse
:::
:::tab-item{label="Rockets" iconName="Rocket"}
Falcon 9, Starship, Ariane 6
:::
:::

---

## FileTree

Renders a file and directory structure as an interactive tree with collapsible directories using native `<details>` elements.

### Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Items` | `IReadOnlyList<FileTreeItem>` | *(required)* | Root-level tree items |

### FileTreeItem

| Property | Type | Description |
|---|---|---|
| `Name` | `string` | File or directory name |
| `IsDirectory` | `bool` | Whether this item is a directory |
| `Children` | `IReadOnlyList<FileTreeItem>?` | Child items (directories only) |
| `IsHighlighted` | `bool` | When `true`, the item is visually emphasised |

### Usage

```csharp
var tree = new List<FileTreeItem>
{
    new("src", isDirectory: true, children: new List<FileTreeItem>
    {
        new("Components", isDirectory: true, children: new List<FileTreeItem>
        {
            new("Header.cs"),
            new("Footer.cs", isHighlighted: true),
        }),
        new("Program.cs"),
    }),
    new("docs", isDirectory: true, children: new List<FileTreeItem>
    {
        new("getting-started.md"),
    }),
    new("README.md"),
};

await RenderAsync<FileTree>(new { Items = tree });
```

### Output

```html
<div class="file-tree" role="tree">
  <ul role="group">
    <li role="treeitem" class="file-tree-dir">
      <details open>
        <summary><svg ...>...</svg> src/</summary>
        <ul role="group">
          <li role="treeitem" class="file-tree-file file-tree-file-highlight">
            <svg ...>...</svg> Footer.cs
          </li>
          ...
        </ul>
      </details>
    </li>
  </ul>
</div>
```

---

## LinkCard

A prominent anchor element styled as a navigation card. Use for linking to key pages or resources.

### Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Title` | `string` | *(required)* | Card title |
| `Href` | `string` | *(required)* | Navigation URL |
| `Description` | `string?` | `null` | Optional description beneath the title |
| `IconName` | `IconName?` | `null` | Optional icon before the title |

### Usage

```csharp
await RenderAsync<LinkCard>(new
{
    Title = "Read the Guide",
    Href = "/docs/getting-started",
    Description = "Learn how to set up your first Atoll project.",
    IconName = IconName.ArrowRight,
});
```

### Output

```html
<a href="/docs/getting-started" class="link-card">
  <span class="link-card-title"><svg ...>...</svg> Read the Guide</span>
  <span class="link-card-description">Learn how to set up your first Atoll project.</span>
</a>
```

### Live example

:::link-card{title="Read the Guide" href="/docs/getting-started" description="Learn how to set up your first Atoll project."}
:::

:::link-card{title="Component Reference" href="/docs/lagoon/content-components" description="Browse all available content components." iconName="Document"}
:::

---

## LinkButton

A styled anchor element rendered as a call-to-action button. Three visual variants are available.

### Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Href` | `string` | *(required)* | Button URL |
| `Label` | `string` | *(required)* | Button label text |
| `Variant` | `LinkButtonVariant` | `Primary` | Visual style: `Primary`, `Secondary`, or `Minimal` |
| `IconName` | `IconName?` | `null` | Optional icon |
| `IconPlacement` | `IconPlacement` | `Start` | Icon position: `Start` (before label) or `End` (after label) |

### Usage

```csharp
await RenderAsync<LinkButton>(new
{
    Href = "/docs/getting-started",
    Label = "Get Started",
    Variant = LinkButtonVariant.Primary,
    IconName = IconName.Rocket,
});

await RenderAsync<LinkButton>(new
{
    Href = "https://github.com/damianh/atoll",
    Label = "View on GitHub",
    Variant = LinkButtonVariant.Secondary,
    IconName = IconName.ExternalLink,
    IconPlacement = IconPlacement.End,
});
```

### Live example

:::link-button{href="/docs/getting-started" label="Get Started" variant="Primary" iconName="Rocket"}
:::

:::link-button{href="https://github.com/damianh/atoll" label="View on GitHub" variant="Secondary" iconName="ExternalLink" iconPlacement="End"}
:::

:::link-button{href="/docs/lagoon/content-components" label="Browse Components" variant="Minimal"}
:::

---

## Icon

Renders an inline SVG icon from Lagoon's built-in icon set.

### Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Name` | `IconName` | *(required)* | The icon to render |
| `Label` | `string?` | `null` | Accessibility label; when `null`, the icon is hidden from screen readers |
| `Size` | `string` | `"1em"` | CSS size value (e.g. `"1.5rem"`, `"24px"`) |
| `Color` | `string?` | `null` | CSS colour value; defaults to `currentColor` |

### Available icons

| Icon name | Description |
|---|---|
| `Information` | Info circle |
| `Tip` | Lightbulb |
| `Warning` | Warning triangle |
| `Danger` | Error circle |
| `Star` | Star |
| `Rocket` | Rocket |
| `ExternalLink` | External link arrow |
| `Document` | Document page |
| `Folder` / `FolderOpen` | Folder icons |
| `File` | File icon |
| `Pencil` | Edit pencil |
| `Check` | Checkmark |
| `Heart` | Heart |
| `ArrowRight` / `ArrowLeft` | Directional arrows |
| `Plus` / `Minus` | Add/remove |
| `ChevronRight` / `ChevronDown` | Chevron indicators |
| `Sun` / `Moon` | Theme toggle icons |
| `Search` | Magnifying glass |
| `Menu` / `Close` | Hamburger menu / close |

### Usage

```csharp
await RenderAsync<Icon>(new { Name = IconName.Rocket, Label = "Launch", Size = "1.5rem" });
```

### Live example

:::icon{name="Rocket" label="Launch" size="1.5rem"}
:::
:::icon{name="Star" label="Favourite" size="1.5rem" color="gold"}
:::
:::icon{name="Heart" label="Like" size="1.5rem" color="crimson"}
:::
:::icon{name="Check" label="Done" size="1.5rem" color="green"}
:::
