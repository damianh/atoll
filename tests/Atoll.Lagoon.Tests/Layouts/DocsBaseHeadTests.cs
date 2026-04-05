using Atoll.Components;
using Atoll.Lagoon.Configuration;
using Atoll.Lagoon.Layouts;
using Atoll.Rendering;
using Shouldly;
using Xunit;

namespace Atoll.Lagoon.Tests.Layouts;

public sealed class DocsBaseHeadTests
{
    private static DocsConfig MakeConfig(string title = "My Docs", string description = "")
    {
        return new DocsConfig
        {
            Title = title,
            Description = description,
        };
    }

    private static async Task<string> RenderHeadAsync(
        DocsConfig config,
        string pageTitle = "",
        string? pageDescription = null,
        string? pageHeadContent = null)
    {
        var destination = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            ["Config"] = config,
            ["PageTitle"] = pageTitle,
            ["PageDescription"] = pageDescription,
            ["PageHeadContent"] = pageHeadContent,
        };
        await ComponentRenderer.RenderComponentAsync<DocsBaseHead>(destination, props);
        return destination.GetOutput();
    }

    // --- PageHeadContent: null ---

    [Fact]
    public async Task ShouldNotInjectHeadContentWhenNull()
    {
        var html = await RenderHeadAsync(MakeConfig(), pageHeadContent: null);

        html.ShouldContain("<head>");
        html.ShouldContain("</head>");
        // Baseline: head closes immediately after custom CSS (none here) and FOUC script
        html.ShouldNotContain("<meta property=");
    }

    // --- PageHeadContent: empty string ---

    [Fact]
    public async Task ShouldNotInjectHeadContentWhenEmpty()
    {
        var html = await RenderHeadAsync(MakeConfig(), pageHeadContent: "");

        html.ShouldContain("<head>");
        html.ShouldContain("</head>");
        html.ShouldNotContain("<meta property=");
    }

    // --- PageHeadContent: non-empty ---

    [Fact]
    public async Task ShouldInjectHeadContentBeforeClosingHeadTag()
    {
        var html = await RenderHeadAsync(
            MakeConfig(),
            pageHeadContent: "<meta property=\"og:title\" content=\"Test\">");

        html.ShouldContain("<meta property=\"og:title\" content=\"Test\">");
        var contentIndex = html.IndexOf("<meta property=\"og:title\"", StringComparison.Ordinal);
        var closeHeadIndex = html.IndexOf("</head>", StringComparison.Ordinal);
        contentIndex.ShouldBeLessThan(closeHeadIndex);
    }

    // --- PageHeadContent: ordering after CustomCss ---

    [Fact]
    public async Task ShouldInjectHeadContentAfterCustomCss()
    {
        var config = new DocsConfig
        {
            Title = "My Docs",
            CustomCss = ["/styles/custom.css"],
        };
        var html = await RenderHeadAsync(
            config,
            pageHeadContent: "<script>analytics()</script>");

        var cssIndex = html.IndexOf("/styles/custom.css", StringComparison.Ordinal);
        var scriptIndex = html.IndexOf("<script>analytics()</script>", StringComparison.Ordinal);
        cssIndex.ShouldBeLessThan(scriptIndex);
    }

    // --- Favicon ---

    [Fact]
    public async Task ShouldRenderCustomFaviconHref()
    {
        var config = new DocsConfig { Title = "My Docs", FaviconHref = "/icons/custom-favicon.ico" };

        var html = await RenderHeadAsync(config);

        html.ShouldContain("rel=\"icon\"");
        html.ShouldContain("/icons/custom-favicon.ico");
    }

    [Fact]
    public async Task ShouldRenderDefaultFaviconWhenFaviconHrefIsNull()
    {
        var html = await RenderHeadAsync(MakeConfig());

        html.ShouldContain("rel=\"icon\"");
        // Default favicon path from LagoonAssets
        html.ShouldContain("/_atoll/logo.png");
    }
}
