using Atoll.Lagoon.Configuration;
using Atoll.Lagoon.Navigation;

namespace Atoll.Lagoon.Components;

/// <summary>
/// Model for the <c>SidebarTemplate</c> Razor slice.
/// </summary>
/// <param name="Items">The resolved sidebar items to render.</param>
/// <param name="Hash">The structural fingerprint hash of the sidebar.</param>
/// <param name="NavLabel">The HTML-encoded aria-label for the nav element.</param>
/// <param name="ChevronPosition">The chevron position for collapsible group indicators.</param>
/// <param name="Counter">The shared group index counter.</param>
public sealed record SidebarModel(
    IReadOnlyList<ResolvedSidebarItem> Items,
    string Hash,
    string NavLabel,
    SidebarChevronPosition ChevronPosition,
    GroupIndexCounter Counter);
