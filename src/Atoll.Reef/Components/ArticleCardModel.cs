namespace Atoll.Reef.Components;

/// <summary>
/// Model for the <c>ArticleCardTemplate</c> Razor slice.
/// </summary>
/// <param name="Title">The article title.</param>
/// <param name="Href">The resolved article URL.</param>
/// <param name="Description">The short article description.</param>
/// <param name="PubDate">The article publication date.</param>
/// <param name="Author">The author display name, or <c>null</c>.</param>
/// <param name="Tags">The tag names associated with this article.</param>
/// <param name="ImageSrc">The cover image URL, or <c>null</c>.</param>
/// <param name="ImageAlt">The alt text for the cover image.</param>
/// <param name="ReadingTimeMinutes">The estimated reading time in minutes, or <c>null</c>.</param>
/// <param name="BasePath">The base URL path prefix.</param>
public sealed record ArticleCardModel(
    string Title,
    string Href,
    string Description,
    DateTime PubDate,
    string? Author,
    string[] Tags,
    string? ImageSrc,
    string ImageAlt,
    int? ReadingTimeMinutes,
    string BasePath);
