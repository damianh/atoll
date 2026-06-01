using Atoll.Components;
using Atoll.Slots;
using Atoll.Swell.Markdown;

namespace Atoll.Swell.Layouts;

/// <summary>
/// The top-level layout that renders the full HTML page for a Swell slide deck.
/// Produces the complete HTML document including <c>&lt;head&gt;</c>,
/// <c>&lt;body&gt;</c>, and all slide <c>&lt;section&gt;</c> elements.
/// </summary>
/// <remarks>
/// The default slot receives all pre-rendered slide sections (each wrapped in a
/// <c>&lt;section&gt;</c> with <c>data-slide-index</c>). <c>SwellPage</c> constructs
/// and passes this slot.
/// </remarks>
public sealed class SwellDeckLayout : AtollComponent
{
    private static readonly IReadOnlyDictionary<string, string> AspectRatioMap =
        new Dictionary<string, string>
        {
            [nameof(Configuration.AspectRatio.Ratio16x9)] = "16/9",
            [nameof(Configuration.AspectRatio.Ratio4x3)] = "4/3",
            [nameof(Configuration.AspectRatio.Ratio3x2)] = "3/2",
        };

    /// <summary>Gets or sets the deck-wide configuration. Required.</summary>
    [Parameter(Required = true)]
    public DeckConfig Config { get; set; } = null!;

    /// <summary>Gets or sets the per-slide entries for metadata (notes, config).</summary>
    [Parameter(Required = true)]
    public IReadOnlyList<RenderedSlideEntry> Slides { get; set; } = null!;

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var aspectRatioCss = ResolveAspectRatioCss(Config.AspectRatio);
        var model = new SwellDeckLayoutModel(Config, Slides, aspectRatioCss);
        var pageSlot = context.Slots.GetSlotFragment(SlotCollection.DefaultSlotName);
        var templateSlots = SlotCollection.FromDefault(pageSlot);

        await ComponentRenderer.RenderSliceAsync<SwellDeckLayoutTemplate, SwellDeckLayoutModel>(
            context.Destination, model, templateSlots);
    }

    private static string ResolveAspectRatioCss(Configuration.AspectRatio ratio) =>
        ratio switch
        {
            Configuration.AspectRatio.Ratio16x9 => "16/9",
            Configuration.AspectRatio.Ratio4x3 => "4/3",
            Configuration.AspectRatio.Ratio3x2 => "3/2",
            _ => "16/9",
        };
}
