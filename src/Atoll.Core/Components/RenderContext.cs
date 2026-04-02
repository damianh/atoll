using System.Collections.ObjectModel;
using Atoll.Core.Rendering;
using Atoll.Core.Slots;

namespace Atoll.Core.Components;

/// <summary>
/// Provides the rendering context for an Atoll component.
/// This is the Atoll equivalent of Astro's <c>Astro</c> global object.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="RenderContext"/> is the primary interface through which components access
/// their props, slots, and the render destination. It also provides helper methods
/// for creating <see cref="RenderFragment"/> values from HTML or text content.
/// </para>
/// <para>
/// A new <see cref="RenderContext"/> is created for each component render invocation.
/// It captures the component's destination, props, and slots.
/// </para>
/// </remarks>
public sealed class RenderContext
{
    private readonly IRenderDestination _destination;

    /// <summary>
    /// Initializes a new <see cref="RenderContext"/> with the specified destination, props, and slots.
    /// </summary>
    /// <param name="destination">The render destination to write output to.</param>
    /// <param name="props">The component's props dictionary.</param>
    /// <param name="slots">The component's slot collection.</param>
    public RenderContext(
        IRenderDestination destination,
        IReadOnlyDictionary<string, object?> props,
        SlotCollection slots)
    {
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentNullException.ThrowIfNull(props);
        ArgumentNullException.ThrowIfNull(slots);
        _destination = destination;
        Props = props;
        Slots = slots;
    }

    /// <summary>
    /// Initializes a new <see cref="RenderContext"/> with the specified destination
    /// and no props or slots.
    /// </summary>
    /// <param name="destination">The render destination to write output to.</param>
    public RenderContext(IRenderDestination destination)
        : this(destination, EmptyProps, SlotCollection.Empty)
    {
    }

    /// <summary>
    /// Initializes a new <see cref="RenderContext"/> with the specified destination and props,
    /// and no slots.
    /// </summary>
    /// <param name="destination">The render destination to write output to.</param>
    /// <param name="props">The component's props dictionary.</param>
    public RenderContext(IRenderDestination destination, IReadOnlyDictionary<string, object?> props)
        : this(destination, props, SlotCollection.Empty)
    {
    }

    /// <summary>
    /// Initializes a new <see cref="RenderContext"/> with the specified destination and slots,
    /// and no props.
    /// </summary>
    /// <param name="destination">The render destination to write output to.</param>
    /// <param name="slots">The component's slot collection.</param>
    public RenderContext(IRenderDestination destination, SlotCollection slots)
        : this(destination, EmptyProps, slots)
    {
    }

    /// <summary>
    /// Gets the component's props dictionary.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Props { get; }

    /// <summary>
    /// Gets the component's slot collection.
    /// </summary>
    public SlotCollection Slots { get; }

    /// <summary>
    /// Gets a typed prop value by name.
    /// </summary>
    /// <typeparam name="T">The expected type of the prop value.</typeparam>
    /// <param name="name">The prop name.</param>
    /// <returns>The typed prop value.</returns>
    /// <exception cref="KeyNotFoundException">The prop name does not exist.</exception>
    /// <exception cref="InvalidCastException">The prop value cannot be cast to <typeparamref name="T"/>.</exception>
    public T GetProp<T>(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (!Props.TryGetValue(name, out var value))
        {
            throw new KeyNotFoundException($"Prop '{name}' not found.");
        }

        return (T)value!;
    }

    /// <summary>
    /// Gets a typed prop value by name, or the specified default if the prop does not exist.
    /// </summary>
    /// <typeparam name="T">The expected type of the prop value.</typeparam>
    /// <param name="name">The prop name.</param>
    /// <param name="defaultValue">The default value to return if the prop is not found.</param>
    /// <returns>The typed prop value, or <paramref name="defaultValue"/> if not found.</returns>
    public T GetProp<T>(string name, T defaultValue)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (Props.TryGetValue(name, out var value) && value is T typed)
        {
            return typed;
        }

        return defaultValue;
    }

    /// <summary>
    /// Gets a value indicating whether a slot with the specified name exists.
    /// </summary>
    /// <param name="name">The slot name to check.</param>
    /// <returns><c>true</c> if the slot exists; otherwise, <c>false</c>.</returns>
    public bool HasSlot(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        return Slots.HasSlot(name);
    }

    /// <summary>
    /// Renders the slot with the specified name to the context's destination.
    /// If the slot does not exist, nothing is rendered.
    /// </summary>
    /// <param name="name">The slot name.</param>
    /// <returns>A <see cref="ValueTask"/> representing the render operation.</returns>
    public ValueTask RenderSlotAsync(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        return Slots.RenderSlotAsync(name, _destination);
    }

    /// <summary>
    /// Renders the default slot to the context's destination.
    /// If the default slot does not exist, nothing is rendered.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the render operation.</returns>
    public ValueTask RenderSlotAsync()
    {
        return Slots.RenderSlotAsync(SlotCollection.DefaultSlotName, _destination);
    }

    /// <summary>
    /// Renders the slot with the specified name to the context's destination.
    /// If the slot does not exist, the <paramref name="fallback"/> is rendered instead.
    /// </summary>
    /// <param name="name">The slot name.</param>
    /// <param name="fallback">The fallback fragment to render if the slot does not exist.</param>
    /// <returns>A <see cref="ValueTask"/> representing the render operation.</returns>
    public ValueTask RenderSlotAsync(string name, RenderFragment fallback)
    {
        ArgumentNullException.ThrowIfNull(name);
        return Slots.RenderSlotAsync(name, _destination, fallback);
    }

    /// <summary>
    /// Writes trusted HTML content directly to the destination.
    /// The content will not be escaped.
    /// </summary>
    /// <param name="html">The trusted HTML string.</param>
    public void WriteHtml(string html)
    {
        ArgumentNullException.ThrowIfNull(html);
        if (html.Length > 0)
        {
            _destination.Write(RenderChunk.Html(html));
        }
    }

    /// <summary>
    /// Writes plain text to the destination. The text will be HTML-escaped.
    /// </summary>
    /// <param name="text">The text to write.</param>
    public void WriteText(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        if (text.Length > 0)
        {
            _destination.Write(RenderChunk.Text(text));
        }
    }

    /// <summary>
    /// Renders a <see cref="RenderFragment"/> to the context's destination.
    /// </summary>
    /// <param name="fragment">The fragment to render.</param>
    /// <returns>A <see cref="ValueTask"/> representing the render operation.</returns>
    public ValueTask RenderAsync(RenderFragment fragment)
    {
        return fragment.RenderAsync(_destination);
    }

    /// <summary>
    /// Creates a <see cref="RenderFragment"/> from trusted HTML content.
    /// </summary>
    /// <param name="html">The trusted HTML string.</param>
    /// <returns>A new <see cref="RenderFragment"/>.</returns>
    public static RenderFragment Html(string html)
    {
        return RenderFragment.FromHtml(html);
    }

    /// <summary>
    /// Creates a <see cref="RenderFragment"/> from plain text that will be HTML-escaped.
    /// </summary>
    /// <param name="text">The text to escape and render.</param>
    /// <returns>A new <see cref="RenderFragment"/>.</returns>
    public static RenderFragment Text(string text)
    {
        return RenderFragment.FromText(text);
    }

    private static readonly IReadOnlyDictionary<string, object?> EmptyProps =
        new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?>());
}
