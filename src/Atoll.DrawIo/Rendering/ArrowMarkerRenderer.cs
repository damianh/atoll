using System.Xml.Linq;

namespace Atoll.DrawIo.Rendering;

/// <summary>
/// Generates SVG <c>&lt;marker&gt;</c> elements for mxGraph arrow styles.
/// Markers are placed in the <c>&lt;defs&gt;</c> section of the SVG document.
/// </summary>
internal static class ArrowMarkerRenderer
{
    /// <summary>
    /// Returns an SVG <c>&lt;marker&gt;</c> element for the given arrow style name.
    /// Returns <c>null</c> for the <c>"none"</c> style or unrecognised names.
    /// </summary>
    /// <param name="id">The unique marker ID to use as the <c>id</c> attribute.</param>
    /// <param name="arrowStyle">
    /// The mxGraph arrow style keyword (e.g. <c>"classic"</c>, <c>"open"</c>,
    /// <c>"block"</c>, <c>"oval"</c>, <c>"diamond"</c>).
    /// </param>
    /// <param name="color">The fill/stroke color for the marker.</param>
    internal static XElement? CreateMarker(string id, string? arrowStyle, string color)
    {
        if (string.IsNullOrEmpty(arrowStyle) || arrowStyle.Equals("none", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return arrowStyle.ToLowerInvariant() switch
        {
            "classic" or "block" => CreateBlockArrow(id, color, filled: true),
            "open"               => CreateOpenArrow(id, color),
            "oval"               => CreateOvalArrow(id, color),
            "diamond"            => CreateDiamondArrow(id, color),
            _                    => CreateBlockArrow(id, color, filled: true),
        };
    }

    // ── Marker shapes ─────────────────────────────────────────────────────────

    private static XElement CreateBlockArrow(string id, string color, bool filled)
    {
        var fillColor = filled ? color : "none";
        return SvgElementBuilder.Svg("marker",
            new XAttribute("id", id),
            new XAttribute("viewBox", "0 0 10 10"),
            new XAttribute("refX", "9"),
            new XAttribute("refY", "5"),
            new XAttribute("markerWidth", "6"),
            new XAttribute("markerHeight", "6"),
            new XAttribute("orient", "auto"),
            SvgElementBuilder.Svg("path",
                new XAttribute("d", "M 0 0 L 10 5 L 0 10 z"),
                new XAttribute("style", $"fill:{fillColor};stroke:{color}")));
    }

    private static XElement CreateOpenArrow(string id, string color)
    {
        return SvgElementBuilder.Svg("marker",
            new XAttribute("id", id),
            new XAttribute("viewBox", "0 0 10 10"),
            new XAttribute("refX", "9"),
            new XAttribute("refY", "5"),
            new XAttribute("markerWidth", "6"),
            new XAttribute("markerHeight", "6"),
            new XAttribute("orient", "auto"),
            SvgElementBuilder.Svg("path",
                new XAttribute("d", "M 0 0 L 10 5 L 0 10"),
                new XAttribute("style", $"fill:none;stroke:{color};stroke-width:2")));
    }

    private static XElement CreateOvalArrow(string id, string color)
    {
        return SvgElementBuilder.Svg("marker",
            new XAttribute("id", id),
            new XAttribute("viewBox", "0 0 10 10"),
            new XAttribute("refX", "5"),
            new XAttribute("refY", "5"),
            new XAttribute("markerWidth", "6"),
            new XAttribute("markerHeight", "6"),
            new XAttribute("orient", "auto"),
            SvgElementBuilder.Svg("circle",
                new XAttribute("cx", "5"),
                new XAttribute("cy", "5"),
                new XAttribute("r", "4"),
                new XAttribute("style", $"fill:{color};stroke:{color}")));
    }

    private static XElement CreateDiamondArrow(string id, string color)
    {
        return SvgElementBuilder.Svg("marker",
            new XAttribute("id", id),
            new XAttribute("viewBox", "0 0 10 10"),
            new XAttribute("refX", "5"),
            new XAttribute("refY", "5"),
            new XAttribute("markerWidth", "6"),
            new XAttribute("markerHeight", "6"),
            new XAttribute("orient", "auto"),
            SvgElementBuilder.Svg("path",
                new XAttribute("d", "M 0 5 L 5 0 L 10 5 L 5 10 z"),
                new XAttribute("style", $"fill:{color};stroke:{color}")));
    }
}
