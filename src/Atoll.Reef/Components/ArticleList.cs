using Atoll.Components;

namespace Atoll.Reef.Components;

/// <summary>
/// Renders a compact vertical list of article entries, each showing title, date, and description.
/// </summary>
/// <remarks>
/// Rendering is delegated to <c>ArticleListTemplate.cshtml</c>.
/// </remarks>
public sealed class ArticleList : AtollComponent
{
    /// <summary>Gets or sets the list of article items to display.</summary>
    [Parameter(Required = true)]
    public IReadOnlyList<ArticleListItem> Items { get; set; } = [];

    /// <summary>
    /// Gets or sets the base URL path prefix for the articles site (e.g. <c>"/blog"</c>).
    /// </summary>
    [Parameter]
    public string BasePath { get; set; } = "";

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var model = new ArticleListModel(Items, BasePath.TrimEnd('/'));

        await ComponentRenderer.RenderSliceAsync<ArticleListTemplate, ArticleListModel>(
            context.Destination,
            model);
    }
}
