namespace Atoll.Swell.Configuration;

/// <summary>
/// CSS transition animations between slides.
/// </summary>
public enum TransitionType
{
    /// <summary>No transition — slides switch instantly.</summary>
    None,

    /// <summary>Cross-fade between slides.</summary>
    Fade,

    /// <summary>Slides move left to reveal the next slide.</summary>
    SlideLeft,

    /// <summary>Slides move right (used when navigating backwards).</summary>
    SlideRight,

    /// <summary>Slides move up to reveal the next slide.</summary>
    SlideUp,
}
