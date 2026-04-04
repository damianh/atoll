namespace Atoll.Lagoon.Components;

/// <summary>
/// Represents a file or directory node in a <see cref="FileTree"/> component.
/// </summary>
public sealed class FileTreeItem
{
    /// <summary>Initializes a file item.</summary>
    public FileTreeItem(string name, bool isHighlighted = false)
    {
        Name = name;
        IsDirectory = false;
        IsHighlighted = isHighlighted;
        Children = null;
    }

    /// <summary>Initializes a directory item with optional children.</summary>
    public FileTreeItem(string name, bool isDirectory, IReadOnlyList<FileTreeItem>? children = null, bool isHighlighted = false)
    {
        Name = name;
        IsDirectory = isDirectory;
        Children = children;
        IsHighlighted = isHighlighted;
    }

    /// <summary>Gets the display name of the file or directory.</summary>
    public string Name { get; }

    /// <summary>Gets a value indicating whether this item is a directory.</summary>
    public bool IsDirectory { get; }

    /// <summary>Gets the child items for a directory. <c>null</c> for files.</summary>
    public IReadOnlyList<FileTreeItem>? Children { get; }

    /// <summary>Gets a value indicating whether this item should be visually highlighted.</summary>
    public bool IsHighlighted { get; }
}
