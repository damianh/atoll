using Atoll.Reef.Components;
using Atoll.Reef.Navigation;
using Shouldly;
using Xunit;

namespace Atoll.Reef.Tests.Navigation;

public sealed class TagCountBuilderTests
{
    private static ArticleListItem MakeItem(string slug, string[] tags) =>
        new(slug, slug, "", DateTime.UtcNow, null, tags, null);

    [Fact]
    public void ShouldReturnEmptyListForNoArticles()
    {
        var result = TagCountBuilder.Build([]);

        result.ShouldBeEmpty();
    }

    [Fact]
    public void ShouldReturnEmptyListWhenNoTags()
    {
        var result = TagCountBuilder.Build([MakeItem("a", []), MakeItem("b", [])]);

        result.ShouldBeEmpty();
    }

    [Fact]
    public void ShouldCountSingleTag()
    {
        var result = TagCountBuilder.Build([MakeItem("a", ["dotnet"])]);

        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("dotnet");
        result[0].Count.ShouldBe(1);
    }

    [Fact]
    public void ShouldCountTagAcrossMultipleArticles()
    {
        var articles = new[]
        {
            MakeItem("a", ["dotnet"]),
            MakeItem("b", ["dotnet"]),
            MakeItem("c", ["csharp"]),
        };

        var result = TagCountBuilder.Build(articles);

        result.Count.ShouldBe(2);
        result[0].Name.ShouldBe("dotnet");
        result[0].Count.ShouldBe(2);
        result[1].Name.ShouldBe("csharp");
        result[1].Count.ShouldBe(1);
    }

    [Fact]
    public void ShouldDeduplicateCaseInsensitively()
    {
        var articles = new[]
        {
            MakeItem("a", ["DotNet"]),
            MakeItem("b", ["dotnet"]),
            MakeItem("c", ["DOTNET"]),
        };

        var result = TagCountBuilder.Build(articles);

        result.Count.ShouldBe(1);
        result[0].Count.ShouldBe(3);
    }

    [Fact]
    public void ShouldPreserveFirstEncounteredCasing()
    {
        var articles = new[]
        {
            MakeItem("a", ["CSharp"]),
            MakeItem("b", ["csharp"]),
        };

        var result = TagCountBuilder.Build(articles);

        result[0].Name.ShouldBe("CSharp");
    }

    [Fact]
    public void ShouldOrderByCountDescending()
    {
        var articles = new[]
        {
            MakeItem("a", ["rare"]),
            MakeItem("b", ["popular", "rare"]),
            MakeItem("c", ["popular"]),
            MakeItem("d", ["popular"]),
        };

        var result = TagCountBuilder.Build(articles);

        result[0].Name.ShouldBe("popular");
        result[0].Count.ShouldBe(3);
        result[1].Name.ShouldBe("rare");
        result[1].Count.ShouldBe(2);
    }

    [Fact]
    public void ShouldOrderAlphabeticallyWhenCountsAreEqual()
    {
        var articles = new[]
        {
            MakeItem("a", ["zebra", "apple"]),
        };

        var result = TagCountBuilder.Build(articles);

        result[0].Name.ShouldBe("apple");
        result[1].Name.ShouldBe("zebra");
    }

    [Fact]
    public void ShouldHandleMultipleTagsPerArticle()
    {
        var articles = new[]
        {
            MakeItem("a", ["dotnet", "csharp", "blazor"]),
        };

        var result = TagCountBuilder.Build(articles);

        result.Count.ShouldBe(3);
        result.ShouldAllBe(t => t.Count == 1);
    }

    [Fact]
    public void SlugShouldBeLowercase()
    {
        var result = TagCountBuilder.Build([MakeItem("a", ["DotNet"])]);

        result[0].Slug.ShouldBe("dotnet");
    }
}
