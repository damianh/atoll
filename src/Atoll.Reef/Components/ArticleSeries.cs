using Atoll.Components;

namespace Atoll.Reef.Components;

/// <summary>
/// Renders a multi-part article series indicator showing the current part number,
/// the total part count, and links to all parts in the series.
/// </summary>
/// <remarks>
/// Rendering is delegated to <c>ArticleSeriesTemplate.cshtml</c>.
/// </remarks>
public sealed class ArticleSeries : AtollComponent
{
    /// <summary>Gets or sets the display name of the series.</summary>
    [Parameter(Required = true)]
    public string SeriesName { get; set; } = "";

    /// <summary>Gets or sets all parts in the series.</summary>
    [Parameter(Required = true)]
    public IReadOnlyList<SeriesPart> Parts { get; set; } = [];

    /// <summary>Gets or sets the 1-based index of the currently viewed part.</summary>
    [Parameter(Required = true)]
    public int CurrentPart { get; set; }

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var model = new ArticleSeriesModel(SeriesName, Parts, CurrentPart);

        await ComponentRenderer.RenderSliceAsync<ArticleSeriesTemplate, ArticleSeriesModel>(
            context.Destination,
            model);
    }
}
