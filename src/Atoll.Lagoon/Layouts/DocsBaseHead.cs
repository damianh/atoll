using Atoll.Components;
using Atoll.Lagoon.Assets;
using Atoll.Lagoon.Configuration;

namespace Atoll.Lagoon.Layouts;

/// <summary>
/// Renders the <c>&lt;head&gt;</c> section for documentation pages, including meta tags,
/// viewport settings, title template, custom CSS, and the theme FOUC-prevention inline script.
/// When <see cref="DocsConfig.OpenGraph"/> is configured, also renders OG and Twitter Card meta tags.
/// </summary>
/// <remarks>
/// Rendering is delegated to <c>DocsBaseHeadTemplate.cshtml</c>.
/// </remarks>
public sealed class DocsBaseHead : AtollComponent
{
    /// <summary>Gets or sets the docs site configuration.</summary>
    [Parameter(Required = true)]
    public DocsConfig Config { get; set; } = null!;

    /// <summary>Gets or sets the page-specific title. Appended to the site title.</summary>
    [Parameter]
    public string PageTitle { get; set; } = "";

    /// <summary>Gets or sets the page-specific description for the meta description tag.</summary>
    [Parameter]
    public string? PageDescription { get; set; }

    /// <summary>Gets or sets optional raw HTML to inject into the head section per page (e.g., analytics, social meta tags).</summary>
    [Parameter]
    public string? PageHeadContent { get; set; }

    /// <summary>Gets or sets the current page URL path (e.g. <c>/identityserver/overview/big-picture</c>).
    /// Used to compute OG image and page URLs when <see cref="DocsConfig.OpenGraph"/> is configured.</summary>
    [Parameter]
    public string CurrentPath { get; set; } = "/";

    /// <summary>Gets or sets the site base URL (e.g. <c>https://docs.example.com</c>).
    /// Required for absolute OG image and page URLs when <see cref="DocsConfig.OpenGraph"/> is configured.</summary>
    [Parameter]
    public string SiteUrl { get; set; } = "";

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var description = PageDescription ?? Config.Description;
        var faviconHref = Config.FaviconHref ?? LagoonAssets.DefaultFaviconPath;

        string? ogImageUrl = null;
        string? ogUrl = null;
        if (Config.OpenGraph is not null && !string.IsNullOrEmpty(SiteUrl))
        {
            var basePath = string.IsNullOrEmpty(Config.BasePath) ? "" : Config.BasePath.TrimEnd('/');
            var normalizedPath = CurrentPath.StartsWith('/') ? CurrentPath : "/" + CurrentPath;
            ogImageUrl = SiteUrl.TrimEnd('/') + basePath + "/og" + normalizedPath + ".png";
            ogUrl = SiteUrl.TrimEnd('/') + basePath + normalizedPath;
        }

        var model = new DocsBaseHeadModel(
            Config,
            PageTitle,
            description,
            faviconHref,
            PageHeadContent,
            ogImageUrl,
            ogUrl,
            PageTitle,
            description,
            Config.Title);

        await ComponentRenderer.RenderSliceAsync<DocsBaseHeadTemplate, DocsBaseHeadModel>(
            context.Destination,
            model);
    }
}
