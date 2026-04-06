using Atoll.Components;
using Atoll.Lagoon.I18n;
using Atoll.Lagoon.Navigation;

namespace Atoll.Lagoon.Components;

/// <summary>
/// Renders previous/next navigation links at the bottom of documentation pages.
/// </summary>
/// <remarks>
/// Rendering is delegated to <c>PaginationTemplate.cshtml</c>.
/// </remarks>
public sealed class Pagination : AtollComponent
{
    /// <summary>Gets or sets the link to the previous page, or <c>null</c> if this is the first page.</summary>
    [Parameter]
    public PaginationLink? Previous { get; set; }

    /// <summary>Gets or sets the link to the next page, or <c>null</c> if this is the last page.</summary>
    [Parameter]
    public PaginationLink? Next { get; set; }

    /// <summary>Gets or sets the UI translations. Defaults to English.</summary>
    [Parameter]
    public UiTranslations Translations { get; set; } = UiTranslations.Default;

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        if (Previous is null && Next is null)
        {
            return;
        }

        var model = new PaginationModel(Previous, Next, Translations);

        await ComponentRenderer.RenderSliceAsync<PaginationTemplate, PaginationModel>(
            context.Destination,
            model);
    }
}
