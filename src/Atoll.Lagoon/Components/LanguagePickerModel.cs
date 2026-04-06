using Atoll.Lagoon.I18n;

namespace Atoll.Lagoon.Components;

/// <summary>
/// Model for the <c>LanguagePickerTemplate</c> Razor slice.
/// </summary>
/// <param name="Locales">The locale map.</param>
/// <param name="CurrentLocaleKey">The currently selected locale key.</param>
/// <param name="CurrentContentPath">The content path without locale prefix.</param>
/// <param name="BasePath">The site base path.</param>
/// <param name="Translations">The UI translations.</param>
public sealed record LanguagePickerModel(
    IReadOnlyDictionary<string, LocaleConfig> Locales,
    string CurrentLocaleKey,
    string CurrentContentPath,
    string BasePath,
    UiTranslations Translations);
