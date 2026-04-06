using Atoll.Build.Content.Collections;
using Atoll.Lagoon.Configuration;

namespace Atoll.Lagoon.OpenGraph;

/// <summary>
/// Configuration interface that user projects implement to declare which pages should have
/// OpenGraph images generated at build time.
/// Implement this interface in a single class and it will be discovered automatically
/// by the <c>atoll build</c> CLI command via assembly scanning.
/// </summary>
/// <remarks>
/// Similar to <c>ISearchIndexConfiguration</c> in Atoll.Lagoon, this interface is discovered
/// at build time by scanning the compiled assembly.
/// </remarks>
/// <example>
/// <code>
/// public sealed class OgConfig : IOgImageConfiguration
/// {
///     public OpenGraphConfig GetOpenGraphConfig() =&gt; new OpenGraphConfig
///     {
///         BackgroundImagePath = "assets/og-background.png",
///         Categories = new Dictionary&lt;string, string&gt;
///         {
///             ["identityserver"] = "IdentityServer",
///             ["bff"] = "BFF",
///         },
///     };
///
///     public IEnumerable&lt;OgImageInput&gt; GetDocuments(CollectionQuery query)
///     {
///         var docs = query.GetCollection&lt;DocSchema&gt;("docs");
///         foreach (var entry in docs)
///         {
///         yield return new OgImageInput(
///                 entry.Data.Title,
///                 $"/docs/{entry.Slug}",
///                 entry.Data.Description,
///                 null);
///         }
///     }
/// }
/// </code>
/// </example>
public interface IOgImageConfiguration
{
    /// <summary>
    /// Returns the OpenGraph rendering configuration (background image, fonts, colors, category mappings).
    /// </summary>
    /// <returns>A configured <see cref="OpenGraphConfig"/> instance.</returns>
    OpenGraphConfig GetOpenGraphConfig();

    /// <summary>
    /// Returns the documents for which OG images should be generated.
    /// </summary>
    /// <param name="query">The content collection query for accessing content entries.</param>
    /// <returns>An enumerable of <see cref="OgImageInput"/> descriptors.</returns>
    IEnumerable<OgImageInput> GetDocuments(CollectionQuery query);
}
