namespace Atoll.Reef.Components;

/// <summary>
/// Model for the <c>ArticleMetaTemplate</c> Razor slice.
/// </summary>
/// <param name="PubDate">The article publication date.</param>
/// <param name="Author">The author display name, or <c>null</c>.</param>
/// <param name="ReadingTimeMinutes">The estimated reading time in minutes, or <c>null</c>.</param>
/// <param name="Tags">The array of tag names to display as linked pills.</param>
/// <param name="BaseTrimmed">The trimmed base URL path prefix.</param>
public sealed record ArticleMetaModel(
    DateTime PubDate,
    string? Author,
    int? ReadingTimeMinutes,
    string[] Tags,
    string BaseTrimmed);
