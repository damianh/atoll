using Atoll.Components;
using Atoll.Lagoon.Assets;
using Atoll.Lagoon.Configuration;
using Atoll.Lagoon.Islands;

namespace Atoll.Lagoon.Layouts;

/// <summary>
/// A full-width, sidebar-free layout for splash and landing pages. Assembles the
/// full HTML document shell with the shared header (logo, search, theme toggle) and
/// footer, but omits the sidebar, table of contents, breadcrumbs, pagination, and
/// mobile-nav toggle — yielding a wide, uncluttered canvas suitable for landing pages.
/// </summary>
/// <remarks>
/// Usage: set <see cref="Config"/> (required), optional page-specific parameters
/// (<see cref="PageTitle"/>, <see cref="PageDescription"/>), then place page content
/// (e.g. a <c>Hero</c> component) in the default slot.
/// </remarks>
public sealed class SplashLayout : AtollComponent
{
    /// <summary>Gets or sets the docs site configuration. Required.</summary>
    [Parameter(Required = true)]
    public DocsConfig Config { get; set; } = null!;

    /// <summary>Gets or sets the page-specific title. Appended to the site title in the &lt;title&gt; tag.</summary>
    [Parameter]
    public string PageTitle { get; set; } = "";

    /// <summary>Gets or sets the page-specific description for the meta description tag.</summary>
    [Parameter]
    public string? PageDescription { get; set; }

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<!DOCTYPE html>");
        WriteHtml("<html lang=\"en\">");

        // <head>
        await RenderAsync(ComponentRenderer.ToFragment<DocsBaseHead>(new Dictionary<string, object?>
        {
            ["Config"] = Config,
            ["PageTitle"] = PageTitle,
            ["PageDescription"] = PageDescription,
        }));

        WriteHtml("<body>");

        // Header (shared chrome — no MobileNav, no sidebar to toggle)
        WriteHtml("<header class=\"docs-header\">");
        WriteHtml("<div class=\"docs-header-inner\">");

        // Logo / site title
        WriteHtml("<a href=\"/\" class=\"docs-brand\">");
        if (!string.IsNullOrEmpty(Config.LogoSrc))
        {
            WriteHtml($"<img src=\"{HtmlEncode(Config.LogoSrc)}\" alt=\"{HtmlEncode(Config.LogoAlt)}\" class=\"docs-logo\" />");
        }
        else
        {
            WriteHtml($"<img src=\"{LagoonAssets.DefaultFaviconPath}\" alt=\"{HtmlEncode(Config.LogoAlt)}\" class=\"docs-logo\" />");
        }

        WriteText(Config.Title);
        WriteHtml("</a>");

        // Header right: search + social + theme toggle
        WriteHtml("<div class=\"docs-header-actions\">");

        // Search dialog island
        await RenderAsync(ComponentRenderer.ToFragment<SearchDialog>(new Dictionary<string, object?>()));

        // Social links
        foreach (var social in Config.Social)
        {
            WriteHtml($"<a href=\"{HtmlEncode(social.Url)}\" class=\"docs-social-link\" rel=\"noopener noreferrer\" target=\"_blank\">");
            WriteText(social.Label);
            WriteHtml("</a>");
        }

        // Theme toggle island
        await RenderAsync(ComponentRenderer.ToFragment<ThemeToggle>(new Dictionary<string, object?>()));

        WriteHtml("</div>"); // .docs-header-actions
        WriteHtml("</div>"); // .docs-header-inner
        WriteHtml("</header>");

        // Main content — full-width, no sidebar or TOC
        WriteHtml("<main class=\"splash-main\" id=\"main-content\">");
        WriteHtml("<article class=\"splash-content\">");
        await RenderSlotAsync();
        WriteHtml("</article>");
        WriteHtml("</main>");

        // Footer
        WriteHtml("<footer class=\"docs-footer\">");
        WriteHtml("<p>Built with <a href=\"https://github.com/damianh/atoll\">Atoll</a> &mdash; a .NET-native framework inspired by Astro.</p>");
        WriteHtml("</footer>");

        // Mermaid support (conditionally injected)
        if (Config.EnableMermaid)
        {
            WriteHtml("<script src=\"/scripts/atoll-docs-mermaid-init.js\" type=\"module\"></script>");
        }

        WriteHtml("</body>");
        WriteHtml("</html>");
    }

    private static string HtmlEncode(string value) =>
        System.Net.WebUtility.HtmlEncode(value);
}
