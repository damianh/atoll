using System.Collections.ObjectModel;
using System.Reflection;
using Atoll.Rendering;
using Atoll.Slots;

namespace Atoll.Components;

/// <summary>
/// Resolves and applies layout wrapping for page components. Reads the
/// <see cref="LayoutAttribute"/> from a component type and wraps the page's
/// rendered content as the default slot of the layout component.
/// </summary>
/// <remarks>
/// <para>
/// The resolver supports nested layouts: if a layout component itself has a
/// <see cref="LayoutAttribute"/>, the resolver chains them so the innermost
/// page content bubbles up through all layout layers.
/// </para>
/// <para>
/// Circular layout references (e.g., Layout A → Layout B → Layout A) are detected
/// and result in an <see cref="InvalidOperationException"/>.
/// </para>
/// </remarks>
public static class LayoutResolver
{
    /// <summary>
    /// Gets the layout type specified by the <see cref="LayoutAttribute"/> on the given type,
    /// or <c>null</c> if no layout is declared.
    /// </summary>
    /// <param name="componentType">The component type to inspect.</param>
    /// <returns>The layout type, or <c>null</c> if no layout attribute is present.</returns>
    public static Type? GetLayoutType(Type componentType)
    {
        ArgumentNullException.ThrowIfNull(componentType);

        var attr = componentType.GetCustomAttribute<LayoutAttribute>(inherit: true);
        return attr?.LayoutType;
    }

    /// <summary>
    /// Gets a value indicating whether the specified type has a <see cref="LayoutAttribute"/>.
    /// </summary>
    /// <param name="componentType">The component type to inspect.</param>
    /// <returns><c>true</c> if the type has a layout attribute; otherwise, <c>false</c>.</returns>
    public static bool HasLayout(Type componentType)
    {
        ArgumentNullException.ThrowIfNull(componentType);

        return componentType.GetCustomAttribute<LayoutAttribute>(inherit: true) is not null;
    }

    /// <summary>
    /// Resolves the full layout chain for the specified component type.
    /// Returns the chain from outermost to innermost layout.
    /// </summary>
    /// <param name="componentType">The component type to start from.</param>
    /// <returns>
    /// A list of layout types, ordered from outermost (first) to innermost (last).
    /// Returns an empty list if the component has no layout.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a circular layout reference is detected.
    /// </exception>
    public static IReadOnlyList<Type> ResolveLayoutChain(Type componentType)
    {
        ArgumentNullException.ThrowIfNull(componentType);

        var chain = new List<Type>();
        var visited = new HashSet<Type>();
        var current = componentType;

        while (true)
        {
            var layoutType = GetLayoutType(current);
            if (layoutType is null)
            {
                break;
            }

            if (!visited.Add(layoutType))
            {
                throw new InvalidOperationException(
                    $"Circular layout reference detected: '{layoutType.FullName}' appears more than once " +
                    $"in the layout chain starting from '{componentType.FullName}'.");
            }

            chain.Add(layoutType);
            current = layoutType;
        }

        // Reverse so outermost layout is first
        chain.Reverse();
        return chain;
    }

    /// <summary>
    /// Wraps the specified page content fragment with all layouts in the chain resolved
    /// from the given component type. Each layout receives the inner content as its
    /// default slot.
    /// </summary>
    /// <param name="componentType">The page component type (used to resolve layouts).</param>
    /// <param name="pageContent">The rendered page content to wrap.</param>
    /// <returns>
    /// A <see cref="RenderFragment"/> that renders the page content wrapped in all applicable
    /// layouts. If no layout is declared, returns <paramref name="pageContent"/> unchanged.
    /// </returns>
    public static RenderFragment WrapWithLayouts(Type componentType, RenderFragment pageContent)
    {
        ArgumentNullException.ThrowIfNull(componentType);

        var chain = ResolveLayoutChain(componentType);
        if (chain.Count == 0)
        {
            return pageContent;
        }

        // Build from inside out: innermost layout wraps the page content,
        // then each outer layout wraps the result of the inner one.
        // chain is ordered outermost-first, so we iterate in reverse.
        var current = pageContent;
        for (var i = chain.Count - 1; i >= 0; i--)
        {
            var layoutType = chain[i];
            current = WrapWithLayout(layoutType, current);
        }

        return current;
    }

    /// <summary>
    /// Wraps the specified page content fragment with all layouts in the chain resolved
    /// from the given component type. Each layout receives the inner content as its
    /// default slot. Props are passed to the innermost layout.
    /// </summary>
    /// <param name="componentType">The page component type (used to resolve layouts).</param>
    /// <param name="pageContent">The rendered page content to wrap.</param>
    /// <param name="props">Props to pass to the innermost layout component.</param>
    /// <returns>
    /// A <see cref="RenderFragment"/> that renders the page content wrapped in all applicable
    /// layouts. If no layout is declared, returns <paramref name="pageContent"/> unchanged.
    /// </returns>
    public static RenderFragment WrapWithLayouts(
        Type componentType,
        RenderFragment pageContent,
        IReadOnlyDictionary<string, object?> props)
    {
        ArgumentNullException.ThrowIfNull(componentType);
        ArgumentNullException.ThrowIfNull(props);

        var chain = ResolveLayoutChain(componentType);
        if (chain.Count == 0)
        {
            return pageContent;
        }

        // Build from inside out: innermost layout gets props + page content as default slot.
        // Outer layouts get only the inner content as default slot (no props).
        var current = pageContent;
        for (var i = chain.Count - 1; i >= 0; i--)
        {
            var layoutType = chain[i];
            if (i == chain.Count - 1)
            {
                // Innermost layout receives props
                current = WrapWithLayout(layoutType, current, props);
            }
            else
            {
                current = WrapWithLayout(layoutType, current);
            }
        }

        return current;
    }

    /// <summary>
    /// Creates a <see cref="RenderFragment"/> that renders a layout component
    /// with the specified content as its default slot.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, object?> EmptyProps =
        new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?>());

    private static RenderFragment WrapWithLayout(Type layoutType, RenderFragment innerContent)
    {
        return RenderFragment.FromAsync(async destination =>
        {
            var layout = (IAtollComponent)Activator.CreateInstance(layoutType)!;
            var slots = SlotCollection.FromDefault(innerContent);
            await ComponentRenderer.RenderComponentAsync(layout, destination, EmptyProps, slots);
        });
    }

    /// <summary>
    /// Creates a <see cref="RenderFragment"/> that renders a layout component
    /// with the specified content as its default slot and the given props.
    /// </summary>
    private static RenderFragment WrapWithLayout(
        Type layoutType,
        RenderFragment innerContent,
        IReadOnlyDictionary<string, object?> props)
    {
        return RenderFragment.FromAsync(async destination =>
        {
            var layout = (IAtollComponent)Activator.CreateInstance(layoutType)!;
            var slots = SlotCollection.FromDefault(innerContent);
            await ComponentRenderer.RenderComponentAsync(layout, destination, props, slots);
        });
    }
}
