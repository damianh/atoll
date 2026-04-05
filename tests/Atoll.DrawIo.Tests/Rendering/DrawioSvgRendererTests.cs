using Atoll.DrawIo.Parsing;
using Atoll.DrawIo.Rendering;
using Shouldly;
using Xunit;

namespace Atoll.DrawIo.Tests.Rendering;

public sealed class DrawioSvgRendererTests
{
    private static readonly string FixturesDir =
        Path.Combine(AppContext.BaseDirectory, "Fixtures");

    private static DrawioFile ParseFixture(string name) =>
        DrawioFileParser.Parse(File.ReadAllText(Path.Combine(FixturesDir, name)));

    [Fact]
    public void RenderToSvgShouldReturnSvgElement()
    {
        var file = ParseFixture("simple.drawio");

        var svg = DrawioSvgRenderer.RenderToSvg(file);

        svg.ShouldStartWith("<svg");
        svg.ShouldContain("xmlns");
    }

    [Fact]
    public void RenderToSvgShouldContainViewBox()
    {
        var file = ParseFixture("simple.drawio");

        var svg = DrawioSvgRenderer.RenderToSvg(file);

        svg.ShouldContain("viewBox=");
    }

    [Fact]
    public void RenderToSvgShouldContainVertexShapes()
    {
        var file = ParseFixture("simple.drawio");

        var svg = DrawioSvgRenderer.RenderToSvg(file);

        // Should have rect elements for the rectangle vertices
        svg.ShouldContain("<rect");
    }

    [Fact]
    public void RenderToSvgShouldContainEllipseForEllipseShape()
    {
        var file = ParseFixture("simple.drawio");

        var svg = DrawioSvgRenderer.RenderToSvg(file);

        svg.ShouldContain("<ellipse");
    }

    [Fact]
    public void RenderToSvgShouldContainPathForEdges()
    {
        var file = ParseFixture("simple.drawio");

        var svg = DrawioSvgRenderer.RenderToSvg(file);

        svg.ShouldContain("<path");
    }

    [Fact]
    public void RenderPageToSvgShouldRenderSpecificPage()
    {
        var file = ParseFixture("multi-page.drawio");

        var svg = DrawioSvgRenderer.RenderPageToSvg(file.Pages[1]);

        // Page 2 ("Details") has an ellipse
        svg.ShouldContain("<ellipse");
    }

    [Fact]
    public void RenderWithPageIndexShouldSelectCorrectPage()
    {
        var file = ParseFixture("multi-page.drawio");
        var options = new DrawioRenderOptions { PageIndex = 2 };

        var svg = DrawioSvgRenderer.RenderToSvg(file, options);

        // Page 3 ("Flow") has rectangles + an edge
        svg.ShouldContain("<rect");
    }

    [Fact]
    public void RenderWithPageNameShouldSelectCorrectPage()
    {
        var file = ParseFixture("multi-page.drawio");
        var options = new DrawioRenderOptions { PageName = "Details" };

        var svg = DrawioSvgRenderer.RenderToSvg(file, options);

        svg.ShouldContain("<ellipse");
    }

    [Fact]
    public void RenderWithWidthOptionShouldSetWidthAttribute()
    {
        var file = ParseFixture("simple.drawio");
        var options = new DrawioRenderOptions { Width = "800px" };

        var svg = DrawioSvgRenderer.RenderToSvg(file, options);

        svg.ShouldContain("width=\"800px\"");
    }

    [Fact]
    public void RenderWithBackgroundShouldContainRectWithFill()
    {
        var file = ParseFixture("simple.drawio");
        var options = new DrawioRenderOptions { Background = "#ffffff" };

        var svg = DrawioSvgRenderer.RenderToSvg(file, options);

        svg.ShouldContain("fill:#ffffff");
    }

    [Fact]
    public void RenderWithLayersShouldGroupByLayer()
    {
        var file = ParseFixture("layers.drawio");

        var svg = DrawioSvgRenderer.RenderToSvg(file);

        svg.ShouldContain("<g");
        svg.ShouldContain("data-layer-name=");
    }

    [Fact]
    public void RenderWithHiddenLayerShouldSetDisplayNone()
    {
        var file = ParseFixture("layers.drawio");
        var options = new DrawioRenderOptions { HiddenLayers = ["Annotations"] };

        var svg = DrawioSvgRenderer.RenderToSvg(file, options);

        svg.ShouldContain("display=\"none\"");
    }

    [Fact]
    public void RenderEmptyFileShouldThrow()
    {
        var emptyFile = DrawioFileParser.Parse(string.Empty);

        Should.Throw<ArgumentException>(() => DrawioSvgRenderer.RenderToSvg(emptyFile));
    }

    [Fact]
    public void RenderWithInvalidPageNameShouldThrow()
    {
        var file = ParseFixture("simple.drawio");
        var options = new DrawioRenderOptions { PageName = "Nonexistent" };

        Should.Throw<ArgumentException>(() => DrawioSvgRenderer.RenderToSvg(file, options));
    }

    [Fact]
    public void RenderStyledDiagramShouldContainColorAttributes()
    {
        var file = ParseFixture("styled.drawio");

        var svg = DrawioSvgRenderer.RenderToSvg(file);

        svg.ShouldContain("#dae8fc");
    }
}
