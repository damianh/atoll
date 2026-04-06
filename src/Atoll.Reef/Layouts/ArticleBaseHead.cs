using Atoll.Components;
using Atoll.Reef.Configuration;

namespace Atoll.Reef.Layouts;

/// <summary>
/// Renders the <c>&lt;head&gt;</c> section for article/blog pages, including meta tags,
/// viewport settings, title template, custom CSS, and the theme FOUC-prevention inline script.
/// </summary>
/// <remarks>
/// Rendering is delegated to <c>ArticleBaseHeadTemplate.cshtml</c>.
/// </remarks>
public sealed class ArticleBaseHead : AtollComponent
{
    /// <summary>Gets or sets the Reef theme configuration.</summary>
    [Parameter(Required = true)]
    public ReefConfig Config { get; set; } = null!;

    /// <summary>Gets or sets the page-specific title. Appended to the site title.</summary>
    [Parameter]
    public string PageTitle { get; set; } = "";

    /// <summary>Gets or sets the page-specific description for the meta description tag.</summary>
    [Parameter]
    public string? PageDescription { get; set; }

    /// <summary>Gets or sets optional raw HTML to inject into the head section per page (e.g. OG tags, analytics).</summary>
    [Parameter]
    public string? PageHeadContent { get; set; }

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var description = PageDescription ?? Config.Description;

        var model = new ArticleBaseHeadModel(Config, PageTitle, description, PageHeadContent);

        await ComponentRenderer.RenderSliceAsync<ArticleBaseHeadTemplate, ArticleBaseHeadModel>(
            context.Destination,
            model);
    }
}
