using System.Text;
using Atoll.Core.Rendering;

namespace Atoll.Core.Head;

/// <summary>
/// Manages head content collection and deduplication during page rendering.
/// Components add head elements (stylesheets, meta tags, scripts, etc.) during
/// rendering, and the manager deduplicates and renders them at the appropriate
/// injection point.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="HeadManager"/> is typically created once per page render. Components
/// add elements via <see cref="Add"/>, and the page renderer calls
/// <see cref="RenderAllHeadContentAsync"/> to inject all collected head content into
/// the <c>&lt;head&gt;</c> element.
/// </para>
/// <para>
/// Deduplication uses <see cref="HeadDeduplicator.GenerateKey"/> to produce stable
/// keys that are independent of attribute insertion order. For example,
/// <c>&lt;link rel="stylesheet" href="/a.css"&gt;</c> and
/// <c>&lt;link href="/a.css" rel="stylesheet"&gt;</c> produce the same key and
/// only one will be emitted.
/// </para>
/// </remarks>
public sealed class HeadManager
{
    private readonly List<HeadElement> _elements = [];
    private readonly HashSet<string> _seenKeys = [];

    /// <summary>
    /// Adds a head element. If an element with the same deduplication key has already
    /// been added, this call is a no-op.
    /// </summary>
    /// <param name="element">The head element to add.</param>
    /// <returns><c>true</c> if the element was added; <c>false</c> if it was a duplicate.</returns>
    public bool Add(HeadElement element)
    {
        ArgumentNullException.ThrowIfNull(element);

        var key = HeadDeduplicator.GenerateKey(element);
        if (!_seenKeys.Add(key))
        {
            return false;
        }

        _elements.Add(element);
        return true;
    }

    /// <summary>
    /// Gets the number of unique head elements that have been collected.
    /// </summary>
    public int Count => _elements.Count;

    /// <summary>
    /// Gets all collected head elements in insertion order.
    /// </summary>
    /// <returns>A read-only list of head elements.</returns>
    public IReadOnlyList<HeadElement> GetElements()
    {
        return _elements;
    }

    /// <summary>
    /// Renders all collected head content to the specified destination.
    /// Each element is rendered as HTML, separated by newlines.
    /// </summary>
    /// <param name="destination">The render destination.</param>
    /// <returns>A <see cref="ValueTask"/> representing the render operation.</returns>
    public ValueTask RenderAllHeadContentAsync(IRenderDestination destination)
    {
        ArgumentNullException.ThrowIfNull(destination);

        foreach (var element in _elements)
        {
            var html = RenderElementToHtml(element);
            destination.Write(RenderChunk.Html(html));
            destination.Write(RenderChunk.Html("\n"));
        }

        return default;
    }

    /// <summary>
    /// Clears all collected head elements.
    /// </summary>
    public void Clear()
    {
        _elements.Clear();
        _seenKeys.Clear();
    }

    /// <summary>
    /// Renders a <see cref="HeadElement"/> to an HTML string. Attribute values are
    /// HTML-encoded. Boolean attributes (null value) are rendered as name-only.
    /// </summary>
    internal static string RenderElementToHtml(HeadElement element)
    {
        var builder = new StringBuilder();
        builder.Append('<');
        builder.Append(element.Tag);

        foreach (var (name, value) in element.Attributes)
        {
            builder.Append(' ');
            builder.Append(name);
            if (value is not null)
            {
                builder.Append("=\"");
                builder.Append(HtmlEncoder.Encode(value));
                builder.Append('"');
            }
        }

        if (element.IsVoid)
        {
            builder.Append('>');
        }
        else
        {
            builder.Append('>');
            if (element.Content is not null)
            {
                builder.Append(element.Content);
            }

            builder.Append("</");
            builder.Append(element.Tag);
            builder.Append('>');
        }

        return builder.ToString();
    }
}
