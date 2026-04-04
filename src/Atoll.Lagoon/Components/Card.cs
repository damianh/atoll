using Atoll.Components;

namespace Atoll.Lagoon.Components;

/// <summary>
/// A content card with a title, optional icon, and slotted body content.
/// </summary>
public sealed class Card : AtollComponent
{
    /// <summary>Gets or sets the card title.</summary>
    [Parameter(Required = true)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets an optional icon displayed alongside the title.</summary>
    [Parameter]
    public IconName? IconName { get; set; }

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<div class=\"card\">");

        WriteHtml("<div class=\"card-header\">");

        if (IconName.HasValue)
        {
            var iconProps = new Dictionary<string, object?> { ["Name"] = IconName.Value };
            var iconFragment = ComponentRenderer.ToFragment<Icon>(iconProps);
            await RenderAsync(iconFragment);
        }

        WriteHtml("<h3 class=\"card-title\">");
        WriteText(Title);
        WriteHtml("</h3>");

        WriteHtml("</div>");

        WriteHtml("<div class=\"card-body\">");
        await RenderSlotAsync();
        WriteHtml("</div>");

        WriteHtml("</div>");
    }
}
