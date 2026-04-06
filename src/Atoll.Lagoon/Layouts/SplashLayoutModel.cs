using Atoll.Lagoon.Configuration;

namespace Atoll.Lagoon.Layouts;

/// <summary>
/// Model for the <c>SplashLayoutTemplate</c> Razor slice, carrying all data
/// needed to render the splash/landing page layout.
/// </summary>
/// <param name="Config">The docs site configuration.</param>
/// <param name="PageTitle">The page-specific title appended to the site title.</param>
/// <param name="PageDescription">Optional page-specific meta description.</param>
/// <param name="LogoSrc">The resolved logo image URL (custom or default fallback).</param>
/// <param name="EnableMermaid">Whether to inject the Mermaid initialization script.</param>
/// <param name="CurrentPath">The current page URL path, used for OG image URL computation.</param>
/// <param name="SiteUrl">The site base URL, used for absolute OG URLs.</param>
public sealed record SplashLayoutModel(
    DocsConfig Config,
    string PageTitle,
    string? PageDescription,
    string LogoSrc,
    bool EnableMermaid,
    string CurrentPath,
    string SiteUrl);

