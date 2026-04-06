using Atoll.Components;
using Atoll.Lagoon.I18n;
using Atoll.Lagoon.Navigation;

namespace Atoll.Lagoon.Components;

/// <summary>
/// Renders a breadcrumb trail as an accessible <c>&lt;nav&gt;</c> with an ordered list.
/// </summary>
/// <remarks>
/// Rendering is delegated to <c>BreadcrumbsTemplate.cshtml</c>.
/// </remarks>
public sealed class Breadcrumbs : AtollComponent
{
    /// <summary>Gets or sets the breadcrumb items to render.</summary>
    [Parameter(Required = true)]
    public IReadOnlyList<BreadcrumbItem> Items { get; set; } = [];

    /// <summary>Gets or sets the UI translations. Defaults to English.</summary>
    [Parameter]
    public UiTranslations Translations { get; set; } = UiTranslations.Default;

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        if (Items.Count == 0)
        {
            return;
        }

        var model = new BreadcrumbsModel(Items, Translations);

        await ComponentRenderer.RenderSliceAsync<BreadcrumbsTemplate, BreadcrumbsModel>(
            context.Destination,
            model);
    }
}
