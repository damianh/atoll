using Atoll.Reef.Configuration;

namespace Atoll.Reef.Layouts;

/// <summary>
/// Model for the <c>ArticleLayoutTemplate</c> Razor slice, carrying all data
/// needed to render the article page layout shell.
/// </summary>
/// <param name="Config">The Reef theme configuration.</param>
/// <param name="PageTitle">The page-specific title appended to the site title.</param>
/// <param name="PageDescription">Optional page-specific meta description.</param>
/// <param name="PageHeadContent">Optional raw HTML to inject into the head section.</param>
/// <param name="BrandHref">The resolved brand link href.</param>
public sealed record ArticleLayoutModel(
    ReefConfig Config,
    string PageTitle,
    string? PageDescription,
    string? PageHeadContent,
    string BrandHref);
