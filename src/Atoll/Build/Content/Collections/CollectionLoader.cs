using Atoll.Build.Content.Frontmatter;

namespace Atoll.Build.Content.Collections;

/// <summary>
/// Loads content entries from the filesystem for a given collection.
/// Scans the collection directory for Markdown files, parses frontmatter,
/// validates data against the schema type, and produces <see cref="ContentEntry{TData}"/> instances.
/// </summary>
/// <remarks>
/// <para>
/// The loader uses an <see cref="IFileProvider"/> abstraction to read files, making it
/// testable without filesystem access. For production use, create a <see cref="CollectionLoader"/>
/// with a <see cref="PhysicalFileProvider"/> (or similar). For testing, use an in-memory provider.
/// </para>
/// </remarks>
public sealed class CollectionLoader
{
    private readonly CollectionConfig _config;
    private readonly IFileProvider _fileProvider;

    /// <summary>
    /// Initializes a new instance of <see cref="CollectionLoader"/>.
    /// </summary>
    /// <param name="config">The content collection configuration.</param>
    /// <param name="fileProvider">The file provider used to read content files.</param>
    public CollectionLoader(CollectionConfig config, IFileProvider fileProvider)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(fileProvider);
        _config = config;
        _fileProvider = fileProvider;
    }

    /// <summary>
    /// Loads all entries from the specified collection.
    /// </summary>
    /// <typeparam name="TData">The schema type for frontmatter data.</typeparam>
    /// <param name="collectionName">The name of the collection to load.</param>
    /// <returns>A list of content entries, sorted by file path.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="collectionName"/> is <c>null</c>.</exception>
    /// <exception cref="KeyNotFoundException">The collection is not registered.</exception>
    public IReadOnlyList<ContentEntry<TData>> LoadCollection<TData>(string collectionName)
        where TData : class, new()
    {
        ArgumentNullException.ThrowIfNull(collectionName);
        var collection = _config.GetCollection(collectionName);
        ValidateSchemaType<TData>(collection);

        var directory = _config.GetCollectionDirectory(collectionName);
        var files = _fileProvider.GetMarkdownFiles(directory);
        var entries = new List<ContentEntry<TData>>();

        foreach (var file in files.OrderBy(f => f.RelativePath, StringComparer.OrdinalIgnoreCase))
        {
            var entry = LoadEntry<TData>(collectionName, file);
            entries.Add(entry);
        }

        return entries;
    }

    /// <summary>
    /// Loads all entries from the specified collection, optionally scanning subdirectories recursively.
    /// </summary>
    /// <typeparam name="TData">The schema type for frontmatter data.</typeparam>
    /// <param name="collectionName">The name of the collection to load.</param>
    /// <param name="recursive">
    /// When <c>true</c>, subdirectories are scanned recursively and the <c>RelativePath</c>
    /// of each entry includes the subdirectory (e.g., <c>guides/getting-started.md</c>).
    /// When <c>false</c>, behaves identically to <see cref="LoadCollection{TData}(string)"/>.
    /// </param>
    /// <returns>A list of content entries, sorted by relative file path.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="collectionName"/> is <c>null</c>.</exception>
    /// <exception cref="KeyNotFoundException">The collection is not registered.</exception>
    public IReadOnlyList<ContentEntry<TData>> LoadCollection<TData>(string collectionName, bool recursive)
        where TData : class, new()
    {
        ArgumentNullException.ThrowIfNull(collectionName);

        if (!recursive)
        {
            return LoadCollection<TData>(collectionName);
        }

        var collection = _config.GetCollection(collectionName);
        ValidateSchemaType<TData>(collection);

        var directory = _config.GetCollectionDirectory(collectionName);
        var files = _fileProvider.GetMarkdownFiles(directory, recursive: true);
        var entries = new List<ContentEntry<TData>>();

        foreach (var file in files.OrderBy(f => f.RelativePath, StringComparer.OrdinalIgnoreCase))
        {
            var entry = LoadEntry<TData>(collectionName, file);
            entries.Add(entry);
        }

        return entries;
    }

    /// <summary>
    /// Loads a single entry from the specified collection by slug.
    /// </summary>
    /// <typeparam name="TData">The schema type for frontmatter data.</typeparam>
    /// <param name="collectionName">The name of the collection.</param>
    /// <param name="slug">The entry slug (file name without extension).</param>
    /// <returns>The content entry, or <c>null</c> if not found.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="collectionName"/> or <paramref name="slug"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="KeyNotFoundException">The collection is not registered.</exception>
    public ContentEntry<TData>? LoadEntry<TData>(string collectionName, string slug)
        where TData : class, new()
    {
        ArgumentNullException.ThrowIfNull(collectionName);
        ArgumentNullException.ThrowIfNull(slug);
        var collection = _config.GetCollection(collectionName);
        ValidateSchemaType<TData>(collection);

        var directory = _config.GetCollectionDirectory(collectionName);
        var file = _fileProvider.GetMarkdownFile(directory, slug);

        if (file is null)
        {
            return null;
        }

        return LoadEntry<TData>(collectionName, file);
    }

    private static ContentEntry<TData> LoadEntry<TData>(string collectionName, ContentFile file)
        where TData : class, new()
    {
        var parseResult = FrontmatterParser.Parse(file.Content);
        var data = FrontmatterBinder.Bind<TData>(parseResult.RawFrontmatter);
        var validationResult = FrontmatterValidator.Validate(data);
        var entryId = $"{collectionName}/{file.RelativePath}";
        validationResult.ThrowIfInvalid(entryId);

        // Slug is the relative path without extension, using forward slashes.
        // For top-level files: "my-post.md" → "my-post"
        // For nested files:    "guides/getting-started.md" → "guides/getting-started"
        var relativePath = file.RelativePath.Replace('\\', '/');
        var slug = relativePath.Contains('/')
            ? relativePath[..relativePath.LastIndexOf('.')] // strip extension only
            : Path.GetFileNameWithoutExtension(relativePath);

        return new ContentEntry<TData>(entryId, collectionName, slug, parseResult.Body, data);
    }

    private static void ValidateSchemaType<TData>(ContentCollection collection) where TData : class, new()
    {
        if (collection.SchemaType != typeof(TData))
        {
            throw new InvalidOperationException(
                $"Collection '{collection.Name}' was defined with schema type " +
                $"'{collection.SchemaType.Name}' but was queried with type '{typeof(TData).Name}'.");
        }
    }
}

