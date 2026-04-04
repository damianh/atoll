using Atoll.Components;
using Atoll.Rendering;
using Atoll.Slots;

namespace Atoll.Routing;

/// <summary>
/// Abstract base class for Atoll pages authored as Razor templates (no model).
/// Extends <see cref="AtollSlice"/> and enables Razor-authored pages to participate
/// in the Atoll routing and SSG pipeline.
/// </summary>
/// <remarks>
/// <para>
/// Razor template files should declare <c>@inherits AtollPageSlice</c>. Route declaration
/// must be done in a companion <c>.cs</c> partial class file using the <c>[PageRoute]</c>
/// attribute, because <c>@attribute</c> directives in <c>.cshtml</c> files are applied to the
/// Razor-generated implementation partial, not to the source-generated proxy type that
/// <c>RouteDiscovery</c> scans.
/// </para>
/// <para>
/// Example companion file:
/// <code>
/// [PageRoute("/about")]
/// public partial class AboutPage;
/// </code>
/// </para>
/// <para>
/// Source-generated proxy types do NOT extend this class — the proxy has
/// <c>BaseType = System.Object</c>. Route discovery and SSG detect proxy types
/// via <c>IRazorSliceProxy</c> and instantiate them via <c>CreateSlice()</c>.
/// </para>
/// <para>
/// File-based routing only scans <c>*.cs</c> files. Razor pages declared with
/// <c>[PageRoute(...)]</c> via a companion file are discovered via attribute-based
/// route discovery (<c>RouteDiscovery.DiscoverRoutesFromAttributes()</c>).
/// </para>
/// </remarks>
public abstract class AtollPageSlice : AtollSlice, IAtollPage
{
    /// <summary>
    /// Renders this page to the specified render context.
    /// Sets up the Atoll slot/destination bridge then delegates to the Razor template.
    /// </summary>
    /// <param name="context">The render context providing destination, props, and slots.</param>
    /// <returns>A <see cref="ValueTask"/> representing the render operation.</returns>
    async Task IAtollComponent.RenderAsync(RenderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        Destination = context.Destination;
        Slots = context.Slots;
        await using var writer = new RenderDestinationTextWriter(context.Destination);
        await RenderAsync(writer);
    }
}
