namespace Atoll.Reef.Components;

/// <summary>
/// Model for the <c>RelatedArticlesTemplate</c> Razor slice.
/// </summary>
/// <param name="Items">The related article links to display (already limited to MaxItems).</param>
/// <param name="Heading">The section heading.</param>
public sealed record RelatedArticlesModel(
    IReadOnlyList<ArticleNavLink> Items,
    string Heading);
