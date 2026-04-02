namespace Atoll.Core.Rendering;

/// <summary>
/// Defines the contract for a destination that receives rendered output chunks.
/// This is the Atoll equivalent of Astro's <c>RenderDestination</c>.
/// Implementations include string buffers (for <c>renderToString</c>),
/// stream writers (for streaming SSR), and buffered renderers (for async order preservation).
/// </summary>
public interface IRenderDestination
{
    /// <summary>
    /// Writes a chunk of rendered content to the destination.
    /// </summary>
    /// <param name="chunk">The chunk of content to write.</param>
    void Write(RenderChunk chunk);
}
