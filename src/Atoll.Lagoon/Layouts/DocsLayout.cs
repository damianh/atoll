using Atoll.Build.Content.Markdown;
using Atoll.Components;
using Atoll.Lagoon.Assets;
using Atoll.Lagoon.Configuration;
using Atoll.Lagoon.I18n;
using Atoll.Lagoon.Navigation;
using Atoll.Lagoon.Versioning;
using Atoll.Slots;

namespace Atoll.Lagoon.Layouts;

/// <summary>
/// The main documentation page layout. Assembles the full HTML document shell,
/// composing header (logo, search, theme toggle), mobile nav, sidebar, breadcrumbs,
/// main content area, table of contents, pagination, and footer.
/// </summary>
/// <remarks>
/// Usage: set <see cref="Config"/> (required), optional page-specific parameters
/// (<see cref="PageTitle"/>, <see cref="PageDescription"/>, <see cref="PageHeadContent"/>,
/// <see cref="Headings"/>, <see cref="SidebarItems"/>, <see cref="Previous"/>,
/// <see cref="Next"/>, <see cref="BreadcrumbItems"/>), then place page content in the default slot.
/// Rendering is delegated to <c>DocsLayoutTemplate.cshtml</c>.
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

    /// <summary>Gets or sets optional raw HTML to inject into the page's head section.</summary>
    [Parameter]
    public string? PageHeadContent { get; set; }

    /// <summary>Gets or sets the HTML lang attribute value. Defaults to <c>"en"</c>.</summary>
    [Parameter]
    public string Lang { get; set; } = "en";

    /// <summary>Gets or sets the current page URL path, used to resolve the active locale. Defaults to <c>"/"</c>.</summary>
    [Parameter]
    public string CurrentPath { get; set; } = "/";

    /// <summary>
    /// Gets or sets a value indicating whether the current page is showing untranslated (fallback) content.
    /// When <c>true</c> and locales are configured, a notice banner is rendered before the main content.
    /// </summary>
    [Parameter]
    public bool IsUntranslatedContent { get; set; }

    /// <summary>Gets or sets the page slug appended to <see cref="DocsConfig.EditUrl"/> to form the edit link. Defaults to <c>null</c> (no edit link).</summary>
    [Parameter]
    public string? PageSlug { get; set; }

    /// <summary>Gets or sets the last-modified timestamp for the page. When set, a "Last updated" date is rendered below the article. Defaults to <c>null</c>.</summary>
    [Parameter]
    public DateTimeOffset? LastUpdated { get; set; }

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        // Resolve locale from path when multi-locale configuration is present.
        var locale = LocaleResolver.Resolve(CurrentPath, Config.Locales, Config.BasePath);
        var effectiveLang = locale?.Config.Lang ?? Lang;
        var effectiveDir = locale?.Config.Dir ?? "ltr";
        var translations = locale?.Config.Translations ?? Config.Translations;

        // Resolve version from the content path after locale resolution.
        var contentPathForVersion = locale?.ContentPath ?? CurrentPath;
        var version = VersionResolver.Resolve(contentPathForVersion, Config.Versions);

        // Compute version- and locale-aware search index URL.
        var localePathPrefix = locale is not null && !string.IsNullOrEmpty(locale.PathPrefix) ? locale.PathPrefix : "";
        var versionPathPrefix = version is not null && !string.IsNullOrEmpty(version.PathPrefix) ? version.PathPrefix : "";
        var searchIndexUrl = ComputeSearchIndexUrl(localePathPrefix, versionPathPrefix);

        var logoSrc = !string.IsNullOrEmpty(Config.LogoSrc)
            ? Config.LogoSrc
            : LagoonAssets.DefaultFaviconPath;

        // Compute edit link URL.
        string? editHref = null;
        if (Config.EditUrl is not null && PageSlug is not null)
        {
            editHref = Config.EditUrl.TrimEnd('/') + "/" + PageSlug.TrimStart('/');
        }

        // Compute path to current version for deprecated version notices.
        string? currentVersionPath = null;
        if (version?.Config.IsDeprecated == true)
        {
            currentVersionPath = VersionPathHelper.PrefixPath(version.ContentPath, "", localePathPrefix, Config.BasePath);
        }

        var model = new DocsLayoutModel(
            Config,
            PageTitle,
            PageDescription,
            PageHeadContent,
            effectiveLang,
            effectiveDir,
            translations,
            searchIndexUrl,
            logoSrc,
            locale,
            localePathPrefix,
            version,
            Headings,
            SidebarItems,
            Previous,
            Next,
            BreadcrumbItems,
            IsUntranslatedContent,
            currentVersionPath,
            editHref,
            LastUpdated,
            Config.EnableMermaid);

        // Pass the page content slot through to the Razor template.
        var pageSlot = context.Slots.GetSlotFragment(SlotCollection.DefaultSlotName);
        var templateSlots = SlotCollection.FromDefault(pageSlot);

        await ComponentRenderer.RenderSliceAsync<DocsLayoutTemplate, DocsLayoutModel>(
            context.Destination,
            model,
            templateSlots);
    }

    private string ComputeSearchIndexUrl(string localePathPrefix, string versionPathPrefix)
    {
        if (!string.IsNullOrEmpty(localePathPrefix) || !string.IsNullOrEmpty(versionPathPrefix))
        {
            var baseTrimmed = Config.BasePath.TrimEnd('/');
            var localeTrimmed = localePathPrefix.TrimEnd('/');
            var versionTrimmed = versionPathPrefix.TrimEnd('/');
            return baseTrimmed + localeTrimmed + versionTrimmed + "/search-index.json";
        }

        if (!string.IsNullOrEmpty(Config.BasePath))
        {
            return Config.BasePath.TrimEnd('/') + "/search-index.json";
        }

        return "/search-index.json";
    }
}
