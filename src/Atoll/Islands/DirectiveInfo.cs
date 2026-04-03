using Atoll.Instructions;

namespace Atoll.Islands;

/// <summary>
/// Describes a client directive extracted from a component type, including the
/// directive type and optional value.
/// </summary>
/// <param name="DirectiveType">The client directive type (Load, Idle, Visible, or Media).</param>
/// <param name="Value">
/// The directive value, if any. For <see cref="ClientDirectiveType.Media"/>,
/// this is the CSS media query string. For <see cref="ClientDirectiveType.Visible"/>,
/// this may be the root margin. <c>null</c> for directives without values.
/// </param>
public sealed record DirectiveInfo(ClientDirectiveType DirectiveType, string? Value);
