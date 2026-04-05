using System.Xml.Linq;

namespace Atoll.DrawIo.Rendering;

/// <summary>
/// Provides helper methods for constructing SVG <see cref="XElement"/> nodes
/// with correct namespaces and common attribute patterns.
/// </summary>
internal static class SvgElementBuilder
{
    /// <summary>The SVG namespace URI.</summary>
    internal static readonly XNamespace SvgNs = "http://www.w3.org/2000/svg";

    /// <summary>Creates an SVG element in the SVG namespace.</summary>
    internal static XElement Svg(string name, params object?[] content) =>
        new(SvgNs + name, content);

    /// <summary>Builds a CSS <c>fill</c>/<c>stroke</c>/<c>stroke-width</c> style string from common parameters.</summary>
    /// <param name="fill">Fill color, or <c>null</c>/<c>"none"</c> for no fill.</param>
    /// <param name="stroke">Stroke color, or <c>null</c> for default.</param>
    /// <param name="strokeWidth">Stroke width in pixels.</param>
    /// <param name="opacity">Opacity percentage (0–100).</param>
    /// <param name="dashed">Whether to apply a dash pattern.</param>
    /// <returns>A CSS style attribute string.</returns>
    internal static string BuildStyle(
        string? fill,
        string? stroke,
        double strokeWidth,
        int opacity,
        bool dashed)
    {
        var parts = new List<string>();

        var fillValue = string.IsNullOrEmpty(fill) ? "#ffffff" : fill;
        parts.Add($"fill:{fillValue}");

        var strokeValue = string.IsNullOrEmpty(stroke) ? "#000000" : stroke;
        parts.Add($"stroke:{strokeValue}");

        parts.Add(FormattableString.Invariant($"stroke-width:{strokeWidth}"));

        if (opacity < 100)
        {
            parts.Add(FormattableString.Invariant($"opacity:{opacity / 100.0:F2}"));
        }

        if (dashed)
        {
            parts.Add("stroke-dasharray:8 4");
        }

        return string.Join(";", parts);
    }

    /// <summary>Formats a <see cref="double"/> to a compact invariant string for SVG attributes.</summary>
    internal static string F(double value) =>
        value.ToString("G", System.Globalization.CultureInfo.InvariantCulture);
}
