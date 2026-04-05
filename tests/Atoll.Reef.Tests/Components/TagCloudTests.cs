using Atoll.Components;
using Atoll.Reef.Components;
using Atoll.Rendering;
using Shouldly;
using Xunit;

namespace Atoll.Reef.Tests.Components;

public sealed class TagCloudTests
{
    private static TagCount Tag(string name, int count) => new(name, count);

    private static async Task<string> RenderAsync(
        IReadOnlyList<TagCount>? tags = null,
        string basePath = "/blog")
    {
        var dest = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            [nameof(TagCloud.Tags)] = tags ?? [],
            [nameof(TagCloud.BasePath)] = basePath,
        };
        await ComponentRenderer.RenderComponentAsync<TagCloud>(dest, props);
        return dest.GetOutput();
    }

    [Fact]
    public async Task ShouldRenderNavWithAriaLabel()
    {
        var html = await RenderAsync([Tag("dotnet", 5)]);

        html.ShouldContain("<nav");
        html.ShouldContain("class=\"tag-cloud\"");
        html.ShouldContain("aria-label=\"Tags\"");
    }

    [Fact]
    public async Task ShouldRenderTagPillsWithLinks()
    {
        var html = await RenderAsync([Tag("dotnet", 5)], basePath: "/blog");

        html.ShouldContain("class=\"tag-pill\"");
        html.ShouldContain("href=\"/blog/tag/dotnet\"");
    }

    [Fact]
    public async Task ShouldRenderTagNameAndCount()
    {
        var html = await RenderAsync([Tag("csharp", 3)]);

        html.ShouldContain("csharp");
        html.ShouldContain("(3)");
        html.ShouldContain("tag-count");
    }

    [Fact]
    public async Task ShouldRenderMultipleTags()
    {
        var html = await RenderAsync([Tag("dotnet", 5), Tag("azure", 2)]);

        html.ShouldContain("dotnet");
        html.ShouldContain("azure");
    }

    [Fact]
    public async Task ShouldUseLowercaseSlugForLink()
    {
        var html = await RenderAsync([Tag("DotNet", 1)]);

        html.ShouldContain("href=\"/blog/tag/dotnet\"");
    }

    [Fact]
    public async Task ShouldRenderEmptyNavWhenNoTags()
    {
        var html = await RenderAsync([]);

        html.ShouldContain("<nav");
        html.ShouldNotContain("tag-pill");
    }

    [Fact]
    public async Task ShouldHandleBasePathWithTrailingSlash()
    {
        var html = await RenderAsync([Tag("dotnet", 1)], basePath: "/blog/");

        html.ShouldContain("href=\"/blog/tag/dotnet\"");
    }
}
