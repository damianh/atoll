namespace Atoll.Reef.Components;

/// <summary>
/// Model for the <c>ArticleListTemplate</c> Razor slice.
/// </summary>
/// <param name="Items">The list of article items to display.</param>
/// <param name="BaseTrimmed">The trimmed base URL path prefix.</param>
public sealed record ArticleListModel(
    IReadOnlyList<ArticleListItem> Items,
    string BaseTrimmed);
