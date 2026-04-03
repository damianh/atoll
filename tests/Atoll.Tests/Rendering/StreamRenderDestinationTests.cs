using System.IO.Pipelines;
using System.Text;
using Atoll.Rendering;
using Shouldly;
using Xunit;

namespace Atoll.Tests.Rendering;

public sealed class StreamRenderDestinationTests
{
    [Fact]
    public async Task ShouldWriteHtmlChunkToStream()
    {
        using var ms = new MemoryStream();
        var destination = new StreamRenderDestination(ms);

        destination.Write(RenderChunk.Html("<h1>Hello</h1>"));
        await destination.FlushAsync();
        await destination.CompleteAsync();

        var result = Encoding.UTF8.GetString(ms.ToArray());
        result.ShouldBe("<h1>Hello</h1>");
    }

    [Fact]
    public async Task ShouldEscapeTextChunkInStream()
    {
        using var ms = new MemoryStream();
        var destination = new StreamRenderDestination(ms);

        destination.Write(RenderChunk.Text("<script>alert('xss')</script>"));
        await destination.FlushAsync();
        await destination.CompleteAsync();

        var result = Encoding.UTF8.GetString(ms.ToArray());
        result.ShouldBe("&lt;script&gt;alert(&#39;xss&#39;)&lt;/script&gt;");
    }

    [Fact]
    public async Task ShouldWriteMultipleChunksInOrder()
    {
        using var ms = new MemoryStream();
        var destination = new StreamRenderDestination(ms);

        destination.Write(RenderChunk.Html("<div>"));
        destination.Write(RenderChunk.Text("Hello & World"));
        destination.Write(RenderChunk.Html("</div>"));
        await destination.FlushAsync();
        await destination.CompleteAsync();

        var result = Encoding.UTF8.GetString(ms.ToArray());
        result.ShouldBe("<div>Hello &amp; World</div>");
    }

    [Fact]
    public async Task ShouldWorkWithPipeWriter()
    {
        var pipe = new Pipe();
        var destination = new StreamRenderDestination(pipe.Writer);

        destination.Write(RenderChunk.Html("<p>PipeWriter test</p>"));
        await destination.FlushAsync();
        await destination.CompleteAsync();

        var readResult = await pipe.Reader.ReadAsync();
        var result = Encoding.UTF8.GetString(readResult.Buffer);
        pipe.Reader.AdvanceTo(readResult.Buffer.End);

        result.ShouldBe("<p>PipeWriter test</p>");
    }

    [Fact]
    public async Task ShouldRenderFragmentToStream()
    {
        using var ms = new MemoryStream();
        var destination = new StreamRenderDestination(ms);

        var fragment = RenderFragment.FromHtml("<h1>Stream Fragment</h1>");
        await fragment.RenderAsync(destination);
        await destination.FlushAsync();
        await destination.CompleteAsync();

        var result = Encoding.UTF8.GetString(ms.ToArray());
        result.ShouldBe("<h1>Stream Fragment</h1>");
    }

    [Fact]
    public async Task ShouldRenderInterpolatedTemplateToStream()
    {
        using var ms = new MemoryStream();
        var destination = new StreamRenderDestination(ms);

        var template = new InterpolatedTemplate(
            ["<div>", " and ", "</div>"],
            [RenderFragment.FromHtml("A"), RenderFragment.FromHtml("B")]);

        await template.ToRenderFragment().RenderAsync(destination);
        await destination.FlushAsync();
        await destination.CompleteAsync();

        var result = Encoding.UTF8.GetString(ms.ToArray());
        result.ShouldBe("<div>A and B</div>");
    }

    [Fact]
    public async Task ShouldHandleAsyncFragmentWithStream()
    {
        using var ms = new MemoryStream();
        var destination = new StreamRenderDestination(ms);

        var template = new InterpolatedTemplate(
            ["<div>", "</div>"],
            [RenderFragment.FromAsync(async dest =>
            {
                await Task.Delay(5);
                dest.Write(RenderChunk.Html("async-in-stream"));
            })]);

        await template.ToRenderFragment().RenderAsync(destination);
        await destination.FlushAsync();
        await destination.CompleteAsync();

        var result = Encoding.UTF8.GetString(ms.ToArray());
        result.ShouldBe("<div>async-in-stream</div>");
    }

    [Fact]
    public async Task ShouldWriteEmptyChunkWithoutError()
    {
        using var ms = new MemoryStream();
        var destination = new StreamRenderDestination(ms);

        destination.Write(RenderChunk.Html(""));
        await destination.FlushAsync();

        var result = Encoding.UTF8.GetString(ms.ToArray());
        result.ShouldBe(string.Empty);

        await destination.CompleteAsync();
    }

    [Fact]
    public void ShouldThrowForNullStream()
    {
        Should.Throw<ArgumentNullException>(() =>
            new StreamRenderDestination((Stream)null!));
    }

    [Fact]
    public void ShouldThrowForNullPipeWriter()
    {
        Should.Throw<ArgumentNullException>(() =>
            new StreamRenderDestination((PipeWriter)null!));
    }

    [Fact]
    public void ShouldThrowForNullEncodingWithStream()
    {
        using var ms = new MemoryStream();
        Should.Throw<ArgumentNullException>(() =>
            new StreamRenderDestination(ms, null!));
    }

    [Fact]
    public void ShouldThrowForNullEncodingWithPipeWriter()
    {
        var pipe = new Pipe();
        Should.Throw<ArgumentNullException>(() =>
            new StreamRenderDestination(pipe.Writer, null!));
    }

    [Fact]
    public async Task ShouldSupportCustomEncoding()
    {
        using var ms = new MemoryStream();
        var destination = new StreamRenderDestination(ms, Encoding.ASCII);

        destination.Write(RenderChunk.Html("ASCII test"));
        await destination.FlushAsync();
        await destination.CompleteAsync();

        var result = Encoding.ASCII.GetString(ms.ToArray());
        result.ShouldBe("ASCII test");
    }

    [Fact]
    public async Task FlushWithCancellationTokenShouldWork()
    {
        using var ms = new MemoryStream();
        var destination = new StreamRenderDestination(ms);

        destination.Write(RenderChunk.Html("<p>cancellable</p>"));
        await destination.FlushAsync(CancellationToken.None);
        await destination.CompleteAsync();

        var result = Encoding.UTF8.GetString(ms.ToArray());
        result.ShouldBe("<p>cancellable</p>");
    }
}
