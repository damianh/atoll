using Atoll.Components;
using Atoll.Slots;

namespace Atoll.Lagoon.Components;

/// <summary>
/// A styled ordered-list wrapper for step-by-step instructions.
/// CSS counter-based numbering handles the visual circles and connecting lines.
/// Slot content is expected to contain an <c>&lt;ol&gt;</c> element.
/// </summary>
/// <remarks>
/// Rendering is delegated to <c>StepsTemplate.cshtml</c>.
/// </remarks>
public sealed class Steps : AtollComponent
{
    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var slot = context.Slots.GetSlotFragment(SlotCollection.DefaultSlotName);
        var templateSlots = SlotCollection.FromDefault(slot);

        await ComponentRenderer.RenderSliceAsync<StepsTemplate>(
            context.Destination,
            templateSlots);
    }
}
