using Atoll.Lagoon.Configuration;

namespace Atoll.Lagoon.Versioning;

/// <summary>
/// Represents the configuration for a single version in a multi-version documentation site.
/// Each version maps to a URL path prefix (or <c>"current"</c> for the default, unprefixed version).
/// </summary>
public sealed record VersionConfig
{
    /// <summary>Gets the display label for this version (e.g. "v2.0", "Latest").</summary>
    public required string Label { get; init; }

    /// <summary>
    /// Gets the URL slug for this version (e.g. "v2.0", "v1.0").
    /// For the <c>"current"</c> key, this is unused since no prefix is added to URLs.
    /// </summary>
    public required string Slug { get; init; }

    /// <summary>
    /// Gets a value indicating whether this version is deprecated.
    /// When <c>true</c>, a deprecated version notice is rendered above the page content.
    /// Default: <c>false</c>.
    /// </summary>
    public bool IsDeprecated { get; init; }

    /// <summary>
    /// Gets a custom deprecation message override.
    /// When <c>null</c>, falls back to <see cref="I18n.UiTranslations.OutdatedVersionNotice"/>.
    /// </summary>
    public string? DeprecationMessage { get; init; }

    /// <summary>
    /// Gets per-version sidebar override items.
    /// When <c>null</c>, falls back to <see cref="Configuration.DocsConfig.Sidebar"/>.
    /// </summary>
    public IReadOnlyList<SidebarItem>? Sidebar { get; init; }
}
