using Atoll.Build.Content.Collections;

namespace Atoll.Lagoon.LlmsTxt;

/// <summary>
/// Configuration interface that user projects implement to declare how to build the
/// <c>llms.txt</c> and <c>llms-full.txt</c> files for LLM-optimised content export.
/// Implement this interface in your project to enable automatic generation during <c>atoll build</c>.
/// </summary>
/// <remarks>
/// Similar to <c>ISearchIndexConfiguration</c> in Atoll.Lagoon, this interface is discovered
/// at build time by scanning the compiled assembly. Implement it in a single class and it will
/// be found automatically by the <c>atoll build</c> CLI command.
/// <para>
/// The generated <c>llms.txt</c> file follows the <see href="https://llmstxt.org/">llms.txt specification</see>:
/// an H1 title, optional blockquote summary, and H2-grouped lists of markdown links to documentation pages.
/// The optional <c>llms-full.txt</c> file inlines each page's full markdown content below its link entry.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public sealed class LlmsTxtConfig : ILlmsTxtConfiguration
/// {
///     public LlmsTxtSiteInfo GetSiteInfo() => new("My Docs", "Developer documentation for My Project.");
///
///     public IEnumerable&lt;LlmsTxtDocumentInput&gt; GetDocuments(CollectionQuery query)
///     {
///         var docs = query.GetCollection&lt;DocSchema&gt;("docs");
///         foreach (var entry in docs)
///         {
///             yield return new LlmsTxtDocumentInput(entry.Data.Title, $"/docs/{entry.Slug}/")
///             {
///                 Description = entry.Data.Description,
///                 Section = entry.Data.Section,
///                 MarkdownBody = entry.Body,
///             };
///         }
///     }
/// }
/// </code>
/// </example>
public interface ILlmsTxtConfiguration
{
    /// <summary>
    /// Returns metadata about the site used in the generated <c>llms.txt</c> header.
    /// </summary>
    /// <returns>A <see cref="LlmsTxtSiteInfo"/> with the site title and optional summary.</returns>
    LlmsTxtSiteInfo GetSiteInfo();

    /// <summary>
    /// Returns the documents to include in the <c>llms.txt</c> and <c>llms-full.txt</c> output.
    /// </summary>
    /// <param name="query">The content collection query for accessing content entries.</param>
    /// <returns>An enumerable of <see cref="LlmsTxtDocumentInput"/> descriptors.</returns>
    IEnumerable<LlmsTxtDocumentInput> GetDocuments(CollectionQuery query);
}
