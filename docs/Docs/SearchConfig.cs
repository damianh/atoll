using Atoll.Build.Content.Collections;
using Atoll.Lagoon.Search;

namespace Docs;

/// <summary>
/// Search index configuration for the documentation site.
/// Provides all documentation entries for client-side search.
/// </summary>
public sealed class SearchConfig : ISearchIndexConfiguration
{
    /// <inheritdoc />
    public IEnumerable<SearchDocumentInput> GetDocuments(CollectionQuery query)
    {
        var docs = query.GetCollection<DocSchema>("docs");
        foreach (var entry in docs)
        {
            var rendered = query.Render(entry);
            yield return new SearchDocumentInput(entry.Data.Title, $"/docs/{entry.Slug}")
            {
                Description = entry.Data.Description,
                Section = entry.Data.Section.Length > 0 ? entry.Data.Section : null,
                HtmlBody = rendered.Html,
            };
        }
    }
}
