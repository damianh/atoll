using Atoll.Build.Content.Collections;
using Atoll.Reef.Components;
using Atoll.Reef.Configuration;

namespace Atoll.Reef.Navigation;

/// <summary>
/// Resolves all parts of a named article series and the 1-based index of the current part,
/// ordered by <see cref="ArticleSchema.SeriesOrder"/> (or publication date as fallback).
/// </summary>
public static class SeriesResolver
{
    /// <summary>
    /// Finds all entries in the given series, sorted by <c>SeriesOrder</c> then
    /// <c>PubDate</c>, and returns the parts list alongside the current article's part number.
    /// </summary>
    /// <param name="seriesName">The series name to match (case-insensitive).</param>
    /// <param name="currentSlug">The slug of the article currently being viewed.</param>
    /// <param name="entries">All content entries to search.</param>
    /// <param name="basePath">Base URL prefix for building hrefs.</param>
    /// <returns>
    /// A tuple of the ordered <see cref="SeriesPart"/> list and the 1-based index of the
    /// current article within that list (0 if not found).
    /// </returns>
    public static (IReadOnlyList<SeriesPart> Parts, int CurrentPart) Resolve(
        string seriesName,
        string currentSlug,
        IReadOnlyList<ContentEntry<ArticleSchema>> entries,
        string basePath)
    {
        var baseTrimmed = basePath.TrimEnd('/');

        var seriesEntries = entries
            .Where(e => string.Equals(e.Data.Series, seriesName, StringComparison.OrdinalIgnoreCase))
            .OrderBy(e => e.Data.SeriesOrder ?? int.MaxValue)
            .ThenBy(e => e.Data.PubDate)
            .ToList();

        var parts = seriesEntries
            .Select(e => new SeriesPart(
                e.Data.Title,
                $"{baseTrimmed}/{e.Slug.TrimStart('/')}"))
            .ToList();

        var currentIndex = seriesEntries.FindIndex(e => e.Slug == currentSlug);
        var currentPart = currentIndex >= 0 ? currentIndex + 1 : 0;

        return (parts, currentPart);
    }
}
