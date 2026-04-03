namespace Atoll.Islands;

/// <summary>
/// Defines the contract for a component that has a client-side JavaScript module
/// for hydration. The component provides both server-side rendered HTML and the
/// URL of the JavaScript module that makes it interactive on the client.
/// </summary>
/// <remarks>
/// <para>
/// This is the Atoll equivalent of Astro's client-side component contract.
/// Components implementing <see cref="IClientComponent"/> must:
/// </para>
/// <list type="bullet">
/// <item>Render their initial HTML during SSR (via <c>RenderAsync</c>)</item>
/// <item>Provide a <see cref="ClientModuleUrl"/> pointing to the JavaScript module</item>
/// <item>Optionally specify a <see cref="ClientExportName"/> for non-default exports</item>
/// </list>
/// <para>
/// The client-side JavaScript module is loaded by the <c>atoll-island</c> Web Component
/// during hydration. For vanilla JS islands, the module should export a function that
/// accepts <c>(element, props, slots, metadata)</c>.
/// </para>
/// </remarks>
public interface IClientComponent
{
    /// <summary>
    /// Gets the URL of the client-side JavaScript module for this component.
    /// </summary>
    /// <remarks>
    /// This URL is used as the <c>component-url</c> attribute on the
    /// <c>&lt;atoll-island&gt;</c> element. It can be a relative or absolute URL.
    /// </remarks>
    string ClientModuleUrl { get; }

    /// <summary>
    /// Gets the export name from the client-side module. Defaults to <c>"default"</c>.
    /// </summary>
    /// <remarks>
    /// If the JavaScript module uses a named export instead of a default export,
    /// override this property to return the export name. Supports dot-separated
    /// paths for nested exports (e.g., <c>"Namespace.Component"</c>).
    /// </remarks>
    string ClientExportName => "default";
}
