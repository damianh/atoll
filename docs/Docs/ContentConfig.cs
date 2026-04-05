using Atoll.Build.Content.Collections;
using Atoll.Lagoon.Markdown;

namespace Docs;

/// <summary>
/// Content collection configuration for the documentation site.
/// Declares the "docs" collection with <see cref="DocSchema"/> frontmatter.
/// </summary>
public sealed class ContentConfig : IContentConfiguration
{
    /// <inheritdoc />
    public CollectionConfig Configure()
    {
        var config = new CollectionConfig("Content")
            .AddCollection(ContentCollection.Define<DocSchema>("docs"));

        // Apply Lagoon markdown extensions (syntax highlighting, Mermaid)
        // so that CollectionQuery.Render() uses them.
        config.Markdown = DocsMarkdownRenderer.CreateMarkdownOptions(DocsSetup.Config);

        return config;
    }
}
