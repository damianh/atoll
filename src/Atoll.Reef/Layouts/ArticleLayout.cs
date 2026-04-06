using Atoll.Components;
using Atoll.Reef.Configuration;
using Atoll.Slots;

namespace Atoll.Reef.Layouts;

/// <summary>
/// Full-page layout for individual article pages. Assembles the HTML document shell
/// with header (logo, site title, social links), main content area, and footer.
/// Page content is rendered via the default slot.
/// </summary>
/// <remarks>
/// Rendering is delegated to <c>ArticleLayoutTemplate.cshtml</c>.
/// </remarks>
public sealed class ArticleLayout : AtollComponent
{
    /// <summary>Gets or sets the Reef theme configuration. Required.</summary>
    [Parameter(Required = true)]
    public ReefConfig Config { get; set; } = null!;

    /// <summary>Gets or sets the page-specific title. Appended to the site title in the &lt;title&gt; tag.</summary>
    [Parameter]
    public string PageTitle { get; set; } = "";

    /// <summary>Gets or sets the page-specific description for the meta description tag.</summary>
    [Parameter]
    public string? PageDescription { get; set; }

    /// <summary>Gets or sets optional raw HTML injected into the page &lt;head&gt; (e.g. OG tags).</summary>
    [Parameter]
    public string? PageHeadContent { get; set; }

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var basePath = Config.BasePath.TrimEnd('/');
        var brandHref = string.IsNullOrEmpty(basePath) ? "/" : basePath + "/";

        var model = new ArticleLayoutModel(Config, PageTitle, PageDescription, PageHeadContent, brandHref);

        var pageSlot = context.Slots.GetSlotFragment(SlotCollection.DefaultSlotName);
        var templateSlots = SlotCollection.FromDefault(pageSlot);

        await ComponentRenderer.RenderSliceAsync<ArticleLayoutTemplate, ArticleLayoutModel>(
            context.Destination,
            model,
            templateSlots);
    }
}
