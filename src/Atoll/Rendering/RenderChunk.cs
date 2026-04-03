using System.Diagnostics.CodeAnalysis;

namespace Atoll.Rendering;

/// <summary>
/// Represents a chunk of renderable content that can be written to an <see cref="IRenderDestination"/>.
/// This is a discriminated union modeled after Astro's <c>RenderDestinationChunk</c>, which can be:
/// a raw HTML string (trusted), a plain text string (requires escaping), or a render instruction.
/// </summary>
public readonly struct RenderChunk : IEquatable<RenderChunk>
{
    private readonly RenderChunkKind _kind;
    private readonly string? _value;

    private RenderChunk(RenderChunkKind kind, string? value)
    {
        _kind = kind;
        _value = value;
    }

    /// <summary>
    /// Gets the kind of this chunk.
    /// </summary>
    public RenderChunkKind Kind => _kind;

    /// <summary>
    /// Creates a chunk containing trusted HTML content that will not be escaped.
    /// </summary>
    /// <param name="html">The trusted HTML string.</param>
    /// <returns>A new <see cref="RenderChunk"/> containing trusted HTML.</returns>
    public static RenderChunk Html(string html)
    {
        ArgumentNullException.ThrowIfNull(html);
        return new RenderChunk(RenderChunkKind.Html, html);
    }

    /// <summary>
    /// Creates a chunk containing plain text that will be HTML-escaped when rendered.
    /// </summary>
    /// <param name="text">The plain text to escape.</param>
    /// <returns>A new <see cref="RenderChunk"/> containing text to be escaped.</returns>
    public static RenderChunk Text(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        return new RenderChunk(RenderChunkKind.Text, text);
    }

    /// <summary>
    /// Returns the raw string value of this chunk. For <see cref="RenderChunkKind.Html"/> chunks,
    /// this is the trusted HTML. For <see cref="RenderChunkKind.Text"/> chunks, this is the
    /// unescaped text.
    /// </summary>
    /// <returns>The string value, or an empty string if this is a default-constructed chunk.</returns>
    public string GetValue() => _value ?? string.Empty;

    /// <summary>
    /// Gets the rendered string value of this chunk. For <see cref="RenderChunkKind.Text"/> chunks,
    /// the value is HTML-escaped. For <see cref="RenderChunkKind.Html"/> chunks, the value is
    /// returned as-is.
    /// </summary>
    /// <returns>The rendered string representation.</returns>
    public string GetRenderedValue() => _kind switch
    {
        RenderChunkKind.Html => _value ?? string.Empty,
        RenderChunkKind.Text => HtmlEncoder.Encode(_value ?? string.Empty),
        _ => string.Empty,
    };

    /// <inheritdoc />
    public bool Equals(RenderChunk other) =>
        _kind == other._kind && _value == other._value;

    /// <inheritdoc />
    public override bool Equals([NotNullWhen(true)] object? obj) =>
        obj is RenderChunk other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(_kind, _value);

    /// <summary>
    /// Determines whether two <see cref="RenderChunk"/> instances are equal.
    /// </summary>
    public static bool operator ==(RenderChunk left, RenderChunk right) => left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="RenderChunk"/> instances are not equal.
    /// </summary>
    public static bool operator !=(RenderChunk left, RenderChunk right) => !left.Equals(right);
}
