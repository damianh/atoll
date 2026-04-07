using Atoll.DrawIo.Parsing;

namespace Atoll.DrawIo.Tests.Parsing;

public sealed class DrawioDecompressorTests
{
    // The compressed fixture content is the same fragment used in compressed.drawio
    private const string CompressedContent =
        "ZY87DoMwEESvstoLYFOkwUuTgjSpcgIrXmFLNkZm%2Bd0%2BQtCgVKPRaEbzTNq6Ykf%2F" +
        "zo5ja0rO0pq0PTlGCI5QYXXzGmG0hQf5j2qExcaZCV8cY0aYZI9MWPI8OHakm9UH4c9ov0xrsWP" +
        "jJUXSDcLCRXi7rWs8xjvOiaXscIRKIeyXrsGJJ9S1QvAcei%2BED4VgJ8L%2BKh3%2FqvNga6oTrbrx%2FgA%3D";

    [Fact]
    public void DecompressPlainXmlShouldReturnSameString()
    {
        const string xml = "<mxGraphModel><root></root></mxGraphModel>";

        var result = DrawioDecompressor.Decompress(xml);

        result.ShouldBe(xml);
    }

    [Fact]
    public void DecompressCompressedContentShouldReturnXml()
    {
        var result = DrawioDecompressor.Decompress(CompressedContent);

        result.ShouldStartWith("<mxGraphModel");
        result.ShouldContain("<mxCell");
    }

    [Fact]
    public void DecompressEmptyStringShouldReturnEmpty()
    {
        var result = DrawioDecompressor.Decompress(string.Empty);

        result.ShouldBe(string.Empty);
    }

    [Fact]
    public void DecompressInvalidDataShouldThrowInvalidOperationException()
    {
        Should.Throw<InvalidOperationException>(() =>
            DrawioDecompressor.Decompress("not-valid-base64-or-compressed!!!"));
    }

    [Fact]
    public void DecompressedContentShouldBeValidXml()
    {
        var result = DrawioDecompressor.Decompress(CompressedContent);

        var act = () => System.Xml.Linq.XElement.Parse(result);
        act.ShouldNotThrow();
    }
}
