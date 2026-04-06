using Atoll.Lagoon.Configuration;

namespace Atoll.Lagoon.Layouts;

/// <summary>
/// Model for the <c>DocsBaseHeadTemplate</c> Razor slice, carrying all data
/// needed to render the document head section.
/// </summary>
/// <param name="Config">The docs site configuration.</param>
/// <param name="PageTitle">The page-specific title appended to the site title.</param>
/// <param name="Description">The resolved description (page-specific or config default).</param>
/// <param name="FaviconHref">The resolved favicon URL.</param>
/// <param name="PageHeadContent">Optional raw HTML to inject into the head section.</param>
/// <param name="OgImageUrl">The absolute OG image URL, or <c>null</c> if OG is not configured.</param>
/// <param name="OgUrl">The absolute canonical page URL for OG meta tags, or <c>null</c> if OG is not configured.</param>
/// <param name="OgTitle">The title for OG meta tags (same as <paramref name="PageTitle"/>).</param>
/// <param name="OgDescription">The description for OG meta tags.</param>
/// <param name="SiteName">The site name for the <c>og:site_name</c> meta tag.</param>
public sealed record DocsBaseHeadModel(
    DocsConfig Config,
    string PageTitle,
    string? Description,
    string FaviconHref,
    string? PageHeadContent,
    string? OgImageUrl,
    string? OgUrl,
    string OgTitle,
    string? OgDescription,
    string SiteName);

