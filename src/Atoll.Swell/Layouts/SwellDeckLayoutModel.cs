using Atoll.Swell.Markdown;

namespace Atoll.Swell.Layouts;

/// <summary>
/// Model passed to the <c>SwellDeckLayoutTemplate.cshtml</c> Razor template.
/// Carries deck-wide configuration and per-slide rendered data for the full page output.
/// </summary>
/// <param name="Config">Deck-wide configuration from the headmatter.</param>
/// <param name="Slides">Pre-rendered slide data in deck order.</param>
/// <param name="AspectRatioCss">
/// The CSS <c>aspect-ratio</c> value to apply to the deck container (e.g. <c>"16/9"</c>).
/// </param>
public sealed record SwellDeckLayoutModel(
    DeckConfig Config,
    IReadOnlyList<RenderedSlideEntry> Slides,
    string AspectRatioCss);

/// <summary>
/// A single rendered slide entry ready for inclusion in the deck layout template.
/// </summary>
/// <param name="Index">Zero-based slide index (used as <c>data-slide-index</c>).</param>
/// <param name="Config">Per-slide configuration.</param>
/// <param name="Notes">Presenter notes extracted from HTML comments.</param>
public sealed record RenderedSlideEntry(
    int Index,
    SlideConfig Config,
    string Notes);
