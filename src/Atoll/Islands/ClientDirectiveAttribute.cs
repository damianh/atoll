using Atoll.Instructions;

namespace Atoll.Islands;

/// <summary>
/// Base class for client directive attributes that mark a component for client-side hydration.
/// The directive determines when hydration occurs on the client.
/// </summary>
/// <remarks>
/// <para>
/// This is the Atoll equivalent of Astro's <c>client:*</c> directives. Subclasses
/// correspond to specific hydration strategies:
/// </para>
/// <list type="bullet">
/// <item><see cref="ClientLoadAttribute"/> — Hydrate immediately on page load</item>
/// <item><see cref="ClientIdleAttribute"/> — Hydrate when the browser is idle</item>
/// <item><see cref="ClientVisibleAttribute"/> — Hydrate when the component enters the viewport</item>
/// <item><see cref="ClientMediaAttribute"/> — Hydrate when a CSS media query matches</item>
/// </list>
/// <para>
/// When a component type has a <see cref="ClientDirectiveAttribute"/>, the
/// <see cref="DirectiveExtractor"/> detects it and the island renderer wraps
/// the component in an <c>&lt;atoll-island&gt;</c> element with the appropriate
/// hydration metadata.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public abstract class ClientDirectiveAttribute : Attribute
{
    /// <summary>
    /// Initializes a new <see cref="ClientDirectiveAttribute"/> with the specified directive type.
    /// </summary>
    /// <param name="directiveType">The client directive type.</param>
    protected ClientDirectiveAttribute(ClientDirectiveType directiveType)
    {
        DirectiveType = directiveType;
    }

    /// <summary>
    /// Gets the client directive type that determines when hydration occurs.
    /// </summary>
    public ClientDirectiveType DirectiveType { get; }

    /// <summary>
    /// Gets the directive value, if any. For <see cref="ClientDirectiveType.Media"/>,
    /// this is the CSS media query string. Returns <c>null</c> for directives that
    /// do not require a value.
    /// </summary>
    public virtual string? Value => null;
}
