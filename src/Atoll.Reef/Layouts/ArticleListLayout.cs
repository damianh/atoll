using Atoll.Components;
using Atoll.Reef.Configuration;
using Atoll.Slots;

namespace Atoll.Reef.Layouts;

/// <summary>
/// Full-page layout for article listing pages (index, tag pages, author pages, paginated pages).
/// Assembles the HTML document shell with header, listing content area, and footer.
/// Page content (the article list/grid and pagination) is rendered via the default slot.
/// </summary>
/// <remarks>
/// Rendering is delegated to <c>ArticleListLayoutTemplate.cshtml</c>.
/// </remarks>
public sealed class ArticleListLayout : AtollComponent
{
    /// <summary>Gets or sets the Reef theme configuration. Required.</summary>
    [Parameter(Required = true)]
    public ReefConfig Config { get; set; } = null!;

    /// <summary>Gets or sets the page-specific title shown in the &lt;h1&gt; heading and &lt;title&gt; tag.</summary>
    [Parameter]
    public string PageTitle { get; set; } = "";

    /// <summary>Gets or sets the page-specific description for the meta description tag.</summary>
    [Parameter]
    public string? PageDescription { get; set; }

    /// <summary>Gets or sets optional raw HTML injected into the page &lt;head&gt;.</summary>
    [Parameter]
    public string? PageHeadContent { get; set; }

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var basePath = Config.BasePath.TrimEnd('/');
        var brandHref = string.IsNullOrEmpty(basePath) ? "/" : basePath + "/";

        var model = new ArticleListLayoutModel(Config, PageTitle, PageDescription, PageHeadContent, brandHref);

        var pageSlot = context.Slots.GetSlotFragment(SlotCollection.DefaultSlotName);
        var templateSlots = SlotCollection.FromDefault(pageSlot);

        await ComponentRenderer.RenderSliceAsync<ArticleListLayoutTemplate, ArticleListLayoutModel>(
            context.Destination,
            model,
            templateSlots);
    }
}
