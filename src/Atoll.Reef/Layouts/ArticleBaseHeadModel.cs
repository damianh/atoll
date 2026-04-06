using Atoll.Reef.Configuration;

namespace Atoll.Reef.Layouts;

/// <summary>
/// Model for the <c>ArticleBaseHeadTemplate</c> Razor slice, carrying all data
/// needed to render the article page head section.
/// </summary>
/// <param name="Config">The Reef theme configuration.</param>
/// <param name="PageTitle">The page-specific title appended to the site title.</param>
/// <param name="Description">The resolved description (page-specific or config default).</param>
/// <param name="PageHeadContent">Optional raw HTML to inject into the head section.</param>
public sealed record ArticleBaseHeadModel(
    ReefConfig Config,
    string PageTitle,
    string? Description,
    string? PageHeadContent);
