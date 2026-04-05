using System.Xml.Linq;
using Atoll.DrawIo.Model;
using Atoll.DrawIo.Parsing;

namespace Atoll.DrawIo.Rendering;

/// <summary>
/// Renders mxGraph vertex cells as SVG shape elements.
/// Shapes are dispatched through a registry of named renderers.
/// </summary>
internal static class ShapeRenderer
{
    // ── Shape renderer registry ───────────────────────────────────────────────

    private static readonly Dictionary<string, Func<MxCell, MxStyle, XElement?>> Registry =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["ellipse"]   = RenderEllipse,
            ["rhombus"]   = RenderRhombus,
            ["diamond"]   = RenderRhombus,
            ["triangle"]  = RenderTriangle,
            ["cylinder"]  = RenderCylinder,
            ["cloud"]     = RenderCloud,
            ["text"]      = RenderText,
            ["image"]     = RenderImage,
        };

    /// <summary>
    /// Renders a vertex cell into an SVG element.
    /// Returns <c>null</c> for cells without geometry.
    /// </summary>
    internal static XElement? Render(MxCell cell)
    {
        if (cell.Geometry == null)
        {
            return null;
        }

        var style = MxStyleParser.Parse(cell.StyleString);
        var shapeName = (style.EffectiveShape ?? string.Empty).ToLowerInvariant();

        if (Registry.TryGetValue(shapeName, out var renderer))
        {
            return renderer(cell, style);
        }

        // Default: rectangle
        return RenderRectangle(cell, style);
    }

    // ── Individual shape renderers ────────────────────────────────────────────

    private static XElement RenderRectangle(MxCell cell, MxStyle style)
    {
        var geo = cell.Geometry!;
        var radius = style.Rounded ? "10" : "0";
        return SvgElementBuilder.Svg("rect",
            new XAttribute("x", SvgElementBuilder.F(geo.X)),
            new XAttribute("y", SvgElementBuilder.F(geo.Y)),
            new XAttribute("width", SvgElementBuilder.F(geo.Width)),
            new XAttribute("height", SvgElementBuilder.F(geo.Height)),
            new XAttribute("rx", radius),
            new XAttribute("ry", radius),
            new XAttribute("style", BuildShapeStyle(style)));
    }

    private static XElement RenderEllipse(MxCell cell, MxStyle style)
    {
        var geo = cell.Geometry!;
        var cx = geo.X + geo.Width / 2;
        var cy = geo.Y + geo.Height / 2;
        return SvgElementBuilder.Svg("ellipse",
            new XAttribute("cx", SvgElementBuilder.F(cx)),
            new XAttribute("cy", SvgElementBuilder.F(cy)),
            new XAttribute("rx", SvgElementBuilder.F(geo.Width / 2)),
            new XAttribute("ry", SvgElementBuilder.F(geo.Height / 2)),
            new XAttribute("style", BuildShapeStyle(style)));
    }

    private static XElement RenderRhombus(MxCell cell, MxStyle style)
    {
        var geo = cell.Geometry!;
        var x = geo.X;
        var y = geo.Y;
        var w = geo.Width;
        var h = geo.Height;
        // Diamond points: top, right, bottom, left
        var points = FormattableString.Invariant(
            $"{x + w / 2},{y} {x + w},{y + h / 2} {x + w / 2},{y + h} {x},{y + h / 2}");
        return SvgElementBuilder.Svg("polygon",
            new XAttribute("points", points),
            new XAttribute("style", BuildShapeStyle(style)));
    }

    private static XElement RenderTriangle(MxCell cell, MxStyle style)
    {
        var geo = cell.Geometry!;
        var x = geo.X;
        var y = geo.Y;
        var w = geo.Width;
        var h = geo.Height;
        var points = FormattableString.Invariant(
            $"{x},{y + h} {x + w / 2},{y} {x + w},{y + h}");
        return SvgElementBuilder.Svg("polygon",
            new XAttribute("points", points),
            new XAttribute("style", BuildShapeStyle(style)));
    }

    private static XElement RenderCylinder(MxCell cell, MxStyle style)
    {
        var geo = cell.Geometry!;
        var x = geo.X;
        var y = geo.Y;
        var w = geo.Width;
        var h = geo.Height;
        var ry = h * 0.12; // ellipse height for top/bottom caps

        // Build a path: top ellipse arc + sides + bottom ellipse arc
        var path = string.Create(System.Globalization.CultureInfo.InvariantCulture,
            $"M {x},{y + ry} a {w / 2},{ry} 0 1,0 {w},0 a {w / 2},{ry} 0 1,0 -{w},0 v {h - ry} a {w / 2},{ry} 0 1,0 {w},0 V {y + ry}");

        return SvgElementBuilder.Svg("path",
            new XAttribute("d", path),
            new XAttribute("style", BuildShapeStyle(style)));
    }

    private static XElement RenderCloud(MxCell cell, MxStyle style)
    {
        var geo = cell.Geometry!;
        var x = geo.X;
        var y = geo.Y;
        var w = geo.Width;
        var h = geo.Height;

        // Simplified cloud path using multiple arcs
        var path = string.Create(System.Globalization.CultureInfo.InvariantCulture,
            $"M {x + w * 0.25},{y + h * 0.8} a {w * 0.15},{h * 0.2} 0 0,1 {-w * 0.05},{-h * 0.25} a {w * 0.15},{h * 0.25} 0 0,1 {w * 0.15},{-h * 0.25} a {w * 0.2},{h * 0.3} 0 0,1 {w * 0.2},{-h * 0.1} a {w * 0.2},{h * 0.35} 0 0,1 {w * 0.3},{h * 0.05} a {w * 0.15},{h * 0.2} 0 0,1 {w * 0.05},{h * 0.25} Z");

        return SvgElementBuilder.Svg("path",
            new XAttribute("d", path),
            new XAttribute("style", BuildShapeStyle(style)));
    }

    private static XElement? RenderText(MxCell cell, MxStyle style)
    {
        // Text shapes have no visible border; just return a transparent rect as a container.
        // The TextRenderer handles the actual <text> element.
        var geo = cell.Geometry;
        if (geo == null)
        {
            return null;
        }

        return SvgElementBuilder.Svg("rect",
            new XAttribute("x", SvgElementBuilder.F(geo.X)),
            new XAttribute("y", SvgElementBuilder.F(geo.Y)),
            new XAttribute("width", SvgElementBuilder.F(geo.Width)),
            new XAttribute("height", SvgElementBuilder.F(geo.Height)),
            new XAttribute("style", "fill:none;stroke:none"));
    }

    private static XElement? RenderImage(MxCell cell, MxStyle style)
    {
        var geo = cell.Geometry;
        if (geo == null)
        {
            return null;
        }

        // Extract image href from style (image;image=data:...; or image=url)
        var href = style["image"] ?? string.Empty;

        return SvgElementBuilder.Svg("image",
            new XAttribute("x", SvgElementBuilder.F(geo.X)),
            new XAttribute("y", SvgElementBuilder.F(geo.Y)),
            new XAttribute("width", SvgElementBuilder.F(geo.Width)),
            new XAttribute("height", SvgElementBuilder.F(geo.Height)),
            new XAttribute("{http://www.w3.org/1999/xlink}href", href),
            new XAttribute("preserveAspectRatio", "xMidYMid meet"));
    }

    // ── Style helpers ─────────────────────────────────────────────────────────

    private static string BuildShapeStyle(MxStyle style)
    {
        return SvgElementBuilder.BuildStyle(
            style.FillColor,
            style.StrokeColor,
            style.StrokeWidth,
            style.Opacity,
            style.Dashed);
    }
}
