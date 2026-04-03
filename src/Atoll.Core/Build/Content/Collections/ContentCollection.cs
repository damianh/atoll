namespace Atoll.Content.Collections;

/// <summary>
/// Defines a content collection by name and schema type. This is the Atoll equivalent
/// of Astro's collection definition in <c>src/content/config.ts</c>.
/// </summary>
/// <remarks>
/// <para>
/// A content collection maps a named directory (e.g., <c>"blog"</c>) to a schema type
/// that defines the expected frontmatter structure. Each file in the collection directory
/// becomes a <see cref="ContentEntry{TData}"/> whose <c>Data</c> is validated against
/// the schema type using DataAnnotations.
/// </para>
/// <para>
/// Use <see cref="ContentCollection.Define{TData}(string)"/> to create collection definitions
/// with type-safe schema binding.
/// </para>
/// </remarks>
public sealed class ContentCollection
{
    private ContentCollection(string name, Type schemaType)
    {
        Name = name;
        SchemaType = schemaType;
    }

    /// <summary>
    /// Gets the name of this collection. This corresponds to the directory name
    /// under the content base directory (e.g., <c>"blog"</c>).
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the <see cref="Type"/> of the schema class used to validate frontmatter data.
    /// </summary>
    public Type SchemaType { get; }

    /// <summary>
    /// Defines a new content collection with the specified name and schema type.
    /// </summary>
    /// <typeparam name="TData">
    /// The schema type for frontmatter data. Must be a class with a parameterless constructor.
    /// Properties should use DataAnnotation attributes for validation.
    /// </typeparam>
    /// <param name="name">
    /// The collection name, which corresponds to the directory name under the content base directory.
    /// </param>
    /// <returns>A new <see cref="ContentCollection"/> definition.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException"><paramref name="name"/> is empty or whitespace.</exception>
    public static ContentCollection Define<TData>(string name) where TData : class, new()
    {
        ArgumentNullException.ThrowIfNull(name);
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Collection name cannot be empty or whitespace.", nameof(name));
        }

        return new ContentCollection(name, typeof(TData));
    }
}
