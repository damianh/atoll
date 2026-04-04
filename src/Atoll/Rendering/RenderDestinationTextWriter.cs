using System.Text;

namespace Atoll.Rendering;

/// <summary>
/// A <see cref="TextWriter"/> implementation that bridges RazorSlices output to Atoll's
/// <see cref="IRenderDestination"/> pipeline. All content written through this writer is
/// forwarded as <see cref="RenderChunk.Html(string)"/> chunks, since Razor handles
/// HTML encoding at the template level before writing.
/// </summary>
public sealed class RenderDestinationTextWriter : TextWriter
{
    private readonly IRenderDestination _destination;

    /// <summary>
    /// Initializes a new instance of <see cref="RenderDestinationTextWriter"/>.
    /// </summary>
    /// <param name="destination">The destination to forward output to.</param>
    public RenderDestinationTextWriter(IRenderDestination destination)
    {
        ArgumentNullException.ThrowIfNull(destination);
        _destination = destination;
    }

    /// <inheritdoc />
    public override Encoding Encoding => Encoding.UTF8;

    /// <inheritdoc />
    public override void Write(char value)
    {
        _destination.Write(RenderChunk.Html(value.ToString()));
    }

    /// <inheritdoc />
    public override void Write(string? value)
    {
        if (value is { Length: > 0 })
        {
            _destination.Write(RenderChunk.Html(value));
        }
    }

    /// <inheritdoc />
    public override void Write(char[] buffer, int index, int count)
    {
        if (count > 0)
        {
            _destination.Write(RenderChunk.Html(new string(buffer, index, count)));
        }
    }

    /// <inheritdoc />
    public override void Write(ReadOnlySpan<char> buffer)
    {
        if (buffer.Length > 0)
        {
            _destination.Write(RenderChunk.Html(new string(buffer)));
        }
    }

    /// <inheritdoc />
    public override Task WriteAsync(string? value)
    {
        Write(value);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override Task WriteAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled(cancellationToken);
        }
        Write(buffer.Span);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override Task WriteAsync(char value)
    {
        Write(value);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override Task WriteAsync(char[] buffer, int index, int count)
    {
        Write(buffer, index, count);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override Task WriteLineAsync()
    {
        _destination.Write(RenderChunk.Html(NewLine));
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override Task WriteLineAsync(string? value)
    {
        if (value is { Length: > 0 })
        {
            _destination.Write(RenderChunk.Html(value));
        }
        _destination.Write(RenderChunk.Html(NewLine));
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override Task WriteLineAsync(char value)
    {
        Write(value);
        _destination.Write(RenderChunk.Html(NewLine));
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override Task WriteLineAsync(char[] buffer, int index, int count)
    {
        Write(buffer, index, count);
        _destination.Write(RenderChunk.Html(NewLine));
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override Task WriteLineAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled(cancellationToken);
        }
        Write(buffer.Span);
        _destination.Write(RenderChunk.Html(NewLine));
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override Task FlushAsync() => Task.CompletedTask;

    /// <inheritdoc />
    public override Task FlushAsync(CancellationToken cancellationToken) =>
        cancellationToken.IsCancellationRequested
            ? Task.FromCanceled(cancellationToken)
            : Task.CompletedTask;
}
