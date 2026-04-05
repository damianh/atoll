using System.Text.RegularExpressions;
using System.Xml.Linq;
using Atoll.DrawIo.Model;
using Atoll.DrawIo.Parsing;

namespace Atoll.DrawIo.Rendering;

/// <summary>
/// Renders cell label text as SVG <c>&lt;text&gt;</c> elements,
/// positioned and styled according to the cell's mxGraph style properties.
/// </summary>
internal static partial class TextRenderer
{
    // Approximate character width factor relative to font size
    private const double CharWidthFactor = 0.6;

    /// <summary>
    /// Renders the label of a vertex or edge cell as an SVG <c>&lt;text&gt;</c> element.
    /// Returns <c>null</c> when the cell has no label or no geometry.
    /// </summary>
    internal static XElement? Render(MxCell cell, double x, double y, double width, double height)
    {
        var label = cell.Value;
        if (string.IsNullOrEmpty(label))
        {
            return null;
        }

        var style = MxStyleParser.Parse(cell.StyleString);

        // Strip HTML tags if label contains HTML
        var plainText = style.IsHtml ? StripHtml(label) : label;
        if (string.IsNullOrWhiteSpace(plainText))
        {
            return null;
        }

        var lines = WrapText(plainText, style.FontSize, width);

        // Compute text-anchor from align
        var textAnchor = style.Align switch
        {
            "left"  => "start",
            "right" => "end",
            _       => "middle",
        };

        // Compute x position
        var textX = style.Align switch
        {
            "left"  => x + 4,
            "right" => x + width - 4,
            _       => x + width / 2,
        };

        // Compute y start position from verticalAlign
        var lineHeight = style.FontSize * 1.3;
        var totalTextHeight = lines.Count * lineHeight;

        var textY = style.VerticalAlign switch
        {
            "top"    => y + style.FontSize + 2,
            "bottom" => y + height - totalTextHeight + style.FontSize,
            _        => y + (height - totalTextHeight) / 2 + style.FontSize,
        };

        var fontWeight  = style.IsBold ? "bold" : "normal";
        var fontStyle   = style.IsItalic ? "italic" : "normal";
        var decoration  = style.IsUnderline ? "underline" : "none";
        var fontColor   = string.IsNullOrEmpty(style.FontColor) ? "#000000" : style.FontColor;
        var fontFamily  = string.IsNullOrEmpty(style.FontFamily) ? "Helvetica,Arial,sans-serif" : style.FontFamily;
        var fontSize    = SvgElementBuilder.F(style.FontSize);

        var textStyle = $"font-size:{fontSize}px;font-family:{fontFamily};" +
                        $"font-weight:{fontWeight};font-style:{fontStyle};" +
                        $"text-decoration:{decoration};fill:{fontColor};";

        var textElement = SvgElementBuilder.Svg("text",
            new XAttribute("x", SvgElementBuilder.F(textX)),
            new XAttribute("y", SvgElementBuilder.F(textY)),
            new XAttribute("text-anchor", textAnchor),
            new XAttribute("style", textStyle));

        if (lines.Count == 1)
        {
            textElement.Add(new XText(lines[0]));
        }
        else
        {
            for (var i = 0; i < lines.Count; i++)
            {
                var dy = i == 0 ? "0" : SvgElementBuilder.F(lineHeight);
                var tspan = SvgElementBuilder.Svg("tspan",
                    new XAttribute("x", SvgElementBuilder.F(textX)),
                    new XAttribute("dy", dy),
                    new XText(lines[i]));
                textElement.Add(tspan);
            }
        }

        return textElement;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Wraps plain text into lines that fit within the given pixel width,
    /// using an approximate character-width heuristic.
    /// </summary>
    private static List<string> WrapText(string text, double fontSize, double boxWidth)
    {
        var maxChars = boxWidth > 0
            ? (int)(boxWidth / (fontSize * CharWidthFactor))
            : int.MaxValue;

        if (maxChars <= 0)
        {
            maxChars = 20;
        }

        var result = new List<string>();
        foreach (var paragraph in text.Split('\n'))
        {
            var words = paragraph.Split(' ');
            var currentLine = new System.Text.StringBuilder();

            foreach (var word in words)
            {
                if (currentLine.Length == 0)
                {
                    currentLine.Append(word);
                }
                else if (currentLine.Length + 1 + word.Length <= maxChars)
                {
                    currentLine.Append(' ').Append(word);
                }
                else
                {
                    result.Add(currentLine.ToString());
                    currentLine.Clear();
                    currentLine.Append(word);
                }
            }

            if (currentLine.Length > 0)
            {
                result.Add(currentLine.ToString());
            }
        }

        return result.Count > 0 ? result : [text];
    }

    /// <summary>Strips HTML tags from a label string.</summary>
    private static string StripHtml(string html)
    {
        // Replace <br>, <br/>, <br /> with newlines
        var withNewlines = BrTagRegex().Replace(html, "\n");
        // Remove remaining tags
        return HtmlTagRegex().Replace(withNewlines, string.Empty).Trim();
    }

    [GeneratedRegex(@"<br\s*/?>", RegexOptions.IgnoreCase)]
    private static partial Regex BrTagRegex();

    [GeneratedRegex(@"<[^>]+>")]
    private static partial Regex HtmlTagRegex();
}
