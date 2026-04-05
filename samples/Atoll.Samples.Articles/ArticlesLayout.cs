using Atoll.Components;
using Atoll.Reef.Layouts;

namespace Atoll.Samples.Articles;

/// <summary>
/// Site-wide layout for the Articles sample. Delegates to <see cref="ArticleLayout"/>
/// with the site's <see cref="ArticlesConfig.Current"/> configuration, passing the page
/// content through as the default slot.
/// </summary>
internal sealed class ArticlesLayout : AtollComponent
{
    /// <summary>Gets or sets the page title suffix.</summary>
    [Parameter]
    public string PageTitle { get; set; } = "";

    /// <summary>Gets or sets the page meta description.</summary>
    [Parameter]
    public string? PageDescription { get; set; }

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<!DOCTYPE html>");
        WriteHtml("<html lang=\"en\">");

        await RenderAsync(ComponentRenderer.ToFragment<ArticleBaseHead>(new Dictionary<string, object?>
        {
            [nameof(ArticleBaseHead.Config)] = ArticlesConfig.Current,
            [nameof(ArticleBaseHead.PageTitle)] = PageTitle,
            [nameof(ArticleBaseHead.PageDescription)] = PageDescription,
        }));

        WriteHtml("<body>");
        WriteHtml("<header class=\"reef-header\" role=\"banner\">");
        WriteHtml("<div class=\"reef-header-inner\">");
        WriteHtml("<a href=\"/articles\" class=\"reef-brand\">Atoll Articles</a>");
        WriteHtml("</div>");
        WriteHtml("</header>");

        WriteHtml("<main class=\"reef-main\" id=\"main-content\" role=\"main\">");
        await RenderSlotAsync();
        WriteHtml("</main>");

        WriteHtml("<footer class=\"reef-footer\" role=\"contentinfo\">");
        WriteHtml("<p>Atoll Articles &mdash; Built with <a href=\"https://github.com/damianh/atoll\">Atoll</a></p>");
        WriteHtml("</footer>");

        WriteHtml("</body>");
        WriteHtml("</html>");
    }
}
