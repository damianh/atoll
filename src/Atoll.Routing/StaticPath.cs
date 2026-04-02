using System.Collections.ObjectModel;

namespace Atoll.Routing;

/// <summary>
/// Represents a single path entry returned by <see cref="IStaticPathsProvider.GetStaticPathsAsync"/>
/// for static site generation of dynamic routes.
/// </summary>
/// <remarks>
/// <para>
/// When a page has a dynamic route pattern (e.g., <c>/blog/[slug]</c>), the SSG engine
/// needs to know all possible values for the dynamic segments. Each <see cref="StaticPath"/>
/// provides the parameter values for one instance of the page, and optionally custom
/// props to pass to the page component at render time.
/// </para>
/// <para>
/// This is the Atoll equivalent of Astro's <c>getStaticPaths()</c> return entries.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // For a route pattern /blog/[slug]:
/// new StaticPath(new Dictionary&lt;string, string&gt; { ["slug"] = "hello-world" })
///
/// // With custom props:
/// new StaticPath(
///     new Dictionary&lt;string, string&gt; { ["slug"] = "hello-world" },
///     new Dictionary&lt;string, object?&gt; { ["title"] = "Hello World" })
/// </code>
/// </example>
public sealed class StaticPath
{
    /// <summary>
    /// Initializes a new <see cref="StaticPath"/> with the specified route parameters
    /// and component props.
    /// </summary>
    /// <param name="parameters">
    /// The route parameter values that fill the dynamic segments of the route pattern.
    /// For example, for <c>/blog/[slug]</c>, this would contain <c>{ "slug": "my-post" }</c>.
    /// </param>
    /// <param name="props">
    /// Optional props to pass to the page component when rendering this path.
    /// These are merged with (and override) any route-level props.
    /// </param>
    public StaticPath(
        IReadOnlyDictionary<string, string> parameters,
        IReadOnlyDictionary<string, object?> props)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentNullException.ThrowIfNull(props);
        Parameters = parameters;
        Props = props;
    }

    /// <summary>
    /// Initializes a new <see cref="StaticPath"/> with the specified route parameters
    /// and no additional props.
    /// </summary>
    /// <param name="parameters">
    /// The route parameter values that fill the dynamic segments of the route pattern.
    /// </param>
    public StaticPath(IReadOnlyDictionary<string, string> parameters)
        : this(parameters, EmptyProps)
    {
    }

    /// <summary>
    /// Gets the route parameter values for this path.
    /// Keys correspond to dynamic segment names in the route pattern.
    /// </summary>
    /// <example>
    /// For a route pattern <c>/blog/[slug]</c>, this might contain
    /// <c>{ "slug": "hello-world" }</c>.
    /// </example>
    public IReadOnlyDictionary<string, string> Parameters { get; }

    /// <summary>
    /// Gets the props to pass to the page component when rendering this path.
    /// </summary>
    /// <remarks>
    /// Props allow pre-computed data to be passed directly to the page component
    /// during SSG, avoiding redundant data fetching. For example, when generating
    /// blog post pages, the post data can be loaded once in <c>GetStaticPathsAsync</c>
    /// and passed as props rather than re-loaded during rendering.
    /// </remarks>
    public IReadOnlyDictionary<string, object?> Props { get; }

    private static readonly IReadOnlyDictionary<string, object?> EmptyProps =
        new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?>());
}
