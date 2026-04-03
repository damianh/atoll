using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Atoll.Rendering;

namespace Atoll.Slots;

/// <summary>
/// Holds a collection of named slots, each represented as a <see cref="RenderFragment"/>.
/// Slots are the Atoll equivalent of Astro's slot system — named insertion points
/// where parent components inject content into child components.
/// </summary>
/// <remarks>
/// <para>
/// Slot rendering is lazy: the <see cref="RenderFragment"/> for a slot is only executed
/// when the child component actually renders it. This means unused slots have zero cost.
/// </para>
/// <para>
/// The default slot is stored under the name <see cref="DefaultSlotName"/>.
/// </para>
/// </remarks>
public sealed class SlotCollection : IReadOnlyDictionary<string, RenderFragment>
{
    /// <summary>
    /// The name used for the default (unnamed) slot.
    /// </summary>
    public const string DefaultSlotName = "default";

    private readonly Dictionary<string, RenderFragment> _slots;

    /// <summary>
    /// An empty <see cref="SlotCollection"/> with no slots.
    /// </summary>
    public static readonly SlotCollection Empty = new([]);

    /// <summary>
    /// Initializes a new <see cref="SlotCollection"/> from the specified dictionary of named slots.
    /// </summary>
    /// <param name="slots">The named slot fragments.</param>
    public SlotCollection(Dictionary<string, RenderFragment> slots)
    {
        ArgumentNullException.ThrowIfNull(slots);
        _slots = slots;
    }

    /// <summary>
    /// Gets a value indicating whether a slot with the specified name exists.
    /// </summary>
    /// <param name="name">The slot name to check.</param>
    /// <returns><c>true</c> if the slot exists; otherwise, <c>false</c>.</returns>
    public bool HasSlot(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        return _slots.ContainsKey(name);
    }

    /// <summary>
    /// Gets a value indicating whether the default slot exists.
    /// </summary>
    public bool HasDefaultSlot => _slots.ContainsKey(DefaultSlotName);

    /// <summary>
    /// Renders the slot with the specified name to the given destination.
    /// If the slot does not exist, nothing is rendered.
    /// </summary>
    /// <param name="name">The slot name.</param>
    /// <param name="destination">The render destination.</param>
    /// <returns>A <see cref="ValueTask"/> representing the render operation.</returns>
    public ValueTask RenderSlotAsync(string name, IRenderDestination destination)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(destination);

        if (_slots.TryGetValue(name, out var fragment))
        {
            return fragment.RenderAsync(destination);
        }

        return default;
    }

    /// <summary>
    /// Renders the slot with the specified name to the given destination.
    /// If the slot does not exist, the <paramref name="fallback"/> is rendered instead.
    /// </summary>
    /// <param name="name">The slot name.</param>
    /// <param name="destination">The render destination.</param>
    /// <param name="fallback">The fallback fragment to render if the slot does not exist.</param>
    /// <returns>A <see cref="ValueTask"/> representing the render operation.</returns>
    public ValueTask RenderSlotAsync(string name, IRenderDestination destination, RenderFragment fallback)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(destination);

        if (_slots.TryGetValue(name, out var fragment))
        {
            return fragment.RenderAsync(destination);
        }

        return fallback.RenderAsync(destination);
    }

    /// <summary>
    /// Gets the <see cref="RenderFragment"/> for the specified slot name.
    /// Returns <see cref="RenderFragment.Empty"/> if the slot does not exist.
    /// </summary>
    /// <param name="name">The slot name.</param>
    /// <returns>The slot's render fragment, or <see cref="RenderFragment.Empty"/> if not found.</returns>
    public RenderFragment GetSlotFragment(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        return _slots.TryGetValue(name, out var fragment) ? fragment : RenderFragment.Empty;
    }

    /// <inheritdoc />
    public int Count => _slots.Count;

    /// <inheritdoc />
    public IEnumerable<string> Keys => _slots.Keys;

    /// <inheritdoc />
    public IEnumerable<RenderFragment> Values => _slots.Values;

    /// <inheritdoc />
    public RenderFragment this[string key] => _slots[key];

    /// <inheritdoc />
    public bool ContainsKey(string key) => _slots.ContainsKey(key);

    /// <inheritdoc />
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out RenderFragment value) =>
        _slots.TryGetValue(key, out value);

    /// <inheritdoc />
    public IEnumerator<KeyValuePair<string, RenderFragment>> GetEnumerator() => _slots.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Creates a new <see cref="SlotCollection"/> containing a single default slot.
    /// </summary>
    /// <param name="defaultSlot">The render fragment for the default slot.</param>
    /// <returns>A new <see cref="SlotCollection"/> with the default slot.</returns>
    public static SlotCollection FromDefault(RenderFragment defaultSlot)
    {
        return new SlotCollection(new Dictionary<string, RenderFragment>
        {
            [DefaultSlotName] = defaultSlot,
        });
    }
}
