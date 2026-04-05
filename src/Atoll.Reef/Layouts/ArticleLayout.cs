using Atoll.Components;
using Atoll.Reef.Configuration;

namespace Atoll.Reef.Layouts;

/// <summary>
/// Full-page layout for individual article pages. Assembles the HTML document shell
/// with header (logo, site title, social links), main content area, and footer.
/// Page content is rendered via the default slot.
/// </summary>
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
        WriteHtml("<!DOCTYPE html>");
        WriteHtml("<html lang=\"en\">");

        // <head>
        await RenderAsync(ComponentRenderer.ToFragment<ArticleBaseHead>(new Dictionary<string, object?>
        {
            [nameof(ArticleBaseHead.Config)] = Config,
            [nameof(ArticleBaseHead.PageTitle)] = PageTitle,
            [nameof(ArticleBaseHead.PageDescription)] = PageDescription,
            [nameof(ArticleBaseHead.PageHeadContent)] = PageHeadContent,
        }));

        WriteHtml("<body>");

        // Header
        WriteHtml("<header class=\"reef-header\" role=\"banner\">");
        WriteHtml("<div class=\"reef-header-inner\">");

        // Logo / site title link
        var basePath = Config.BasePath.TrimEnd('/');
        WriteHtml("<a href=\"");
        WriteHtml(HtmlEncode(string.IsNullOrEmpty(basePath) ? "/" : basePath + "/"));
        WriteHtml("\" class=\"reef-brand\">");

        if (!string.IsNullOrEmpty(Config.LogoSrc))
        {
            WriteHtml("<img src=\"");
            WriteHtml(HtmlEncode(Config.LogoSrc));
            WriteHtml("\" alt=\"");
            WriteHtml(HtmlEncode(Config.LogoAlt));
            WriteHtml("\" class=\"reef-logo\" />");
        }

        WriteText(Config.Title);
        WriteHtml("</a>");

        // Social links
        if (Config.Social.Count > 0)
        {
            WriteHtml("<nav class=\"reef-social\" aria-label=\"Social links\">");
            foreach (var social in Config.Social)
            {
                WriteHtml("<a href=\"");
                WriteHtml(HtmlEncode(social.Url));
                WriteHtml("\" class=\"reef-social-link\" rel=\"noopener noreferrer\" target=\"_blank\">");
                WriteText(social.Label);
                WriteHtml("</a>");
            }

            WriteHtml("</nav>");
        }

        WriteHtml("</div>"); // .reef-header-inner
        WriteHtml("</header>");

        // Main content
        WriteHtml("<main class=\"reef-main\" id=\"main-content\" role=\"main\">");
        WriteHtml("<article class=\"reef-article prose\">");
        await RenderSlotAsync();
        WriteHtml("</article>");
        WriteHtml("</main>");

        // Footer
        WriteHtml("<footer class=\"reef-footer\" role=\"contentinfo\">");
        WriteHtml("<p>");
        WriteText(Config.Title);
        WriteHtml(" &mdash; Built with <a href=\"https://github.com/damianh/atoll\">Atoll</a></p>");
        WriteHtml("</footer>");

        WriteHtml("</body>");
        WriteHtml("</html>");
    }

    private static string HtmlEncode(string value) =>
        System.Net.WebUtility.HtmlEncode(value);
}
