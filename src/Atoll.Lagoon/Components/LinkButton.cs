using Atoll.Components;

namespace Atoll.Lagoon.Components;

/// <summary>
/// A call-to-action button rendered as an anchor element, supporting primary, secondary,
/// and minimal visual variants with an optional icon.
/// </summary>
public sealed class LinkButton : AtollComponent
{
    /// <summary>Gets or sets the URL the button links to.</summary>
    [Parameter(Required = true)]
    public string Href { get; set; } = string.Empty;

    /// <summary>Gets or sets the button label text.</summary>
    [Parameter(Required = true)]
    public string Label { get; set; } = string.Empty;

    /// <summary>Gets or sets the visual style variant. Defaults to <see cref="LinkButtonVariant.Primary"/>.</summary>
    [Parameter]
    public LinkButtonVariant Variant { get; set; } = LinkButtonVariant.Primary;

    /// <summary>Gets or sets the optional icon to display alongside the label.</summary>
    [Parameter]
    public IconName? IconName { get; set; }

    /// <summary>Gets or sets where the icon is placed relative to the label. Defaults to <see cref="IconPlacement.Start"/>.</summary>
    [Parameter]
    public IconPlacement IconPlacement { get; set; } = IconPlacement.Start;

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var variantClass = Variant switch
        {
            LinkButtonVariant.Primary => "link-button link-button-primary",
            LinkButtonVariant.Secondary => "link-button link-button-secondary",
            LinkButtonVariant.Minimal => "link-button link-button-minimal",
            _ => "link-button link-button-primary",
        };

        WriteHtml($"<a href=\"{HtmlEncode(Href)}\" class=\"{variantClass}\">");

        if (IconName.HasValue && IconPlacement == IconPlacement.Start)
        {
            var iconProps = new Dictionary<string, object?> { ["Name"] = IconName.Value };
            var iconFragment = ComponentRenderer.ToFragment<Icon>(iconProps);
            await RenderAsync(iconFragment);
        }

        WriteText(Label);

        if (IconName.HasValue && IconPlacement == IconPlacement.End)
        {
            var iconProps = new Dictionary<string, object?> { ["Name"] = IconName.Value };
            var iconFragment = ComponentRenderer.ToFragment<Icon>(iconProps);
            await RenderAsync(iconFragment);
        }

        WriteHtml("</a>");
    }

    private static string HtmlEncode(string value) =>
        System.Net.WebUtility.HtmlEncode(value);
}
