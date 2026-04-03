namespace Atoll.Docs.Configuration;

/// <summary>
/// Root configuration for the <c>Atoll.Docs</c> documentation theme.
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
    /// When <c>null</c>, no logo is displayed.
    /// </summary>
    public string? LogoSrc { get; set; }

    /// <summary>
    /// Gets or sets the alt text for the logo image.
    /// </summary>
    public string LogoAlt { get; set; } = "";

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
}
