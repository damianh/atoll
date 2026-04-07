using System.Text;

namespace Atoll.Rendering;

/// <summary>
/// Contains the result of a page render operation. Provides the rendered HTML
/// as a string and supports writing to a stream.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="PageRenderResult"/> is produced by <see cref="PageRenderer"/> after
/// the page component has been rendered, head content has been collected and injected,
/// and the DOCTYPE has been ensured. The final HTML output can be retrieved as a
/// string via <see cref="Html"/> or written to a stream via <see cref="WriteToStreamAsync(Stream)"/>.
/// </para>
/// </remarks>
public sealed class PageRenderResult
{
    /// <summary>
    /// Initializes a new <see cref="PageRenderResult"/> with the specified HTML content
    /// and a default HTTP status code of 200.
    /// </summary>
    /// <param name="html">The fully rendered HTML page content.</param>
    public PageRenderResult(string html)
        : this(html, 200)
    {
    }

    /// <summary>
    /// Initializes a new <see cref="PageRenderResult"/> with the specified HTML content
    /// and HTTP status code.
    /// </summary>
    /// <param name="html">The fully rendered HTML page content.</param>
    /// <param name="statusCode">The HTTP status code to return with the response.</param>
    public PageRenderResult(string html, int statusCode)
    {
        ArgumentNullException.ThrowIfNull(html);
        Html = html;
        StatusCode = statusCode;
    }

    /// <summary>
    /// Gets the fully rendered HTML page content, including DOCTYPE, head content,
    /// and body content.
    /// </summary>
    public string Html { get; }

    /// <summary>
    /// Gets the HTTP status code for the page response. Defaults to 200.
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// Writes the rendered HTML to the specified stream using UTF-8 encoding.
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
    /// <returns>A <see cref="Task"/> representing the write operation.</returns>
    public Task WriteToStreamAsync(Stream stream)
    {
        return WriteToStreamAsync(stream, Encoding.UTF8);
    }

    /// <summary>
    /// Writes the rendered HTML to the specified stream using the given encoding.
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
    /// <param name="encoding">The character encoding to use.</param>
    /// <returns>A <see cref="Task"/> representing the write operation.</returns>
    public async Task WriteToStreamAsync(Stream stream, Encoding encoding)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(encoding);

        var bytes = encoding.GetBytes(Html);
        await stream.WriteAsync(bytes).ConfigureAwait(false);
    }
}
