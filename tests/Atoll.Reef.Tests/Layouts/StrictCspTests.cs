using Atoll.Components;
using Atoll.Reef.Configuration;
using Atoll.Reef.Islands;
using Atoll.Reef.Layouts;
using Atoll.Rendering;
using Atoll.Tests.Shared;

namespace Atoll.Reef.Tests.Layouts;

/// <summary>
/// Verifies Reef output works under a strict Content-Security-Policy (<c>script-src 'self'</c>).
/// </summary>
public sealed class StrictCspTests
{
    private static ReefConfig MakeConfig() => new() { Title = "Blog", RssEnabled = true, FaviconHref = "/favicon.svg" };

    [Fact]
    public async Task ArticleLayoutShouldHaveNoInlineScriptsOrHandlers()
    {
        var destination = new StringRenderDestination();
        await ComponentRenderer.RenderComponentAsync<ArticleLayout>(destination, new Dictionary<string, object?>
        {
            [nameof(ArticleLayout.Config)] = MakeConfig(),
            [nameof(ArticleLayout.PageTitle)] = "Post",
        });
        var html = destination.GetOutput();

        html.ShouldContain("<script src=\"/scripts/atoll-reef-theme-init.js\"></script>");
        html.ShouldBeStrictCspCompatible();
    }

    [Fact]
    public async Task ArticleListLayoutShouldHaveNoInlineScriptsOrHandlers()
    {
        var destination = new StringRenderDestination();
        await ComponentRenderer.RenderComponentAsync<ArticleListLayout>(destination, new Dictionary<string, object?>
        {
            [nameof(ArticleListLayout.Config)] = MakeConfig(),
            [nameof(ArticleListLayout.PageTitle)] = "Articles",
        });
        var html = destination.GetOutput();

        html.ShouldContain("<script src=\"/scripts/atoll-reef-theme-init.js\"></script>");
        html.ShouldBeStrictCspCompatible();
    }

    [Fact]
    public void ThemeInitAssetShouldBeEmbedded()
    {
        var asset = new ReefIslandAssetProvider().GetAssets()
            .Single(a => a.OutputPath == "scripts/atoll-reef-theme-init.js");
        using var stream = asset.ResourceAssembly.GetManifestResourceStream(asset.ResourceName);
        stream.ShouldNotBeNull();
        using var reader = new StreamReader(stream);
        reader.ReadToEnd().ShouldContain("atoll-theme");
    }
}
