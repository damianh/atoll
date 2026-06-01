using System.IO.Compression;
using System.Text;
using System.Xml;

namespace Atoll.Swell.Export;

/// <summary>
/// Exports a Swell slide deck to ODP (OpenDocument Presentation) format.
/// Each slide is screenshotted and inserted as a full-bleed image in an ODP slide.
/// Speaker notes are included. Fidelity loss is acceptable.
/// </summary>
public sealed class OdpExporter
{
    /// <summary>
    /// Exports the deck to an <c>.odp</c> file.
    /// </summary>
    /// <param name="options">Export configuration.</param>
    /// <param name="notes">Optional per-slide presenter notes (index → note text).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The path of the generated ODP file.</returns>
    public async Task<string> ExportAsync(
        ExportOptions options,
        IReadOnlyList<string>? notes = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        ExportHelper.ValidateSlideCount(options);

        var outputFile = options.OutputPath.EndsWith(".odp", StringComparison.OrdinalIgnoreCase)
            ? options.OutputPath
            : options.OutputPath + ".odp";

        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(outputFile) ?? ".");

        var height = ExportHelper.ResolveHeight(options.AspectRatio);
        var screenshots = await ExportHelper.CaptureScreenshotsAsync(options, height, cancellationToken);

        BuildOdp(outputFile, screenshots, notes ?? [], ExportHelper.BaseWidth, height);

