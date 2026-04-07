using Atoll.Reef.Components;
using Atoll.Reef.Navigation;

namespace Atoll.Reef.Tests.Navigation;

public sealed class RelatedArticlesResolverTests
{
    private static ArticleListItem MakeItem(string slug, string[] tags, DateTime? pubDate = null) =>
        new("Title for " + slug, slug, "Description", pubDate ?? new DateTime(2025, 1, 1), null, tags, null);

    [Fact]
    public void ShouldReturnEmptyWhenCurrentTagsEmpty()
    {
        var articles = new[]
        {
            MakeItem("other", ["dotnet", "csharp"]),
        };

        var result = RelatedArticlesResolver.Resolve("current", [], articles, "/blog");

        result.ShouldBeEmpty();
    }

    [Fact]
    public void ShouldExcludeCurrentArticle()
    {
        var articles = new[]
        {
            MakeItem("current", ["dotnet"]),
            MakeItem("related", ["dotnet"]),
        };

        var result = RelatedArticlesResolver.Resolve("current", ["dotnet"], articles, "/blog");

        result.ShouldNotContain(r => r.Href.Contains("current"));
        result.ShouldContain(r => r.Href.Contains("related"));
    }

    [Fact]
    public void ShouldReturnArticlesWithSharedTags()
    {
        var articles = new[]
        {
            MakeItem("no-match", ["python"]),
            MakeItem("one-match", ["dotnet"]),
            MakeItem("two-match", ["dotnet", "csharp"]),
        };

        var result = RelatedArticlesResolver.Resolve("current", ["dotnet", "csharp"], articles, "/blog");

        result.ShouldContain(r => r.Href.Contains("one-match"));
        result.ShouldContain(r => r.Href.Contains("two-match"));
        result.ShouldNotContain(r => r.Href.Contains("no-match"));
    }

    [Fact]
    public void ShouldOrderByDescendingTagMatchScore()
    {
        var articles = new[]
        {
            MakeItem("one-match", ["dotnet"]),
            MakeItem("two-match", ["dotnet", "csharp"]),
        };

        var result = RelatedArticlesResolver.Resolve(
            "current", ["dotnet", "csharp"], articles, "/blog");

        result[0].Href.ShouldContain("two-match");
        result[1].Href.ShouldContain("one-match");
    }

    [Fact]
    public void ShouldRespectMaxItems()
    {
        var articles = Enumerable.Range(1, 10)
            .Select(i => MakeItem($"post-{i}", ["dotnet"]))
            .ToList();

        var result = RelatedArticlesResolver.Resolve(
            "current", ["dotnet"], articles, "/blog", maxItems: 3);

        result.Count.ShouldBe(3);
    }

    [Fact]
    public void ShouldBuildCorrectHref()
    {
        var articles = new[] { MakeItem("my-post", ["dotnet"]) };

        var result = RelatedArticlesResolver.Resolve(
            "current", ["dotnet"], articles, "/blog");

        result[0].Href.ShouldBe("/blog/my-post");
    }

    [Fact]
    public void ShouldBuildCorrectHrefWithTrailingSlashOnBasePath()
    {
        var articles = new[] { MakeItem("my-post", ["dotnet"]) };

        var result = RelatedArticlesResolver.Resolve(
            "current", ["dotnet"], articles, "/blog/");

        result[0].Href.ShouldBe("/blog/my-post");
    }

    [Fact]
    public void ShouldMatchTagsCaseInsensitively()
    {
        var articles = new[] { MakeItem("post", ["DotNet"]) };

        var result = RelatedArticlesResolver.Resolve(
            "current", ["dotnet"], articles, "/blog");

        result.ShouldNotBeEmpty();
    }

    [Fact]
    public void ShouldOrderTiedScoresByNewestFirst()
    {
        var articles = new[]
        {
            MakeItem("older", ["dotnet"], new DateTime(2024, 1, 1)),
            MakeItem("newer", ["dotnet"], new DateTime(2025, 6, 1)),
        };

        var result = RelatedArticlesResolver.Resolve(
            "current", ["dotnet"], articles, "/blog");

        result[0].Href.ShouldContain("newer");
        result[1].Href.ShouldContain("older");
    }
}
