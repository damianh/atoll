namespace Atoll.Lagoon.I18n;

/// <summary>
/// Contains all translatable UI strings used by Lagoon components.
/// Create custom instances to override defaults for a specific locale.
/// </summary>
public sealed record UiTranslations
{
    // -- Skip link --

    /// <summary>Gets the label for the skip-to-content link.</summary>
    public string SkipLinkLabel { get; init; } = "Skip to content";

    // -- Search --

    /// <summary>Gets the accessible label for the search trigger button.</summary>
    public string SearchLabel { get; init; } = "Search";

    /// <summary>Gets the placeholder text for the search input.</summary>
    public string SearchPlaceholder { get; init; } = "Search docs...";

    /// <summary>Gets the accessible label for the search dialog.</summary>
    public string SearchDialogLabel { get; init; } = "Search docs";

    /// <summary>Gets the accessible label for the close-search button.</summary>
    public string SearchCloseLabel { get; init; } = "Close search";

    /// <summary>Gets the accessible label for the search results list.</summary>
    public string SearchResultsLabel { get; init; } = "Search results";

    /// <summary>Gets the message shown when a search yields no results.</summary>
    public string SearchNoResults { get; init; } = "No results found.";

    /// <summary>Gets the accessible label for the topic filter chip bar in the search dialog.</summary>
    public string SearchTopicFilterLabel { get; init; } = "Filter by topic";

    // -- Theme --

    /// <summary>Gets the accessible label for the theme toggle button.</summary>
    public string ThemeToggleLabel { get; init; } = "Toggle theme";

    /// <summary>Gets the label applied when the theme switches to light mode.</summary>
    public string ThemeSwitchToLight { get; init; } = "Switch to light theme";

    /// <summary>Gets the label applied when the theme switches to dark mode.</summary>
    public string ThemeSwitchToDark { get; init; } = "Switch to dark theme";

    // -- Navigation --

    /// <summary>Gets the accessible label for the sidebar navigation.</summary>
    public string SidebarNavLabel { get; init; } = "Main";

    /// <summary>Gets the accessible label for the site navigation landmark.</summary>
    public string SiteNavigationLabel { get; init; } = "Site navigation";

    /// <summary>Gets the accessible label for the mobile navigation button.</summary>
    public string MobileNavOpenLabel { get; init; } = "Open navigation";

    /// <summary>Gets the accessible label for the breadcrumbs navigation.</summary>
    public string BreadcrumbsLabel { get; init; } = "Breadcrumbs";

    /// <summary>Gets the accessible label for the pagination navigation.</summary>
    public string PaginationLabel { get; init; } = "Pagination";

    /// <summary>Gets the label for the previous-page pagination link.</summary>
    public string PaginationPrevious { get; init; } = "Previous";

    /// <summary>Gets the label for the next-page pagination link.</summary>
    public string PaginationNext { get; init; } = "Next";

    // -- Table of Contents --

    /// <summary>Gets the label for the table-of-contents heading.</summary>
    public string TocLabel { get; init; } = "On this page";

    // -- Content footer --

    /// <summary>Gets the display text for the edit-page link.</summary>
    public string EditPageLabel { get; init; } = "Edit page";

    /// <summary>Gets the label shown before the last-updated date.</summary>
    public string LastUpdatedLabel { get; init; } = "Last updated";

    // -- Footer --

    /// <summary>Gets the prefix text for the footer attribution.</summary>
    public string BuiltWithLabel { get; init; } = "Built with";

    // -- Language picker --

    /// <summary>Gets the accessible label for the language selector.</summary>
    public string LanguageSelectLabel { get; init; } = "Select language";

    // -- Version picker --

    /// <summary>Gets the accessible label for the version selector.</summary>
    public string VersionSelectLabel { get; init; } = "Select version";

    // -- Content notices --

    /// <summary>Gets the notice text shown when a page has not been translated for the current locale.</summary>
    public string UntranslatedContentNotice { get; init; } = "This page has not been translated yet.";

    /// <summary>Gets the notice text shown when viewing documentation for a deprecated version.</summary>
    public string OutdatedVersionNotice { get; init; } = "You are viewing documentation for an older version.";

    /// <summary>Gets the link text in the deprecated version notice pointing to the current version.</summary>
    public string OutdatedVersionLinkText { get; init; } = "View latest version";

    /// <summary>English defaults.</summary>
    public static UiTranslations Default { get; } = new();
}
