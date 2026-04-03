namespace Atoll.Routing;

/// <summary>
/// Declares the URL route pattern for a page component.
/// When present, this attribute takes precedence over file-based or
/// convention-based route discovery.
/// </summary>
/// <remarks>
/// <para>
/// Use this attribute to explicitly control the route pattern when the page is
/// not located in a file-based routing directory, or when the type name conventions
/// do not produce the desired route.
/// </para>
/// <para>
/// The pattern follows Atoll's file-based routing conventions:
/// <list type="bullet">
/// <item><description><c>/about</c> — static route.</description></item>
/// <item><description><c>/blog/[slug]</c> — dynamic segment.</description></item>
/// <item><description><c>/docs/[...rest]</c> — catch-all segment.</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// [PageRoute("/blog/[slug]")]
/// public sealed class BlogPostPage : AtollComponent, IAtollPage, IStaticPathsProvider
/// {
///     // ...
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class PageRouteAttribute : Attribute
{
    /// <summary>
    /// Initializes a new <see cref="PageRouteAttribute"/> with the specified route pattern.
    /// </summary>
    /// <param name="pattern">The URL route pattern (e.g., <c>/blog/[slug]</c>).</param>
    public PageRouteAttribute(string pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        Pattern = pattern;
    }

    /// <summary>
    /// Gets the URL route pattern for this page.
    /// </summary>
    public string Pattern { get; }
}
