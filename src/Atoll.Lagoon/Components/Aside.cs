using Atoll.Components;

namespace Atoll.Lagoon.Components;

/// <summary>
/// A callout box with note/tip/caution/danger variants, an optional custom title,
/// and slotted content body.
/// </summary>
public sealed class Aside : AtollComponent
{
    /// <summary>Gets or sets the aside variant. Defaults to <see cref="AsideType.Note"/>.</summary>
    [Parameter]
    public AsideType Type { get; set; } = AsideType.Note;

    /// <summary>Gets or sets a custom title. When <c>null</c>, the variant name is used.</summary>
    [Parameter]
    public string? Title { get; set; }

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var (variantClass, defaultTitle, iconName) = Type switch
        {
            AsideType.Note => ("aside-note", "Note", IconName.Information),
            AsideType.Tip => ("aside-tip", "Tip", IconName.Tip),
            AsideType.Caution => ("aside-caution", "Caution", IconName.Warning),
            AsideType.Danger => ("aside-danger", "Danger", IconName.Danger),
            _ => ("aside-note", "Note", IconName.Information),
        };

        var title = Title ?? defaultTitle;
        var encodedTitle = HtmlEncode(title);

        WriteHtml($"<aside class=\"aside {variantClass}\" role=\"note\" aria-label=\"{encodedTitle}\">");

        // Title line with icon
        WriteHtml("<p class=\"aside-title\">");
        var iconProps = new Dictionary<string, object?> { ["Name"] = iconName };
        var iconFragment = ComponentRenderer.ToFragment<Icon>(iconProps);
        await RenderAsync(iconFragment);
        WriteText(title);
        WriteHtml("</p>");

        // Content
        WriteHtml("<div class=\"aside-content\">");
        await RenderSlotAsync();
        WriteHtml("</div>");

        WriteHtml("</aside>");
    }

    private static string HtmlEncode(string value) =>
        System.Net.WebUtility.HtmlEncode(value);
}
