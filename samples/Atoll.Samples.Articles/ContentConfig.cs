using Atoll.Build.Content.Collections;
using Atoll.Reef.Configuration;

namespace Atoll.Samples.Articles;

/// <summary>
/// Content collection configuration for the articles sample.
/// Declares the "articles" collection with <see cref="ArticleSchema"/> frontmatter.
/// </summary>
public sealed class ContentConfig : IContentConfiguration
{
    /// <inheritdoc />
    public CollectionConfig Configure()
    {
        return new CollectionConfig("Content")
            .AddCollection(ContentCollection.Define<ArticleSchema>("articles"));
    }
}
