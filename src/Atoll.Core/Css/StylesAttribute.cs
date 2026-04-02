namespace Atoll.Core.Css;

/// <summary>
/// Declares inline CSS styles for an Atoll component. The CSS is automatically
/// scoped to the component using the <c>:where(.atoll-HASH)</c> strategy unless
/// the component is also marked with <see cref="GlobalStyleAttribute"/>.
/// </summary>
/// <remarks>
/// <para>
/// The CSS text is extracted at render time and processed through
/// <see cref="StyleScoper"/> to apply hash-based scoping. Components
/// using this attribute should write standard CSS without manually adding
/// scope selectors.
/// </para>
/// <para>
/// Multiple <see cref="StylesAttribute"/> instances can be applied to a single
/// component; all CSS blocks will be collected and scoped together.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// [Styles(".card { padding: 1rem; } .card h2 { color: blue; }")]
/// public sealed class Card : AtollComponent { ... }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class StylesAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StylesAttribute"/> class
    /// with the specified CSS text.
    /// </summary>
    /// <param name="css">The CSS text to scope and inject.</param>
    public StylesAttribute(string css)
    {
        ArgumentNullException.ThrowIfNull(css);
        Css = css;
    }

    /// <summary>
    /// Gets the CSS text declared by this attribute.
    /// </summary>
    public string Css { get; }
}
