namespace Atoll.Content.Collections;

/// <summary>
/// Defines the content collection configuration for a project.
/// Implement this interface in your project to declare which content
/// collections are available and their frontmatter schema types.
/// </summary>
/// <remarks>
/// <para>
/// This is the Atoll equivalent of Astro's <c>src/content/config.ts</c>.
/// The SSG build pipeline discovers implementations of this interface
/// via assembly scanning and uses the returned <see cref="CollectionConfig"/>
/// to set up the content query system.
/// </para>
/// <para>
/// A project should have exactly one implementation. If multiple are found,
/// the first is used.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public sealed class ContentConfig : IContentConfiguration
/// {
///     public CollectionConfig Configure()
///     {
///         return new CollectionConfig("Content")
///             .AddCollection(ContentCollection.Define&lt;BlogPostSchema&gt;("blog"));
///     }
/// }
/// </code>
/// </example>
public interface IContentConfiguration
{
    /// <summary>
    /// Configures the content collections for this project.
    /// </summary>
    /// <returns>A <see cref="CollectionConfig"/> with all collection definitions.</returns>
    CollectionConfig Configure();
}
