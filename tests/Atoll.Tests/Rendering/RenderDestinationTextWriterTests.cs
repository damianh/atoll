using Atoll.Rendering;

namespace Atoll.Tests.Rendering;

public sealed class RenderDestinationTextWriterTests
{
    [Fact]
    public void EncodingShouldBeUtf8()
    {
        var destination = new StringRenderDestination();
        using var writer = new RenderDestinationTextWriter(destination);

        writer.Encoding.ShouldBe(System.Text.Encoding.UTF8);
    }

    [Fact]
    public void WriteStringShouldForwardAsHtmlChunk()
    {
        var destination = new StringRenderDestination();
        using var writer = new RenderDestinationTextWriter(destination);

        writer.Write("<p>Hello</p>");

        destination.GetOutput().ShouldBe("<p>Hello</p>");
    }

    [Fact]
    public void WriteCharShouldForwardAsHtmlChunk()
    {
        var destination = new StringRenderDestination();
        using var writer = new RenderDestinationTextWriter(destination);

        writer.Write('<');
        writer.Write('p');
        writer.Write('>');

        destination.GetOutput().ShouldBe("<p>");
    }

    [Fact]
    public void WriteCharArrayShouldForwardAsHtmlChunk()
    {
        var destination = new StringRenderDestination();
        using var writer = new RenderDestinationTextWriter(destination);
        var buffer = "<h1>Title</h1>".ToCharArray();

        writer.Write(buffer, 0, buffer.Length);

        destination.GetOutput().ShouldBe("<h1>Title</h1>");
    }

    [Fact]
    public void WriteSpanShouldForwardAsHtmlChunk()
    {
        var destination = new StringRenderDestination();
        using var writer = new RenderDestinationTextWriter(destination);

        writer.Write("<span>test</span>".AsSpan());

        destination.GetOutput().ShouldBe("<span>test</span>");
    }

    [Fact]
    public void WriteNullStringShouldWriteNothing()
    {
        var destination = new StringRenderDestination();
        using var writer = new RenderDestinationTextWriter(destination);

        writer.Write((string?)null);

        destination.GetOutput().ShouldBe(string.Empty);
    }

    [Fact]
    public void WriteEmptyStringShouldWriteNothing()
    {
        var destination = new StringRenderDestination();
        using var writer = new RenderDestinationTextWriter(destination);

        writer.Write(string.Empty);

        destination.GetOutput().ShouldBe(string.Empty);
    }

    [Fact]
    public async Task WriteAsyncStringShouldForwardAsHtmlChunk()
    {
        var destination = new StringRenderDestination();
        await using var writer = new RenderDestinationTextWriter(destination);

        await writer.WriteAsync("<div>async</div>");

        destination.GetOutput().ShouldBe("<div>async</div>");
    }

    [Fact]
    public async Task WriteAsyncMemoryShouldForwardAsHtmlChunk()
    {
        var destination = new StringRenderDestination();
        await using var writer = new RenderDestinationTextWriter(destination);

        await writer.WriteAsync("<em>memory</em>".AsMemory());

        destination.GetOutput().ShouldBe("<em>memory</em>");
    }

    [Fact]
    public async Task WriteAsyncCharShouldForwardAsHtmlChunk()
    {
        var destination = new StringRenderDestination();
        await using var writer = new RenderDestinationTextWriter(destination);

        await writer.WriteAsync('X');

        destination.GetOutput().ShouldBe("X");
    }

    [Fact]
    public async Task WriteAsyncCharArrayShouldForwardAsHtmlChunk()
    {
        var destination = new StringRenderDestination();
        await using var writer = new RenderDestinationTextWriter(destination);
        var buffer = "chars".ToCharArray();

        await writer.WriteAsync(buffer, 0, buffer.Length);

        destination.GetOutput().ShouldBe("chars");
    }

    [Fact]
    public async Task WriteLineAsyncShouldAppendNewLine()
    {
        var destination = new StringRenderDestination();
        await using var writer = new RenderDestinationTextWriter(destination);

        await writer.WriteLineAsync();

        destination.GetOutput().ShouldBe(writer.NewLine);
    }

    [Fact]
    public async Task WriteLineAsyncStringShouldAppendValueAndNewLine()
    {
        var destination = new StringRenderDestination();
        await using var writer = new RenderDestinationTextWriter(destination);

        await writer.WriteLineAsync("<p>line</p>");

        destination.GetOutput().ShouldBe("<p>line</p>" + writer.NewLine);
    }

    [Fact]
    public async Task WriteLineAsyncNullStringShouldAppendOnlyNewLine()
    {
        var destination = new StringRenderDestination();
        await using var writer = new RenderDestinationTextWriter(destination);

        await writer.WriteLineAsync((string?)null);

        destination.GetOutput().ShouldBe(writer.NewLine);
    }

    [Fact]
    public async Task WriteLineAsyncMemoryShouldAppendValueAndNewLine()
    {
        var destination = new StringRenderDestination();
        await using var writer = new RenderDestinationTextWriter(destination);

        await writer.WriteLineAsync("<li>item</li>".AsMemory());

        destination.GetOutput().ShouldBe("<li>item</li>" + writer.NewLine);
    }

    [Fact]
    public async Task FlushAsyncShouldCompleteSuccessfully()
    {
        var destination = new StringRenderDestination();
        await using var writer = new RenderDestinationTextWriter(destination);

        await writer.FlushAsync();

        // No exception — flush is a no-op since writes are synchronous
    }

    [Fact]
    public async Task WriteAsyncShouldRespectCancellation()
    {
        var destination = new StringRenderDestination();
        await using var writer = new RenderDestinationTextWriter(destination);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Should.ThrowAsync<TaskCanceledException>(
            async () => await writer.WriteAsync("<p>cancelled</p>".AsMemory(), cts.Token));
    }

    [Fact]
    public void AllWritesShouldProduceHtmlChunksNotTextChunks()
    {
        // Verify that content written through the bridge is always treated as trusted HTML
        // (i.e. angle brackets are NOT re-escaped). Razor handles escaping before writing.
        var destination = new StringRenderDestination();
        using var writer = new RenderDestinationTextWriter(destination);

        writer.Write("<script>alert('xss')</script>");

        // The string is forwarded as-is (trusted HTML), not double-escaped
        destination.GetOutput().ShouldBe("<script>alert('xss')</script>");
    }

    [Fact]
    public void MultipleWritesShouldConcatenateInOrder()
    {
        var destination = new StringRenderDestination();
        using var writer = new RenderDestinationTextWriter(destination);

        writer.Write("<ul>");
        writer.Write("<li>one</li>");
        writer.Write("<li>two</li>");
        writer.Write("</ul>");

        destination.GetOutput().ShouldBe("<ul><li>one</li><li>two</li></ul>");
    }
}
