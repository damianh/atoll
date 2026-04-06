using Atoll.Lagoon.Configuration;
using Atoll.Lagoon.Navigation;

namespace Atoll.Lagoon.Components;

/// <summary>
/// Model for the <c>SidebarGroupTemplate</c> Razor slice.
/// </summary>
/// <param name="Group">The resolved group item to render.</param>
/// <param name="GroupIndex">The sequential index assigned to this group.</param>
/// <param name="PositionClass">The CSS class for chevron position.</param>
/// <param name="ChevronPosition">The chevron position enum value, propagated to child groups.</param>
/// <param name="Counter">The shared group index counter, propagated to child groups.</param>
public sealed record SidebarGroupModel(
    ResolvedSidebarItem Group,
    int GroupIndex,
    string PositionClass,
    SidebarChevronPosition ChevronPosition,
    GroupIndexCounter Counter);
