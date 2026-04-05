using System.IO.Compression;
using System.Net;
using System.Text;

namespace Atoll.DrawIo.Parsing;

/// <summary>
/// Decompresses the content of a draw.io diagram element.
/// Draw.io uses deflate+base64+URL-encoding to compress diagram XML.
/// The encoding chain is: XML → deflate → base64 → URL-encode.
/// Decompression reverses this: URL-decode → base64-decode → inflate.
/// </summary>
internal static class DrawioDecompressor
{
    /// <summary>
    /// Decompresses the content of a draw.io diagram element.
    /// If the content is already plain XML (starts with '&lt;'), it is returned as-is.
    /// </summary>
    /// <param name="content">The raw diagram content string (may be compressed or plain XML).</param>
    /// <returns>The decompressed XML string.</returns>
    /// <exception cref="InvalidOperationException">Thrown when decompression fails.</exception>
    internal static string Decompress(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return string.Empty;
        }

        // Plain XML: already decompressed
        var trimmed = content.Trim();
        if (trimmed.StartsWith('<'))
        {
            return trimmed;
        }

        try
        {
            // Step 1: URL-decode
            var urlDecoded = WebUtility.UrlDecode(content);

            // Step 2: Base64-decode
            var bytes = Convert.FromBase64String(urlDecoded);

            // Step 3: Raw inflate (deflate stream without zlib header)
            using var inputStream = new MemoryStream(bytes);
            using var deflateStream = new DeflateStream(inputStream, CompressionMode.Decompress);
            using var outputStream = new MemoryStream();
            deflateStream.CopyTo(outputStream);

            return Encoding.UTF8.GetString(outputStream.ToArray());
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "Failed to decompress draw.io diagram content. " +
                "The content may be corrupted or use an unsupported encoding.", ex);
        }
    }
}
