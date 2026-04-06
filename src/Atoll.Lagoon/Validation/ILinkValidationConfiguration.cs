using Atoll.Build.Content.Collections;

namespace Atoll.Lagoon.Validation;

/// <summary>
/// Configuration interface that user projects implement to declare which pages should be
/// validated for broken internal links.
/// Implement this interface to enable automatic link validation during the <c>atoll build</c> step.
/// </summary>
/// <remarks>
/// Similar to <c>ISearchIndexConfiguration</c>, this interface is discovered at build time
/// by scanning the compiled assembly. Implement it in a single class and it will be found
/// automatically by the <c>atoll build</c> CLI command.
/// </remarks>
/// <example>
/// <code>
/// public sealed class LinkValidationConfig : ILinkValidationConfiguration
/// {
///     public IEnumerable&lt;LinkValidationInput&gt; GetPages(CollectionQuery query)
///     {
///         var docs = query.GetCollection&lt;DocSchema&gt;("docs");
///         foreach (var entry in docs)
///         {
///             var rendered = query.Render(entry);
///             var anchorIds = rendered.Headings.Select(h =&gt; h.Id).ToList();
///             yield return new LinkValidationInput(
///                 $"/docs/{entry.Slug}/",
///                 anchorIds,
///                 rendered.Html);
///         }
///     }
/// }
/// </code>
/// </example>
public interface ILinkValidationConfiguration
{
    /// <summary>
    /// Returns the pages to include in link validation.
    /// </summary>
    /// <param name="query">The content collection query for accessing content entries.</param>
    /// <returns>An enumerable of <see cref="LinkValidationInput"/> descriptors.</returns>
    IEnumerable<LinkValidationInput> GetPages(CollectionQuery query);

    /// <summary>
    /// Gets the options that control validation behaviour.
    /// Override to customise severity and exclusions.
    /// </summary>
    LinkValidationOptions Options => new LinkValidationOptions();
}
