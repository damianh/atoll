using Atoll.Swell.Markdown;

namespace Atoll.Swell.Layouts;

/// <summary>
/// Model for the presenter mode page layout.
/// </summary>
/// <param name="Config">Deck-wide configuration.</param>
/// <param name="Slides">Per-slide entries including presenter notes.</param>
/// <param name="Title">The deck title shown in the presenter window.</param>
public sealed record PresenterLayoutModel(
    DeckConfig Config,
    IReadOnlyList<RenderedSlideEntry> Slides,
    string Title);
