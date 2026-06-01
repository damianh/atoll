using Atoll.Components;
using Atoll.Slots;
using Atoll.Swell.Markdown;

namespace Atoll.Swell.Layouts;

/// <summary>
/// Base class for all Swell slide layout components. Provides the common parameters
/// and the <see cref="BuildModel"/> helper for subclasses.
/// </summary>
public abstract class SlideLayoutBase : AtollComponent
{
    /// <summary>Gets or sets the per-slide configuration. Required.</summary>
    [Parameter(Required = true)]
    public SlideConfig Config { get; set; } = null!;

    /// <summary>Gets or sets the one-based slide index for display purposes.</summary>
    [Parameter]
    public int SlideIndex { get; set; }

    /// <summary>Gets or sets the total number of slides in the deck.</summary>
    [Parameter]
    public int TotalSlides { get; set; }

    /// <summary>Gets or sets whether to render the slide number on this slide.</summary>
    [Parameter]
    public bool ShowSlideNumber { get; set; }

    /// <summary>Builds the shared <see cref="SlideLayoutModel"/> from current parameters.</summary>
    protected SlideLayoutModel BuildModel() =>
        new(Config, SlideIndex, TotalSlides, ShowSlideNumber);

    /// <summary>
    /// Extracts the default slot fragment and wraps it in a single-slot
    /// <see cref="SlotCollection"/> for passing to the Razor template.
    /// </summary>
    protected static SlotCollection BuildSlots(RenderContext context)
    {
        var slot = context.Slots.GetSlotFragment(SlotCollection.DefaultSlotName);
        return SlotCollection.FromDefault(slot);
    }
}
