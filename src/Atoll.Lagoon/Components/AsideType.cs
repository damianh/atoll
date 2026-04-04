namespace Atoll.Lagoon.Components;

/// <summary>
/// Defines the visual variant and semantic meaning of an <see cref="Aside"/> callout.
/// </summary>
public enum AsideType
{
    /// <summary>A general informational note.</summary>
    Note,

    /// <summary>A helpful tip or best-practice suggestion.</summary>
    Tip,

    /// <summary>A caution or warning about potential issues.</summary>
    Caution,

    /// <summary>A danger notice about critical or destructive actions.</summary>
    Danger,
}
