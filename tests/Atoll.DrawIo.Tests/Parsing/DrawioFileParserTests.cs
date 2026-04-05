using Atoll.DrawIo.Parsing;
using Shouldly;
using Xunit;

namespace Atoll.DrawIo.Tests.Parsing;

public sealed class DrawioFileParserTests
{
    private static readonly string FixturesDir =
        Path.Combine(AppContext.BaseDirectory, "Fixtures");

    [Fact]
    public void ParsePlainXmlShouldReturnSinglePage()
    {
        var xml = File.ReadAllText(Path.Combine(FixturesDir, "simple.drawio"));

        var file = DrawioFileParser.Parse(xml);

        file.Pages.Count.ShouldBe(1);
        file.Pages[0].Name.ShouldBe("Page-1");
    }

    [Fact]
    public void ParsePlainXmlShouldExtractVertices()
    {
        var xml = File.ReadAllText(Path.Combine(FixturesDir, "simple.drawio"));

        var file = DrawioFileParser.Parse(xml);

        var vertices = file.Pages[0].Model.Vertices;
        vertices.Count.ShouldBe(2);
    }

    [Fact]
    public void ParsePlainXmlShouldExtractEdge()
    {
        var xml = File.ReadAllText(Path.Combine(FixturesDir, "simple.drawio"));

        var file = DrawioFileParser.Parse(xml);

        var edges = file.Pages[0].Model.Edges;
        edges.Count.ShouldBe(1);
    }

    [Fact]
    public void ParseCompressedShouldReturnSinglePage()
    {
        var xml = File.ReadAllText(Path.Combine(FixturesDir, "compressed.drawio"));

        var file = DrawioFileParser.Parse(xml);

        file.Pages.Count.ShouldBe(1);
    }

    [Fact]
    public void ParseCompressedShouldExtractVertex()
    {
        var xml = File.ReadAllText(Path.Combine(FixturesDir, "compressed.drawio"));

        var file = DrawioFileParser.Parse(xml);

        var vertices = file.Pages[0].Model.Vertices;
        vertices.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void ParseMultiPageShouldReturnThreePages()
    {
        var xml = File.ReadAllText(Path.Combine(FixturesDir, "multi-page.drawio"));

        var file = DrawioFileParser.Parse(xml);

        file.Pages.Count.ShouldBe(3);
    }

    [Fact]
    public void ParseMultiPageShouldPreservePageNames()
    {
        var xml = File.ReadAllText(Path.Combine(FixturesDir, "multi-page.drawio"));

        var file = DrawioFileParser.Parse(xml);

        file.Pages[0].Name.ShouldBe("Overview");
        file.Pages[1].Name.ShouldBe("Details");
        file.Pages[2].Name.ShouldBe("Flow");
    }

    [Fact]
    public void ParseEmptyStringShouldReturnEmptyFile()
    {
        var file = DrawioFileParser.Parse(string.Empty);

        file.Pages.ShouldBeEmpty();
    }

    [Fact]
    public void ParseInvalidXmlShouldThrowInvalidOperationException()
    {
        Should.Throw<InvalidOperationException>(() => DrawioFileParser.Parse("not xml at all!!"));
    }

    [Fact]
    public void ParseMxGraphModelDirectlyShouldReturnSinglePage()
    {
        const string xml = """
            <mxGraphModel>
              <root>
                <mxCell id="0" />
                <mxCell id="1" parent="0" />
                <mxCell id="2" value="Test" vertex="1" parent="1">
                  <mxGeometry x="10" y="10" width="80" height="40" as="geometry" />
                </mxCell>
              </root>
            </mxGraphModel>
            """;

        var file = DrawioFileParser.Parse(xml);

        file.Pages.Count.ShouldBe(1);
        file.Pages[0].Model.Vertices.Count.ShouldBe(1);
    }

    [Fact]
    public void ParseFromStreamShouldProduceSameResultAsString()
    {
        var path = Path.Combine(FixturesDir, "simple.drawio");
        var stringResult = DrawioFileParser.Parse(File.ReadAllText(path));

        DrawioFile streamResult;
        using (var fs = File.OpenRead(path))
        {
            streamResult = DrawioFileParser.Parse(fs);
        }

        streamResult.Pages.Count.ShouldBe(stringResult.Pages.Count);
    }
}
