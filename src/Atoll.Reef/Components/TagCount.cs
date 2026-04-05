namespace Atoll.Reef.Components;

/// <summary>
/// Represents a tag with its associated article count.
/// </summary>
public sealed class TagCount
{
    /// <summary>Initialises a new <see cref="TagCount"/> with the given name and count.</summary>
    public TagCount(string name, int count)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
        Count = count;
    }

    /// <summary>Gets the tag's display name.</summary>
    public string Name { get; }

    /// <summary>Gets the URL-safe slug (lowercase) derived from <see cref="Name"/>.</summary>
    public string Slug => Name.ToLowerInvariant();

    /// <summary>Gets the number of articles that carry this tag.</summary>
    public int Count { get; }
}
