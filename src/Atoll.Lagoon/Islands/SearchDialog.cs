using Atoll.Components;
using Atoll.Islands;
using Atoll.Lagoon.I18n;

namespace Atoll.Lagoon.Islands;

/// <summary>
/// A search dialog island that renders a search trigger button and a <c>&lt;dialog&gt;</c>
/// overlay. Loads the pre-built JSON search index lazily on first open and performs
/// client-side full-text search. Uses <c>client:idle</c> since search is not critical path.
/// </summary>
/// <remarks>
/// Opens on click or Ctrl+K / ⌘K keyboard shortcut.
/// Results support arrow-key navigation and Enter to navigate.
/// Fetches the search index from <see cref="IndexUrl"/> on first open.
/// </remarks>
[ClientIdle]
public sealed class SearchDialog : VanillaJsIsland
{
    /// <inheritdoc />
    public override string ClientModuleUrl => "/scripts/atoll-docs-search-dialog.js";

    /// <summary>Gets or sets the placeholder text for the search input.</summary>
    [Obsolete("Use UiTranslations.SearchPlaceholder instead.")]
    [Parameter]
    public string Placeholder { get; set; } = "Search docs...";

    /// <summary>
    /// Gets or sets the URL to fetch the search index JSON from.
    /// Defaults to <c>/search-index.json</c> (root-relative).
    /// Set this when your site is hosted at a base path, e.g. <c>/docs/search-index.json</c>.
    /// </summary>
    [Parameter]
    public string IndexUrl { get; set; } = "/search-index.json";

    /// <summary>Gets or sets the UI translations. Defaults to English.</summary>
    [Parameter]
    public UiTranslations Translations { get; set; } = UiTranslations.Default;

#pragma warning disable CS0618 // Obsolete member usage is intentional for backward compatibility
    private string EffectivePlaceholder => Placeholder != "Search docs..." ? Placeholder : Translations.SearchPlaceholder;
#pragma warning restore CS0618

    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<div class=\"search-wrapper\" data-index-url=\"");
        WriteHtml(System.Net.WebUtility.HtmlEncode(IndexUrl));
        WriteHtml("\" data-no-results=\"");
        WriteHtml(System.Net.WebUtility.HtmlEncode(Translations.SearchNoResults));
        WriteHtml("\">");
        WriteHtml($"<button id=\"search-trigger\" type=\"button\" aria-label=\"{System.Net.WebUtility.HtmlEncode(Translations.SearchLabel)}\" aria-haspopup=\"dialog\">");
        WriteText(EffectivePlaceholder);
        WriteHtml(" <kbd>Ctrl+K</kbd>");
        WriteHtml("</button>");

        WriteHtml($"<dialog id=\"search-dialog\" aria-label=\"{System.Net.WebUtility.HtmlEncode(Translations.SearchDialogLabel)}\">");
        WriteHtml("<div class=\"search-dialog-inner\">");
        WriteHtml("<div class=\"search-dialog-header\">");
        WriteHtml("<input id=\"search-input\" type=\"search\" placeholder=\"");
        WriteHtml(System.Net.WebUtility.HtmlEncode(EffectivePlaceholder));
        WriteHtml("\" autofocus />");
        WriteHtml($"<button id=\"search-close\" type=\"button\" aria-label=\"{System.Net.WebUtility.HtmlEncode(Translations.SearchCloseLabel)}\">&times;</button>");
        WriteHtml("</div>");
        WriteHtml($"<div id=\"search-results\" role=\"listbox\" aria-label=\"{System.Net.WebUtility.HtmlEncode(Translations.SearchResultsLabel)}\"></div>");
        WriteHtml("</div>");
        WriteHtml("</dialog>");

        WriteHtml("</div>");
        return Task.CompletedTask;
    }
}
