using Atoll.Lagoon.I18n;
using Atoll.Lagoon.Navigation;

namespace Atoll.Lagoon.Components;

/// <summary>
/// Model for the <c>PaginationTemplate</c> Razor slice.
/// </summary>
/// <param name="Previous">The link to the previous page, or <c>null</c> if first.</param>
/// <param name="Next">The link to the next page, or <c>null</c> if last.</param>
/// <param name="Translations">The UI translations.</param>
public sealed record PaginationModel(
    PaginationLink? Previous,
    PaginationLink? Next,
    UiTranslations Translations);
