using Atoll.Build.Content.Markdown;
using Atoll.Lagoon.Configuration;
using Atoll.Lagoon.I18n;
using Atoll.Lagoon.Navigation;
using Atoll.Lagoon.Versioning;

namespace Atoll.Lagoon.Layouts;

/// <summary>
/// Model for the <c>DocsLayoutTemplate</c> Razor slice, carrying all pre-processed
/// data needed to render the documentation page layout.
/// </summary>
/// <param name="Config">The docs site configuration.</param>
/// <param name="PageTitle">The page-specific title appended to the site title.</param>
/// <param name="PageDescription">Optional page-specific meta description.</param>
/// <param name="PageHeadContent">Optional raw HTML to inject into the head section.</param>
/// <param name="Lang">The effective HTML lang attribute value (locale-resolved).</param>
/// <param name="Dir">The effective text direction (<c>"ltr"</c> or <c>"rtl"</c>).</param>
/// <param name="Translations">The effective UI translations (locale-resolved).</param>
/// <param name="SearchIndexUrl">The computed version- and locale-aware search index URL.</param>
/// <param name="LogoSrc">The resolved logo image URL (custom or default fallback).</param>
/// <param name="Locale">The resolved locale, or <c>null</c> for single-language sites.</param>
/// <param name="LocalePathPrefix">The locale URL prefix (e.g. <c>"/fr"</c>), or empty.</param>
/// <param name="Version">The resolved version, or <c>null</c> for single-version sites.</param>
/// <param name="Headings">The headings for the table of contents.</param>
/// <param name="SidebarItems">The resolved sidebar items to render.</param>
/// <param name="Previous">The previous page pagination link, or <c>null</c>.</param>
/// <param name="Next">The next page pagination link, or <c>null</c>.</param>
/// <param name="BreadcrumbItems">The breadcrumb items.</param>
/// <param name="IsUntranslatedContent">Whether the page is showing untranslated fallback content.</param>
/// <param name="CurrentVersionPath">The path to the current version of the page for deprecated version notices.</param>
/// <param name="EditHref">The composed edit URL for the page, or <c>null</c> if not applicable.</param>
/// <param name="LastUpdated">The last-modified timestamp, or <c>null</c>.</param>
/// <param name="EnableMermaid">Whether to inject the Mermaid initialization script.</param>
/// <param name="CurrentPath">The current page URL path (e.g. <c>/identityserver/overview/big-picture</c>).</param>
/// <param name="SiteUrl">The site base URL (e.g. <c>https://docs.example.com</c>), used for absolute OG URLs.</param>
public sealed record DocsLayoutModel(
    DocsConfig Config,
    string PageTitle,
    string? PageDescription,
    string? PageHeadContent,
    string Lang,
    string Dir,
    UiTranslations Translations,
    string SearchIndexUrl,
    string LogoSrc,
    ResolvedLocale? Locale,
    string LocalePathPrefix,
    ResolvedVersion? Version,
    IReadOnlyList<MarkdownHeading> Headings,
    IReadOnlyList<ResolvedSidebarItem> SidebarItems,
    PaginationLink? Previous,
    PaginationLink? Next,
    IReadOnlyList<BreadcrumbItem> BreadcrumbItems,
    bool IsUntranslatedContent,
    string? CurrentVersionPath,
    string? EditHref,
    DateTimeOffset? LastUpdated,
    bool EnableMermaid,
    string CurrentPath,
    string SiteUrl);

