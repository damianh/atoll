using Atoll.Lagoon.I18n;
using Atoll.Lagoon.Versioning;

namespace Atoll.Lagoon.Components;

/// <summary>
/// Model for the <c>VersionPickerTemplate</c> Razor slice.
/// </summary>
/// <param name="Versions">The version map.</param>
/// <param name="CurrentVersionKey">The currently selected version key.</param>
/// <param name="CurrentContentPath">The content path without version prefix.</param>
/// <param name="LocalePrefix">The locale URL prefix.</param>
/// <param name="BasePath">The site base path.</param>
/// <param name="Translations">The UI translations.</param>
public sealed record VersionPickerModel(
    IReadOnlyDictionary<string, VersionConfig> Versions,
    string CurrentVersionKey,
    string CurrentContentPath,
    string LocalePrefix,
    string BasePath,
    UiTranslations Translations);
