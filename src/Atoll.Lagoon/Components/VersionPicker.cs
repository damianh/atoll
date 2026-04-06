using Atoll.Components;
using Atoll.Lagoon.I18n;
using Atoll.Lagoon.Versioning;

namespace Atoll.Lagoon.Components;

/// <summary>
/// Renders a version switcher dropdown listing all configured versions.
/// Navigates to the equivalent page in the selected version via a native
/// <c>&lt;select&gt;</c> element with an <c>onchange</c> handler.
/// Renders nothing when there are fewer than two versions configured.
/// </summary>
/// <remarks>
/// Rendering is delegated to <c>VersionPickerTemplate.cshtml</c>.
/// </remarks>
public sealed class VersionPicker : AtollComponent
{
    /// <summary>Gets or sets the version map from <c>DocsConfig.Versions</c>.</summary>
    [Parameter]
    public IReadOnlyDictionary<string, VersionConfig>? Versions { get; set; }

    /// <summary>Gets or sets the current version key (e.g. <c>"current"</c> or <c>"v1.0"</c>).</summary>
    [Parameter]
    public string CurrentVersionKey { get; set; } = "";

    /// <summary>Gets or sets the content path without version prefix, used to build switch URLs.</summary>
    [Parameter]
    public string CurrentContentPath { get; set; } = "/";

    /// <summary>Gets or sets the locale URL prefix (e.g. <c>"/fr"</c>), or empty.</summary>
    [Parameter]
    public string LocalePrefix { get; set; } = "";

    /// <summary>Gets or sets the site base path (e.g. <c>"/docs"</c>).</summary>
    [Parameter]
    public string BasePath { get; set; } = "";

    /// <summary>Gets or sets the UI translations. Defaults to English.</summary>
    [Parameter]
    public UiTranslations Translations { get; set; } = UiTranslations.Default;

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        if (Versions is null || Versions.Count <= 1)
        {
            return;
        }

        var model = new VersionPickerModel(
            Versions, CurrentVersionKey, CurrentContentPath, LocalePrefix, BasePath, Translations);

        await ComponentRenderer.RenderSliceAsync<VersionPickerTemplate, VersionPickerModel>(
            context.Destination,
            model);
    }
}
