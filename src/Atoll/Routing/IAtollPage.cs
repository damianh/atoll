using Atoll.Components;

namespace Atoll.Routing;

/// <summary>
/// Defines the contract for an Atoll page — a component that is directly routable
/// and can be served as a standalone HTTP response.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="IAtollPage"/> extends <see cref="IAtollComponent"/> with route-level
/// metadata. Pages are discovered by <see cref="FileSystem.RouteDiscovery"/> from
/// the <c>src/pages/</c> directory and matched to incoming URLs by
/// <see cref="Matching.RouteMatcher"/>.
/// </para>
/// <para>
/// Unlike regular components, pages:
/// </para>
/// <list type="bullet">
/// <item><description>Have a URL and can be directly navigated to.</description></item>
/// <item><description>Are the root of a render tree (they are not nested inside other pages).</description></item>
/// <item><description>May specify a layout via the <c>[Layout]</c> attribute (see Story 17).</description></item>
/// <item><description>May implement <see cref="IStaticPathsProvider"/> for SSG dynamic route generation.</description></item>
/// </list>
/// <para>
/// Pages with dynamic route patterns (e.g., <c>/blog/[slug]</c>) that are used in
/// SSG mode should also implement <see cref="IStaticPathsProvider"/> to declare all
/// possible paths.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // A simple static page at /about
/// public sealed class AboutPage : AtollComponent, IAtollPage
/// {
///     protected override Task RenderCoreAsync(RenderContext context)
///     {
///         context.WriteHtml("&lt;h1&gt;About Us&lt;/h1&gt;&lt;p&gt;Welcome to our site.&lt;/p&gt;");
///         return Task.CompletedTask;
///     }
/// }
///
/// // A dynamic page at /blog/[slug] with SSG support
/// public sealed class BlogPostPage : AtollComponent, IAtollPage, IStaticPathsProvider
/// {
///     [Parameter(Required = true)]
///     public string Slug { get; set; } = "";
///
///     public async Task&lt;IReadOnlyList&lt;StaticPath&gt;&gt; GetStaticPathsAsync()
///     {
///         var slugs = await LoadAllSlugsAsync();
///         return slugs.Select(s =&gt;
///             new StaticPath(new Dictionary&lt;string, string&gt; { ["slug"] = s })
///         ).ToList();
///     }
///
///     protected override Task RenderCoreAsync(RenderContext context)
///     {
///         context.WriteHtml($"&lt;h1&gt;Blog: {Slug}&lt;/h1&gt;");
///         return Task.CompletedTask;
///     }
/// }
/// </code>
/// </example>
public interface IAtollPage : IAtollComponent
{
}
