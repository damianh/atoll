using Atoll.Components;
using Atoll.Reef.Configuration;

namespace Atoll.Reef.Layouts;

/// <summary>
/// Renders the <c>&lt;head&gt;</c> section for article/blog pages, including meta tags,
/// viewport settings, title template, custom CSS, and the theme FOUC-prevention inline script.
/// </summary>
public sealed class ArticleBaseHead : AtollComponent
{
    /// <summary>Gets or sets the Reef theme configuration.</summary>
    [Parameter(Required = true)]
    public ReefConfig Config { get; set; } = null!;

    /// <summary>Gets or sets the page-specific title. Appended to the site title.</summary>
    [Parameter]
    public string PageTitle { get; set; } = "";

    /// <summary>Gets or sets the page-specific description for the meta description tag.</summary>
    [Parameter]
    public string? PageDescription { get; set; }

    /// <summary>Gets or sets optional raw HTML to inject into the head section per page (e.g. OG tags, analytics).</summary>
    [Parameter]
    public string? PageHeadContent { get; set; }

    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<head>");
        WriteHtml("<meta charset=\"utf-8\" />");
        WriteHtml("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />");

        // Title
        WriteHtml("<title>");
        if (!string.IsNullOrEmpty(PageTitle))
        {
            WriteText(PageTitle);
            WriteHtml(" | ");
        }

        WriteText(Config.Title);
        WriteHtml("</title>");

        // Description meta
        var description = PageDescription ?? Config.Description;
        if (!string.IsNullOrEmpty(description))
        {
            WriteHtml($"<meta name=\"description\" content=\"{HtmlEncode(description)}\" />");
        }

        // Favicon
        if (!string.IsNullOrEmpty(Config.FaviconHref))
        {
            WriteHtml($"<link rel=\"icon\" type=\"image/svg+xml\" href=\"{HtmlEncode(Config.FaviconHref)}\" />");
        }

        // RSS feed auto-discovery link
        if (Config.RssEnabled)
        {
            var basePath = Config.BasePath.TrimEnd('/');
            WriteHtml($"<link rel=\"alternate\" type=\"application/rss+xml\" title=\"{HtmlEncode(Config.Title)}\" href=\"{HtmlEncode($"{basePath}/feed.xml")}\" />");
        }

        // Theme FOUC prevention — must run before page renders
        WriteHtml("""
            <script>
            (function(){var s=localStorage.getItem('atoll-theme');if(s==='dark'||s==='light'){document.documentElement.setAttribute('data-theme',s);}else if(window.matchMedia('(prefers-color-scheme: dark)').matches){document.documentElement.setAttribute('data-theme','dark');}})();
            </script>
            """);

        // Custom CSS
        foreach (var css in Config.CustomCss)
        {
            WriteHtml($"<link rel=\"stylesheet\" href=\"{HtmlEncode(css)}\" />");
        }

        // Per-page head content (e.g., OG meta, analytics snippets)
        if (!string.IsNullOrEmpty(PageHeadContent))
        {
            WriteHtml(PageHeadContent);
        }

        WriteHtml("</head>");
        return Task.CompletedTask;
    }

    private static string HtmlEncode(string value) =>
        System.Net.WebUtility.HtmlEncode(value);
}
