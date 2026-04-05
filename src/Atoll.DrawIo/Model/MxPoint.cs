namespace Atoll.DrawIo.Model;

/// <summary>
/// Represents a point in 2D space used for mxGraph edge waypoints and geometry offsets.
/// </summary>
public sealed class MxPoint
{
    /// <summary>
    /// Initializes a new instance of <see cref="MxPoint"/>.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    public MxPoint(double x, double y)
    {
        X = x;
        Y = y;
    }

    /// <summary>Gets the X coordinate.</summary>
    public double X { get; }

    /// <summary>Gets the Y coordinate.</summary>
    public double Y { get; }
}
