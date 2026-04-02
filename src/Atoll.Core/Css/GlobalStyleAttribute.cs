namespace Atoll.Core.Css;

/// <summary>
/// Marks a component's CSS as global (unscoped). When applied to a component
/// that also has <see cref="StylesAttribute"/>, the CSS is emitted without
/// the <c>:where(.atoll-HASH)</c> scope wrapper.
/// </summary>
/// <remarks>
/// <para>
/// Use this attribute sparingly. Global styles affect the entire page and
/// can cause unintended side effects. Prefer scoped styles via
/// <see cref="StylesAttribute"/> alone.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// [GlobalStyle]
/// [Styles("body { margin: 0; } * { box-sizing: border-box; }")]
/// public sealed class GlobalReset : AtollComponent { ... }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class GlobalStyleAttribute : Attribute
{
}
