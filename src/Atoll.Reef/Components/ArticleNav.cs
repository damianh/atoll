using Atoll.Components;

namespace Atoll.Reef.Components;

/// <summary>
/// Renders a previous/next navigation bar between individual articles.
/// Links are only rendered for non-<see langword="null"/> values; the component
/// renders nothing when both <see cref="Previous"/> and <see cref="Next"/> are
/// <see langword="null"/>.
/// </summary>
/// <remarks>
/// Rendering is delegated to <c>ArticleNavTemplate.cshtml</c>.
/// </remarks>
public sealed class ArticleNav : AtollComponent
{
    /// <summary>Gets or sets the link to the previous article, or <see langword="null"/> if none.</summary>
    [Parameter]
    public ArticleNavLink? Previous { get; set; }

    /// <summary>Gets or sets the link to the next article, or <see langword="null"/> if none.</summary>
    [Parameter]
    public ArticleNavLink? Next { get; set; }

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        if (Previous is null && Next is null)
        {
            return;
        }

        var model = new ArticleNavModel(Previous, Next);

        await ComponentRenderer.RenderSliceAsync<ArticleNavTemplate, ArticleNavModel>(
            context.Destination,
            model);
    }
}
