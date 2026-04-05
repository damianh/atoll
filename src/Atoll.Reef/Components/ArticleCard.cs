using Atoll.Components;

namespace Atoll.Reef.Components;

/// <summary>
/// Renders an article card with optional cover image, title, description, and article metadata.
/// </summary>
public sealed class ArticleCard : AtollComponent
{
    /// <summary>Gets or sets the article title.</summary>
    [Parameter(Required = true)]
    public string Title { get; set; } = "";

    /// <summary>Gets or sets the article URL slug (relative to <see cref="BasePath"/>).</summary>
    [Parameter(Required = true)]
    public string Slug { get; set; } = "";

    /// <summary>Gets or sets the short article description displayed below the title.</summary>
    [Parameter]
    public string Description { get; set; } = "";

    /// <summary>Gets or sets the article publication date.</summary>
    [Parameter]
    public DateTime PubDate { get; set; }

    /// <summary>Gets or sets the author display name.</summary>
    [Parameter]
    public string? Author { get; set; }

    /// <summary>Gets or sets the tag names associated with this article.</summary>
    [Parameter]
    public string[] Tags { get; set; } = [];

    /// <summary>Gets or sets the URL or path to the article cover image.</summary>
    [Parameter]
    public string? ImageSrc { get; set; }

    /// <summary>Gets or sets the alt text for the cover image.</summary>
    [Parameter]
    public string ImageAlt { get; set; } = "";

    /// <summary>Gets or sets the estimated reading time in minutes.</summary>
    [Parameter]
    public int? ReadingTimeMinutes { get; set; }

    /// <summary>
    /// Gets or sets the base URL path prefix for the articles site (e.g. <c>"/blog"</c>).
    /// </summary>
    [Parameter]
    public string BasePath { get; set; } = "";

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var basePath = BasePath.TrimEnd('/');
        var href = $"{basePath}/{Slug.TrimStart('/')}";

        WriteHtml("<article class=\"article-card\">");

        // Cover image
        if (!string.IsNullOrEmpty(ImageSrc))
        {
            WriteHtml("<a class=\"article-card-image-link\" href=\"");
            WriteHtml(HtmlEncode(href));
            WriteHtml("\" tabindex=\"-1\" aria-hidden=\"true\">");
            WriteHtml("<img class=\"article-card-image\" src=\"");
            WriteHtml(HtmlEncode(ImageSrc));
            WriteHtml("\" alt=\"");
            WriteHtml(HtmlEncode(ImageAlt));
            WriteHtml("\" loading=\"lazy\" />");
            WriteHtml("</a>");
        }

        WriteHtml("<div class=\"article-card-body\">");

        // Title
        WriteHtml("<h3 class=\"article-card-title\">");
        WriteHtml("<a href=\"");
        WriteHtml(HtmlEncode(href));
        WriteHtml("\">");
        WriteText(Title);
        WriteHtml("</a>");
        WriteHtml("</h3>");

        // Description
        if (!string.IsNullOrEmpty(Description))
        {
            WriteHtml("<p class=\"article-card-description\">");
            WriteText(Description);
            WriteHtml("</p>");
        }

        // Meta (date, author, reading time, tags)
        var metaProps = new Dictionary<string, object?>
        {
            [nameof(ArticleMeta.PubDate)] = PubDate,
            [nameof(ArticleMeta.Author)] = Author,
            [nameof(ArticleMeta.ReadingTimeMinutes)] = ReadingTimeMinutes,
            [nameof(ArticleMeta.Tags)] = Tags,
            [nameof(ArticleMeta.BasePath)] = BasePath,
        };
        var metaFragment = ComponentRenderer.ToFragment<ArticleMeta>(metaProps);
        await RenderAsync(metaFragment);

        WriteHtml("</div>");
        WriteHtml("</article>");
    }

    private static string HtmlEncode(string value) =>
        System.Net.WebUtility.HtmlEncode(value);
}
