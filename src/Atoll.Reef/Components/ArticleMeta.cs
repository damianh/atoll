using Atoll.Components;

namespace Atoll.Reef.Components;

/// <summary>
/// Renders the article metadata strip: publication date, optional author,
/// optional reading time, and tag pills with links.
/// </summary>
/// <remarks>
/// Rendering is delegated to <c>ArticleMetaTemplate.cshtml</c>.
/// </remarks>
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
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var model = new ArticleMetaModel(PubDate, Author, ReadingTimeMinutes, Tags, BasePath.TrimEnd('/'));

        await ComponentRenderer.RenderSliceAsync<ArticleMetaTemplate, ArticleMetaModel>(
            context.Destination,
            model);
    }
}
