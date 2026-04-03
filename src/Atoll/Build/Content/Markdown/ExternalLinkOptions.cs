namespace Atoll.Build.Content.Markdown;

/// <summary>
/// Configuration options for adding security attributes to external links in Markdown content.
/// When set on <see cref="MarkdownOptions.ExternalLinks"/>, the Markdown renderer will add
/// <c>target="_blank"</c> and <c>rel="noopener noreferrer"</c> to links with absolute URLs
/// beginning with <c>http://</c> or <c>https://</c>.
/// </summary>
public sealed class ExternalLinkOptions
{
    /// <summary>
    /// Gets or sets the value to use for the <c>target</c> attribute on external links.
    /// Set to <c>null</c> to suppress the <c>target</c> attribute.
    /// Default: <c>"_blank"</c>.
    /// </summary>
    public string? Target { get; set; } = "_blank";

    /// <summary>
    /// Gets or sets the value to use for the <c>rel</c> attribute on external links.
    /// Set to <c>null</c> to suppress the <c>rel</c> attribute.
    /// Default: <c>"noopener noreferrer"</c>.
    /// </summary>
    public string? Rel { get; set; } = "noopener noreferrer";

    /// <summary>
    /// Gets or sets a list of hostnames to exclude from external link processing.
    /// Links whose host exactly matches an entry in this list will not have attributes added.
    /// Comparison is case-insensitive.
    /// Default: empty (no exclusions).
    /// </summary>
    public IReadOnlyList<string> ExcludedHosts { get; set; } = [];
}