/// <summary>
/// Represents a content file loaded from the filesystem.
/// </summary>
public sealed class ContentFile
{
    /// <summary>
    /// Initializes a new instance of <see cref="ContentFile"/>.
    /// </summary>
    /// <param name="relativePath">The relative path within the collection directory.</param>
    /// <param name="content">The file content.</param>
    public ContentFile(string relativePath, string content)
    {
        ArgumentNullException.ThrowIfNull(relativePath);
        ArgumentNullException.ThrowIfNull(content);
        RelativePath = relativePath;
        Content = content;
    }

    /// <summary>
    /// Gets the file path relative to the collection directory (e.g., <c>"my-post.md"</c>).
    /// </summary>
    public string RelativePath { get; }

    /// <summary>
    /// Gets the full content of the file.
    /// </summary>
    public string Content { get; }
}

/// <summary>
/// Abstraction for reading content files from a directory.
/// Enables testing without filesystem access.
/// </summary>
public interface IFileProvider
{
    /// <summary>
    /// Gets all Markdown files (*.md) in the specified directory (non-recursive).
    /// </summary>
    /// <param name="directory">The directory path to scan.</param>
    /// <returns>A collection of content files.</returns>
    IReadOnlyList<ContentFile> GetMarkdownFiles(string directory);

    /// <summary>
    /// Gets all Markdown files (*.md) in the specified directory, optionally scanning subdirectories.
    /// </summary>
    /// <param name="directory">The directory path to scan.</param>
    /// <param name="recursive">
    /// When <c>true</c>, subdirectories are scanned recursively and each file's
    /// <see cref="ContentFile.RelativePath"/> includes the subdirectory
    /// (e.g., <c>"guides/getting-started.md"</c>).
    /// When <c>false</c>, behaves identically to <see cref="GetMarkdownFiles(string)"/>.
    /// </param>
    /// <returns>A collection of content files.</returns>
    IReadOnlyList<ContentFile> GetMarkdownFiles(string directory, bool recursive);

    /// <summary>
    /// Gets a single Markdown file by slug (file name without extension).
    /// </summary>
    /// <param name="directory">The directory path to search.</param>
    /// <param name="slug">The file name without extension.</param>
    /// <returns>The content file, or <c>null</c> if not found.</returns>
    ContentFile? GetMarkdownFile(string directory, string slug);
}

/// <summary>
/// An <see cref="IFileProvider"/> that reads from the physical filesystem.
/// </summary>
public sealed class PhysicalFileProvider : IFileProvider
{
    /// <inheritdoc />
    public IReadOnlyList<ContentFile> GetMarkdownFiles(string directory)
    {
        ArgumentNullException.ThrowIfNull(directory);

        if (!Directory.Exists(directory))
        {
            return [];
        }

        var files = Directory.GetFiles(directory, "*.md", SearchOption.TopDirectoryOnly);
        var entries = new List<ContentFile>();

        foreach (var file in files)
        {
            var relativePath = Path.GetFileName(file);
            var content = File.ReadAllText(file);
            entries.Add(new ContentFile(relativePath, content));
        }

        return entries;
    }

