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
    /// Gets all entries in the specified collection, scanning subdirectories recursively.
    /// </summary>
    /// <typeparam name="TData">The schema type for frontmatter data.</typeparam>
    /// <param name="collectionName">The collection name.</param>
    /// <returns>A read-only list of all entries in the collection.</returns>
    public IReadOnlyList<ContentEntry<TData>> GetCollection<TData>(string collectionName)
        where TData : class, new()
    {
        return _loader.LoadCollection<TData>(collectionName, recursive: true);
    }

    /// <summary>
    /// Gets all entries in the specified collection that match the given predicate,
    /// scanning subdirectories recursively.
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
        return _loader.LoadCollection<TData>(collectionName, recursive: true)
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

        var options = WithContentAssetBasePath(_markdownOptions, entry);
        var result = MarkdownRenderer.Render(entry.Body, options);
        return result.Fragments is not null
            ? new RenderedContent(result.Html, result.Headings, result.Fragments, result.AllReferences)
            : new RenderedContent(result.Html, result.Headings);
    }

    /// <summary>
    /// Returns the markdown options with <see cref="LinkResolutionOptions.ContentAssetBasePath"/>
    /// set for the given content entry. When no URL base path is available (neither
    /// <see cref="MarkdownOptions.ContentBasePath"/> nor <see cref="LinkResolutionOptions.BasePath"/>
    /// is set), returns the original options unchanged.
    /// </summary>
    private static MarkdownOptions WithContentAssetBasePath<TData>(
        MarkdownOptions options, ContentEntry<TData> entry)
        where TData : class, new()
    {
        // Determine the URL base path: prefer ContentBasePath, fall back to LinkResolution.BasePath.
        var urlBasePath = options.ContentBasePath
            ?? options.LinkResolution?.BasePath;

        if (urlBasePath is null)
        {
            return options;
        }

        // Compute the content asset base path from the entry's collection and slug.
        // The entry's slug directory (parent path in URL space) determines where relative
        // asset URLs are resolved. For example:
        //   Collection "articles", Slug "product-development-process"
        //     → entry directory is "articles/" → asset base = "{BasePath}/articles"
        //   Collection "docs", Slug "guides/getting-started"
        //     → entry directory is "docs/guides/" → asset base = "{BasePath}/docs/guides"
        var basePath = urlBasePath.TrimEnd('/');
        var entryDir = entry.Collection;
        var lastSlash = entry.Slug.LastIndexOf('/');
        if (lastSlash >= 0)
        {
            entryDir = entry.Collection + "/" + entry.Slug[..lastSlash];
        }

        var contentAssetBasePath = $"{basePath}/{entryDir}";

        // Only create a new options instance if the value actually differs.
        if (options.LinkResolution is not null &&
            string.Equals(contentAssetBasePath, options.LinkResolution.ContentAssetBasePath, StringComparison.Ordinal))
        {
            return options;
        }

        // Clone or create the link resolution options with the entry-specific asset base path.
        var entryLinkResolution = options.LinkResolution is not null
            ? new LinkResolutionOptions
            {
                BasePath = options.LinkResolution.BasePath,
                AddTrailingSlash = options.LinkResolution.AddTrailingSlash,
                ExtensionsToStrip = options.LinkResolution.ExtensionsToStrip,
                ContentAssetBasePath = contentAssetBasePath,
            }
            : new LinkResolutionOptions
            {
                ContentAssetBasePath = contentAssetBasePath,
            };

        return new MarkdownOptions
        {
            EnableTables = options.EnableTables,
            EnableAutoLinks = options.EnableAutoLinks,
            EnableTaskLists = options.EnableTaskLists,
            EnableEmphasisExtras = options.EnableEmphasisExtras,
            EnableFootnotes = options.EnableFootnotes,
            EnableAutoIdentifiers = options.EnableAutoIdentifiers,
            CodeBlockClass = options.CodeBlockClass,
            LinkResolution = entryLinkResolution,
            ExternalLinks = options.ExternalLinks,
            Components = options.Components,
            Extensions = options.Extensions,
            ContentBasePath = options.ContentBasePath,
        };
    }
}
