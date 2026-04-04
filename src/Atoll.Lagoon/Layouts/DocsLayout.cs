using Atoll.Build.Content.Markdown;
using Atoll.Components;
using Atoll.Lagoon.Components;
using Atoll.Lagoon.Configuration;
using Atoll.Lagoon.Islands;
using Atoll.Lagoon.Navigation;

namespace Atoll.Lagoon.Layouts;

/// <summary>
/// The main documentation page layout. Assembles the full HTML document shell,
/// composing header (logo, search, theme toggle), mobile nav, sidebar, breadcrumbs,
/// main content area, table of contents, pagination, and footer.
/// </summary>
/// <remarks>
/// Usage: set <see cref="Config"/> (required), optional page-specific parameters
/// (<see cref="PageTitle"/>, <see cref="PageDescription"/>, <see cref="Headings"/>,
/// <see cref="SidebarItems"/>, <see cref="Previous"/>, <see cref="Next"/>,
/// <see cref="BreadcrumbItems"/>), then place page content in the default slot.
/// </remarks>
public sealed class DocsLayout : AtollComponent
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

    /// <summary>Gets or sets the headings for the table of contents. Defaults to empty.</summary>
    [Parameter]
    public IReadOnlyList<MarkdownHeading> Headings { get; set; } = [];

    /// <summary>Gets or sets the resolved sidebar items to render. Defaults to empty.</summary>
    [Parameter]
    public IReadOnlyList<ResolvedSidebarItem> SidebarItems { get; set; } = [];

    /// <summary>Gets or sets the previous page pagination link, or <c>null</c> if none.</summary>
    [Parameter]
    public PaginationLink? Previous { get; set; }

    /// <summary>Gets or sets the next page pagination link, or <c>null</c> if none.</summary>
    [Parameter]
    public PaginationLink? Next { get; set; }

    /// <summary>Gets or sets the breadcrumb items. Defaults to empty.</summary>
    [Parameter]
    public IReadOnlyList<BreadcrumbItem> BreadcrumbItems { get; set; } = [];

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

        // Header
        WriteHtml("<header class=\"docs-header\">");
        WriteHtml("<div class=\"docs-header-inner\">");

        // Mobile nav toggle island (only activates on mobile viewport)
        await RenderAsync(ComponentRenderer.ToFragment<MobileNav>(new Dictionary<string, object?>()));

        // Logo / site title
        WriteHtml("<a href=\"/\" class=\"docs-brand\">");
        if (!string.IsNullOrEmpty(Config.LogoSrc))
        {
            WriteHtml($"<img src=\"{HtmlEncode(Config.LogoSrc)}\" alt=\"{HtmlEncode(Config.LogoAlt)}\" class=\"docs-logo\" />");
        }

        WriteText(Config.Title);
        WriteHtml("</a>");

        // Header right: search + theme toggle + social
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

        // Body: sidebar + main + TOC
        WriteHtml("<div class=\"docs-body\">");

        // Sidebar (also serves as the mobile-nav-menu target)
        WriteHtml("<aside class=\"docs-sidebar\" id=\"mobile-nav-menu\" aria-label=\"Site navigation\">");
        await RenderAsync(ComponentRenderer.ToFragment<Sidebar>(new Dictionary<string, object?>
        {
            ["Items"] = SidebarItems,
        }));
        WriteHtml("</aside>");

        // Main content
        WriteHtml("<main class=\"docs-main\" id=\"main-content\">");

        // Breadcrumbs
        if (BreadcrumbItems.Count > 0)
        {
            await RenderAsync(ComponentRenderer.ToFragment<Breadcrumbs>(new Dictionary<string, object?>
            {
                ["Items"] = BreadcrumbItems,
            }));
        }

        // Article slot
        WriteHtml("<article class=\"docs-article prose\">");
        await RenderSlotAsync();
        WriteHtml("</article>");

        // Pagination
        if (Previous is not null || Next is not null)
        {
            await RenderAsync(ComponentRenderer.ToFragment<Pagination>(new Dictionary<string, object?>
            {
                ["Previous"] = Previous,
                ["Next"] = Next,
            }));
        }

        WriteHtml("</main>");

        // Table of contents sidebar (desktop only)
        WriteHtml("<aside class=\"docs-toc\" aria-label=\"On this page\">");
        await RenderAsync(ComponentRenderer.ToFragment<TableOfContents>(new Dictionary<string, object?>
        {
            ["Headings"] = Headings,
            ["MinLevel"] = Config.TableOfContents.MinHeadingLevel,
            ["MaxLevel"] = Config.TableOfContents.MaxHeadingLevel,
        }));
        WriteHtml("</aside>");

        WriteHtml("</div>"); // .docs-body

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
