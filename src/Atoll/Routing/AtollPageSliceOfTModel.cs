using Atoll.Components;
using Atoll.Rendering;
using Atoll.Slots;

namespace Atoll.Routing;

/// <summary>
/// Abstract base class for Atoll pages authored as Razor templates with a strongly-typed model.
/// Extends <see cref="AtollSlice{TModel}"/> and implements <see cref="IAtollPage"/> so that
/// source-generated proxy types are discoverable by route discovery and renderable by the SSG.
/// </summary>
/// <typeparam name="TModel">The model type, which serves as the strongly-typed props for the page.</typeparam>
/// <remarks>
/// <para>
/// Razor template files should declare <c>@inherits AtollPageSlice&lt;TModel&gt;</c> combined with
/// <c>@attribute [PageRoute("/path")]</c>. The source-generated proxy type will implement
/// <see cref="IAtollComponent.RenderAsync"/> via this base class, making it directly usable
/// with the existing <c>RouteDiscovery</c>, <c>StaticSiteGenerator</c>, and <c>LayoutResolver</c>
/// without any modifications to those classes.
/// </para>
/// </remarks>
public abstract class AtollPageSlice<TModel> : AtollSlice<TModel>, IAtollPage
{
    /// <summary>
    /// Renders this page to the specified render context.
    /// Sets up the Atoll slot/destination bridge then delegates to the Razor template.
    /// </summary>
    /// <param name="context">The render context providing destination, props, and slots.</param>
    /// <returns>A <see cref="Task"/> representing the render operation.</returns>
    async Task IAtollComponent.RenderAsync(RenderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        Destination = context.Destination;
        Slots = context.Slots;
        await using var writer = new RenderDestinationTextWriter(context.Destination);
        await RenderAsync(writer);
    }
}
