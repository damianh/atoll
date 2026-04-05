using Atoll.Components;
using Atoll.Reef.Components;
using Atoll.Rendering;
using Shouldly;
using Xunit;

namespace Atoll.Reef.Tests.Components;

public sealed class AuthorCardTests
{
    private static async Task<string> RenderAsync(
        string name = "Alice",
        string? avatarUrl = null,
        string? bio = null,
        string? url = null)
    {
        var dest = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            [nameof(AuthorCard.Name)] = name,
            [nameof(AuthorCard.AvatarUrl)] = avatarUrl,
            [nameof(AuthorCard.Bio)] = bio,
            [nameof(AuthorCard.Url)] = url,
        };
        await ComponentRenderer.RenderComponentAsync<AuthorCard>(dest, props);
        return dest.GetOutput();
    }

    [Fact]
    public async Task ShouldRenderAsideWithClass()
    {
        var html = await RenderAsync();

        html.ShouldContain("<aside");
        html.ShouldContain("class=\"author-card\"");
    }

    [Fact]
    public async Task ShouldRenderAuthorName()
    {
        var html = await RenderAsync(name: "Alice Smith");

        html.ShouldContain("Alice Smith");
    }

    [Fact]
    public async Task ShouldRenderAvatarImageWhenProvided()
    {
        var html = await RenderAsync(avatarUrl: "https://example.com/alice.jpg");

        html.ShouldContain("<img");
        html.ShouldContain("author-card__avatar");
        html.ShouldContain("https://example.com/alice.jpg");
    }

    [Fact]
    public async Task ShouldNotRenderAvatarWhenNull()
    {
        var html = await RenderAsync(avatarUrl: null);

        html.ShouldNotContain("<img");
    }

    [Fact]
    public async Task ShouldRenderBioWhenProvided()
    {
        var html = await RenderAsync(bio: "Writes about .NET");

        html.ShouldContain("author-card__bio");
        html.ShouldContain("Writes about .NET");
    }

    [Fact]
    public async Task ShouldNotRenderBioWhenNull()
    {
        var html = await RenderAsync(bio: null);

        html.ShouldNotContain("author-card__bio");
    }

    [Fact]
    public async Task ShouldRenderNameAsLinkWhenUrlProvided()
    {
        var html = await RenderAsync(name: "Alice", url: "https://alice.dev");

        html.ShouldContain("<a");
        html.ShouldContain("href=\"https://alice.dev\"");
        html.ShouldContain("author-card__name");
    }

    [Fact]
    public async Task ShouldRenderNameAsParagraphWhenNoUrl()
    {
        var html = await RenderAsync(name: "Bob", url: null);

        html.ShouldNotContain("<a ");
        html.ShouldContain("<p class=\"author-card__name\">");
    }

    [Fact]
    public async Task ShouldHtmlEncodeNameInAltAttribute()
    {
        var html = await RenderAsync(
            name: "<Script>",
            avatarUrl: "https://example.com/avatar.jpg");

        html.ShouldContain("alt=\"&lt;Script&gt;\"");
    }
}
