using System.Text;
using System.Xml.Linq;
using Atoll.DrawIo.Model;
using Atoll.DrawIo.Parsing;

namespace Atoll.DrawIo.Rendering;

/// <summary>
/// Renders mxGraph edge cells as SVG <c>&lt;path&gt;</c> elements with optional arrow markers.
/// </summary>
internal static class EdgeRenderer
{
    /// <summary>
    /// Renders an edge cell as a collection of SVG elements:
    /// one <c>&lt;path&gt;</c> for the connector line, plus an optional <c>&lt;text&gt;</c> for edge labels.
    /// Marker IDs (for arrowheads) must already be defined in the SVG <c>&lt;defs&gt;</c>.
    /// </summary>
    /// <param name="cell">The edge cell to render.</param>
    /// <param name="cellLookup">A function to look up a cell by its ID (for resolving source/target geometry).</param>
    /// <returns>SVG elements for the edge. May be empty if geometry cannot be determined.</returns>
    internal static IEnumerable<XElement> Render(MxCell cell, Func<string, MxCell?> cellLookup)
    {
        var style = MxStyleParser.Parse(cell.StyleString);
        var strokeColor = string.IsNullOrEmpty(style.StrokeColor) ? "#000000" : style.StrokeColor;

        var (start, end) = ResolveEndpoints(cell, cellLookup);
        if (start == null || end == null)
        {
            yield break;
        }

        var pathData = BuildPathData(cell, start.Value, end.Value);

        var pathStyle = SvgElementBuilder.BuildStyle(
            "none",
            strokeColor,
            style.StrokeWidth,
            style.Opacity,
            style.Dashed);

        var pathElement = SvgElementBuilder.Svg("path",
            new XAttribute("d", pathData),
            new XAttribute("style", pathStyle),
            new XAttribute("fill", "none"));

        // Add marker-end/start attributes
        var endArrow   = style.EndArrow   ?? "classic";
        var startArrow = style.StartArrow ?? "none";

        if (!string.Equals(endArrow, "none", StringComparison.OrdinalIgnoreCase))
        {
            var markerId = MarkerIdFor(cell.Id, "end", endArrow);
            pathElement.Add(new XAttribute("marker-end", $"url(#{markerId})"));
        }

        if (!string.IsNullOrEmpty(startArrow) &&
            !string.Equals(startArrow, "none", StringComparison.OrdinalIgnoreCase))
        {
            var markerId = MarkerIdFor(cell.Id, "start", startArrow);
            pathElement.Add(new XAttribute("marker-start", $"url(#{markerId})"));
        }

        yield return pathElement;

        // Edge label
        if (!string.IsNullOrEmpty(cell.Value))
        {
            var midX = (start.Value.X + end.Value.X) / 2;
            var midY = (start.Value.Y + end.Value.Y) / 2;
            const double labelW = 100;
            const double labelH = 20;
            var labelEl = TextRenderer.Render(cell, midX - labelW / 2, midY - labelH / 2, labelW, labelH);
            if (labelEl != null)
            {
                yield return labelEl;
            }
        }
    }

    /// <summary>
    /// Collects all arrow marker definitions required for a set of edge cells.
    /// Returns <see cref="XElement"/> instances suitable for placement in SVG <c>&lt;defs&gt;</c>.
    /// </summary>
    internal static IEnumerable<XElement> CollectMarkers(IEnumerable<MxCell> edges)
    {
        foreach (var cell in edges)
        {
            var style = MxStyleParser.Parse(cell.StyleString);
            var strokeColor = string.IsNullOrEmpty(style.StrokeColor) ? "#000000" : style.StrokeColor;

            var endArrow   = style.EndArrow   ?? "classic";
            var startArrow = style.StartArrow ?? "none";

            if (!string.Equals(endArrow, "none", StringComparison.OrdinalIgnoreCase))
            {
                var marker = ArrowMarkerRenderer.CreateMarker(
                    MarkerIdFor(cell.Id, "end", endArrow), endArrow, strokeColor);
                if (marker != null)
                {
                    yield return marker;
                }
            }

            if (!string.IsNullOrEmpty(startArrow) &&
                !string.Equals(startArrow, "none", StringComparison.OrdinalIgnoreCase))
            {
                var marker = ArrowMarkerRenderer.CreateMarker(
                    MarkerIdFor(cell.Id, "start", startArrow), startArrow, strokeColor);
                if (marker != null)
                {
                    yield return marker;
                }
            }
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Generates a stable marker element ID for a given edge/direction/style.</summary>
    private static string MarkerIdFor(string cellId, string direction, string style) =>
        $"marker-{cellId}-{direction}-{style}";

    /// <summary>
    /// Resolves the start and end points for an edge, using source/target cell geometry
    /// if available, or the edge geometry's own source/target points.
    /// </summary>
    private static ((double X, double Y)? start, (double X, double Y)? end)
        ResolveEndpoints(MxCell cell, Func<string, MxCell?> lookup)
    {
        (double X, double Y)? start = null;
        (double X, double Y)? end   = null;

        // Resolve start point
        if (!string.IsNullOrEmpty(cell.Source))
        {
            var src = lookup(cell.Source);
            if (src?.Geometry != null)
            {
                start = Center(src.Geometry);
            }
        }

        if (start == null && cell.Geometry?.SourcePoint != null)
        {
            start = (cell.Geometry.SourcePoint.X, cell.Geometry.SourcePoint.Y);
        }

        // Resolve end point
        if (!string.IsNullOrEmpty(cell.Target))
        {
            var tgt = lookup(cell.Target);
            if (tgt?.Geometry != null)
            {
                end = Center(tgt.Geometry);
            }
        }

        if (end == null && cell.Geometry?.TargetPoint != null)
        {
            end = (cell.Geometry.TargetPoint.X, cell.Geometry.TargetPoint.Y);
        }

        return (start, end);
    }

    /// <summary>Returns the center point of a geometry rectangle.</summary>
    private static (double X, double Y) Center(MxGeometry geo) =>
        (geo.X + geo.Width / 2, geo.Y + geo.Height / 2);

    /// <summary>
    /// Builds an SVG path data string from the edge's start, waypoints, and end.
    /// Uses straight line segments; orthogonal routing is approximated via waypoints.
    /// </summary>
    private static string BuildPathData(MxCell cell, (double X, double Y) start, (double X, double Y) end)
    {
        var sb = new StringBuilder();
        sb.Append(FormattableString.Invariant($"M {start.X} {start.Y}"));

        var waypoints = cell.Geometry?.Points ?? [];
        foreach (var pt in waypoints)
        {
            sb.Append(FormattableString.Invariant($" L {pt.X} {pt.Y}"));
        }

        sb.Append(FormattableString.Invariant($" L {end.X} {end.Y}"));
        return sb.ToString();
    }
}
