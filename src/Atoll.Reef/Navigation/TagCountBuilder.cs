using Atoll.Reef.Components;

namespace Atoll.Reef.Navigation;

/// <summary>
/// Builds a <see cref="TagCount"/> list from a collection of articles,
/// counting tag occurrences case-insensitively and deduplicating them.
/// </summary>
public static class TagCountBuilder
{
    /// <summary>
    /// Extracts all unique tags with counts from the given article list.
    /// Tags are deduplicated case-insensitively; the first encountered casing is preserved.
    /// Results are ordered by descending count, then by tag name.
    /// </summary>
    /// <param name="articles">The articles to extract tags from.</param>
    /// <returns>A list of <see cref="TagCount"/> sorted by count descending.</returns>
    public static IReadOnlyList<TagCount> Build(IReadOnlyList<ArticleListItem> articles)
    {
        var counts = new Dictionary<string, (string Display, int Count)>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var article in articles)
        {
            foreach (var tag in article.Tags)
            {
                if (counts.TryGetValue(tag, out var existing))
                {
                    counts[tag] = (existing.Display, existing.Count + 1);
                }
                else
                {
                    counts[tag] = (tag, 1);
                }
            }
        }

        return counts.Values
            .OrderByDescending(v => v.Count)
            .ThenBy(v => v.Display, StringComparer.OrdinalIgnoreCase)
            .Select(v => new TagCount(v.Display, v.Count))
            .ToList();
    }
}
