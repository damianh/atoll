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

    private static async Task<string> RenderHeadAsync(
        DocsConfig config,
        string pageTitle,
        string? pageDescription,
        string currentPath,
        string siteUrl)
    {
        var destination = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            ["Config"] = config,
            ["PageTitle"] = pageTitle,
            ["PageDescription"] = pageDescription,
            ["CurrentPath"] = currentPath,
            ["SiteUrl"] = siteUrl,
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

    // --- OG meta tags ---

    [Fact]
    public async Task ShouldNotRenderOgTagsWhenOpenGraphIsNull()
    {
        var config = MakeConfig();
        // OpenGraph is null by default

        var html = await RenderHeadAsync(config, "My Page", "My description", "/my-page", "https://example.com");

        html.ShouldNotContain("og:title");
        html.ShouldNotContain("og:image");
        html.ShouldNotContain("twitter:card");
    }

    [Fact]
    public async Task ShouldNotRenderOgTagsWhenSiteUrlIsEmpty()
    {
        var config = new DocsConfig
        {
            Title = "My Docs",
            OpenGraph = new OpenGraphConfig(),
        };

        var html = await RenderHeadAsync(config, "My Page", "My description", "/my-page", siteUrl: "");

        html.ShouldNotContain("og:title");
        html.ShouldNotContain("og:image");
    }

    [Fact]
    public async Task ShouldRenderOgTagsWhenOpenGraphIsConfigured()
    {
        var config = new DocsConfig
        {
            Title = "My Docs",
            OpenGraph = new OpenGraphConfig(),
        };

        var html = await RenderHeadAsync(config, "My Page", "A great description", "/my-page", "https://example.com");

        html.ShouldContain("og:title");
        html.ShouldContain("og:description");
        html.ShouldContain("og:image");
        html.ShouldContain("og:url");
        html.ShouldContain("og:site_name");
        html.ShouldContain("twitter:card");
        html.ShouldContain("twitter:title");
        html.ShouldContain("twitter:description");
        html.ShouldContain("twitter:image");
    }

    [Fact]
    public async Task ShouldComputeCorrectOgImageUrl()
    {
        var config = new DocsConfig
        {
            Title = "My Docs",
            OpenGraph = new OpenGraphConfig(),
        };

        var html = await RenderHeadAsync(config, "My Page", null, "/identityserver/overview", "https://docs.example.com");

        html.ShouldContain("https://docs.example.com/og/identityserver/overview.png");
    }

    [Fact]
    public async Task ShouldComputeCorrectOgPageUrl()
    {
        var config = new DocsConfig
        {
            Title = "My Docs",
            OpenGraph = new OpenGraphConfig(),
        };

        var html = await RenderHeadAsync(config, "My Page", null, "/identityserver/overview", "https://docs.example.com");

        html.ShouldContain("https://docs.example.com/identityserver/overview");
    }

    [Fact]
    public async Task ShouldIncludeBasePathInOgImageUrl()
    {
        var config = new DocsConfig
        {
            Title = "My Docs",
            BasePath = "/docs",
            OpenGraph = new OpenGraphConfig(),
        };

        var html = await RenderHeadAsync(config, "My Page", null, "/guide/intro", "https://example.com");

        html.ShouldContain("https://example.com/docs/og/guide/intro.png");
    }

    [Fact]
    public async Task ShouldRenderOgTagsBeforeClosingHeadTag()
    {
        var config = new DocsConfig
        {
            Title = "My Docs",
            OpenGraph = new OpenGraphConfig(),
        };

        var html = await RenderHeadAsync(config, "Page Title", "A description", "/page", "https://example.com");

        var ogIndex = html.IndexOf("og:title", StringComparison.Ordinal);
        var closeHeadIndex = html.IndexOf("</head>", StringComparison.Ordinal);

        ogIndex.ShouldBeGreaterThan(0);
        ogIndex.ShouldBeLessThan(closeHeadIndex);
    }
}

