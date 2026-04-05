using Atoll.Components;

namespace Atoll.Reef.Components;

/// <summary>
/// Renders a responsive CSS grid container that wraps <see cref="ArticleCard"/> components.
/// The number of columns is controlled by the <see cref="Columns"/> parameter and is injected
/// via a CSS custom property (<c>--grid-cols</c>).
/// </summary>
public sealed class ArticleGrid : AtollComponent
{
    /// <summary>
    /// Gets or sets the number of grid columns. Defaults to <c>3</c>.
    /// The value is surfaced as <c>--grid-cols</c> on the container element.
    /// </summary>
    [Parameter]
    public int Columns { get; set; } = 3;

    /// <summary>Gets or sets the list of article items to display as cards.</summary>
    [Parameter(Required = true)]
    public IReadOnlyList<ArticleListItem> Items { get; set; } = [];

    /// <summary>Gets or sets the base URL path prefix for the articles site.</summary>
    [Parameter]
    public string BasePath { get; set; } = "";

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<div class=\"article-grid\" style=\"--grid-cols:");
        WriteHtml(Columns.ToString());
        WriteHtml("\">");

        foreach (var item in Items)
        {
            var cardFragment = ComponentRenderer.ToFragment<ArticleCard>(new Dictionary<string, object?>
            {
                [nameof(ArticleCard.Title)] = item.Title,
                [nameof(ArticleCard.Slug)] = item.Slug,
                [nameof(ArticleCard.Description)] = item.Description,
                [nameof(ArticleCard.PubDate)] = item.PubDate,
                [nameof(ArticleCard.Author)] = item.Author,
                [nameof(ArticleCard.Tags)] = item.Tags,
                [nameof(ArticleCard.ReadingTimeMinutes)] = item.ReadingTimeMinutes,
                [nameof(ArticleCard.BasePath)] = BasePath,
            });
            await RenderAsync(cardFragment);
        }

        WriteHtml("</div>");
    }
}
