namespace Atoll.Lsp.Context;

/// <summary>
/// Metadata about a single property in a frontmatter schema type.
/// </summary>
internal sealed class SchemaPropertyInfo
{
    internal SchemaPropertyInfo(string name, string yamlKey, Type type, bool required)
    {
        Name = name;
        YamlKey = yamlKey;
        Type = type;
        Required = required;
    }

    /// <summary>The C# property name (e.g., "PubDate").</summary>
    internal string Name { get; }

    /// <summary>The YAML key (camelCase, e.g., "pubDate").</summary>
    internal string YamlKey { get; }

    internal Type Type { get; }
    internal bool Required { get; }
}

/// <summary>
/// Metadata about a content collection's frontmatter schema.
/// </summary>
internal sealed class CollectionSchemaInfo
{
    internal CollectionSchemaInfo(string name, string directory, IReadOnlyList<SchemaPropertyInfo> properties)
    {
        Name = name;
        Directory = directory;
        Properties = properties;
    }

    /// <summary>The collection name (e.g., "blog").</summary>
    internal string Name { get; }

    /// <summary>The collection directory path (relative to workspace root), e.g., "src/content/blog".</summary>
    internal string Directory { get; }

    internal IReadOnlyList<SchemaPropertyInfo> Properties { get; }
}
