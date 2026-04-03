using Atoll.Content.Collections;

namespace Atoll.Docs;

/// <summary>
/// Content collection configuration for the documentation site.
/// Declares the "docs" collection with <see cref="DocSchema"/> frontmatter.
/// </summary>
public sealed class ContentConfig : IContentConfiguration
{
    /// <inheritdoc />
    public CollectionConfig Configure()
    {
        return new CollectionConfig("Content")
            .AddCollection(ContentCollection.Define<DocSchema>("docs"));
    }
}
