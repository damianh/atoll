using Atoll.Components;

namespace Atoll.Reef.Components;

/// <summary>
/// Renders OpenGraph and Twitter Card meta tags in the document <c>&lt;head&gt;</c>
/// for improved social sharing and SEO for individual article pages.
/// </summary>
public sealed class OpenGraphMeta : AtollComponent
{
    /// <summary>Gets or sets the article title for <c>og:title</c> and <c>twitter:title</c>.</summary>
    [Parameter(Required = true)]
    public string Title { get; set; } = "";

    /// <summary>Gets or sets the article description for <c>og:description</c> and <c>twitter:description</c>.</summary>
    [Parameter]
    public string? Description { get; set; }

    /// <summary>Gets or sets the absolute URL of the featured image for <c>og:image</c> and <c>twitter:image</c>.</summary>
    [Parameter]
    public string? ImageUrl { get; set; }

    /// <summary>Gets or sets the canonical URL of the page for <c>og:url</c>.</summary>
    [Parameter]
    public string? Url { get; set; }

    /// <summary>Gets or sets the author display name for <c>article:author</c>.</summary>
    [Parameter]
    public string? Author { get; set; }

    /// <summary>Gets or sets the publication date for <c>article:published_time</c>.</summary>
    [Parameter]
    public DateTime? PubDate { get; set; }

    /// <summary>Gets or sets the site name for <c>og:site_name</c>.</summary>
    [Parameter]
    public string SiteName { get; set; } = "";

    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteMeta("og:type", "article");
        WriteMeta("og:title", Title);

        if (!string.IsNullOrEmpty(SiteName))
        {
            WriteMeta("og:site_name", SiteName);
        }

        if (!string.IsNullOrEmpty(Description))
        {
            WriteMeta("og:description", Description);
        }

        if (!string.IsNullOrEmpty(Url))
        {
            WriteMeta("og:url", Url);
        }

        if (!string.IsNullOrEmpty(ImageUrl))
        {
            WriteMeta("og:image", ImageUrl);
            WriteNameMeta("twitter:image", ImageUrl);
        }

        if (!string.IsNullOrEmpty(Author))
        {
            WriteMeta("article:author", Author);
        }

        if (PubDate.HasValue)
        {
            WriteMeta("article:published_time",
                PubDate.Value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"));
        }

        WriteHtml("<meta name=\"twitter:card\" content=\"summary_large_image\" />");
        WriteNameMeta("twitter:title", Title);

        if (!string.IsNullOrEmpty(Description))
        {
            WriteNameMeta("twitter:description", Description);
        }

        return Task.CompletedTask;
    }

    private void WriteMeta(string property, string content)
    {
        WriteHtml("<meta property=\"");
        WriteHtml(HtmlEncode(property));
        WriteHtml("\" content=\"");
        WriteHtml(HtmlEncode(content));
        WriteHtml("\" />");
    }

    private void WriteNameMeta(string name, string content)
    {
        WriteHtml("<meta name=\"");
        WriteHtml(HtmlEncode(name));
        WriteHtml("\" content=\"");
        WriteHtml(HtmlEncode(content));
        WriteHtml("\" />");
    }

    private static string HtmlEncode(string value) =>
        System.Net.WebUtility.HtmlEncode(value);
}
