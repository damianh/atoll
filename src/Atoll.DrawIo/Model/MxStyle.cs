using System.Collections.ObjectModel;

namespace Atoll.DrawIo.Model;

/// <summary>
/// Represents the parsed style properties of an mxGraph cell.
/// Style strings use a semicolon-delimited <c>key=value</c> format with an optional leading shape name.
/// </summary>
public sealed class MxStyle
{
    private readonly IReadOnlyDictionary<string, string> _properties;

    /// <summary>
    /// Initializes a new instance of <see cref="MxStyle"/> with the given shape name and property map.
    /// </summary>
    /// <param name="shapeName">The optional leading shape identifier (e.g. <c>"rhombus"</c>, <c>"ellipse"</c>).</param>
    /// <param name="properties">The parsed key-value style properties.</param>
    public MxStyle(string? shapeName, IReadOnlyDictionary<string, string> properties)
    {
        ShapeName = shapeName;
        _properties = properties;
    }

    /// <summary>
    /// Gets the leading shape identifier, if present (e.g. <c>"rhombus"</c>, <c>"ellipse"</c>, <c>"text"</c>).
    /// This is the first token in the style string when it does not contain an <c>=</c> sign.
    /// </summary>
    public string? ShapeName { get; }

    /// <summary>
    /// Gets the effective shape name for rendering. Returns the <c>shape</c> property value if set,
    /// then falls back to <see cref="ShapeName"/>, then returns <c>null</c> (default rectangle).
    /// </summary>
    public string? EffectiveShape => TryGetValue("shape", out var shape) ? shape : ShapeName;

    /// <summary>Gets all raw key-value style properties.</summary>
    public IReadOnlyDictionary<string, string> Properties => _properties;

    /// <summary>
    /// Tries to get the value of a style property by key (case-insensitive).
    /// </summary>
    /// <param name="key">The property key.</param>
    /// <param name="value">The property value, or <c>null</c> if not found.</param>
    /// <returns><c>true</c> if the property exists; otherwise <c>false</c>.</returns>
    public bool TryGetValue(string key, out string value)
    {
        if (_properties.TryGetValue(key, out var v))
        {
            value = v;
            return true;
        }
        value = string.Empty;
        return false;
    }

    /// <summary>Gets the value of a style property by key, or <c>null</c> if not set.</summary>
    public string? this[string key] => _properties.TryGetValue(key, out var v) ? v : null;

    // ── Common style helpers ──────────────────────────────────────────────────

    /// <summary>Gets the fill color (e.g. <c>"#dae8fc"</c>, <c>"none"</c>). Default <c>null</c>.</summary>
    public string? FillColor => this["fillColor"];

    /// <summary>Gets the stroke color. Default <c>null</c>.</summary>
    public string? StrokeColor => this["strokeColor"];

    /// <summary>Gets the stroke width. Default <c>2.0</c> when not set.</summary>
    public double StrokeWidth
    {
        get
        {
            if (TryGetValue("strokeWidth", out var s) && double.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var v))
            {
                return v;
            }
            return 2.0;
        }
    }

    /// <summary>Gets the font size in points. Default <c>11</c> when not set.</summary>
    public double FontSize
    {
        get
        {
            if (TryGetValue("fontSize", out var s) && double.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var v))
            {
                return v;
            }
            return 11.0;
        }
    }

    /// <summary>Gets the font color. Default <c>null</c> (inherits).</summary>
    public string? FontColor => this["fontColor"];

    /// <summary>Gets the font family. Default <c>null</c> (inherits).</summary>
    public string? FontFamily => this["fontFamily"];

    /// <summary>
    /// Gets the font style bitmask. Bit 0 = bold, bit 1 = italic, bit 2 = underline.
    /// </summary>
    public int FontStyle
    {
        get
        {
            if (TryGetValue("fontStyle", out var s) && int.TryParse(s, out var v))
            {
                return v;
            }
            return 0;
        }
    }

    /// <summary>Gets a value indicating whether the font is bold (<c>fontStyle</c> bit 0).</summary>
    public bool IsBold => (FontStyle & 1) != 0;

    /// <summary>Gets a value indicating whether the font is italic (<c>fontStyle</c> bit 1).</summary>
    public bool IsItalic => (FontStyle & 2) != 0;

    /// <summary>Gets a value indicating whether the font has underline (<c>fontStyle</c> bit 2).</summary>
    public bool IsUnderline => (FontStyle & 4) != 0;

    /// <summary>Gets a value indicating whether corners are rounded (<c>rounded=1</c>).</summary>
    public bool Rounded => this["rounded"] == "1";

    /// <summary>Gets a value indicating whether the stroke is dashed (<c>dashed=1</c>).</summary>
    public bool Dashed => this["dashed"] == "1";

    /// <summary>Gets the opacity as a percentage (0–100). Default <c>100</c>.</summary>
    public int Opacity
    {
        get
        {
            if (TryGetValue("opacity", out var s) && int.TryParse(s, out var v))
            {
                return v;
            }
            return 100;
        }
    }

    /// <summary>Gets the horizontal text alignment (<c>left</c>, <c>center</c>, <c>right</c>). Default <c>"center"</c>.</summary>
    public string Align => this["align"] ?? "center";

    /// <summary>Gets the vertical text alignment (<c>top</c>, <c>middle</c>, <c>bottom</c>). Default <c>"middle"</c>.</summary>
    public string VerticalAlign => this["verticalAlign"] ?? "middle";

    /// <summary>Gets whether the label contains HTML (<c>html=1</c>).</summary>
    public bool IsHtml => this["html"] == "1";

    /// <summary>Gets the start arrow style (e.g. <c>"classic"</c>, <c>"open"</c>, <c>"none"</c>).</summary>
    public string? StartArrow => this["startArrow"];

    /// <summary>Gets the end arrow style (e.g. <c>"classic"</c>, <c>"open"</c>, <c>"none"</c>).</summary>
    public string? EndArrow => this["endArrow"];

    /// <summary>Gets the edge routing style (e.g. <c>"orthogonalEdgeStyle"</c>).</summary>
    public string? EdgeStyle => this["edgeStyle"];

    /// <summary>Returns an empty <see cref="MxStyle"/> with no properties.</summary>
    public static MxStyle Empty { get; } = new MxStyle(null, new ReadOnlyDictionary<string, string>(new Dictionary<string, string>()));
}
