---
title: Versioned Documentation
description: Multi-version documentation with a version selector, per-version sidebars, deprecated version notices, and version-scoped search indices.
order: 27
section: Lagoon Plugin
---

# Versioned Documentation

Lagoon supports opt-in multi-version documentation. When configured, it adds a version selector dropdown, per-version content directories, per-version sidebar overrides, deprecated version notices, and version-scoped search indices.

Versioning is fully backward compatible: sites that do not set `DocsConfig.Versions` render identically to before.

## Quick start

Add a `Versions` dictionary to your `DocsConfig`. The key `"current"` designates the default (latest) version — it has no URL prefix. All other keys become URL prefixes (e.g. `/v1.0/`, `/v2.0/`).

```csharp
var config = new DocsConfig
{
    Title = "My Docs",
    Versions = new Dictionary<string, VersionConfig>
    {
        ["current"] = new VersionConfig { Label = "Latest" },
        ["v1.0"]    = new VersionConfig { Label = "v1.0", IsDeprecated = true },
    },
};
```

With this configuration:

- Pages at `/guide` resolve to the current (latest) version.
- Pages at `/v1.0/guide` resolve to the v1.0 version.
- A version picker dropdown appears in the header.
- Visiting a deprecated version shows a notice banner with a link to the current version.

## URL structure

Versioned URLs follow the pattern `/{locale}/{version}/{page}` when both i18n and versioning are configured, or `/{version}/{page}` for versioning alone.

| Version key | URL prefix | Example page URL |
|-------------|------------|-----------------|
| `"current"` | _(none)_   | `/guide/intro`  |
| `"v1.0"`    | `/v1.0`    | `/v1.0/guide/intro` |
| `"v2.0"`    | `/v2.0`    | `/v2.0/guide/intro` |

## Content organisation

Store version-specific content in subdirectories matching the version key:

```
Content/
  docs/
    guide/
      intro.md          ← current version
    v1.0/
      guide/
        intro.md        ← v1.0 content
```

Use `VersionContentResolver.GetVersionContentPath()` to compute the physical content path from the resolved version key and content path.

## `VersionConfig` properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Label` | `string` | _(required)_ | Display name shown in the version picker (e.g. `"Latest"`, `"v1.0"`). |
| `Slug` | `string` | _(required)_ | URL slug for archived versions. Unused for the `"current"` key. |
| `IsDeprecated` | `bool` | `false` | When `true`, renders a deprecated version notice banner above the article. |
| `DeprecationMessage` | `string?` | `null` | Custom deprecation message. When `null`, falls back to `UiTranslations.OutdatedVersionNotice`. |
| `Sidebar` | `IReadOnlyList<SidebarItem>?` | `null` | Per-version sidebar override. When `null`, falls back to `DocsConfig.Sidebar`. |

## Per-version sidebars

Override the sidebar for a specific version by setting `VersionConfig.Sidebar`:

```csharp
["v1.0"] = new VersionConfig
{
    Label = "v1.0",
    IsDeprecated = true,
    Sidebar =
    [
        new SidebarItem { Label = "Legacy Guide", Link = "/v1.0/guide" },
    ],
},
```

When `Sidebar` is `null`, the global `DocsConfig.Sidebar` is used. Pass the resolved sidebar to `SidebarBuilder` using the version-aware `Build()` overload that accepts `versionPrefix` and `versionKey`.

## Deprecated version notices

Set `IsDeprecated = true` on a `VersionConfig` to show a notice banner above the article content for that version. The notice includes:

- A message (custom `DeprecationMessage` or the default `UiTranslations.OutdatedVersionNotice`).
- A link to the same page in the current version (`UiTranslations.OutdatedVersionLinkText`).

Customise the notice text globally via `DocsConfig.Translations` or per-locale via `LocaleConfig.Translations`.

## Version-scoped search indices

Generate a separate search index per version using `LagoonSearchIndexGenerator.GenerateAsync()` with `localePrefix` and `versionPrefix` parameters:

```csharp
var generator = new LagoonSearchIndexGenerator(outputDirectory);

// Current version (root)
await generator.GenerateAsync(currentDocs, "", "");

// Archived version
await generator.GenerateAsync(v1Docs, "", "v1.0");

// Archived version + locale
await generator.GenerateAsync(v1FrDocs, "fr", "v1.0");
```

Each call writes to the appropriate subdirectory:

| Call | Output path |
|---|---|
| `("", "")` | `{output}/search-index.json` |
| `("", "v1.0")` | `{output}/v1.0/search-index.json` |
| `("fr", "v1.0")` | `{output}/fr/v1.0/search-index.json` |

`DocsLayout` automatically computes the correct search index URL based on the resolved locale and version.

## Combining versioning with i18n

Versioning and i18n compose naturally. URL order is `/{locale}/{version}/{page}`:

```csharp
var config = new DocsConfig
{
    Locales = new Dictionary<string, LocaleConfig>
    {
        ["root"] = new LocaleConfig { Label = "English", Lang = "en" },
        ["fr"]   = new LocaleConfig { Label = "Français", Lang = "fr" },
    },
    Versions = new Dictionary<string, VersionConfig>
    {
        ["current"] = new VersionConfig { Label = "Latest" },
        ["v1.0"]    = new VersionConfig { Label = "v1.0", IsDeprecated = true },
    },
};
```

Example URLs:
- `/guide` — English, current version
- `/fr/guide` — French, current version
- `/v1.0/guide` — English, v1.0
- `/fr/v1.0/guide` — French, v1.0

## VersionPicker component

The `VersionPicker` component is rendered automatically by `DocsLayout` when `DocsConfig.Versions` has two or more entries. You can also use it standalone:

```csharp
await RenderAsync(ComponentRenderer.ToFragment<VersionPicker>(new Dictionary<string, object?>
{
    ["Versions"] = config.Versions,
    ["CurrentVersionKey"] = resolvedVersion.Key,
    ["CurrentContentPath"] = resolvedVersion.ContentPath,
    ["LocalePrefix"] = localePrefix,
    ["BasePath"] = config.BasePath,
    ["Translations"] = translations,
}));
```
