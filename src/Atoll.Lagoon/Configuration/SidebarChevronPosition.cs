namespace Atoll.Lagoon.Configuration;

/// <summary>
/// Controls the position of the collapse/expand chevron indicator
/// on sidebar group headings.
/// </summary>
public enum SidebarChevronPosition
{
    /// <summary>
    /// Chevron is displayed after the group label (right side in LTR layouts).
    /// This matches the Astro Starlight default.
    /// </summary>
    End,

    /// <summary>
    /// Chevron is displayed before the group label (left side in LTR layouts).
    /// This matches the Duende IdentityServer docs style.
    /// </summary>
    Start,
}
