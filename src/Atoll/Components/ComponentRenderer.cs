using System.Collections.ObjectModel;
using System.Reflection;
using Atoll.Islands;
using Atoll.Rendering;
using Atoll.Slots;
using RazorSlices;

namespace Atoll.Components;

/// <summary>
/// Resolves and renders Atoll components by type or delegate.
/// Handles creating component instances, binding <see cref="ParameterAttribute"/>-marked
/// properties from the props dictionary, and invoking the render method.
/// </summary>
/// <remarks>
/// <para>
/// The renderer supports two component models:
/// </para>
/// <list type="bullet">
/// <item>
/// <description>
/// <strong>Class-based:</strong> Types implementing <see cref="IAtollComponent"/>.
/// The renderer creates an instance, binds parameters, and calls <see cref="IAtollComponent.RenderAsync"/>.
/// </description>
/// </item>
/// <item>
/// <description>
/// <strong>Functional:</strong> <see cref="ComponentDelegate"/> delegates.
/// The renderer invokes the delegate directly with the context.
/// </description>
/// </item>
/// </list>
/// </remarks>
public sealed class ComponentRenderer
{
    /// <summary>
    /// Renders a component of the specified type to the given destination.
    /// If the component implements <see cref="IClientComponent"/> and has a client
    /// directive attribute, it is automatically wrapped in an <c>&lt;atoll-island&gt;</c>
    /// element for client-side hydration.
    /// </summary>
    /// <typeparam name="TComponent">The component type. Must implement <see cref="IAtollComponent"/>
    /// and have a parameterless constructor.</typeparam>
    /// <param name="destination">The render destination.</param>
    /// <param name="props">The props dictionary.</param>
    /// <param name="slots">The slot collection.</param>
    /// <returns>A <see cref="Task"/> representing the async render operation.</returns>
    public static Task RenderComponentAsync<TComponent>(
        IRenderDestination destination,
        IReadOnlyDictionary<string, object?> props,
        SlotCollection slots)
        where TComponent : IAtollComponent, new()
    {
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentNullException.ThrowIfNull(props);
        ArgumentNullException.ThrowIfNull(slots);

        // Island detection: if TComponent is an island with a directive, wrap in <atoll-island>.
        if (typeof(IClientComponent).IsAssignableFrom(typeof(TComponent)))
        {
            var probe = new TComponent();
            if (probe is IClientComponent clientComponent)
            {
                var metadata = IslandMetadataFactory.Create(clientComponent);
                if (metadata is not null)
                {
                    return IslandRenderer.RenderIslandAsync(destination, metadata, typeof(TComponent), props, slots);
                }
            }
        }

        return RenderComponentCoreAsync<TComponent>(destination, props, slots);
    }

    /// <summary>
    /// Renders a component of the specified type to the given destination without island
    /// detection. Used internally by <see cref="IslandRenderer"/> to render the SSR content
    /// inside the <c>&lt;atoll-island&gt;</c> wrapper, avoiding infinite recursion.
    /// </summary>
    internal static Task RenderComponentCoreAsync<TComponent>(
        IRenderDestination destination,
        IReadOnlyDictionary<string, object?> props,
        SlotCollection slots)
        where TComponent : IAtollComponent, new()
    {
        var component = new TComponent();
        BindParameters(component, props);
        var context = new RenderContext(destination, props, slots);
        return component.RenderAsync(context);
    }

    /// <summary>
    /// Renders a component of the specified type to the given destination with no slots.
    /// </summary>
    /// <typeparam name="TComponent">The component type.</typeparam>
    /// <param name="destination">The render destination.</param>
    /// <param name="props">The props dictionary.</param>
    /// <returns>A <see cref="Task"/> representing the async render operation.</returns>
    public static Task RenderComponentAsync<TComponent>(
        IRenderDestination destination,
        IReadOnlyDictionary<string, object?> props)
        where TComponent : IAtollComponent, new()
    {
        return RenderComponentAsync<TComponent>(destination, props, SlotCollection.Empty);
    }

    /// <summary>
    /// Renders a component of the specified type to the given destination with no props or slots.
    /// </summary>
    /// <typeparam name="TComponent">The component type.</typeparam>
    /// <param name="destination">The render destination.</param>
    /// <returns>A <see cref="Task"/> representing the async render operation.</returns>
    public static Task RenderComponentAsync<TComponent>(IRenderDestination destination)
        where TComponent : IAtollComponent, new()
    {
        return RenderComponentAsync<TComponent>(destination, EmptyProps, SlotCollection.Empty);
    }

