using Atoll.Reef.Configuration;

namespace Atoll.Samples.Articles;

/// <summary>
/// Shared <see cref="ReefConfig"/> instance for the Articles sample site.
/// </summary>
internal static class ArticlesConfig
{
    /// <summary>Gets the site-wide Reef configuration.</summary>
    internal static readonly ReefConfig Current = new()
    {
        Title = "Atoll Articles",
        Description = "Articles and tutorials for the Atoll framework.",
        BasePath = "/articles",
        SiteUrl = "https://example.com",
        RssEnabled = true,
        ArticlesPerPage = 10,
        DefaultView = DefaultView.List,
    };
}
