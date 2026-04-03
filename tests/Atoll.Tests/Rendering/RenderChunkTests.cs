using Atoll.Rendering;
using Shouldly;
using Xunit;

namespace Atoll.Tests.Rendering;

public sealed class RenderChunkTests
{
    [Fact]
    public void HtmlChunkShouldPreserveContent()
    {
        var chunk = RenderChunk.Html("<h1>Hello</h1>");

        chunk.Kind.ShouldBe(RenderChunkKind.Html);
        chunk.GetValue().ShouldBe("<h1>Hello</h1>");
    }

    [Fact]
    public void HtmlChunkShouldNotEscapeOnRenderedValue()
    {
        var chunk = RenderChunk.Html("<script>alert('xss')</script>");

        chunk.GetRenderedValue().ShouldBe("<script>alert('xss')</script>");
    }

    [Fact]
    public void TextChunkShouldPreserveRawValue()
    {
        var chunk = RenderChunk.Text("<script>alert('xss')</script>");

        chunk.Kind.ShouldBe(RenderChunkKind.Text);
        chunk.GetValue().ShouldBe("<script>alert('xss')</script>");
    }

    [Fact]
    public void TextChunkShouldEscapeOnRenderedValue()
    {
        var chunk = RenderChunk.Text("<script>alert('xss')</script>");

        chunk.GetRenderedValue().ShouldBe("&lt;script&gt;alert(&#39;xss&#39;)&lt;/script&gt;");
    }

    [Fact]
    public void TextChunkShouldEscapeAmpersandAndQuotes()
    {
        var chunk = RenderChunk.Text("A & B \"quoted\"");

        chunk.GetRenderedValue().ShouldBe("A &amp; B &quot;quoted&quot;");
    }

    [Fact]
    public void HtmlChunkShouldThrowForNullHtml()
    {
        Should.Throw<ArgumentNullException>(() => RenderChunk.Html(null!));
    }

    [Fact]
    public void TextChunkShouldThrowForNullText()
    {
        Should.Throw<ArgumentNullException>(() => RenderChunk.Text(null!));
    }

    [Fact]
    public void DefaultChunkShouldReturnEmptyValues()
    {
        var chunk = default(RenderChunk);

        chunk.Kind.ShouldBe(RenderChunkKind.Html);
        chunk.GetValue().ShouldBe(string.Empty);
        chunk.GetRenderedValue().ShouldBe(string.Empty);
    }

    [Fact]
    public void EqualChunksShouldBeEqual()
    {
        var a = RenderChunk.Html("<p>test</p>");
        var b = RenderChunk.Html("<p>test</p>");

        a.ShouldBe(b);
        (a == b).ShouldBeTrue();
        (a != b).ShouldBeFalse();
        a.GetHashCode().ShouldBe(b.GetHashCode());
    }

    [Fact]
    public void DifferentChunksShouldNotBeEqual()
    {
        var html = RenderChunk.Html("content");
        var text = RenderChunk.Text("content");

        html.ShouldNotBe(text);
        (html == text).ShouldBeFalse();
        (html != text).ShouldBeTrue();
    }

    [Fact]
    public void DifferentValueChunksShouldNotBeEqual()
    {
        var a = RenderChunk.Html("<p>a</p>");
        var b = RenderChunk.Html("<p>b</p>");

        a.ShouldNotBe(b);
    }

    [Fact]
    public void ChunkShouldNotEqualNonChunkObject()
    {
        var chunk = RenderChunk.Html("test");

        chunk.Equals("test").ShouldBeFalse();
    }

    [Fact]
    public void ChunkShouldEqualBoxedChunk()
    {
        var chunk = RenderChunk.Html("test");
        object boxed = RenderChunk.Html("test");

        chunk.Equals(boxed).ShouldBeTrue();
    }

    [Fact]
    public void TextChunkWithNoSpecialCharsShouldReturnSameString()
    {
        var chunk = RenderChunk.Text("plain text no specials");

        chunk.GetRenderedValue().ShouldBe("plain text no specials");
    }
}
