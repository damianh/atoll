using Atoll.Build.Content.Markdown;

namespace Atoll.Build.Content.Collections;

/// <summary>
/// Provides the query API for content collections: loading, filtering, and rendering entries.
/// This is the Atoll equivalent of Astro's <c>getCollection()</c>, <c>getEntry()</c>,
/// and <c>render()</c> functions from <c>astro:content</c>.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="CollectionQuery"/> is the main entry point for accessing content collections.
/// It wraps a <see cref="CollectionLoader"/> to provide type-safe access to collection entries,
/// and a <see cref="MarkdownRenderer"/> to render entry bodies to HTML.
/// </para>
/// </remarks>
public sealed class CollectionQuery
{
    private readonly CollectionLoader _loader;
    private readonly MarkdownOptions _markdownOptions;

    /// <summary>
    /// Initializes a new instance of <see cref="CollectionQuery"/> with default Markdown options.
    /// </summary>
    /// <param name="loader">The collection loader.</param>
    public CollectionQuery(CollectionLoader loader)
        : this(loader, new MarkdownOptions())
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="CollectionQuery"/> with the specified Markdown options.
    /// </summary>
    /// <param name="loader">The collection loader.</param>
    /// <param name="markdownOptions">The Markdown rendering options.</param>
    public CollectionQuery(CollectionLoader loader, MarkdownOptions markdownOptions)
    {
        ArgumentNullException.ThrowIfNull(loader);
        ArgumentNullException.ThrowIfNull(markdownOptions);
        _loader = loader;
        _markdownOptions = markdownOptions;
    }

    /// <summary>
    /// Gets all entries in the specified collection.
    /// </summary>
    /// <typeparam name="TData">The schema type for frontmatter data.</typeparam>
    /// <param name="collectionName">The collection name.</param>
    /// <returns>A read-only list of all entries in the collection.</returns>
    public IReadOnlyList<ContentEntry<TData>> GetCollection<TData>(string collectionName)
        where TData : class, new()
    {
        return _loader.LoadCollection<TData>(collectionName);
    }

    /// <summary>
    /// Gets all entries in the specified collection that match the given predicate.
    /// </summary>
    /// <typeparam name="TData">The schema type for frontmatter data.</typeparam>
    /// <param name="collectionName">The collection name.</param>
    /// <param name="predicate">A filter function applied to each entry.</param>
    /// <returns>A read-only list of matching entries.</returns>
    public IReadOnlyList<ContentEntry<TData>> GetCollection<TData>(
        string collectionName,
        Func<ContentEntry<TData>, bool> predicate)
        where TData : class, new()
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return _loader.LoadCollection<TData>(collectionName)
            .Where(predicate)
            .ToList();
    }

    /// <summary>
    /// Gets a single entry by slug from the specified collection.
    /// </summary>
    /// <typeparam name="TData">The schema type for frontmatter data.</typeparam>
    /// <param name="collectionName">The collection name.</param>
    /// <param name="slug">The entry slug.</param>
    /// <returns>The content entry, or <c>null</c> if not found.</returns>
    public ContentEntry<TData>? GetEntry<TData>(string collectionName, string slug)
        where TData : class, new()
    {
        return _loader.LoadEntry<TData>(collectionName, slug);
    }

    /// <summary>
    /// Renders the body of a content entry to HTML.
    /// </summary>
    /// <typeparam name="TData">The schema type for frontmatter data.</typeparam>
    /// <param name="entry">The content entry to render.</param>
    /// <returns>A <see cref="RenderedContent"/> containing the HTML and heading metadata.</returns>
    public RenderedContent Render<TData>(ContentEntry<TData> entry)
        where TData : class, new()
    {
        ArgumentNullException.ThrowIfNull(entry);
        var result = MarkdownRenderer.Render(entry.Body, _markdownOptions);
        return result.Fragments is not null
            ? new RenderedContent(result.Html, result.Headings, result.Fragments)
            : new RenderedContent(result.Html, result.Headings);
    }
}
