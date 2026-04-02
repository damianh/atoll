namespace Atoll.Core.Rendering;

/// <summary>
/// Captures output from a <see cref="RenderFragment"/> into an internal buffer,
/// then flushes the buffered chunks to a real <see cref="IRenderDestination"/> on demand.
/// This is the Atoll equivalent of Astro's <c>createBufferedRenderer</c>.
/// </summary>
/// <remarks>
/// <para>
/// Used by <see cref="InterpolatedTemplate"/> to preserve output order when async
/// expressions are encountered. Once an async expression is hit, all remaining
/// expressions are immediately started (rendered into their own buffers) and then
/// flushed sequentially to the real destination after each resolves.
/// </para>
/// </remarks>
public sealed class BufferedRenderer : IRenderDestination
{
    private readonly List<RenderChunk> _buffer = [];
    private readonly RenderFragment _fragment;
    private ValueTask _renderTask;
    private bool _started;

    /// <summary>
    /// Initializes a new <see cref="BufferedRenderer"/> that will capture output
    /// from the specified <paramref name="fragment"/>.
    /// </summary>
    /// <param name="fragment">The fragment whose output will be buffered.</param>
    public BufferedRenderer(RenderFragment fragment)
    {
        _fragment = fragment;
    }

    /// <inheritdoc />
    public void Write(RenderChunk chunk)
    {
        _buffer.Add(chunk);
    }

    /// <summary>
    /// Starts rendering the fragment into this buffer.
    /// The rendering runs asynchronously; call <see cref="FlushAsync"/> to wait
    /// for completion and write the buffered output to the real destination.
    /// </summary>
    /// <remarks>
    /// This method is idempotent. Calling it multiple times has no effect beyond the first call.
    /// </remarks>
    public void Start()
    {
        if (_started)
        {
            return;
        }

        _started = true;
        _renderTask = _fragment.RenderAsync(this);
    }

    /// <summary>
    /// Waits for the buffered rendering to complete, then writes all captured chunks
    /// to the specified <paramref name="destination"/> in order.
    /// </summary>
    /// <param name="destination">The real destination to flush buffered output to.</param>
    /// <returns>A <see cref="ValueTask"/> representing the flush operation.</returns>
    public async ValueTask FlushAsync(IRenderDestination destination)
    {
        ArgumentNullException.ThrowIfNull(destination);

        if (!_started)
        {
            Start();
        }

        await _renderTask.ConfigureAwait(false);

        foreach (var chunk in _buffer)
        {
            destination.Write(chunk);
        }
    }
}
