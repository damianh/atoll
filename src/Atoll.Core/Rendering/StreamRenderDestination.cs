using System.IO.Pipelines;
using System.Text;

namespace Atoll.Core.Rendering;

/// <summary>
/// An <see cref="IRenderDestination"/> that writes rendered output to a <see cref="Stream"/>
/// or <see cref="PipeWriter"/>. Used for streaming SSR where content is sent to the client
/// incrementally as it is rendered.
/// </summary>
public sealed class StreamRenderDestination : IRenderDestination
{
    private readonly PipeWriter _writer;
    private readonly Encoding _encoding;

    /// <summary>
    /// Initializes a new <see cref="StreamRenderDestination"/> that writes to the specified
    /// <paramref name="stream"/> using UTF-8 encoding.
    /// </summary>
    /// <param name="stream">The stream to write rendered output to.</param>
    public StreamRenderDestination(Stream stream)
        : this(stream, Encoding.UTF8)
    {
    }

    /// <summary>
    /// Initializes a new <see cref="StreamRenderDestination"/> that writes to the specified
    /// <paramref name="stream"/> using the given <paramref name="encoding"/>.
    /// </summary>
    /// <param name="stream">The stream to write rendered output to.</param>
    /// <param name="encoding">The character encoding to use.</param>
    public StreamRenderDestination(Stream stream, Encoding encoding)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(encoding);
        _writer = PipeWriter.Create(stream);
        _encoding = encoding;
    }

    /// <summary>
    /// Initializes a new <see cref="StreamRenderDestination"/> that writes to the specified
    /// <paramref name="pipeWriter"/> using UTF-8 encoding.
    /// </summary>
    /// <param name="pipeWriter">The pipe writer to write rendered output to.</param>
    public StreamRenderDestination(PipeWriter pipeWriter)
        : this(pipeWriter, Encoding.UTF8)
    {
    }

    /// <summary>
    /// Initializes a new <see cref="StreamRenderDestination"/> that writes to the specified
    /// <paramref name="pipeWriter"/> using the given <paramref name="encoding"/>.
    /// </summary>
    /// <param name="pipeWriter">The pipe writer to write rendered output to.</param>
    /// <param name="encoding">The character encoding to use.</param>
    public StreamRenderDestination(PipeWriter pipeWriter, Encoding encoding)
    {
        ArgumentNullException.ThrowIfNull(pipeWriter);
        ArgumentNullException.ThrowIfNull(encoding);
        _writer = pipeWriter;
        _encoding = encoding;
    }

    /// <inheritdoc />
    public void Write(RenderChunk chunk)
    {
        var rendered = chunk.GetRenderedValue();
        if (rendered.Length == 0)
        {
            return;
        }

        var byteCount = _encoding.GetByteCount(rendered);
        var span = _writer.GetSpan(byteCount);
        var written = _encoding.GetBytes(rendered, span);
        _writer.Advance(written);
    }

    /// <summary>
    /// Flushes any buffered data to the underlying stream or pipe.
    /// Call this after rendering is complete to ensure all output has been written.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the flush operation.</param>
    /// <returns>A <see cref="ValueTask{FlushResult}"/> representing the flush operation.</returns>
    public ValueTask<FlushResult> FlushAsync(CancellationToken cancellationToken)
    {
        return _writer.FlushAsync(cancellationToken);
    }

    /// <summary>
    /// Flushes any buffered data to the underlying stream or pipe.
    /// Call this after rendering is complete to ensure all output has been written.
    /// </summary>
    /// <returns>A <see cref="ValueTask{FlushResult}"/> representing the flush operation.</returns>
    public ValueTask<FlushResult> FlushAsync()
    {
        return _writer.FlushAsync();
    }

    /// <summary>
    /// Marks the pipe writer as complete, signaling no more data will be written.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the completion.</returns>
    public ValueTask CompleteAsync()
    {
        return _writer.CompleteAsync();
    }
}
