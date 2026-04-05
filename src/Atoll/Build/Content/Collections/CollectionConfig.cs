using System.Collections.ObjectModel;
using Atoll.Build.Content.Markdown;

namespace Atoll.Build.Content.Collections;

/// <summary>
/// Configuration for content collections, specifying the base content directory
/// and the set of defined collections. This is the Atoll equivalent of Astro's
/// <c>src/content/config.ts</c> configuration.
/// </summary>
/// <remarks>
/// <para>
/// The base directory defaults to <c>"src/content"</c> (relative to the project root).
/// Each collection definition maps a subdirectory name to a schema type, so a collection
/// named <c>"blog"</c> will scan files in <c>{BaseDirectory}/blog/</c>.
/// </para>
/// </remarks>
public sealed class CollectionConfig
{
    /// <summary>
    /// The default base directory for content collections, relative to the project root.
    /// </summary>
    public static readonly string DefaultBaseDirectory = Path.Combine("src", "content");

    private readonly Dictionary<string, ContentCollection> _collections = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes a new instance of <see cref="CollectionConfig"/> with the default base directory.
    /// </summary>
    public CollectionConfig()
        : this(DefaultBaseDirectory)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="CollectionConfig"/> with the specified base directory.
    /// </summary>
    /// <param name="baseDirectory">The base directory path for content collections.</param>
    /// <exception cref="ArgumentNullException"><paramref name="baseDirectory"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException"><paramref name="baseDirectory"/> is empty or whitespace.</exception>
    public CollectionConfig(string baseDirectory)
    {
        ArgumentNullException.ThrowIfNull(baseDirectory);
        if (string.IsNullOrWhiteSpace(baseDirectory))
        {
            throw new ArgumentException("Base directory cannot be empty or whitespace.", nameof(baseDirectory));
        }

        BaseDirectory = baseDirectory;
    }

    /// <summary>
    /// Gets the base directory path for content collections.
    /// Collection subdirectories are resolved relative to this path.
    /// </summary>
    public string BaseDirectory { get; }

    /// <summary>
    /// Gets or sets the Markdown rendering options for this content configuration.
    /// When <c>null</c>, the default <see cref="MarkdownOptions"/> are used.
    /// Addon packages (e.g. <c>Atoll.Lagoon</c>) can populate this with
    /// custom pipeline extensions for syntax highlighting, diagrams, etc.
    /// Default: <c>null</c>.
    /// </summary>
    public MarkdownOptions? Markdown { get; set; }

    /// <summary>
    /// Gets a read-only dictionary of all registered collections, keyed by collection name.
    /// </summary>
    public IReadOnlyDictionary<string, ContentCollection> Collections =>
        new ReadOnlyDictionary<string, ContentCollection>(_collections);

    /// <summary>
    /// Adds a content collection to this configuration.
    /// </summary>
    /// <param name="collection">The collection definition to add.</param>
    /// <returns>This <see cref="CollectionConfig"/> instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="collection"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">A collection with the same name already exists.</exception>
    public CollectionConfig AddCollection(ContentCollection collection)
    {
        ArgumentNullException.ThrowIfNull(collection);
        if (!_collections.TryAdd(collection.Name, collection))
        {
            throw new ArgumentException(
                $"A collection with the name '{collection.Name}' has already been registered.",
                nameof(collection));
        }

        return this;
    }

    /// <summary>
    /// Gets the full directory path for a named collection.
    /// </summary>
    /// <param name="collectionName">The collection name.</param>
    /// <returns>The full directory path for the collection.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="collectionName"/> is <c>null</c>.</exception>
    public string GetCollectionDirectory(string collectionName)
    {
        ArgumentNullException.ThrowIfNull(collectionName);
        return Path.Combine(BaseDirectory, collectionName);
    }

    /// <summary>
    /// Gets the collection definition for the specified name.
    /// </summary>
    /// <param name="collectionName">The collection name.</param>
    /// <returns>The <see cref="ContentCollection"/> definition.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="collectionName"/> is <c>null</c>.</exception>
    /// <exception cref="KeyNotFoundException">No collection with the specified name is registered.</exception>
    public ContentCollection GetCollection(string collectionName)
    {
        ArgumentNullException.ThrowIfNull(collectionName);
        if (!_collections.TryGetValue(collectionName, out var collection))
        {
            throw new KeyNotFoundException(
                $"No content collection named '{collectionName}' is registered. " +
                $"Registered collections: {string.Join(", ", _collections.Keys)}");
        }

        return collection;
    }
}
