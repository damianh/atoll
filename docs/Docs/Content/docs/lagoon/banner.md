---
title: Global Banner
description: Site-wide announcement banner with colour variants, CTA links, and dismissal.
order: 26
section: Lagoon Plugin
---

# Global Banner

Lagoon supports a site-wide announcement banner displayed above the page content in both `DocsLayout` and `SplashLayout`. Use it for release announcements, maintenance notices, or any other site-wide message.

## Enabling the banner

Set `DocsConfig.Banner` to a `BannerConfig` instance:

```csharp
// DocsSetup.cs
using Atoll.Lagoon.Configuration;

DocsConfig Config = new DocsConfig
{
    // ... other config ...
    Banner = new BannerConfig
    {
        Content   = "🎉 Atoll 1.0 is now available!",
        Variant   = BannerVariant.Success,
        LinkHref  = "/docs/getting-started",
        LinkText  = "Get started →",
        Dismissible = true,
    },
};
```

## `BannerConfig` properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Content` | `string` | `""` | Raw HTML displayed in the banner. An empty string prevents the banner from rendering. |
| `Variant` | `BannerVariant` | `Info` | Visual colour variant (see table below). |
| `LinkHref` | `string?` | `null` | Optional URL for a call-to-action link. |
| `LinkText` | `string?` | `null` | Display text for the CTA link. Both `LinkHref` and `LinkText` must be non-null for the link to render. |
| `Dismissible` | `bool` | `true` | When `true`, a dismiss button is rendered and the dismissal state is persisted to `localStorage`. |
| `DismissKey` | `string` | `"atoll-banner-dismissed"` | `localStorage` key used to persist dismissal. Change this value to reset all previously dismissed banners. |
| `Enabled` | `bool` | `true` | Master switch. When `false`, no banner HTML is rendered regardless of other settings. |

## `BannerVariant` values

| Variant | Colour | CSS tokens |
|---|---|---|
| `Info` | Blue | `--aside-note-*` |
| `Warning` | Amber | `--aside-caution-*` |
| `Success` | Green | `--aside-tip-*` |
| `Danger` | Red | `--aside-danger-*` |

Banner colours are derived from the same `--aside-*` CSS custom properties used by the `Aside` content component, so they stay consistent across light and dark themes.

## Dismissal behaviour

When `Dismissible = true`, a close button is rendered at the end of the banner. Clicking it writes a flag to `localStorage` under the `DismissKey` value. Once dismissed, the banner stays hidden until:

- The user clears `localStorage`, or
- You change `DismissKey` to a new value (cache-busting).

Changing `DismissKey` is the recommended way to re-show the banner when you update the message content.

```csharp
Banner = new BannerConfig
{
    Content    = "⚠️ Scheduled maintenance on Saturday 10:00–12:00 UTC.",
    Variant    = BannerVariant.Warning,
    Dismissible = true,
    // Bump this value whenever the banner message changes
    DismissKey  = "maintenance-2026-04-12",
},
```

## CTA link

A call-to-action link is rendered inside the banner when both `LinkHref` and `LinkText` are set:

```csharp
Banner = new BannerConfig
{
    Content  = "New documentation available.",
    Variant  = BannerVariant.Info,
    LinkHref = "/docs/whats-new",
    LinkText = "Read the release notes →",
},
```

If either property is `null`, no link is rendered.

## Layouts

The banner renders in both `DocsLayout` and `SplashLayout`, positioned immediately after the site header. It is omitted entirely when `Content` is empty or `Enabled` is `false`.