    /// <summary>
    /// Renders a pre-existing component instance to the given destination.
    /// Parameters are bound from the props dictionary before rendering.
    /// If the component implements <see cref="IClientComponent"/> and has a client
    /// directive attribute, it is automatically wrapped in an <c>&lt;atoll-island&gt;</c>
    /// element for client-side hydration.
    /// </summary>
    /// <param name="component">The component instance.</param>
    /// <param name="destination">The render destination.</param>
    /// <param name="props">The props dictionary.</param>
    /// <param name="slots">The slot collection.</param>
    /// <returns>A <see cref="Task"/> representing the async render operation.</returns>
    public static Task RenderComponentAsync(
        IAtollComponent component,
        IRenderDestination destination,
        IReadOnlyDictionary<string, object?> props,
        SlotCollection slots)
    {
        ArgumentNullException.ThrowIfNull(component);
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentNullException.ThrowIfNull(props);
        ArgumentNullException.ThrowIfNull(slots);

        // Island detection: if the instance is an island with a directive, wrap in <atoll-island>.
        if (component is IClientComponent clientComponent)
        {
            var metadata = IslandMetadataFactory.Create(clientComponent);
            if (metadata is not null)
            {
                return IslandRenderer.RenderIslandAsync(destination, metadata, component.GetType(), props, slots);
            }
        }

        return RenderComponentCoreAsync(component, destination, props, slots);
    }

    /// <summary>
    /// Renders a pre-existing component instance to the given destination without island
    /// detection. Used internally by <see cref="IslandRenderer"/> to render the SSR content
    /// inside the <c>&lt;atoll-island&gt;</c> wrapper, avoiding infinite recursion.
    /// </summary>
    internal static Task RenderComponentCoreAsync(
        IAtollComponent component,
        IRenderDestination destination,
        IReadOnlyDictionary<string, object?> props,
        SlotCollection slots)
    {
        BindParameters(component, props);
        var context = new RenderContext(destination, props, slots);
        return component.RenderAsync(context);
    }

    /// <summary>
    /// Renders a pre-existing component instance to the given destination with no slots.
    /// </summary>
    /// <param name="component">The component instance.</param>
    /// <param name="destination">The render destination.</param>
    /// <param name="props">The props dictionary.</param>
    /// <returns>A <see cref="Task"/> representing the async render operation.</returns>
    public static Task RenderComponentAsync(
        IAtollComponent component,
        IRenderDestination destination,
        IReadOnlyDictionary<string, object?> props)
    {
        return RenderComponentAsync(component, destination, props, SlotCollection.Empty);
    }

    /// <summary>
    /// Renders a pre-existing component instance to the given destination with no props or slots.
    /// </summary>
    /// <param name="component">The component instance.</param>
    /// <param name="destination">The render destination.</param>
    /// <returns>A <see cref="Task"/> representing the async render operation.</returns>
    public static Task RenderComponentAsync(
        IAtollComponent component,
        IRenderDestination destination)
    {
        return RenderComponentAsync(component, destination, EmptyProps, SlotCollection.Empty);
    }

    /// <summary>
    /// Renders a <see cref="ComponentDelegate"/> to the given destination.
    /// </summary>
    /// <param name="componentDelegate">The functional component delegate.</param>
    /// <param name="destination">The render destination.</param>
    /// <param name="props">The props dictionary.</param>
    /// <param name="slots">The slot collection.</param>
    /// <returns>A <see cref="Task"/> representing the async render operation.</returns>
    public static Task RenderDelegateAsync(
        ComponentDelegate componentDelegate,
        IRenderDestination destination,
        IReadOnlyDictionary<string, object?> props,
        SlotCollection slots)
    {
        ArgumentNullException.ThrowIfNull(componentDelegate);
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentNullException.ThrowIfNull(props);
        ArgumentNullException.ThrowIfNull(slots);

        var context = new RenderContext(destination, props, slots);
        return componentDelegate(context);
    }

    /// <summary>
    /// Renders a <see cref="ComponentDelegate"/> to the given destination with no slots.
    /// </summary>
    /// <param name="componentDelegate">The functional component delegate.</param>
    /// <param name="destination">The render destination.</param>
    /// <param name="props">The props dictionary.</param>
    /// <returns>A <see cref="Task"/> representing the async render operation.</returns>
    public static Task RenderDelegateAsync(
        ComponentDelegate componentDelegate,
        IRenderDestination destination,
        IReadOnlyDictionary<string, object?> props)
    {
        return RenderDelegateAsync(componentDelegate, destination, props, SlotCollection.Empty);
    }

