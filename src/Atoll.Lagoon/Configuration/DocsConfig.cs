using Atoll.Lagoon.I18n;
using Atoll.Lagoon.Versioning;

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
    /// When <c>null</c>, the built-in Atoll logo PNG is used
    /// (served at <c>/_atoll/logo.png</c>).
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
    /// Gets or sets a value indicating whether server-side syntax highlighting is enabled.
    /// When <c>true</c>, fenced code blocks with recognized language identifiers are
    /// rendered with CSS classes for syntax coloring using TextMate grammars at build time.
    /// Default: <c>false</c>.
    /// </summary>
    public bool EnableSyntaxHighlighting { get; set; }

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
    /// Gets or sets the version configuration for multi-version documentation sites.
    /// Keys are version slugs (e.g. <c>"v1.0"</c>, <c>"v2.0"</c>) or <c>"current"</c> for the default (latest) version.
    /// The <c>"current"</c> version has no URL prefix; archived versions are served under
    /// <c>/{version}/{page}</c> (or <c>/{locale}/{version}/{page}</c> when locales are also configured).
    /// When <c>null</c>, the site operates in single-version mode and rendering is identical to before (backward compatible).
    /// </summary>
    public IReadOnlyDictionary<string, VersionConfig>? Versions { get; set; }

    /// <summary>
    /// Gets or sets the default BCP-47 language tag used when no locale configuration is present.
    /// Defaults to <c>"en"</c>.
    /// </summary>
    public string DefaultLang { get; set; } = "en";

    /// <summary>
    /// Gets or sets the position of the collapse/expand chevron on sidebar group headings.
    /// <see cref="SidebarChevronPosition.End"/> places the chevron after the label (Astro Starlight style).
    /// <see cref="SidebarChevronPosition.Start"/> places the chevron before the label (Duende docs style).
    /// Default: <see cref="SidebarChevronPosition.End"/>.
    /// </summary>
    public SidebarChevronPosition SidebarChevronPosition { get; set; } = SidebarChevronPosition.End;

    /// <summary>
    /// Gets or sets the base URL for "Edit this page" links.
    /// When set, a link to edit the current page on the source repository is rendered below the content.
    /// Example: <c>"https://github.com/org/repo/edit/main/docs/"</c>.
    /// The page slug is appended to this URL at render time.
    /// Default: <c>null</c> (no edit link).
    /// </summary>
    public string? EditUrl { get; set; }

    /// <summary>
    /// Gets or sets optional custom footer configuration.
    /// When <c>null</c>, the default "Built with Atoll" footer is rendered.
    /// </summary>
    public FooterConfig? Footer { get; set; }

    /// <summary>
    /// Gets or sets the site-wide announcement banner configuration.
    /// When set, a banner is rendered above the main content in both <c>DocsLayout</c>
    /// and <c>SplashLayout</c>.
    /// When <c>null</c> (default), no banner HTML is rendered.
    /// </summary>
    public BannerConfig? Banner { get; set; }

    /// <summary>
    /// Gets or sets config-based redirect mappings.
    /// Keys are source paths (e.g. <c>"/old-page"</c>), values are redirect target paths
    /// (e.g. <c>"/new-page"</c>). Paths are base-relative and do not include
    /// <see cref="BasePath"/>.
    /// When <c>null</c> (default), no config-based redirects are active.
    /// </summary>
    public IReadOnlyDictionary<string, string>? Redirects { get; set; }

    /// <summary>
    /// Gets or sets the OpenGraph image generation configuration.
    /// When set, branded 1200×630 PNG images are generated at build time for each documentation page
    /// and OG/Twitter Card meta tags are automatically rendered in the document head.
    /// When <c>null</c>, no OG image generation occurs and no OG meta tags are rendered.
    /// </summary>
    public OpenGraphConfig? OpenGraph { get; set; }
}
