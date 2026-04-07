using Atoll.DrawIo.Parsing;

namespace Atoll.DrawIo.Tests.Parsing;

public sealed class MxGraphModelParserTests
{
    [Fact]
    public void ParseShouldReturnAllCells()
    {
        const string xml = """
            <mxGraphModel>
              <root>
                <mxCell id="0" />
                <mxCell id="1" parent="0" />
                <mxCell id="2" value="A" vertex="1" parent="1">
                  <mxGeometry x="0" y="0" width="100" height="50" as="geometry" />
                </mxCell>
                <mxCell id="3" value="B" vertex="1" parent="1">
                  <mxGeometry x="200" y="0" width="100" height="50" as="geometry" />
                </mxCell>
              </root>
            </mxGraphModel>
            """;

        var model = MxGraphModelParser.Parse(xml);

        model.Cells.Count.ShouldBe(4); // 0, 1, 2, 3
    }

    [Fact]
    public void ParseShouldIdentifyVertices()
    {
        const string xml = """
            <mxGraphModel>
              <root>
                <mxCell id="0" />
                <mxCell id="1" parent="0" />
                <mxCell id="2" value="Node" vertex="1" parent="1">
                  <mxGeometry x="10" y="10" width="80" height="40" as="geometry" />
                </mxCell>
              </root>
            </mxGraphModel>
            """;

        var model = MxGraphModelParser.Parse(xml);

        model.Vertices.Count.ShouldBe(1);
        model.Vertices[0].Id.ShouldBe("2");
        model.Vertices[0].IsVertex.ShouldBeTrue();
    }

    [Fact]
    public void ParseShouldIdentifyEdges()
    {
        const string xml = """
            <mxGraphModel>
              <root>
                <mxCell id="0" />
                <mxCell id="1" parent="0" />
                <mxCell id="2" value="A" vertex="1" parent="1">
                  <mxGeometry x="0" y="0" width="100" height="50" as="geometry" />
                </mxCell>
                <mxCell id="3" value="B" vertex="1" parent="1">
                  <mxGeometry x="200" y="0" width="100" height="50" as="geometry" />
                </mxCell>
                <mxCell id="4" value="" edge="1" source="2" target="3" parent="1">
                  <mxGeometry relative="1" as="geometry" />
                </mxCell>
              </root>
            </mxGraphModel>
            """;

        var model = MxGraphModelParser.Parse(xml);

        model.Edges.Count.ShouldBe(1);
        model.Edges[0].Source.ShouldBe("2");
        model.Edges[0].Target.ShouldBe("3");
    }

    [Fact]
    public void ParseShouldExtractGeometry()
    {
        const string xml = """
            <mxGraphModel>
              <root>
                <mxCell id="0" />
                <mxCell id="1" parent="0" />
                <mxCell id="2" vertex="1" parent="1">
                  <mxGeometry x="50" y="75" width="120" height="80" as="geometry" />
                </mxCell>
              </root>
            </mxGraphModel>
            """;

        var model = MxGraphModelParser.Parse(xml);
        var geo = model.Vertices[0].Geometry!;

        geo.X.ShouldBe(50);
        geo.Y.ShouldBe(75);
        geo.Width.ShouldBe(120);
        geo.Height.ShouldBe(80);
    }

    [Fact]
    public void ParseShouldExtractLayers()
    {
        const string xml = """
            <mxGraphModel>
              <root>
                <mxCell id="0" />
                <mxCell id="layer1" value="Background" parent="0" />
                <mxCell id="layer2" value="Content" parent="0" />
                <mxCell id="2" vertex="1" parent="layer1">
                  <mxGeometry x="0" y="0" width="100" height="50" as="geometry" />
                </mxCell>
              </root>
            </mxGraphModel>
            """;

        var model = MxGraphModelParser.Parse(xml);

        model.Layers.Count.ShouldBe(2);
        model.Layers[0].Value.ShouldBe("Background");
        model.Layers[1].Value.ShouldBe("Content");
    }

    [Fact]
    public void ParseShouldExtractCellStyleString()
    {
        const string xml = """
            <mxGraphModel>
              <root>
                <mxCell id="0" />
                <mxCell id="1" parent="0" />
                <mxCell id="2" style="rounded=1;fillColor=#dae8fc;" vertex="1" parent="1">
                  <mxGeometry x="0" y="0" width="100" height="50" as="geometry" />
                </mxCell>
              </root>
            </mxGraphModel>
            """;

        var model = MxGraphModelParser.Parse(xml);

        model.Vertices[0].StyleString.ShouldBe("rounded=1;fillColor=#dae8fc;");
    }

    [Fact]
    public void ParseShouldExtractCellLabel()
    {
        const string xml = """
            <mxGraphModel>
              <root>
                <mxCell id="0" />
                <mxCell id="1" parent="0" />
                <mxCell id="2" value="Hello World" vertex="1" parent="1">
                  <mxGeometry x="0" y="0" width="100" height="50" as="geometry" />
                </mxCell>
              </root>
            </mxGraphModel>
            """;

        var model = MxGraphModelParser.Parse(xml);

        model.Vertices[0].Value.ShouldBe("Hello World");
    }

    [Fact]
    public void GetCellByIdShouldReturnCorrectCell()
    {
        const string xml = """
            <mxGraphModel>
              <root>
                <mxCell id="0" />
                <mxCell id="1" parent="0" />
                <mxCell id="abc123" value="Found" vertex="1" parent="1">
                  <mxGeometry x="0" y="0" width="100" height="50" as="geometry" />
                </mxCell>
              </root>
            </mxGraphModel>
            """;

        var model = MxGraphModelParser.Parse(xml);

        model.GetCellById("abc123").ShouldNotBeNull();
        model.GetCellById("abc123")!.Value.ShouldBe("Found");
        model.GetCellById("notexist").ShouldBeNull();
    }
}
