namespace Atoll.Components;

/// <summary>
/// Defines the contract for an Atoll component.
/// This is the Atoll equivalent of Astro's component factory pattern
/// <c>(SSRResult, props, slots) => RenderTemplateResult</c>.
/// </summary>
/// <remarks>
/// <para>
/// Components are the fundamental building blocks of Atoll pages.
/// They receive a <see cref="RenderContext"/> that provides access to
/// props, slots, rendering helpers, and request-scoped data.
/// </para>
/// <para>
/// Components can be implemented as classes (implementing this interface or
/// extending <see cref="AtollComponent"/>) or as delegates via
/// <see cref="ComponentDelegate"/>.
/// </para>
/// </remarks>
public interface IAtollComponent
{
    /// <summary>
    /// Renders this component's output to the destination provided by the <paramref name="context"/>.
    /// </summary>
    /// <param name="context">
    /// The rendering context providing access to props, slots, and the render destination.
    /// </param>
    /// <returns>A <see cref="Task"/> representing the asynchronous render operation.</returns>
    Task RenderAsync(RenderContext context);
}
