using System.Text;

namespace Atoll.Rendering;

/// <summary>
/// Provides HTML encoding for text content. Used by <see cref="RenderChunk"/>
/// to escape plain text before output.
/// </summary>
public static class HtmlEncoder
{
    /// <summary>
    /// Encodes the specified text for safe inclusion in HTML content.
    /// Escapes <c>&amp;</c>, <c>&lt;</c>, <c>&gt;</c>, <c>&quot;</c>, and <c>&#39;</c>.
    /// </summary>
    /// <param name="text">The text to encode.</param>
    /// <returns>The HTML-encoded text.</returns>
    public static string Encode(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        // Quick scan to see if encoding is needed
        if (!RequiresEncoding(text))
        {
            return text;
        }

        var sb = new StringBuilder(text.Length + 16);
        foreach (var c in text)
        {
            switch (c)
            {
                case '&':
                    sb.Append("&amp;");
                    break;
                case '<':
                    sb.Append("&lt;");
                    break;
                case '>':
                    sb.Append("&gt;");
                    break;
                case '"':
                    sb.Append("&quot;");
                    break;
                case '\'':
                    sb.Append("&#39;");
                    break;
                default:
                    sb.Append(c);
                    break;
            }
        }

        return sb.ToString();
    }

    private static bool RequiresEncoding(string text)
    {
        foreach (var c in text)
        {
            switch (c)
            {
                case '&':
                case '<':
                case '>':
                case '"':
                case '\'':
                    return true;
            }
        }

        return false;
    }
}
