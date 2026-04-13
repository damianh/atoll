namespace Atoll.Build.Content.Markdown;

/// <summary>
/// Configuration options for resolving relative Markdown file links to clean URL paths.
/// When set on <see cref="MarkdownOptions.LinkResolution"/>, the Markdown renderer will
/// rewrite relative links ending in <c>.md</c>, <c>.mdx</c>, or <c>.mda</c> to clean URL paths.
/// </summary>
public sealed class LinkResolutionOptions
{
    /// <summary>
    /// Gets or sets the base path prefix to prepend to resolved URLs.
    /// For example, <c>"/docs"</c> will resolve <c>./page.md</c> to <c>/docs/page/</c>.
    /// Default: <c>""</c> (empty string — no prefix).
    /// </summary>
    public string BasePath { get; set; } = "";

    /// <summary>
    /// Gets or sets a value indicating whether a trailing slash is appended to resolved URLs.
    /// For example, <c>/docs/page</c> becomes <c>/docs/page/</c>.
    /// Default: <c>true</c>.
    /// </summary>
    public bool AddTrailingSlash { get; set; } = true;

    /// <summary>
    /// Gets or sets the file extensions to strip from relative link URLs.
    /// Extensions are matched case-insensitively.
    /// Default: <c>[".md", ".mdx", ".mda"]</c>.
    /// </summary>
    public IReadOnlyList<string> ExtensionsToStrip { get; set; } = [".md", ".mdx", ".mda"];

    /// <summary>
    /// Gets or sets the URL-space directory path used to resolve relative asset references
    /// (images, SVGs, etc.) in Markdown content. When set, relative URLs on image elements
    /// and non-Markdown link targets are rewritten to absolute paths by prepending this base path.
    /// <para>
    /// For example, when a content entry in the <c>articles</c> collection with
    /// <see cref="BasePath"/> <c>"/docs"</c> has <c>ContentAssetBasePath</c> set to
    /// <c>"/docs/articles"</c>, the Markdown image <c>![alt](images/diagram.svg)</c>
    /// is rewritten to <c>&lt;img src="/docs/articles/images/diagram.svg"&gt;</c>.
    /// </para>
    /// When <c>null</c>, relative asset URLs are left unchanged (browser-resolved).
    /// Default: <c>null</c>.
    /// </summary>
    public string? ContentAssetBasePath { get; set; }
}
