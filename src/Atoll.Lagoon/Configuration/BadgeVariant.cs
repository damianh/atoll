namespace Atoll.Lagoon.Configuration;

/// <summary>
/// Controls the colour variant of a sidebar badge, mirroring Starlight's badge variants.
/// </summary>
public enum BadgeVariant
{
    /// <summary>Default accent colour (site primary/accent).</summary>
    Default,

    /// <summary>Blue — informational note.</summary>
    Note,

    /// <summary>Green — helpful tip.</summary>
    Tip,

    /// <summary>Green — positive outcome or recommended option.</summary>
    Success,

    /// <summary>Amber — proceed with caution.</summary>
    Caution,

    /// <summary>Red — dangerous or destructive action.</summary>
    Danger,
}
