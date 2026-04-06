using Atoll.Lagoon.I18n;
using Atoll.Lagoon.Navigation;

namespace Atoll.Lagoon.Components;

/// <summary>
/// Model for the <c>BreadcrumbsTemplate</c> Razor slice.
/// </summary>
/// <param name="Items">The breadcrumb items to render.</param>
/// <param name="Translations">The UI translations.</param>
public sealed record BreadcrumbsModel(
    IReadOnlyList<BreadcrumbItem> Items,
    UiTranslations Translations);
