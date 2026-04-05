using Atoll.Build.Content.Collections;
using Atoll.Build.Content.Markdown;
using Atoll.Lagoon.Components;
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
        var markdownOptions = DocsMarkdownRenderer.CreateMarkdownOptions(DocsSetup.Config)
            ?? new MarkdownOptions();

        // Register Lagoon content components so :::directive syntax works in .md/.mda files.
        markdownOptions.Components = new ComponentMap()
            .Add<Aside>("aside")
            .Add<Card>("card")
            .Add<CardGrid>("card-grid")
            .Add<Steps>("steps")
            .Add<LinkCard>("link-card")
            .Add<LinkButton>("link-button")
            .Add<Icon>("icon");

        config.Markdown = markdownOptions;

        return config;
    }
}
