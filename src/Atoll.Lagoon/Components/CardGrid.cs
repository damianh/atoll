using Atoll.Components;

namespace Atoll.Lagoon.Components;

/// <summary>
/// A responsive grid container for <see cref="Card"/> components.
/// </summary>
public sealed class CardGrid : AtollComponent
{
    /// <summary>
    /// Gets or sets a value indicating whether alternate cards are offset vertically for a staggered visual effect.
    /// </summary>
    [Parameter]
    public bool Stagger { get; set; }

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var cssClass = Stagger ? "card-grid card-grid-stagger" : "card-grid";
        WriteHtml($"<div class=\"{cssClass}\">");
        await RenderSlotAsync();
        WriteHtml("</div>");
    }
}
