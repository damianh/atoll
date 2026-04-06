using Atoll.Lagoon.Configuration;
using Atoll.Lagoon.Navigation;

namespace Atoll.Lagoon.Components;

/// <summary>
/// Model for the <c>SidebarLinkTemplate</c> Razor slice.
/// </summary>
/// <param name="Item">The resolved sidebar item to render.</param>
public sealed record SidebarLinkModel(ResolvedSidebarItem Item);
