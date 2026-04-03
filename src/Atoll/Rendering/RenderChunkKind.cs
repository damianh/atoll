namespace Atoll.Rendering;

/// <summary>
/// Identifies the kind of content in a <see cref="RenderChunk"/>.
/// </summary>
public enum RenderChunkKind
{
    /// <summary>
    /// Trusted HTML content that should not be escaped.
    /// </summary>
    Html = 0,

    /// <summary>
    /// Plain text content that must be HTML-escaped before output.
    /// </summary>
    Text = 1,
}
