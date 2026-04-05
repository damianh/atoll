using Atoll.Build.Content.Collections;
using Atoll.DrawIo.Parsing;

namespace Atoll.DrawIo.Content;

/// <summary>
/// Loads <c>.drawio</c> files from a directory and returns them as
/// <see cref="ContentEntry{TData}"/> objects with auto-populated <see cref="DrawioDiagramSchema"/> metadata.
/// </summary>
/// <remarks>
/// Unlike the core Atoll content collection pipeline (which is designed for Markdown+frontmatter),
/// this loader operates independently and handles the <c>.drawio</c> binary/XML format directly.
/// Each <c>.drawio</c> file becomes one <see cref="ContentEntry{TData}"/> where:
/// <list type="bullet">
///   <item><description><c>Id</c> — relative file path within the directory.</description></item>
///   <item><description><c>Slug</c> — file name without extension.</description></item>
///   <item><description><c>Body</c> — raw XML content of the file.</description></item>
///   <item><description><c>Data</c> — auto-populated <see cref="DrawioDiagramSchema"/> extracted via <see cref="DrawioFileParser"/>.</description></item>
/// </list>
/// </remarks>
public sealed class DrawioCollectionLoader
{
    private const string DrawioExtension = ".drawio";
    private const string DioExtension = ".dio";

    /// <summary>
    /// Loads all <c>.drawio</c> and <c>.dio</c> files from the specified directory.
    /// </summary>
    /// <param name="directory">The absolute path to the directory to scan.</param>
    /// <param name="collectionName">The name of the collection (used for <see cref="ContentEntry{TData}.Collection"/>).</param>
    /// <returns>
    /// A list of <see cref="ContentEntry{TData}"/> instances, one per <c>.drawio</c> file found.
    /// Returns an empty list if the directory does not exist or contains no matching files.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="directory"/> or <paramref name="collectionName"/> is <c>null</c>.</exception>
    public IReadOnlyList<ContentEntry<DrawioDiagramSchema>> LoadCollection(string directory, string collectionName)
    {
        ArgumentNullException.ThrowIfNull(directory);
        ArgumentNullException.ThrowIfNull(collectionName);

        if (!Directory.Exists(directory))
        {
            return [];
        }

        var files = Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories)
            .Where(f => f.EndsWith(DrawioExtension, StringComparison.OrdinalIgnoreCase)
                     || f.EndsWith(DioExtension, StringComparison.OrdinalIgnoreCase))
            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var entries = new List<ContentEntry<DrawioDiagramSchema>>(files.Count);
        foreach (var filePath in files)
        {
            var entry = LoadEntryFromFile(filePath, directory, collectionName);
            if (entry is not null)
            {
                entries.Add(entry);
            }
        }

        return entries;
    }

    /// <summary>
    /// Loads a single <c>.drawio</c> or <c>.dio</c> file as a content entry.
    /// </summary>
    /// <param name="directory">The base directory the file belongs to.</param>
    /// <param name="collectionName">The name of the collection.</param>
    /// <param name="slug">The slug (file name without extension) to load.</param>
    /// <returns>
    /// The <see cref="ContentEntry{TData}"/> if found; <c>null</c> if no matching file exists.
    /// </returns>
    /// <exception cref="ArgumentNullException">Any argument is <c>null</c>.</exception>
    public ContentEntry<DrawioDiagramSchema>? LoadEntry(string directory, string collectionName, string slug)
    {
        ArgumentNullException.ThrowIfNull(directory);
        ArgumentNullException.ThrowIfNull(collectionName);
        ArgumentNullException.ThrowIfNull(slug);

        foreach (var ext in new[] { DrawioExtension, DioExtension })
        {
            var filePath = Path.Combine(directory, slug + ext);
            if (File.Exists(filePath))
            {
                return LoadEntryFromFile(filePath, directory, collectionName);
            }
        }

        return null;
    }

    private static ContentEntry<DrawioDiagramSchema>? LoadEntryFromFile(
        string filePath, string baseDirectory, string collectionName)
    {
        string body;
        try
        {
            body = File.ReadAllText(filePath);
        }
        catch (IOException)
        {
            return null;
        }

        var relativePath = Path.GetRelativePath(baseDirectory, filePath)
            .Replace('\\', '/');
        var slug = Path.GetFileNameWithoutExtension(filePath);
        var title = slug;

        DrawioDiagramSchema schema;
        try
        {
            var file = DrawioFileParser.Parse(body);
            var pages = file.Pages.Select(p => new DrawioPageInfo
            {
                Name   = p.Name,
                Id     = p.Id,
                Layers = p.Model.Layers
                    .Select(l => string.IsNullOrEmpty(l.Value) ? l.Id : l.Value)
                    .ToList(),
            }).ToList();

            schema = new DrawioDiagramSchema
            {
                Title     = title,
                Pages     = pages,
                PageCount = pages.Count,
            };
        }
        catch (InvalidOperationException)
        {
            // Malformed file — return entry with empty schema
            schema = new DrawioDiagramSchema
            {
                Title     = title,
                Pages     = [],
                PageCount = 0,
            };
        }

        return new ContentEntry<DrawioDiagramSchema>(relativePath, collectionName, slug, body, schema);
    }
}
