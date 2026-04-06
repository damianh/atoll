using Atoll.Components;
using Atoll.Slots;

namespace Atoll.Lagoon.Components;

/// <summary>
/// A responsive grid container for <see cref="Card"/> components.
/// </summary>
/// <remarks>
/// Rendering is delegated to <c>CardGridTemplate.cshtml</c>.
/// </remarks>
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
        var model = new CardGridModel(Stagger);

        var slot = context.Slots.GetSlotFragment(SlotCollection.DefaultSlotName);
        var templateSlots = SlotCollection.FromDefault(slot);

        await ComponentRenderer.RenderSliceAsync<CardGridTemplate, CardGridModel>(
            context.Destination,
            model,
            templateSlots);
    }
}
