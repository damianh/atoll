using Atoll.Components;
using Atoll.Instructions;
using Atoll.Reef.Islands;
using Atoll.Rendering;

namespace Atoll.Reef.Tests.Islands;

public sealed class ArticleFilterTests
{
    private static async Task<string> RenderArticleFilterAsync(
        IReadOnlyList<string>? tags = null,
        IReadOnlyList<string>? authors = null)
    {
        var dest = new StringRenderDestination();
        var props = new Dictionary<string, object?>();
        if (tags is not null) props["Tags"] = tags;
        if (authors is not null) props["Authors"] = authors;
        await ComponentRenderer.RenderComponentAsync<ArticleFilter>(dest, props);
        return dest.GetOutput();
    }

    [Fact]
    public async Task ShouldRenderFilterRoot()
    {
        var html = await RenderArticleFilterAsync();

        html.ShouldContain("data-filter-root");
    }

    [Fact]
    public async Task ShouldRenderAllTagsButton()
    {
        var html = await RenderArticleFilterAsync();

        html.ShouldContain("data-filter-tag=\"\"");
        html.ShouldContain("All");
    }

    [Fact]
    public async Task ShouldRenderTagPills()
    {
        var html = await RenderArticleFilterAsync(tags: ["dotnet", "csharp"]);

        html.ShouldContain("data-filter-tag=\"dotnet\"");
        html.ShouldContain("data-filter-tag=\"csharp\"");
    }

    [Fact]
    public async Task ShouldRenderAuthorDropdownWhenAuthorsProvided()
    {
        var html = await RenderArticleFilterAsync(authors: ["Alice", "Bob"]);

        html.ShouldContain("data-filter-author");
        html.ShouldContain("Alice");
        html.ShouldContain("Bob");
    }

    [Fact]
    public async Task ShouldNotRenderAuthorDropdownWhenNoAuthors()
    {
        var html = await RenderArticleFilterAsync(authors: []);

        html.ShouldNotContain("data-filter-author");
    }

    [Fact]
    public async Task ShouldHtmlEncodeTagNames()
    {
        var html = await RenderArticleFilterAsync(tags: ["<script>evil</script>"]);

        html.ShouldNotContain("<script>evil</script>");
        html.ShouldContain("&lt;script&gt;");
    }

    [Fact]
    public async Task ShouldHtmlEncodeAuthorNames()
    {
        var html = await RenderArticleFilterAsync(authors: ["<script>evil</script>"]);

        html.ShouldNotContain("<script>evil</script>");
        html.ShouldContain("&lt;script&gt;");
    }

    [Fact]
    public void ShouldHaveClientIdleDirective()
    {
        var island = new ArticleFilter();

        var metadata = island.CreateMetadata();

        metadata.ShouldNotBeNull();
        metadata.DirectiveType.ShouldBe(ClientDirectiveType.Idle);
    }

    [Fact]
    public void ShouldHaveCorrectClientModuleUrl()
    {
        var island = new ArticleFilter();

        island.ClientModuleUrl.ShouldBe("/scripts/atoll-reef-article-filter.js");
    }

    [Fact]
    public async Task ShouldRenderAsIslandWrapper()
    {
        var dest = new StringRenderDestination();
        var island = new ArticleFilter();
        await island.RenderIslandAsync(dest);
        var html = dest.GetOutput();

        html.ShouldContain("<atoll-island");
        html.ShouldContain("client=\"idle\"");
        html.ShouldContain("component-url=\"/scripts/atoll-reef-article-filter.js\"");
    }
}
