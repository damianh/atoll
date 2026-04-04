using Atoll.Components;
using Atoll.Rendering;
using Atoll.Slots;

namespace Atoll.Islands;

/// <summary>
/// Abstract base class for Atoll island components authored as Razor templates (no model).
/// Extends <see cref="AtollSlice"/> with <see cref="IClientComponent"/> support for client-side hydration.
/// </summary>
/// <remarks>
/// <para>
/// Razor island files should declare <c>@inherits AtollIslandSlice</c> and override
/// <see cref="ClientModuleUrl"/> to provide the JavaScript module URL.
/// Apply a client directive attribute (e.g., <c>[ClientLoad]</c>) to the class.
/// </para>
/// <para>
/// The island can be rendered as a standalone <c>&lt;atoll-island&gt;</c> wrapper by calling
/// <see cref="RenderIslandAsync(IRenderDestination, IReadOnlyDictionary{string, object?}, SlotCollection)"/>.
/// </para>
/// </remarks>
public abstract class AtollIslandSlice : AtollSlice, IClientComponent
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
    public Task RenderIslandAsync(IRenderDestination destination)
    {
        return RenderIslandAsync(
            destination,
            new Dictionary<string, object?>(),
            SlotCollection.Empty);
    }
}
