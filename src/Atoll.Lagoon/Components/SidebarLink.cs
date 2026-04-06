using Atoll.Components;
using Atoll.Lagoon.Configuration;
using Atoll.Lagoon.Navigation;

namespace Atoll.Lagoon.Components;

/// <summary>
/// Renders a single sidebar navigation link.
/// </summary>
/// <remarks>
/// Rendering is delegated to <c>SidebarLinkTemplate.cshtml</c>.
/// </remarks>
public sealed class SidebarLink : AtollComponent
{
    /// <summary>Gets or sets the resolved sidebar item to render.</summary>
    [Parameter(Required = true)]
    public ResolvedSidebarItem Item { get; set; } = null!;

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var model = new SidebarLinkModel(Item);

        await ComponentRenderer.RenderSliceAsync<SidebarLinkTemplate, SidebarLinkModel>(
            context.Destination,
            model);
    }

    internal static string BadgeCssClass(BadgeVariant variant) => variant switch
    {
        BadgeVariant.Default => "sidebar-badge",
        _ => $"sidebar-badge sidebar-badge-{variant.ToString().ToLowerInvariant()}",
    };
}
