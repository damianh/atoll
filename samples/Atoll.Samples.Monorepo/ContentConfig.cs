using Atoll.Build.Content.Collections;

namespace Atoll.Samples.Monorepo;

/// <summary>
/// Content collection configuration demonstrating custom collection directories.
/// The "blog" collection uses the standard base directory layout, while the
/// "weather-api-docs" collection pulls content from an external directory
/// that simulates a sibling package in a monorepo.
/// </summary>
public sealed class ContentConfig : IContentConfiguration
{
    /// <inheritdoc />
    public CollectionConfig Configure()
    {
        return new CollectionConfig("Content")
            // Standard collection — reads from Content/blog/
            .AddCollection(ContentCollection.Define<BlogPostSchema>("blog"))
            // Custom directory — reads from ExternalDocs/weather-api/
            // In a real monorepo this might be "../../libs/weather-api/docs"
            .AddCollection(ContentCollection.Define<DocSchema>("weather-api-docs")
                .FromDirectory("ExternalDocs/weather-api"));
    }
}
