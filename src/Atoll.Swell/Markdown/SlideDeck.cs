namespace Atoll.Swell.Markdown;

/// <summary>
/// Represents a fully parsed Swell slide deck, containing deck-level configuration
/// and the ordered list of individual slides.
/// </summary>
/// <param name="Config">Deck-wide configuration from the headmatter.</param>
/// <param name="Slides">The ordered list of slides in the deck.</param>
public sealed record SlideDeck(DeckConfig Config, IReadOnlyList<SlideData> Slides);
