using Atoll.Components;

namespace Atoll.Reef.Components;

/// <summary>
/// Renders an article card with optional cover image, title, description, and article metadata.
/// </summary>
/// <remarks>
/// Rendering is delegated to <c>ArticleCardTemplate.cshtml</c>.
/// </remarks>
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

        var model = new ArticleCardModel(
            Title, href, Description, PubDate, Author, Tags,
            ImageSrc, ImageAlt, ReadingTimeMinutes, BasePath);

        await ComponentRenderer.RenderSliceAsync<ArticleCardTemplate, ArticleCardModel>(
            context.Destination,
            model);
    }
}
