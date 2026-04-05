using Atoll.Components;

namespace Atoll.Build.Content.Markdown;

/// <summary>
/// A registry that maps directive names (kebab-case strings) to Atoll component types.
/// Used to resolve component types when processing <c>:::</c> directives in Markdown content
/// and when matching <c>&lt;PascalCaseName&gt;</c> component tags.
/// </summary>
/// <remarks>
/// <para>
/// Components are registered explicitly — there is no automatic scanning. This gives full
/// control over which components are available in a given content collection.
/// </para>
/// <para>
/// When a component is registered, a PascalCase alias derived from the type name is
/// automatically added to allow matching <c>&lt;PascalCaseName&gt;</c> tags in addition
/// to the explicit directive name. For example, <c>Add&lt;CardGrid&gt;("card-grid")</c>
/// also makes <c>"CardGrid"</c> resolvable.
/// </para>
/// <para>
/// Example:
/// <code>
/// var components = new ComponentMap()
///     .Add&lt;Counter&gt;("counter")
///     .Add&lt;Callout&gt;("callout");
/// </code>
/// </para>
/// </remarks>
public sealed class ComponentMap
{
    private readonly Dictionary<string, Type> _map =
        new(StringComparer.OrdinalIgnoreCase);

    // Stores PascalCase type-name aliases (e.g., "CardGrid" for Add<CardGrid>("card-grid")).
    // _map is always checked first; _tagMap is a fallback for tag-name resolution.
    private readonly Dictionary<string, Type> _tagMap =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Registers a component type under the specified directive name.
    /// </summary>
    /// <typeparam name="TComponent">
    /// The component type. Must implement <see cref="IAtollComponent"/> and
    /// have a parameterless constructor.
    /// </typeparam>
    /// <param name="name">
    /// The directive name used in Markdown (e.g., <c>"counter"</c> for <c>:::counter</c>).
    /// </param>
    /// <returns>This <see cref="ComponentMap"/> instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is empty or whitespace, or a component is already registered
    /// under that name.
    /// </exception>
    public ComponentMap Add<TComponent>(string name)
        where TComponent : IAtollComponent, new()
    {
        ArgumentNullException.ThrowIfNull(name);

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Component directive name must not be empty or whitespace.", nameof(name));
        }

        if (_map.ContainsKey(name))
        {
            throw new ArgumentException(
                $"A component is already registered under the name '{name}'.",
                nameof(name));
        }

        _map[name] = typeof(TComponent);

        // Auto-register PascalCase type-name alias for <TagName> syntax.
        // Skip silently if the type name is already in either _map or _tagMap
        // (e.g., Add<Foo>("Foo") — explicit name covers it; no double-registration needed).
        var typeName = typeof(TComponent).Name;
        if (!_map.ContainsKey(typeName) && !_tagMap.ContainsKey(typeName))
        {
            _tagMap[typeName] = typeof(TComponent);
        }

        return this;
    }

    /// <summary>
    /// Resolves the component type registered under the specified directive name.
    /// </summary>
    /// <param name="name">The directive name to resolve.</param>
    /// <returns>The registered component <see cref="Type"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c>.</exception>
    /// <exception cref="KeyNotFoundException">
    /// No component is registered under <paramref name="name"/>. The exception message
    /// lists all registered names.
    /// </exception>
    public Type Resolve(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (_map.TryGetValue(name, out var type))
        {
            return type;
        }

        if (_tagMap.TryGetValue(name, out type))
        {
            return type;
        }

        var registered = _map.Count > 0
            ? string.Join(", ", _map.Keys.Order())
            : "(none)";

        throw new KeyNotFoundException(
            $"No component is registered for directive name '{name}'. " +
            $"Registered names: {registered}.");
    }

    /// <summary>
    /// Attempts to resolve the component type registered under the specified directive name
    /// or its PascalCase type-name alias.
    /// </summary>
    /// <param name="name">The directive name to resolve.</param>
    /// <param name="type">
    /// When this method returns <c>true</c>, contains the registered component type;
    /// otherwise, <c>null</c>.
    /// </param>
    /// <returns><c>true</c> if a component is registered under <paramref name="name"/>; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c>.</exception>
    public bool TryResolve(string name, out Type? type)
    {
        ArgumentNullException.ThrowIfNull(name);
        return _map.TryGetValue(name, out type) || _tagMap.TryGetValue(name, out type);
    }

    /// <summary>
    /// Gets all registered directive names (explicit registrations only; PascalCase aliases are not included).
    /// </summary>
    public IReadOnlyCollection<string> RegisteredNames => _map.Keys;
}
