namespace Atoll.Lagoon.Configuration;

/// <summary>
/// Controls the colour variant of the site-wide announcement banner.
/// Each variant maps to the corresponding <c>--aside-*</c> CSS custom property set
/// so that banner and aside colours stay in sync across light and dark themes.
/// </summary>
public enum BannerVariant
{
    /// <summary>Blue — informational announcement (maps to <c>--aside-note-*</c> tokens).</summary>
    Info,

    /// <summary>Amber — cautionary announcement (maps to <c>--aside-caution-*</c> tokens).</summary>
    Warning,

    /// <summary>Green — positive announcement (maps to <c>--aside-tip-*</c> tokens).</summary>
    Success,

    /// <summary>Red — critical announcement (maps to <c>--aside-danger-*</c> tokens).</summary>
    Danger,
}
