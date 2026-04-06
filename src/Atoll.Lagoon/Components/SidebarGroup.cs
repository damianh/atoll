using Atoll.Components;
using Atoll.Lagoon.Configuration;
using Atoll.Lagoon.Navigation;

namespace Atoll.Lagoon.Components;

/// <summary>
/// Renders a collapsible sidebar group with a heading and child items.
/// Uses an HTML <c>&lt;details&gt;</c> / <c>&lt;summary&gt;</c> element for
/// CSS-only collapse/expand — no JavaScript required.
/// A chevron indicator is rendered whose position is controlled by
/// <see cref="ChevronPosition"/>.
/// </summary>
/// <remarks>
/// <para>
/// Each group consumes one index from the shared <see cref="Counter"/>, emitting
/// it as a <c>data-index</c> attribute on the <c>&lt;details&gt;</c> element and
/// on an <c>&lt;sl-sidebar-restore&gt;</c> custom element inside the group.
/// The client-side script uses these indices to restore open/closed state from
/// <c>sessionStorage</c> without a flash of wrong state.
/// </para>
/// <para>
/// Rendering is delegated to <c>SidebarGroupTemplate.cshtml</c>.
/// </para>
/// </remarks>
public sealed class SidebarGroup : AtollComponent
{
    /// <summary>Gets or sets the resolved group item to render.</summary>
    [Parameter(Required = true)]
    public ResolvedSidebarItem Group { get; set; } = null!;

    /// <summary>
    /// Gets or sets the chevron position for this group's collapse indicator.
    /// Default: <see cref="SidebarChevronPosition.End"/>.
    /// </summary>
    [Parameter]
    public SidebarChevronPosition ChevronPosition { get; set; } = SidebarChevronPosition.End;

    /// <summary>
    /// Gets or sets the shared group index counter. Each group calls
    /// <see cref="GroupIndexCounter.Next"/> once during rendering to obtain
    /// its unique sequential index.
    /// </summary>
    [Parameter(Required = true)]
    public GroupIndexCounter Counter { get; set; } = null!;

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var groupIndex = Counter.Next();
        var positionClass = ChevronPosition == SidebarChevronPosition.Start
            ? "sidebar-chevron-start"
            : "sidebar-chevron-end";

        var model = new SidebarGroupModel(Group, groupIndex, positionClass, ChevronPosition, Counter);

        await ComponentRenderer.RenderSliceAsync<SidebarGroupTemplate, SidebarGroupModel>(
            context.Destination,
            model);
    }
}
