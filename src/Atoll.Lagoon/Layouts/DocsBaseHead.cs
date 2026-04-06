using Atoll.Components;
using Atoll.Lagoon.Assets;
using Atoll.Lagoon.Configuration;

namespace Atoll.Lagoon.Layouts;

/// <summary>
/// Renders the <c>&lt;head&gt;</c> section for documentation pages, including meta tags,
/// viewport settings, title template, custom CSS, and the theme FOUC-prevention inline script.
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

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var description = PageDescription ?? Config.Description;
        var faviconHref = Config.FaviconHref ?? LagoonAssets.DefaultFaviconPath;

        var model = new DocsBaseHeadModel(Config, PageTitle, description, faviconHref, PageHeadContent);

        await ComponentRenderer.RenderSliceAsync<DocsBaseHeadTemplate, DocsBaseHeadModel>(
            context.Destination,
            model);
    }
}
