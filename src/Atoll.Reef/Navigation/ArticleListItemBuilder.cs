using Atoll.Build.Content.Collections;
using Atoll.Reef.Components;
using Atoll.Reef.Configuration;

namespace Atoll.Reef.Navigation;

/// <summary>
/// Converts a collection of <see cref="ContentEntry{TData}"/> with <see cref="ArticleSchema"/>
/// frontmatter into <see cref="ArticleListItem"/> instances suitable for use in Reef components.
/// </summary>
public static class ArticleListItemBuilder
{
    /// <summary>
    /// Builds a list of <see cref="ArticleListItem"/> from content entries.
    /// Reading time is taken from <see cref="ArticleSchema.ReadingTimeMinutes"/> when set;
    /// otherwise it is calculated from the entry body via <see cref="ReadingTimeCalculator"/>.
    /// </summary>
    /// <param name="entries">The content entries to convert.</param>
    /// <returns>A list of <see cref="ArticleListItem"/> in the same order as <paramref name="entries"/>.</returns>
    public static IReadOnlyList<ArticleListItem> Build(
        IReadOnlyList<ContentEntry<ArticleSchema>> entries)
    {
        var result = new List<ArticleListItem>(entries.Count);

        foreach (var entry in entries)
        {
            var readingTime = entry.Data.ReadingTimeMinutes
                ?? ReadingTimeCalculator.Calculate(entry.Body);

            result.Add(new ArticleListItem(
                title: entry.Data.Title,
                slug: entry.Slug,
                description: entry.Data.Description,
                pubDate: entry.Data.PubDate,
                author: string.IsNullOrEmpty(entry.Data.Author) ? null : entry.Data.Author,
                tags: entry.Data.GetTags(),
                readingTimeMinutes: readingTime));
        }

        return result;
    }
}
