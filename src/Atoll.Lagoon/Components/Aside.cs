using Atoll.Components;
using Atoll.Slots;

namespace Atoll.Lagoon.Components;

/// <summary>
/// A callout box with note/tip/caution/danger variants, an optional custom title,
/// and slotted content body.
/// </summary>
/// <remarks>
/// Rendering is delegated to <c>AsideTemplate.cshtml</c>.
/// </remarks>
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
        var model = new AsideModel(variantClass, title, iconName);

        var slot = context.Slots.GetSlotFragment(SlotCollection.DefaultSlotName);
        var templateSlots = SlotCollection.FromDefault(slot);

        await ComponentRenderer.RenderSliceAsync<AsideTemplate, AsideModel>(
            context.Destination,
            model,
            templateSlots);
    }
}
