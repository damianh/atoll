using Atoll.Core.Rendering;
using Atoll.Core.Slots;

namespace Atoll.Core.Components;

/// <summary>
/// Abstract base class for Atoll components providing convenience helpers.
/// This is the Atoll equivalent of Astro's component base pattern.
/// </summary>
/// <remarks>
/// <para>
/// Components that extend <see cref="AtollComponent"/> gain access to protected
/// helper methods for writing HTML, rendering slots, and accessing props.
/// The <see cref="RenderContext"/> is available via the <see cref="Context"/> property
/// during <see cref="IAtollComponent.RenderAsync"/>.
/// </para>
/// <para>
/// For simpler components, consider using <see cref="ComponentDelegate"/> instead.
/// </para>
/// </remarks>
public abstract class AtollComponent : IAtollComponent
{
    private RenderContext? _context;

    /// <summary>
    /// Gets the current rendering context. Only available during <see cref="IAtollComponent.RenderAsync"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when accessed outside of an <see cref="IAtollComponent.RenderAsync"/> call.
    /// </exception>
    protected RenderContext Context =>
        _context ?? throw new InvalidOperationException("Context is only available during RenderAsync.");

    /// <summary>
    /// Gets the component's props dictionary. Only available during <see cref="IAtollComponent.RenderAsync"/>.
    /// </summary>
    protected IReadOnlyDictionary<string, object?> Props => Context.Props;

    /// <summary>
    /// Gets the component's slot collection. Only available during <see cref="IAtollComponent.RenderAsync"/>.
    /// </summary>
    protected SlotCollection Slots => Context.Slots;

    /// <inheritdoc />
    public async Task RenderAsync(RenderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var previous = _context;
        _context = context;
        try
        {
            await RenderCoreAsync(context);
        }
        finally
        {
            _context = previous;
        }
    }

    /// <summary>
    /// When implemented in a derived class, renders the component's output.
    /// </summary>
    /// <param name="context">The rendering context.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous render operation.</returns>
    protected abstract Task RenderCoreAsync(RenderContext context);

    /// <summary>
    /// Gets a typed prop value by name.
    /// </summary>
    /// <typeparam name="T">The expected type of the prop value.</typeparam>
    /// <param name="name">The prop name.</param>
    /// <returns>The typed prop value.</returns>
    protected T GetProp<T>(string name) => Context.GetProp<T>(name);

    /// <summary>
    /// Gets a typed prop value by name, or the specified default if the prop does not exist.
    /// </summary>
    /// <typeparam name="T">The expected type of the prop value.</typeparam>
    /// <param name="name">The prop name.</param>
    /// <param name="defaultValue">The default value to return if the prop is not found.</param>
    /// <returns>The typed prop value, or <paramref name="defaultValue"/> if not found.</returns>
    protected T GetProp<T>(string name, T defaultValue) => Context.GetProp(name, defaultValue);

    /// <summary>
    /// Gets a value indicating whether a slot with the specified name exists.
    /// </summary>
    /// <param name="name">The slot name.</param>
    /// <returns><c>true</c> if the slot exists; otherwise, <c>false</c>.</returns>
    protected bool HasSlot(string name) => Context.HasSlot(name);

    /// <summary>
    /// Renders the default slot to the context's destination.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the render operation.</returns>
    protected ValueTask RenderSlotAsync() => Context.RenderSlotAsync();

    /// <summary>
    /// Renders the slot with the specified name to the context's destination.
    /// </summary>
    /// <param name="name">The slot name.</param>
    /// <returns>A <see cref="ValueTask"/> representing the render operation.</returns>
    protected ValueTask RenderSlotAsync(string name) => Context.RenderSlotAsync(name);

    /// <summary>
    /// Renders the slot with the specified name, or the fallback fragment if the slot doesn't exist.
    /// </summary>
    /// <param name="name">The slot name.</param>
    /// <param name="fallback">The fallback fragment.</param>
    /// <returns>A <see cref="ValueTask"/> representing the render operation.</returns>
    protected ValueTask RenderSlotAsync(string name, RenderFragment fallback) =>
        Context.RenderSlotAsync(name, fallback);

    /// <summary>
    /// Writes trusted HTML content directly to the destination.
    /// </summary>
    /// <param name="html">The trusted HTML string.</param>
    protected void WriteHtml(string html) => Context.WriteHtml(html);

    /// <summary>
    /// Writes plain text (HTML-escaped) to the destination.
    /// </summary>
    /// <param name="text">The text to write.</param>
    protected void WriteText(string text) => Context.WriteText(text);

    /// <summary>
    /// Renders a <see cref="RenderFragment"/> to the context's destination.
    /// </summary>
    /// <param name="fragment">The fragment to render.</param>
    /// <returns>A <see cref="ValueTask"/> representing the render operation.</returns>
    protected ValueTask RenderAsync(RenderFragment fragment) => Context.RenderAsync(fragment);
}
