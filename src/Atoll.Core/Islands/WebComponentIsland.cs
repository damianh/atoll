using Atoll.Core.Components;
using Atoll.Core.Rendering;
using Atoll.Core.Slots;

namespace Atoll.Core.Islands;

/// <summary>
/// Base class for Web Component island components. Renders a custom element
/// during SSR and loads the element definition on the client for hydration.
/// </summary>
/// <remarks>
/// <para>
/// This is the Atoll equivalent of using custom elements with Astro's island
/// architecture. A Web Component island renders a custom element tag with its
/// SSR content as light DOM on the server. On the client, the element definition
/// JavaScript module is loaded, and the browser upgrades the element.
/// </para>
/// <para>
/// The custom element tag is rendered inside the <c>&lt;atoll-island&gt;</c>
/// wrapper. The client-side module should define the custom element via
/// <c>customElements.define()</c>.
/// </para>
/// <para>
/// Example usage:
/// </para>
/// <code>
/// [ClientLoad]
/// public sealed class MyWidget : WebComponentIsland
/// {
///     public override string TagName => "my-widget";
///     public override string ClientModuleUrl => "/components/my-widget.js";
///
///     [Parameter]
///     public string Title { get; set; } = "";
///
///     protected override Task RenderLightDomAsync(RenderContext context)
///     {
///         WriteHtml($"&lt;span&gt;{Title}&lt;/span&gt;");
///         return Task.CompletedTask;
///     }
/// }
/// </code>
/// </remarks>
public abstract class WebComponentIsland : AtollComponent, IClientComponent
{
    /// <summary>
    /// Gets the custom element tag name (e.g., <c>"my-counter"</c>).
    /// Must be a valid custom element name (lowercase, contains a hyphen).
    /// </summary>
    public abstract string TagName { get; }

    /// <inheritdoc />
    public abstract string ClientModuleUrl { get; }

    /// <inheritdoc />
    public virtual string ClientExportName => "default";

    /// <summary>
    /// When implemented in a derived class, renders the light DOM content
    /// that goes inside the custom element tag.
    /// </summary>
    /// <param name="context">The rendering context.</param>
    /// <returns>A <see cref="Task"/> representing the async render operation.</returns>
    protected abstract Task RenderLightDomAsync(RenderContext context);

    /// <summary>
    /// Gets a value indicating whether to render component props as HTML
    /// attributes on the custom element tag. Defaults to <c>true</c>.
    /// </summary>
    /// <remarks>
    /// When enabled, string, numeric, and boolean props are rendered as
    /// attributes on the custom element. Complex object props are always
    /// passed via the island's <c>props</c> JSON attribute instead.
    /// </remarks>
    protected virtual bool RenderPropsAsAttributes => true;

    /// <inheritdoc />
    protected sealed override Task RenderCoreAsync(RenderContext context)
    {
        // Render the custom element tag wrapping the light DOM content
        var openTag = RenderPropsAsAttributes
            ? WebComponentAdapter.GenerateOpeningTag(TagName, context.Props)
            : WebComponentAdapter.GenerateOpeningTag(TagName);

        WriteHtml(openTag);

        // Render light DOM content
        var renderTask = RenderLightDomAsync(context);

        if (renderTask.IsCompletedSuccessfully)
        {
            WriteHtml(WebComponentAdapter.GenerateClosingTag(TagName));
            return Task.CompletedTask;
        }

        return RenderCoreAsyncContinuation(context, renderTask);
    }

    private async Task RenderCoreAsyncContinuation(RenderContext context, Task renderTask)
    {
        await renderTask;
        WriteHtml(WebComponentAdapter.GenerateClosingTag(TagName));
    }

    /// <summary>
    /// Creates <see cref="IslandMetadata"/> for this Web Component island
    /// by combining its <see cref="ClientModuleUrl"/>, <see cref="ClientExportName"/>,
    /// and the directive information from the class attribute.
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
            DisplayName = TagName,
        };
    }

    /// <summary>
    /// Renders this Web Component island as an <c>&lt;atoll-island&gt;</c> wrapper
    /// with the custom element and hydration metadata to the specified destination.
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
    /// Renders this Web Component island as an <c>&lt;atoll-island&gt;</c> wrapper
    /// with the custom element and hydration metadata to the specified destination.
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
    /// Renders this Web Component island as an <c>&lt;atoll-island&gt;</c> wrapper
    /// with the custom element and hydration metadata to the specified destination.
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
