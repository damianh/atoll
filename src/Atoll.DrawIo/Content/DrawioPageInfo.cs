namespace Atoll.DrawIo.Content;

/// <summary>
/// Represents metadata for a single page within a <c>.drawio</c> diagram file.
/// </summary>
public sealed class DrawioPageInfo
{
    /// <summary>Gets or sets the display name of the page (from the <c>name</c> attribute).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the unique identifier of the page (from the <c>id</c> attribute).</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the names/IDs of the layers defined on this page.</summary>
    public IReadOnlyList<string> Layers { get; set; } = [];
}
