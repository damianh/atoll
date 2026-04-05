namespace Atoll.DrawIo.Model;

/// <summary>
/// Represents a parsed mxGraph model — the root of a draw.io diagram page's content.
/// Contains all cells organized by layer.
/// </summary>
public sealed class MxGraphModel
{
    /// <summary>
    /// Initializes a new instance of <see cref="MxGraphModel"/>.
    /// </summary>
    /// <param name="cells">All cells in the model, including root, layer, vertex, and edge cells.</param>
    public MxGraphModel(IReadOnlyList<MxCell> cells)
    {
        ArgumentNullException.ThrowIfNull(cells);
        Cells = cells;
    }

    /// <summary>
    /// Gets all cells in the model (including root cell id=0 and default layer id=1).
    /// </summary>
    public IReadOnlyList<MxCell> Cells { get; }

    /// <summary>
    /// Gets the layer cells (cells whose parent is the root cell, id=0).
    /// Layers are containers that group cells for visibility toggling.
    /// </summary>
    public IReadOnlyList<MxCell> Layers => Cells.Where(c => c.IsLayer).ToList();

    /// <summary>
    /// Gets the vertex cells (shapes) in the model.
    /// </summary>
    public IReadOnlyList<MxCell> Vertices => Cells.Where(c => c.IsVertex).ToList();

    /// <summary>
    /// Gets the edge cells (connectors) in the model.
    /// </summary>
    public IReadOnlyList<MxCell> Edges => Cells.Where(c => c.IsEdge).ToList();

    /// <summary>
    /// Gets a cell by its ID, or <c>null</c> if not found.
    /// </summary>
    /// <param name="id">The cell ID to look up.</param>
    /// <returns>The matching cell, or <c>null</c>.</returns>
    public MxCell? GetCellById(string id)
    {
        ArgumentNullException.ThrowIfNull(id);
        return Cells.FirstOrDefault(c => c.Id == id);
    }

    /// <summary>
    /// Gets all cells whose parent is the specified cell ID.
    /// </summary>
    /// <param name="parentId">The parent cell ID.</param>
    /// <returns>All direct children of the specified cell.</returns>
    public IReadOnlyList<MxCell> GetChildren(string parentId)
    {
        ArgumentNullException.ThrowIfNull(parentId);
        return Cells.Where(c => c.ParentId == parentId).ToList();
    }
}
