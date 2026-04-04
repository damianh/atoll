using Atoll.Components;

namespace Atoll.Lagoon.Components;

/// <summary>
/// A prominent navigation card rendered as an anchor element, with a title,
/// optional description, and optional icon.
/// </summary>
public sealed class LinkCard : AtollComponent
{
    /// <summary>Gets or sets the card title text.</summary>
    [Parameter(Required = true)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the URL the card links to.</summary>
    [Parameter(Required = true)]
    public string Href { get; set; } = string.Empty;

    /// <summary>Gets or sets an optional description shown beneath the title.</summary>
    [Parameter]
    public string? Description { get; set; }

    /// <summary>Gets or sets an optional icon displayed before the title.</summary>
    [Parameter]
    public IconName? IconName { get; set; }

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml($"<a href=\"{HtmlEncode(Href)}\" class=\"link-card\">");

        WriteHtml("<span class=\"link-card-title\">");

        if (IconName.HasValue)
        {
            var iconProps = new Dictionary<string, object?> { ["Name"] = IconName.Value };
            var iconFragment = ComponentRenderer.ToFragment<Icon>(iconProps);
            await RenderAsync(iconFragment);
        }

        WriteText(Title);
        WriteHtml("</span>");

        if (Description is not null)
        {
            WriteHtml("<span class=\"link-card-description\">");
            WriteText(Description);
            WriteHtml("</span>");
        }

        WriteHtml("</a>");
    }

    private static string HtmlEncode(string value) =>
        System.Net.WebUtility.HtmlEncode(value);
}
