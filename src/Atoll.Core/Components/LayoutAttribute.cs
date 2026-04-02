namespace Atoll.Core.Components;

/// <summary>
/// Specifies the layout component that wraps a page during rendering.
/// The layout component receives the page's rendered output as its default slot.
/// </summary>
/// <remarks>
/// <para>
/// This attribute is the Atoll equivalent of Astro's <c>layout</c> frontmatter property.
/// When applied to a page component (a class implementing <see cref="IAtollComponent"/>),
/// the <see cref="LayoutResolver"/> detects it and wraps the page content inside
/// the specified layout component.
/// </para>
/// <para>
/// Layouts are regular Atoll components — they render their structure and use
/// <c>RenderSlotAsync()</c> to insert the page content at the desired location.
/// Nested layouts are supported: a layout can itself have a <see cref="LayoutAttribute"/>
/// pointing to a parent layout.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// [Layout(typeof(BaseLayout))]
/// public sealed class AboutPage : AtollComponent, IAtollPage
/// {
///     protected override Task RenderCoreAsync(RenderContext context)
///     {
///         context.WriteHtml("&lt;h1&gt;About Us&lt;/h1&gt;");
///         return Task.CompletedTask;
///     }
/// }
///
/// public sealed class BaseLayout : AtollComponent
/// {
///     protected override async Task RenderCoreAsync(RenderContext context)
///     {
///         context.WriteHtml("&lt;html&gt;&lt;head&gt;&lt;title&gt;My Site&lt;/title&gt;&lt;/head&gt;&lt;body&gt;");
///         await RenderSlotAsync(); // Page content injected here
///         context.WriteHtml("&lt;/body&gt;&lt;/html&gt;");
///     }
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class LayoutAttribute : Attribute
{
    /// <summary>
    /// Initializes a new <see cref="LayoutAttribute"/> with the specified layout component type.
    /// </summary>
    /// <param name="layoutType">
    /// The type of the layout component. Must implement <see cref="IAtollComponent"/>
    /// and have a parameterless constructor.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="layoutType"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="layoutType"/> does not implement <see cref="IAtollComponent"/>.
    /// </exception>
    public LayoutAttribute(Type layoutType)
    {
        ArgumentNullException.ThrowIfNull(layoutType);

        if (!typeof(IAtollComponent).IsAssignableFrom(layoutType))
        {
            throw new ArgumentException(
                $"Layout type '{layoutType.FullName}' must implement {nameof(IAtollComponent)}.",
                nameof(layoutType));
        }

        LayoutType = layoutType;
    }

    /// <summary>
    /// Gets the type of the layout component.
    /// </summary>
    public Type LayoutType { get; }
}
