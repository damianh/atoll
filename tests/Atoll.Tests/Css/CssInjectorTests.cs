using Atoll.Css;
using Atoll.Head;

namespace Atoll.Tests.Css;

public sealed class CssInjectorTests
{
    // ── CreateInlineStyle ──

    [Fact]
    public void ShouldCreateInlineStyleElement()
    {
        var element = CssInjector.CreateInlineStyle(".card { color: blue; }");

        element.Tag.ShouldBe("style");
        element.Content.ShouldBe(".card { color: blue; }");
    }

    [Fact]
    public void ShouldCreateInlineStyleWithScopeAttribute()
    {
        var element = CssInjector.CreateInlineStyle(".card { color: blue; }", "atoll-12345678");

        element.Tag.ShouldBe("style");
        element.Content.ShouldBe(".card { color: blue; }");
        element.Attributes.ShouldContainKey("data-atoll-scope");
        element.Attributes["data-atoll-scope"].ShouldBe("atoll-12345678");
    }

    // ── CreateStylesheetLink ──

    [Fact]
    public void ShouldCreateStylesheetLinkElement()
    {
        var element = CssInjector.CreateStylesheetLink("/css/main.css");

        element.Tag.ShouldBe("link");
        element.Attributes["rel"].ShouldBe("stylesheet");
        element.Attributes["href"].ShouldBe("/css/main.css");
        element.IsVoid.ShouldBeTrue();
    }

    // ── CreateCombinedStyle ──

    [Fact]
    public void ShouldReturnNullForEmptyAggregator()
    {
        var aggregator = new CssAggregator();

        CssInjector.CreateCombinedStyle(aggregator, false).ShouldBeNull();
    }

    [Fact]
    public void ShouldCreateCombinedStyleFromAggregator()
    {
        var aggregator = new CssAggregator();
        aggregator.Add("a", ".a { color: red; }", false);
        aggregator.Add("b", ".b { color: blue; }", false);

        var element = CssInjector.CreateCombinedStyle(aggregator, false);

        element.ShouldNotBeNull();
        element.Tag.ShouldBe("style");
        element.Content.ShouldNotBeNull();
        element.Content.ShouldContain(".a { color: red; }");
        element.Content.ShouldContain(".b { color: blue; }");
    }

    [Fact]
    public void ShouldMinifyCombinedStyleWhenRequested()
    {
        var aggregator = new CssAggregator();
        aggregator.Add("a", ".card {\n    color: blue;\n}", false);

        var element = CssInjector.CreateCombinedStyle(aggregator, true);

        element.ShouldNotBeNull();
        element.Content.ShouldNotBeNull();
        element.Content.ShouldNotContain("\n");
    }

    // ── RenderInlineStyleHtml ──

    [Fact]
    public void ShouldRenderInlineStyleHtml()
    {
        var html = CssInjector.RenderInlineStyleHtml(".card { color: blue; }");

        html.ShouldBe("<style>.card { color: blue; }</style>");
    }

    [Fact]
    public void ShouldRenderInlineStyleHtmlWithScope()
    {
        var html = CssInjector.RenderInlineStyleHtml(".card { color: blue; }", "atoll-12345678");

        html.ShouldBe("<style data-atoll-scope=\"atoll-12345678\">.card { color: blue; }</style>");
    }

    // ── InjectIntoHead ──

    [Fact]
    public void ShouldInjectCssIntoHeadManager()
    {
        var aggregator = new CssAggregator();
        aggregator.Add("styles", ".card { color: blue; }", false);

        var headManager = new HeadManager();
        CssInjector.InjectIntoHead(aggregator, headManager, false).ShouldBeTrue();

        headManager.Count.ShouldBe(1);
        var elements = headManager.GetElements();
        elements[0].Tag.ShouldBe("style");
        elements[0].Content!.ShouldContain(".card { color: blue; }");
    }

    [Fact]
    public void ShouldReturnFalseWhenNoStylesInAggregator()
    {
        var aggregator = new CssAggregator();
        var headManager = new HeadManager();

        CssInjector.InjectIntoHead(aggregator, headManager, false).ShouldBeFalse();
        headManager.Count.ShouldBe(0);
    }

    [Fact]
    public void ShouldMinifyWhenInjectingIntoHead()
    {
        var aggregator = new CssAggregator();
        aggregator.Add("styles", ".card {\n    color: blue;\n}", false);

        var headManager = new HeadManager();
        CssInjector.InjectIntoHead(aggregator, headManager, true);

        var elements = headManager.GetElements();
        elements[0].Content.ShouldNotBeNull();
        elements[0].Content!.ShouldNotContain("\n");
    }

    // ── Null argument validation ──

    [Fact]
    public void CreateInlineStyleShouldThrowForNullCss()
    {
        Should.Throw<ArgumentNullException>(
            () => CssInjector.CreateInlineStyle(null!));
    }

    [Fact]
    public void CreateInlineStyleWithScopeShouldThrowForNullCss()
    {
        Should.Throw<ArgumentNullException>(
            () => CssInjector.CreateInlineStyle(null!, "hash"));
    }

    [Fact]
    public void CreateInlineStyleWithScopeShouldThrowForNullHash()
    {
        Should.Throw<ArgumentNullException>(
            () => CssInjector.CreateInlineStyle(".card { }", null!));
    }

    [Fact]
    public void CreateStylesheetLinkShouldThrowForNull()
    {
        Should.Throw<ArgumentNullException>(
            () => CssInjector.CreateStylesheetLink(null!));
    }

    [Fact]
    public void CreateCombinedStyleShouldThrowForNullAggregator()
    {
        Should.Throw<ArgumentNullException>(
            () => CssInjector.CreateCombinedStyle(null!, false));
    }

    [Fact]
    public void RenderInlineStyleHtmlShouldThrowForNull()
    {
        Should.Throw<ArgumentNullException>(
            () => CssInjector.RenderInlineStyleHtml(null!));
    }

    [Fact]
    public void RenderInlineStyleHtmlWithScopeShouldThrowForNullCss()
    {
        Should.Throw<ArgumentNullException>(
            () => CssInjector.RenderInlineStyleHtml(null!, "hash"));
    }

    [Fact]
    public void RenderInlineStyleHtmlWithScopeShouldThrowForNullHash()
    {
        Should.Throw<ArgumentNullException>(
            () => CssInjector.RenderInlineStyleHtml(".card { }", null!));
    }

    [Fact]
    public void InjectIntoHeadShouldThrowForNullAggregator()
    {
        Should.Throw<ArgumentNullException>(
            () => CssInjector.InjectIntoHead(null!, new HeadManager(), false));
    }

    [Fact]
    public void InjectIntoHeadShouldThrowForNullHeadManager()
    {
        Should.Throw<ArgumentNullException>(
            () => CssInjector.InjectIntoHead(new CssAggregator(), null!, false));
    }
}
