using Atoll.Rendering;

namespace Atoll.Slots;

/// <summary>
/// Fluent builder for constructing a <see cref="SlotCollection"/>.
/// Provides a convenient API for defining named slots when composing components.
/// </summary>
/// <remarks>
/// <para>
/// Instead of manually constructing a <c>Dictionary&lt;string, RenderFragment&gt;</c>,
/// use <see cref="SlotBuilder"/> for a fluent, readable slot definition:
/// </para>
/// <code>
/// var slots = new SlotBuilder()
///     .Default(RenderFragment.FromHtml("&lt;p&gt;Main content&lt;/p&gt;"))
///     .Named("header", RenderFragment.FromHtml("&lt;h1&gt;Title&lt;/h1&gt;"))
///     .Named("footer", RenderFragment.FromHtml("&lt;footer&gt;Footer&lt;/footer&gt;"))
///     .Build();
/// </code>
/// <para>
/// Slots are lazy: the <see cref="RenderFragment"/> for each slot is only executed
/// when the child component actually renders it via <c>RenderSlotAsync</c>.
/// </para>
/// </remarks>
public sealed class SlotBuilder
{
    private readonly Dictionary<string, RenderFragment> _slots = [];

    /// <summary>
    /// Defines the default (unnamed) slot with the specified <see cref="RenderFragment"/>.
    /// </summary>
    /// <param name="content">The render fragment for the default slot.</param>
    /// <returns>This builder instance for chaining.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the default slot has already been defined.
    /// </exception>
    public SlotBuilder Default(RenderFragment content)
    {
        return Named(SlotCollection.DefaultSlotName, content);
    }

    /// <summary>
    /// Defines the default (unnamed) slot with trusted HTML content.
    /// </summary>
    /// <param name="html">The trusted HTML string for the default slot.</param>
    /// <returns>This builder instance for chaining.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the default slot has already been defined.
    /// </exception>
    public SlotBuilder DefaultHtml(string html)
    {
        ArgumentNullException.ThrowIfNull(html);
        return Default(RenderFragment.FromHtml(html));
    }

    /// <summary>
    /// Defines the default (unnamed) slot with plain text that will be HTML-escaped.
    /// </summary>
    /// <param name="text">The text for the default slot.</param>
    /// <returns>This builder instance for chaining.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the default slot has already been defined.
    /// </exception>
    public SlotBuilder DefaultText(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        return Default(RenderFragment.FromText(text));
    }

    /// <summary>
    /// Defines a named slot with the specified <see cref="RenderFragment"/>.
    /// </summary>
    /// <param name="name">The slot name.</param>
    /// <param name="content">The render fragment for the slot.</param>
    /// <returns>This builder instance for chaining.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a slot with the specified name has already been defined.
    /// </exception>
    public SlotBuilder Named(string name, RenderFragment content)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (!_slots.TryAdd(name, content))
        {
            throw new InvalidOperationException(
                $"Slot '{name}' has already been defined. Each slot name must be unique.");
        }

        return this;
    }

    /// <summary>
    /// Defines a named slot with trusted HTML content.
    /// </summary>
    /// <param name="name">The slot name.</param>
    /// <param name="html">The trusted HTML string for the slot.</param>
    /// <returns>This builder instance for chaining.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a slot with the specified name has already been defined.
    /// </exception>
    public SlotBuilder NamedHtml(string name, string html)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(html);
        return Named(name, RenderFragment.FromHtml(html));
    }

    /// <summary>
    /// Defines a named slot with plain text that will be HTML-escaped.
    /// </summary>
    /// <param name="name">The slot name.</param>
    /// <param name="text">The text for the slot.</param>
    /// <returns>This builder instance for chaining.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a slot with the specified name has already been defined.
    /// </exception>
    public SlotBuilder NamedText(string name, string text)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(text);
        return Named(name, RenderFragment.FromText(text));
    }

    /// <summary>
    /// Defines a named slot with an async rendering function.
    /// </summary>
    /// <param name="name">The slot name.</param>
    /// <param name="renderer">The async function that writes content to a destination.</param>
    /// <returns>This builder instance for chaining.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a slot with the specified name has already been defined.
    /// </exception>
    public SlotBuilder NamedAsync(string name, Func<IRenderDestination, ValueTask> renderer)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(renderer);
        return Named(name, RenderFragment.FromAsync(renderer));
    }

    /// <summary>
    /// Defines the default (unnamed) slot with an async rendering function.
    /// </summary>
    /// <param name="renderer">The async function that writes content to a destination.</param>
    /// <returns>This builder instance for chaining.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the default slot has already been defined.
    /// </exception>
    public SlotBuilder DefaultAsync(Func<IRenderDestination, ValueTask> renderer)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        return Default(RenderFragment.FromAsync(renderer));
    }

    /// <summary>
    /// Gets the number of slots defined so far.
    /// </summary>
    public int Count => _slots.Count;

    /// <summary>
    /// Gets a value indicating whether a slot with the specified name has been defined.
    /// </summary>
    /// <param name="name">The slot name to check.</param>
    /// <returns><c>true</c> if the slot has been defined; otherwise, <c>false</c>.</returns>
    public bool HasSlot(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        return _slots.ContainsKey(name);
    }

    /// <summary>
    /// Builds the <see cref="SlotCollection"/> from all defined slots.
    /// </summary>
    /// <returns>A new <see cref="SlotCollection"/> containing all defined slots.</returns>
    /// <remarks>
    /// After calling <see cref="Build"/>, the builder is reset and can be reused
    /// to build another <see cref="SlotCollection"/>.
    /// </remarks>
    public SlotCollection Build()
    {
        if (_slots.Count == 0)
        {
            return SlotCollection.Empty;
        }

        var result = new SlotCollection(new Dictionary<string, RenderFragment>(_slots));
        _slots.Clear();
        return result;
    }
}
