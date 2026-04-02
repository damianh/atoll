using Atoll.Core.Instructions;
using Atoll.Core.Rendering;
using Shouldly;
using Xunit;

namespace Atoll.Core.Tests.Instructions;

public sealed class HeadInstructionTests
{
    [Fact]
    public async Task StylesheetShouldRenderLinkElement()
    {
        var instruction = HeadInstruction.Stylesheet("/css/main.css");
        var dest = new StringRenderDestination();

        await instruction.RenderAsync(dest);

        dest.GetOutput().ShouldBe("<link rel=\"stylesheet\" href=\"/css/main.css\">");
    }

    [Fact]
    public async Task StylesheetShouldEncodeHref()
    {
        var instruction = HeadInstruction.Stylesheet("/css/file\"name.css");
        var dest = new StringRenderDestination();

        await instruction.RenderAsync(dest);

        dest.GetOutput().ShouldBe("<link rel=\"stylesheet\" href=\"/css/file&quot;name.css\">");
    }

    [Fact]
    public void StylesheetShouldHaveCorrectKey()
    {
        var instruction = HeadInstruction.Stylesheet("/css/main.css");

        instruction.Key.ShouldBe("link:stylesheet:/css/main.css");
    }

    [Fact]
    public async Task InlineStyleShouldRenderStyleElement()
    {
        var instruction = HeadInstruction.InlineStyle("component-abc", ".card { color: red; }");
        var dest = new StringRenderDestination();

        await instruction.RenderAsync(dest);

        dest.GetOutput().ShouldBe("<style>.card { color: red; }</style>");
    }

    [Fact]
    public void InlineStyleShouldHaveCorrectKey()
    {
        var instruction = HeadInstruction.InlineStyle("component-abc", ".card { color: red; }");

        instruction.Key.ShouldBe("style:component-abc");
    }

    [Fact]
    public async Task MetaShouldRenderMetaElement()
    {
        var instruction = HeadInstruction.Meta("description", "A great page");
        var dest = new StringRenderDestination();

        await instruction.RenderAsync(dest);

        dest.GetOutput().ShouldBe("<meta name=\"description\" content=\"A great page\">");
    }

    [Fact]
    public async Task MetaShouldEncodeContent()
    {
        var instruction = HeadInstruction.Meta("description", "A \"great\" page & more");
        var dest = new StringRenderDestination();

        await instruction.RenderAsync(dest);

        dest.GetOutput().ShouldBe(
            "<meta name=\"description\" content=\"A &quot;great&quot; page &amp; more\">");
    }

    [Fact]
    public void MetaShouldHaveCorrectKey()
    {
        var instruction = HeadInstruction.Meta("description", "A great page");

        instruction.Key.ShouldBe("meta:description");
    }

    [Fact]
    public async Task TitleShouldRenderTitleElement()
    {
        var instruction = HeadInstruction.Title("My Page");
        var dest = new StringRenderDestination();

        await instruction.RenderAsync(dest);

        dest.GetOutput().ShouldBe("<title>My Page</title>");
    }

    [Fact]
    public async Task TitleShouldEncodeContent()
    {
        var instruction = HeadInstruction.Title("Page <1> & \"More\"");
        var dest = new StringRenderDestination();

        await instruction.RenderAsync(dest);

        dest.GetOutput().ShouldBe("<title>Page &lt;1&gt; &amp; &quot;More&quot;</title>");
    }

    [Fact]
    public void TitleShouldHaveFixedKey()
    {
        var instruction = HeadInstruction.Title("My Page");

        instruction.Key.ShouldBe("title");
    }

    [Fact]
    public async Task CustomHeadInstructionShouldRenderContent()
    {
        var content = RenderFragment.FromHtml("<link rel=\"icon\" href=\"/favicon.ico\">");
        var instruction = new HeadInstruction("link:icon:/favicon.ico", content);
        var dest = new StringRenderDestination();

        await instruction.RenderAsync(dest);

        dest.GetOutput().ShouldBe("<link rel=\"icon\" href=\"/favicon.ico\">");
    }

    // ── Null argument validation ──

    [Fact]
    public void StylesheetShouldThrowForNullHref()
    {
        Should.Throw<ArgumentNullException>(() => HeadInstruction.Stylesheet(null!));
    }

    [Fact]
    public void InlineStyleShouldThrowForNullScopeId()
    {
        Should.Throw<ArgumentNullException>(
            () => HeadInstruction.InlineStyle(null!, "css"));
    }

    [Fact]
    public void InlineStyleShouldThrowForNullCss()
    {
        Should.Throw<ArgumentNullException>(
            () => HeadInstruction.InlineStyle("scope", null!));
    }

    [Fact]
    public void MetaShouldThrowForNullName()
    {
        Should.Throw<ArgumentNullException>(
            () => HeadInstruction.Meta(null!, "content"));
    }

    [Fact]
    public void MetaShouldThrowForNullContent()
    {
        Should.Throw<ArgumentNullException>(
            () => HeadInstruction.Meta("name", null!));
    }

    [Fact]
    public void TitleShouldThrowForNullTitle()
    {
        Should.Throw<ArgumentNullException>(() => HeadInstruction.Title(null!));
    }
}
