using System.Text;

namespace Atoll.Head;

/// <summary>
/// Generates stable deduplication keys for <see cref="HeadElement"/> instances.
/// Two head elements with the same tag, attributes (regardless of insertion order),
/// and content produce the same key.
/// </summary>
/// <remarks>
/// <para>
/// This is the Atoll equivalent of Astro's stable-props-key algorithm. The key
/// is constructed by sorting attribute names alphabetically and concatenating them
/// with their values, ensuring that attribute insertion order does not affect the key.
/// </para>
/// <para>
/// For elements that are considered unique by a specific attribute (e.g., <c>&lt;meta&gt;</c>
/// elements are keyed by <c>name</c> or <c>property</c>), the deduplicator uses a
/// well-known attribute as the primary key component instead of hashing all attributes.
/// </para>
/// </remarks>
public static class HeadDeduplicator
{
    /// <summary>
    /// Generates a stable deduplication key for the specified head element.
    /// </summary>
    /// <param name="element">The head element.</param>
    /// <returns>A stable string key suitable for set-based deduplication.</returns>
    public static string GenerateKey(HeadElement element)
    {
        ArgumentNullException.ThrowIfNull(element);

        // Well-known keying strategies for specific element types.
        // These match how browsers and Astro deduplicate head content.
        return element.Tag switch
        {
            "title" => "title",
            "meta" => GenerateMetaKey(element),
            "link" => GenerateLinkKey(element),
            _ => GenerateGenericKey(element),
        };
    }

    private static string GenerateMetaKey(HeadElement element)
    {
        // Meta elements are keyed by their identifying attribute:
        // - name (e.g., <meta name="description">)
        // - property (e.g., <meta property="og:title">)
        // - http-equiv (e.g., <meta http-equiv="content-type">)
        // - charset (e.g., <meta charset="utf-8">)
        if (element.Attributes.TryGetValue("name", out var name) && name is not null)
        {
            return $"meta:name:{name}";
        }

        if (element.Attributes.TryGetValue("property", out var property) && property is not null)
        {
            return $"meta:property:{property}";
        }

        if (element.Attributes.TryGetValue("http-equiv", out var httpEquiv) && httpEquiv is not null)
        {
            return $"meta:http-equiv:{httpEquiv}";
        }

        if (element.Attributes.ContainsKey("charset"))
        {
            return "meta:charset";
        }

        return GenerateGenericKey(element);
    }

    private static string GenerateLinkKey(HeadElement element)
    {
        // Link elements are keyed by rel + href.
        // e.g., <link rel="stylesheet" href="/a.css"> → "link:stylesheet:/a.css"
        var rel = element.Attributes.TryGetValue("rel", out var r) ? r : null;
        var href = element.Attributes.TryGetValue("href", out var h) ? h : null;

        if (rel is not null && href is not null)
        {
            return $"link:{rel}:{href}";
        }

        return GenerateGenericKey(element);
    }

    private static string GenerateGenericKey(HeadElement element)
    {
        // Generic key: tag + sorted attributes + content.
        // Sorting ensures insertion order doesn't affect the key.
        var builder = new StringBuilder();
        builder.Append(element.Tag);

        var sortedKeys = element.Attributes.Keys
            .OrderBy(k => k, StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var key in sortedKeys)
        {
            builder.Append('|');
            builder.Append(key);
            if (element.Attributes[key] is { } value)
            {
                builder.Append('=');
                builder.Append(value);
            }
        }

        if (element.Content is not null)
        {
            builder.Append("||");
            builder.Append(element.Content);
        }

        return builder.ToString();
    }
}
