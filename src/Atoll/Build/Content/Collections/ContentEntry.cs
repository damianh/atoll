namespace Atoll.Build.Content.Collections;

/// <summary>
/// Represents a single entry in a content collection, combining frontmatter data
/// with the raw Markdown body. This is the Atoll equivalent of Astro's content entry module.
/// </summary>
/// <typeparam name="TData">
/// The schema type for the entry's frontmatter data. Must have a parameterless constructor.
/// </typeparam>
/// <remarks>
/// <para>
/// Each content entry corresponds to a single file (typically Markdown) in a content collection
/// directory. The <see cref="Id"/> is derived from the file path relative to the collection
/// directory, the <see cref="Slug"/> is derived from the file name without extension,
/// and the <see cref="Data"/> property contains the parsed and validated frontmatter.
/// </para>
/// </remarks>
public sealed class ContentEntry<TData> where TData : class, new()
{
    /// <summary>
    /// Initializes a new instance of <see cref="ContentEntry{TData}"/>.
    /// </summary>
    /// <param name="id">The unique identifier for this entry, typically the relative file path.</param>
    /// <param name="collection">The name of the collection this entry belongs to.</param>
    /// <param name="slug">The URL-friendly slug derived from the file name.</param>
    /// <param name="body">The raw content body (Markdown text without frontmatter).</param>
    /// <param name="data">The parsed and validated frontmatter data.</param>
    public ContentEntry(string id, string collection, string slug, string body, TData data)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(collection);
        ArgumentNullException.ThrowIfNull(slug);
        ArgumentNullException.ThrowIfNull(body);
        ArgumentNullException.ThrowIfNull(data);

        Id = id;
        Collection = collection;
        Slug = slug;
        Body = body;
        Data = data;
    }

    /// <summary>
    /// Gets the unique identifier for this entry, typically the relative file path
    /// within the collection directory (e.g., <c>"blog/my-post.md"</c>).
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the name of the collection this entry belongs to (e.g., <c>"blog"</c>).
    /// </summary>
    public string Collection { get; }

    /// <summary>
    /// Gets the URL-friendly slug derived from the file name
    /// (e.g., <c>"my-post"</c> from <c>"my-post.md"</c>).
    /// </summary>
    public string Slug { get; }

    /// <summary>
    /// Gets the raw content body (Markdown text without the frontmatter section).
    /// </summary>
    public string Body { get; }

    /// <summary>
    /// Gets the parsed and validated frontmatter data.
    /// </summary>
    public TData Data { get; }
}
