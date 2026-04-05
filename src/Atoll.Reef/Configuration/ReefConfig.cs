namespace Atoll.Reef.Configuration;

/// <summary>
/// Root configuration for the <c>Atoll.Reef</c> articles/blog theme.
/// </summary>
public sealed class ReefConfig
{
    /// <summary>
    /// Gets or sets the site title displayed in the header and browser tab.
    /// </summary>
    public string Title { get; set; } = "";

    /// <summary>
    /// Gets or sets a short description of the site used in meta tags.
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// Gets or sets the URL or path to the site logo image.
    /// </summary>
    public string? LogoSrc { get; set; }

    /// <summary>
    /// Gets or sets the alt text for the logo image.
    /// </summary>
    public string LogoAlt { get; set; } = "";

    /// <summary>
    /// Gets or sets the URL or path to the site favicon.
    /// </summary>
    public string? FaviconHref { get; set; }

    /// <summary>
    /// Gets or sets the number of articles to display per listing page.
    /// Default: <c>10</c>.
    /// </summary>
    public int ArticlesPerPage { get; set; } = 10;

    /// <summary>
    /// Gets or sets the default article listing view.
    /// Default: <see cref="DefaultView.List"/>.
    /// </summary>
    public DefaultView DefaultView { get; set; } = DefaultView.List;

    /// <summary>
    /// Gets or sets a value indicating whether per-tag listing pages are generated.
    /// Default: <c>true</c>.
    /// </summary>
    public bool TagPageEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether per-author listing pages are generated.
    /// Default: <c>true</c>.
    /// </summary>
    public bool AuthorPageEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether an RSS/Atom feed is generated.
    /// Default: <c>true</c>.
    /// </summary>
    public bool RssEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the base URL path prefix for the articles site.
    /// For example, <c>"/blog"</c> when articles are hosted at <c>https://example.com/blog/</c>.
    /// Default: <c>""</c> (root).
    /// </summary>
    public string BasePath { get; set; } = "";

    /// <summary>
    /// Gets or sets the canonical site URL (e.g. <c>"https://example.com"</c>).
    /// Used to build absolute URLs in RSS feeds and OpenGraph meta tags.
    /// </summary>
    public string SiteUrl { get; set; } = "";

    /// <summary>
    /// Gets or sets the name of the content collection to query for articles.
    /// Default: <c>"articles"</c>.
    /// </summary>
    public string CollectionName { get; set; } = "articles";

    /// <summary>
    /// Gets or sets the social links shown in the site header.
    /// </summary>
    public IReadOnlyList<SocialLink> Social { get; set; } = [];

    /// <summary>
    /// Gets or sets paths to additional CSS files to load in every page's <c>&lt;head&gt;</c>.
    /// </summary>
    public IReadOnlyList<string> CustomCss { get; set; } = [];

    /// <summary>
    /// Gets or sets the author registry, keyed by the author identifier used in article frontmatter.
    /// Used to display author bios, avatars, and author listing pages.
    /// </summary>
    public IReadOnlyDictionary<string, AuthorInfo> Authors { get; set; }
        = new Dictionary<string, AuthorInfo>();
}
