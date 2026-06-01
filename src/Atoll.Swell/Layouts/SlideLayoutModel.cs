using Atoll.Swell.Markdown;

namespace Atoll.Swell.Layouts;

/// <summary>
/// Shared model passed to all slide layout Razor templates. Carries the per-slide
/// configuration, slot content, and computed display state.
/// </summary>
/// <param name="Config">The per-slide configuration (layout, background, class, transition).</param>
/// <param name="SlideIndex">One-based slide number (used for display).</param>
/// <param name="TotalSlides">Total number of slides in the deck (used for display).</param>
/// <param name="ShowSlideNumber">Whether to render the slide number element on this slide.</param>
public sealed record SlideLayoutModel(
    SlideConfig Config,
    int SlideIndex,
    int TotalSlides,
    bool ShowSlideNumber);
