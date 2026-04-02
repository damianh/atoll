using Atoll.Core.Rendering;
using Shouldly;
using Xunit;

namespace Atoll.Core.Tests.Rendering;

public sealed class StringRenderDestinationTests
{
    [Fact]
    public void ShouldAccumulateHtmlChunks()
    {
        var destination = new StringRenderDestination();

        destination.Write(RenderChunk.Html("<h1>"));
        destination.Write(RenderChunk.Html("Title"));
        destination.Write(RenderChunk.Html("</h1>"));

        destination.GetOutput().ShouldBe("<h1>Title</h1>");
    }

    [Fact]
    public void ShouldEscapeTextChunks()
    {
        var destination = new StringRenderDestination();

        destination.Write(RenderChunk.Html("<p>"));
        destination.Write(RenderChunk.Text("Hello <World> & \"Friends\""));
        destination.Write(RenderChunk.Html("</p>"));

        destination.GetOutput().ShouldBe("<p>Hello &lt;World&gt; &amp; &quot;Friends&quot;</p>");
    }

    [Fact]
    public void ShouldReturnEmptyStringWhenNothingWritten()
    {
        var destination = new StringRenderDestination();

        destination.GetOutput().ShouldBe(string.Empty);
    }

    [Fact]
    public void ResetShouldClearAccumulatedOutput()
    {
        var destination = new StringRenderDestination();

        destination.Write(RenderChunk.Html("<p>first</p>"));
        destination.Reset();
        destination.Write(RenderChunk.Html("<p>second</p>"));

        destination.GetOutput().ShouldBe("<p>second</p>");
    }

    [Fact]
    public void ShouldHandleMixedHtmlAndTextChunks()
    {
        var destination = new StringRenderDestination();

        destination.Write(RenderChunk.Html("<div class=\"container\">"));
        destination.Write(RenderChunk.Text("User input: <b>bold</b>"));
        destination.Write(RenderChunk.Html("</div>"));

        destination.GetOutput().ShouldBe(
            "<div class=\"container\">User input: &lt;b&gt;bold&lt;/b&gt;</div>");
    }

    [Fact]
    public void ConstructorWithCapacityShouldWork()
    {
        var destination = new StringRenderDestination(1024);

        destination.Write(RenderChunk.Html("<p>test</p>"));

        destination.GetOutput().ShouldBe("<p>test</p>");
    }

    [Fact]
    public void ShouldHandleLargeNumberOfChunks()
    {
        var destination = new StringRenderDestination();

        for (var i = 0; i < 1000; i++)
        {
            destination.Write(RenderChunk.Html($"<li>{i}</li>"));
        }

        var output = destination.GetOutput();

        output.ShouldContain("<li>0</li>");
        output.ShouldContain("<li>999</li>");
    }
}
