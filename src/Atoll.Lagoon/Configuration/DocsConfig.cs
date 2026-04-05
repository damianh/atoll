using Atoll.Lagoon.I18n;

namespace Atoll.Lagoon.Configuration;

/// <summary>
/// Root configuration for the <c>Atoll.Lagoon</c> documentation theme.
/// Mirrors the configuration model of Starlight (<c>starlight()</c> config) for feature parity.
/// </summary>
public sealed class DocsConfig
{
    /// <summary>
    /// Gets or sets the site title displayed in the header and browser tab.
    /// </summary>
    public string Title { get; set; } = "";

    /// <summary>
    /// Gets or sets a short description of the site used in meta tags.
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// Gets or sets the URL or path to the site logo image.
    /// When <c>null</c>, the built-in Atoll icon is used.
    /// </summary>
    public string? LogoSrc { get; set; }

    /// <summary>
    /// Gets or sets the alt text for the logo image.
    /// </summary>
    public string LogoAlt { get; set; } = "";

    /// <summary>
    /// Gets or sets the URL or path to the site favicon.
    /// When <c>null</c>, the built-in Atoll icon SVG is used
    /// (served at <c>/_atoll/favicon.svg</c>).
    /// </summary>
    public string? FaviconHref { get; set; }

    /// <summary>
    /// Gets or sets the sidebar navigation items.
    /// Items can be leaf links, manual groups, or auto-generated directory groups.
    /// </summary>
    public IReadOnlyList<SidebarItem> Sidebar { get; set; } = [];

    /// <summary>
    /// Gets or sets the table of contents configuration.
    /// Controls heading levels shown in the "On this page" sidebar.
    /// </summary>
    public TableOfContentsConfig TableOfContents { get; set; } = new TableOfContentsConfig();

    /// <summary>
    /// Gets or sets the social links shown in the header.
    /// </summary>
    public IReadOnlyList<SocialLink> Social { get; set; } = [];

    /// <summary>
    /// Gets or sets paths to additional CSS files to load in every page's <c>&lt;head&gt;</c>.
    /// Paths are relative to the site root or absolute URLs.
    /// </summary>
    public IReadOnlyList<string> CustomCss { get; set; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether Mermaid diagram rendering is enabled.
    /// When <c>true</c>, the Mermaid JS library is loaded and fenced <c>mermaid</c> code
    /// blocks are rendered as diagrams.
    /// Default: <c>false</c>.
    /// </summary>
    public bool EnableMermaid { get; set; }

    /// <summary>
    /// Gets or sets the base URL path prefix for the documentation site.
    /// For example, <c>"/docs"</c> when docs are hosted at <c>https://example.com/docs/</c>.
    /// Default: <c>""</c> (root).
    /// </summary>
    public string BasePath { get; set; } = "";

    /// <summary>
    /// Gets or sets the UI translations used by all Lagoon components.
    /// Defaults to <see cref="UiTranslations.Default"/> (English).
    /// </summary>
    public UiTranslations Translations { get; set; } = UiTranslations.Default;

    /// <summary>
    /// Gets or sets the locale configuration for multi-language sites.
    /// Keys are URL path prefixes (e.g. <c>"fr"</c>, <c>"zh-cn"</c>) or <c>"root"</c> for the default locale.
    /// When <c>null</c> or empty, the site operates in single-language mode (backward compatible).
    /// </summary>
    public IReadOnlyDictionary<string, LocaleConfig>? Locales { get; set; }

    /// <summary>
    /// Gets or sets the default BCP-47 language tag used when no locale configuration is present.
    /// Defaults to <c>"en"</c>.
    /// </summary>
    public string DefaultLang { get; set; } = "en";
}
