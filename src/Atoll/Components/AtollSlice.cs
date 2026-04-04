using Atoll.Rendering;
using Atoll.Slots;
using RazorSlices;

namespace Atoll.Components;

/// <summary>
/// Abstract base class for Atoll components authored as Razor templates (no model).
/// Extends <see cref="RazorSlice"/> with Atoll-specific slot rendering and component embedding support.
/// </summary>
/// <remarks>
/// <para>
/// Razor template files should declare <c>@inherits AtollSlice</c> to gain access to
/// <see cref="RenderSlotAsync()"/>, <see cref="RenderSlotAsync(string)"/>, and
/// <see cref="RenderComponentAsync{TComponent}()"/> helpers.
/// </para>
/// <para>
/// The <see cref="Destination"/> property is injected by the component adapter before rendering.
/// It cannot be accessed during the constructor.
/// </para>
/// </remarks>
public abstract class AtollSlice : RazorSlice, IAtollSliceContext
{
    /// <summary>
    /// Gets or sets the render destination. Injected by the component adapter before rendering.
    /// </summary>
    internal IRenderDestination? Destination { get; set; }

    /// <inheritdoc />
    IRenderDestination? IAtollSliceContext.Destination
    {
        get => Destination;
        set => Destination = value;
    }

    /// <summary>
    /// Gets or sets the slot collection. Injected by the component adapter before rendering.
    /// Defaults to <see cref="SlotCollection.Empty"/>.
    /// </summary>
    public SlotCollection Slots { get; set; } = SlotCollection.Empty;

    /// <summary>
    /// Gets the current destination, throwing if not set.
    /// </summary>
    private IRenderDestination RequiredDestination =>
        Destination ?? throw new InvalidOperationException(
            "Destination is not set. Ensure the slice is rendered via SliceComponentAdapter or ComponentRenderer.");

    /// <summary>
    /// Renders the default slot to the current destination.
    /// If no default slot exists, nothing is rendered.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the render operation.</returns>
    protected ValueTask RenderSlotAsync()
    {
        return Slots.RenderSlotAsync(SlotCollection.DefaultSlotName, RequiredDestination);
    }

    /// <summary>
    /// Renders the named slot to the current destination.
    /// If the slot does not exist, nothing is rendered.
    /// </summary>
    /// <param name="name">The slot name.</param>
    /// <returns>A <see cref="ValueTask"/> representing the render operation.</returns>
    protected ValueTask RenderSlotAsync(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        return Slots.RenderSlotAsync(name, RequiredDestination);
    }

    /// <summary>
    /// Renders the named slot to the current destination, or the fallback if the slot does not exist.
    /// </summary>
    /// <param name="name">The slot name.</param>
    /// <param name="fallback">The fallback fragment.</param>
    /// <returns>A <see cref="ValueTask"/> representing the render operation.</returns>
    protected ValueTask RenderSlotAsync(string name, RenderFragment fallback)
    {
        ArgumentNullException.ThrowIfNull(name);
        return Slots.RenderSlotAsync(name, RequiredDestination, fallback);
    }

    /// <summary>
    /// Gets a value indicating whether a slot with the specified name exists.
    /// </summary>
    /// <param name="name">The slot name.</param>
    /// <returns><c>true</c> if the slot exists; otherwise, <c>false</c>.</returns>
    protected bool HasSlot(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        return Slots.HasSlot(name);
    }

    /// <summary>
    /// Renders a C# component inline to the current destination.
    /// </summary>
    /// <typeparam name="TComponent">The component type.</typeparam>
    /// <returns>A <see cref="ValueTask"/> representing the render operation.</returns>
    protected async ValueTask RenderComponentAsync<TComponent>()
        where TComponent : IAtollComponent, new()
    {
        await ComponentRenderer.RenderComponentAsync<TComponent>(RequiredDestination);
    }

    /// <summary>
    /// Renders a C# component inline to the current destination with the specified props.
    /// </summary>
    /// <typeparam name="TComponent">The component type.</typeparam>
    /// <param name="props">The props dictionary.</param>
    /// <returns>A <see cref="ValueTask"/> representing the render operation.</returns>
    protected async ValueTask RenderComponentAsync<TComponent>(IReadOnlyDictionary<string, object?> props)
        where TComponent : IAtollComponent, new()
    {
        ArgumentNullException.ThrowIfNull(props);
        await ComponentRenderer.RenderComponentAsync<TComponent>(RequiredDestination, props);
    }

    /// <summary>
    /// Renders a C# component inline to the current destination with the specified props and slots.
    /// </summary>
    /// <typeparam name="TComponent">The component type.</typeparam>
    /// <param name="props">The props dictionary.</param>
    /// <param name="slots">The slot collection.</param>
    /// <returns>A <see cref="ValueTask"/> representing the render operation.</returns>
    protected async ValueTask RenderComponentAsync<TComponent>(
        IReadOnlyDictionary<string, object?> props,
        SlotCollection slots)
        where TComponent : IAtollComponent, new()
    {
        ArgumentNullException.ThrowIfNull(props);
        ArgumentNullException.ThrowIfNull(slots);
        await ComponentRenderer.RenderComponentAsync<TComponent>(RequiredDestination, props, slots);
    }
}
