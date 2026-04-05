using System.Globalization;
using System.Xml.Linq;
using Atoll.DrawIo.Model;

namespace Atoll.DrawIo.Parsing;

/// <summary>
/// Parses an <c>&lt;mxGraphModel&gt;</c> XML element into a typed <see cref="MxGraphModel"/>.
/// </summary>
internal static class MxGraphModelParser
{
    /// <summary>
    /// Parses the given <c>&lt;mxGraphModel&gt;</c> XML string into an <see cref="MxGraphModel"/>.
    /// </summary>
    /// <param name="xml">The XML string of the mxGraphModel element.</param>
    /// <returns>The parsed model.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the XML is not a valid mxGraphModel.</exception>
    internal static MxGraphModel Parse(string xml)
    {
        ArgumentNullException.ThrowIfNull(xml);

        XElement root;
        try
        {
            root = XElement.Parse(xml);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to parse mxGraphModel XML.", ex);
        }

        if (root.Name.LocalName != "mxGraphModel")
        {
            // If the XML wraps mxGraphModel inside another element, unwrap
            var inner = root.Element("mxGraphModel");
            if (inner != null)
            {
                root = inner;
            }
        }

        var rootElement = root.Element("root");
        if (rootElement == null)
        {
            return new MxGraphModel([]);
        }

        var cells = new List<MxCell>();
        foreach (var cellElement in rootElement.Elements("mxCell"))
        {
            cells.Add(ParseCell(cellElement));
        }

        return new MxGraphModel(cells);
    }

    private static MxCell ParseCell(XElement element)
    {
        var cell = new MxCell
        {
            Id = (string?)element.Attribute("id") ?? string.Empty,
            ParentId = (string?)element.Attribute("parent") ?? string.Empty,
            Value = (string?)element.Attribute("value") ?? string.Empty,
            StyleString = (string?)element.Attribute("style") ?? string.Empty,
            IsVertex = (string?)element.Attribute("vertex") == "1",
            IsEdge = (string?)element.Attribute("edge") == "1",
            Source = (string?)element.Attribute("source") ?? string.Empty,
            Target = (string?)element.Attribute("target") ?? string.Empty,
        };

        var geoElement = element.Element("mxGeometry");
        if (geoElement != null)
        {
            cell.Geometry = ParseGeometry(geoElement);
        }

        return cell;
    }

    private static MxGeometry ParseGeometry(XElement element)
    {
        var geo = new MxGeometry
        {
            X = ParseDouble((string?)element.Attribute("x")),
            Y = ParseDouble((string?)element.Attribute("y")),
            Width = ParseDouble((string?)element.Attribute("width")),
            Height = ParseDouble((string?)element.Attribute("height")),
            Relative = (string?)element.Attribute("relative") == "1",
        };

        // Parse waypoints from Array elements
        var points = new List<MxPoint>();
        foreach (var arrayElement in element.Elements("Array"))
        {
            if ((string?)arrayElement.Attribute("as") == "points")
            {
                foreach (var pointElement in arrayElement.Elements("mxPoint"))
                {
                    points.Add(ParsePoint(pointElement));
                }
            }
        }
        geo.Points = points;

        // Parse source/target point overrides
        foreach (var pointElement in element.Elements("mxPoint"))
        {
            var asAttr = (string?)pointElement.Attribute("as");
            if (asAttr == "sourcePoint")
            {
                geo.SourcePoint = ParsePoint(pointElement);
            }
            else if (asAttr == "targetPoint")
            {
                geo.TargetPoint = ParsePoint(pointElement);
            }
        }

        return geo;
    }

    private static MxPoint ParsePoint(XElement element)
    {
        return new MxPoint(
            ParseDouble((string?)element.Attribute("x")),
            ParseDouble((string?)element.Attribute("y")));
    }

    private static double ParseDouble(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return 0.0;
        }

        return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result)
            ? result
            : 0.0;
    }
}
