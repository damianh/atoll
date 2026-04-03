using Atoll.Components;
using Atoll.Rendering;

namespace Atoll.Islands;

/// <summary>
/// Bridges the Atoll island protocol with the custom element lifecycle.
/// Provides utilities for rendering Web Components as islands, including
/// custom element tag generation and shadow DOM / light DOM handling.
/// </summary>
/// <remarks>
/// <para>
/// This is the Atoll equivalent of Astro's Web Component integration.
/// A Web Component island renders a custom element tag during SSR with the
/// component's content as light DOM. On the client, the custom element
/// definition is loaded via the component's module URL and the browser
/// upgrades the element.
/// </para>
/// <para>
/// The adapter handles:
/// </para>
/// <list type="bullet">
/// <item>Custom element tag name validation (must contain a hyphen per spec)</item>
/// <item>Prop serialization as HTML attributes or as JSON via the island wrapper</item>
/// <item>SSR content rendered inside the custom element tag</item>
/// </list>
/// </remarks>
public static class WebComponentAdapter
{
    /// <summary>
    /// Validates that the specified tag name is a valid custom element name
    /// per the HTML specification (must contain a hyphen and not start with a digit).
    /// </summary>
    /// <param name="tagName">The custom element tag name to validate.</param>
    /// <returns><c>true</c> if the tag name is valid; otherwise, <c>false</c>.</returns>
    public static bool IsValidCustomElementName(string tagName)
    {
        ArgumentNullException.ThrowIfNull(tagName);

        if (tagName.Length == 0)
        {
            return false;
        }

        // Must contain at least one hyphen
        if (!tagName.Contains('-'))
        {
            return false;
        }

        // Must not start with a digit
        if (char.IsDigit(tagName[0]))
        {
            return false;
        }

        // Must be lowercase
        if (tagName != tagName.ToLowerInvariant())
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Generates an opening custom element tag with optional HTML attributes
    /// derived from the component's props.
    /// </summary>
    /// <param name="tagName">The custom element tag name.</param>
    /// <param name="props">
    /// The props to render as HTML attributes. Only string, numeric, and boolean
    /// prop values are rendered as attributes. Complex objects are skipped.
    /// </param>
    /// <returns>The opening HTML tag string.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="tagName"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="tagName"/> is not a valid custom element name.
    /// </exception>
    public static string GenerateOpeningTag(string tagName, IReadOnlyDictionary<string, object?> props)
    {
        ArgumentNullException.ThrowIfNull(tagName);
        ArgumentNullException.ThrowIfNull(props);

        if (!IsValidCustomElementName(tagName))
        {
            throw new ArgumentException(
                $"'{tagName}' is not a valid custom element name. " +
                "Custom element names must contain a hyphen, be lowercase, and not start with a digit.",
                nameof(tagName));
        }

        if (props.Count == 0)
        {
            return $"<{tagName}>";
        }

        var attributes = new System.Text.StringBuilder();
        attributes.Append('<');
        attributes.Append(tagName);

        foreach (var (key, value) in props)
        {
            if (value is null)
            {
                continue;
            }

            var attributeValue = ConvertToAttribute(value);
            if (attributeValue is null)
            {
                continue;
            }

            attributes.Append(' ');
            attributes.Append(key);
            attributes.Append("=\"");
            attributes.Append(System.Net.WebUtility.HtmlEncode(attributeValue));
            attributes.Append('"');
        }

        attributes.Append('>');
        return attributes.ToString();
    }

    /// <summary>
    /// Generates an opening custom element tag with no attributes.
    /// </summary>
    /// <param name="tagName">The custom element tag name.</param>
    /// <returns>The opening HTML tag string.</returns>
    public static string GenerateOpeningTag(string tagName)
    {
        ArgumentNullException.ThrowIfNull(tagName);

        if (!IsValidCustomElementName(tagName))
        {
            throw new ArgumentException(
                $"'{tagName}' is not a valid custom element name. " +
                "Custom element names must contain a hyphen, be lowercase, and not start with a digit.",
                nameof(tagName));
        }

        return $"<{tagName}>";
    }

    /// <summary>
    /// Generates a closing custom element tag.
    /// </summary>
    /// <param name="tagName">The custom element tag name.</param>
    /// <returns>The closing HTML tag string.</returns>
    public static string GenerateClosingTag(string tagName)
    {
        ArgumentNullException.ThrowIfNull(tagName);

        return $"</{tagName}>";
    }

    private static string? ConvertToAttribute(object value)
    {
        return value switch
        {
            string s => s,
            bool b => b ? "true" : "false",
            int or long or float or double or decimal or short or byte => value.ToString(),
            _ => null // Complex objects are not rendered as attributes
        };
    }
}
