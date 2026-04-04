using Atoll.Build.Content.Collections;

namespace Atoll.Lagoon.Search;

/// <summary>
/// Configuration interface that user projects implement to declare how to build the search index.
/// Implement this interface in your project to enable automatic search index generation during SSG.
/// </summary>
/// <remarks>
/// Similar to <c>IContentConfiguration</c> in Atoll, this interface is discovered at build time
/// by scanning the compiled assembly. Implement it in a single class and it will be found automatically
/// by the <c>atoll build</c> CLI command.
/// </remarks>
/// <example>
/// <code>
/// public sealed class SearchConfig : ISearchIndexConfiguration
/// {
///     public IEnumerable&lt;SearchDocumentInput&gt; GetDocuments(CollectionQuery query)
///     {
///         var docs = query.GetCollection&lt;DocSchema&gt;("docs");
///         foreach (var entry in docs)
///         {
///             var rendered = query.Render(entry);
///             yield return new SearchDocumentInput(entry.Data.Title, $"/docs/{entry.Slug}/")
///             {
///                 Description = entry.Data.Description,
///                 HtmlBody = rendered.Html,
///             };
///         }
///     }
/// }
/// </code>
/// </example>
public interface ISearchIndexConfiguration
{
    /// <summary>
    /// Returns the documents to include in the search index.
    /// </summary>
    /// <param name="query">The content collection query for accessing content entries.</param>
    /// <returns>An enumerable of <see cref="SearchDocumentInput"/> descriptors.</returns>
    IEnumerable<SearchDocumentInput> GetDocuments(CollectionQuery query);
}
