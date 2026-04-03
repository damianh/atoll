using System.Net;
using System.Text.Json;
using Atoll.Components;
using Atoll.Instructions;
using Atoll.Rendering;
using Atoll.Slots;

namespace Atoll.Islands;

/// <summary>
/// Renders island components by wrapping their server-side rendered HTML in an
/// <c>&lt;atoll-island&gt;</c> custom element with hydration metadata attributes.
/// </summary>
/// <remarks>
/// <para>
/// This is the Atoll equivalent of Astro's island rendering in
/// <c>runtime/server/render/server-islands.ts</c> and <c>hydration.ts</c>.
/// During SSR, the component's HTML output is wrapped in:
/// </para>
/// <code>
/// &lt;atoll-island component-url="..." client="load" props="..." ssr&gt;
///     ...SSR HTML...
/// &lt;/atoll-island&gt;
/// </code>
/// <para>
/// The client-side <c>atoll-island</c> Web Component reads these attributes
/// to load the component's JavaScript module and hydrate the island when
/// the directive conditions are met.
/// </para>
/// </remarks>
public static class IslandRenderer
{
    /// <summary>
    /// Renders an island component to the specified destination, wrapping the SSR output
    /// in an <c>&lt;atoll-island&gt;</c> custom element.
    /// </summary>
    /// <typeparam name="TComponent">The component type. Must implement <see cref="IAtollComponent"/>
    /// and have a parameterless constructor.</typeparam>
    /// <param name="destination">The render destination.</param>
    /// <param name="metadata">The island metadata (component URL, directive, etc.).</param>
    /// <param name="props">The props dictionary to pass to the component and serialize for hydration.</param>
    /// <param name="slots">The slot collection.</param>
    /// <returns>A <see cref="Task"/> representing the async render operation.</returns>
    public static async Task RenderIslandAsync<TComponent>(
        IRenderDestination destination,
        IslandMetadata metadata,
        IReadOnlyDictionary<string, object?> props,
        SlotCollection slots)
        where TComponent : IAtollComponent, new()
    {
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(props);
        ArgumentNullException.ThrowIfNull(slots);

        await RenderIslandCoreAsync(
            destination, metadata, props, slots,
            async (dest, p, s) =>
            {
                await ComponentRenderer.RenderComponentAsync<TComponent>(dest, p, s);
            });
    }

    /// <summary>
    /// Renders an island component to the specified destination, wrapping the SSR output
    /// in an <c>&lt;atoll-island&gt;</c> custom element.
    /// </summary>
    /// <param name="destination">The render destination.</param>
    /// <param name="metadata">The island metadata (component URL, directive, etc.).</param>
    /// <param name="componentType">The component type.</param>
    /// <param name="props">The props dictionary to pass to the component and serialize for hydration.</param>
    /// <param name="slots">The slot collection.</param>
    /// <returns>A <see cref="Task"/> representing the async render operation.</returns>
    public static async Task RenderIslandAsync(
        IRenderDestination destination,
        IslandMetadata metadata,
        Type componentType,
        IReadOnlyDictionary<string, object?> props,
        SlotCollection slots)
    {
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(componentType);
        ArgumentNullException.ThrowIfNull(props);
        ArgumentNullException.ThrowIfNull(slots);

        if (!typeof(IAtollComponent).IsAssignableFrom(componentType))
        {
            throw new ArgumentException(
                $"Type '{componentType.FullName}' must implement {nameof(IAtollComponent)}.",
                nameof(componentType));
        }

        await RenderIslandCoreAsync(
            destination, metadata, props, slots,
            async (dest, p, s) =>
            {
                var component = (IAtollComponent)Activator.CreateInstance(componentType)!;
                await ComponentRenderer.RenderComponentAsync(component, dest, p, s);
            });
    }

    /// <summary>
    /// Generates the opening <c>&lt;atoll-island&gt;</c> tag HTML string
    /// for the given metadata and props.
    /// </summary>
    /// <param name="metadata">The island metadata.</param>
    /// <param name="serializedProps">The pre-serialized props JSON string.</param>
    /// <returns>The opening tag HTML string.</returns>
    public static string GenerateOpeningTag(IslandMetadata metadata, string serializedProps)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(serializedProps);

        var directiveName = metadata.DirectiveType switch
        {
            ClientDirectiveType.Load => "load",
            ClientDirectiveType.Idle => "idle",
            ClientDirectiveType.Visible => "visible",
            ClientDirectiveType.Media => "media",
            _ => "load",
        };

        var optsJson = JsonSerializer.Serialize(new
        {
            name = metadata.DisplayName,
            value = metadata.DirectiveValue ?? "",
        });

        var html = $"<atoll-island" +
            $" component-url=\"{EscapeAttribute(metadata.ComponentUrl)}\"" +
            $" component-export=\"{EscapeAttribute(metadata.ComponentExport)}\"" +
            $" client=\"{directiveName}\"" +
            $" props=\"{EscapeAttribute(serializedProps)}\"" +
            $" opts=\"{EscapeAttribute(optsJson)}\"" +
            " ssr";

        if (metadata.BeforeHydrationUrl is not null)
        {
            html += $" before-hydration-url=\"{EscapeAttribute(metadata.BeforeHydrationUrl)}\"";
        }

        html += ">";

        return html;
    }

    /// <summary>
    /// The closing tag for the island wrapper element.
    /// </summary>
    public const string ClosingTag = "</atoll-island>";

    private static async Task RenderIslandCoreAsync(
        IRenderDestination destination,
        IslandMetadata metadata,
        IReadOnlyDictionary<string, object?> props,
        SlotCollection slots,
        Func<IRenderDestination, IReadOnlyDictionary<string, object?>, SlotCollection, Task> renderComponent)
    {
        // Serialize props for the client
        var serializedProps = PropSerializer.Serialize(props, metadata.DisplayName);

        // Write opening tag
        var openingTag = GenerateOpeningTag(metadata, serializedProps);
        destination.Write(RenderChunk.Html(openingTag));

        // Render the component's SSR output into a buffer so it goes inside the island
        await renderComponent(destination, props, slots);

        // Write closing tag
        destination.Write(RenderChunk.Html(ClosingTag));
    }

    private static string EscapeAttribute(string value)
    {
        return WebUtility.HtmlEncode(value);
    }
}
