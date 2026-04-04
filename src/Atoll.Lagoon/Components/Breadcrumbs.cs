using Atoll.Components;
using Atoll.Lagoon.I18n;
using Atoll.Lagoon.Navigation;

namespace Atoll.Lagoon.Components;

/// <summary>
/// Renders a breadcrumb trail as an accessible <c>&lt;nav&gt;</c> with an ordered list.
/// </summary>
public sealed class Breadcrumbs : AtollComponent
{
    /// <summary>Gets or sets the breadcrumb items to render.</summary>
    [Parameter(Required = true)]
    public IReadOnlyList<BreadcrumbItem> Items { get; set; } = [];

    /// <summary>Gets or sets the UI translations. Defaults to English.</summary>
    [Parameter]
    public UiTranslations Translations { get; set; } = UiTranslations.Default;

    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context)
    {
        if (Items.Count == 0)
        {
            return Task.CompletedTask;
        }

        WriteHtml($"<nav class=\"docs-breadcrumbs\" aria-label=\"{HtmlEncode(Translations.BreadcrumbsLabel)}\"><ol>");

        foreach (var item in Items)
        {
            if (item.IsCurrent)
            {
                WriteHtml("<li aria-current=\"page\">");
                WriteText(item.Label);
                WriteHtml("</li>");
            }
            else
            {
                WriteHtml($"<li><a href=\"{HtmlEncode(item.Href ?? "")}\">");
                WriteText(item.Label);
                WriteHtml("</a></li>");
            }
        }

        WriteHtml("</ol></nav>");
        return Task.CompletedTask;
    }

    private static string HtmlEncode(string value) =>
        System.Net.WebUtility.HtmlEncode(value);
}
