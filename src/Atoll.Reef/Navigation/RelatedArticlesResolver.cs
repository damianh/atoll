using Atoll.Reef.Components;

namespace Atoll.Reef.Navigation;

/// <summary>
/// Resolves related articles by scoring how many tags they share with the current article.
/// Higher scores rank first; the current article is excluded from results.
/// </summary>
public static class RelatedArticlesResolver
{
    /// <summary>
    /// Finds the most related articles based on shared tag count, returning up to 3 results.
    /// </summary>
    /// <param name="currentSlug">The slug of the article being viewed — excluded from results.</param>
    /// <param name="currentTags">Tags of the current article used for scoring.</param>
    /// <param name="allArticles">All available articles to score against.</param>
    /// <param name="basePath">Base URL prefix for building hrefs.</param>
    /// <returns>A list of <see cref="ArticleNavLink"/> ordered by descending relevance score.</returns>
    public static IReadOnlyList<ArticleNavLink> Resolve(
        string currentSlug,
        string[] currentTags,
        IReadOnlyList<ArticleListItem> allArticles,
        string basePath)
    {
        return Resolve(currentSlug, currentTags, allArticles, basePath, 3);
    }

    /// <summary>
    /// Finds the most related articles based on shared tag count.
    /// </summary>
    /// <param name="currentSlug">The slug of the article being viewed — excluded from results.</param>
    /// <param name="currentTags">Tags of the current article used for scoring.</param>
    /// <param name="allArticles">All available articles to score against.</param>
    /// <param name="basePath">Base URL prefix for building hrefs.</param>
    /// <param name="maxItems">Maximum number of results to return.</param>
    /// <returns>A list of <see cref="ArticleNavLink"/> ordered by descending relevance score.</returns>
    public static IReadOnlyList<ArticleNavLink> Resolve(
        string currentSlug,
        string[] currentTags,
        IReadOnlyList<ArticleListItem> allArticles,
        string basePath,
        int maxItems)
    {
        if (currentTags.Length == 0)
        {
            return [];
        }

        var tagSet = new HashSet<string>(currentTags, StringComparer.OrdinalIgnoreCase);
        var baseTrimmed = basePath.TrimEnd('/');

        return allArticles
            .Where(a => a.Slug != currentSlug)
            .Select(a => new
            {
                Article = a,
                Score = a.Tags.Count(t => tagSet.Contains(t)),
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.Article.PubDate)
            .Take(maxItems)
            .Select(x => new ArticleNavLink(
                x.Article.Title,
                $"{baseTrimmed}/{x.Article.Slug.TrimStart('/')}"))
            .ToList();
    }
}
