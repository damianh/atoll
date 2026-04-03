using Atoll.Instructions;

namespace Atoll.Islands;

/// <summary>
/// Contains metadata about an island component required for rendering
/// the <c>&lt;atoll-island&gt;</c> custom element wrapper.
/// </summary>
/// <remarks>
/// <para>
/// This is the Atoll equivalent of Astro's island element attributes.
/// The metadata describes the component's client-side module URL, export name,
/// hydration directive, and serialized props — everything the client-side
/// <c>atoll-island</c> Web Component needs to hydrate the island.
/// </para>
/// </remarks>
public sealed class IslandMetadata
{
    /// <summary>
    /// Initializes a new <see cref="IslandMetadata"/> with the specified component URL
    /// and client directive.
    /// </summary>
    /// <param name="componentUrl">
    /// The URL of the client-side JavaScript module for this island component.
    /// </param>
    /// <param name="directiveType">The client directive type determining when hydration occurs.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="componentUrl"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="componentUrl"/> is empty or whitespace.
    /// </exception>
    public IslandMetadata(string componentUrl, ClientDirectiveType directiveType)
    {
        ArgumentNullException.ThrowIfNull(componentUrl);

        if (string.IsNullOrWhiteSpace(componentUrl))
        {
            throw new ArgumentException(
                "Component URL must not be empty or whitespace.",
                nameof(componentUrl));
        }

        ComponentUrl = componentUrl;
        DirectiveType = directiveType;
    }

    /// <summary>
    /// Gets the URL of the client-side JavaScript module for this island component.
    /// </summary>
    public string ComponentUrl { get; }

    /// <summary>
    /// Gets the client directive type determining when hydration occurs.
    /// </summary>
    public ClientDirectiveType DirectiveType { get; }

    /// <summary>
    /// Gets or sets the export name from the client-side module.
    /// Defaults to <c>"default"</c>.
    /// </summary>
    public string ComponentExport { get; set; } = "default";

    /// <summary>
    /// Gets or sets the directive value, if any. For <see cref="ClientDirectiveType.Media"/>,
    /// this is the CSS media query. For <see cref="ClientDirectiveType.Visible"/>,
    /// this may be the root margin.
    /// </summary>
    public string? DirectiveValue { get; set; }

    /// <summary>
    /// Gets or sets the display name of the component, used in the <c>opts</c> attribute
    /// for debugging purposes.
    /// </summary>
    public string DisplayName { get; set; } = "unknown";

    /// <summary>
    /// Gets or sets the URL of a script to execute before hydration begins.
    /// </summary>
    public string? BeforeHydrationUrl { get; set; }
}
