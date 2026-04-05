using Atoll.Components;

namespace Atoll.Reef.Components;

/// <summary>
/// Renders the article metadata strip: publication date, optional author,
/// optional reading time, and tag pills with links.
/// </summary>
public sealed class ArticleMeta : AtollComponent
{
    /// <summary>Gets or sets the article publication date.</summary>
    [Parameter(Required = true)]
    public DateTime PubDate { get; set; }

    /// <summary>Gets or sets the author display name. Omitted when <c>null</c> or empty.</summary>
    [Parameter]
    public string? Author { get; set; }

    /// <summary>Gets or sets the estimated reading time in minutes. Omitted when <c>null</c>.</summary>
    [Parameter]
    public int? ReadingTimeMinutes { get; set; }

    /// <summary>Gets or sets the array of tag names to display as linked pills.</summary>
    [Parameter]
    public string[] Tags { get; set; } = [];

    /// <summary>
    /// Gets or sets the base URL path prefix for the articles site (e.g. <c>"/blog"</c>).
    /// Used when building tag links.
    /// </summary>
    [Parameter]
    public string BasePath { get; set; } = "";

    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<div class=\"article-meta\">");

        // Publication date
        WriteHtml("<time class=\"article-date\" datetime=\"");
        WriteHtml(PubDate.ToString("yyyy-MM-dd"));
        WriteHtml("\">");
        WriteText(PubDate.ToString("MMMM d, yyyy"));
        WriteHtml("</time>");

        // Author
        if (!string.IsNullOrEmpty(Author))
        {
            WriteHtml("<span class=\"article-author\">");
            WriteText(Author);
            WriteHtml("</span>");
        }

        // Reading time
        if (ReadingTimeMinutes.HasValue)
        {
            WriteHtml("<span class=\"article-reading-time\">");
            WriteText($"{ReadingTimeMinutes.Value} min read");
            WriteHtml("</span>");
        }

        // Tags
        if (Tags.Length > 0)
        {
            WriteHtml("<ul class=\"article-tags\" aria-label=\"Tags\">");
            foreach (var tag in Tags)
            {
                var slug = tag.ToLowerInvariant();
                var basePath = BasePath.TrimEnd('/');
                WriteHtml("<li><a class=\"tag-pill\" href=\"");
                WriteHtml(HtmlEncode($"{basePath}/tag/{slug}"));
                WriteHtml("\">");
                WriteText(tag);
                WriteHtml("</a></li>");
            }

            WriteHtml("</ul>");
        }

        WriteHtml("</div>");
        return Task.CompletedTask;
    }

    private static string HtmlEncode(string value) =>
        System.Net.WebUtility.HtmlEncode(value);
}
