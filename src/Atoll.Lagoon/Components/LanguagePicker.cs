using Atoll.Components;
using Atoll.Lagoon.I18n;

namespace Atoll.Lagoon.Components;

/// <summary>
/// Renders a language/locale switcher dropdown listing all configured locales.
/// Navigates to the equivalent page in the selected locale via a native
/// <c>&lt;select&gt;</c> element with an <c>onchange</c> handler.
/// Renders nothing when there are fewer than two locales configured.
/// </summary>
/// <remarks>
/// Rendering is delegated to <c>LanguagePickerTemplate.cshtml</c>.
/// </remarks>
public sealed class LanguagePicker : AtollComponent
{
    /// <summary>Gets or sets the locale map from <see cref="Configuration.DocsConfig.Locales"/>.</summary>
    [Parameter]
    public IReadOnlyDictionary<string, LocaleConfig>? Locales { get; set; }

    /// <summary>Gets or sets the current locale key (e.g. <c>"root"</c> or <c>"fr"</c>).</summary>
    [Parameter]
    public string CurrentLocaleKey { get; set; } = "";

    /// <summary>Gets or sets the content path without locale prefix, used to build switch URLs.</summary>
    [Parameter]
    public string CurrentContentPath { get; set; } = "/";

    /// <summary>Gets or sets the site base path (e.g. <c>"/docs"</c>).</summary>
    [Parameter]
    public string BasePath { get; set; } = "";

    /// <summary>Gets or sets the UI translations. Defaults to English.</summary>
    [Parameter]
    public UiTranslations Translations { get; set; } = UiTranslations.Default;

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        if (Locales is null || Locales.Count <= 1)
        {
            return;
        }

        var model = new LanguagePickerModel(
            Locales, CurrentLocaleKey, CurrentContentPath, BasePath, Translations);

        await ComponentRenderer.RenderSliceAsync<LanguagePickerTemplate, LanguagePickerModel>(
            context.Destination,
            model);
    }
}
