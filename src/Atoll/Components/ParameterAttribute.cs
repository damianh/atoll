namespace Atoll.Components;

/// <summary>
/// Marks a property on an <see cref="IAtollComponent"/> as a bindable parameter.
/// Parameters are automatically populated from the props dictionary when
/// the component is rendered via <see cref="ComponentRenderer"/>.
/// </summary>
/// <remarks>
/// <para>
/// This attribute is the Atoll equivalent of Astro's component props destructuring.
/// Properties marked with <see cref="ParameterAttribute"/> are resolved by name
/// (case-insensitive) from the props dictionary passed to the component.
/// </para>
/// <para>
/// If <see cref="Required"/> is <c>true</c> and the prop is not present in the
/// dictionary, <see cref="ComponentRenderer"/> will throw an
/// <see cref="InvalidOperationException"/>.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class ParameterAttribute : Attribute
{
    /// <summary>
    /// Gets or sets a value indicating whether this parameter is required.
    /// When <c>true</c>, the component renderer will throw if the prop is not provided.
    /// Defaults to <c>false</c>.
    /// </summary>
    public bool Required { get; set; }
}
