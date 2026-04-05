using Atoll.DrawIo.Model;

namespace Atoll.DrawIo.Rendering;

/// <summary>
/// Computes the bounding box for a collection of mxGraph cells.
/// </summary>
internal static class BoundsCalculator
{
    /// <summary>
    /// Computes the axis-aligned bounding box (AABB) for all visible vertex cells
    /// in the given collection. Returns a zero-sized box at the origin if no
    /// geometry is found.
    /// </summary>
    /// <param name="cells">The cells to include in the bounding box calculation.</param>
    /// <param name="padding">Extra padding to add on each side.</param>
    /// <returns>A <see cref="BoundingBox"/> encompassing all cell geometries.</returns>
    internal static BoundingBox Compute(IEnumerable<MxCell> cells, double padding = 0)
    {
        double minX = double.MaxValue;
        double minY = double.MaxValue;
        double maxX = double.MinValue;
        double maxY = double.MinValue;
        var found = false;

        foreach (var cell in cells)
        {
            if (cell.Geometry == null || (!cell.IsVertex && !cell.IsEdge))
            {
                continue;
            }

            if (cell.IsVertex)
            {
                var geo = cell.Geometry;
                minX = Math.Min(minX, geo.X);
                minY = Math.Min(minY, geo.Y);
                maxX = Math.Max(maxX, geo.X + geo.Width);
                maxY = Math.Max(maxY, geo.Y + geo.Height);
                found = true;
            }

            // Include edge waypoints and source/target points in bounds
            if (cell.IsEdge && cell.Geometry != null)
            {
                foreach (var pt in cell.Geometry.Points)
                {
                    minX = Math.Min(minX, pt.X);
                    minY = Math.Min(minY, pt.Y);
                    maxX = Math.Max(maxX, pt.X);
                    maxY = Math.Max(maxY, pt.Y);
                    found = true;
                }

                if (cell.Geometry.SourcePoint != null)
                {
                    minX = Math.Min(minX, cell.Geometry.SourcePoint.X);
                    minY = Math.Min(minY, cell.Geometry.SourcePoint.Y);
                    maxX = Math.Max(maxX, cell.Geometry.SourcePoint.X);
                    maxY = Math.Max(maxY, cell.Geometry.SourcePoint.Y);
                    found = true;
                }

                if (cell.Geometry.TargetPoint != null)
                {
                    minX = Math.Min(minX, cell.Geometry.TargetPoint.X);
                    minY = Math.Min(minY, cell.Geometry.TargetPoint.Y);
                    maxX = Math.Max(maxX, cell.Geometry.TargetPoint.X);
                    maxY = Math.Max(maxY, cell.Geometry.TargetPoint.Y);
                    found = true;
                }
            }
        }

        if (!found)
        {
            return new BoundingBox(0, 0, 0, 0);
        }

        return new BoundingBox(
            minX - padding,
            minY - padding,
            maxX - minX + padding * 2,
            maxY - minY + padding * 2);
    }
}

/// <summary>
/// Represents an axis-aligned bounding box.
/// </summary>
/// <param name="X">The left edge X coordinate.</param>
/// <param name="Y">The top edge Y coordinate.</param>
/// <param name="Width">The total width.</param>
/// <param name="Height">The total height.</param>
internal sealed record BoundingBox(double X, double Y, double Width, double Height);
