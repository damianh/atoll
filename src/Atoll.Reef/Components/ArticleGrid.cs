using Atoll.Components;

namespace Atoll.Reef.Components;

/// <summary>
/// Renders a responsive CSS grid container that wraps <see cref="ArticleCard"/> components.
/// The number of columns is controlled by the <see cref="Columns"/> parameter and is injected
/// via a CSS custom property (<c>--grid-cols</c>).
/// </summary>
/// <remarks>
/// Rendering is delegated to <c>ArticleGridTemplate.cshtml</c>.
/// </remarks>
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
        var model = new ArticleGridModel(Columns, Items, BasePath);

        await ComponentRenderer.RenderSliceAsync<ArticleGridTemplate, ArticleGridModel>(
            context.Destination,
            model);
    }
}
