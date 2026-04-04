namespace Atoll.Lagoon.I18n;

/// <summary>
/// Represents the configuration for a single locale in a multi-language documentation site.
/// Each locale maps to a URL path prefix (or <c>"root"</c> for the default locale).
/// </summary>
public sealed record LocaleConfig
{
    /// <summary>Gets the display label for this locale (e.g. "English", "Français").</summary>
    public required string Label { get; init; }

    /// <summary>Gets the BCP-47 language tag (e.g. "en", "fr", "zh-CN"). Used for the HTML <c>lang</c> attribute.</summary>
    public required string Lang { get; init; }

    /// <summary>Gets the text direction. Defaults to <c>"ltr"</c>.</summary>
    public string Dir { get; init; } = "ltr";

    /// <summary>Gets the per-locale UI translation overrides. Falls back to <see cref="UiTranslations.Default"/>.</summary>
    public UiTranslations Translations { get; init; } = UiTranslations.Default;
}
