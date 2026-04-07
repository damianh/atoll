using Atoll.Rendering;

namespace Atoll.Tests.Rendering;

public sealed class HtmlStringTests
{
    [Fact]
    public void ShouldStoreHtmlValue()
    {
        var html = new HtmlString("<div>content</div>");

        html.Value.ShouldBe("<div>content</div>");
        html.IsEmpty.ShouldBeFalse();
    }

    [Fact]
    public void ShouldReturnValueFromToString()
    {
        var html = new HtmlString("<p>test</p>");

        html.ToString().ShouldBe("<p>test</p>");
    }

    [Fact]
    public void ShouldConvertToHtmlRenderChunk()
    {
        var html = new HtmlString("<span>test</span>");
        var chunk = html.ToChunk();

        chunk.Kind.ShouldBe(RenderChunkKind.Html);
        chunk.GetValue().ShouldBe("<span>test</span>");
    }

    [Fact]
    public async Task ShouldConvertToRenderFragment()
    {
        var html = new HtmlString("<article>content</article>");
        var fragment = html.ToFragment();

        var output = await fragment.RenderToStringAsync();

        output.ShouldBe("<article>content</article>");
    }

    [Fact]
    public void EmptyShouldBeEmpty()
    {
        HtmlString.Empty.IsEmpty.ShouldBeTrue();
        HtmlString.Empty.Value.ShouldBe(string.Empty);
    }

    [Fact]
    public void ShouldThrowForNullValue()
    {
        Should.Throw<ArgumentNullException>(() => new HtmlString(null!));
    }

    [Fact]
    public void EqualHtmlStringsShouldBeEqual()
    {
        var a = new HtmlString("<p>same</p>");
        var b = new HtmlString("<p>same</p>");

        a.ShouldBe(b);
        (a == b).ShouldBeTrue();
        (a != b).ShouldBeFalse();
        a.GetHashCode().ShouldBe(b.GetHashCode());
    }

    [Fact]
    public void DifferentHtmlStringsShouldNotBeEqual()
    {
        var a = new HtmlString("<p>a</p>");
        var b = new HtmlString("<p>b</p>");

        a.ShouldNotBe(b);
        (a == b).ShouldBeFalse();
        (a != b).ShouldBeTrue();
    }

    [Fact]
    public void ShouldNotEqualNonHtmlStringObject()
    {
        var html = new HtmlString("test");

        html.Equals("test").ShouldBeFalse();
    }

    [Fact]
    public void ShouldEqualBoxedHtmlString()
    {
        var html = new HtmlString("test");
        object boxed = new HtmlString("test");

        html.Equals(boxed).ShouldBeTrue();
    }

    [Fact]
    public void ShouldImplicitlyConvertToRenderChunk()
    {
        var html = new HtmlString("<em>emphasis</em>");
        RenderChunk chunk = html;

        chunk.Kind.ShouldBe(RenderChunkKind.Html);
        chunk.GetValue().ShouldBe("<em>emphasis</em>");
    }

    [Fact]
    public void DefaultHtmlStringShouldBeEmpty()
    {
        var html = default(HtmlString);

        html.IsEmpty.ShouldBeTrue();
        html.Value.ShouldBe(string.Empty);
        html.ToString().ShouldBe(string.Empty);
    }
}
