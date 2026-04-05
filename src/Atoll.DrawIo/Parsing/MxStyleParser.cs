using System.Collections.ObjectModel;
using Atoll.DrawIo.Model;

namespace Atoll.DrawIo.Parsing;

/// <summary>
/// Parses mxGraph style strings into <see cref="MxStyle"/> objects.
/// </summary>
/// <remarks>
/// Style strings use a semicolon-delimited <c>key=value</c> grammar with an optional
/// leading shape name:
/// <code>
/// style = [shape_name ";"] { key "=" value ";" }
/// </code>
/// Examples:
/// <list type="bullet">
///   <item><description><c>"rounded=1;whiteSpace=wrap;html=1;"</c></description></item>
///   <item><description><c>"rhombus;fillColor=#fff2cc;strokeColor=#d6b656;"</c></description></item>
///   <item><description><c>"edgeStyle=orthogonalEdgeStyle;"</c></description></item>
/// </list>
/// </remarks>
public static class MxStyleParser
{
    /// <summary>
    /// Parses the given style string into an <see cref="MxStyle"/>.
    /// </summary>
    /// <param name="styleString">
    /// The raw mxGraph style string. May be <c>null</c> or empty, in which case
    /// <see cref="MxStyle.Empty"/> is returned.
    /// </param>
    /// <returns>A parsed <see cref="MxStyle"/> instance.</returns>
    public static MxStyle Parse(string? styleString)
    {
        if (string.IsNullOrWhiteSpace(styleString))
        {
            return MxStyle.Empty;
        }

        var tokens = styleString.Split(';', StringSplitOptions.RemoveEmptyEntries);
        string? shapeName = null;
        var properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < tokens.Length; i++)
        {
            var token = tokens[i].Trim();
            if (string.IsNullOrEmpty(token))
            {
                continue;
            }

            var eqIndex = token.IndexOf('=', StringComparison.Ordinal);
            if (eqIndex < 0)
            {
                // No '=' → this is the leading shape name (only valid as the first token)
                if (i == 0)
                {
                    shapeName = token;
                }
                // Tokens without '=' that are not the first are silently ignored
            }
            else
            {
                var key = token[..eqIndex].Trim();
                var value = token[(eqIndex + 1)..].Trim();
                if (!string.IsNullOrEmpty(key))
                {
                    properties[key] = value;
                }
            }
        }

        return new MxStyle(shapeName, new ReadOnlyDictionary<string, string>(properties));
    }
}