        return outputFile;
    }

    private static void BuildOdp(
        string outputFile,
        List<byte[]> screenshots,
        IReadOnlyList<string> notes,
        int widthPx,
        int heightPx)
    {
        // ODP is a ZIP archive with specific XML files.
        using var zipStream = System.IO.File.Create(outputFile);
        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: false);

        // Write mimetype (must be first, uncompressed)
        var mimeEntry = archive.CreateEntry("mimetype", CompressionLevel.NoCompression);
        using (var mimeWriter = new StreamWriter(mimeEntry.Open(), Encoding.ASCII))
        {
            mimeWriter.Write("application/vnd.oasis.opendocument.presentation");
        }

        // Write manifest
        WriteManifest(archive, screenshots.Count);

        // Write slide images
        for (var i = 0; i < screenshots.Count; i++)
        {
            var imageEntry = archive.CreateEntry($"Pictures/slide-{i}.png", CompressionLevel.Optimal);
            using var imageStream = imageEntry.Open();
            imageStream.Write(screenshots[i], 0, screenshots[i].Length);
        }

        // Write content.xml
        WriteContentXml(archive, screenshots.Count, notes, widthPx, heightPx);

        // Write meta.xml
        WriteMetaXml(archive);
    }

    private static void WriteManifest(ZipArchive archive, int slideCount)
    {
        var entry = archive.CreateEntry("META-INF/manifest.xml", CompressionLevel.Optimal);
        using var writer = XmlWriter.Create(entry.Open(), XmlSettings());
        writer.WriteStartDocument();
        writer.WriteStartElement("manifest:manifest", OdfNs.Manifest);
        writer.WriteAttributeString("xmlns:manifest", OdfNs.Manifest);

        WriteManifestEntry(writer, "/", "application/vnd.oasis.opendocument.presentation");
        WriteManifestEntry(writer, "content.xml", "text/xml");
        WriteManifestEntry(writer, "meta.xml", "text/xml");

        for (var i = 0; i < slideCount; i++)
        {
            WriteManifestEntry(writer, $"Pictures/slide-{i}.png", "image/png");
        }

        writer.WriteEndElement(); // manifest:manifest
    }

    private static void WriteManifestEntry(XmlWriter writer, string fullPath, string mediaType)
    {
        writer.WriteStartElement("manifest:file-entry", OdfNs.Manifest);
        writer.WriteAttributeString("manifest:full-path", OdfNs.Manifest, fullPath);
        writer.WriteAttributeString("manifest:media-type", OdfNs.Manifest, mediaType);
        writer.WriteEndElement();
    }

    private static void WriteContentXml(
        ZipArchive archive,
        int slideCount,
        IReadOnlyList<string> notes,
        int widthPx,
        int heightPx)
    {
        // Convert px to cm (96 DPI)
        var widthCm = Math.Round(widthPx / 96.0 * 2.54, 4);
        var heightCm = Math.Round(heightPx / 96.0 * 2.54, 4);
        var wStr = widthCm.ToString("F4", System.Globalization.CultureInfo.InvariantCulture) + "cm";
        var hStr = heightCm.ToString("F4", System.Globalization.CultureInfo.InvariantCulture) + "cm";

        var entry = archive.CreateEntry("content.xml", CompressionLevel.Optimal);
        using var writer = XmlWriter.Create(entry.Open(), XmlSettings());

        writer.WriteStartDocument();
        writer.WriteStartElement("office:document-content", OdfNs.Office);
        writer.WriteAttributeString("xmlns:office", OdfNs.Office);
        writer.WriteAttributeString("xmlns:draw", OdfNs.Draw);
        writer.WriteAttributeString("xmlns:presentation", OdfNs.Presentation);
        writer.WriteAttributeString("xmlns:svg", OdfNs.Svg);
        writer.WriteAttributeString("xmlns:xlink", OdfNs.XLink);
        writer.WriteAttributeString("xmlns:text", OdfNs.Text);
        writer.WriteAttributeString("xmlns:style", OdfNs.Style);
        writer.WriteAttributeString("xmlns:fo", OdfNs.Fo);

        writer.WriteStartElement("office:body", OdfNs.Office);
        writer.WriteStartElement("office:presentation", OdfNs.Office);

        for (var i = 0; i < slideCount; i++)
        {
            writer.WriteStartElement("draw:page", OdfNs.Draw);
            writer.WriteAttributeString("draw:name", OdfNs.Draw, $"page{i + 1}");

            // Full-bleed image frame
            writer.WriteStartElement("draw:frame", OdfNs.Draw);
            writer.WriteAttributeString("draw:name", OdfNs.Draw, $"image{i + 1}");
            writer.WriteAttributeString("svg:x", OdfNs.Svg, "0cm");
            writer.WriteAttributeString("svg:y", OdfNs.Svg, "0cm");
            writer.WriteAttributeString("svg:width", OdfNs.Svg, wStr);
            writer.WriteAttributeString("svg:height", OdfNs.Svg, hStr);

            writer.WriteStartElement("draw:image", OdfNs.Draw);
            writer.WriteAttributeString("xlink:href", OdfNs.XLink, $"Pictures/slide-{i}.png");
            writer.WriteAttributeString("xlink:type", OdfNs.XLink, "simple");
            writer.WriteAttributeString("xlink:show", OdfNs.XLink, "embed");
            writer.WriteAttributeString("xlink:actuate", OdfNs.XLink, "onLoad");
            writer.WriteEndElement(); // draw:image
            writer.WriteEndElement(); // draw:frame

            // Presenter notes
            var note = i < notes.Count ? notes[i] : "";
            if (!string.IsNullOrWhiteSpace(note))
            {
                writer.WriteStartElement("presentation:notes", OdfNs.Presentation);
                writer.WriteStartElement("draw:frame", OdfNs.Draw);
                writer.WriteAttributeString("presentation:class", OdfNs.Presentation, "notes");
                writer.WriteAttributeString("svg:x", OdfNs.Svg, "0cm");
                writer.WriteAttributeString("svg:y", OdfNs.Svg, "0cm");
                writer.WriteAttributeString("svg:width", OdfNs.Svg, wStr);
                writer.WriteAttributeString("svg:height", OdfNs.Svg, "5cm");
                writer.WriteStartElement("draw:text-box", OdfNs.Draw);
                writer.WriteStartElement("text:p", OdfNs.Text);
                writer.WriteString(note);
                writer.WriteEndElement(); // text:p
                writer.WriteEndElement(); // draw:text-box
                writer.WriteEndElement(); // draw:frame
                writer.WriteEndElement(); // presentation:notes
            }

            writer.WriteEndElement(); // draw:page
        }

        writer.WriteEndElement(); // office:presentation
        writer.WriteEndElement(); // office:body
        writer.WriteEndElement(); // office:document-content
    }

    private static void WriteMetaXml(ZipArchive archive)
    {
        var entry = archive.CreateEntry("meta.xml", CompressionLevel.Optimal);
        using var writer = XmlWriter.Create(entry.Open(), XmlSettings());
        writer.WriteStartDocument();
        writer.WriteStartElement("office:document-meta", OdfNs.Office);
        writer.WriteAttributeString("xmlns:office", OdfNs.Office);
        writer.WriteStartElement("office:meta", OdfNs.Office);
        writer.WriteElementString("meta:generator", "urn:schemas-microsoft-com:office:office", "Atoll.Swell");
        writer.WriteEndElement();
        writer.WriteEndElement();
    }

    private static XmlWriterSettings XmlSettings() =>
        new() { Indent = true, Encoding = Encoding.UTF8 };

    private static class OdfNs
    {
        internal const string Office = "urn:oasis:names:tc:opendocument:xmlns:office:1.0";
        internal const string Draw = "urn:oasis:names:tc:opendocument:xmlns:drawing:1.0";
        internal const string Presentation = "urn:oasis:names:tc:opendocument:xmlns:presentation:1.0";
        internal const string Svg = "urn:oasis:names:tc:opendocument:xmlns:svg-compatible:1.0";
        internal const string XLink = "http://www.w3.org/1999/xlink";
        internal const string Text = "urn:oasis:names:tc:opendocument:xmlns:text:1.0";
        internal const string Style = "urn:oasis:names:tc:opendocument:xmlns:style:1.0";
        internal const string Fo = "urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0";
        internal const string Manifest = "urn:oasis:names:tc:opendocument:xmlns:manifest:1.0";
    }
}