    /// <inheritdoc />
    public IReadOnlyList<ContentFile> GetMarkdownFiles(string directory, bool recursive)
    {
        ArgumentNullException.ThrowIfNull(directory);

        if (!recursive)
        {
            return GetMarkdownFiles(directory);
        }

        if (!Directory.Exists(directory))
        {
            return [];
        }

        var files = Directory.GetFiles(directory, "*.md", SearchOption.AllDirectories);
        var entries = new List<ContentFile>();
        var normalizedBase = directory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        foreach (var file in files)
        {
            var fullPath = file;
            var relativePath = fullPath[(normalizedBase.Length + 1)..]
                .Replace(Path.DirectorySeparatorChar, '/');
            var content = File.ReadAllText(file);
            entries.Add(new ContentFile(relativePath, content));
        }

        return entries;
    }

    /// <inheritdoc />
    public ContentFile? GetMarkdownFile(string directory, string slug)
    {
        ArgumentNullException.ThrowIfNull(directory);
        ArgumentNullException.ThrowIfNull(slug);

        var filePath = Path.Combine(directory, slug + ".md");
        if (!File.Exists(filePath))
        {
            return null;
        }

        var content = File.ReadAllText(filePath);
        return new ContentFile(slug + ".md", content);
    }
}

/// <summary>
/// An in-memory <see cref="IFileProvider"/> for testing content collections
/// without filesystem access.
/// </summary>
public sealed class InMemoryFileProvider : IFileProvider
{
    private readonly Dictionary<string, List<ContentFile>> _files = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Adds a file to the in-memory provider.
    /// </summary>
    /// <param name="directory">The directory path this file belongs to.</param>
    /// <param name="relativePath">The relative file name (e.g., <c>"my-post.md"</c>).</param>
    /// <param name="content">The file content.</param>
    /// <returns>This instance for fluent chaining.</returns>
    public InMemoryFileProvider AddFile(string directory, string relativePath, string content)
    {
        ArgumentNullException.ThrowIfNull(directory);
        ArgumentNullException.ThrowIfNull(relativePath);
        ArgumentNullException.ThrowIfNull(content);

        // Normalize path separators
        var normalizedDir = NormalizePath(directory);

        if (!_files.TryGetValue(normalizedDir, out var list))
        {
            list = [];
            _files[normalizedDir] = list;
        }

        list.Add(new ContentFile(relativePath, content));
        return this;
    }

    /// <inheritdoc />
    public IReadOnlyList<ContentFile> GetMarkdownFiles(string directory)
    {
        ArgumentNullException.ThrowIfNull(directory);
        var normalizedDir = NormalizePath(directory);

        if (!_files.TryGetValue(normalizedDir, out var list))
        {
            return [];
        }

        return list.Where(f => f.RelativePath.EndsWith(".md", StringComparison.OrdinalIgnoreCase)).ToList();
    }

    /// <inheritdoc />
    public IReadOnlyList<ContentFile> GetMarkdownFiles(string directory, bool recursive)
    {
        ArgumentNullException.ThrowIfNull(directory);

        if (!recursive)
        {
            return GetMarkdownFiles(directory);
        }

        var normalizedDir = NormalizePath(directory);

        // Collect all files under the directory prefix (including subdirectories).
        var results = new List<ContentFile>();
        foreach (var (dir, files) in _files)
        {
            // Match exact directory OR any subdirectory that starts with normalizedDir + "/"
            if (dir.Equals(normalizedDir, StringComparison.OrdinalIgnoreCase) ||
                dir.StartsWith(normalizedDir + "/", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var file in files)
                {
                    if (!file.RelativePath.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    // Build relative path from base directory.
                    var subdirRelative = dir.Equals(normalizedDir, StringComparison.OrdinalIgnoreCase)
                        ? ""
                        : dir[(normalizedDir.Length + 1)..] + "/";

                    var fullRelativePath = subdirRelative + file.RelativePath;
                    results.Add(new ContentFile(fullRelativePath, file.Content));
                }
            }
        }

        return results;
    }

    /// <inheritdoc />
    public ContentFile? GetMarkdownFile(string directory, string slug)
    {
        ArgumentNullException.ThrowIfNull(directory);
        ArgumentNullException.ThrowIfNull(slug);
        var normalizedDir = NormalizePath(directory);

        if (!_files.TryGetValue(normalizedDir, out var list))
        {
            return null;
        }

        var fileName = slug + ".md";
        return list.FirstOrDefault(f =>
            string.Equals(f.RelativePath, fileName, StringComparison.OrdinalIgnoreCase));
    }

    private static string NormalizePath(string path)
    {
        return path.Replace('\\', '/').TrimEnd('/');
    }
}
