using Atoll.Build.Content.Collections;

namespace Atoll.Lagoon.Redirects;

/// <summary>
/// Configuration interface that user projects implement to declare URL redirect rules.
/// Implement this interface in your project to enable automatic <c>_redirects</c> file
/// generation during <c>atoll build</c>.
/// </summary>
/// <remarks>
/// Similar to <c>ISearchIndexConfiguration</c> in Atoll.Lagoon, this interface is discovered
/// at build time by scanning the compiled assembly. Implement it in a single class and it will
/// be found automatically by the <c>atoll build</c> CLI command.
/// <para>
/// A common pattern is to store the old URL path in a frontmatter field (e.g. <c>redirectFrom</c>)
/// on your content schema, then enumerate the collection and yield a <see cref="RedirectRule"/>
/// for each entry that has a redirect defined.
/// </para>
/// </remarks>
/// <example>
/// Schema with <c>redirectFrom</c> frontmatter field:
/// <code>
/// public sealed class DocSchema
/// {
///     [Required] public string Title { get; set; } = "";
///     public string? RedirectFrom { get; set; }
/// }
/// </code>
/// Redirect configuration implementation:
/// <code>
/// public sealed class RedirectConfig : IRedirectConfiguration
/// {
///     public IEnumerable&lt;RedirectRule&gt; GetRedirects(CollectionQuery query)
///     {
///         var docs = query.GetCollection&lt;DocSchema&gt;("docs");
///         foreach (var entry in docs)
///         {
///             if (!string.IsNullOrWhiteSpace(entry.Data.RedirectFrom))
///             {
///                 yield return new RedirectRule(entry.Data.RedirectFrom, $"/docs/{entry.Slug}/");
///             }
///         }
///     }
/// }
/// </code>
/// Frontmatter example:
/// <code>
/// ---
/// title: New Page Title
/// redirectFrom: /old/url
/// ---
/// </code>
/// </example>
public interface IRedirectConfiguration
{
    /// <summary>
    /// Returns the redirect rules to include in the generated <c>_redirects</c> file.
    /// </summary>
    /// <param name="query">The content collection query for accessing content entries.</param>
    /// <returns>An enumerable of <see cref="RedirectRule"/> descriptors.</returns>
    IEnumerable<RedirectRule> GetRedirects(CollectionQuery query);
}
