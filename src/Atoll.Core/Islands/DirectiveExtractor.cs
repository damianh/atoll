using System.Reflection;
using Atoll.Core.Instructions;

namespace Atoll.Core.Islands;

/// <summary>
/// Extracts client directive information from component types by reading
/// <see cref="ClientDirectiveAttribute"/> subclasses applied to the class.
/// </summary>
/// <remarks>
/// <para>
/// This utility inspects a component type for <see cref="ClientLoadAttribute"/>,
/// <see cref="ClientIdleAttribute"/>, <see cref="ClientVisibleAttribute"/>,
/// or <see cref="ClientMediaAttribute"/> attributes. When a directive attribute
/// is present, the component is an "island" that requires client-side hydration.
/// </para>
/// <para>
/// The extractor returns a <see cref="DirectiveInfo"/> record containing the
/// directive type and optional value (e.g., the media query for
/// <see cref="ClientMediaAttribute"/>).
/// </para>
/// </remarks>
public static class DirectiveExtractor
{
    /// <summary>
    /// Gets the client directive applied to the specified component type, if any.
    /// </summary>
    /// <param name="componentType">The component type to inspect.</param>
    /// <returns>
    /// A <see cref="DirectiveInfo"/> describing the directive, or <c>null</c> if
    /// no client directive attribute is applied.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="componentType"/> is <c>null</c>.
    /// </exception>
    public static DirectiveInfo? GetDirective(Type componentType)
    {
        ArgumentNullException.ThrowIfNull(componentType);

        var attribute = componentType.GetCustomAttribute<ClientDirectiveAttribute>(inherit: false);

        if (attribute is null)
        {
            return null;
        }

        return new DirectiveInfo(attribute.DirectiveType, attribute.Value);
    }

    /// <summary>
    /// Determines whether the specified component type has a client directive,
    /// indicating it requires client-side hydration.
    /// </summary>
    /// <param name="componentType">The component type to inspect.</param>
    /// <returns>
    /// <c>true</c> if the component type has a <see cref="ClientDirectiveAttribute"/>;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="componentType"/> is <c>null</c>.
    /// </exception>
    public static bool HasDirective(Type componentType)
    {
        ArgumentNullException.ThrowIfNull(componentType);

        return componentType.GetCustomAttribute<ClientDirectiveAttribute>(inherit: false) is not null;
    }
}
