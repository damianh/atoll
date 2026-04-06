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
public sealed record DocsBaseHeadModel(
    DocsConfig Config,
    string PageTitle,
    string? Description,
    string FaviconHref,
    string? PageHeadContent);
