using Atoll.Components;
using Atoll.Slots;

namespace Atoll.Lagoon.Components;

/// <summary>
/// A content card with a title, optional icon, and slotted body content.
/// </summary>
/// <remarks>
/// Rendering is delegated to <c>CardTemplate.cshtml</c>.
/// </remarks>
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
        var model = new CardModel(Title, IconName);

        var slot = context.Slots.GetSlotFragment(SlotCollection.DefaultSlotName);
        var templateSlots = SlotCollection.FromDefault(slot);

        await ComponentRenderer.RenderSliceAsync<CardTemplate, CardModel>(
            context.Destination,
            model,
            templateSlots);
    }
}
