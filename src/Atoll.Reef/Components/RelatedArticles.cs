using Atoll.Components;

namespace Atoll.Reef.Components;

/// <summary>
/// Renders a "Related Articles" section with a heading and a list of article links,
/// typically populated by <c>RelatedArticlesResolver</c> based on shared tags.
/// </summary>
/// <remarks>
/// Rendering is delegated to <c>RelatedArticlesTemplate.cshtml</c>.
/// </remarks>
public sealed class RelatedArticles : AtollComponent
{
    /// <summary>Gets or sets the related article links to display.</summary>
    [Parameter(Required = true)]
    public IReadOnlyList<ArticleNavLink> Articles { get; set; } = [];

    /// <summary>Gets or sets the section heading. Defaults to <c>"Related Articles"</c>.</summary>
    [Parameter]
    public string Heading { get; set; } = "Related Articles";

    /// <summary>Gets or sets the maximum number of items to render. Defaults to <c>3</c>.</summary>
    [Parameter]
    public int MaxItems { get; set; } = 3;

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var items = Articles.Take(MaxItems).ToList();
        if (items.Count == 0)
        {
            return;
        }

        var model = new RelatedArticlesModel(items, Heading);

        await ComponentRenderer.RenderSliceAsync<RelatedArticlesTemplate, RelatedArticlesModel>(
            context.Destination,
            model);
    }
}