    /// <summary>
    /// Renders a <see cref="ComponentDelegate"/> to the given destination with no props or slots.
    /// </summary>
    /// <param name="componentDelegate">The functional component delegate.</param>
    /// <param name="destination">The render destination.</param>
    /// <returns>A <see cref="Task"/> representing the async render operation.</returns>
    public static Task RenderDelegateAsync(
        ComponentDelegate componentDelegate,
        IRenderDestination destination)
    {
        return RenderDelegateAsync(componentDelegate, destination, EmptyProps, SlotCollection.Empty);
    }

    /// <summary>
    /// Creates a <see cref="RenderFragment"/> that renders the specified component type when evaluated.
    /// </summary>
    /// <typeparam name="TComponent">The component type.</typeparam>
    /// <param name="props">The props dictionary.</param>
    /// <param name="slots">The slot collection.</param>
    /// <returns>A <see cref="RenderFragment"/> that renders the component.</returns>
    public static RenderFragment ToFragment<TComponent>(
        IReadOnlyDictionary<string, object?> props,
        SlotCollection slots)
        where TComponent : IAtollComponent, new()
    {
        ArgumentNullException.ThrowIfNull(props);
        ArgumentNullException.ThrowIfNull(slots);

        return RenderFragment.FromAsync(async destination =>
        {
            await RenderComponentAsync<TComponent>(destination, props, slots);
        });
    }

    /// <summary>
    /// Creates a <see cref="RenderFragment"/> that renders the specified component type with no slots.
    /// </summary>
    /// <typeparam name="TComponent">The component type.</typeparam>
    /// <param name="props">The props dictionary.</param>
    /// <returns>A <see cref="RenderFragment"/> that renders the component.</returns>
    public static RenderFragment ToFragment<TComponent>(IReadOnlyDictionary<string, object?> props)
        where TComponent : IAtollComponent, new()
    {
        return ToFragment<TComponent>(props, SlotCollection.Empty);
    }

    /// <summary>
    /// Creates a <see cref="RenderFragment"/> that renders the specified component type
    /// with no props or slots.
    /// </summary>
    /// <typeparam name="TComponent">The component type.</typeparam>
    /// <returns>A <see cref="RenderFragment"/> that renders the component.</returns>
    public static RenderFragment ToFragment<TComponent>()
        where TComponent : IAtollComponent, new()
    {
        return ToFragment<TComponent>(EmptyProps, SlotCollection.Empty);
    }

    /// <summary>
    /// Creates a <see cref="RenderFragment"/> that renders the specified delegate when evaluated.
    /// </summary>
    /// <param name="componentDelegate">The functional component delegate.</param>
    /// <param name="props">The props dictionary.</param>
    /// <param name="slots">The slot collection.</param>
    /// <returns>A <see cref="RenderFragment"/> that renders the delegate.</returns>
    public static RenderFragment ToFragment(
        ComponentDelegate componentDelegate,
        IReadOnlyDictionary<string, object?> props,
        SlotCollection slots)
    {
        ArgumentNullException.ThrowIfNull(componentDelegate);
        ArgumentNullException.ThrowIfNull(props);
        ArgumentNullException.ThrowIfNull(slots);

        return RenderFragment.FromAsync(async destination =>
        {
            await RenderDelegateAsync(componentDelegate, destination, props, slots);
        });
    }

    /// <summary>
    /// Creates a <see cref="RenderFragment"/> that renders the specified delegate with no slots.
    /// </summary>
    /// <param name="componentDelegate">The functional component delegate.</param>
    /// <param name="props">The props dictionary.</param>
    /// <returns>A <see cref="RenderFragment"/> that renders the delegate.</returns>
    public static RenderFragment ToFragment(
        ComponentDelegate componentDelegate,
        IReadOnlyDictionary<string, object?> props)
    {
        return ToFragment(componentDelegate, props, SlotCollection.Empty);
    }

    /// <summary>
    /// Creates a <see cref="RenderFragment"/> that renders the specified delegate
    /// with no props or slots.
    /// </summary>
    /// <param name="componentDelegate">The functional component delegate.</param>
    /// <returns>A <see cref="RenderFragment"/> that renders the delegate.</returns>
    public static RenderFragment ToFragment(ComponentDelegate componentDelegate)
    {
        return ToFragment(componentDelegate, EmptyProps, SlotCollection.Empty);
    }

    // ── Razor Slice rendering methods ──

