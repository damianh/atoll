using Atoll.Build.Content.Collections;
using Atoll.Lagoon.LlmsTxt;

namespace Docs;

/// <summary>
/// LLM-optimised content export configuration for the documentation site.
/// Generates <c>llms.txt</c> and <c>llms-full.txt</c> files for AI agent consumption.
/// </summary>
public sealed class LlmsTxtConfig : ILlmsTxtConfiguration
{
    /// <inheritdoc />
    public LlmsTxtSiteInfo GetSiteInfo() => new(
        DocsSetup.Config.Title,
        DocsSetup.Config.Description);

    /// <inheritdoc />
    public IEnumerable<LlmsTxtDocumentInput> GetDocuments(CollectionQuery query)
    {
        var docs = query.GetCollection<DocSchema>("docs");
        foreach (var entry in docs)
        {
            yield return new LlmsTxtDocumentInput(entry.Data.Title, $"/{entry.Slug}")
            {
                Description = entry.Data.Description,
                Section = entry.Data.Section.Length > 0 ? entry.Data.Section : null,
                MarkdownBody = entry.Body,
            };
        }
    }
}
