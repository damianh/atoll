namespace Atoll.Reef.Components;

/// <summary>
/// Model for the <c>ArticleNavTemplate</c> Razor slice.
/// </summary>
/// <param name="Previous">The link to the previous article, or <c>null</c>.</param>
/// <param name="Next">The link to the next article, or <c>null</c>.</param>
public sealed record ArticleNavModel(
    ArticleNavLink? Previous,
    ArticleNavLink? Next);
