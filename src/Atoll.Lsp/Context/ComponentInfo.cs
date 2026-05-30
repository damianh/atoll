namespace Atoll.Lsp.Context;

/// <summary>
/// Metadata about a single component parameter discovered via reflection.
/// </summary>
internal sealed class ComponentParameterInfo
{
    internal ComponentParameterInfo(string name, Type type, bool required)
    {
        Name = name;
        Type = type;
        Required = required;
    }

    internal string Name { get; }
    internal Type Type { get; }
    internal bool Required { get; }
}

/// <summary>
/// Metadata about a registered Atoll component, including its parameters.
/// </summary>
internal sealed class ComponentInfo
{
    internal ComponentInfo(string name, string typeName, string? fullTypeName, IReadOnlyList<ComponentParameterInfo> parameters)
    {
        Name = name;
        TypeName = typeName;
        FullTypeName = fullTypeName;
        Parameters = parameters;
    }

    /// <summary>The directive/tag name (e.g., "aside", "card-grid").</summary>
    internal string Name { get; }

    /// <summary>The simple C# type name (e.g., "Aside").</summary>
    internal string TypeName { get; }

    /// <summary>The fully-qualified C# type name (e.g., "Atoll.Lagoon.Components.Aside").</summary>
    internal string? FullTypeName { get; }

    internal IReadOnlyList<ComponentParameterInfo> Parameters { get; }
}
