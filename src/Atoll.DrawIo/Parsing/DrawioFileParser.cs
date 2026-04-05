using System.Xml.Linq;

namespace Atoll.DrawIo.Parsing;

/// <summary>
/// Parses <c>.drawio</c> and <c>.dio</c> files into a <see cref="DrawioFile"/> model.
/// Handles both plain XML and compressed (deflate+base64+URL-encoded) diagram content.
/// </summary>
public static class DrawioFileParser
{
    /// <summary>
    /// Parses a draw.io file from the given XML string.
    /// </summary>
    /// <param name="xml">The raw content of the <c>.drawio</c> file.</param>
    /// <returns>The parsed <see cref="DrawioFile"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the file content is not a valid draw.io XML file.
    /// </exception>
    public static DrawioFile Parse(string xml)
    {
        ArgumentNullException.ThrowIfNull(xml);

        if (string.IsNullOrWhiteSpace(xml))
        {
            return new DrawioFile([]);
        }

        XElement root;
        try
        {
            root = XElement.Parse(xml.Trim());
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to parse draw.io file: invalid XML.", ex);
        }

        return ParseRoot(root);
    }

    /// <summary>
    /// Parses a draw.io file from the given stream.
    /// </summary>
    /// <param name="stream">A stream containing the raw draw.io file content.</param>
    /// <returns>The parsed <see cref="DrawioFile"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the stream contains invalid draw.io XML.</exception>
    public static DrawioFile Parse(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        XElement root;
        try
        {
            root = XElement.Load(stream);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to parse draw.io stream: invalid XML.", ex);
        }

        return ParseRoot(root);
    }

    private static DrawioFile ParseRoot(XElement root)
    {
        // Support both <mxfile> root (standard) and direct <mxGraphModel> (embedded)
        if (root.Name.LocalName == "mxGraphModel")
        {
            var singleModel = MxGraphModelParser.Parse(root.ToString());
            var singlePage = new DrawioPage("Page-1", "page-1", singleModel);
            return new DrawioFile([singlePage]);
        }

        if (root.Name.LocalName != "mxfile")
        {
            throw new InvalidOperationException(
                $"Expected root element <mxfile> or <mxGraphModel>, but found <{root.Name.LocalName}>.");
        }

        var pages = new List<DrawioPage>();
        int pageIndex = 1;

        foreach (var diagramElement in root.Elements("diagram"))
        {
            var name = (string?)diagramElement.Attribute("name") ?? $"Page-{pageIndex}";
            var id = (string?)diagramElement.Attribute("id") ?? $"page-{pageIndex}";
            var content = diagramElement.Value?.Trim() ?? string.Empty;

            // If the diagram element has child elements, it's already plain XML embedded
            var childElement = diagramElement.Elements().FirstOrDefault();
            string modelXml;
            if (childElement != null)
            {
                modelXml = childElement.ToString();
            }
            else
            {
                // Content is the compressed/encoded diagram XML
                modelXml = DrawioDecompressor.Decompress(content);
            }

            var model = MxGraphModelParser.Parse(modelXml);
            pages.Add(new DrawioPage(name, id, model));
            pageIndex++;
        }

        return new DrawioFile(pages);
    }
}
