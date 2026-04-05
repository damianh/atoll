using Atoll.DrawIo.Model;
using Atoll.DrawIo.Parsing;
using Shouldly;
using Xunit;

namespace Atoll.DrawIo.Tests.Parsing;

public sealed class MxStyleParserTests
{
    [Fact]
    public void ParseNullShouldReturnEmpty()
    {
        var style = MxStyleParser.Parse(null);

        style.ShouldBeSameAs(MxStyle.Empty);
    }

    [Fact]
    public void ParseEmptyStringShouldReturnEmpty()
    {
        var style = MxStyleParser.Parse(string.Empty);

        style.ShouldBeSameAs(MxStyle.Empty);
    }

    [Fact]
    public void ParseSimpleKeyValuePairs()
    {
        var style = MxStyleParser.Parse("rounded=1;fillColor=#dae8fc;strokeColor=#6c8ebf;");

        style.ShapeName.ShouldBeNull();
        style["rounded"].ShouldBe("1");
        style["fillColor"].ShouldBe("#dae8fc");
        style["strokeColor"].ShouldBe("#6c8ebf");
    }

    [Fact]
    public void ParseLeadingShapeName()
    {
        var style = MxStyleParser.Parse("rhombus;fillColor=#fff2cc;strokeColor=#d6b656;");

        style.ShapeName.ShouldBe("rhombus");
        style["fillColor"].ShouldBe("#fff2cc");
    }

    [Fact]
    public void ParseEllipseShapeName()
    {
        var style = MxStyleParser.Parse("ellipse;whiteSpace=wrap;html=1;");

        style.ShapeName.ShouldBe("ellipse");
        style["whiteSpace"].ShouldBe("wrap");
    }

    [Fact]
    public void ParseRoundedProperty()
    {
        var style = MxStyleParser.Parse("rounded=1;");

        style.Rounded.ShouldBeTrue();
    }

    [Fact]
    public void ParseDashedProperty()
    {
        var style = MxStyleParser.Parse("dashed=1;");

        style.Dashed.ShouldBeTrue();
    }

    [Fact]
    public void ParseFontSizeProperty()
    {
        var style = MxStyleParser.Parse("fontSize=14;");

        style.FontSize.ShouldBe(14.0);
    }

    [Fact]
    public void DefaultFontSizeIsSeven()
    {
        var style = MxStyleParser.Parse("rounded=1;");

        // Default is 11 when not set
        style.FontSize.ShouldBe(11.0);
    }

    [Fact]
    public void ParseFontStyleBoldBitmask()
    {
        var style = MxStyleParser.Parse("fontStyle=1;");

        style.IsBold.ShouldBeTrue();
        style.IsItalic.ShouldBeFalse();
        style.IsUnderline.ShouldBeFalse();
    }

    [Fact]
    public void ParseFontStyleItalicBitmask()
    {
        var style = MxStyleParser.Parse("fontStyle=2;");

        style.IsItalic.ShouldBeTrue();
        style.IsBold.ShouldBeFalse();
    }

    [Fact]
    public void ParseFontStyleCombinedBitmask()
    {
        var style = MxStyleParser.Parse("fontStyle=3;");

        style.IsBold.ShouldBeTrue();
        style.IsItalic.ShouldBeTrue();
        style.IsUnderline.ShouldBeFalse();
    }

    [Fact]
    public void ParseAlignProperty()
    {
        var style = MxStyleParser.Parse("align=left;");

        style.Align.ShouldBe("left");
    }

    [Fact]
    public void DefaultAlignIsCenter()
    {
        var style = MxStyleParser.Parse("rounded=1;");

        style.Align.ShouldBe("center");
    }

    [Fact]
    public void ParseOpacityProperty()
    {
        var style = MxStyleParser.Parse("opacity=50;");

        style.Opacity.ShouldBe(50);
    }

    [Fact]
    public void ParseIsHtmlProperty()
    {
        var style = MxStyleParser.Parse("html=1;");

        style.IsHtml.ShouldBeTrue();
    }

    [Fact]
    public void ParseEdgeStyleProperty()
    {
        var style = MxStyleParser.Parse("edgeStyle=orthogonalEdgeStyle;");

        style.EdgeStyle.ShouldBe("orthogonalEdgeStyle");
    }

    [Fact]
    public void ParseEndArrowProperty()
    {
        var style = MxStyleParser.Parse("endArrow=open;endFill=0;");

        style.EndArrow.ShouldBe("open");
    }

    [Fact]
    public void EffectiveShapeUsesShapePropertyFirst()
    {
        var style = MxStyleParser.Parse("shape=cylinder3;fillColor=#f5f5f5;");

        style.EffectiveShape.ShouldBe("cylinder3");
    }

    [Fact]
    public void EffectiveShapeFallsBackToShapeName()
    {
        var style = MxStyleParser.Parse("rhombus;fillColor=#fff;");

        style.EffectiveShape.ShouldBe("rhombus");
    }

    [Fact]
    public void ParseCaseInsensitiveKeys()
    {
        var style = MxStyleParser.Parse("FillColor=#aabbcc;");

        style.FillColor.ShouldBe("#aabbcc");
    }
}