    /// <summary>
    /// Renders a Razor slice of the specified type to the given destination.
    /// </summary>
    /// <typeparam name="TSlice">The Razor slice type. Must implement <see cref="IRazorSliceProxy"/>.</typeparam>
    /// <param name="destination">The render destination.</param>
    /// <returns>A <see cref="ValueTask"/> representing the async render operation.</returns>
    public static ValueTask RenderSliceAsync<TSlice>(IRenderDestination destination)
        where TSlice : IRazorSliceProxy
    {
        ArgumentNullException.ThrowIfNull(destination);
        return RenderSliceAsync<TSlice>(destination, SlotCollection.Empty);
    }

    /// <summary>
    /// Renders a Razor slice of the specified type to the given destination with the specified slots.
    /// </summary>
    /// <typeparam name="TSlice">The Razor slice type. Must implement <see cref="IRazorSliceProxy"/>.</typeparam>
    /// <param name="destination">The render destination.</param>
    /// <param name="slots">The slot collection.</param>
    /// <returns>A <see cref="ValueTask"/> representing the async render operation.</returns>
    public static async ValueTask RenderSliceAsync<TSlice>(IRenderDestination destination, SlotCollection slots)
        where TSlice : IRazorSliceProxy
    {
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentNullException.ThrowIfNull(slots);

        using var slice = TSlice.CreateSlice();
        InjectAtollSliceContext(slice, destination, slots);
        await using var writer = new RenderDestinationTextWriter(destination);
        await slice.RenderAsync(writer);
    }

    /// <summary>
    /// Renders a typed Razor slice of the specified type to the given destination with the specified model.
    /// </summary>
    /// <typeparam name="TSlice">The Razor slice type. Must implement <see cref="IRazorSliceProxy{TModel}"/>.</typeparam>
    /// <typeparam name="TModel">The model type.</typeparam>
    /// <param name="destination">The render destination.</param>
    /// <param name="model">The model instance.</param>
    /// <returns>A <see cref="ValueTask"/> representing the async render operation.</returns>
    public static ValueTask RenderSliceAsync<TSlice, TModel>(IRenderDestination destination, TModel model)
        where TSlice : IRazorSliceProxy<TModel>
    {
        ArgumentNullException.ThrowIfNull(destination);
        return RenderSliceAsync<TSlice, TModel>(destination, model, SlotCollection.Empty);
    }

    /// <summary>
    /// Renders a typed Razor slice of the specified type to the given destination with the specified model and slots.
    /// </summary>
    /// <typeparam name="TSlice">The Razor slice type. Must implement <see cref="IRazorSliceProxy{TModel}"/>.</typeparam>
    /// <typeparam name="TModel">The model type.</typeparam>
    /// <param name="destination">The render destination.</param>
    /// <param name="model">The model instance.</param>
    /// <param name="slots">The slot collection.</param>
    /// <returns>A <see cref="ValueTask"/> representing the async render operation.</returns>
    public static async ValueTask RenderSliceAsync<TSlice, TModel>(
        IRenderDestination destination,
        TModel model,
        SlotCollection slots)
        where TSlice : IRazorSliceProxy<TModel>
    {
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentNullException.ThrowIfNull(slots);

        using var slice = TSlice.CreateSlice(model);
        InjectAtollSliceContext(slice, destination, slots);
        await using var writer = new RenderDestinationTextWriter(destination);
        await slice.RenderAsync(writer);
    }

    /// <summary>
    /// Creates a <see cref="RenderFragment"/> that renders the specified Razor slice type when evaluated.
    /// </summary>
    /// <typeparam name="TSlice">The Razor slice type. Must implement <see cref="IRazorSliceProxy"/>.</typeparam>
    /// <returns>A <see cref="RenderFragment"/> that renders the slice.</returns>
    public static RenderFragment ToSliceFragment<TSlice>()
        where TSlice : IRazorSliceProxy
    {
        return ToSliceFragment<TSlice>(SlotCollection.Empty);
    }

    /// <summary>
    /// Creates a <see cref="RenderFragment"/> that renders the specified Razor slice type
    /// with the specified slots when evaluated.
    /// </summary>
    /// <typeparam name="TSlice">The Razor slice type. Must implement <see cref="IRazorSliceProxy"/>.</typeparam>
    /// <param name="slots">The slot collection.</param>
    /// <returns>A <see cref="RenderFragment"/> that renders the slice.</returns>
    public static RenderFragment ToSliceFragment<TSlice>(SlotCollection slots)
        where TSlice : IRazorSliceProxy
    {
        ArgumentNullException.ThrowIfNull(slots);

        return RenderFragment.FromAsync(destination => RenderSliceAsync<TSlice>(destination, slots));
    }

