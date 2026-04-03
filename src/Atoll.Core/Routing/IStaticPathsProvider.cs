namespace Atoll.Routing;

/// <summary>
/// Provides static path generation for pages with dynamic route patterns during
/// static site generation (SSG).
/// </summary>
/// <remarks>
/// <para>
/// This is the Atoll equivalent of Astro's <c>getStaticPaths()</c> function.
/// When a page has a dynamic route pattern (e.g., <c>/blog/[slug]</c>) and is
/// being statically generated, the SSG engine calls
/// <see cref="GetStaticPathsAsync"/> to enumerate all possible parameter values.
/// </para>
/// <para>
/// Pages implementing <see cref="IStaticPathsProvider"/> must return one
/// <see cref="StaticPath"/> for each URL that should be generated. Each
/// <see cref="StaticPath"/> specifies the route parameter values and optionally
/// pre-computed props.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public sealed class BlogPostPage : AtollComponent, IAtollPage, IStaticPathsProvider
/// {
///     [Parameter(Required = true)]
///     public string Title { get; set; } = "";
///
///     public async Task&lt;IReadOnlyList&lt;StaticPath&gt;&gt; GetStaticPathsAsync()
///     {
///         var posts = await LoadBlogPostsAsync();
///         return posts.Select(p =&gt; new StaticPath(
///             new Dictionary&lt;string, string&gt; { ["slug"] = p.Slug },
///             new Dictionary&lt;string, object?&gt; { ["Title"] = p.Title }
///         )).ToList();
///     }
///
///     protected override Task RenderCoreAsync(RenderContext context)
///     {
///         context.WriteHtml($"&lt;h1&gt;{Context.GetProp&lt;string&gt;("Title")}&lt;/h1&gt;");
///         return Task.CompletedTask;
///     }
/// }
/// </code>
/// </example>
public interface IStaticPathsProvider
{
    /// <summary>
    /// Returns all static paths that should be generated for this page's dynamic route.
    /// </summary>
    /// <returns>
    /// A task that resolves to a list of <see cref="StaticPath"/> values, one for each
    /// URL that should be statically generated. An empty list produces no pages for this route.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method is called once per dynamic route during static site generation.
    /// The returned paths are used to:
    /// </para>
    /// <list type="number">
    /// <item><description>Determine the set of URLs to render.</description></item>
    /// <item><description>Provide route parameters for each URL.</description></item>
    /// <item><description>Optionally pre-load data as props for each page render.</description></item>
    /// </list>
    /// </remarks>
    Task<IReadOnlyList<StaticPath>> GetStaticPathsAsync();
}
