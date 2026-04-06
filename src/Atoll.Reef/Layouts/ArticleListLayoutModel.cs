using Atoll.Reef.Configuration;

namespace Atoll.Reef.Layouts;

/// <summary>
/// Model for the <c>ArticleListLayoutTemplate</c> Razor slice, carrying all data
/// needed to render the article listing page layout shell.
/// </summary>
/// <param name="Config">The Reef theme configuration.</param>
/// <param name="PageTitle">The page-specific title shown in the heading and title tag.</param>
/// <param name="PageDescription">Optional page-specific meta description.</param>
/// <param name="PageHeadContent">Optional raw HTML to inject into the head section.</param>
/// <param name="BrandHref">The resolved brand link href.</param>
public sealed record ArticleListLayoutModel(
    ReefConfig Config,
    string PageTitle,
    string? PageDescription,
    string? PageHeadContent,
    string BrandHref);
