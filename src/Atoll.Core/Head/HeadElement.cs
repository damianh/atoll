namespace Atoll.Core.Head;

/// <summary>
/// Represents an HTML element intended for the <c>&lt;head&gt;</c> section of a page.
/// A head element has a tag name, an optional set of attributes, and optional inner content.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="HeadElement"/> is a data-oriented model that can represent any head element:
/// <c>&lt;link&gt;</c>, <c>&lt;meta&gt;</c>, <c>&lt;style&gt;</c>, <c>&lt;script&gt;</c>,
/// <c>&lt;title&gt;</c>, etc. It is used by <see cref="HeadManager"/> and
/// <see cref="HeadDeduplicator"/> for collection and deduplication of head content.
/// </para>
/// <para>
/// Attributes are stored as key-value pairs. A <c>null</c> value represents a boolean
/// attribute (e.g., <c>defer</c>). The <see cref="Content"/> property holds inner text
/// for elements like <c>&lt;style&gt;</c> or <c>&lt;title&gt;</c>.
/// </para>
/// </remarks>
public sealed class HeadElement
{
    private readonly Dictionary<string, string?> _attributes;

    /// <summary>
    /// Initializes a new <see cref="HeadElement"/> with the specified tag name.
    /// </summary>
    /// <param name="tag">The HTML tag name (e.g., "link", "meta", "style", "title").</param>
    public HeadElement(string tag)
        : this(tag, new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase))
    {
    }

    /// <summary>
    /// Initializes a new <see cref="HeadElement"/> with the specified tag name and attributes.
    /// </summary>
    /// <param name="tag">The HTML tag name.</param>
    /// <param name="attributes">The element's attributes.</param>
    public HeadElement(string tag, Dictionary<string, string?> attributes)
    {
        ArgumentNullException.ThrowIfNull(tag);
        ArgumentNullException.ThrowIfNull(attributes);
        Tag = tag;
        _attributes = attributes;
    }

    /// <summary>
    /// Gets the HTML tag name (e.g., "link", "meta", "style", "title").
    /// </summary>
    public string Tag { get; }

    /// <summary>
    /// Gets the element's attributes as a read-only dictionary.
    /// </summary>
    public IReadOnlyDictionary<string, string?> Attributes => _attributes;

    /// <summary>
    /// Gets or sets the inner content of the element (e.g., CSS text for a
    /// <c>&lt;style&gt;</c> element, or title text for a <c>&lt;title&gt;</c> element).
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// Sets an attribute on the element. A <c>null</c> value represents a boolean attribute.
    /// </summary>
    /// <param name="name">The attribute name.</param>
    /// <param name="value">The attribute value, or <c>null</c> for boolean attributes.</param>
    /// <returns>This <see cref="HeadElement"/> for method chaining.</returns>
    public HeadElement SetAttribute(string name, string? value)
    {
        ArgumentNullException.ThrowIfNull(name);
        _attributes[name] = value;
        return this;
    }

    /// <summary>
    /// Gets whether this element is a void element (self-closing, no inner content).
    /// Void elements include: link, meta, base.
    /// </summary>
    public bool IsVoid => Tag is "link" or "meta" or "base";
}
