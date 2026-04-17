namespace Atoll.Lsp.Context;

/// <summary>
/// The full project context extracted from the user's compiled assembly.
/// Contains component metadata and collection schema information needed for diagnostics and completions.
/// </summary>
internal sealed class ProjectContext
{
    internal ProjectContext(
        IReadOnlyDictionary<string, ComponentInfo> components,
        IReadOnlyDictionary<string, CollectionSchemaInfo> collections,
        string baseDirectory)
    {
        Components = components;
        Collections = collections;
        BaseDirectory = baseDirectory;
    }

    /// <summary>
    /// All registered components, keyed by their directive/tag names (case-insensitive).
    /// Both the explicit kebab-case name (e.g., "card-grid") and the PascalCase alias (e.g., "CardGrid")
    /// are present as separate entries pointing to the same <see cref="ComponentInfo"/>.
    /// </summary>
    internal IReadOnlyDictionary<string, ComponentInfo> Components { get; }

    /// <summary>
    /// All content collections, keyed by collection name (e.g., "blog").
    /// </summary>
    internal IReadOnlyDictionary<string, CollectionSchemaInfo> Collections { get; }

    /// <summary>
    /// The base content directory (relative to workspace root), e.g., "src/content".
    /// </summary>
    internal string BaseDirectory { get; }

    /// <summary>
    /// Tries to find the collection schema for a document at the given workspace-relative path.
    /// </summary>
    internal bool TryGetCollectionForPath(string workspaceRelativePath, out CollectionSchemaInfo? schema)
    {
        var normalized = workspaceRelativePath.Replace('\\', '/');
        foreach (var collection in Collections.Values)
        {
            var prefix = collection.Directory.Replace('\\', '/');
            if (!prefix.EndsWith('/'))
            {
                prefix += '/';
            }

            if (normalized.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                schema = collection;
                return true;
            }
        }

        schema = null;
        return false;
    }
}
