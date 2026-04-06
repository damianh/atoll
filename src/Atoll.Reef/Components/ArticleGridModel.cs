namespace Atoll.Reef.Components;

/// <summary>
/// Model for the <c>ArticleGridTemplate</c> Razor slice.
/// </summary>
/// <param name="Columns">The number of grid columns.</param>
/// <param name="Items">The list of article items to display as cards.</param>
/// <param name="BasePath">The base URL path prefix.</param>
public sealed record ArticleGridModel(
    int Columns,
    IReadOnlyList<ArticleListItem> Items,
    string BasePath);
