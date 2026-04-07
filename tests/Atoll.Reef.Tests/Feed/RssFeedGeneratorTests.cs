using Atoll.Reef.Components;
using Atoll.Reef.Configuration;
using Atoll.Reef.Feed;

namespace Atoll.Reef.Tests.Feed;

public sealed class RssFeedGeneratorTests
{
    private static ReefConfig MakeConfig(
        string title = "My Blog",
        string description = "Test blog",
        string siteUrl = "https://example.com",
        bool rssEnabled = true) =>
        new()
        {
            Title = title,
            Description = description,
            SiteUrl = siteUrl,
            RssEnabled = rssEnabled,
        };

    private static ArticleListItem MakeArticle(
        string slug = "post",
        string title = "Post Title",
        string description = "Description",
        string? author = null,
        string[] tags = null!,
        DateTime? pubDate = null) =>
        new(title, slug, description, pubDate ?? new DateTime(2025, 3, 15, 0, 0, 0, DateTimeKind.Utc),
            author, tags ?? [], null);

    [Fact]
    public void ShouldGenerateRssElement()
    {
        var xml = RssFeedGenerator.Generate(MakeConfig(), [], "/blog");

        xml.ShouldContain("<rss");
        xml.ShouldContain("version=\"2.0\"");
    }

    [Fact]
    public void ShouldGenerateChannelWithTitle()
    {
        var xml = RssFeedGenerator.Generate(MakeConfig(title: "Tech Blog"), [], "/blog");

        xml.ShouldContain("<title>Tech Blog</title>");
    }

    [Fact]
    public void ShouldGenerateChannelWithDescription()
    {
        var xml = RssFeedGenerator.Generate(
            MakeConfig(description: "Technology articles"), [], "/blog");

        xml.ShouldContain("<description>Technology articles</description>");
    }

    [Fact]
    public void ShouldIncludeChannelLink()
    {
        var xml = RssFeedGenerator.Generate(
            MakeConfig(siteUrl: "https://myblog.com"), [], "/blog");

        xml.ShouldContain("https://myblog.com/blog");
    }

    [Fact]
    public void ShouldIncludeAtomSelfLinkWhenRssEnabled()
    {
        var xml = RssFeedGenerator.Generate(
            MakeConfig(siteUrl: "https://myblog.com"), [], "/blog");

        xml.ShouldContain("application/rss+xml");
        xml.ShouldContain("https://myblog.com/blog/feed.xml");
    }

    [Fact]
    public void ShouldGenerateItemForEachArticle()
    {
        var articles = new[]
        {
            MakeArticle("first", "First Post"),
            MakeArticle("second", "Second Post"),
        };

        var xml = RssFeedGenerator.Generate(MakeConfig(), articles, "/blog");

        xml.ShouldContain("<item>");
        xml.ShouldContain("First Post");
        xml.ShouldContain("Second Post");
    }

    [Fact]
    public void ShouldIncludeItemLink()
    {
        var articles = new[] { MakeArticle("my-post") };

        var xml = RssFeedGenerator.Generate(
            MakeConfig(siteUrl: "https://example.com"), articles, "/blog");

        xml.ShouldContain("https://example.com/blog/my-post");
    }

    [Fact]
    public void ShouldIncludeItemPubDate()
    {
        var date = new DateTime(2025, 3, 15, 0, 0, 0, DateTimeKind.Utc);
        var articles = new[] { MakeArticle(pubDate: date) };

        var xml = RssFeedGenerator.Generate(MakeConfig(), articles, "/blog");

        xml.ShouldContain("<pubDate>");
    }

    [Fact]
    public void ShouldIncludeAuthorWhenPresent()
    {
        var articles = new[] { MakeArticle(author: "Alice") };

        var xml = RssFeedGenerator.Generate(MakeConfig(), articles, "/blog");

        xml.ShouldContain("<author>Alice</author>");
    }

    [Fact]
    public void ShouldIncludeCategoriesForTags()
    {
        var articles = new[] { MakeArticle(tags: ["dotnet", "csharp"]) };

        var xml = RssFeedGenerator.Generate(MakeConfig(), articles, "/blog");

        xml.ShouldContain("<category>dotnet</category>");
        xml.ShouldContain("<category>csharp</category>");
    }

    [Fact]
    public void ShouldIncludeAtomNamespace()
    {
        var xml = RssFeedGenerator.Generate(MakeConfig(), [], "/blog");

        xml.ShouldContain("http://www.w3.org/2005/Atom");
    }
}
