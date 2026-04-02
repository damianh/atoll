using System.Diagnostics.CodeAnalysis;

namespace Atoll.Core.Rendering;

/// <summary>
/// Represents a string that contains trusted HTML content and should not be escaped.
/// This is the Atoll equivalent of Astro's <c>HTMLString</c> / <c>markHTMLString()</c>.
/// Use <see cref="HtmlString"/> to mark pre-escaped or trusted HTML content
/// that should be rendered as-is without further encoding.
/// </summary>
public readonly struct HtmlString : IEquatable<HtmlString>
{
    private readonly string? _value;

    /// <summary>
    /// Initializes a new instance of <see cref="HtmlString"/> with the specified trusted HTML content.
    /// </summary>
    /// <param name="value">The trusted HTML content.</param>
    public HtmlString(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        _value = value;
    }

    /// <summary>
    /// Gets the trusted HTML content.
    /// </summary>
    public string Value => _value ?? string.Empty;

    /// <summary>
    /// Gets a value indicating whether this <see cref="HtmlString"/> is empty.
    /// </summary>
    public bool IsEmpty => string.IsNullOrEmpty(_value);

    /// <summary>
    /// Returns the trusted HTML content as a string.
    /// </summary>
    /// <returns>The HTML string value.</returns>
    public override string ToString() => Value;

    /// <summary>
    /// Converts this <see cref="HtmlString"/> to a <see cref="RenderChunk"/>
    /// of kind <see cref="RenderChunkKind.Html"/>.
    /// </summary>
    /// <returns>A render chunk containing this trusted HTML.</returns>
    public RenderChunk ToChunk() => RenderChunk.Html(Value);

    /// <summary>
    /// Converts this <see cref="HtmlString"/> to a <see cref="RenderFragment"/>
    /// that writes this HTML to a destination.
    /// </summary>
    /// <returns>A render fragment that writes this HTML.</returns>
    public RenderFragment ToFragment() => RenderFragment.FromHtml(Value);

    /// <summary>
    /// An empty <see cref="HtmlString"/>.
    /// </summary>
    public static readonly HtmlString Empty = new(string.Empty);

    /// <inheritdoc />
    public bool Equals(HtmlString other) => Value == other.Value;

    /// <inheritdoc />
    public override bool Equals([NotNullWhen(true)] object? obj) =>
        obj is HtmlString other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    /// <summary>
    /// Determines whether two <see cref="HtmlString"/> instances are equal.
    /// </summary>
    public static bool operator ==(HtmlString left, HtmlString right) => left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="HtmlString"/> instances are not equal.
    /// </summary>
    public static bool operator !=(HtmlString left, HtmlString right) => !left.Equals(right);

    /// <summary>
    /// Implicitly converts a <see cref="HtmlString"/> to a <see cref="RenderChunk"/>.
    /// </summary>
    public static implicit operator RenderChunk(HtmlString htmlString) => htmlString.ToChunk();
}