    /// <summary>
    /// Creates a <see cref="RenderFragment"/> that renders the specified typed Razor slice type
    /// with the specified model when evaluated.
    /// </summary>
    /// <typeparam name="TSlice">The Razor slice type. Must implement <see cref="IRazorSliceProxy{TModel}"/>.</typeparam>
    /// <typeparam name="TModel">The model type.</typeparam>
    /// <param name="model">The model instance.</param>
    /// <returns>A <see cref="RenderFragment"/> that renders the slice.</returns>
    public static RenderFragment ToSliceFragment<TSlice, TModel>(TModel model)
        where TSlice : IRazorSliceProxy<TModel>
    {
        return ToSliceFragment<TSlice, TModel>(model, SlotCollection.Empty);
    }

    /// <summary>
    /// Creates a <see cref="RenderFragment"/> that renders the specified typed Razor slice type
    /// with the specified model and slots when evaluated.
    /// </summary>
    /// <typeparam name="TSlice">The Razor slice type. Must implement <see cref="IRazorSliceProxy{TModel}"/>.</typeparam>
    /// <typeparam name="TModel">The model type.</typeparam>
    /// <param name="model">The model instance.</param>
    /// <param name="slots">The slot collection.</param>
    /// <returns>A <see cref="RenderFragment"/> that renders the slice.</returns>
    public static RenderFragment ToSliceFragment<TSlice, TModel>(TModel model, SlotCollection slots)
        where TSlice : IRazorSliceProxy<TModel>
    {
        ArgumentNullException.ThrowIfNull(slots);

        return RenderFragment.FromAsync(
            destination => RenderSliceAsync<TSlice, TModel>(destination, model, slots));
    }

    /// <summary>
    /// Injects Atoll context (destination and slots) into the slice if it implements
    /// <see cref="IAtollSliceContext"/>.
    /// </summary>
    private static void InjectAtollSliceContext(
        RazorSlice slice,
        IRenderDestination destination,
        SlotCollection slots)
    {
        if (slice is IAtollSliceContext atollContext)
        {
            atollContext.Destination = destination;
            atollContext.Slots = slots;
        }
    }

    /// <summary>
    /// Binds <see cref="ParameterAttribute"/>-marked properties on the component
    /// from the props dictionary.
    /// </summary>
    private static void BindParameters(IAtollComponent component, IReadOnlyDictionary<string, object?> props)
    {
        var componentType = component.GetType();
        var properties = componentType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            var paramAttr = property.GetCustomAttribute<ParameterAttribute>();
            if (paramAttr is null)
            {
                continue;
            }

            if (!property.CanWrite)
            {
                throw new InvalidOperationException(
                    $"Property '{property.Name}' on component '{componentType.Name}' is marked with " +
                    $"[Parameter] but has no setter.");
            }

            // Look up case-insensitively in the props dictionary
            var propValue = FindPropValue(props, property.Name, out var found);

            if (!found)
            {
                if (paramAttr.Required)
                {
                    throw new InvalidOperationException(
                        $"Required parameter '{property.Name}' was not provided for component '{componentType.Name}'.");
                }

                continue;
            }

            if (propValue is null)
            {
                if (property.PropertyType.IsValueType && Nullable.GetUnderlyingType(property.PropertyType) is null)
                {
                    throw new InvalidOperationException(
                        $"Cannot assign null to non-nullable value type parameter '{property.Name}' " +
                        $"(type '{property.PropertyType.Name}') on component '{componentType.Name}'.");
                }

                property.SetValue(component, null);
            }
            else
            {
                var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

                if (!targetType.IsAssignableFrom(propValue.GetType()))
                {
                    // Attempt conversion for common types
                    try
                    {
                        var converted = Convert.ChangeType(propValue, targetType);
                        property.SetValue(component, converted);
                    }
                    catch (Exception ex) when (ex is InvalidCastException or FormatException or OverflowException)
                    {
                        throw new InvalidOperationException(
                            $"Cannot convert prop '{property.Name}' value of type '{propValue.GetType().Name}' " +
                            $"to parameter type '{property.PropertyType.Name}' on component '{componentType.Name}'.",
                            ex);
                    }
                }
                else
                {
                    property.SetValue(component, propValue);
                }
            }
        }
    }

    private static object? FindPropValue(
        IReadOnlyDictionary<string, object?> props,
        string propertyName,
        out bool found)
    {
        // Try exact match first
        if (props.TryGetValue(propertyName, out var value))
        {
            found = true;
            return value;
        }

        // Try case-insensitive match
        foreach (var kvp in props)
        {
            if (string.Equals(kvp.Key, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                found = true;
                return kvp.Value;
            }
        }

        found = false;
        return null;
    }

    private static readonly IReadOnlyDictionary<string, object?> EmptyProps =
        new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?>());
}
