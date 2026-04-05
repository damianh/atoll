namespace Atoll.DrawIo.Model;

/// <summary>
/// Represents a single cell in an mxGraph model. A cell may be a vertex (shape),
/// an edge (connector), or a layer (grouping container).
/// </summary>
public sealed class MxCell
{
    /// <summary>Gets or sets the unique identifier of this cell within the diagram.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ID of the parent cell. Cells with parent ID "0" are top-level layers.
    /// Cells with parent ID "1" are in the default layer.
    /// </summary>
    public string ParentId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display value (label text) of this cell.
    /// May contain HTML if <c>html=1</c> is in the style string.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>Gets or sets the raw mxGraph style string for this cell.</summary>
    public string StyleString { get; set; } = string.Empty;

    /// <summary>Gets or sets the geometry of this cell. May be <c>null</c> for root cells.</summary>
    public MxGeometry? Geometry { get; set; }

    /// <summary>Gets or sets a value indicating whether this cell is a vertex (shape).</summary>
    public bool IsVertex { get; set; }

    /// <summary>Gets or sets a value indicating whether this cell is an edge (connector).</summary>
    public bool IsEdge { get; set; }

    /// <summary>
    /// Gets or sets the ID of the source vertex cell for edges. May be empty for floating edges.
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ID of the target vertex cell for edges. May be empty for floating edges.
    /// </summary>
    public string Target { get; set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether this cell is a layer (parent is the root cell with ID "0").
    /// </summary>
    public bool IsLayer => ParentId == "0" && !IsVertex && !IsEdge;
}
