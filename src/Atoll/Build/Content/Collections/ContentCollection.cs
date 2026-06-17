namespace Atoll.Build.Content.Collections;

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
    /// Gets the name of this collection. This is the logical identifier for the collection
    /// and does not necessarily correspond to a directory name when <see cref="Directory"/> is set.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the <see cref="Type"/> of the schema class used to validate frontmatter data.
    /// </summary>
    public Type SchemaType { get; }

    /// <summary>
    /// Gets the optional directory path override for this collection.
    /// When set, this path is used instead of <c>BaseDirectory/Name</c>.
    /// The path is resolved relative to the project root.
    /// </summary>
    /// <remarks>
    /// This is intended for monorepo scenarios where content lives outside the default
    /// content base directory — for example, co-located with a component library.
    /// No path escaping restrictions are applied; any path reachable from the project root
    /// is permitted by design.
    /// </remarks>
    public string? Directory { get; private set; }

    /// <summary>
    /// Gets the optional URL path prefix for this collection.
    /// When set, pages in this collection are expected to live under this prefix
    /// (e.g., <c>"/docs"</c> means entries are accessible at <c>/docs/{slug}</c>).
    /// When <c>null</c>, the collection name is used as the default prefix.
    /// </summary>
    public string? Prefix { get; private set; }

    /// <summary>
    /// Overrides the directory for this collection to the specified path,
    /// resolved relative to the project root.
    /// </summary>
    /// <param name="directory">
    /// The directory path, relative to the project root (e.g.,
    /// <c>"../../libs/identity-server/docs/content"</c>).
    /// </param>
    /// <returns>This <see cref="ContentCollection"/> instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="directory"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException"><paramref name="directory"/> is empty or whitespace.</exception>
    public ContentCollection FromDirectory(string directory)
    {
        ArgumentNullException.ThrowIfNull(directory);
        if (string.IsNullOrWhiteSpace(directory))
        {
            throw new ArgumentException("Directory cannot be empty or whitespace.", nameof(directory));
        }

        Directory = directory;
        return this;
    }

    /// <summary>
    /// Sets the URL path prefix for this collection. Pages belonging to this collection
    /// will have their hrefs built as <c>{prefix}/{slug}</c>.
    /// </summary>
    /// <param name="prefix">
    /// The URL path prefix (e.g., <c>"/docs"</c> or <c>"/guides"</c>).
    /// Must start with <c>/</c> and must not end with <c>/</c>.
    /// </param>
    /// <returns>This <see cref="ContentCollection"/> instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="prefix"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="prefix"/> is empty, whitespace, does not start with <c>/</c>,
    /// or ends with <c>/</c>.
    /// </exception>
    public ContentCollection WithPrefix(string prefix)
    {
        ArgumentNullException.ThrowIfNull(prefix);
        if (string.IsNullOrWhiteSpace(prefix))
        {
            throw new ArgumentException("Prefix cannot be empty or whitespace.", nameof(prefix));
        }

        if (!prefix.StartsWith('/'))
        {
            throw new ArgumentException("Prefix must start with '/'.", nameof(prefix));
        }

        if (prefix.Length > 1 && prefix.EndsWith('/'))
        {
            throw new ArgumentException("Prefix must not end with '/'.", nameof(prefix));
        }

        Prefix = prefix;
        return this;
    }

    /// <summary>
    /// Defines a new content collection with the specified name and schema type.
    /// </summary>
    /// <typeparam name="TData">
    /// The schema type for frontmatter data. Must be a class with a parameterless constructor.
    /// Properties should use DataAnnotation attributes for validation.
    /// </typeparam>
    /// <param name="name">
    /// The logical name of the collection. When no <see cref="FromDirectory"/> override is set,
    /// this also corresponds to the subdirectory name under <see cref="CollectionConfig.BaseDirectory"/>.
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
