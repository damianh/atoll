using Atoll.Components;
using Atoll.Rendering;
using Atoll.Slots;

namespace Atoll.Islands;

/// <summary>
/// Base class for vanilla JavaScript island components. Combines server-side rendering
/// with a client-side JavaScript module URL for hydration.
/// </summary>
/// <remarks>
/// <para>
/// This is the Atoll equivalent of using vanilla JS with Astro's island architecture.
/// A vanilla JS island renders static HTML on the server, then loads a JavaScript
/// module on the client to make the content interactive.
/// </para>
/// <para>
/// The client-side JavaScript module should export a function matching one of:
/// </para>
/// <list type="bullet">
/// <item><c>export default function(element, props, slots, metadata) { ... }</c></item>
/// <item><c>export function init(element, props, slots, metadata) { ... }</c></item>
/// </list>
/// <para>
/// Example usage:
/// </para>
/// <code>
/// [ClientLoad]
/// public sealed class Counter : VanillaJsIsland
/// {
///     public override string ClientModuleUrl => "/scripts/counter.js";
///
///     [Parameter]
///     public int InitialCount { get; set; }
///
///     protected override Task RenderCoreAsync(RenderContext context)
///     {
///         WriteHtml($"&lt;div class=\"counter\"&gt;{InitialCount}&lt;/div&gt;");
///         return Task.CompletedTask;
///     }
/// }
/// </code>
/// </remarks>
public abstract class VanillaJsIsland : AtollComponent, IClientComponent
{
    /// <inheritdoc />
    public abstract string ClientModuleUrl { get; }

    /// <inheritdoc />
    public virtual string ClientExportName => "default";

    /// <summary>
    /// Creates <see cref="IslandMetadata"/> for this component by combining its
    /// <see cref="ClientModuleUrl"/>, <see cref="ClientExportName"/>, and the
    /// directive information from the class attribute.
    /// </summary>
    /// <returns>
    /// The island metadata, or <c>null</c> if the component does not have a
    /// <see cref="ClientDirectiveAttribute"/>.
    /// </returns>
    public IslandMetadata? CreateMetadata()
    {
        var directiveInfo = DirectiveExtractor.GetDirective(GetType());
        if (directiveInfo is null)
        {
            return null;
        }

        return new IslandMetadata(ClientModuleUrl, directiveInfo.DirectiveType)
        {
            ComponentExport = ClientExportName,
            DirectiveValue = directiveInfo.Value,
            DisplayName = GetType().Name,
        };
    }

    /// <summary>
    /// Renders this island component as an <c>&lt;atoll-island&gt;</c> wrapper
    /// with SSR content and hydration metadata to the specified destination.
    /// </summary>
    /// <param name="destination">The render destination.</param>
    /// <param name="props">The props dictionary.</param>
    /// <param name="slots">The slot collection.</param>
    /// <returns>A <see cref="Task"/> representing the async render operation.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when this component type does not have a <see cref="ClientDirectiveAttribute"/>.
    /// </exception>
    public async Task RenderIslandAsync(
        IRenderDestination destination,
        IReadOnlyDictionary<string, object?> props,
        SlotCollection slots)
    {
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentNullException.ThrowIfNull(props);
        ArgumentNullException.ThrowIfNull(slots);

        var metadata = CreateMetadata();
        if (metadata is null)
        {
            throw new InvalidOperationException(
                $"Type '{GetType().FullName}' does not have a client directive attribute. " +
                "Apply [ClientLoad], [ClientIdle], [ClientVisible], or [ClientMedia] to use island rendering.");
        }

        await IslandRenderer.RenderIslandAsync(
            destination,
            metadata,
            GetType(),
            props,
            slots);
    }

    /// <summary>
    /// Renders this island component as an <c>&lt;atoll-island&gt;</c> wrapper
    /// with SSR content and hydration metadata to the specified destination.
    /// </summary>
    /// <param name="destination">The render destination.</param>
    /// <param name="props">The props dictionary.</param>
    /// <returns>A <see cref="Task"/> representing the async render operation.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when this component type does not have a <see cref="ClientDirectiveAttribute"/>.
    /// </exception>
    public Task RenderIslandAsync(
        IRenderDestination destination,
        IReadOnlyDictionary<string, object?> props)
    {
        return RenderIslandAsync(destination, props, SlotCollection.Empty);
    }

    /// <summary>
    /// Renders this island component as an <c>&lt;atoll-island&gt;</c> wrapper
    /// with SSR content and hydration metadata to the specified destination.
    /// </summary>
    /// <param name="destination">The render destination.</param>
    /// <returns>A <see cref="Task"/> representing the async render operation.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when this component type does not have a <see cref="ClientDirectiveAttribute"/>.
    /// </exception>
    public Task RenderIslandAsync(IRenderDestination destination)
    {
        return RenderIslandAsync(
            destination,
            new Dictionary<string, object?>(),
            SlotCollection.Empty);
    }
}
