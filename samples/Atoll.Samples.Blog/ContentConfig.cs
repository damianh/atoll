using Atoll.Build.Content.Collections;

namespace Atoll.Samples.Blog;

/// <summary>
/// Content collection configuration for the blog sample.
/// Declares the "blog" collection with <see cref="BlogPostSchema"/> frontmatter.
/// </summary>
public sealed class ContentConfig : IContentConfiguration
{
    /// <inheritdoc />
    public CollectionConfig Configure()
    {
        return new CollectionConfig("Content")
            .AddCollection(ContentCollection.Define<BlogPostSchema>("blog"));
    }
}
