namespace Atoll.DrawIo.Model;

/// <summary>
/// Represents the geometry of an mxGraph cell — position, size, and optional edge waypoints.
/// </summary>
public sealed class MxGeometry
{
    /// <summary>Gets or sets the X coordinate (left edge for vertices).</summary>
    public double X { get; set; }

    /// <summary>Gets or sets the Y coordinate (top edge for vertices).</summary>
    public double Y { get; set; }

    /// <summary>Gets or sets the width of the cell (vertices only).</summary>
    public double Width { get; set; }

    /// <summary>Gets or sets the height of the cell (vertices only).</summary>
    public double Height { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the geometry is relative to the parent cell.
    /// Typically <c>true</c> for edge labels.
    /// </summary>
    public bool Relative { get; set; }

    /// <summary>
    /// Gets or sets the list of waypoints for edge cells. May be empty.
    /// </summary>
    public IReadOnlyList<MxPoint> Points { get; set; } = [];

    /// <summary>
    /// Gets or sets the source point override for an edge (when no source cell is connected).
    /// </summary>
    public MxPoint? SourcePoint { get; set; }

    /// <summary>
    /// Gets or sets the target point override for an edge (when no target cell is connected).
    /// </summary>
    public MxPoint? TargetPoint { get; set; }
}
