using System.Text;

namespace Atoll.Rendering;

/// <summary>
/// An <see cref="IRenderDestination"/> that accumulates rendered output into a string.
/// Used for <c>renderToString</c> scenarios (SSG, testing) where the complete
/// output is needed as a single string.
/// </summary>
public sealed class StringRenderDestination : IRenderDestination
{
    private readonly StringBuilder _sb;

    /// <summary>
    /// Initializes a new instance of <see cref="StringRenderDestination"/>.
    /// </summary>
    public StringRenderDestination()
    {
        _sb = new StringBuilder();
    }

    /// <summary>
    /// Initializes a new instance of <see cref="StringRenderDestination"/> with
    /// the specified initial capacity.
    /// </summary>
    /// <param name="capacity">The initial capacity of the internal buffer.</param>
    public StringRenderDestination(int capacity)
    {
        _sb = new StringBuilder(capacity);
    }

    /// <inheritdoc />
    public void Write(RenderChunk chunk)
    {
        _sb.Append(chunk.GetRenderedValue());
    }

    /// <summary>
    /// Gets the accumulated rendered output as a string.
    /// </summary>
    /// <returns>The complete rendered output.</returns>
    public string GetOutput() => _sb.ToString();

    /// <summary>
    /// Resets the destination, clearing all accumulated output.
    /// </summary>
    public void Reset() => _sb.Clear();
}
